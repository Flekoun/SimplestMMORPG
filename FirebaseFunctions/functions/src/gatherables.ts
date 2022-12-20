import * as functions from "firebase-functions";
// [START import]

import { CharacterDocument, characterDocumentConverter, ContentContainer, CONTENT_TYPE, SimpleTally, WorldPosition } from ".";
import { generateContentItemSimple, ITEMS, RARITY } from "./equip";
import {  randomIntFromInterval } from "./general2";
import { getLocationsForZoneId, getPoIsForLocationId, ZONE } from "./worldMap";

const admin = require('firebase-admin');
// // [END import]


export const enum PROFESSION {
  HERBALISM = "HERBALISM",
  MINING = "MINING"
}

export const enum GATHERABLE_TYPE {
  COPPER_VEIN = "COPPER_VEIN",
  MARIGOLD_SPOT = "MARIGOLD_SPOT",
  KINGSLEAF_SPOT = "KINGSLEAF_SPOT"
}

export class Gatherable {
  constructor(
    public uid: string,
    public gatherableType: string,
    public position: WorldPosition,
    public professionNeeded: SimpleTally[],
    public rarity: string
  ) { }
}



function generateGatherableContent(_gatherable: Gatherable): ContentContainer[] {

  let contents: ContentContainer[] = [];

  switch (_gatherable.gatherableType) {
    case GATHERABLE_TYPE.COPPER_VEIN:
      contents.push(new ContentContainer(CONTENT_TYPE.ITEM, generateContentItemSimple(ITEMS.COPPER_ORE, 1), undefined, undefined, undefined));
      break;
    case GATHERABLE_TYPE.MARIGOLD_SPOT:
      contents.push(new ContentContainer(CONTENT_TYPE.ITEM, generateContentItemSimple(ITEMS.MARIGOLD, 1), undefined, undefined, undefined));
      break;
    case GATHERABLE_TYPE.MARIGOLD_SPOT:
      contents.push(new ContentContainer(CONTENT_TYPE.ITEM, generateContentItemSimple(ITEMS.KINGSLEAF, 1), undefined, undefined, undefined));
      break;

    default:
      break;
  }
  return contents;
}

exports.claimGatherable = functions.https.onCall(async (data, context) => {

  const callerCharacterUid = data.characterUid;
  const gatherableUid = data.gatherableUid;
  const characterDb = await admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);
  const gatherableDb = await admin.firestore().collection('gatherables').doc(gatherableUid);
  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {

      const characterDoc = await t.get(characterDb);
      let characterData: CharacterDocument = characterDoc.data();

      const gatherableDoc = await t.get(gatherableDb);
      let gatherableData: Gatherable = gatherableDoc.data();
      //zkontroluju jestli mas profesi na to gathernout
      gatherableData.professionNeeded.forEach(profNeeded => {
        let hasProfession = false;
        characterData.professions.forEach(prof => {

          if (prof.id == profNeeded.id)
            if (prof.count >= profNeeded.count)
              hasProfession = true;

        });
        if (hasProfession == false) {
          let skillsRequired = "";
          gatherableData.professionNeeded.forEach(element => {
            skillsRequired+= element.count + " " + element.id +" ";
          });
          throw "You need "+ skillsRequired  +"to gather this!";
        }
      });

      //pokud ano dam ti obsah gatherablu
      generateGatherableContent(gatherableData).forEach(content => {
        characterData.addContentToInventory(content, true, false);
      });

      //zvednu profesi, zatim vzdy o 1 na 100%
      gatherableData.professionNeeded.forEach(element => {
        characterData.increaseProfessionSkill(element.id, 1);

      });

      t.delete(gatherableDb);
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
  return SpawnGatherables();
});

export async function SpawnGatherables() {

  const CHANGE_TO_REMOVE_LINGERING_GATHERABLE = 0.75;  // sance na smazani starych gatherable
  const TOTAL_GATHERABLES_AMOUNT = 10; //celkovy pocet gatherables ktere maji byt ve svete po spawnu

  const gatherableDb = await admin.firestore().collection('gatherables');

  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {

      // const gatherableDoc = await t.get(gatherableDb);
      // let gatherableData: Gatherable = gatherableDoc.data();
      console.log("A");
      //ziskam stavajici nesebrane gatherables a X% z nich promazu
      let lingeringGatherablesCount = 0;
      await t.get(gatherableDb).then(querry => {
        querry.docs.forEach(async doc => {
          if (Math.random() <= CHANGE_TO_REMOVE_LINGERING_GATHERABLE)//50% na smazani 
            await t.delete(gatherableDb.doc(doc.id));
          else
            lingeringGatherablesCount++;
        });
      });
      console.log("B");
      //doplnim gatherables do sveta
      const gatherablesToSpawn = TOTAL_GATHERABLES_AMOUNT - lingeringGatherablesCount;

      for (let index = 0; index < gatherablesToSpawn; index++) {

        const locs = getLocationsForZoneId(ZONE.DUNOTAR);
        let choosenLoc = locs[randomIntFromInterval(0, locs.length - 1)];

        let pois = getPoIsForLocationId(choosenLoc);
        let choosenPoi = pois[randomIntFromInterval(0, pois.length - 1)];

        const position = new WorldPosition(ZONE.DUNOTAR, choosenLoc, choosenPoi);
        let newDocument = gatherableDb.doc();
        let gatherable: Gatherable | null = null;

        if (Math.random() > 0.33)
          gatherable = new Gatherable(newDocument.id, GATHERABLE_TYPE.MARIGOLD_SPOT, position, [new SimpleTally(PROFESSION.HERBALISM, 0)], RARITY.COMMON);
        else if (Math.random() > 0.1)
          gatherable = new Gatherable(newDocument.id, GATHERABLE_TYPE.KINGSLEAF_SPOT, position, [new SimpleTally(PROFESSION.HERBALISM, 25)], RARITY.UNCOMMON);
        else
          gatherable = new Gatherable(newDocument.id, GATHERABLE_TYPE.COPPER_VEIN, position, [new SimpleTally(PROFESSION.MINING, 0)], RARITY.UNCOMMON);

        if (gatherable != null)
          await t.set(newDocument, JSON.parse(JSON.stringify(gatherable)));

      }
      console.log("C");
      return "New gatherables spawned : " + gatherablesToSpawn;
    });


    console.log('Transaction success', result);
    return result;
  } catch (e) {
    console.log('Transaction failure:', e);
    throw new functions.https.HttpsError("aborted", "Error : " + e);
  }

}

  // [END allAdd]