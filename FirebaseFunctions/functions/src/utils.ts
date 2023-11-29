
// [START import]
import * as functions from "firebase-functions";
import { CHEATS_ENABLED, CharacterDocument, ContentContainer, GlobalMetadata, characterDocumentConverter, generateContentContainer, randomIntFromInterval } from ".";
import { EQUIP_SLOT_ID, QuerryForSkillDefinitions, RARITY, generateEquip } from "./equip";
import { awardSeasonalLeaderboardRewards } from "./leaderboards";


const admin = require('firebase-admin');
// // [END import]

export interface SetOperation {
  docRef: FirebaseFirestore.DocumentReference;
  data: any;
  options?: FirebaseFirestore.SetOptions;
}

export class ServerData_Economy {
  constructor(
    public goldCoinsAwardedThroughGatherables: number
  ) { }
}




export class InboxItem {
  constructor(
    public uid: string,
    public recipientUid: string,
    public content: ContentContainer,
    public messageTitle: string,
    public messageBody: string,
    public expireDate: string,


  ) { }
}

exports.addSimpleItemToDatabase = functions.https.onCall(async (data, context) => {

  const inboxItemUid = data.inboxItemUid;

  const characterDb = await admin.firestore().collection('descriptions').doc("items");
  const inboxDb = await admin.firestore().collection('inbox').doc(inboxItemUid);

  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {

      const characterDoc = await t.get(characterDb);
      let characterData: CharacterDocument = characterDoc.data();

      const inboxItemDoc = await t.get(inboxDb);
      let inboxItemData: InboxItem = inboxItemDoc.data();

      // inboxItemData.content = new ContentContainer(inboxItemData.content.contentType,inboxItemData.content.contentItem,inboxItemData.content.contentEquip,inboxItemData.content.contentCurrency,inboxItemData.content.contentFood); //kvuli tomu ze nemam withConverter...

      characterData.addContentToInventory(inboxItemData.content, true, false);
      t.delete(inboxDb);
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



exports.cheatTime = functions.https.onCall(async (data, context) => {

  if (!CHEATS_ENABLED)
    throw "Cheats are not enabled!";

  const callerCharacterUid = data.characterUid;

  const characterDb = await admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);

  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {

      const characterDoc = await t.get(characterDb);
      let characterData: CharacterDocument = characterDoc.data();

      characterData.addTime(12);

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


exports.test = functions.https.onCall(async (data, context) => {

  if (!CHEATS_ENABLED)
    throw "Cheats are not enabled!";


  const globalDataRef = admin.firestore().collection("_metadata_coreDefinitions").doc("Global");
  const globalSnapshot = await globalDataRef.get();
  let globalData: GlobalMetadata = globalSnapshot.data();

  await awardSeasonalLeaderboardRewards("CHARACTER_LEVEL", globalData.seasonNumber);
  await awardSeasonalLeaderboardRewards("DAMAGE_DONE", globalData.seasonNumber);
  await awardSeasonalLeaderboardRewards("HEALING_DONE", globalData.seasonNumber);
  await awardSeasonalLeaderboardRewards("ITEMS_CRAFTED", globalData.seasonNumber);
  return await awardSeasonalLeaderboardRewards("MONSTER_KILLS", globalData.seasonNumber);

  // const charactersRef = admin.firestore().collection("characters").where('currency.time', '<', TIME_MAX);
  // const batch = admin.firestore().batch();

  // const dataSnapshot = await charactersRef.get();

  // dataSnapshot.forEach(doc => {
  //   // const characterDoc = await t.get(characterDb);
  //   let character: CharacterDocument = doc.data();
  //   if (character.currency.time <= character.currency.timeMax - TIME_INCREMENT_PER_SCHEDULED)
  //     character.currency.time += TIME_INCREMENT_PER_SCHEDULED;
  //   else
  //     character.currency.time += character.currency.timeMax;

  //   if (character.currency.travelPoints <= character.currency.travelPointsMax - TRAVEL_POINTS_INCREMENT_PER_SCHEDULED)
  //     character.currency.travelPoints += TRAVEL_POINTS_INCREMENT_PER_SCHEDULED;
  //   else
  //     character.currency.travelPoints = character.currency.travelPointsMax;

  //   // const characterRef = admin.firestore().collection('characters').doc(doc.id);
  //   batch.update(doc.ref, JSON.parse(JSON.stringify(character)), { merge: true }); // Update the document in batch
  // });
  // return batch.commit() // Commit the batch
  //   .then(() => {
  //     console.log("Batch update completed.");
  //     console.log("Increase Time - Number of effected characters: ", dataSnapshot.size);
  //   }
  //   )
  //   .catch(err => console.error("Error: ", err));


});



exports.grantNewEquip = functions.https.onCall(async (data, context) => {

  if (!CHEATS_ENABLED)
    throw "Cheats are not enabled!";

  // 
  const callerCharacterUid = data.characterUid;
  // 
  const callerCharacterDb = admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);
  // 
  // 
  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {
      // 
      const callerCharacterDoc = await t.get(callerCharacterDb);
      let callerCharacterData: CharacterDocument = callerCharacterDoc.data();
      // 

      let skillDefinitions = await QuerryForSkillDefinitions(t);

      for (let i = 0; i < 3; i++) {
        // 

        const content = generateContentContainer(generateEquip(1, RARITY.UNCOMMON, EQUIP_SLOT_ID.ANY, callerCharacterData.characterClass, skillDefinitions));//new ContentContainer(CONTENT_TYPE.EQUIP, undefined, generateEquip(randomIntFromInterval(1, 20), RARITY.UNCOMMON, EQUIP_SLOT_ID.ANY, CHARACTER_CLASS.ANY), undefined, undefined);
        callerCharacterData.addContentToInventory(content, true, false);
      }
      // 
      for (let i = 0; i < 3; i++) {
        const content = generateContentContainer(generateEquip(1, RARITY.RARE, EQUIP_SLOT_ID.ANY, callerCharacterData.characterClass, skillDefinitions));
        callerCharacterData.addContentToInventory(content, true, false);
      }
      // 
      for (let i = 0; i < 3; i++) {
        const content = generateContentContainer(generateEquip(1, RARITY.COMMON, EQUIP_SLOT_ID.ANY, callerCharacterData.characterClass, skillDefinitions));;
        callerCharacterData.addContentToInventory(content, true, false);
      }
      for (let i = 0; i < 3; i++) {
        const content = generateContentContainer(generateEquip(1, RARITY.EPIC, EQUIP_SLOT_ID.ANY, callerCharacterData.characterClass, skillDefinitions));;
        callerCharacterData.addContentToInventory(content, true, false);
      }
      // for (let i = 0; i < 3; i++) {
      //   const content = generateContentContainer(generateEquip(randomIntFromInterval(1, 20), RARITY.LEGENDARY, EQUIP_SLOT_ID.ANY, callerCharacterData.characterClass, skillDefinitions));;
      //   callerCharacterData.addContentToInventory(content, true, false);
      //  }
      // 
      // for (let i = 0; i < 3; i++) {

      //   const content = generateContentContainer(generateEquip(randomIntFromInterval(1, 20), RARITY.LEGENDARY, EQUIP_SLOT_ID.ANY, CHARACTER_CLASS.ANY, skillDefinitions));;
      //   callerCharacterData.addContentToInventory(content, true, false);
      // }
      // 
      t.set(callerCharacterDb, JSON.parse(JSON.stringify(callerCharacterData)), { merge: true });
      // 
      // 
    });
    // 
    // 
    console.log('Transaction success', result);
    return result;
  } catch (e) {
    console.log('Transaction failure:', e);
    throw new functions.https.HttpsError("aborted", "Error : " + e);
  }
  // 
});
// [END allAdd]