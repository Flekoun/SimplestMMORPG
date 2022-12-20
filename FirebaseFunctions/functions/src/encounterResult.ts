
// [START import]
import * as functions from "firebase-functions";
import { CharacterDocument, characterDocumentConverter, ContentContainer, CURRENCY_ID, getMillisPassedSinceTimestamp, millisToSeconds, PlayerData, WorldPosition } from ".";

import { randomIntFromInterval } from "./general2";

const admin = require('firebase-admin');
// // [END import]

export const RESULT_ITEM_WANT_DURATION_SECONDS = 90;

export class EncounterResult {
  constructor(
    public uid: string,
    public enemies: EncounterResultEnemy[],
    public combatantsList: string[],
    public combatantsWithUnclaimedRewardsList: string[],
    public combatantsWithUnchoosenWantedItemList: string[],
    public combatantsData: EncounterResultCombatant[],
    //  tohle je uplne zbytecne combatMember, udelat si vlastniho nejakeho , mozna by sel pozit ten EncounterResultWanter? A tomu pridat "EXP gained" property kterou naplnim tou funkci na odhad expu z boje
    //   je to pro me pohodlnejsi protoze predam CombatMembery z combatu a hotovo, takze asi zase nejaky convertor nebo to runce povytvaret.....

    //public contentLoot: EncounterResultContentLoot[],
    public silver: number,
    public wantItemPhaseFinished: boolean,
    public turnsNumber: number,
    public expireDateWantItemPhase: string,
    public position: WorldPosition

  ) { }
}
export class EncounterResultContentLoot {
  constructor(
    public content: ContentContainer,
    public charactersWhoWantThis: EncounterResultCombatant[],
    public characterWhoWillHaveThis: EncounterResultCombatant | null,
  ) { }
}

export class EncounterResultCombatant {
  constructor(
    public uid: string,
    public displayName: string,
    public characterClass: string,
    public level: number,
    public expGainedEstimate: number
  ) { }
}

export class EncounterResultEnemy {
  constructor(
    public id: string,
    public displayName: string,
    public level: number,
    public contentLoot: EncounterResultContentLoot[],
  ) { }
}


exports.forceEndWantItemPhase = functions.https.onCall(async (data, context) => {
  const encounterResultUid = data.encounterResultUid;
 // const callerCharacterUid = data.characterUid;

  const encounterResultsDb = admin.firestore().collection('encounterResults').doc(encounterResultUid);

  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {

      const encounterDoc = await t.get(encounterResultsDb);
      let encounterResultData: EncounterResult = encounterDoc.data();

      //Snazis se claimovat po vyprseni timeru na vyber a ne vsichni si jete vybrali, tedy kdyz nekdo zdrzuje, tak forcneme vyber
      const secondsPassedOverTurnLimit = millisToSeconds(getMillisPassedSinceTimestamp(encounterResultData.expireDateWantItemPhase));
      if (secondsPassedOverTurnLimit > 0) {
        if (!encounterResultData.wantItemPhaseFinished) {
          console.log("playerUid :" + context.auth?.uid + " is claiming rewards eventhoug not all players wanted item because Turn timer is overdue by : " + secondsPassedOverTurnLimit + " seconds");
          chooseWinnersOfLoot(encounterResultData);
          encounterResultData.wantItemPhaseFinished = true
        }
      }

      t.set(encounterResultsDb, JSON.parse(JSON.stringify(encounterResultData)), { merge: true });

      return "OK";
    });
    console.log('Transaction success', result);
    return result;
  } catch (e) {
    console.log('Transaction failure:', e);
    throw new functions.https.HttpsError("aborted", "Error : " + e);
  }
});



exports.collectEncounterResultReward = functions.https.onCall(async (data, context) => {
  const encounterResultUid = data.encounterResultUid;
  const callerCharacterUid = data.characterUid;
  console.log("encounterResultUid: " + encounterResultUid);
  // const encounterDb = admin.firestore().collection('encounters').doc(encounterUid).withConverter(encounterDocumentConverter);

  const encounterResultsDb = admin.firestore().collection('encounterResults').doc(encounterResultUid);
  const characterDb = await admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);

  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {
      const encounterDoc = await t.get(encounterResultsDb);
      let encounterResultData: EncounterResult = encounterDoc.data();

      //pokud jeste neskoncila faze vyberu want itemu, nemas co claimovat! (pokud si sam tak ignoruju)
      if (!encounterResultData.wantItemPhaseFinished)
        throw "Cant claim reward, yet! Players still choosing item they want to to roll on!";

      const characterDoc = await t.get(characterDb);
      let characterData: CharacterDocument = characterDoc.data();


      //Najdu svuj bojovy zaznam
      let myCombatEntry: EncounterResultCombatant | undefined;

      encounterResultData.combatantsData.forEach(item => {
        if (item.uid == callerCharacterUid) {
          myCombatEntry = item;
          console.log("found your combat entry ");
        }
      });

      if (myCombatEntry == undefined)
        throw "cannot find your combat entry in encounter! How can you try to claim reward?! character Id ! - " + callerCharacterUid;



      //Zkontroluju jestli jsi mezi tema co bojovali za tento reward
      if (!encounterResultData.combatantsWithUnclaimedRewardsList.includes(callerCharacterUid))
        throw ("You are not one of participants in this battle! Cannot claim rewards!");

      //odstanim te ze zaznamu abys nemohl vybirat reward vickrat
      encounterResultData.combatantsWithUnclaimedRewardsList.splice(encounterResultData.combatantsWithUnclaimedRewardsList.indexOf(callerCharacterUid), 1);

      //dam Fatigue
      characterData.addCurrency(CURRENCY_ID.FATIGUE, encounterResultData.turnsNumber);

      //dam Sikver
      characterData.addCurrency(CURRENCY_ID.SILVER, encounterResultData.silver);

      //dam ti itemy co jsi vyhral
      encounterResultData.enemies.forEach(enemy => {

        enemy.contentLoot.forEach(content => {
          if (content.characterWhoWillHaveThis == null) //byl sem v encounteru sam, neni vyplneni ani kdo ten item ma mit, protoze se nehlasovalo, tak si ten item dam rovnou do invntare
            characterData.addContentToInventory(content.content, true, false)
          else if (content.characterWhoWillHaveThis.uid == characterData.uid) //jinak ti ho dam jen pokud jsi ten co ho ma mit
            characterData.addContentToInventory(content.content, true, false);

        });
      });

      // encounterResultData.equipLoot.forEach(equip => { if (equip.characterWhoWillHaveThis!.uid == characterData.uid) characterData.addContentToInventory(equip.equip); });
      //   encounterResultData.itemLoot.forEach(item => { if (item.characterWhoWillHaveThis!.uid == characterData.uid) characterData.addItemSimpleToInventory(item.item); });

      //zapisu killy 
      encounterResultData.enemies.forEach(enemy => { characterData.recordMonsterKill(enemy.id) });

      //Dam expy
      let characterLevel = characterData.stats.level; //ulozim si level driv nez awardnu expy
      characterData.giveExp(encounterResultData);

      //Natavim ze uz nejsi v boji
     // characterData.isJoinedInEncounter = false;

      if (characterLevel < characterData.stats.level) //pokud sem dostal level, ta ho chci updatnout i v CharacterPreview 
      {
        const playerDb = admin.firestore().collection('players').doc(characterData.userUid);
        const playerDoc = await t.get(playerDb);
        let playerData: PlayerData = playerDoc.data();
        playerData.characters.forEach(characterPreview => {
          if (characterPreview.uid == characterData.uid)
            characterPreview.level = characterData.stats.level;
        });
        t.set(playerDb, JSON.parse(JSON.stringify(playerData)), { merge: true });

      }

      //zkontroluju jestli uz vsichni nevybrali odmeny, pokud jo deletnu zaznam uplne, jinak jen updatnu
      if (encounterResultData.combatantsWithUnclaimedRewardsList.length == 0) {
        t.delete(encounterResultsDb);
      }
      else {
        t.set(encounterResultsDb, JSON.parse(JSON.stringify(encounterResultData)), { merge: true });
      }

      t.set(characterDb, JSON.parse(JSON.stringify(characterData)), { merge: true });

      return "OK";
    });




    console.log('Transaction success', result);
    return result;
  } catch (e) {
    console.log('Transaction failure:', e);
    throw new functions.https.HttpsError("aborted", "Error : " + e);
  }
});

function chooseWinnersOfLoot(_encounterResultData: EncounterResult) {

  //projdu vsechen loot a...
  _encounterResultData.enemies.forEach(enemy => {
    enemy.contentLoot.forEach(content => {

      //..jeste nema zadneho vyherce?
      if (content.characterWhoWillHaveThis == null) {
        //nekdo ten item chce, vylosuju vyhrece mezi wanterama
        if (content.charactersWhoWantThis.length > 0) {

          let randomGuy = content.charactersWhoWantThis[randomIntFromInterval(0, content.charactersWhoWantThis.length - 1)];
          content.characterWhoWillHaveThis = randomGuy;// new EncounterResultCombatant(randomGuy.uid, randomGuy.displayName, randomGuy.characterClass, randomGuy.level, randomGuy.expGainedEstimate);
        }
        //nikdo ten item nechce, budeme losovat mezi vsemi combatantami
        else {
          let randomGuy = _encounterResultData.combatantsData[randomIntFromInterval(0, _encounterResultData.combatantsData.length - 1)];
          content.characterWhoWillHaveThis = randomGuy;//new EncounterResultCombatant(randomGuy.uid, randomGuy.displayName, randomGuy.characterClass, randomGuy.level, randomGuy.expGainedEstimate);
        }
      }



    });
  });

}

exports.selectWantItemInEncounterResult = functions.https.onCall(async (data, context) => {
  const encounterResultUid = data.encounterResultUid;
  const callerCharacterUid = data.characterUid;
  const wantedItemId = data.wantedItemId;

  // const encounterDb = admin.firestore().collection('encounters').doc(encounterUid).withConverter(encounterDocumentConverter);
  const encounterResultsDb = admin.firestore().collection('encounterResults').doc(encounterResultUid);
  //  const characterDb = await admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);

  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {
      const encounterDoc = await t.get(encounterResultsDb);
      let encounterResultData: EncounterResult = encounterDoc.data();

      //Snazis se vybirat wanted item po vyprseni timeru na vyber, smula ----ale prd necham ho dokud nekdo neforcne end
      // const secondsPassedOverTurnLimit = millisToSeconds(getMillisPassedSinceTimestamp(encounterResultData.expireDateWantItemPhase));
      // if (secondsPassedOverTurnLimit > 0) {
      //   console.log("playerUid :" + context.auth?.uid + " is trying to claim rewards eventhoug timer is overdue by : " + secondsPassedOverTurnLimit + " seconds");
      //   throw "Timer expired, yet. Cant choose desired item!";
      // }

      //Zkontroluju jestli uz si nahodou nevybral wanted item
      if (!encounterResultData.combatantsWithUnchoosenWantedItemList.includes(callerCharacterUid))
        throw "You have already choosen your wanted items!";

      //Odstranim te ze seznamu lidi co si jeste nevybrali wwant item
      encounterResultData.combatantsWithUnchoosenWantedItemList.splice(encounterResultData.combatantsWithUnchoosenWantedItemList.indexOf(callerCharacterUid), 1);

      //najdu si tvuj zaznam mezi combatantama
      let myCombatantDataEntry: EncounterResultCombatant;

      encounterResultData.combatantsData.forEach(combatant => {
        if (combatant.uid == callerCharacterUid)
          myCombatantDataEntry = combatant;
      });

      //priradim te jako wantera k tebou vybranemi itemu
      encounterResultData.enemies.forEach(enemy => {
        enemy.contentLoot.forEach(content => {
          content.content = new ContentContainer(content.content.contentType, content.content.contentItem, content.content.contentEquip, content.content.contentCurrency, content.content.contentFood); //kvuli tomu ze nemam withConverter...

          if (content.content.getItem().uid == wantedItemId) {
            content.charactersWhoWantThis.push(myCombatantDataEntry);//new EncounterResultCombatant(myCombatantDataEntry.uid, myCombatantDataEntry.displayName, myCombatantDataEntry.characterClass, myCombatantDataEntry.level,myCombatantDataEntry.expGainedEstimate));
          }
        });
      });


      //zkontroluju jestli uz vsichni nevybrali want item, pokud jo rollnu nahodne vyherce vsem itemum a nastvaim encounter result jako claimable 
      if (encounterResultData.combatantsWithUnchoosenWantedItemList.length == 0) {

        chooseWinnersOfLoot(encounterResultData);
        // //projdu vsechen loot a...
        // encounterResultData.enemies.forEach(enemy => {
        //   enemy.contentLoot.forEach(content => {

        //     //..jeste nema zadneho vyherce?
        //     if (content.characterWhoWillHaveThis == null) {
        //       //nekdo ten item chce, vylosuju vyhrece mezi wanterama
        //       if (content.charactersWhoWantThis.length > 0) {

        //         let randomGuy = content.charactersWhoWantThis[randomIntFromInterval(0, content.charactersWhoWantThis.length - 1)];
        //         content.characterWhoWillHaveThis = new EncounterResultWanter(randomGuy.uid, randomGuy.displayName, randomGuy.characterClass, randomGuy.level);
        //       }
        //       //nikdo ten item nechce, budeme losovat mezi vsemi combatantami
        //       else {
        //         let randomGuy = encounterResultData.combatantsData[randomIntFromInterval(0, encounterResultData.combatantsData.length - 1)];
        //         content.characterWhoWillHaveThis = new EncounterResultWanter(randomGuy.characterUid, randomGuy.displayName, randomGuy.characterClass, randomGuy.level);
        //       }
        //     }

        //     // content.content = new ContentContainer(content.content.contentType, content.content.contentItem, content.content.contentEquip, content.content.contentCurrency, content.content.contentFood); //kvuli tomu ze nemam withConverter...

        //     // if (content.content.getItem().uid == wantedItemId) {
        //     //   content.charactersWhoWantThis.push(new EncounterResultWanter(myCombatantDataEntry.characterUid, myCombatantDataEntry.displayName, myCombatantDataEntry.characterClass, myCombatantDataEntry.level));
        //     // }


        //   });
        // });

        encounterResultData.wantItemPhaseFinished = true;
      }

      t.set(encounterResultsDb, JSON.parse(JSON.stringify(encounterResultData)), { merge: true });

      return "OK";
    });




    console.log('Transaction success', result);
    return result;
  } catch (e) {
    console.log('Transaction failure:', e);
    throw new functions.https.HttpsError("aborted", "Error : " + e);
  }
});

  // [END allAdd]