
// [START import]
import * as functions from "firebase-functions";
import { CharacterDocument, characterDocumentConverter, CURRENCY_ID } from ".";

import { QuerryForPointOfInterestCharacterIsAt } from "./worldMap";

const admin = require('firebase-admin');
// // [END import]



export class Trainer {
  constructor(
    public id: string,
    //   public position: WorldPosition,
    public professionHeTrains: string, // what PROFESSION he trains 
    public professionMinAmountNeededToTrain: number, //  minimum you must have so he can train
    public professionMaxTrainAmount: number, //  maxiumum he can train
    public trainPrice: number

  ) { }
}


exports.trainAtTrainer = functions.https.onCall(async (data, context) => {

  const callerCharacterUid = data.characterUid;
  const trainerUid = data.trainerUid;
  //const vendorItemsToBuyUids: string[] = data.vendorItemsToBuyUids;

  const characterDb = await admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);
  // const vendorDb = await admin.firestore().collection('_metadata_vendors').doc(vendorUid);

  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {

      const characterDoc = await t.get(characterDb);
      let characterData: CharacterDocument = characterDoc.data();

      const pointOfInterest = await QuerryForPointOfInterestCharacterIsAt(t, characterData);
      const trainer = pointOfInterest.getTrainerById(trainerUid);

      if (characterData.currency.silver < trainer.trainPrice)
        throw ("Not enough silver!");

      let characterProfession = characterData.getProfessionById(trainer.professionHeTrains);


      if (characterProfession != null) //uz mas tu profesi
      {
        if (characterProfession.countMax > trainer.professionMaxTrainAmount)
          throw ("This trainer cant train you anything new!");

        //splnil si minimum co musis mit na trenovani
        if (characterProfession.count >= trainer.professionMinAmountNeededToTrain) {

          characterData.subCurrency(CURRENCY_ID.SILVER, trainer.trainPrice);
          characterProfession.countMax = trainer.professionMaxTrainAmount;

        }

      }
      else {
        //trener trenuje novycky a davaji jim tim profesi
        if (trainer.professionMinAmountNeededToTrain == 0) {
          characterData.subCurrency(CURRENCY_ID.SILVER, trainer.trainPrice);

          characterData.addProfession(trainer.professionHeTrains, trainer.professionMaxTrainAmount);
        }
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