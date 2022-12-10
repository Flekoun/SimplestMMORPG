
// [START import]
import * as functions from "firebase-functions";
import { ContentContainer, CharacterDocument, characterDocumentConverter, SimpleTally, WorldPosition, CONTENT_TYPE, CHARACTER_CLASS } from ".";
import { generateEquip } from "./equip";
const admin = require('firebase-admin');
// // [END import]

export class QuestgiverMeta {
  constructor(

    public uid: string,
    public position: WorldPosition,
    public displayName: string,
    public minLevel: number, //slouzi k limitaci zobrazeni/splneni questu
    public qLevel: number, //slouzi k definici jakeho levelu maji byt random itemy co generuje
    public killsRequired: SimpleTally[],
    public rewards: QuestgiverRewardsMeta[],
    public hasExpireDate: boolean,
    public expireDate: string,
    public itemsRequired: SimpleTally[],
    public rewardsRandomEquip: QuestgiverRewardRandomEquipsMeta[],
    public prereqQuests: string[]
  ) { }

}

export class QuestgiverRewardsMeta {
  constructor(
    public characterClassIds: string[],
    public content: ContentContainer,
  ) { }

}

export class QuestgiverRewardRandomEquipsMeta {
  constructor(
    public rarity: string,
    public equipSlotId: string,

  ) { }

}

exports.claimQuestgiverReward = functions.https.onCall(async (data, context) => {

  //TODO : nekotroluju vubec na prereqQuests.....

  const callerCharacterUid = data.characterUid;
  const questgiverUid = data.questgiverUid;

  const callerCharacterDb = admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);
  const questgiverDb = admin.firestore().collection('_metadata_questgivers').doc(questgiverUid);

  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {

      const callerCharacterDoc = await t.get(callerCharacterDb);
      let callerCharacterData: CharacterDocument = callerCharacterDoc.data();

      const questgiverDoc = await t.get(questgiverDb);
      let questgiverData: QuestgiverMeta = questgiverDoc.data();

      //zkontroluju jestli jste ve stejne lokaci
      if (!callerCharacterData.isOnSameWorldPosition(questgiverData.position))
        throw ("Questgiver : " + questgiverData.displayName + " is not at same location as you are! Cannot claim rewards!");

      //zkontroluju jesttli jsi uz neclaimnul reward od tohodle questgivera
      if (callerCharacterData.questgiversClaimed.includes(questgiverData.uid))
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

      //dam reward hraci
      for (const reward of questgiverData.rewards) {
       // console.log("reward_A : " + reward.content.getItem().itemId);
        if (reward.characterClassIds.includes(callerCharacterData.characterClass) || reward.characterClassIds.includes(CHARACTER_CLASS.ANY) ) {
        //  console.log("reward_B : " + reward.content.getItem().itemId);
          callerCharacterData.addContentToInventory(reward.content, true, false);
         // break;
        }
      }
      //...a jeste rando equip reward dam jestli nejaky je
      for (const reward of questgiverData.rewardsRandomEquip) {
        callerCharacterData.addContentToInventory(new ContentContainer(CONTENT_TYPE.EQUIP, undefined, generateEquip(questgiverData.qLevel, reward.rarity, reward.equipSlotId, callerCharacterData.characterClass), undefined, undefined), true, false);
      }

      //ulozim si ze sem questgivera claimul
      callerCharacterData.questgiversClaimed.push(questgiverData.uid);

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