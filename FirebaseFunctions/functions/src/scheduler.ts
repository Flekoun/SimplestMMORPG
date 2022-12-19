
// [START import]
import * as functions from "firebase-functions";
import { SpawnGatherables } from "./gatherables";

//const admin = require('firebase-admin');
// // [END import]

exports.scheduledFunction = functions.pubsub.schedule('every 30 minutes').onRun((context) => {

  return SpawnGatherables();
  //  console.log('This will be run every 30 minutes!');
 // return CreateWorldBossEncounter(new WorldPosition(ZONE.DUNOTAR, LOC.NONE,POI.NONE));


});
  // [END allAdd]