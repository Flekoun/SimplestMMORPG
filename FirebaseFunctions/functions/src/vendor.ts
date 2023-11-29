
// [START import]
import * as functions from "firebase-functions";
import { ContentContainer, CharacterDocument, characterDocumentConverter, generateContentContainer, CURRENCY_ID, validateCallerBulletProof } from ".";

import { generateContent, generateEquip, ItemIdWithAmount, QuerryForSkillDefinitions } from "./equip";
import { QuerryForPointOfInterestCharacterIsAt } from "./worldMap";
import { RandomEquip } from "./questgiver";

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

  substractStock(_vendorGoodUid: string, _amount: number) {

    let good = this.goods.find(good => good.uid == _vendorGoodUid)

    if (good) {
      if (good.stockTotalLeft >= _amount) {
        good.stockTotalLeft -= _amount;
      }
      else
        "Not enought stock left! Stock left " + good.stockTotalLeft + " but you want to substract " + _amount;
    }
    else
      throw "Could not find Vendor good : " + _vendorGoodUid
  }
}

export class VendorGood {
  constructor(
    public uid: string,
    public sellPrice: number,
    public content: ContentContainer | undefined, //pro presny equip
    public contentGenerated: ItemIdWithAmount | undefined, //pro generovany equip
    public contentRandomEquip: RandomEquip | undefined,
    public stockTotal: number,  //-1 = nekonecno
    public stockTotalLeft: number,
    public stockPerCharacter: number, //kolik si muze maximalne kazdy charakter nakoupit
    public currencyType: string   // typ meny, goldy, mosnter essence atd

  ) { }

}


exports.tradeWithVendor = functions.https.onCall(async (data, context) => {

  const callerCharacterUid = data.characterUid;
  const vendorUid = data.vendorUid;
  const vendorItemsToBuyUids: string[] = data.vendorItemsToBuyUids;
  const vendorItemsToBuyAmounts: number[] = data.vendorItemsToBuyAmounts;
  const itemsToSellUids: string[] = data.itemsToSellUids;
  const itemsToSellAmounts: number[] = data.itemsToSellAmounts;

  console.log("vendorItemsToBuyUids:" + vendorItemsToBuyUids);
  console.log("vendorItemsToBuyAmounts:" + vendorItemsToBuyAmounts);
  console.log("itemsToSellUids:" + itemsToSellUids);
  console.log("itemsToSellAmounts:" + itemsToSellAmounts);

  const characterDb = await admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);
  // const vendorDb = await admin.firestore().collection('_metadata_vendors').doc(vendorUid);

  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {

      const characterDoc = await t.get(characterDb);
      let characterData: CharacterDocument = characterDoc.data();
      validateCallerBulletProof(characterData, context);

      const pointOfInterestData = await QuerryForPointOfInterestCharacterIsAt(t, characterData);

      let vendorData = pointOfInterestData.getVendorById(vendorUid);
      let vendorDataChanged: boolean = false;
      // if(!characterData.isOnSameWorldPosition(vendorData.position))
      // throw("You cant trade with vendor at diffrent world position!")

      let totalPurchasePrice: number = 0;
      let currentyType = "";
      let isFirstItem = true;
      //Najdu vsechny itemy co chci koupit u vendora a pridam si je do inventare a spocitam celkovou cenu
      let skillDefinitions = await QuerryForSkillDefinitions(t);

      vendorItemsToBuyUids.forEach((uid, idx) => {
        const item = vendorData.goods.find(good => good.uid === uid);

        if (item) {
          if (isFirstItem) {
            currentyType = item.currencyType;
            isFirstItem = false;
          }
          if (currentyType != item.currencyType)
            throw "You are trying to buy vendor items with missmatching currency prices....this is not implemented!";

          //zkontroluju jestli mi zbyva dost personal stocku a pak i global stocku
          //    if (item.stockTotalLeft >= vendorItemsToBuyAmounts[idx] && (item.stockPerCharacter - characterData.getStockPurchasedForGivenVendorGood(vendorData.id, item.uid) >= vendorItemsToBuyAmounts[idx])) {
          if ((item.stockTotalLeft == -1 || item.stockTotalLeft >= vendorItemsToBuyAmounts[idx]) &&
            (item.stockPerCharacter == -1 || (item.stockPerCharacter - characterData.getStockPurchasedForGivenVendorGood(vendorData.id, item.uid) >= vendorItemsToBuyAmounts[idx]))) {

            characterData.addStockPurchasedForGivenVendorGood(vendorData.id, item.uid, vendorItemsToBuyAmounts[idx]);

            //pokud je totalStockLeft -1, je ho nekonecno, proto ho nemenim a nezajima me
            if (item.stockTotalLeft != -1) {
              vendorData.substractStock(item.uid, vendorItemsToBuyAmounts[idx]);
              vendorDataChanged = true;
            }


            totalPurchasePrice += item.sellPrice * vendorItemsToBuyAmounts[idx];

            if (item.content) {
              characterData.addContentToInventory(item.content, true, false);
            } else if (item.contentGenerated) {
              const generatedContent = generateContent(item.contentGenerated.itemId, vendorItemsToBuyAmounts[idx]);
              characterData.addContentToInventory(generateContentContainer(generatedContent), true, false);
            }
            else if (item.contentRandomEquip) {
              for (let index = 0; index < vendorItemsToBuyAmounts[idx]; index++) {
                characterData.addContentToInventory(generateContentContainer(generateEquip(item.contentRandomEquip.mLevel, item.contentRandomEquip.rarity, item.contentRandomEquip.equipSlotId, characterData.characterClass, skillDefinitions, undefined, undefined, false)), true, false);;

              }
            }
          }
          else
            throw "You want to buy more than what is in stock!"
        }
        else
          throw "good.uid not found : " + uid + " vendor id : " + vendorData.id;
      });
      // for (let index = 0; index < vendorItemsToBuyUids.length; index++) {
      //   for (var i = vendorData.goods.length - 1; i >= 0; i--) {
      //     if (vendorData.goods[i].uid == vendorItemsToBuyUids[index]) {
      //       totalPurchasePrice += vendorData.goods[i].sellPrice * vendorItemsToBuyAmounts[index];
      //       if (vendorData.goods[i].content != undefined)
      //         characterData.addContentToInventory(vendorData.goods[i].content!, true, false);
      //       else if (vendorData.goods[i].contentGenerated != undefined) {
      //         characterData.addContentToInventory(generateContentContainer(generateContent(vendorData.goods[i].contentGenerated!.itemId, vendorItemsToBuyAmounts[index])), true, false);
      //       }

      //       break;
      //     }

      //   }
      // }

      //prodam veci hrace

      let totalSellPrice: number = 0;

      for (var i = itemsToSellUids.length - 1; i >= 0; i--) {
        for (var j = characterData.inventory.content.length - 1; j >= 0; j--) {
          if (itemsToSellUids[i] == characterData.inventory.content[j].getItem().uid) {

            if (isFirstItem) {
              currentyType = characterData.inventory.content[j].getItem().currencyType;
              isFirstItem = false;
            }
            if (currentyType != characterData.inventory.content[j].getItem().currencyType)
              throw "You are trying to sell vendor items with missmatching currency prices....this is not implemented!";

            totalSellPrice += characterData.inventory.content[j].getItem().sellPrice * itemsToSellAmounts[i];//characterData.inventory.content[i].getItem().amount;
            if (itemsToSellAmounts[i] == characterData.inventory.content[j].getItem().amount) { //prodavam vse
              characterData.inventory.content.splice(j, 1);
              characterData.inventory.capacityLeft++;
            }
            else
              characterData.inventory.content[j].getItem().amount -= itemsToSellAmounts[i];

            break;
          }
        }
      }

      console.log("Total sell price: " + totalSellPrice);
      characterData.addCurrency(currentyType, totalSellPrice);


      console.log("Total pruchase price: " + totalPurchasePrice);
      // if (totalPurchasePrice <= characterData.currency.gold) {
      characterData.subCurrency(currentyType, totalPurchasePrice);
      //}
      //else {
      // throw ("Not enough gold");
      //throw ("Not enough Gold for purchase, you have " + characterData.currency.gold + " with sales but price of all items you want to buy is : " + totalPurchasePrice);
      //   }

      t.set(characterDb, JSON.parse(JSON.stringify(characterData)), { merge: true });

      //pokud sme zmenili mnostvi stocku left u Vendora (udelali sme nakup), musime ulozit cely point of interest do DB
      if (vendorDataChanged) {
        const pointOfInterestDb = admin.firestore().collection('_metadata_zones').doc(characterData.position.zoneId).collection("locations").doc(characterData.position.locationId).collection("pointsOfInterest").doc(characterData.position.pointOfInterestId);
        t.set(pointOfInterestDb, JSON.parse(JSON.stringify(pointOfInterestData)), { merge: true });
      }

      return "OK";
    });

    console.log('Transaction success', result);
    return result;
  } catch (e) {
    console.log('Transaction failure:', e);
    throw new functions.https.HttpsError("aborted", e as string);
  }


});

// exports.buyVendorItems = functions.https.onCall(async (data, context) => {

//   const callerCharacterUid = data.characterUid;
//   const vendorUid = data.vendorUid;
//   const vendorItemsToBuyUids: string[] = data.vendorItemsToBuyUids;
//   const vendorItemsToBuyUAmounts: string[] = data.vendorItemsToBuyUAmounts;

//   const characterDb = await admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);
//   // const vendorDb = await admin.firestore().collection('_metadata_vendors').doc(vendorUid);

//   try {
//     const result = await admin.firestore().runTransaction(async (t: any) => {

//       const characterDoc = await t.get(characterDb);
//       let characterData: CharacterDocument = characterDoc.data();


//       const pointOfInterestData = await QuerryForPointOfInterestCharacterIsAt(t, characterData);

//       let vendorData = pointOfInterestData.getVendorById(vendorUid);

//       // if(!characterData.isOnSameWorldPosition(vendorData.position))
//       // throw("You cant trade with vendor at diffrent world position!")

//       let totalPurchasePrice: number = 0;

//       //Najdu vsechny itemy co chci koupit u vendora a pridam si je do inventare a spocitam celkovou cenu
//       for (let index = 0; index < vendorItemsToBuyUids.length; index++) {
//         for (var i = vendorData.goods.length - 1; i >= 0; i--) {
//           if (vendorData.goods[i].uid == vendorItemsToBuyUids[index]) {
//             totalPurchasePrice += vendorData.goods[i].sellPrice;
//             if (vendorData.goods[i].content != undefined)
//               characterData.addContentToInventory(vendorData.goods[i].content!, true, false);
//             else if (vendorData.goods[i].contentGenerated != undefined) {
//               characterData.addContentToInventory(generateContentContainer(generateContent(vendorData.goods[i].contentGenerated!.itemId, vendorData.goods[i].contentGenerated!.amount)), true, false);
//             }

//             break;
//           }

//         }
//       }


//       console.log("Total pruchase price: " + totalPurchasePrice);
//       if (totalPurchasePrice <= characterData.currency.gold) {
//         characterData.currency.gold -= totalPurchasePrice;
//       }
//       else {
//         throw ("Not enough Gold for purchase, you have " + characterData.currency.gold + " but price of all items you want to buy is : " + totalPurchasePrice);
//       }

//       t.set(characterDb, JSON.parse(JSON.stringify(characterData)), { merge: true });

//       return "OK";
//     });


//     console.log('Transaction success', result);
//     return result;
//   } catch (e) {
//     console.log('Transaction failure:', e);
//     throw new functions.https.HttpsError("aborted", "Error : " + e);
//   }


// });


// exports.sellInventoryItems = functions.https.onCall(async (data, context) => {

//   const callerCharacterUid = data.characterUid;
//   const inventoryItemsToSellUids: string[] = data.inventoryItemsToSellEquipUids;

//   const characterDb = await admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);

//   try {
//     const result = await admin.firestore().runTransaction(async (t: any) => {

//       const characterDoc = await t.get(characterDb);
//       let characterData: CharacterDocument = characterDoc.data();



//       let totalSellPrice: number = 0;

//       for (var i = characterData.inventory.content.length - 1; i >= 0; i--) {
//         if (inventoryItemsToSellUids.includes(characterData.inventory.content[i].getItem().uid)) {
//           totalSellPrice += characterData.inventory.content[i].getItem().sellPrice * characterData.inventory.content[i].getItem().amount;
//           characterData.inventory.content.splice(i, 1);
//           characterData.inventory.capacityLeft++;
//         }
//       }

//       console.log("Total sell price: " + totalSellPrice);
//       characterData.addCurrency(CURRENCY_ID.GOLD, totalSellPrice);

//       t.set(characterDb, JSON.parse(JSON.stringify(characterData)), { merge: true });

//       return "OK";
//     });


//     console.log('Transaction success', result);
//     return result;
//   } catch (e) {
//     console.log('Transaction failure:', e);
//     throw new functions.https.HttpsError("aborted", "Error : " + e);
//   }


// });


// [END allAdd]