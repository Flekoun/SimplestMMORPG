
// [START import]
import * as functions from "firebase-functions";
import { ContentContainer, CharacterDocument, characterDocumentConverter, SimpleTally, CHARACTER_CLASS, generateContentContainer, PlayerData } from ".";
import { generateContent, generateEquip, ItemIdWithAmount, QuerryForSkillDefinitions } from "./equip";

import { PointOfInterest, PointOfInterestConverter } from "./worldMap";
import { setMyCharacterLevelLeaderboards } from "./leaderboards";
const admin = require('firebase-admin');
// // [END import]

export class Questgiver {
  constructor(

    public id: string,
    // public position: WorldPosition,
    public minLevel: number, //slouzi k limitaci zobrazeni/splneni questu
    public qLevel: number, //slouzi k definici jakeho levelu maji byt random itemy co generuje a kolik expu dostane
    public killsRequired: SimpleTally[],
    public rewards: RewardClassSpecific[],
    public hasExpireDate: boolean,
    public expireDate: string,
    public itemsRequired: SimpleTally[],
    public rewardsRandomEquip: RandomEquip[],
    public prereqQuests: string[],
    public expRewardPerLevel: number,
    public rewardsGenerated: ItemIdWithAmount[] | undefined
    //TODO: jeste pridat QuestfiverRewardItemDropDefinition  - pro itemy obyc co nejsou ani konkretni equip, ani random equip ale proste jen item jako jabko nebo lahvicka?
  ) { }

}



export class RewardClassSpecific {
  constructor(
    public characterClassIds: string[],
    public content: ContentContainer,
  ) { }

}

export class RandomEquip {
  constructor(
    public rarity: string,
    public equipSlotId: string,
    public mLevel: number

  ) { }

}

exports.claimQuestgiverReward = functions.https.onCall(async (data, context) => {

  //TODO : nekotroluju vubec na prereqQuests.....

  const callerCharacterUid = data.characterUid;
  const questgiverUid = data.questgiverUid;

  const callerCharacterDb = admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);

  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {

      const callerCharacterDoc = await t.get(callerCharacterDb);
      let callerCharacterData: CharacterDocument = callerCharacterDoc.data();

      const pointOfInterestDb = admin.firestore().collection('_metadata_zones').doc(callerCharacterData.position.zoneId).collection("locations").doc(callerCharacterData.position.locationId).collection("pointsOfInterest").doc(callerCharacterData.position.pointOfInterestId).withConverter(PointOfInterestConverter);//.doc(questgiverUid);

      const pointOfInterestDoc = await t.get(pointOfInterestDb);
      let pointOfInterestData: PointOfInterest = pointOfInterestDoc.data();

      console.log("poi: " + pointOfInterestData.id);
      //   const questgiverDoc = await t.get(questgiverDb);
      //  const poi = locationData.getPointOfInterestById(callerCharacterData.position.pointOfInterestId);
      const questgiverData = pointOfInterestData.getQuestgiverById(questgiverUid);// questgiverDoc.data();


      //zkontroluju jestli jste ve stejne lokaci...ziskavam data lokace na ktere je hrac, timpadem nemuzu ziskat jine QG nez ty co jsou na lokaci hrace....
      // if (!callerCharacterData.isOnSameWorldPosition(  questgiverData.position))
      //   throw ("Questgiver : " + questgiverData.id + " is not at same location as you are! Cannot claim rewards!");

      //zkontroluju jesttli jsi uz neclaimnul reward od tohodle questgivera
      if (callerCharacterData.questgiversClaimed.includes(questgiverData.id))
        throw ("You have already claimed reward from this quest giver!");

      //zkontroluju jesttli mas level
      if (callerCharacterData.stats.level < questgiverData.minLevel)
        throw ("Your level is too low to take this quest!");

      let reqFulfilled = false;
      //zkontroluju ze si splnil co questgiver chce...killy
      questgiverData.killsRequired.forEach(req => {
        reqFulfilled = false;
        callerCharacterData.monsterKills.forEach(kills => {
          if (kills.id == req.id) {
            if (kills.count >= req.count)
              reqFulfilled = true;
          }
        });
        if (!reqFulfilled)
          throw "You didnt fulfilled enough kills the quest!";
      });

      //zkontroluju ze si splnil co questgiver chce...itemy
      questgiverData.itemsRequired.forEach(req => {
        if (callerCharacterData.getNumberOfItemsInInventory(req.id) >= req.count) {
          callerCharacterData.removeContentFromInventoryById(req.id, req.count);
        }
        else
          throw "You dont have enought items in inventory to fullfill the quest requirements!"
      });

      //dam expy hraci
      var gainedNewLevel = callerCharacterData.giveExp(questgiverData.qLevel * questgiverData.expRewardPerLevel);



      //pokud sem dostal level, ta ho chci updatnout i v CharacterPreview 
      let playerData: PlayerData | undefined;
      const playerDb = admin.firestore().collection('players').doc(callerCharacterData.userUid);

      if (gainedNewLevel) {
        console.log("dostal se level, upatuju preview!");
        const playerDoc = await t.get(playerDb);
        playerData = playerDoc.data();
        if (playerData != undefined) {
          playerData.characters.forEach(characterPreview => {
            if (characterPreview.characterUid == callerCharacterData.uid) {
              characterPreview.level = callerCharacterData.stats.level;
              console.log("nasel sem preview " + characterPreview.characterUid + "level : " + characterPreview.level);
            }
          });
        }
      }



      //dam reward hraci
      for (const reward of questgiverData.rewards) {
        // console.log("reward_A : " + reward.content.getItem().itemId);
        if (reward.characterClassIds.includes(callerCharacterData.characterClass) || reward.characterClassIds.includes(CHARACTER_CLASS.ANY)) {
          //  console.log("reward_B : " + reward.content.getItem().itemId);
          callerCharacterData.addContentToInventory(reward.content, true, false);
          // break;
        }
      }
      //...a jeste rando equip reward dam jestli nejaky je
      if (questgiverData.rewardsRandomEquip.length > 0) {
        let skillDefinitions = await QuerryForSkillDefinitions(t);
        for (const reward of questgiverData.rewardsRandomEquip) {
          callerCharacterData.addContentToInventory(generateContentContainer(generateEquip(reward.mLevel, reward.rarity, reward.equipSlotId, callerCharacterData.characterClass, skillDefinitions)), true, false);
        }
      }
      //...a jeste rando equip reward dam jestli nejaky je NEMEL BY BYT!
      if (questgiverData.rewardsGenerated != undefined) {
        for (const reward of questgiverData.rewardsGenerated) {
          callerCharacterData.addContentToInventory(generateContentContainer(generateContent(reward.itemId, reward.amount)), true, false);
        }
      }
      //ulozim si ze sem questgivera claimul
      callerCharacterData.questgiversClaimed.push(questgiverData.id);

      //pokud si dostal level ulozim do CHARACTER LEVEL leaderboards
      if (gainedNewLevel) {
        let result = await setMyCharacterLevelLeaderboards(t, callerCharacterData);
        t.set(result.docRef, result.data, result.options);
      }

      if (playerData != undefined)
        t.set(playerDb, JSON.parse(JSON.stringify(playerData)), { merge: true });

      t.set(callerCharacterDb, JSON.parse(JSON.stringify(callerCharacterData)), { merge: true });
    });

    console.log('Transaction success', result);
    return result;
  } catch (e) {
    console.log('Transaction failure:', e);
    throw new functions.https.HttpsError("aborted", "Error : " + e);
  }

});




// [END allAdd]