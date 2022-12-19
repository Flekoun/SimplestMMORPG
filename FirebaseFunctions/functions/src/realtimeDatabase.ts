
// [START import]
import * as functions from "firebase-functions";
import { Party } from "./party";
const admin = require('firebase-admin');
// // [END import]




export class OnlineStatus {
  constructor(
    public characterUid: string,
    public isOnline: boolean

  ) { }
}

// Create a new function which is triggered on changes to /status/{uid}
// Note: This is a Realtime Database trigger, *not* Firestore.
//exports.onUserStatusChanged = functions.database.ref('/status/{uid}').onDelete(
exports.onUserPresenceStatusDeleted = functions.database.ref('/presenceStatus/{uid}').onDelete(
  async (doc, context) => {


    // Get the data written to Realtime Database
    const eventStatus = doc.val();

    const statusSnapshot = await doc.ref.once('value');
    const status = statusSnapshot.val();
    functions.logger.log(status, eventStatus);

    console.log("status pro uid :" + eventStatus.characterUid + " je : " + eventStatus.state)

    try {
      const result = await admin.firestore().runTransaction(async (t: any) => {



        let presenceStatusData: OnlineStatus = new OnlineStatus(eventStatus.characterUid, false);


        //najdeme jeste pokud sem v nejake parte tak nastavime onlineStatus i tam aby to ostatni videli hned
        const myPartyDb = admin.firestore().collection('parties').where("partyMembersUidList", "array-contains", presenceStatusData.characterUid);
        //ziskam svoji partu
        let myPartyData: Party = new Party("", "", 0, [], [],null);
        let partyDocId = "";
        await t.get(myPartyDb).then(querry => {
          if (querry.size == 1) {
            querry.docs.forEach(doc => {
              console.log("Sem tu: " + doc.id);
              partyDocId = doc.id;
              myPartyData = doc.data();
              myPartyData.partyMembers.forEach(member => {
                if (member.uid == presenceStatusData.characterUid) {
                  member.isOnline = false;
                }
              });
            });
          }
          else if (querry.size > 1)
            throw "You are more than in 1 party! How could this be? DATABASE ERROR!";
        });
        console.log("PartyDoc : " + partyDocId);
        if (partyDocId != "") {
          let partyRef = admin.firestore().collection('parties').doc(partyDocId);
          t.set(partyRef, JSON.parse(JSON.stringify(myPartyData)), { merge: true });
        }

        // ... and write it to Firestore.
        let presenceStatusRef = admin.firestore().collection('presenceStatus').doc(presenceStatusData.characterUid);
        t.set(presenceStatusRef, JSON.parse(JSON.stringify(presenceStatusData)), { merge: true });
      });


      console.log('Transaction success', result);
      return result;
    } catch (e) {
      console.log('Transaction failure:', e);
      throw new functions.https.HttpsError("aborted", "Error : " + e);
    }
  });


// Create a new function which is triggered on changes to /status/{uid}
// Note: This is a Realtime Database trigger, *not* Firestore.
exports.onUserPresenceStatusCreated = functions.database.ref('/presenceStatus/{uid}').onCreate(
  async (doc, context) => {
    // Get the data written to Realtime Database
    const eventStatus = doc.val();

    const statusSnapshot = await doc.ref.once('value');
    const status = statusSnapshot.val();
    functions.logger.log(status, eventStatus);

    console.log("status pro uid :" + eventStatus.characterUid + " je : " + eventStatus.state)



    try {
      const result = await admin.firestore().runTransaction(async (t: any) => {


        let presenceStatusData: OnlineStatus = new OnlineStatus(eventStatus.characterUid, true);

        //najdeme jeste pokud sem v nejake parte tak nastavime onlineStatus i tam aby to ostatni videli hned
        const myPartyDb = admin.firestore().collection('parties').where("partyMembersUidList", "array-contains", presenceStatusData.characterUid);
        //ziskam svoji partu
        let myPartyData: Party = new Party("", "", 0, [], [],null);
        let partyDocId = "";
        await t.get(myPartyDb).then(querry => {
          if (querry.size == 1) {
            querry.docs.forEach(doc => {
              console.log("Sem tu: " + doc.id);
              partyDocId = doc.id;
              myPartyData = doc.data();
              myPartyData.partyMembers.forEach(member => {
                if (member.uid == presenceStatusData.characterUid) {
                  member.isOnline = true;
                }
              });
            });
          }
          else if (querry.size > 1)
            throw "You are more than in 1 party! How could this be? DATABASE ERROR!";
        });

        console.log("PartyDoc : " + partyDocId);
        if (partyDocId != "") {
          let partyRef = admin.firestore().collection('parties').doc(partyDocId);
          t.set(partyRef, JSON.parse(JSON.stringify(myPartyData)), { merge: true });
        }

        // ... and write it to Firestore.
        let presenceStatusRef = admin.firestore().collection('presenceStatus').doc(presenceStatusData.characterUid);
        t.set(presenceStatusRef, JSON.parse(JSON.stringify(presenceStatusData)), { merge: true });

      });


      console.log('Transaction success', result);
      return result;
    } catch (e) {
      console.log('Transaction failure:', e);
      throw new functions.https.HttpsError("aborted", "Error : " + e);
    }
  });



// // Create a new function which is triggered on changes to /status/{uid}
// // Note: This is a Realtime Database trigger, *not* Firestore.
// exports.onUserStatusChanged = functions.database.ref('/status/{uid}').onUpdate(
//   async (doc, context) => {
//     // Get the data written to Realtime Database
//     const eventStatus = doc.after.val();

//     // Then use other event data to create a reference to the
//     // corresponding Firestore document.
//   //  const userStatusFirestoreRef = admin.firestore().doc(`status/${context.params.uid}`);

//     // It is likely that the Realtime Database change that triggered
//     // this event has already been overwritten by a fast change in
//     // online / offline status, so we'll re-read the current data
//     // and compare the timestamps.
//     const statusSnapshot = await doc.after.ref.once('value');
//     const status = statusSnapshot.val();
//     functions.logger.log(status, eventStatus);

//     console.log("status pro uid :" + eventStatus.uid + " je : " + eventStatus.state)

//     // If the current timestamp for this data is newer than
//     // the data that triggered this event, we exit this function.
//     //   if (status.last_changed > eventStatus.last_changed) {
//     //     return null;
//     //   }

//     // Otherwise, we convert the last_changed field to a Date
//     //   eventStatus.last_changed = new Date(eventStatus.last_changed);

//     let statusObj: OnlineStatus = new OnlineStatus(eventStatus.uid, true);
//     // ... and write it to Firestore.
//     let ref = admin.firestore().collection('status').doc(statusObj.uid);
//     ref.set(JSON.parse(JSON.stringify(statusObj)), { merge: true });
//   });



// // Create a new function which is triggered on changes to /status/{uid}
// // Note: This is a Realtime Database trigger, *not* Firestore.
// exports.onUserStatusChanged = functions.database.ref('/status/{uid}').onUpdate(
//     async (change, context) => {
//       // Get the data written to Realtime Database
//       const eventStatus = change.after.val();

//       // Then use other event data to create a reference to the
//       // corresponding Firestore document.
//       const userStatusFirestoreRef = firestore.doc(`status/${context.params.uid}`);

//       // It is likely that the Realtime Database change that triggered
//       // this event has already been overwritten by a fast change in
//       // online / offline status, so we'll re-read the current data
//       // and compare the timestamps.
//       const statusSnapshot = await change.after.ref.once('value');
//       const status = statusSnapshot.val();
//       functions.logger.log(status, eventStatus);
//       // If the current timestamp for this data is newer than
//       // the data that triggered this event, we exit this function.
//       if (status.last_changed > eventStatus.last_changed) {
//         return null;
//       }

//       // Otherwise, we convert the last_changed field to a Date
//       eventStatus.last_changed = new Date(eventStatus.last_changed);

//       // ... and write it to Firestore.
//       return userStatusFirestoreRef.set(eventStatus);
//     });



  // [END allAdd]