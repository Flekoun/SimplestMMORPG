
// [START import]
import * as functions from "firebase-functions";
import { CHARACTER_CLASS, CharacterDocument, characterDocumentConverter, ContentContainer, CURRENCY_ID, generateContentContainer } from ".";
import { generateContent, generateEquip, ItemIdWithAmount, QuerryForSkillDefinitions } from "./equip";
import { LEADERBOARD, incrementScoreToLeaderboard } from "./leaderboards";
import { RandomEquip } from "./questgiver";

const admin = require('firebase-admin');
// // [END import]



export const enum PROFESSION {
  HERBALISM = "HERBALISM",
  MINING = "MINING",
  ALCHEMY = "ALCHEMY",
  BLACKSMITHING = "BLACKSMITHING",
  COOKING = "COOKING",
}


export class CraftingRecipe {
  constructor(
    public id: string,
    public professionId: string,
    public professionAmountNeeded: number,
    public materials: ItemIdWithAmount[],
    public timePrice: number,
    public product: ItemIdWithAmount | undefined,
    public productContent: ContentContainer | undefined,
    public productRandomEquip: RandomEquip | undefined,

  ) { }
}

exports.craftRecipe = functions.https.onCall(async (data, context) => {

  const callerCharacterUid = data.characterUid;
  const recipeId = data.recipeId;

  const characterDb = await admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);
  const recipeDb = await admin.firestore().collection('_metadata_craftingRecipes').doc(recipeId);


  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {

      const characterDoc = await t.get(characterDb);
      let characterData: CharacterDocument = characterDoc.data();

      const recipeDoc = await t.get(recipeDb);
      let recipeData: CraftingRecipe = recipeDoc.data();



      //zkontrolujem esli mas dost skillu a profesi
      // let profession = characterData.getProfessionById(recipeData.professionId);
      // if (profession == undefined)
      //   throw ("You dont have profession " + recipeData.professionId);
      // if (profession.count < recipeData.professionAmountNeeded)
      //   throw ("Not enough profession skill you have : " + profession.count + " you need : " + recipeData.professionId);

      //odstranime mats z invtory
      for (const mat of recipeData.materials) {
        characterData.removeContentFromInventoryById(mat.itemId, mat.amount);
      }

      //zvedneme skill profesi
      // if (profession.count < profession.countMax) {
      //   characterData.getProfessionById(recipeData.professionId)!.count++;// profession.count++;
      //   //  console.log("zvedam");
      // }

      characterData.subCurrency(CURRENCY_ID.TIME, recipeData.timePrice);



      //dame produkt
      if (recipeData.product != undefined)
        characterData.addContentToInventory(generateContentContainer(generateContent(recipeData.product.itemId, recipeData.product.amount)), true, false);
      else if (recipeData.productContent != undefined)
        characterData.addContentToInventory(recipeData.productContent, true, false);
      else if (recipeData.productRandomEquip != undefined) {
        let skillDefinitions = await QuerryForSkillDefinitions(t);
        characterData.addContentToInventory(generateContentContainer(generateEquip(recipeData.productRandomEquip.mLevel, recipeData.productRandomEquip.rarity, recipeData.productRandomEquip.equipSlotId, CHARACTER_CLASS.ANY /*characterData.characterClass*/, skillDefinitions)), true, false);
      }


      //updatnem leaderboards
      let result = await incrementScoreToLeaderboard(t, characterData, LEADERBOARD.ITEMS_CRAFTED, 1);
      await t.set(result.docRef, result.data, result.options);

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


// [END allAdd]