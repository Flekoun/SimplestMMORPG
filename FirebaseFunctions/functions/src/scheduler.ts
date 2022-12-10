
// [START import]
import * as functions from "firebase-functions";
import { WorldPosition } from ".";
import { CreateWorldBossEncounter } from "./encounter";
import { LOC, POI, ZONE } from "./worldMap";

//const admin = require('firebase-admin');
// // [END import]

exports.scheduledFunction = functions.pubsub.schedule('every 30 minutes').onRun((context) => {
  //  console.log('This will be run every 30 minutes!');
  return CreateWorldBossEncounter(new WorldPosition(ZONE.DUNOTAR, LOC.NONE,POI.NONE));
});
  // [END allAdd]