
// [START import]
import * as functions from "firebase-functions";
import { CharacterDocument, characterDocumentConverter, getCurrentDateTime, QuerryIfCharacterIsInCombatAtAnyEncounter, WorldPosition } from ".";
import { CombatEnemy, CombatMember, CombatStats, EncounterDocument, encounterDocumentConverter, ENCOUNTER_CONTEXT } from "./encounter";
import { firestoreAutoId } from "./general2";
import { getStartingPointOfInterestForLocation, MapLocation, LocationConverter, LOC_TYPE } from "./worldMap";

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
    public dungeonProgress: DungeonProgress | null

  ) { }

  getPartyMemberByUid(_uid: string): PartyMember | null {

    let result: PartyMember | null = null;
    this.partyMembers.forEach(element => {
      if (element.uid == _uid)
        result = element;
    });
    return result;
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
      dungeonProgress: _party.dungeonProgress

    };
  },
  fromFirestore: (snapshot: any, options: any) => {
    const data = snapshot.data(options);

    // let pointsOnInterest: PointOfInterest[] = [];

    // data.pointsOfInterest.forEach(element => {
    //   pointsOnInterest.push(new PointOfInterest(element.id, element.enemies, element.exploreTimePrice, element.pointOfInterestType, element.questgivers, element.vendors));
    // });

    return new Party(data.uid, data.partyLeaderUid, data.partySizeMax, data.partyMembers, data.partyMembersUidList, data.dungeonProgress);
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
    public isPartyLeader: boolean,
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
    public invitedCharacterUid: string
    //  public partyUid: string,
  ) { }
}

export class DungeonProgress {
  constructor(
    public dungeonLocationId: string,
    public exploredPointsOnInterest: string[],
  ) { }
}


exports.enterDungeon = functions.https.onCall(async (data, context) => {


  let callerCharacterUid = data.callerCharacterUid;
  let dungeonLocationId = data.dungeonLocationId;


  //const partyInviteDb = admin.firestore().collection('partyInvites').doc(callerCharacterUid);
  const PartiesDb = admin.firestore().collection('parties');
  const myPartyDb = PartiesDb.where("partyMembersUidList", "array-contains", callerCharacterUid);
  const encounterDb = admin.firestore().collection('encounters');
  ///  const characterDb = await admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter).get();


  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {

      let myPartyData: Party = new Party("", "", 0, [], [], null);

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


      if (myPartyData.partyLeaderUid != callerCharacterUid)
        throw "Only party leader order to enter dungeon!";

      myPartyData.partyMembers.forEach(partyMember => {
        if (partyMember.position.locationId != dungeonLocationId)
          throw "All party members must be at dungeon location to enter!";
      });

      const dungeonLocationMetadataDb = admin.firestore().collection('_metadata_zones').doc(myPartyData.partyMembers[0].position.zoneId).collection("locations").doc(myPartyData.partyMembers[0].position.locationId).withConverter(LocationConverter);

      const dungeonLocationMetadataDoc = await t.get(dungeonLocationMetadataDb);
      let dungeonLocationMetadataData: MapLocation = dungeonLocationMetadataDoc.data();

      if (dungeonLocationMetadataData.locationType != LOC_TYPE.DUNGEON)
        throw "This location is not a Dungeon! Cant enter!";

      if (myPartyData.dungeonProgress != null)
        throw "You are already in dungeon! : " + myPartyData.dungeonProgress.dungeonLocationId;

      //vse ok tak vytvorime dungeon progress s prvni prozkoumanou PoI a enemy teda spawnem
      myPartyData.dungeonProgress = new DungeonProgress(dungeonLocationId, []);
      myPartyData.dungeonProgress.exploredPointsOnInterest.push(getStartingPointOfInterestForLocation(dungeonLocationId));


      //vytvorime rovnou enemy groupu (v dungeonu se nebere nahodny ale vsechny a gnorujem uplne nejaky chanceToSpawn)
      const startingPointOfInterest = dungeonLocationMetadataData.getPointOfInterestById(getStartingPointOfInterestForLocation(dungeonLocationId));
      let spawnedEnemies: CombatEnemy[] = [];
      startingPointOfInterest.enemies.forEach(enemy => {
        spawnedEnemies.push(new CombatEnemy(firestoreAutoId(), enemy.enemyId, new CombatStats(0, 0, enemy.health, enemy.health, 0, 0, 0, 0, 0, 0), enemy.damageMin, enemy.damageMax, enemy.level, enemy.mLevel, enemy.isRare, enemy.dropTable, "", [], []))
      });

      var combatants: CombatMember[] = [];//combatants.push(new CombatMember(characterData.characterName, characterData.uid, characterData.characterClass, [], characterData.converSkillsToCombatSkills(), [], characterData.converStatsToCombatStats(), 0, 0, characterData.stats.level));
      var combatantList: string[] = []; //combatantList.push(callerCharacterUid);
      var watchersList: string[] = []; watchersList.push(callerCharacterUid);
      const expireDate = getCurrentDateTime(2);
      var maxCombatants: number = 5;
      var isFull: boolean = false;

      let dungeonEncounter: EncounterDocument = new EncounterDocument(encounterDb.doc().id, spawnedEnemies, combatants, combatantList, Math.random(), expireDate, callerCharacterUid, maxCombatants, watchersList, isFull, "Ambushed", ENCOUNTER_CONTEXT.DUNGEON, myPartyData.partyMembers[0].position, 1, "Combat started!\n", "0", myPartyData.uid);

      //pridam pripadne vsechny party membery do encounteru
      myPartyData.partyMembersUidList.forEach(partyMemberUid => {
        if (!dungeonEncounter!.watchersList.includes(partyMemberUid))
          dungeonEncounter!.watchersList.push(partyMemberUid);
      });


      t.set(encounterDb.doc(dungeonEncounter.uid), JSON.parse(JSON.stringify(dungeonEncounter)), { merge: true });



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

      let myPartyData: Party = new Party("", "", 0, [], [], null);
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


      const partyInvite = new PartyInvite(callerCharacterUid, callerCharacterName, invitedCharacterUid);
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

  var partyLeaderUid: string = data.partyLeaderUid;
  var callerCharacterUid: string = data.callerCharacterUid;
  // console.log("partyInviteUid: " + partyInviteUid);
  const encountersDb = admin.firestore().collection('encounters');
  const callerPersonalEncounters = encountersDb.where("foundByCharacterUid", "==", callerCharacterUid).where("encounterContext", "==", ENCOUNTER_CONTEXT.PERSONAL).withConverter(encounterDocumentConverter);

  const PartiesDb = admin.firestore().collection('parties');
  const partyInviteDb = admin.firestore().collection('partyInvites').doc(partyLeaderUid);
  const callerCharacterDb = admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);


  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {

      const characterDoc = await t.get(callerCharacterDb);
      let callerCharacterData: CharacterDocument = characterDoc.data();

      const partyInviteDoc = await t.get(partyInviteDb);
      const partyInviteData: PartyInvite = partyInviteDoc.data();

      const partyLeaderUidCharacterDb = admin.firestore().collection('characters').doc(partyInviteData.partyLeaderUid).withConverter(characterDocumentConverter);
      const partyLeaderPersonalEncounters = encountersDb.where("foundByCharacterUid", "==", partyInviteData.partyLeaderUid).where("encounterContext", "==", ENCOUNTER_CONTEXT.PERSONAL).withConverter(encounterDocumentConverter);

      console.log("null 0 : " + partyInviteData.partyLeaderUid);
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

      //Parta jeste nebyla vytvorena, budeme prvni dvojka...teoreticky bych mohl i checkovat na "partyUid ==""  "
      if (partyData == undefined) {
        console.log("Parta neexistuje jeste, vytvorim ji a pridam vas dva do ni");

        const partyLeaderUidCharacterDoc = await t.get(partyLeaderUidCharacterDb);
        let partyLeaderUidCharacterData: CharacterDocument = partyLeaderUidCharacterDoc.data();

        let partyMembers: PartyMember[] = [];
        let partyMembersUidList: string[] = [];


        //pridam invitovaneho hrace
        partyMembers.push(new PartyMember(callerCharacterUid, callerCharacterData.characterName, callerCharacterData.characterClass, callerCharacterData.stats.level, callerCharacterData.position, false, true, callerCharacterData.characterPortrait));
        partyMembersUidList.push(callerCharacterUid);

        //TODO: Jeste by tu mel byt check na presenceStatus jestli je skutecne online.....
        //pridam hosta, tedy partyLeadera
        partyMembers.push(new PartyMember(partyLeaderUidCharacterData.uid, partyLeaderUidCharacterData.characterName, partyLeaderUidCharacterData.characterClass, partyLeaderUidCharacterData.stats.level, partyLeaderUidCharacterData.position, true, true, partyLeaderUidCharacterData.characterPortrait));
        partyMembersUidList.push(partyLeaderUidCharacterData.uid);

        partyData = new Party(PartiesDb.doc().id, partyLeaderUidCharacterData.uid, PARTY_MAX_SIZE, partyMembers, partyMembersUidList, null);
        // t.set(PartiesDb, JSON.parse(JSON.stringify(newParty)));
      }
      //parta uz existuje, tak se tam jen pridame
      else {

        console.log("Parta uz existuje pridam te do ni");
        //TODO: tady transakce failne a nesmaze se tim padem ten partyInvite
        if (partyData.partyMembers.length == partyData.partySizeMax) {
          // await t.delete(partyInviteDb);
          throw "Party is already full!";  //TODO: tady transakce failne a nesmaze se tim padem ten partyInvite, dat tam return misto throwM??? ale pak klient tezko zjisti ze byla full nejak?
        }

        //pridam invitovaneho hrace
        partyData.partyMembers.push(new PartyMember(callerCharacterUid, callerCharacterData.characterName, callerCharacterData.characterClass, callerCharacterData.stats.level, callerCharacterData.position, false, true, callerCharacterData.characterPortrait));
        partyData.partyMembersUidList.push(callerCharacterUid);

        //pokud existuje dungeon encounter party do ktere se pridavam, dam sebe jako watchera abych ho videl
        const dungeonEncounterDb = admin.firestore().collection('encounters').where("foundByPartyUid", "==", partyData.uid).where("encounterContext", "==", ENCOUNTER_CONTEXT.DUNGEON).withConverter(encounterDocumentConverter);
        await t.get(dungeonEncounterDb).then(querry => {
          querry.docs.forEach(doc => {
            dungeonEncounterData = doc.data();
            if (dungeonEncounterData != undefined) {
              dungeonEncounterData.watchersList.push(callerCharacterUid);
            }
          });
        });

      }



      //ziskam  personal encounter invitovaneho hace pokud existuje....
      let callerPersonalEncounter: EncounterDocument | undefined;
      await t.get(callerPersonalEncounters).then(querry => {
        querry.docs.forEach(doc => {
          callerPersonalEncounter = doc.data();
          console.log("jo invitovany hrac ma encounter personal :" + callerPersonalEncounter?.uid);
        });
      });

      //...  pokud existuje, jako watchery pridam sve party membery
      if (callerPersonalEncounter != undefined) {
        partyData.partyMembersUidList.forEach(partyMemberUid => {
          console.log("je tam uz tento watacher? :" + partyMemberUid);
          if (!callerPersonalEncounter!.watchersList.includes(partyMemberUid))
            callerPersonalEncounter!.watchersList.push(partyMemberUid);
          console.log("pridavam jako watchera :" + partyMemberUid);
        });

      }

      //ziskam  personal encounter leadera  pokud existuje....
      let leaderPersonalEncounter: EncounterDocument | undefined;
      await t.get(partyLeaderPersonalEncounters).then(querry => {
        querry.docs.forEach(doc => {
          leaderPersonalEncounter = doc.data();

        });
      });

      //...  pokud existuje, jako watchery pridam sve party membery 
      if (leaderPersonalEncounter != undefined) {
        partyData.partyMembersUidList.forEach(partyMemberUid => {
          if (!leaderPersonalEncounter!.watchersList.includes(partyMemberUid))
            leaderPersonalEncounter!.watchersList.push(partyMemberUid);
        });

      }

      if (callerPersonalEncounter != undefined)
        t.set(encountersDb.doc(callerPersonalEncounter.uid), JSON.parse(JSON.stringify(callerPersonalEncounter)), { merge: true });

      if (leaderPersonalEncounter != undefined)
        t.set(encountersDb.doc(leaderPersonalEncounter.uid), JSON.parse(JSON.stringify(leaderPersonalEncounter)), { merge: true });

      if (dungeonEncounterData != undefined)
        t.set(encountersDb.doc(dungeonEncounterData.uid), JSON.parse(JSON.stringify(dungeonEncounterData)), { merge: true });

      t.set(PartiesDb.doc(partyData.uid), JSON.parse(JSON.stringify(partyData)), { merge: true });



      //smazeme party invite
      t.delete(partyInviteDb);




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
  const myPersonalEncounters = encountersDb.where("foundByCharacterUid", "==", characterUid).where("encounterContext", "==", ENCOUNTER_CONTEXT.PERSONAL).withConverter(encounterDocumentConverter);
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


  //TODO KDYZ ochazi party leader at preda nekomu leadership!
  var callerCharacterUid: string = data.callerCharacterUid;

  const partiesDb = admin.firestore().collection('parties');

  const callerCharacterDb = admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);
  const encountersDb = admin.firestore().collection('encounters');
  const leaverPersonalEncountersDb = encountersDb.where("foundByCharacterUid", "==", callerCharacterUid).where("encounterContext", "==", ENCOUNTER_CONTEXT.PERSONAL).withConverter(encounterDocumentConverter);
  //ALERT: Vyzaduje composite Index , vytvoren v consoli
  const otherThanLeaverPersonalEncountersDb = encountersDb.where("foundByCharacterUid", "!=", callerCharacterUid).where("watchersList", "array-contains", callerCharacterUid).where("encounterContext", "==", ENCOUNTER_CONTEXT.PERSONAL).withConverter(encounterDocumentConverter);


  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {

      const characterDoc = await t.get(callerCharacterDb);
      let callerCharacterData: CharacterDocument = characterDoc.data();


      //   if (callerCharacterData.isJoinedInEncounter)
      if (await QuerryIfCharacterIsInCombatAtAnyEncounter(t, callerCharacterData.uid))
        throw "Cant leave party while in combat!";


      let partyData: Party = new Party("", "", 0, [], [], null);
      //najdu partu ktere jsi clenem
      await t.get(partiesDb.where("partyMembersUidList", "array-contains", callerCharacterUid).withConverter(PartyConverter)).then(querry => {
        if (querry.size == 0) {
          throw "You are not in any party!";
        }
        if (querry.size > 1) {
          throw "You are in more than 1 party?! Database error!";
        }

        querry.docs.forEach(doc => {

          partyData = doc.data();
        }
        );
      });

      //dungeon encounter party
      const dungeonEncounterDb = admin.firestore().collection('encounters').where("foundByPartyUid", "==", partyData.uid).where("encounterContext", "==", ENCOUNTER_CONTEXT.DUNGEON);

      //Smazu ostatni party membery jako watchery ze sveho personal encounteru

      //ziskam  personal encounter hrace co leavuje ...
      let leaverPersonalEncounter: EncounterDocument | undefined;
      await t.get(leaverPersonalEncountersDb).then(querry => {
        querry.docs.forEach(doc => {
          leaverPersonalEncounter = doc.data();

        });
      });

      //smazu ostatni party membery 
      if (leaverPersonalEncounter != undefined) {
        if (leaverPersonalEncounter.combatantList.length == 0) { //jan pokud v tom encounteru nikdo nebojuje, abych ho nesmazal ostatnim kdyz sou v boji
          leaverPersonalEncounter.watchersList = [];   //uplne smazu cely watchers list a pridam zas jen sebe , nemely by tam byt stejne nikdo jini nez party memberi a ja
          leaverPersonalEncounter.watchersList.push(callerCharacterUid);
        }
      }

      //smazu hrace co leavuje z personal encounteru party memberu 

      //ziskam  encountery ostatnich party memberu a odeberu leavera z watcher listu
      let otherThanLeaverPersonalEncounters: EncounterDocument[] = [];
      await t.get(otherThanLeaverPersonalEncountersDb).then(querry => {
        querry.docs.forEach(doc => {
          let encounter: EncounterDocument = doc.data();
          console.log("encounter " + encounter.uid)
          if (encounter.watchersList.includes(callerCharacterUid)) {
            console.log("obsahuje  jako watchera leavera tedy" + callerCharacterUid + " pocet :" + encounter.watchersList.length);
            encounter.watchersList.splice(encounter.watchersList.indexOf(callerCharacterUid), 1);
          }
          console.log("nove tedy pocet :" + encounter.watchersList.length);
          otherThanLeaverPersonalEncounters.push(encounter);
        });
      });

      let dungeonEncounterData: EncounterDocument | undefined;


      //pokud parta byla v dungeonu
      if (partyData.dungeonProgress != null) {
        //nastavim si startovni PoI na vychozi PoI dungeonu. Aby mi nezustala PoI nekde uvnitr dungeonu kdyz sem leavnul pro priste.... pokud jsem teda na lokaci dungeonu...ufffff
        if (callerCharacterData.position.locationId == partyData.dungeonProgress.dungeonLocationId) {
          callerCharacterData.position.pointOfInterestId = getStartingPointOfInterestForLocation(partyData.dungeonProgress.dungeonLocationId);

        }
      }

      //jsem posledni clen co odchazi z party....smazu ji i vsechny partyInvite  .....i vsechny dungeon encountery kterych sem watcher(je to totiz lingering dungeon encounter ktery bez party neexistuje)
      if (partyData.partyMembers.length == 2) {

        //TODO: TADY JE ERROR, ten druhy party member keremu sem znicil partu a vyhodil z dungu ma v charakteru stale posledni PoI lokaci kterou mel kdyz byl v dungeonu ! Takze pokud hned se prida do jina paty 
        //a pujde znovu dungeonu tak bude mit spatny PoI ne nazacatku ale nekde kde byl predtim....!!!!
        const partyInviteDb = admin.firestore().collection('partyInvites').where("partyLeaderUid", "==", partyData.partyLeaderUid);

        //smazu vsechny PartyInvite
        await t.get(partyInviteDb).then(querry => {
          querry.docs.forEach(doc => {
            // let data : PartyInvite  = doc.data();
            t.delete(admin.firestore().collection('partyInvites').doc(doc.id));
          });

        });
        console.log("A: ");
        //smazu ten dungeon encounter
        await t.get(dungeonEncounterDb).then(querry => {
          console.log("A1: ");
          querry.docs.forEach(doc => {
            console.log("docId: " + doc.id);
            // let data : EncounterDocument  = doc.data();
            t.delete(encountersDb.doc(doc.id));
          });

        });

        console.log("B");
        //smazu partu
        t.delete(partiesDb.doc(partyData.uid));
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



        //smazu svoje zaznamy z party

        console.log("nejsem  posledni v parte, smazu jen sebe");
        partyData.partyMembers.forEach(member => { //prohledam membery party

          let index: number = -1;

          if (member.uid == callerCharacterUid) //nase sem sebe mezi memberama,
            index = partyData.partyMembers.indexOf(member);

          if (index > -1) // only splice array when item is found
            partyData.partyMembers.splice(index, 1);  //smazu se z Party zaznamu

        });

        partyData.partyMembersUidList.forEach(member => { //prohledam uid list

          let index: number = -1;

          if (member == callerCharacterUid) //nase sem sebe mezi memberama,
            index = partyData.partyMembersUidList.indexOf(member);

          if (index > -1) // only splice array when item is found
            partyData.partyMembersUidList.splice(index, 1);  //smazu se z Party zaznamu

        });

        //predam leadership dalsimu v poradi
        partyData.partyLeaderUid = partyData.partyMembersUidList[0];
        partyData.partyMembers.forEach(element => {
          if (element.uid == partyData.partyLeaderUid) element.isPartyLeader = true;

        });


        t.set(partiesDb.doc(partyData.uid), JSON.parse(JSON.stringify(partyData)), { merge: true });

      }

      otherThanLeaverPersonalEncounters.forEach(encounter => {
        console.log("ukladam encounter : " + encounter.uid)
        console.log("ukladam encounter length : " + encounter.watchersList.length)
        t.set(encountersDb.doc(encounter.uid), JSON.parse(JSON.stringify(encounter)), { merge: true });
      });

      if (leaverPersonalEncounter != undefined) {
        console.log("ukladam leaverPersonalEncounter : " + leaverPersonalEncounter.uid)
        t.set(encountersDb.doc(leaverPersonalEncounter.uid), JSON.parse(JSON.stringify(leaverPersonalEncounter)), { merge: true });
      }

      if (dungeonEncounterData != undefined) {
        t.set(encountersDb.doc(dungeonEncounterData.uid), JSON.parse(JSON.stringify(dungeonEncounterData)), { merge: true });
      }


      return "Party Left!";
    });



    console.log('Transaction success', result);
    return result;
  } catch (e) {
    console.log('Transaction failure:', e);
    throw new functions.https.HttpsError("aborted", "Error : " + e);
  }




});
