
// [START import]
import * as functions from "firebase-functions";
import { CharacterDocument, characterDocumentConverter, ContentContainer } from ".";

const admin = require('firebase-admin');
// // [END import]



export class InboxItem {
  constructor(
    public uid: string,
    public characterRecipientUid: string,
    public content: ContentContainer,
    public messageTitle: string,
    public messageBody: string,

    public expireDate: string

  ) { }
}

exports.claimInboxItem = functions.https.onCall(async (data, context) => {

  const callerCharacterUid = data.characterUid;
  const inboxItemUid = data.inboxItemUid;

  const characterDb = await admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);
  const inboxDb = await admin.firestore().collection('inbox').doc(inboxItemUid);

  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {

      const characterDoc = await t.get(characterDb);
      let characterData: CharacterDocument = characterDoc.data();

      const inboxItemDoc = await t.get(inboxDb);
      let inboxItemData: InboxItem = inboxItemDoc.data();

      inboxItemData.content = new ContentContainer(inboxItemData.content.contentType,inboxItemData.content.contentItem,inboxItemData.content.contentEquip,inboxItemData.content.contentCurrency,inboxItemData.content.contentFood); //kvuli tomu ze nemam withConverter...

      characterData.addContentToInventory(inboxItemData.content, true,false);
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


  // [END allAdd]