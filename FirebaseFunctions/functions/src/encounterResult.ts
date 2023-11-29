
// [START import]
import * as functions from "firebase-functions";
import { CharacterDocument, characterDocumentConverter, ContentContainer, CURRENCY_ID, getExpAmountFromEncounterForGivenCharacterLevel, getMillisPassedSinceTimestamp, millisToSeconds, PlayerData, randomIntFromInterval, WorldPosition } from ".";


import { setScoreToLeaderboard, setMyCharacterLevelLeaderboards, updateMyMonsterKillsAndDamageDoneLeaderboards_GET_SET } from "./leaderboards";
import { PerkOfferDefinition } from "./perks";
import { Combatskill } from "./skills";
import { LocationConverter, MapLocation, PointOfInterest, PointOfInterestServerDataDefinitions, PointOfInterestServerDataDefinitionsConverter } from "./worldMap";
import { SetOperation } from "./utils";
import { QuerryForSkillDefinitions } from "./equip";
import { PerkChoiceParticipant } from "./encounter";


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
    public position: WorldPosition,
    public bonusLoot: EncounterResultContentLoot[],
    public dungeonLoot: EncounterResultContentLoot[],
    public perkChoices: PerkChoiceParticipant[],
    //  public perkOffersRare: PerkOfferDefinition[],
    public foundBy: string,
    public dungeonData: DungeonData | null, //pokud origin encounter z ktereho result vznikl byl dungeon, tady se predaji nejake zajimave data
    public tier: number // tier dokonceneho monster encounteru......pridat monsterData? stejne jako ma dungeon?

  ) { }
}

export class DungeonData {
  constructor(
    public dungeonId: string,
    public tier: number,
    public isFinished: boolean, //pokud sme zrovna dokoncili dungeon, obyc
    public isFinalDungeon: boolean,
    public isEndlessDungeon: boolean
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
    public expGainedEstimate: number,
    public damageDone: number,
    public leastHealth: number,
    public deckShuffleCount: number,
    public curses: Combatskill[], //curses ktere ti zbyly po boji
    public healingDone: number,
    public hasAlreadyFinishedEncounterOfThisTier: boolean
    // public successSkillsRolled: SimpleTally[], //ktery skill byl vylosovan a kolik ma  success slots
    // public successResult: number //soucet successu z narolovaneho equipu....pro snadnost...je to jen soucet
  ) { }
}

export class EncounterResultEnemy {
  constructor(
    public id: string,
    public displayName: string,
    public level: number,
    public contentLoot: EncounterResultContentLoot[],
    public monsterEssences: number
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
      let myCombatResultEntry: EncounterResultCombatant | undefined;

      encounterResultData.combatantsData.forEach(item => {
        if (item.uid == callerCharacterUid) {
          myCombatResultEntry = item;
          console.log("found your combat entry ");
        }
      });

      if (myCombatResultEntry == undefined)
        throw "cannot find your combat entry in encounter! How can you try to claim reward?! character Id ! - " + callerCharacterUid;

      //Zkontroluju jestli jsi mezi tema co bojovali za tento reward
      if (!encounterResultData.combatantsWithUnclaimedRewardsList.includes(callerCharacterUid))
        throw ("You are not one of participants in this battle! Cannot claim rewards!");

      //odstanim te ze zaznamu abys nemohl vybirat reward vickrat
      encounterResultData.combatantsWithUnclaimedRewardsList.splice(encounterResultData.combatantsWithUnclaimedRewardsList.indexOf(callerCharacterUid), 1);

      //dam Fatigue
      // characterData.addCurrency(CURRENCY_ID.FATIGUE, 2);// myCombatResultEntry.deckShuffleCount * DECK_SHUFFLE_FATIGUE_PENALTY);

      //dam Sikver(Gold)
      characterData.addCurrency(CURRENCY_ID.GOLD, encounterResultData.silver);

      //updatnu tvuj currentHealth podle toho jak si dopadl v boji
      characterData.stats.currentHealth = myCombatResultEntry.leastHealth;//? characterData.stats.currentHealth = myCombatResultEntry.healthLeft : characterData.stats.currentHealth = -1;//characterData.getMaxHealth();

      //vylecim te starym foodem pokud nejaky mas
      // if (characterData.foodEffect != null) {

      //   let overhealAmount = characterData.giveHealth(characterData.foodEffect.count);
      //   if (overhealAmount > 0)
      //     characterData.foodEffect.count = overhealAmount;
      //   else
      //     characterData.foodEffect = null;
      // }


      const PointOfInterestServerDataDb = admin.firestore().collection("_metadata_zones").doc(encounterResultData.position.zoneId).collection("locations").doc(encounterResultData.position.locationId).collection("pointsOfInterest").doc(encounterResultData.position.pointOfInterestId).collection("definitions").doc("SERVER_DATA").withConverter(PointOfInterestServerDataDefinitionsConverter);
      let PointOfInterestServerData: PointOfInterestServerDataDefinitions | null = null

      const locationDefinitionDb = admin.firestore().collection("_metadata_zones").doc(encounterResultData.position.zoneId).collection("locations").doc(encounterResultData.position.locationId).withConverter(LocationConverter);
      let locationDefinitionData: MapLocation | null = null;

      //perk dam
      //let tierPerkDefinitionsIfRarePerkWasClaimed: { tierDefinitions: TierDefinitions, dbPath: any } | null = null;
      //pokud encounterResult nema zadny perkOffers (asi ambus nebo guardiani bez vyberu perku tak toto preskocim nebo rare enemy)
      if (encounterResultData.perkChoices.length > 0) {

        //pokud si porazil encounter ktery je o 1 tier vyssi nez tier ktery mas dokonceny,.....tedy je to ten nasledujici tier tak zvednu tier, a dam ti perk jinak NE!
        if (characterData.getMaxTierReachedForPointOfInterest(encounterResultData.position.pointOfInterestId) + 1 == encounterResultData.tier) {
          characterData.incrementMaxTierReachedForPointOfInterest(encounterResultData.position.pointOfInterestId);

          //dam ti perky
          let perksWithLimitedStock: PerkOfferDefinition[] = [];

          //Dam ti perk co sis vybral
          //TODO :muzu zbytecne ziskavat skilldefinitions kdyz perkreward nema zadny equip reward na udeleni!!!
          let skillDefinitions = await QuerryForSkillDefinitions(t);
          encounterResultData.perkChoices.forEach(perkChoice => {

            //je to muj choice
            if (characterData.uid == perkChoice.characterUid && perkChoice.choosenPerk != null) {
              characterData.addPendingReward(perkChoice.choosenPerk, skillDefinitions);

              if (perkChoice.choosenPerk.stockLeft != -1)
                perksWithLimitedStock.push(perkChoice.choosenPerk);
            }



          });

          //jdeme zapsat snizeni stocku o 1 - TODO: zbytecne vzdycky ziskavam 2 dokumenty, ikdyz treba potrebuju jen jeden z nich 

          //cesta k obyc perku v point of interestu
          const PointOfInterestServerDataDoc = await t.get(PointOfInterestServerDataDb);
          PointOfInterestServerData = PointOfInterestServerDataDoc.data();

          //cesta k rare perku tierovemu
          const locationDefinitionDoc = await t.get(locationDefinitionDb);
          locationDefinitionData = locationDefinitionDoc.data();

          perksWithLimitedStock.forEach(perk => {

            if (perk.rarePerkGroupId == null) {
              //neni to rare perk
              PointOfInterestServerData?.increaseStockClaimedForPerk(perk.uid);
            }
            else //je to rare perk
            {
              locationDefinitionData?.increaseStockClaimedForRarePerk(perk.uid, perk.rarePerkGroupId);
            }

          });

        }
      }
      //dam ti itemy co jsi vyhral
      encounterResultData.enemies.forEach(enemy => {

        enemy.contentLoot.forEach(content => {
          if (content.characterWhoWillHaveThis == null) //byl sem v encounteru sam, neni vyplneni ani kdo ten item ma mit, protoze se nehlasovalo, tak si ten item dam rovnou do invntare
            characterData.addContentToInventory(content.content, true, false)
          else if (content.characterWhoWillHaveThis.uid == characterData.uid) //jinak ti ho dam jen pokud jsi ten co ho ma mit
            characterData.addContentToInventory(content.content, true, false);

        });
      });

      encounterResultData.bonusLoot.forEach(loot => {
        if (loot.characterWhoWillHaveThis == null) //byl sem v encounteru sam, neni vyplneni ani kdo ten item ma mit, protoze se nehlasovalo, tak si ten item dam rovnou do invntare
          characterData.addContentToInventory(loot.content, true, false)
        else if (loot.characterWhoWillHaveThis.uid == characterData.uid) //jinak ti ho dam jen pokud jsi ten co ho ma mit
          characterData.addContentToInventory(loot.content, true, false);
      });

      encounterResultData.dungeonLoot.forEach(loot => {
        if (loot.characterWhoWillHaveThis == null) //byl sem v encounteru sam, neni vyplneni ani kdo ten item ma mit, protoze se nehlasovalo, tak si ten item dam rovnou do invntare
          characterData.addContentToInventory(loot.content, true, false)
        else if (loot.characterWhoWillHaveThis.uid == characterData.uid) //jinak ti ho dam jen pokud jsi ten co ho ma mit
          characterData.addContentToInventory(loot.content, true, false);
      });


      //odstranim curses kterych ses zbavil v boji z charakteru
      characterData.curses = myCombatResultEntry.curses;

      //zapisem PoI jako explored pokud nebyl...tedy zabil si zrovna guardian groupu na neprozkoumane PoI a ted claimujes jeji loot
      //if (pointOfInterestData.pointOfInterestType == POI_TYPE.ENCOUNTER || pointOfInterestData.pointOfInterestType == POI_TYPE.TOWN) {
      if (!characterData.hasExploredPosition(encounterResultData.position)) {
        characterData.exploredPositions.push(encounterResultData.position);
        //nactu si data o PoI abych mohl ulozit memory teto PoI
        const pointOfInterestDb = admin.firestore().collection('_metadata_zones').doc(characterData.position.zoneId).collection("locations").doc(characterData.position.locationId).collection("pointsOfInterest").doc(characterData.position.pointOfInterestId);//.withConverter(LocationConverter);
        var pointOfInterestDoc = await t.get(pointOfInterestDb);
        const pointOfInterestData: PointOfInterest = pointOfInterestDoc.data();
        characterData.updateMemmoryMap(pointOfInterestData);
      }



      //zapisu killy 
      encounterResultData.enemies.forEach(enemy => { characterData.recordMonsterKill(enemy.id) });

      //Dam expy
      //let characterLevel = characterData.stats.level; //ulozim si level driv nez awardnu expy
      var expGained = getExpAmountFromEncounterForGivenCharacterLevel(encounterResultData.enemies, characterData.stats.level)
      const gainedNewLevel = characterData.giveExp(expGained);

      //pokud sem dostal level, ta ho chci updatnout i v CharacterPreview 
      let playerData: PlayerData | undefined;
      const playerDb = admin.firestore().collection('players').doc(characterData.userUid);
      if (gainedNewLevel) {
        console.log("dostal se level, upatuju preview!");
        const playerDoc = await t.get(playerDb);
        playerData = playerDoc.data();
        if (playerData != undefined) {
          playerData.characters.forEach(characterPreview => {
            if (characterPreview.characterUid == characterData.uid) {
              characterPreview.level = characterData.stats.level;
              console.log("nasel sem preview " + characterPreview.characterUid + "level : " + characterPreview.level);
            }
          });
        }
      }

      const setOperations: SetOperation[] = [];

      //Pridame killy/damage do leaderboardu...1x write 1xread za kazdeho combatanta....TODO: asi mit jen urcite casove okna kdy bezi monter slain leaderboard....a tady bych teda musel readnout jen jednou, ale pokazde jestli bezi leaderboard..
      //   if (expGained > 0) //jen kdyz ti do nejake expy jinak killy nepocitam
      await updateMyMonsterKillsAndDamageDoneLeaderboards_GET_SET(t, encounterResultData, myCombatResultEntry, characterData);
      //await updateMyScoreAtLeaderboard_GET_SET(t,"MONSTER_KILLS_"+encounterResultData.position.locationId, characterData, encounterResultData.enemies.length);

      //pokud is byl v dungeonu
      if (encounterResultData.dungeonData != null) {

        //pokud si tenhle dungeon dokoncil a neni v tvych dokoncenych dam ho tam
        if (encounterResultData.dungeonData.isFinished) {
          if (!characterData.dungeonsFinished.includes(encounterResultData.dungeonData.dungeonId)) {
            characterData.dungeonsFinished.push(encounterResultData.dungeonData.dungeonId);

          }
        }
        //pokud to byl endless dungeon, ulotim floor ktery si dosahl do leaderborass
        if (encounterResultData.dungeonData.isEndlessDungeon) {
          //updatnem leaderboards
          setOperations.push(await setScoreToLeaderboard(t, characterData, encounterResultData.dungeonData.dungeonId, encounterResultData.dungeonData.tier));
        }
      }

      //pokud si dostal level ulozim do CHARACTER LEVEL leaderboards
      if (gainedNewLevel)
        setOperations.push(await setMyCharacterLevelLeaderboards(t, characterData));

      //zkontroluju jestli uz vsichni nevybrali odmeny, pokud jo deletnu zaznam uplne, jinak jen updatnu
      if (encounterResultData.combatantsWithUnclaimedRewardsList.length == 0) {
        t.delete(encounterResultsDb);
      }
      else {
        t.set(encounterResultsDb, JSON.parse(JSON.stringify(encounterResultData)), { merge: true });
      }

      if (playerData != undefined) {
        //console.log("savuju do dB sem preview " + playerData.characters[2].uid + "level : " + playerData.characters[2].level);
        t.set(playerDb, JSON.parse(JSON.stringify(playerData)), { merge: true });
      }
      else
        console.log("playerData je null");


      //pokud sme updatnuli poi Perk claim count... musime setnou do DB
      if (PointOfInterestServerData != null)
        t.set(PointOfInterestServerDataDb, JSON.parse(JSON.stringify(PointOfInterestServerData)), { merge: true });

      //pokud sme updatnuli rare Perk claim count... musime setnou do DB
      if (locationDefinitionData != null)
        t.set(locationDefinitionDb, JSON.parse(JSON.stringify(locationDefinitionData)), { merge: true });

      // Execute leaderboards sets
      for (const op of setOperations) {
        await t.set(op.docRef, op.data, op.options);
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

  _encounterResultData.bonusLoot.forEach(loot => {
    //..jeste nema zadneho vyherce?
    if (loot.characterWhoWillHaveThis == null) {
      //nekdo ten item chce, vylosuju vyhrece mezi wanterama
      if (loot.charactersWhoWantThis.length > 0) {

        let randomGuy = loot.charactersWhoWantThis[randomIntFromInterval(0, loot.charactersWhoWantThis.length - 1)];
        loot.characterWhoWillHaveThis = randomGuy;// new EncounterResultCombatant(randomGuy.uid, randomGuy.displayName, randomGuy.characterClass, randomGuy.level, randomGuy.expGainedEstimate);
      }
      //nikdo ten item nechce, budeme losovat mezi vsemi combatantami
      else {
        let randomGuy = _encounterResultData.combatantsData[randomIntFromInterval(0, _encounterResultData.combatantsData.length - 1)];
        loot.characterWhoWillHaveThis = randomGuy;//new EncounterResultCombatant(randomGuy.uid, randomGuy.displayName, randomGuy.characterClass, randomGuy.level, randomGuy.expGainedEstimate);
      }
    }
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
          content.content = new ContentContainer(content.content.content, content.content.contentEquip); //kvuli tomu ze nemam withConverter...

          if (content.content.getItem().uid == wantedItemId) {
            content.charactersWhoWantThis.push(myCombatantDataEntry);
          }
        });
      });

      //priradim te jako wantera k tebou vybranemi itemu
      encounterResultData.bonusLoot.forEach(loot => {
        loot.content = new ContentContainer(loot.content.content, loot.content.contentEquip); //kvuli tomu ze nemam withConverter...
        if (loot.content.getItem().uid == wantedItemId) {
          loot.charactersWhoWantThis.push(myCombatantDataEntry);
        }
      });


      //zkontroluju jestli uz vsichni nevybrali want item, pokud jo rollnu nahodne vyherce vsem itemum a nastvaim encounter result jako claimable 
      if (encounterResultData.combatantsWithUnchoosenWantedItemList.length == 0) {
        chooseWinnersOfLoot(encounterResultData);
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