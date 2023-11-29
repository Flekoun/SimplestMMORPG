
// [START import]
import * as functions from "firebase-functions";
import { CharacterDocument, characterDocumentConverter, ContentContainer, getCurrentDateTime, PlayerData, PlayerDataConverter } from ".";

const admin = require('firebase-admin');
// // [END import]



export class InboxItem {
  constructor(
    public uid: string,
    public recipientUid: string,
    public content: ContentContainer,
    public messageTitle: string,
    public messageBody: string,

    public expireDate: string

  ) { }
}

export async function sendContentToInbox(_transaction: any, _itemToSend: ContentContainer, _recieverCharacterUid: string, _messageTitle: string, _messageBody: string) {

  const inboxDb = admin.firestore().collection('inbox').doc();
  const inboxEntry = new InboxItem(inboxDb.id, _recieverCharacterUid, _itemToSend, _messageTitle, _messageBody, getCurrentDateTime(480));

  _transaction.set(inboxDb, JSON.parse(JSON.stringify(inboxEntry)));
}


exports.claimInboxItem = functions.https.onCall(async (data, context) => {

  const callerCharacterUid = data.characterUid;
  const inboxItemUid = data.inboxItemUid;

  const characterDb = admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);
  const inboxDb = admin.firestore().collection('inbox').doc(inboxItemUid);

  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {

      const characterDoc = await t.get(characterDb);
      let characterData: CharacterDocument = characterDoc.data();

      const inboxItemDoc = await t.get(inboxDb);
      let inboxItemData: InboxItem = inboxItemDoc.data();


      inboxItemData.content = new ContentContainer(inboxItemData.content.content, inboxItemData.content.contentEquip); //kvuli tomu ze nemam withConverter...

      characterData.addContentToInventory(inboxItemData.content, true, false);
      t.delete(inboxDb);
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


exports.claimPlayerInboxItem = functions.https.onCall(async (data, context) => {

  //const callerPlayerUid = data.characterUid;
  const callerPlayerUid = context.auth?.uid;
  const inboxItemUid = data.inboxItemUid;

  const playerDb = admin.firestore().collection('players').doc(callerPlayerUid).withConverter(PlayerDataConverter);
  const inboxDb = admin.firestore().collection('inboxPlayer').doc(inboxItemUid);

  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {

      const playerDoc = await t.get(playerDb);
      let playerData: PlayerData = playerDoc.data();

      const inboxItemDoc = await t.get(inboxDb);
      let inboxItemData: InboxItem = inboxItemDoc.data();


      inboxItemData.content = new ContentContainer(inboxItemData.content.content, inboxItemData.content.contentEquip); //kvuli tomu ze nemam withConverter...

      playerData.addContentToInventory(inboxItemData.content, true, false);
      t.delete(inboxDb);
      t.set(playerDb, JSON.parse(JSON.stringify(playerData)), { merge: true });

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