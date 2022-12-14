
// [START import]
import * as functions from "firebase-functions";
import { _databaseWithOptions } from "firebase-functions/v1/firestore";
import { CharacterDocument, characterDocumentConverter, CURRENCY_ID, getCurrentDateTime, QuerryIfCharacterIsInAnyEncounter, QuerryIfCharacterIsWatcherInAnyDungeonEncounter } from ".";
import { CombatEnemy, CombatMember, CombatStats, EncounterDocument, encounterDocumentConverter, ENCOUNTER_CONTEXT } from "./encounter";
import { EnemyMeta, firestoreAutoId, MineralMeta } from "./general2";
import { Party } from "./party";
import { Questgiver } from "./questgiver";
//import { CharacterDocument, characterDocumentConverter } from ".";

const admin = require('firebase-admin');
// // [END import]



export enum LOC {
  NONE = "NONE",
  VALLEY_OF_TRIALS = "VALLEY_OF_TRIALS",
  VILLAGE_OF_MALAKA = "VILLAGE_OF_MALAKA",
  DEEP_RAVINE = "DEEP_RAVINE",
  MALAKA_DUNGEON = "MALAKA_DUNGEON",
}

export enum ZONE {
  DUNOTAR = "DUNOTAR",
}

//should I use?
export enum POI {
  NONE = "NONE",
  //VALLEY OF TRIALS
  VALLEY_OF_TRIALS_PLAINS = "PLAINS",
  VALLEY_OF_TRIALS_SCORPID_LAIR = "SCORPID_LAIR",
  VALLEY_OF_TRIALS_VILE_DEN = "VILE_DEN",
  VALLEY_OF_TRIALS_MUDDY_PLAINS = "MUDDY_PLAINS",
  //DEEP_RAVINE
  DEEP_RAVINE_ROCKY_PATH = "ROCKY_PATH",
  //MALAKA_DUNGEON
  MALAKA_DUNGEON_ENTRANCE = "MALAKA_DUNGEON_ENTRANCE",
  MALAKA_DUNGEON_STAIRWAY = "MALAKA_DUNGEON_STAIRWAY",
}


export enum POI_TYPE {
  ENCOUNTER = "ENCOUNTER",
  DUNGEON = "DUNGEON"
}

export enum LOC_TYPE {
  ENCOUNTERS = "ENCOUNTERS",
  DUNGEON = "DUNGEON",
  TOWN = "TOWN"
}


export class LocationMeta {
  constructor(
    public locationType: string,
    public pointsOfInterest: PointOfInterest[],
    public questgivers: Questgiver[],

  ) { }

  getPointOfInterestById(_id: string): PointOfInterest {

    for (const element of this.pointsOfInterest) {
      if (element.id == _id)
        return element;
    }

    throw "Could not find Point of Interest ID : " + _id;

  }

  getQuestgiverById(_id: string): Questgiver {

    for (const element of this.questgivers) {
      if (element.id == _id)
        return element;
    }

    throw "Could not find Questgiver with ID : " + _id;

  }
}

export class PointOfInterest {
  constructor(
    public id: string,
    public enemies: EnemyMeta[],
    //  public minerals: MineralMeta[],
    public exploreTimePrice: number,
    public pointOfInterestType: string
  ) { }

}

export const LocationMetaConverter = {
  toFirestore: (_location: LocationMeta) => {


    return {
      // displayName: _location.displayName,
      pointsOfInterest: _location.pointsOfInterest,

    };
  },
  fromFirestore: (snapshot: any, options: any) => {
    const data = snapshot.data(options);
    return new LocationMeta(data.locationType, data.pointsOfInterest, data.questgivers);
  }
};



class NodeVertex {
  constructor(
    public idOfVertex: string,
    public weight: number,
  ) { }
}

class Vertex {
  id: string;
  nodes: NodeVertex[];
  weight: number;

  constructor(theId: string, theNodes: NodeVertex[]) {//, theWeight: number) {
    this.id = theId;
    this.nodes = theNodes;
    this.weight = 0;//theWeight;
  }
}


class VertexExport { //pouze pro export do konzole pak copy paste do databaze
  id: string;
  nodes: NodeVertex[];

  constructor(theId: string, theNodes: NodeVertex[]) {//, theWeight: number) {
    this.id = theId;
    this.nodes = theNodes;
  }
}

class DijskraResult {
  constructor(
    public resultPathWeigth: number,
    public nodesOnPath: string[]
  ) {


  }
}

// class DijkstraForExportOnly {

//   dijkstraMap: any[];
//   constructor() {
//     this.dijkstraMap = [];
//   }


//   addVertex(vertex: Vertex): void {
//     //  this.vertices[vertex.name] = vertex;
//     this.dijkstraMap.push(vertex);
//   }
// }

class Dijkstra {

  exportMap: VertexExport[]; //used for export only
  vertices: any;
  constructor() {
    this.vertices = {};
    this.exportMap = [];//used for export only
  }


  addVertex(vertex: Vertex): void {
    this.vertices[vertex.id] = vertex;
    this.exportMap.push(new VertexExport(vertex.id, vertex.nodes));
  }

  findPointsOfShortestWay(start: string, finish: string, weight: number): string[] {

    let nextVertex: string = finish;
    let arrayWithVertex: string[] = [];
    while (nextVertex !== start) {

      let minWeigth: number = Number.MAX_VALUE;
      let minVertex: string = "";
      for (let i of this.vertices[nextVertex].nodes) {
        if (i.weight + this.vertices[i.idOfVertex].weight < minWeigth) {
          minWeigth = this.vertices[i.idOfVertex].weight;
          minVertex = i.idOfVertex;
        }
      }
      arrayWithVertex.push(minVertex);
      nextVertex = minVertex;
    }
    return arrayWithVertex;
  }


  findShortestWay(start: string, finish: string): DijskraResult {//string[] {

    let result = new DijskraResult(0, []);
    let nodes: any = {};
    //  let visitedVertex: string[] = [];

    for (let i in this.vertices) {
      if (this.vertices[i].id === start) {
        this.vertices[i].weight = 0;

      } else {
        this.vertices[i].weight = Number.MAX_VALUE;
      }
      nodes[this.vertices[i].id] = this.vertices[i].weight;
    }

    while (Object.keys(nodes).length !== 0) {
      let sortedVisitedByWeight: string[] = Object.keys(nodes).sort((a, b) => this.vertices[a].weight - this.vertices[b].weight);
      let currentVertex: Vertex = this.vertices[sortedVisitedByWeight[0]];
      for (let j of currentVertex.nodes) {
        const calculateWeight: number = currentVertex.weight + j.weight;
        //const calculateWeight: number =  j.weight;
        if (calculateWeight < this.vertices[j.idOfVertex].weight) {
          this.vertices[j.idOfVertex].weight = calculateWeight;
        }
      }
      delete nodes[sortedVisitedByWeight[0]];
    }
    const finishWeight: number = this.vertices[finish].weight;
    let arrayWithVertex: string[] = this.findPointsOfShortestWay(start, finish, finishWeight).reverse();
    arrayWithVertex.push(finish, finishWeight.toString());


    result.nodesOnPath = arrayWithVertex;
    result.resultPathWeigth = finishWeight;
    //return arrayWithVertex;
    return result;
  }

}

export function getStartingPointOfInterestForLocation(_locationId: string): string {
  switch (_locationId) {
    case LOC.VALLEY_OF_TRIALS: return POI.VALLEY_OF_TRIALS_PLAINS;
    case LOC.VILLAGE_OF_MALAKA: return POI.NONE;
    case LOC.DEEP_RAVINE: return POI.DEEP_RAVINE_ROCKY_PATH;
    case LOC.MALAKA_DUNGEON: return POI.MALAKA_DUNGEON_ENTRANCE;
    default:
      {
        throw console.log("Could not find starting point of interest for location : " + _locationId);
      }
  }
}


function getWorldMap(): Dijkstra {

  let dijkstra = new Dijkstra();

  dijkstra.addVertex(new Vertex(LOC.VALLEY_OF_TRIALS, [new NodeVertex(LOC.VILLAGE_OF_MALAKA, 8)]));
  dijkstra.addVertex(new Vertex(
    LOC.VILLAGE_OF_MALAKA, [
    new NodeVertex(LOC.DEEP_RAVINE, 50),
    new NodeVertex(LOC.VALLEY_OF_TRIALS, 8),
    new NodeVertex(LOC.MALAKA_DUNGEON, 10),
  ]));
  dijkstra.addVertex(new Vertex(LOC.DEEP_RAVINE, [new NodeVertex(LOC.VILLAGE_OF_MALAKA, 50)]));
  dijkstra.addVertex(new Vertex(LOC.MALAKA_DUNGEON, [new NodeVertex(LOC.VILLAGE_OF_MALAKA, 10)]));

  return dijkstra;
}

function getLocationMap(_locationId: string): Dijkstra {

  let dijkstra = new Dijkstra();

  switch (_locationId) {
    case LOC.VALLEY_OF_TRIALS:
      dijkstra.addVertex(new Vertex(POI.VALLEY_OF_TRIALS_PLAINS,
        [
          new NodeVertex(POI.VALLEY_OF_TRIALS_SCORPID_LAIR, 2),
          new NodeVertex(POI.VALLEY_OF_TRIALS_MUDDY_PLAINS, 3)
        ]));

      dijkstra.addVertex(new Vertex(POI.VALLEY_OF_TRIALS_SCORPID_LAIR,
        [
          new NodeVertex(POI.VALLEY_OF_TRIALS_VILE_DEN, 2),
          new NodeVertex(POI.VALLEY_OF_TRIALS_PLAINS, 2),
          new NodeVertex(POI.VALLEY_OF_TRIALS_MUDDY_PLAINS, 2)

        ]));

      dijkstra.addVertex(new Vertex(POI.VALLEY_OF_TRIALS_VILE_DEN,
        [
          new NodeVertex(POI.VALLEY_OF_TRIALS_SCORPID_LAIR, 2)
        ]));

      dijkstra.addVertex(new Vertex(POI.VALLEY_OF_TRIALS_MUDDY_PLAINS,
        [
          new NodeVertex(POI.VALLEY_OF_TRIALS_SCORPID_LAIR, 2),
          new NodeVertex(POI.VALLEY_OF_TRIALS_PLAINS, 3)
        ]));
      break;
    case LOC.MALAKA_DUNGEON:
      dijkstra.addVertex(new Vertex(POI.MALAKA_DUNGEON_ENTRANCE,
        [
          new NodeVertex(POI.MALAKA_DUNGEON_STAIRWAY, 1)
        ]));

      dijkstra.addVertex(new Vertex(POI.MALAKA_DUNGEON_STAIRWAY,
        [
          new NodeVertex(POI.MALAKA_DUNGEON_ENTRANCE, 1)
        ]));
      break;


    default:
      throw console.log("Could not map for location : " + _locationId);
  }


  return dijkstra;
}

function printWorldMap() {

  console.log("-----------------------WORLD_MAP--------------------------");
  const worldMap = getWorldMap();
  console.log("Toto ulozit do DB Maps jako WoldMapu : " + JSON.stringify(worldMap.exportMap));

  for (let i in worldMap.vertices) {
    worldMap.vertices[i].nodes.forEach(node => {
      console.log((worldMap.vertices[i] as Vertex).id + " " + (node as NodeVertex).idOfVertex + " " + (node as NodeVertex).weight);
    });
  }
  console.log("-----------------------WORLD_MAP--------------------------");

}

function printLocationMap(_map: Dijkstra) {

  console.log("-----------------------LOCATION_MAP--------------------------");
  //const locationMap = getLocationMap(_locationId);
  console.log("Tohle ulozit do DB Maps pro danou lokaci : " + JSON.stringify(_map.exportMap));

  for (let i in _map.vertices) {
    _map.vertices[i].nodes.forEach(node => {
      console.log((_map.vertices[i] as Vertex).id + " " + (node as NodeVertex).idOfVertex + " " + (node as NodeVertex).weight);
    });
  }
  console.log("-----------------------LOCATION_MAP--------------------------");

}


//TODO: tady toto rozstepit na travel to DungeonPoi...v clientovi pak mit jinou prefabu na dugeonpoi a volat jiny cloud funkci tot vse
exports.travelToPoI = functions.https.onCall(async (data, context) => {

  //TODO:  asi checky jestli vubec si v lokaci kde chces cestovat mezi POI? at nejsou nejake corrupted data o lokaci hrace v DB

  const callerCharacterUid = data.characterUid;
  const destinationPointOfInterestId = data.destinationPointOfInterestId;



  console.log("callerCharacterUid: " + callerCharacterUid);
  console.log("destinationPointOfInterestId: " + destinationPointOfInterestId);

  const characterDb = await admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);
  const partiesDb = admin.firestore().collection('parties')
  const myPartyDb = partiesDb.where("partyMembersUidList", "array-contains", callerCharacterUid);
  const encounterDb = admin.firestore().collection('encounters');
  //Tento querry VYZADUJE COMPOSITE INDEX , byl vytvoren v FIRESTORE!!!
  // const callerPersonalEncounterWithoutCombatants = encountersDb.where("foundByCharacterUid", "==", callerCharacterUid).where("encounterContext", "==", ENCOUNTER_CONTEXT.PERSONAL).where("combatantList", "==", []).withConverter(encounterDocumentConverter);

  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {

      const characterDoc = await t.get(characterDb);
      let characterData: CharacterDocument = characterDoc.data();

      const destinationLocationMetadataDb = admin.firestore().collection('_metadata_zones').doc(characterData.position.zoneId).collection("locations").doc(characterData.position.locationId).withConverter(LocationMetaConverter);

      const destinationLocationMetadataDoc = await t.get(destinationLocationMetadataDb);
      let destinationLocationMetadataData: LocationMeta = destinationLocationMetadataDoc.data();
      let destinationPointOfInterestMetadataData: PointOfInterest = destinationLocationMetadataData.getPointOfInterestById(destinationPointOfInterestId);


      var map = getLocationMap(characterData.position.locationId);
      printLocationMap(map);

      const resultTravel = map.findShortestWay(characterData.position.pointOfInterestId, destinationPointOfInterestId);

      console.log("Total travel time from :" + characterData.position.pointOfInterestId + " to : " + destinationPointOfInterestId + " is : " + resultTravel.resultPathWeigth);
      console.log("Nodes on path are: ");
      resultTravel.nodesOnPath.forEach(node => { console.log(node); });


      //Nastavime novou pozici a sebereme cas
      if (characterData.currency.time >= resultTravel.resultPathWeigth) {
        characterData.subCurrency(CURRENCY_ID.TIME, resultTravel.resultPathWeigth);
        characterData.position.pointOfInterestId = destinationPointOfInterestId;
      }
      else
        throw ("Not enough Time to travel! You have :" + characterData.currency.time + " but the travel needs : " + resultTravel.resultPathWeigth);


      //Ulozime ze si prozkoumal tuto lokaci pokud nebyla neprozkoumana
      // pokud je to normalni PoI tak do charakteru
      if (destinationPointOfInterestMetadataData.pointOfInterestType == POI_TYPE.ENCOUNTER) {
        if (!characterData.exploredPositions.pointsOfInterest.includes(destinationPointOfInterestId))
          characterData.exploredPositions.pointsOfInterest.push(destinationPointOfInterestId);
      }

      //pokud jsi v parte updatnu tvoji lokaci i tam
      let myPartyData: Party | undefined;
      await t.get(myPartyDb).then(querry => {
        if (querry.size == 1) {
          querry.docs.forEach(doc => {
            myPartyData = doc.data();
            if (myPartyData != undefined) {
              //najdu svuj zaznam v parte a updatnu lokaci
              myPartyData.partyMembers.forEach(element => {
                if (element.uid == callerCharacterUid)
                  element.position.pointOfInterestId = destinationPointOfInterestId;
              });
            }
          });
        }
        else if (querry.size > 1)
          throw "You are more than in 1 party! How could this be? DATABASE ERROR!";
      });

      //pokud jsi tedy v parte, chces cestovat ale uz je vytvoreny Dungeon encounter ktereho si tim padem watcherem, nemuzes cestovat ( je to kvuli tomu aby lidi prozkoumavali dungeon hezky postupne)
      //bohuzel toto pridat 1 read KDYKOLIV kdyz v parte prozkoumavas PoI....
      if (myPartyData != undefined) {
        if (await QuerryIfCharacterIsWatcherInAnyDungeonEncounter(t, callerCharacterUid))
          throw ("There are enemies nearby your party!");

        //Ulozime ze si prozkoumal tuto lokaci do party pokud nebyla neprozkoumana
        if (destinationPointOfInterestMetadataData.pointOfInterestType == POI_TYPE.DUNGEON) {
          if (myPartyData.dungeonProgress == null)
            throw "How is it possible that you are in party, traveling in dungeon, but party has no dungeonProgress entry??";

          //pokud lokace nebyla jeste prozkoumana, ulozime ze je a vytvorime rovnou skupinu enemy
          if (!myPartyData.dungeonProgress.exploredPointsOnInterest.includes(destinationPointOfInterestId)) {

            //ulozime si do party ze byla lokace prozkoumana
            myPartyData.dungeonProgress.exploredPointsOnInterest.push(destinationPointOfInterestId);

            //vytvorime rovnou enemy groupu (v dungeonu se nebere nahodny ale vsechny a gnorujem uplne nejaky chanceToSpawn)
            let spawnedEnemies: CombatEnemy[] = [];
            destinationPointOfInterestMetadataData.enemies.forEach(enemy => {
              spawnedEnemies.push(new CombatEnemy(firestoreAutoId(), enemy.enemyId, new CombatStats(0, 0, enemy.health, enemy.health, 0, 0, 0, 0, 0, 0), enemy.damageMin, enemy.damageMax, enemy.level, enemy.mLevel, enemy.isRare, enemy.dropTable, "", [], []))
            });

            var combatants: CombatMember[] = [];//combatants.push(new CombatMember(characterData.characterName, characterData.uid, characterData.characterClass, [], characterData.converSkillsToCombatSkills(), [], characterData.converStatsToCombatStats(), 0, 0, characterData.stats.level));
            var combatantList: string[] = []; //combatantList.push(callerCharacterUid);
            var watchersList: string[] = []; watchersList.push(callerCharacterUid);
            const expireDate = getCurrentDateTime(2);
            var maxCombatants: number = 5;
            var isFull: boolean = false;

            let dungeonEncounter: EncounterDocument = new EncounterDocument(encounterDb.doc().id, spawnedEnemies, combatants, combatantList, Math.random(), expireDate, callerCharacterUid, maxCombatants, watchersList, isFull, characterData.characterName, ENCOUNTER_CONTEXT.DUNGEON, characterData.position, 1, "Combat started!\n", "0");

            //pridam pripadne vsechny party membery do encounteru
            myPartyData.partyMembersUidList.forEach(partyMemberUid => {
              if (!dungeonEncounter!.watchersList.includes(partyMemberUid))
                dungeonEncounter!.watchersList.push(partyMemberUid);
            });


            t.set(encounterDb.doc(dungeonEncounter.uid), JSON.parse(JSON.stringify(dungeonEncounter)), { merge: true });
          }
        }

      }


      if (myPartyData != undefined)
        t.set(partiesDb.doc(myPartyData.uid), JSON.parse(JSON.stringify(myPartyData)), { merge: true });

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

exports.travel = functions.https.onCall(async (data, context) => {

  printWorldMap();

  const callerCharacterUid = data.characterUid;
  const destinationLocationId = data.locationId;

  console.log("callerCharacterUid: " + callerCharacterUid);
  console.log("destinationLocationId: " + destinationLocationId);

  const characterDb = await admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);
  const encountersDb = admin.firestore().collection('encounters');
  const partiesDb = admin.firestore().collection('parties')
  const myPartyDb = partiesDb.where("partyMembersUidList", "array-contains", callerCharacterUid);
  //Tento querry VYZADUJE COMPOSITE INDEX , byl vytvoren v FIRESTORE!!!
  const callerPersonalEncounterWithoutCombatants = encountersDb.where("foundByCharacterUid", "==", callerCharacterUid).where("encounterContext", "==", ENCOUNTER_CONTEXT.PERSONAL).where("combatantList", "==", []).withConverter(encounterDocumentConverter);

  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {

      const characterDoc = await t.get(characterDb);
      let characterData: CharacterDocument = characterDoc.data();

      const destinationLocationMetadataDb = admin.firestore().collection('_metadata_zones').doc("DUNOTAR").collection("locations").doc(destinationLocationId);

      const destinationLocationMetadataDoc = await t.get(destinationLocationMetadataDb);
      let destinationLocationMetadataData: LocationMeta = destinationLocationMetadataDoc.data();


      //TODO:  nemel by byt problem moc profiltrovat world map dijkstru podle toho c characetdocument ma explored a nema
      const resultTravel = getWorldMap().findShortestWay(characterData.position.locationId, destinationLocationId);

      console.log("Total travel time from :" + characterData.position.locationId + " to : " + destinationLocationId + " is : " + resultTravel.resultPathWeigth);
      console.log("Nodes on path are: ");
      resultTravel.nodesOnPath.forEach(node => { console.log(node); });


      //Nastavime novou pozici a sebereme cas
      if (characterData.currency.time >= resultTravel.resultPathWeigth) {
        characterData.subCurrency(CURRENCY_ID.TIME, resultTravel.resultPathWeigth);
        characterData.position.locationId = destinationLocationId;
        characterData.position.pointOfInterestId = getStartingPointOfInterestForLocation(destinationLocationId);
      }
      else
        throw ("Not enough Time to travel! You have :" + characterData.currency.time + " but the travel needs : " + resultTravel.resultPathWeigth);


      //Kdyz si v boji nemuzes cestovat
      if (await QuerryIfCharacterIsInAnyEncounter(t, characterData.uid))
        throw ("You cannot travel while in combat!");

      //Poku si v lokaci poprve a prave jsi ji prozkoumal ulozime ze si ji prozkoumal a jeji startovni pozici taky.
      if (!characterData.exploredPositions.locations.includes(destinationLocationId)) {
        characterData.exploredPositions.locations.push(destinationLocationId);

        if (destinationLocationMetadataData.locationType != LOC_TYPE.DUNGEON)//..pokud  to neni dungeon tam se startovni pozice prozkovama v parte pri enter dungeonu
          characterData.exploredPositions.pointsOfInterest.push(getStartingPointOfInterestForLocation(destinationLocationId));
      }

      //smazu tvuj personal encounter pokud v lokaci mas nejaky
      let myEncounterDoc: EncounterDocument | undefined;
      await t.get(callerPersonalEncounterWithoutCombatants).then(querry => {
        querry.docs.forEach(doc => {
          myEncounterDoc = doc.data();
        });
      });

      //pokud jsi v parte updatnu tvoji lokaci i tam
      let myPartyData: Party | undefined;
      await t.get(myPartyDb).then(querry => {
        if (querry.size == 1) {
          querry.docs.forEach(doc => {
            myPartyData = doc.data();
            //najdu svuj zaznam a updatnu lokaci
            if (myPartyData != undefined) {
              myPartyData.partyMembers.forEach(element => {
                if (element.uid == callerCharacterUid)
                  element.position.locationId = destinationLocationId;
              });
            }
          });
        }
        else if (querry.size > 1)
          throw "You are more than in 1 party! How could this be? DATABASE ERROR!";
      });

      if (myEncounterDoc != undefined)
        t.delete(encountersDb.doc(myEncounterDoc.uid));

      if (myPartyData != undefined)
        t.set(partiesDb.doc(myPartyData.uid), JSON.parse(JSON.stringify(myPartyData)), { merge: true });
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