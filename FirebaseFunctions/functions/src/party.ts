
// [START import]
import * as functions from "firebase-functions";
import { CharacterDocument, characterDocumentConverter, ContentContainer, QuerryForParty, QuerryIfCharacterIsInCombatAtAnyEncounter, SimpleStringDictionary, WorldPosition } from ".";
import { EncounterDocument, encounterDocumentConverter, ENCOUNTER_CONTEXT } from "./encounter";

import { PointOfInterest, POI_SPECIALS, PointOfInterestServerDataDefinitions, PointOfInterestServerDataDefinitionsConverter } from "./worldMap";

import { RandomEquip } from "./questgiver";
import { ItemIdWithAmount } from "./equip";

const admin = require('firebase-admin');
// // [END import]


// [Party]
export class Party {
  constructor(
    public uid: string,
    public partyLeaderUid: string,
    public partySizeMax: number,
    public partyMembers: PartyMember[],
    public partyMembersUidList: string[],
    public dungeonProgress: DungeonProgress | null,
    public dungeonEnterConsents: SimpleStringDictionary[] //string1 = dungeonID , string2 = characterUid

  ) { }

  getPartyMemberByUid(_uid: string): PartyMember | null {

    let result: PartyMember | null = null;
    this.partyMembers.forEach(element => {
      if (element.uid == _uid)
        result = element;
    });
    return result;
  }

  giveConsentToEnterDungeon(_dungeonId: string, _characterUid: string) {



    if (this.dungeonEnterConsents.find(entry => entry.string1 == _dungeonId && entry.string2 == _characterUid))
      throw ("You have already gave consent to enter :" + _dungeonId);

    this.dungeonEnterConsents.push(new SimpleStringDictionary(_dungeonId, _characterUid));
  }

  getNumberOfConsentsToEnterDungeon(_dungeonId: string): number {
    let consents = this.dungeonEnterConsents.filter(entry => entry.string1 == _dungeonId);
    if (!consents)
      return 0;
    else
      return consents.length
  }

  cleanUpConsents(): void {
    this.dungeonEnterConsents = this.dungeonEnterConsents.filter(consent =>
      this.partyMembersUidList.includes(consent.string2)
    );
  }


}


export const PartyConverter = {
  toFirestore: (_party: Party) => {

    return {
      uid: _party.uid,
      partyLeaderUid: _party.partyLeaderUid,
      partySizeMax: _party.partySizeMax,
      partyMembers: _party.partyMembers,
      partyMembersUidList: _party.partyMembersUidList,
      dungeonProgress: _party.dungeonProgress,
      dungeonEnterConsents: _party.dungeonEnterConsents

    };
  },
  fromFirestore: (snapshot: any, options: any) => {
    const data = snapshot.data(options);

    // let pointsOnInterest: PointOfInterest[] = [];

    // data.pointsOfInterest.forEach(element => {
    //   pointsOnInterest.push(new PointOfInterest(element.id, element.enemies, element.exploreTimePrice, element.pointOfInterestType, element.questgivers, element.vendors));
    // });

    return new Party(data.uid, data.partyLeaderUid, data.partySizeMax, data.partyMembers, data.partyMembersUidList, data.dungeonProgress, data.dungeonEnterConsents);
  }
};



// [Party]
export class PartyMember {
  constructor(
    public uid: string,
    public displayName: string,
    public characterClass: string,
    public level: number,
    public position: WorldPosition,
    // public isPartyLeader: boolean,
    public isOnline: boolean,
    public characterPortrait: string
  ) { }
}

//Nemuze tu byt PartyUid na kterou je invite poslany, protoze kdyz hrac nema jeste partu a posle vic lidem invite, tak co? Prvni acceptne zalozi partu a ostatni? ti nevi jaky je party Uid vubec
export class PartyInvite {
  constructor(
    //public uid: string,
    public partyLeaderUid: string,
    public partyLeaderDisplayName: string,
    public invitedCharacterUid: string,
    public partyUid: string
    //  public partyUid: string,
  ) { }
}

export class DungeonProgress {
  constructor(
    public dungeonId: string,
    public partySize: number,
    public tierReached: number,
    public tiersMax: number,
    public rewards: ContentContainer[], //tohle je presny content. Tedy equip nebo i bezny item s presne definovanyma vecma jako price a stackSize atd...defakto nevyuzivane..presnou kopii dostanes do inventare
    public rewardsRandomEquip: RandomEquip[], // tohle je nahodny equip....co to bude presne se dogeneruje
    public rewardsGenerated: ItemIdWithAmount[], //tohle jsou bezne itemy, kde vim ID a dogeneruje se u nich ty blbosti, stcksize a u jabka a lahvi kolik leci atd nez se pridaji do inventare a stane se z nich content container
    public isEndlessDungeon: boolean,
    public isFinalDungeon: boolean,
    public characterLevelMax: number

  ) { }
}



exports.exitDungeon = functions.https.onCall(async (data, context) => {

  //TODO : dat nejaky fatigue/time punish za exit dungeonu? protoze jinak muzu pendlovat sem a tam a zatezovat servery zadara
  let callerCharacterUid = data.callerCharacterUid;

  const PartiesDb = admin.firestore().collection('parties');
  const myPartyDb = PartiesDb.where("partyMembersUidList", "array-contains", callerCharacterUid).withConverter(PartyConverter);
  const characterDb = await admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);

  try {

    const result = await admin.firestore().runTransaction(async (t: any) => {

      const characterDoc = await t.get(characterDb);
      let characterData: CharacterDocument = characterDoc.data();

      let myPartyData: Party = new Party("", "", 0, [], [], null, []);

      //ziskam tvoji partu
      await t.get(myPartyDb).then(querry => {
        if (querry.size == 1) {
          querry.docs.forEach(doc => {
            myPartyData = doc.data();
          });
        }
        else if (querry.size > 1)
          throw "You are more than in 1 party! How could this be? DATABASE ERROR!";
      });

      //zkontroluju jestli na tvojem PoI je exit z dungeonu
      const pointOfInterestDb = admin.firestore().collection('_metadata_zones').doc(characterData.position.zoneId).collection("locations").doc(characterData.position.locationId).collection("pointsOfInterest").doc(characterData.position.pointOfInterestId);
      const pointOfInterestDoc = await t.get(pointOfInterestDb);
      let pointOfInterestData: PointOfInterest = pointOfInterestDoc.data();

      //  console.log(dungeonLocationMetadataData.getPointOfInterestById(characterData.position.pointOfInterestId).specials.includes("DUNGEON_EXIT"));
      if (pointOfInterestData.specials.includes(POI_SPECIALS.DUNGEON_EXIT))
        throw "There is no Dungeon Exit on your Point of interest!";

      if (myPartyData.dungeonProgress != null) {

        //presunu se ven z dungeonu
        // characterData.position.locationId = myPartyData.dungeonProgress.dungeonExitLocationId;
        // characterData.position.pointOfInterestId = myPartyData.dungeonProgress.dungeonExitPointOfInterestId;

        //jeste updatnu svou novou pozici v parte
        let myPartyEntry = myPartyData.getPartyMemberByUid(characterData.uid);
        if (myPartyEntry != undefined) {
          myPartyEntry.position.locationId = characterData.position.locationId;
          myPartyEntry.position.pointOfInterestId = characterData.position.pointOfInterestId;
        }
        else
          throw ("Database error, cant find your entry in party!");


      }
      else {
        throw ("Database error, you want to exit dungeon but have no dungeonProgress data?!");
      }

      t.set(PartiesDb.doc(myPartyData.uid), JSON.parse(JSON.stringify(myPartyData)), { merge: true });
      t.set(characterDb, JSON.parse(JSON.stringify(characterData)), { merge: true });

      return "Ok";

    });

    console.log('Transaction success', result);
    return result;
  } catch (e) {
    console.log('Transaction failure:', e);

    throw new functions.https.HttpsError("aborted", "Error : " + e);
  }


});


exports.enterDungeon = functions.https.onCall(async (data, context) => {


  let callerCharacterUid = data.callerCharacterUid;
  // let dungeonPointOfInterestId = data.dungeonPointOfInterestId;  //defakto potreba jen pro party leadera kery chce 


  //const partyInviteDb = admin.firestore().collection('partyInvites').doc(callerCharacterUid);
  const PartiesDb = admin.firestore().collection('parties');
  // const myPartyDb = PartiesDb.where("partyMembersUidList", "array-contains", callerCharacterUid).withConverter(PartyConverter);
  const characterDb = await admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);
  // const encountersDb = admin.firestore().collection('encounters');


  try {

    const result = await admin.firestore().runTransaction(async (t: any) => {

      const characterDoc = await t.get(characterDb);
      let characterData: CharacterDocument = characterDoc.data();


      let myPartyData = await QuerryForParty(t, characterData.uid);

      if (myPartyData == null)
        throw ("You must be in party to open the dungeon!");


      // if (myPartyData.partyLeaderUid != characterData.uid)
      //   throw ("Only party leader can open the dungeon!");




      const locationDb = admin.firestore().collection('_metadata_zones').doc(characterData.position.zoneId).collection("locations").doc(characterData.position.locationId);//.withConverter(LocationConverter);
      // const locationEnemyDefinitionsDb = locationDb.collection("definitions").doc("ENEMIES");


      // const locationLootSpotDefinitionsDb = locationDb.collection("definitions").doc("LOOT_SPOTS");
      const pointOfInterestDb = locationDb.collection("pointsOfInterest").doc(characterData.position.pointOfInterestId);
      const pointOfInterestServerDataDefinitionsDb = locationDb.collection("pointsOfInterest").doc(characterData.position.pointOfInterestId).collection("definitions").doc("SERVER_DATA");

      //nactu si data o PoI
      var pointOfInterestDoc = await t.get(pointOfInterestDb);
      const pointOfInterestData: PointOfInterest = pointOfInterestDoc.data();

      //nactu si definice o Server datech
      let pointOfInterestServerDataDefinitionsDoc = await t.get(pointOfInterestServerDataDefinitionsDb.withConverter(PointOfInterestServerDataDefinitionsConverter));
      let pointOfInterestServerDataDefinitionsData: PointOfInterestServerDataDefinitions = pointOfInterestServerDataDefinitionsDoc.data();



      if (pointOfInterestData.dungeon == null)
        throw ("Database error : you are trying to enter a dungeon where there are no data about dungeon!!");

      if (pointOfInterestServerDataDefinitionsData.dungeon == null)
        throw ("Database error : you are trying to enter a dungeon where there are no data about dungeon!");

      if (pointOfInterestData.dungeon.characterLevelMax != -1 && characterData.stats.level > pointOfInterestData.dungeon.characterLevelMax)
        throw ("You level it too hight to enter!");

      if (pointOfInterestData.dungeon.characterLevelMin != -1 && characterData.stats.level < pointOfInterestData.dungeon.characterLevelMin)
        throw ("Your level is too low to enter!");

      if (characterData.dungeonsFinished.includes(pointOfInterestData.typeId))
        throw "You have already completed this dungeon. Cant join the dungeon!";


      //parta jeste neni v zadnem dungeonu
      if (myPartyData.dungeonProgress != null)
        throw "Your party  already opened some other dungeon!";

      if (myPartyData.partyMembersUidList.length != pointOfInterestData.dungeon.partySize)
        throw "Your party has wrong amount of heroes to enter to the dungeon. Your party size: " + myPartyData.partyMembersUidList.length + " but dungeon is for " + pointOfInterestData.dungeon.partySize + " heroes";

      myPartyData.giveConsentToEnterDungeon(pointOfInterestData.typeId, characterData.uid);

      //mame dost consentu, vytvorime dungeon progress
      if (myPartyData.getNumberOfConsentsToEnterDungeon(pointOfInterestData.typeId) == pointOfInterestData.dungeon.partySize) {
        myPartyData.dungeonProgress = new DungeonProgress(pointOfInterestData.typeId, pointOfInterestData.dungeon.partySize, 0, pointOfInterestServerDataDefinitionsData.dungeon.tiers.length, pointOfInterestData.dungeon.rewards, pointOfInterestData.dungeon.rewardsRandomEquip, pointOfInterestData.dungeon.rewardsGenerated, pointOfInterestData.dungeon.isEndlessDungeon, pointOfInterestData.dungeon.isFinalDungeon, pointOfInterestData.dungeon.characterLevelMax);
      }

      // t.set(encountersDb.doc(dungeonEncounter.uid), JSON.parse(JSON.stringify(dungeonEncounter)), { merge: true });
      t.set(PartiesDb.doc(myPartyData.uid), JSON.parse(JSON.stringify(myPartyData)), { merge: true });

      return "Ok";

    });

    console.log('Transaction success', result);
    return result;
  } catch (e) {
    console.log('Transaction failure:', e);

    throw new functions.https.HttpsError("aborted", "Error : " + e);
  }


});



exports.sendPartyInvite = functions.https.onCall(async (data, context) => {//1 R , 1 W

  let callerCharacterName = data.callerCharacterName;
  let callerCharacterUid = data.callerCharacterUid;
  let invitedCharacterUid = data.invitedCharacterUid;

  const partyInviteDb = admin.firestore().collection('partyInvites').doc(callerCharacterUid);
  const myPartyDb = admin.firestore().collection('parties').where("partyMembersUidList", "array-contains", callerCharacterUid);
  const invitedCharacterPartyDb = admin.firestore().collection('parties').where("partyMembersUidList", "array-contains", invitedCharacterUid);
  const invitedCharacterPartyInviteDb = admin.firestore().collection('partyInvites').where("invitedCharacterUid", "==", invitedCharacterUid);
  ///  const characterDb = await admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter).get();


  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {

      let myPartyData: Party = new Party("", "", 0, [], [], null, []);
      //  let myPartyUid = "";
      //Nemuzes invitnout sam sebe
      if (invitedCharacterUid == callerCharacterUid)
        throw "you cant invite self!";

      //ziskam tvoji partu
      await t.get(myPartyDb).then(querry => {
        if (querry.size == 1) {
          querry.docs.forEach(doc => {

            myPartyData = doc.data();
            //   myPartyUid = myPartyData.uid;
            if (myPartyData.partyLeaderUid != callerCharacterUid)
              throw "Only party leader can invite new members!";

            if (myPartyData.partyMembers.length == myPartyData.partySizeMax)
              throw "Party is already full!";

            if (myPartyData.dungeonProgress != null)
              throw "You are in Dungeon. Cant invite new members!"

          });
        }
        else if (querry.size > 1)
          throw "You are more than in 1 party! How could this be? DATABASE ERROR!";
      });

      //zkontroluje ze invited player neni uz v parte
      await t.get(invitedCharacterPartyDb).then(querry => {
        if (querry.size > 0) {
          throw "Player you want to invite is already in party!";
        }

      });

      //zkontroluje ze invited player nema uz invite nejaky jiny
      await t.get(invitedCharacterPartyInviteDb).then(querry => {
        if (querry.size > 0) {
          throw "Player is bussy!";
        }

      });


      const partyInvite = new PartyInvite(callerCharacterUid, callerCharacterName, invitedCharacterUid, myPartyData.uid);
      t.set(partyInviteDb, JSON.parse(JSON.stringify(partyInvite)));
      return "Party invite created";

    });

    console.log('Transaction success', result);
    return result;
  } catch (e) {
    console.log('Transaction failure:', e);

    throw new functions.https.HttpsError("aborted", "Error : " + e);
  }


});



exports.acceptPartyInvite = functions.https.onCall(async (data, context) => {//1 R , 1 W

  const PARTY_MAX_SIZE = 5;

  const batch = admin.firestore().batch();

  var partyLeaderUid: string = data.partyLeaderUid;
  var callerCharacterUid: string = data.callerCharacterUid;
  // console.log("partyInviteUid: " + partyInviteUid);
  const encountersDb = admin.firestore().collection('encounters');
  const partiesDb = admin.firestore().collection('parties');

  const callerRareEncountersDb = encountersDb.where("foundByCharacterUid", "==", callerCharacterUid).where("encounterContext", "==", ENCOUNTER_CONTEXT.PERSONAL).where("maxCombatants", ">", 1).withConverter(encounterDocumentConverter);
  const partyLeaderRareEncountersDb = encountersDb.where("foundByCharacterUid", "==", partyLeaderUid).where("encounterContext", "==", ENCOUNTER_CONTEXT.PERSONAL).where("maxCombatants", ">", 1).withConverter(encounterDocumentConverter);
  // const callerPersonalEncounters = encountersDb.where("foundByCharacterUid", "==", callerCharacterUid).withConverter(encounterDocumentConverter);

  const partyInviteDb = admin.firestore().collection('partyInvites').doc(partyLeaderUid);
  const callerCharacterDb = admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);


  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {

      const characterDoc = await t.get(callerCharacterDb);
      let callerCharacterData: CharacterDocument = characterDoc.data();

      const partyInviteDoc = await t.get(partyInviteDb);
      const partyInviteData: PartyInvite = partyInviteDoc.data();


      const partyLeaderCharacterDb = admin.firestore().collection('characters').doc(partyInviteData.partyLeaderUid).withConverter(characterDocumentConverter);
      const partyRareEncountersDb = encountersDb.where("encounterContext", "==", ENCOUNTER_CONTEXT.PERSONAL).where("maxCombatants", ">", 1).where("partyId", "==", partyInviteData.partyUid).withConverter(encounterDocumentConverter);

      //     const partyLeaderPersonalEncounters = encountersDb.where("foundByCharacterUid", "==", partyInviteData.partyLeaderUid).where("encounterContext", "==", ENCOUNTER_CONTEXT.PERSONAL).withConverter(encounterDocumentConverter);
      // const partyMonsterEncounters = encountersDb.where("foundByCharacterUid", "==", partyInviteData.partyUid).withConverter(encounterDocumentConverter);

      //console.log("null 0 : " + partyInviteData.partyLeaderUid);
      //podle me nestaci ziskat tam kde jsi leader....muze se stat ze ten co ti poslal invite do party se stane party memberem jine party do ktere ho pozval nekdo jiny! a tim padem je v parte ale neni paryt leader! Proto toto  misto toho zakomentovaneho
      const partyDb = admin.firestore().collection('parties').where("partyMembersUidList", "array-contains", partyInviteData.partyLeaderUid);
      // const partyDb = admin.firestore().collection('parties').where("partyLeaderUid", "==", partyInviteData.partyLeaderUid);



      let partyData: Party | undefined;
      //ziskam  partu
      await t.get(partyDb).then(querry => {
        if (querry.size == 1) {
          querry.docs.forEach(doc => {

            partyData = doc.data();
          });
        }
        else if (querry.size > 1)
          throw "Party leader has more than in 1 party! How could this be? DATABASE ERROR!";
      });


      let dungeonEncounterData: EncounterDocument | undefined;
      const callerRareEncounters: EncounterDocument[] = [];
      const partyLeaderRareEncounters: EncounterDocument[] = [];
      const partyRareEncounters: EncounterDocument[] = [];

      //Parta jeste nebyla vytvorena, budeme prvni dvojka...teoreticky bych mohl i checkovat na "partyUid ==""  "
      if (partyData == undefined) {
        console.log("Parta neexistuje jeste, vytvorim ji a pridam vas dva do ni");

        const partyLeaderUidCharacterDoc = await t.get(partyLeaderCharacterDb);
        let partyLeaderCharacterData: CharacterDocument = partyLeaderUidCharacterDoc.data();

        let partyMembers: PartyMember[] = [];
        let partyMembersUidList: string[] = [];


        //pridam invitovaneho hrace
        partyMembers.push(new PartyMember(callerCharacterUid, callerCharacterData.characterName, callerCharacterData.characterClass, callerCharacterData.stats.level, callerCharacterData.position, false, callerCharacterData.characterPortrait));
        partyMembersUidList.push(callerCharacterUid);

        //TODO: Jeste by tu mel byt check na presenceStatus jestli je skutecne online.....
        //pridam hosta, tedy partyLeadera
        partyMembers.push(new PartyMember(partyLeaderCharacterData.uid, partyLeaderCharacterData.characterName, partyLeaderCharacterData.characterClass, partyLeaderCharacterData.stats.level, partyLeaderCharacterData.position, true, partyLeaderCharacterData.characterPortrait));
        partyMembersUidList.push(partyLeaderCharacterData.uid);

        partyData = new Party(partiesDb.doc().id, partyLeaderCharacterData.uid, PARTY_MAX_SIZE, partyMembers, partyMembersUidList, null, []);


        //party leaeder si zmeni u svych rare encounters partyId na id party a prida invitovaneho hraje jako watchera
        await t.get(partyLeaderRareEncountersDb).then(query => {
          query.docs.forEach(doc => {
            let encounter: EncounterDocument = doc.data();
            console.log("docLeader: " + encounter.uid);

            encounter.partyId = partyData!.uid;
            if (!encounter.watchersList.includes(callerCharacterUid)) encounter.watchersList.push(callerCharacterUid);
            partyLeaderRareEncounters.push(encounter);
          });
        });


      }
      //parta uz existuje, tak tam jen pridame invitovaneho hrace a vzajemne priadame rare encountery
      else {

        console.log("Parta uz existuje pridam te do ni");

        if (partyData.partyMembers.length == partyData.partySizeMax) {
          // await t.delete(partyInviteDb);
          throw "Party is already full!";  //TODO: tady transakce failne a nesmaze se tim padem ten partyInvite, dat tam return misto throwM??? ale pak klient tezko zjisti ze byla full nejak?
        }

        //pridam invitovaneho hrace
        partyData.partyMembers.push(new PartyMember(callerCharacterUid, callerCharacterData.characterName, callerCharacterData.characterClass, callerCharacterData.stats.level, callerCharacterData.position, false, callerCharacterData.characterPortrait));
        partyData.partyMembersUidList.push(callerCharacterUid);

        //pokud existuje dungeon encounter party do ktere se pridavam, dam sebe jako watchera abych ho videl
        const dungeonEncounterDb = admin.firestore().collection('encounters').where("foundByCharacterUid"/*"foundByPartyUid"*/, "==", partyData.uid).where("encounterContext", "==", ENCOUNTER_CONTEXT.DUNGEON).withConverter(encounterDocumentConverter);
        await t.get(dungeonEncounterDb).then(querry => {
          querry.docs.forEach(doc => {
            dungeonEncounterData = doc.data();
            if (dungeonEncounterData != undefined) {
              dungeonEncounterData.watchersList.push(callerCharacterUid);
            }
          });
        });


      }


      //invitovany hrac si zmeni u svych rare encounters partyId na id party a prida party membery jako watchery
      await t.get(callerRareEncountersDb).then(query => {
        query.docs.forEach(doc => {
          let encounter: EncounterDocument = doc.data();
          console.log("docCaller: " + encounter.uid);

          encounter.partyId = partyData!.uid;
          console.log("   encounter.partyId: " + encounter.partyId);
          partyData?.partyMembers.forEach(partyMember => { if (!encounter.watchersList.includes(partyMember.uid)) encounter.watchersList.push(partyMember.uid); });
          callerRareEncounters.push(encounter);
        });
      });

      //invitovany hrac se prida jako watcher do vsech party encounteru
      await t.get(partyRareEncountersDb).then(query => {
        query.docs.forEach(doc => {
          let encounter: EncounterDocument = doc.data();
          if (!encounter.watchersList.includes(callerCharacterUid)) encounter.watchersList.push(callerCharacterUid);
          partyRareEncounters.push(encounter);
        });
      });

      if (dungeonEncounterData != undefined)
        batch.set(encountersDb.doc(dungeonEncounterData.uid), JSON.parse(JSON.stringify(dungeonEncounterData)), { merge: true });

      for (const doc of partyLeaderRareEncounters) {
        batch.set(encountersDb.doc(doc.uid), JSON.parse(JSON.stringify(doc)), { merge: true });
      }

      for (const doc of callerRareEncounters) {
        batch.set(encountersDb.doc(doc.uid), JSON.parse(JSON.stringify(doc)), { merge: true });
      }

      for (const doc of partyRareEncounters) {
        batch.set(encountersDb.doc(doc.uid), JSON.parse(JSON.stringify(doc)), { merge: true });
      }

      batch.set(partiesDb.doc(partyData.uid), JSON.parse(JSON.stringify(partyData)), { merge: true });

      //smazeme party invite
      batch.delete(partyInviteDb);

      batch.commit();


    });

    console.log('Transaction success', result);
    return result;
  } catch (e) {
    console.log('Transaction failure:', e);
    throw new functions.https.HttpsError("aborted", "Error : " + e);
  }

});

//UNUSED!
export async function addPartyMembersAsWatchersToMyPersonalEncounters(characterUid: string, partyData: Party) {
  //asi dat jako samostanout funkci?
  //ziskam vsechny encountery
  const encountersDb = admin.firestore().collection('encounters');
  //const myPersonalEncounters = encountersDb.where("foundByCharacterUid", "==", characterUid).where("encounterContext", "==", ENCOUNTER_CONTEXT.PERSONAL).withConverter(encounterDocumentConverter);
  const myPersonalEncounters = encountersDb.where("foundByCharacterUid", "==", characterUid).withConverter(encounterDocumentConverter);
  const myPartyDb = admin.firestore().collection('parties').where("partyMembersUidList", "array-contains", characterUid);


  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {

      //pokud mi nebyly predany data party, ziskam si ji z db
      if (partyData == null) {
        await t.get(myPartyDb).then(querry => {
          if (querry.size > 1)
            throw "You are more than in 1 party! How could this be? DATABASE ERROR!";

          querry.docs.forEach(doc => {
            partyData = doc.data();
          });

        });
      }

      //projdu vsechny sve personal encountery a jako watchery pridam sve party membery
      await t.get(myPersonalEncounters).then(querry => {
        querry.docs.forEach(doc => {

          const encounter: EncounterDocument = doc.data();

          partyData.partyMembersUidList.forEach(partyMemberUid => {
            if (!encounter.watchersList.includes(partyMemberUid))
              encounter.watchersList.push(partyMemberUid);
          });

          t.set(encountersDb, JSON.parse(JSON.stringify(encounter)), { merge: true });

        });

      });

      return "OK";
    });


    console.log('Transaction success', result);
    return result;
  } catch (e) {
    console.log('Transaction failure:', e);
    throw new functions.https.HttpsError("aborted", "Error : " + e);
  }

}


exports.declinePartyInvite = functions.https.onCall(async (data, context) => {

  //TODO: zadne checky kdo declinuje atd???
  var partyLeaderUid: string = data.partyLeaderUid;
  console.log("partyLeaderUid: " + partyLeaderUid);
  const partyInviteDb = admin.firestore().collection('partyInvites').doc(partyLeaderUid);

  //smazneme invite
  partyInviteDb.delete(partyInviteDb);


});


exports.leaveParty = functions.https.onCall(async (data, context) => {//1 R , 1 W


  const batch = admin.firestore().batch();
  //TODO KDYZ ochazi party leader at preda nekomu leadership!
  var callerCharacterUid: string = data.callerCharacterUid;

  const partiesDb = admin.firestore().collection('parties');

  const callerCharacterDb = admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);
  const encountersDb = admin.firestore().collection('encounters');
  //const leaverPersonalEncountersDb = encountersDb.where("foundByCharacterUid", "==", callerCharacterUid).where("encounterContext", "==", ENCOUNTER_CONTEXT.PERSONAL).withConverter(encounterDocumentConverter);
  //const leaverPersonalEncountersDb = encountersDb.where("foundByCharacterUid", "==", callerCharacterUid).withConverter(encounterDocumentConverter);
  //ALERT: Vyzaduje composite Index , vytvoren v consoli
  //const otherThanLeaverPersonalEncountersDb = encountersDb.where("foundByCharacterUid", "!=", callerCharacterUid).where("watchersList", "array-contains", callerCharacterUid).where("encounterContext", "==", ENCOUNTER_CONTEXT.PERSONAL).withConverter(encounterDocumentConverter);
  //const otherThanLeaverPersonalEncountersDb = encountersDb.where("foundByCharacterUid", "!=", callerCharacterUid).where("watchersList", "array-contains", callerCharacterUid).withConverter(encounterDocumentConverter);

  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {

      const characterDoc = await t.get(callerCharacterDb);
      let callerCharacterData: CharacterDocument = characterDoc.data();


      //   if (callerCharacterData.isJoinedInEncounter)
      if (await QuerryIfCharacterIsInCombatAtAnyEncounter(t, callerCharacterData.uid))
        throw "Cant leave party while in combat!";


      // let partyData: Party = new Party("", "", 0, [], [], null, []);
      // //najdu partu ktere jsi clenem
      // await t.get(partiesDb.where("partyMembersUidList", "array-contains", callerCharacterUid).withConverter(PartyConverter)).then(querry => {
      //   if (querry.size == 0) {
      //     throw "You are not in any party!";
      //   }
      //   if (querry.size > 1) {
      //     throw "You are in more than 1 party?! Database error!";
      //   }

      //   querry.docs.forEach(doc => {

      //     partyData = doc.data();
      //   }
      //   );
      // });
      let partyData: Party | null = await QuerryForParty(t, callerCharacterData.uid);
      if (partyData == null)
        throw ("Database error! You are not in any party!");



      //pokud sem v endless dungeonu, nemuzu odejit z party...
      if (partyData.dungeonProgress != null)
        if (partyData.dungeonProgress.isEndlessDungeon)
          throw "You cant leave party. You need to finish the dungeon first!";

      //dungeon encounter party
      const dungeonEncounterDb = admin.firestore().collection('encounters').where("foundByCharacterUid"/*"foundByPartyUid"*/, "==", partyData.uid).where("encounterContext", "==", ENCOUNTER_CONTEXT.DUNGEON);

      //rare encountery party ktere nejsou moje
      const rarePartyEncountersNotMineDb = encountersDb.where("encounterContext", "==", ENCOUNTER_CONTEXT.PERSONAL).where("partyId", "==", partyData.uid).where("foundByCharacterUid", "!=", callerCharacterData.uid).withConverter(encounterDocumentConverter)

      //rare encountery party ktere jsou moje
      const rarePartyEncountersMineDb = encountersDb.where("encounterContext", "==", ENCOUNTER_CONTEXT.PERSONAL).where("partyId", "==", partyData.uid).where("foundByCharacterUid", "==", callerCharacterData.uid).withConverter(encounterDocumentConverter)


      //Smazu ostatni party membery jako watchery ze sveho personal encounteru

      //ziskam  personal encounter hrace co leavuje ...
      // let leaverPersonalEncounter: EncounterDocument | undefined;
      // await t.get(leaverPersonalEncountersDb).then(querry => {
      //   querry.docs.forEach(doc => {
      //     leaverPersonalEncounter = doc.data();

      //   });
      // });

      //smazu ostatni party membery 
      // if (leaverPersonalEncounter != undefined) {
      //   if (leaverPersonalEncounter.combatantList.length == 0) { //jan pokud v tom encounteru nikdo nebojuje, abych ho nesmazal ostatnim kdyz sou v boji
      //     leaverPersonalEncounter.watchersList = [];   //uplne smazu cely watchers list a pridam zas jen sebe , nemely by tam byt stejne nikdo jini nez party memberi a ja
      //     leaverPersonalEncounter.watchersList.push(callerCharacterUid);
      //   }
      // }

      //smazu hrace co leavuje z personal encounteru party memberu 

      //ziskam  encountery ostatnich party memberu a odeberu leavera z watcher listu
      // let otherThanLeaverPersonalEncounters: EncounterDocument[] = [];
      // await t.get(otherThanLeaverPersonalEncountersDb).then(querry => {
      //   querry.docs.forEach(doc => {
      //     let encounter: EncounterDocument = doc.data();
      //     console.log("encounter " + encounter.uid)
      //     if (encounter.watchersList.includes(callerCharacterUid)) {
      //       console.log("obsahuje  jako watchera leavera tedy" + callerCharacterUid + " pocet :" + encounter.watchersList.length);
      //       encounter.watchersList.splice(encounter.watchersList.indexOf(callerCharacterUid), 1);
      //     }
      //     console.log("nove tedy pocet :" + encounter.watchersList.length);
      //     otherThanLeaverPersonalEncounters.push(encounter);
      //   });
      // });


      const rarePartyEncountersNotMine: EncounterDocument[] = [];
      const rarePartyEncountersMine: EncounterDocument[] = [];
      let dungeonEncounterData: EncounterDocument | undefined;
      // let partyMonsteEncounterData: EncounterDocument | undefined;

      //jsem posledni clen co odchazi z party....smazu ji i vsechny partyInvite  .....i vsechny dungeon encountery kterych sem watcher(je to totiz lingering dungeon encounter ktery bez party neexistuje) 
      if (partyData.partyMembers.length == 2) {

        //TODO: TADY JE ERROR, ten druhy party member keremu sem znicil partu a vyhodil z dungu ma v charakteru stale posledni PoI lokaci kterou mel kdyz byl v dungeonu ! Takze pokud hned se prida do jina paty 
        //a pujde znovu dungeonu tak bude mit spatny PoI ne nazacatku ale nekde kde byl predtim....!!!!
        const partyInviteDb = admin.firestore().collection('partyInvites').where("partyLeaderUid", "==", partyData.partyLeaderUid);

        //odchazejici hrac sebe vyhodi jako watchera z rare encounteru party(ostatnich party memberu)
        await t.get(rarePartyEncountersNotMineDb).then(query => {
          query.docs.forEach(doc => {
            let encounter: EncounterDocument = doc.data();
            //partyData?.partyMembers.forEach(partyMember => { if (!encounter.watchersList.includes(partyMember.uid)) encounter.watchersList.push(partyMember.uid); });
            encounter.watchersList = encounter.watchersList.filter(item => item !== callerCharacterUid);//samzu sebe z watcher listu,
            rarePartyEncountersNotMine.push(encounter);
          });
        });

        //odchazejici hrac smaze party membery jako watchery ze svych rare encounteru...pripadne je i vyhodi z boje
        //TODO: toto je docela hnus pro hrace co bojujou a debil odejde z party...mozna to vyladit 
        await t.get(rarePartyEncountersMineDb).then(query => {
          query.docs.forEach(doc => {
            let encounter: EncounterDocument = doc.data();
            encounter.partyId = "";
            /// partyData?.partyMembers.forEach(partyMember => { if (!encounter.watchersList.includes(partyMember.uid)) encounter.watchersList.push(partyMember.uid); });
            encounter.watchersList = [];
            encounter.watchersList.push(callerCharacterUid);// encounter.watchersList.filter(item => item !== callerCharacterUid);//samzu sebe z watcher listu,
            rarePartyEncountersMine.push(encounter);
          });
        });

        const partyInviteDocsToDelete: FirebaseFirestore.DocumentReference[] = [];
        const dungeonEncounterDocsToDelete: FirebaseFirestore.DocumentReference[] = [];
        // const partyMonsterDocsToDelete: FirebaseFirestore.DocumentReference[] = [];

        // Gather all the PartyInvite documents to delete
        await t.get(partyInviteDb).then(query => {
          query.docs.forEach(doc => {
            partyInviteDocsToDelete.push(admin.firestore().collection('partyInvites').doc(doc.id));
          });
        });

        // Gather all the Dungeon Encounter documents to delete
        await t.get(dungeonEncounterDb).then(query => {
          query.docs.forEach(doc => {
            dungeonEncounterDocsToDelete.push(encountersDb.doc(doc.id));
          });
        });

        // Gather all the Monster Encounter documents to delete
        // await t.get(partyMonsterEncountersDb).then(query => {
        //   query.docs.forEach(doc => {
        //     partyMonsterDocsToDelete.push(encountersDb.doc(doc.id));
        //   });
        // });

        // Delete all the PartyInvite documents
        for (const docRef of partyInviteDocsToDelete) {
          batch.delete(docRef);
        }

        // Delete all the Dungeon Encounter documents
        for (const docRef of dungeonEncounterDocsToDelete) {
          batch.delete(docRef);
        }

        // Delete all the monster party documents
        // for (const docRef of partyMonsterDocsToDelete) {
        //   t.delete(docRef);
        // }

        // Delete the party
        batch.delete(partiesDb.doc(partyData.uid));

      }
      else {


        //smazu sebe jako watchera  z dungeon encounteru, pokud teda parta  v dungeonu nejaky vytvorila..

        await t.get(dungeonEncounterDb).then(querry => {
          querry.docs.forEach(doc => {

            dungeonEncounterData = doc.data();
            if (dungeonEncounterData != undefined) {
              if (dungeonEncounterData.watchersList.includes(callerCharacterUid))
                dungeonEncounterData.watchersList.splice(dungeonEncounterData.watchersList.indexOf(callerCharacterUid), 1);
            }
          });
        });


        //smazu sebe jako watchera z rare party monster encouneru


        //odchazejici hrac sebe vyhodi jako watchera z rare encounteru party(ostatnich party memberu)
        await t.get(rarePartyEncountersNotMineDb).then(query => {
          query.docs.forEach(doc => {
            let encounter: EncounterDocument = doc.data();
            //partyData?.partyMembers.forEach(partyMember => { if (!encounter.watchersList.includes(partyMember.uid)) encounter.watchersList.push(partyMember.uid); });
            encounter.watchersList = encounter.watchersList.filter(item => item !== callerCharacterUid);//samzu sebe z watcher listu,
            rarePartyEncountersNotMine.push(encounter);
          });
        });

        //odchazejici hrac smaze party membery jako watchery ze svych rare encounteru...pripadne je i vyhodi z boje
        //TODO: toto je docela hnus pro hrace co bojujou a debil odejde z party...mozna to vyladit 
        await t.get(rarePartyEncountersMineDb).then(query => {
          query.docs.forEach(doc => {
            let encounter: EncounterDocument = doc.data();
            encounter.partyId = "";
            /// partyData?.partyMembers.forEach(partyMember => { if (!encounter.watchersList.includes(partyMember.uid)) encounter.watchersList.push(partyMember.uid); });
            encounter.watchersList = [];
            encounter.watchersList.push(callerCharacterUid);// encounter.watchersList.filter(item => item !== callerCharacterUid);//samzu sebe z watcher listu,
            rarePartyEncountersMine.push(encounter);
          });
        });

        // await t.get(partyMonsterEncountersDb).then(querry => {
        //   querry.docs.forEach(doc => {
        //     partyMonsteEncounterData = doc.data();
        //     if (partyMonsteEncounterData) {
        //       if (partyMonsteEncounterData.watchersList.includes(callerCharacterUid)) {
        //         partyMonsteEncounterData.watchersList = partyMonsteEncounterData.watchersList.filter(item => item !== callerCharacterUid);//samzu sebe z watcher listu,
        //         //partyMonsteEncounterData.watchersList.splice(partyMonsteEncounterData.watchersList.indexOf(callerCharacterUid), 1); //samzu sebe z watcher listu,
        //       }
        //       //pokud sem vybral perk tak to smazu
        //       let myPerkChoice = partyMonsteEncounterData.perkChoices.find(choice => choice.characterUid == callerCharacterUid)
        //       if (myPerkChoice) {
        //         partyMonsteEncounterData.perkChoices.splice(partyMonsteEncounterData.perkChoices.indexOf(myPerkChoice), 1); //samzu muj vyber perku
        //       }

        //     }

        //   });
        // });


        //smazu svoje zaznamy z party

        console.log("nejsem  posledni v parte, smazu jen sebe");
        partyData.partyMembers.forEach(member => { //prohledam membery party

          let index: number = -1;

          if (member.uid == callerCharacterUid) //nase sem sebe mezi memberama,
            index = partyData!.partyMembers.indexOf(member);

          if (index > -1) // only splice array when item is found
            partyData!.partyMembers.splice(index, 1);  //smazu se z Party zaznamu

        });

        partyData.partyMembersUidList.forEach(member => { //prohledam uid list

          let index: number = -1;

          if (member == callerCharacterUid) //nase sem sebe mezi memberama,
            index = partyData!.partyMembersUidList.indexOf(member);

          if (index > -1) // only splice array when item is found
            partyData!.partyMembersUidList.splice(index, 1);  //smazu se z Party zaznamu

        });

        //pokud parta je v dungeonu promaznu svoje consenty
        // if (partyData.dungeonProgress != null)
        partyData.cleanUpConsents();


        //pokud odchazi party leader predam leadership dalsimu v poradi
        if (partyData.partyLeaderUid == callerCharacterData.uid)
          partyData.partyLeaderUid = partyData.partyMembersUidList[0];

        // partyData.partyMembers.forEach(element => {
        //   if (element.uid == partyData.partyLeaderUid) element.isPartyLeader = true;

        // });


        batch.set(partiesDb.doc(partyData.uid), JSON.parse(JSON.stringify(partyData)), { merge: true });


      }

      // otherThanLeaverPersonalEncounters.forEach(encounter => {
      //   console.log("ukladam encounter : " + encounter.uid)
      //   console.log("ukladam encounter length : " + encounter.watchersList.length)
      //   t.set(encountersDb.doc(encounter.uid), JSON.parse(JSON.stringify(encounter)), { merge: true });
      // });

      // if (leaverPersonalEncounter != undefined) {
      //   console.log("ukladam leaverPersonalEncounter : " + leaverPersonalEncounter.uid)
      //   t.set(encountersDb.doc(leaverPersonalEncounter.uid), JSON.parse(JSON.stringify(leaverPersonalEncounter)), { merge: true });
      // }


      for (const doc of rarePartyEncountersMine) {
        batch.set(encountersDb.doc(doc.uid), JSON.parse(JSON.stringify(doc)), { merge: true });
      }

      for (const doc of rarePartyEncountersNotMine) {
        batch.set(encountersDb.doc(doc.uid), JSON.parse(JSON.stringify(doc)), { merge: true });
      }

      if (dungeonEncounterData) {
        batch.set(encountersDb.doc(dungeonEncounterData.uid), JSON.parse(JSON.stringify(dungeonEncounterData)), { merge: true });
      }
      // if (partyMonsteEncounterData) {
      //   t.set(encountersDb.doc(partyMonsteEncounterData.uid), JSON.parse(JSON.stringify(partyMonsteEncounterData)), { merge: true });
      // }

      batch.commit();
      return "Party Left!";
    });



    console.log('Transaction success', result);
    return result;
  } catch (e) {
    console.log('Transaction failure:', e);
    throw new functions.https.HttpsError("aborted", "Error : " + e);
  }




});
