
// [START import]
import * as functions from "firebase-functions";
import { CharacterDocument, getCurrentDateTimeInMillis, GlobalMetadata, TIME_INCREMENT_PER_SCHEDULED, TIME_MAX } from ".";
import { awardPoILeaderboardRewards, awardSeasonalLeaderboardRewards } from "./leaderboards";
import axios from 'axios';
import { InternalDefinition } from "./adminTools";



const admin = require('firebase-admin');
// // [END import]


export async function EconomyChecker() {

  // const GOLD_IN_ECONOMY_MAX = 1000;  // maximalni mnostvi zlata v ekonomice

  const charactersWithGoldDb = await admin.firestore().collection('characters').where("currency.gold", ">", 0);  //mozna pridat jeste podminku na lastLoginDate

  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {

      let totalGold = 0;
      console.log("totalGold: " + totalGold);
      //ziskam stavajici nesebrane gatherables a X% z nich promazu
      //   let lingeringGatherablesCount = 0;
      await t.get(charactersWithGoldDb).then(querry => {
        querry.docs.forEach(async doc => {
          totalGold += doc.data().currency.gold;
        });
      });
      console.log("totalGold :" + totalGold);


      return "Total gold in economy : " + totalGold;
    });


    console.log('Transaction success', result);
    return result;
  } catch (e) {
    console.log('Transaction failure:', e);
    throw new functions.https.HttpsError("aborted", "Error : " + e);
  }

}

//exports.scheduledFunction = functions.pubsub.schedule('every 24 hours').onRun((context) => {
exports.scheduledFunction = functions.pubsub.schedule('0 0 * * *').onRun((context) => {
  const globalDataDb = admin.firestore().collection('_metadata_coreDefinitions').doc("Global");


  try {
    const result = admin.firestore().runTransaction(async (t: any) => {

      const globalDataDoc = await t.get(globalDataDb);
      let globalData: GlobalMetadata = globalDataDoc.data();




      //sezona skoncila!
      if (globalData.gameDay >= globalData.seasonDurationDays && globalData.isSeasonInProgress == true) {

        globalData.nextSeasonStartTimestamp = getCurrentDateTimeInMillis(globalData.nextSeasonStartDelayInHours).toString();
        globalData.isSeasonInProgress = false;

        //dame rewardy z leaderboardu
        await awardSeasonalLeaderboardRewards("CHARACTER_LEVEL", globalData.seasonNumber);
        await awardSeasonalLeaderboardRewards("DAMAGE_DONE", globalData.seasonNumber);
        await awardSeasonalLeaderboardRewards("HEALING_DONE", globalData.seasonNumber);
        await awardSeasonalLeaderboardRewards("ITEMS_CRAFTED", globalData.seasonNumber);
        await awardSeasonalLeaderboardRewards("MONSTER_KILLS", globalData.seasonNumber);
        await awardSeasonalLeaderboardRewards("DUNGEON_ENDLESS_1", globalData.seasonNumber);
        await awardSeasonalLeaderboardRewards("DUNGEON_ENDGAME_1", globalData.seasonNumber);

      }

      //nova sezona zacala!
      else if (getCurrentDateTimeInMillis(1) > Number.parseInt(globalData.nextSeasonStartTimestamp) && globalData.isSeasonInProgress == false) {
        globalData.gameDay = 1;
        globalData.seasonNumber++;
        globalData.nextSeasonStartTimestamp = "-1";
        globalData.isSeasonInProgress = true;

        //reset leaderboardu

      }
      //sezona je v prubehu, pokracujem normalen zvednutim dne
      else {
        globalData.gameDay++;
        globalData.nextGameDayTimestamp = getCurrentDateTimeInMillis(24).toString();

        //dam POI leaderboard rewardy prvnimu hraci
        const internalDefinitionsDb = admin.firestore().collection('_internal_definitions').doc("MAP_GENERATOR");
        const internalDefinitionsDoc = await t.get(internalDefinitionsDb);
        let internalDefinitionsData: InternalDefinition = internalDefinitionsDoc.data();
        internalDefinitionsData.MONSTER_SOLO.forEach(async PoI => {
          await awardPoILeaderboardRewards(PoI.id, globalData.seasonNumber, globalData.nextGameDayTimestamp);
        });

      }

      await t.set(globalDataDb, JSON.parse(JSON.stringify(globalData)), { merge: true });

      return "OK";
    });


    console.log('Transaction success', result);
    return result;
  } catch (e) {
    console.log('Transaction failure:', e);
    throw new functions.https.HttpsError("aborted", "Error : " + e);
  }

});


//exports.scheduledFunction24 = functions.pubsub.schedule('every 24 hours').onRun(async (context) => {
// exports.scheduledFunction24 = functions.pubsub.schedule('0 0 * * *').onRun(async (context) => {




//   const charactersRef = admin.firestore().collection("characters").where('currency.scavengePoints', '<', SCAVENGE_POINTS_MAX);
//   const batch = admin.firestore().batch();
//   const dataSnapshot = await charactersRef.get();

//   dataSnapshot.forEach(doc => {
//     // const characterDoc = await t.get(characterDb);
//     let character: CharacterDocument = doc.data();
//     if (character.currency.scavengePoints <= character.currency.scavengePointsMax - SCAVENGE_POINTS_INCREMENT_PER_SCHEDULED)
//       character.currency.scavengePoints += SCAVENGE_POINTS_INCREMENT_PER_SCHEDULED;
//     else
//       character.currency.scavengePoints += character.currency.scavengePointsMax;

//     batch.update(doc.ref, JSON.parse(JSON.stringify(character)), { merge: true }); // Update the document in batch
//   });
//   return batch.commit() // Commit the batch
//     .then(() => {
//       console.log("Batch update completed.");
//       console.log("Increase Scavenge Points - Number of effected characters: ", dataSnapshot.size);
//     }
//     )
//     .catch(err => console.error("Error: ", err));
// });


//TODO: pak bude stacit kazdy den?!

exports.leaderboardsReset = functions.pubsub.schedule('0 * * * *').onRun(async (context) => {
  //exports.leaderboardsReset = functions.pubsub.schedule('every hour').onRun(async (context) => {



  try {
    // Fetching Bitcoin to USD exchange rate from CoinGecko API
    const response = await axios.get('https://api.coingecko.com/api/v3/simple/price?ids=bitcoin&vs_currencies=usd');
    const exchangeRate = response.data.bitcoin.usd; // Parsing the specific exchange rate
    const satoshiumRate = parseFloat(((1 / (100 * exchangeRate)) * 100000000).toFixed(1));

    // Save the exchange rate to Firestore
    await admin.firestore().collection('/_metadata_coreDefinitions').doc('Global').set({
      BTC_USD_ExchangeRate: exchangeRate,
      SATOSHIUM_SATS_ExchangeRate: satoshiumRate,
    }, { merge: true });

    console.log('Bitcoin to USD exchange rate updated:', exchangeRate);
  } catch (error) {
    console.error('Error fetching exchange rate:', error);
  }


  const batch = admin.firestore().batch();

  const globalDataRef = admin.firestore().collection("_metadata_coreDefinitions").doc("Global");
  const globalSnapshot = await globalDataRef.get();
  let globalData = globalSnapshot.data();
  //globalData.schedulerTimestamp1hour = getCurrentDateTimeInMillis(0);
  batch.update(globalSnapshot.ref, JSON.parse(JSON.stringify(globalData)), { merge: true });

  const charactersRef = admin.firestore().collection("characters").where('currency.time', '<', TIME_MAX);

  const dataSnapshot = await charactersRef.get();

  dataSnapshot.forEach(doc => {
    // const characterDoc = await t.get(characterDb);
    let character: CharacterDocument = doc.data();
    if (character.currency.time <= character.currency.timeMax - TIME_INCREMENT_PER_SCHEDULED)
      character.currency.time += TIME_INCREMENT_PER_SCHEDULED;
    else
      character.currency.time += character.currency.timeMax;

    // if (character.currency.travelPoints <= character.currency.travelPointsMax - TRAVEL_POINTS_INCREMENT_PER_SCHEDULED)
    //   character.currency.travelPoints += TRAVEL_POINTS_INCREMENT_PER_SCHEDULED;
    // else
    //   character.currency.travelPoints = character.currency.travelPointsMax;

    // const characterRef = admin.firestore().collection('characters').doc(doc.id);
    batch.update(doc.ref, JSON.parse(JSON.stringify(character)), { merge: true }); // Update the document in batch
  });
  return batch.commit() // Commit the batch
    .then(() => {
      console.log("Batch update completed.");
      console.log("Increase Time - Number of effected characters: ", dataSnapshot.size);
    }
    )
    .catch(err => console.error("Error: ", err));
});



// const charactersRef = admin.firestore().collection("characters");
// const batch = admin.firestore().batch();

// charactersRef.get()
//   .then(snapshot => {
//     snapshot.docs.forEach(doc => {
//       // const characterDoc = await t.get(characterDb);
//       let character: CharacterDocument = doc.data();
//       //  const character = doc.data();
//       if (character.currency.time < TIME_MAX) { // Only update if 'time' property is less than 200
//         character.currency.time += TIME_INCREMENT_PER_SCHEDULED; // Increase the 'time' property by 5
//         const characterRef = admin.firestore().collection('characters').doc(doc.id);
//         batch.update(characterRef, JSON.parse(JSON.stringify(character)), { merge: true }); // Update the document in batch
//       }
//     });
//     return batch.commit() // Commit the batch
//       .then(() => console.log("Batch update completed."))
//       .catch(err => console.error("Error: ", err));
//   })
//   .catch(err => console.error('Error getting documents', err));

//  checkForLeaderboardReset(LEADERBOARD.CHARACTER_LEVEL);
//  checkForLeaderboardReset(LEADERBOARD.ITEMS_CRAFTED);
// checkForLeaderboardReset(LEADERBOARD.LOCATION_DEEP_RAVINE);
// checkForLeaderboardReset(LEADERBOARD.LOCATION_MALAKA_DUNGEON);
// checkForLeaderboardReset(LEADERBOARD.LOCATION_VALLEY_OF_TRIALS);
// checkForLeaderboardReset(LEADERBOARD.LOCATION_VILLAGE_OF_MALAKA);



//  return checkForLeaderboardReset(LEADERBOARD.MONSTER_KILLS);
//  console.log('This will be run every 30 minutes!');
// return CreateWorldBossEncounter(new WorldPosition(ZONE.DUNOTAR, LOC.NONE,POI.NONE));




// [END allAdd]