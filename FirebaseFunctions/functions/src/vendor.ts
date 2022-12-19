
// [START import]
import * as functions from "firebase-functions";
import { ContentContainer, CharacterDocument, characterDocumentConverter } from ".";
import {  QuerryForPointOfInterestCharacterIsAt } from "./worldMap";

const admin = require('firebase-admin');
// // [END import]



export class Vendor {
  constructor(
    public id: string,
    public displayName: string,
  //  public position: WorldPosition,
    public goods: VendorGood[]
    //  public itemsSimple: InventoryItemSimple[],
    //   public itemsEquip: Equip[]
  ) { }
}

export class VendorGood {
  constructor(
    public uid: string,
    public sellPrice: number,
    public content: ContentContainer,
  ) { }

}


exports.buyVendorItems = functions.https.onCall(async (data, context) => {

  const callerCharacterUid = data.characterUid;
  const vendorUid = data.vendorUid;
  const vendorItemsToBuyUids: string[] = data.vendorItemsToBuyUids;

  const characterDb = await admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);
  // const vendorDb = await admin.firestore().collection('_metadata_vendors').doc(vendorUid);

  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {

      const characterDoc = await t.get(characterDb);
      let characterData: CharacterDocument = characterDoc.data();


      const pointOfInterestData = await QuerryForPointOfInterestCharacterIsAt(t, characterData);

      let vendorData = pointOfInterestData.getVendorById(vendorUid);

      // if(!characterData.isOnSameWorldPosition(vendorData.position))
      // throw("You cant trade with vendor at diffrent world position!")

      let totalPurchasePrice: number = 0;


      //Najdu vsechny itemy co chci koupit u vendora a pridam si je do inventare a spocitam celkovou cenu
      for (let index = 0; index < vendorItemsToBuyUids.length; index++) {
        for (var i = vendorData.goods.length - 1; i >= 0; i--) {
          if (vendorData.goods[i].uid == vendorItemsToBuyUids[index]) {
            totalPurchasePrice += vendorData.goods[i].sellPrice;
            characterData.addContentToInventory(vendorData.goods[i].content, true, false);
            break;
          }

        }
      }

      // for (var i = vendorData.itemsEquip.length - 1; i >= 0; i--) {
      //   if (vendorItemsToBuyUids.includes(vendorData.itemsEquip[i].uid)) {
      //     totalPurchasePrice += vendorData.itemsEquip[i].sellPrice;
      //     vendorData.itemsEquip[i].uid = firestoreAutoId();
      //     characterData.addEquipToInventory(vendorData.itemsEquip[i]);
      //     break;
      //   }
      // }

      // for (var i = vendorData.itemsSimple.length - 1; i >= 0; i--) {
      //   if (vendorItemsToBuyUids.includes(vendorData.itemsSimple[i].uid)) {
      //     totalPurchasePrice += vendorData.itemsSimple[i].sellPrice * vendorData.itemsSimple[i].amount;
      //     vendorData.itemsSimple[i].uid = firestoreAutoId();
      //     characterData.addItemSimpleToInventory(vendorData.itemsSimple[i]);
      //     break;
      //   }
      // }

      console.log("Total pruchase price: " + totalPurchasePrice);
      if (totalPurchasePrice <= characterData.currency.silver) {
        characterData.currency.silver -= totalPurchasePrice;
      }
      else {
        throw ("Not enough Silver for purchase, you have " + characterData.currency.silver + " but price of all items you want to buy is : " + totalPurchasePrice);
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


  // [END allAdd]