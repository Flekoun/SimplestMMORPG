
// [START import]

// import * as functions from "firebase-functions";
// import { PerkOfferDefinition } from "./perks";
// import { Coordinates2DCartesian } from "./worldMap";
// const admin = require('firebase-admin');


// //definice stromu pro vygenerovani stromu, obsahuje list perku ktere muzou byt na kazdem flooru
// export class RewardTreeDefinition {
//   public vertices: RewardTreeVertex[]
// }

// //to co admin vygeneruje a bude ulozeno nekde na serveru spolecne pro vsechny
// export class RewardTree {
//   public vertices: RewardTreeVertex[]
// }


// export class RewardTreeVertex {
//   public id: string;
//   public nodes: RewardTreeNode[];
//   public treePosition: Coordinates2DCartesian;
//   public reward: PerkOfferDefinition
// }

// export class RewardTreeNode {
//   public idOfVertex: string
// }


// // // [END import]



// export class Trainer {
//   constructor(
//     public id: string,
//     //   public position: WorldPosition,
//     public professionHeTrains: string, // what PROFESSION he trains 
//     public professionMinAmountNeededToTrain: number, //  minimum you must have so he can train
//     public professionMaxTrainAmount: number, //  maxiumum he can train
//     public trainPrice: number

//   ) { }
// }


// exports.trainAtTrainer = functions.https.onCall(async (data, context) => {

//   const callerCharacterUid = data.characterUid;
//   const trainerUid = data.trainerUid;
//   //const vendorItemsToBuyUids: string[] = data.vendorItemsToBuyUids;

//   const characterDb = await admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);
//   // const vendorDb = await admin.firestore().collection('_metadata_vendors').doc(vendorUid);

//   try {
//     const result = await admin.firestore().runTransaction(async (t: any) => {

//       const characterDoc = await t.get(characterDb);
//       let characterData: CharacterDocument = characterDoc.data();

//       const pointOfInterest = await QuerryForPointOfInterestCharacterIsAt(t, characterData);
//       const trainer = pointOfInterest.getTrainerById(trainerUid);

//       if (characterData.currency.gold < trainer.trainPrice)
//         throw ("Not enough gold!");

//       // if (trainer.professionHeTrains == PROFESSION.BLACKSMITHING || trainer.professionHeTrains == PROFESSION.ALCHEMY)
//       if (characterData.getProfessionById(PROFESSION.BLACKSMITHING) != null)//|| characterData.getProfessionById(PROFESSION.ALCHEMY))
//         throw ("You can have only 1 profession!");

//       let characterProfession = characterData.getProfessionById(trainer.professionHeTrains);

//       if (characterProfession != null) //uz mas tu profesi
//       {
//         if (characterProfession.countMax > trainer.professionMaxTrainAmount)
//           throw ("This trainer cant train you anything new!");

//         //splnil si minimum co musis mit na trenovani
//         if (characterProfession.count >= trainer.professionMinAmountNeededToTrain) {

//           characterData.subCurrency(CURRENCY_ID.GOLD, trainer.trainPrice);
//           characterProfession.countMax = trainer.professionMaxTrainAmount;

//         }

//       }
//       else {
//         //trener trenuje novycky a davaji jim tim profesi
//         if (trainer.professionMinAmountNeededToTrain == 0) {
//           characterData.subCurrency(CURRENCY_ID.GOLD, trainer.trainPrice);

//           characterData.addProfession(trainer.professionHeTrains, trainer.professionMaxTrainAmount);
//         }
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


  // [END allAdd]