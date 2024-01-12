
// [START import]
import * as functions from "firebase-functions";

import { CharacterDocument, characterDocumentConverter, getCurrentDateTime, getCurrentDateTimeInMillis, ContentContainer, generateContentContainer, CONTENT_TYPE, validateCallerBulletProof } from ".";
import { Equip, generateContent, ITEMS } from "./equip";
import { InboxItem } from "./inbox";


const admin = require('firebase-admin');
//const { Firestore } = require("@google-cloud/firestore");


// function addHours(numOfHours, date = new Date()) {
//   date.setTime(date.getTime() + numOfHours * 60 * 60 * 1000);
//   return date;
// }
//const { getFirestore, Timestamp, FieldValue } = require('firebase-admin/firestore');
//const { FieldValue } = require('firebase-admin/firestore');
// // [END import]

export class AuctionOffer {
  constructor(
    public uid: string,
    public sellerDisplayName: string,
    public sellerUid: string,

    public content: ContentContainer,

    public lastBidPrice: number,
    public nextBidPrice: number,
    public hasBuyoutPrice: boolean,
    public buyoutPrice: number,

    public highestBidderUid: string,
    public highestBidderDiplayName: string,

    public expireDate: string

  ) { }
}

exports.putContentOnAuctionHouse = functions.https.onCall(async (data, context) => {

  // var requestData = {
  const sellerCharacterUid = data.characterUid;
  // const contentType = data.contentType;
  const contentToSellUid = data.contentToSell;
  // const contentSilverAmount = data.contentSilverAmount;
  const buyoutPrice = data.buyoutPrice;
  const bidPrice = data.bidPrice;
  const amountToSell = data.amount;

  //const encounterResultDb = admin.firestore().collection('encountersResults').doc(encounterRewardUid);//.withConverter(encounterDocumentConverter);
  const sellerCharacterDb = admin.firestore().collection('characters').doc(sellerCharacterUid).withConverter(characterDocumentConverter);
  const auctionHouseDb = admin.firestore().collection('auctionHouse').doc();

  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {

      const sellerCharacterDoc = await t.get(sellerCharacterDb);
      let sellerCharacterData: CharacterDocument = sellerCharacterDoc.data();
      validateCallerBulletProof(sellerCharacterData, context);

      const expireDate = getCurrentDateTime(24);

      const itemToSell = sellerCharacterData.getInventoryContent(contentToSellUid);

      if (itemToSell.getItem().amount < amountToSell)
        throw ("Not enough items. You have only " + itemToSell.getItem().amount + " but want to sell " + amountToSell);

      if (itemToSell.getItem().contentType == CONTENT_TYPE.EQUIP) {
        if ((itemToSell.getItem() as Equip).neverEquiped == false)
          throw ("Cant sell used equip");

      }



      //Nastavim itemu tolik kolik jich chci prodat a vytvorim zaznam na aukci...ale potom...
      let oldamount = itemToSell.getItem().amount;
      itemToSell.getItem().amount = amountToSell;

      // console.log("item to sell amount : " + itemToSell.contentFood?.amount);
      const newAuction = new AuctionOffer(auctionHouseDb.id, sellerCharacterData.characterName, sellerCharacterData.uid, new ContentContainer(itemToSell.content, itemToSell.contentEquip), bidPrice, bidPrice, buyoutPrice > 0, buyoutPrice, "", "", expireDate);
      // console.log("newAuction amount : " + newAuction.content.contentFood?.amount);

      t.set(auctionHouseDb, JSON.parse(JSON.stringify(newAuction)));

      //...potom chci amount zase vratit jak byl.... a pak potom rucne odebrat prodane mnostvi...
      itemToSell.getItem().amount = oldamount;
      //BACHA TADY SI TO DRZI REFERENCI, KDYZ ZMENIS NECO TADY TAK PAK SE TO ZMENI I VSUDE JINDE CHAPES? , proto odebiram content az tady po tom co to vlozim na aukci
      //remove the item from seller
      sellerCharacterData.removeContentFromInventory(contentToSellUid, amountToSell);//itemToSell.getItem().amount);

      t.set(sellerCharacterDb, JSON.parse(JSON.stringify(sellerCharacterData)), { merge: true });

    });

    console.log('Transaction success', result);
    return result;
  } catch (e) {
    console.log('Transaction failure:', e);
    throw new functions.https.HttpsError("aborted", "Error : " + e);
  }

});



exports.bidContentOnAuctionHouse = functions.https.onCall(async (data, context) => {

  const callerCharacterUid = data.characterUid;
  const offerUid = data.offerUid;
  const callerBidAmount = data.bidAmount;

  console.log("callerCharacterUid: " + callerCharacterUid);
  console.log("offerUid: " + offerUid);
  console.log("callerBidAmount: " + callerBidAmount);

  const callerCharacterDb = admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);
  const offerDb = admin.firestore().collection('auctionHouse').doc(offerUid);


  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {

      const callerCharacterDoc = await t.get(callerCharacterDb);
      let callerCharacterData: CharacterDocument = callerCharacterDoc.data();
      validateCallerBulletProof(callerCharacterData, context);

      const offerDoc = await t.get(offerDb);
      let offerData: AuctionOffer = offerDoc.data();

      if (offerData.highestBidderUid == callerCharacterUid)
        throw ("Cant bid. You already have the highest bid!");

      if (offerData.sellerUid == callerCharacterUid)
        throw ("Cant bid on your own auctions!");

      const hasHighestBidder = offerData.highestBidderUid != "";

      console.log(callerCharacterUid + " bidded " + callerBidAmount + " on auction offer " + offerUid);

      //zjistil jestli mu vezmi jen autopbid nebo si prisadil vic
      let currentBidPrice = offerData.nextBidPrice;
      if (callerBidAmount > currentBidPrice) {
        console.log(" Buyer bid is higher than that so seting it to this new value : " + callerBidAmount);
        currentBidPrice = callerBidAmount;
      }

      console.log("taking gold from the new bidder, this amount :  " + currentBidPrice);
      //seberu goldy biderovi
      callerCharacterData.subGold(currentBidPrice);


      //vypocitame novy bido vyzadanou hodnotu nebo o autobid amount a to je 5%..   

      let bidIncrease = Math.round(currentBidPrice / 20);
      if (bidIncrease < 1) bidIncrease = 1;//..ale ne min nez 1 gold

      let nextBidAmount = currentBidPrice + bidIncrease;

      offerData.nextBidPrice = nextBidAmount;

      //   TODO: EXTEND auction duration if bid was made too close to expiration date 
      // if(offerData.expireDate <= SERVER TIME ) 


      //pokid mame nejakeho highest biddera, vratime mu goldy do inboxu
      if (hasHighestBidder) {


        offerData.content = new ContentContainer(offerData.content.content, offerData.content.contentEquip);//kvuli tomu ze nemam withConverter...
        offerData.content = generateContentContainer(offerData.content.getItem());
        //new ContentContainer(offerData.content.contentType, offerData.content.contentItem, offerData.content.contentEquip, offerData.content.contentCurrency, offerData.content.contentFood); //kvuli tomu ze nemam withConverter...

        const inboxDb = admin.firestore().collection('inbox').doc();
        console.log("sending inbox refund to old bidder, with this amount of gold :  " + offerData.lastBidPrice);
        // const newContentCurrency = new ContentCurrency(firestoreAutoId(), CURRENCY_ID.GOLD, 100, 1000000000, offerData.lastBidPrice, RARITY.COMMON);
        // const newContent = new ContentContainer(CONTENT_TYPE.CURRENCY, undefined, undefined, newContentCurrency, undefined);
        const newContent = generateContentContainer(generateContent(ITEMS.GOLD, offerData.lastBidPrice))
        const newInbox = new InboxItem(inboxDb.id, offerData.highestBidderUid, newContent, undefined, "Auction House : You were outbid!", "Here is your gold refund for offer <color=\"orange\">" + offerData.content.getItem().itemId + "</color> you were oudbid on", getCurrentDateTime(480));

        t.set(inboxDb, JSON.parse(JSON.stringify(newInbox)));

      }

      //ulozime si novy bid bid
      offerData.lastBidPrice = currentBidPrice;

      offerData.highestBidderDiplayName = callerCharacterData.characterName;
      offerData.highestBidderUid = callerCharacterData.uid;



      t.set(offerDb, JSON.parse(JSON.stringify(offerData)), { merge: true });
      t.set(callerCharacterDb, JSON.parse(JSON.stringify(callerCharacterData)), { merge: true });

    });

    console.log('Transaction success', result);
    return result;
  } catch (e) {
    console.log('Transaction failure:', e);
    throw new functions.https.HttpsError("aborted", "Error : " + e);
  }

});




exports.buyoutContentOnAuctionHouse = functions.https.onCall(async (data, context) => {

  const callerCharacterUid = data.characterUid;
  const offerUid = data.offerUid;

  console.log("callerCharacterUid: " + callerCharacterUid);
  console.log("offerUid: " + offerUid);

  const callerCharacterDb = admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);
  const offerDb = admin.firestore().collection('auctionHouse').doc(offerUid);


  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {

      const callerCharacterDoc = await t.get(callerCharacterDb);
      let callerCharacterData: CharacterDocument = callerCharacterDoc.data();

      const offerDoc = await t.get(offerDb);
      let offerData: AuctionOffer = offerDoc.data();

      offerData.content = new ContentContainer(offerData.content.content, offerData.content.contentEquip);//kvuli tomu ze nemam withConverter...
      // offerData.content = new ContentContainer(offerData.content.contentType, offerData.content.contentItem, offerData.content.contentEquip, offerData.content.contentCurrency, offerData.content.contentFood); //kvuli tomu ze nemam withConverter...
      if (offerData.sellerUid == callerCharacterUid)
        throw ("Cant buyout your own auction!");

      const hasHighestBidder = offerData.highestBidderUid != "";

      //seberu goldy kupujicimu
      callerCharacterData.subGold(offerData.buyoutPrice);

      //pridam mu item do inventare
      //  callerCharacterData.addContentToInventory(offerData.content, true);


      //posleme item kupujicimu do inboxu
      let newInboxBuyer: InboxItem | null = null;
      const inboxDbBuyer = admin.firestore().collection('inbox').doc();
      console.log("sending item to seller inbox :  ");
      // const newContentCurrency = new ContentCurrency(firestoreAutoId(), "CURRENCY_GOLD", "Gold", 100, "GOLD", 1000000000, offerData.buyoutPrice, RARITY.COMMON, CURRENCY_TYPE.GOLD);
      // const newContent = new ContentContainer(CONTENT_TYPE.CURRENCY, undefined, undefined, newContentCurrency, undefined);
      newInboxBuyer = new InboxItem(inboxDbBuyer.id, callerCharacterData.uid, offerData.content, undefined, "Auction House : Your won and Auction!", "Here is an offer you won on Auction House  <color=\"orange\">" + offerData.content.getItem().itemId + "</color>", getCurrentDateTime(480));



      //posleme goldy prodavajicimu do inboxu
      let newInboxSeller: InboxItem | null = null;
      const inboxDbSeller = admin.firestore().collection('inbox').doc();
      // offerData.content = new ContentContainer(offerData.content.contentType, offerData.content.contentItem, offerData.content.contentEquip, offerData.content.contentCurrency, offerData.content.contentFood); //kvuli tomu ze nemam withConverter...
      console.log("sending gold to seller inbox :  " + offerData.buyoutPrice);
      const newContent = generateContentContainer(generateContent(ITEMS.GOLD, offerData.buyoutPrice)); //new Content(firestoreAutoId(), CURRENCY_ID.GOLD, 100, 1000000000, offerData.buyoutPrice, RARITY.COMMON );
      //const newContent = new ContentContainer(newContentCurrency, undefined);
      newInboxSeller = new InboxItem(inboxDbSeller.id, offerData.sellerUid, newContent, undefined, "Auction House : Your Auction offer was sold!", "Here is your gold for offer <color=\"orange\">" + offerData.content.getItem().itemId + "</color>", getCurrentDateTime(480));


      //pokid mame nejakeho highest biddera, vratime mu goldy do inboxu
      let newInboxHighestBidder: InboxItem | null = null;
      const inboxDbHasHighestBidder = admin.firestore().collection('inbox').doc();
      if (hasHighestBidder) {
        // offerData.content = new ContentContainer(offerData.content.contentType, offerData.content.contentItem, offerData.content.contentEquip, offerData.content.contentCurrency, offerData.content.contentFood); //kvuli tomu ze nemam withConverter...
        console.log("sending inbox refund to old bidder, with this amount of gold :  " + offerData.lastBidPrice);

        const newContent = generateContentContainer(generateContent(ITEMS.GOLD, offerData.lastBidPrice))
        //  const newContentCurrency = new ContentCurrency(firestoreAutoId(), CURRENCY_ID.GOLD, 100, 1000000000, offerData.lastBidPrice, RARITY.COMMON);
        // const newContent = new ContentContainer(CONTENT_TYPE.CURRENCY, undefined, undefined, newContentCurrency, undefined);
        newInboxHighestBidder = new InboxItem(inboxDbHasHighestBidder.id, offerData.highestBidderUid, newContent, undefined, "Auction House : You were outbought!", "Here is your gold refund for offer <color=\"orange\">" + offerData.content.getItem().itemId + "</color> you were outbought on", getCurrentDateTime(480));
      }


      // Get a new write batch
      //var batch = admin.batch();

      if (hasHighestBidder)
        t.set(inboxDbHasHighestBidder, JSON.parse(JSON.stringify(newInboxHighestBidder)));

      await t.set(inboxDbBuyer, JSON.parse(JSON.stringify(newInboxBuyer)));
      await t.set(inboxDbSeller, JSON.parse(JSON.stringify(newInboxSeller)));


      //  await t.set(sellerCharacterDb, JSON.parse(JSON.stringify(sellerCharacterData)), { merge: true });
      t.set(callerCharacterDb, JSON.parse(JSON.stringify(callerCharacterData)), { merge: true });
      t.delete(offerDb);
    });

    console.log('Transaction success', result);
    return result;
  } catch (e) {
    console.log('Transaction failure:', e);
    throw new functions.https.HttpsError("aborted", "Error : " + e);
  }

});

exports.collectMyUnsoldContentOnAuctionHouse = functions.https.onCall(async (data, context) => {

  const callerCharacterUid = data.characterUid;
  const offerUid = data.offerUid;

  console.log("callerCharacterUid: " + callerCharacterUid);
  console.log("offerUid: " + offerUid);

  const callerCharacterDb = admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);
  const offerDb = admin.firestore().collection('auctionHouse').doc(offerUid);


  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {

      const callerCharacterDoc = await t.get(callerCharacterDb);
      let callerCharacterData: CharacterDocument = callerCharacterDoc.data();

      const offerDoc = await t.get(offerDb);
      let offerData: AuctionOffer = offerDoc.data();

      if (offerData.sellerUid != callerCharacterUid)
        throw ("Its not your item! You cannot collect it!");

      if (Number.parseInt(offerData.expireDate) > getCurrentDateTimeInMillis(0))
        throw ("Auction is still in progress! You cannot collect it!");

      const hasHighestBidder = offerData.highestBidderUid != "";

      if (hasHighestBidder)
        throw ("Offer was sold to someone! You cannot collect it!");


      callerCharacterData.addContentToInventory(offerData.content, true, false);



      t.set(callerCharacterDb, JSON.parse(JSON.stringify(callerCharacterData)), { merge: true });
      t.delete(offerDb);
    });

    console.log('Transaction success', result);
    return result;
  } catch (e) {
    console.log('Transaction failure:', e);
    throw new functions.https.HttpsError("aborted", "Error : " + e);
  }

});



exports.collectGoldForMySoldContentOnAuctionHouse = functions.https.onCall(async (data, context) => {

  const callerCharacterUid = data.characterUid;
  const offerUid = data.offerUid;

  console.log("callerCharacterUid: " + callerCharacterUid);
  console.log("offerUid: " + offerUid);

  const callerCharacterDb = admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);
  const offerDb = admin.firestore().collection('auctionHouse').doc(offerUid);


  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {

      const callerCharacterDoc = await t.get(callerCharacterDb);
      let callerCharacterData: CharacterDocument = callerCharacterDoc.data();

      const offerDoc = await t.get(offerDb);
      let offerData: AuctionOffer = offerDoc.data();

      if (offerData.sellerUid != callerCharacterUid)
        throw ("Its not your item! You cannot collect reward!");

      if (Number.parseInt(offerData.expireDate) > getCurrentDateTimeInMillis(0))
        throw ("Auction is still in progress! You cannot collect reward for it!");

      const hasHighestBidder = offerData.highestBidderDiplayName != ""; //MUSIM TU pouzit display name, protoze jinak bidder pouziva svoje UID jako flag, kdyz ho natavi na "" tak si vybral odmenu a ja bych tu pak myslel ze nikdo nebidoval

      if (!hasHighestBidder)
        throw ("Offer was not sold to anyone! You cannot collect reward for it!");

      callerCharacterData.addGold(offerData.lastBidPrice);

      offerData.sellerUid = "";

      if (offerData.highestBidderUid == "") // bidder uz sebral claimnul item, muzu smazat zaznam cely
        t.delete(offerDb);
      else
        t.set(offerDb, JSON.parse(JSON.stringify(offerData)), { merge: true });


      t.set(callerCharacterDb, JSON.parse(JSON.stringify(callerCharacterData)), { merge: true });

    });

    console.log('Transaction success', result);
    return result;
  } catch (e) {
    console.log('Transaction failure:', e);
    throw new functions.https.HttpsError("aborted", "Error : " + e);
  }

});


exports.collectContentForMyWonAuctionOnAuctionHouse = functions.https.onCall(async (data, context) => {

  const callerCharacterUid = data.characterUid;
  const offerUid = data.offerUid;

  console.log("callerCharacterUid: " + callerCharacterUid);
  console.log("offerUid: " + offerUid);

  const callerCharacterDb = admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);
  const offerDb = admin.firestore().collection('auctionHouse').doc(offerUid);


  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {

      const callerCharacterDoc = await t.get(callerCharacterDb);
      let callerCharacterData: CharacterDocument = callerCharacterDoc.data();

      const offerDoc = await t.get(offerDb);
      let offerData: AuctionOffer = offerDoc.data();

      if (offerData.highestBidderUid != callerCharacterUid)
        throw ("You didnt won the auction! You cannot collect reward!");

      if (Number.parseInt(offerData.expireDate) > getCurrentDateTimeInMillis(0))
        throw ("Auction is still in progress! You cannot collect reward for it!");


      callerCharacterData.addContentToInventory(offerData.content, true, false);


      offerData.highestBidderUid = "";

      if (offerData.sellerUid == "") // seller uz sebral claimnul item, muzu smazat zaznam cely
        t.delete(offerDb);
      else
        t.set(offerDb, JSON.parse(JSON.stringify(offerData)), { merge: true });

      t.set(callerCharacterDb, JSON.parse(JSON.stringify(callerCharacterData)), { merge: true });

    });

    console.log('Transaction success', result);
    return result;
  } catch (e) {
    console.log('Transaction failure:', e);
    throw new functions.https.HttpsError("aborted", "Error : " + e);
  }

});


// [END allAdd]