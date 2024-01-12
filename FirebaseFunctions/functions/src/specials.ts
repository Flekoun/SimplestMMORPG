
const admin = require('firebase-admin');
import * as functions from "firebase-functions";
import { ChapelInfo, CharacterDocument, characterDocumentConverter, compareWorldPosition, generateContentContainer } from ".";
import { POI_SPECIALS, PointOfInterest, PointOfInterestConverter, QuerryForPointOfInterestCharacterIsAt } from "./worldMap";
import { EQUIP_SLOT_ID, ITEMS, QuerryForSkillDefinitions, RARITY, generateEquip } from "./equip";
import { getRandomCurseAsCombatSkill } from "./skills";

export const enum BLESS {
  UNWEARIED = "UNWEARIED", // no fatigue from fleeing... (mohl bych pridat zajimave veci jakoze jen kdyz jsi posledni a naivu natp..)
  LASTING_HAND = "LASTING_HAND", // Single-use cards remain in the deck after use, but their mana cost increases by 1.
  BEHEMOND = "BEHEMOND", // Max HP increased by 30 but each time you die(flee), gain a curse
  GLASS_CANNON = "GLASS_CANNON",// Max Mana and regen increased by 1 but your max hp is lowered by 60 HP and you generate double the threat
  FOOD_LIMIT_INCREASE = "FOOD_LIMIT_INCREASE",//+1 food limit but you cant eat any organs
  ASSASSIN = "ASSASSIN", //+5% crit chance ,-30 HP
  TOWER_GUARD = "TOWER_GUARD" // you retain your shiled amount between turns...ale kdyz je sshiled broken suffer 30% dmg and lose all threat


  //BULWARK = "BULWARK"//you retain your shiled amount between turns hand size lowered by 1
  //BULWARK ="BULWARK" - you retain your shiled amount between turns...ale kdyz je sshiled broken suffer dmg

}


exports.innCarriage = functions.https.onCall(async (data, context) => {

  const callerCharacterUid = data.characterUid;
  const targetInnId = data.targetInnId;
  const characterDb = await admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);

  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {

      const characterDoc = await t.get(characterDb);
      let characterData: CharacterDocument = characterDoc.data();



      const destinationPoIDb = admin.firestore().collection('_metadata_zones').doc(characterData.position.zoneId).collection("locations").doc(characterData.position.locationId).collection("pointsOfInterest").doc(targetInnId);
      const destinationPoIDoc = await t.get(destinationPoIDb.withConverter(PointOfInterestConverter));
      let destinationPoIData: PointOfInterest = destinationPoIDoc.data();

      if (compareWorldPosition(destinationPoIData.worldPosition, characterData.position))
        throw ("You are already at this inn!");

      if (!destinationPoIData.specials.includes(POI_SPECIALS.INN))
        throw ("There is no Tavern at this position!");

      // console.log("destinationPoIData.worldPosition.zoneId:" + destinationPoIData.worldPosition.zoneId);
      // console.log("destinationPoIData.worldPosition.locationId:" + destinationPoIData.worldPosition.locationId);
      // console.log("destinationPoIData.worldPosition.pointOfInterestId:" + destinationPoIData.worldPosition.pointOfInterestId);

      if (!characterData.hasExploredPosition(destinationPoIData.worldPosition))
        throw ("You must first explore this position!");

      let amount: number;
      amount = Math.pow(characterData.stats.level, 4);
      // if (characterData.stats.level > 20)
      //   amount = Math.pow(Math.E, (20 * Math.log(100000) / 15));
      // else
      //   amount = Math.pow(Math.E, (characterData.innHealhRestsCount * Math.log(100000) / 15));

      amount = Math.round(amount);
      characterData.subGold(amount);


      characterData.position = destinationPoIData.worldPosition;

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


exports.innHealthRestore = functions.https.onCall(async (data, context) => {

  const callerCharacterUid = data.characterUid;
  const characterDb = await admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);

  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {

      const characterDoc = await t.get(characterDb);
      let characterData: CharacterDocument = characterDoc.data();

      characterData.innHealhRestsCount++;
      // let amount: number;

      // if (characterData.innHealhRestsCount > 10) {
      //   amount = Math.pow(Math.E, (30 * Math.log(100000) / 10));
      // } else {
      //   amount = Math.pow(Math.E, (characterData.innHealhRestsCount * Math.log(100000) / 10));
      // }

      // amount = Math.round(amount);

      // characterData.subGold(amount);
      characterData.subFatigueFlat(100);
      characterData.giveHealth(characterData.stats.totalMaxHealth);

      let skillDefinitions = await QuerryForSkillDefinitions(t);
      let curse = getRandomCurseAsCombatSkill(characterData, skillDefinitions, 0);
      characterData.addCurse(curse);
      let curse2 = getRandomCurseAsCombatSkill(characterData, skillDefinitions, 0);
      characterData.addCurse(curse2);
      let curse3 = getRandomCurseAsCombatSkill(characterData, skillDefinitions, 0);
      characterData.addCurse(curse3);
      // characterData.giveHealth(20);
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


exports.innBind = functions.https.onCall(async (data, context) => {

  const callerCharacterUid = data.characterUid;
  const characterDb = await admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);

  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {

      const characterDoc = await t.get(characterDb);
      let characterData: CharacterDocument = characterDoc.data();

      if (compareWorldPosition(characterData.homeInn, characterData.position))
        throw ("This is already your home inn");

      let amount: number;

      // if (characterData.stats.level > 30) {
      //   amount = Math.pow(Math.E, (30 * Math.log(100000) / 30));
      // } else {
      //  amount = Math.pow(Math.E, (characterData.stats.level * Math.log(10000) / 20));
      amount = Math.pow(characterData.stats.level, 3);
      //  }

      amount = Math.round(amount);

      characterData.subGold(amount);
      characterData.homeInn = characterData.position;
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



exports.chapelRemoveCurses = functions.https.onCall(async (data, context) => {

  const callerCharacterUid = data.characterUid;
  const characterDb = await admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);

  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {

      const characterDoc = await t.get(characterDb);
      let characterData: CharacterDocument = characterDoc.data();

      const pointOfInterestData = await QuerryForPointOfInterestCharacterIsAt(t, characterData);

      if (!pointOfInterestData.specials.includes(POI_SPECIALS.CHAPEL))
        throw "There is no chapel on this Point of interest!";

      let chapelInfo: ChapelInfo | null = null;

      for (const info of characterData.chapelInfo) {
        if (compareWorldPosition(info.worldPosition, characterData.position)) {
          chapelInfo = info;
          if (info.used) {
            throw "You have already used this chapel";
          }
        }

      }

      if (characterData.curses.length == 0)
        throw "You have no curses!";

      let amount: number;
      if (characterData.stats.level > 20)
        amount = Math.pow(Math.E, (20 * Math.log(100000) / 30));
      else
        amount = Math.pow(Math.E, (characterData.innHealhRestsCount * Math.log(100000) / 20));

      amount = Math.round(amount);
      characterData.subGold(amount);


      if (chapelInfo != null)
        chapelInfo.used = true
      else
        throw "Could not find info about chapel on position : " + characterData.position;

      characterData.curses = [];

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


exports.chapelRecieveBless = functions.https.onCall(async (data, context) => {

  const callerCharacterUid = data.characterUid;
  const characterDb = await admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);

  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {

      const characterDoc = await t.get(characterDb);
      let characterData: CharacterDocument = characterDoc.data();

      const pointOfInterestData = await QuerryForPointOfInterestCharacterIsAt(t, characterData);

      if (!pointOfInterestData.specials.includes(POI_SPECIALS.CHAPEL))
        throw "There is no chapel on this Point of interest!";

      let chapelInfo: ChapelInfo | null = null;

      for (const info of characterData.chapelInfo) {
        if (compareWorldPosition(info.worldPosition, characterData.position)) {
          chapelInfo = info;
          if (info.used) {
            throw "You have already used this chapel";
          }
        }

      }

      if (chapelInfo != null)
        chapelInfo.used = true
      else
        throw "Could not find info about chapel on position : " + characterData.position;

      characterData.addBless(chapelInfo.blessId);

      t.set(characterDb, JSON.parse(JSON.stringify(characterData)), { merge: true });

      return "OK";
    });

    console.log('Transaction success', result);
    return result;
  } catch (e) {
    console.log('Transaction failure:', e);
    throw new functions.https.HttpsError("aborted", e as string);
  }



});



exports.chapelGift = functions.https.onCall(async (data, context) => {

  const callerCharacterUid = data.characterUid;
  const characterDb = await admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);

  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {

      const characterDoc = await t.get(characterDb);
      let characterData: CharacterDocument = characterDoc.data();

      const pointOfInterestData = await QuerryForPointOfInterestCharacterIsAt(t, characterData);

      if (!pointOfInterestData.specials.includes(POI_SPECIALS.CHAPEL))
        throw "There is no chapel on this Point of interest!";

      let chapelInfo: ChapelInfo | null = null;

      for (const info of characterData.chapelInfo) {
        if (compareWorldPosition(info.worldPosition, characterData.position)) {
          chapelInfo = info;
          if (info.used) {
            throw "You have already used this chapel";
          }
        }

      }

      if (chapelInfo != null)
        chapelInfo.used = true
      else
        throw "Could not find info about chapel on position : " + characterData.position;


      let skillDefinitions = await QuerryForSkillDefinitions(t);

      if (Math.random() < 0.8) {
        const content = generateContentContainer(generateEquip(characterData.stats.level, RARITY.RARE, EQUIP_SLOT_ID.ANY, characterData.characterClass, skillDefinitions));;
        characterData.addContentToInventory(content, true, false);
      }
      else {
        const content = generateContentContainer(generateEquip(characterData.stats.level, RARITY.EPIC, EQUIP_SLOT_ID.ANY, characterData.characterClass, skillDefinitions));;
        characterData.addContentToInventory(content, true, false);
      }

      t.set(characterDb, JSON.parse(JSON.stringify(characterData)), { merge: true });

      return "OK";
    });

    console.log('Transaction success', result);
    return result;
  } catch (e) {
    console.log('Transaction failure:', e);
    throw new functions.https.HttpsError("aborted", e as string);
  }



});


exports.treasureOpenFree = functions.https.onCall(async (data, context) => {

  const callerCharacterUid = data.characterUid;
  const characterDb = await admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);

  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {

      const characterDoc = await t.get(characterDb);
      let characterData: CharacterDocument = characterDoc.data();

      const pointOfInterestData = await QuerryForPointOfInterestCharacterIsAt(t, characterData);

      if (!pointOfInterestData.specials.includes(POI_SPECIALS.TREASURE))
        throw "There is no treasure on this Point of interest!";

      if (characterData.treasuresClaimed.includes(pointOfInterestData.worldPosition))
        throw "You have already claimed this treasure!";

      /*

    //Alternativne to posilat do inboxu?
    let roll = Math.random();
    if (roll < 0.2) {
      //add curse
      let skillDefinitions = await QuerryForSkillDefinitions(t);
      let curse = getRandomCurseAsCombatSkill(characterData, skillDefinitions, 0);
      characterData.addCurse(curse);
    }
    else if (roll < 0.5) {
      //add 1 Gold coin
      characterData.addGold(10000)
    }
    else {
      //legendary item
      let skillDefinitions = await QuerryForSkillDefinitions(t);
      const content = generateContentContainer(generateEquip(characterData.stats.level, RARITY.LEGENDARY, EQUIP_SLOT_ID.ANY, characterData.characterClass, skillDefinitions));;
      characterData.addContentToInventory(content, true, true);
    }
*/
      let skillDefinitions = await QuerryForSkillDefinitions(t);
      const content = generateContentContainer(generateEquip(characterData.stats.level, RARITY.UNCOMMON, EQUIP_SLOT_ID.ANY, characterData.characterClass, skillDefinitions));;
      characterData.addContentToInventory(content, true, true);
      characterData.treasuresClaimed.push(pointOfInterestData.worldPosition);

      t.set(characterDb, JSON.parse(JSON.stringify(characterData)), { merge: true });

      return "OK";
    });

    console.log('Transaction success', result);
    return result;
  } catch (e) {
    console.log('Transaction failure:', e);
    throw new functions.https.HttpsError("aborted", e as string);
  }

});


exports.treasureOpenForCurse = functions.https.onCall(async (data, context) => {

  const callerCharacterUid = data.characterUid;
  const characterDb = await admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);

  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {

      const characterDoc = await t.get(characterDb);
      let characterData: CharacterDocument = characterDoc.data();

      const pointOfInterestData = await QuerryForPointOfInterestCharacterIsAt(t, characterData);

      if (!pointOfInterestData.specials.includes(POI_SPECIALS.TREASURE))
        throw "There is no treasure on this Point of interest!";

      if (characterData.treasuresClaimed.includes(pointOfInterestData.worldPosition))
        throw "You have already claimed this treasure!";

      //give curse
      let skillDefinitions = await QuerryForSkillDefinitions(t);
      let curse = getRandomCurseAsCombatSkill(characterData, skillDefinitions, 0);
      characterData.addCurse(curse);


      //legendary item
      const content = generateContentContainer(generateEquip(characterData.stats.level, RARITY.RARE, EQUIP_SLOT_ID.ANY, characterData.characterClass, skillDefinitions));;
      characterData.addContentToInventory(content, true, true);

      // const content = generateContentContainer(generateEquip(characterData.stats.level, RARITY.EPIC, EQUIP_SLOT_ID.ANY, characterData.characterClass, skillDefinitions));;

      /* //Alternativne to posilat do inboxu?
       let roll = Math.random();
       if (roll < 0.5) {
         //epic item
         let skillDefinitions = await QuerryForSkillDefinitions(t);
         const content = generateContentContainer(generateEquip(characterData.stats.level, RARITY.EPIC, EQUIP_SLOT_ID.ANY, characterData.characterClass, skillDefinitions));;
         characterData.addContentToInventory(content, true, true);
       }
 
       else {
         //legendary item
         let skillDefinitions = await QuerryForSkillDefinitions(t);
         const content = generateContentContainer(generateEquip(characterData.stats.level, RARITY.LEGENDARY, EQUIP_SLOT_ID.ANY, characterData.characterClass, skillDefinitions));;
         characterData.addContentToInventory(content, true, true);
       }
 */
      characterData.treasuresClaimed.push(pointOfInterestData.worldPosition);

      t.set(characterDb, JSON.parse(JSON.stringify(characterData)), { merge: true });

      return "OK";
    });

    console.log('Transaction success', result);
    return result;
  } catch (e) {
    console.log('Transaction failure:', e);
    throw new functions.https.HttpsError("aborted", e as string);
  }

});


exports.treasureOpenWithKey = functions.https.onCall(async (data, context) => {

  const callerCharacterUid = data.characterUid;
  const characterDb = await admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);

  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {

      const characterDoc = await t.get(characterDb);
      let characterData: CharacterDocument = characterDoc.data();

      const pointOfInterestData = await QuerryForPointOfInterestCharacterIsAt(t, characterData);

      if (!pointOfInterestData.specials.includes(POI_SPECIALS.TREASURE))
        throw "There is no treasure on this Point of interest!";

      if (characterData.treasuresClaimed.includes(pointOfInterestData.worldPosition))
        throw "You have already claimed this treasure!";

      //remove key 
      characterData.removeContentFromInventoryById(ITEMS.MAGIC_KEY, 1);

      //legendary item
      let skillDefinitions = await QuerryForSkillDefinitions(t);
      const content = generateContentContainer(generateEquip(characterData.stats.level, RARITY.EPIC, EQUIP_SLOT_ID.ANY, characterData.characterClass, skillDefinitions));;
      characterData.addContentToInventory(content, true, true);

      characterData.treasuresClaimed.push(pointOfInterestData.worldPosition);

      t.set(characterDb, JSON.parse(JSON.stringify(characterData)), { merge: true });

      return "OK";
    });

    console.log('Transaction success', result);
    return result;
  } catch (e) {
    console.log('Transaction failure:', e);
    throw new functions.https.HttpsError("aborted", e as string);
  }

});