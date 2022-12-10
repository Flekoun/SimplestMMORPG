
// [START import]
import * as functions from "firebase-functions";
import { _databaseWithOptions } from "firebase-functions/v1/firestore";
import { CharacterDocument, characterDocumentConverter, CURRENCY_ID, QuerryIsCharacterIsInAnyEncounter } from ".";
import { EncounterDocument, encounterDocumentConverter, ENCOUNTER_CONTEXT } from "./encounter";
import { EnemyMeta, MineralMeta } from "./general2";
import { Party } from "./party";
//import { CharacterDocument, characterDocumentConverter } from ".";

const admin = require('firebase-admin');
// // [END import]



export enum LOC {
  NONE = "NONE",
  VALLEY_OF_TRIALS = "VALLEY_OF_TRIALS",
  VILLAGE_OF_MALAKA = "VILLAGE_OF_MALAKA",
  DEEP_RAVINE = "DEEP_RAVINE",
}

export enum ZONE {
  DUNOTAR = "DUNOTAR",
}

//should I use?
export enum POI {
  NONE = "NONE",
  VALLEY_OF_TRIALS_PLAINS = "PLAINS",
  VALLEY_OF_TRIALS_SCORPID_LAIR = "SCORPID_LAIR",
  VALLEY_OF_TRIALS_VILE_DEN = "VILE_DEN",
  VALLEY_OF_TRIALS_MUDDY_PLAINS = "MUDDY_PLAINS",
  DEEP_RAVINE_ROCKY_PATH = "ROCKY_PATH",
}

export class LocationMeta {
  constructor(
    public displayName: string,
    public pointsOfInterest: PointOfInterest[],

  ) { }

  getPointOfInterestById(_id: string): PointOfInterest {

    for (const element of this.pointsOfInterest) {
      if (element.id == _id)
        return element;
    }

    throw "Could not find Point of Interest ID : " + _id;

  }
}

export class PointOfInterest {
  constructor(
    public id: string,
    public enemies: EnemyMeta[],
    public minerals: MineralMeta[],
    public exploreTimePrice: number
  ) { }

}

export const LocationMetaConverter = {
  toFirestore: (_location: LocationMeta) => {


    return {
      displayName: _location.displayName,
      pointsOfInterest: _location.pointsOfInterest,

    };
  },
  fromFirestore: (snapshot: any, options: any) => {
    const data = snapshot.data(options);
    return new LocationMeta(data.displayName, data.pointsOfInterest);
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

function getStartingPointOfInterestForLocation(_locationId: string): string {
  switch (_locationId) {
    case LOC.VALLEY_OF_TRIALS: return POI.VALLEY_OF_TRIALS_PLAINS;
    case LOC.VILLAGE_OF_MALAKA: return POI.NONE;
    case LOC.DEEP_RAVINE: return POI.DEEP_RAVINE_ROCKY_PATH;
    default:
      {
        throw console.log("Could not find starting point of interest for location : " + _locationId);
      }
  }
}


function getWorldMap(): Dijkstra {

  let dijkstra = new Dijkstra();

  dijkstra.addVertex(new Vertex(LOC.VALLEY_OF_TRIALS, [new NodeVertex(LOC.VILLAGE_OF_MALAKA, 8)]));
  dijkstra.addVertex(new Vertex(LOC.VILLAGE_OF_MALAKA, [new NodeVertex(LOC.DEEP_RAVINE, 50), new NodeVertex(LOC.VALLEY_OF_TRIALS, 8)]));
  dijkstra.addVertex(new Vertex(LOC.DEEP_RAVINE, [new NodeVertex(LOC.VILLAGE_OF_MALAKA, 50)]));

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



exports.travelToPoI = functions.https.onCall(async (data, context) => {

  //TODO:  asi checky jestli vubec si v lokaci kde chces cestovat mezi POI? at nejsou nejake corrupted data o lokaci hrace v DB

  const callerCharacterUid = data.characterUid;
  const destinationPointOfInterestId = data.destinationPointOfInterestId;



  console.log("callerCharacterUid: " + callerCharacterUid);
  console.log("destinationPointOfInterestId: " + destinationPointOfInterestId);

  const characterDb = await admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);
  //const encountersDb = admin.firestore().collection('encounters');
  const partiesDb = admin.firestore().collection('parties')
  const myPartyDb = partiesDb.where("partyMembersUidList", "array-contains", callerCharacterUid);
  //Tento querry VYZADUJE COMPOSITE INDEX , byl vytvoren v FIRESTORE!!!
  // const callerPersonalEncounterWithoutCombatants = encountersDb.where("foundByCharacterUid", "==", callerCharacterUid).where("encounterContext", "==", ENCOUNTER_CONTEXT.PERSONAL).where("combatantList", "==", []).withConverter(encounterDocumentConverter);

  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {

      const characterDoc = await t.get(characterDb);
      let characterData: CharacterDocument = characterDoc.data();


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
        // characterData.position.pointOfInterestId = getStartingPointOfInterestForLocation(destinationLocationId);
      }
      else
        throw ("Not enough Time to travel! You have :" + characterData.currency.time + " but the travel needs : " + resultTravel.resultPathWeigth);


      //Ulozime ze si prozkoumal tuto lokaci pokud byla neprozkoumana
      if (!characterData.exploredPositions.pointsOfInterest.includes(destinationPointOfInterestId))
        characterData.exploredPositions.pointsOfInterest.push(destinationPointOfInterestId);


      //Kdyz si v boji nemuzes cestovat
      //if (characterData.isJoinedInEncounter)
      // if (await QuerryIsCharacterIsInAnyEncounter(t, characterData.uid))
      //   throw ("You cannot explore while in combat!");

      // await t.get(encountersWhereCallerIsCombatant).then(querry => {
      //   if (querry.size > 0)
      //     throw "Cant travel when in combat!";
      // });

      //smazu tvuj personal encounter pokud v lokaci mas nejaky
      // let myEncounterDoc: EncounterDocument | undefined;
      // await t.get(callerPersonalEncounterWithoutCombatants).then(querry => {
      //   querry.docs.forEach(doc => {
      //     myEncounterDoc = doc.data();
      //   });
      // });

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
                  element.position.pointOfInterestId = destinationPointOfInterestId;
              });
            }
          });
        }
        else if (querry.size > 1)
          throw "You are more than in 1 party! How could this be? DATABASE ERROR!";
      });

      // if (myEncounterDoc != undefined)
      //   t.delete(encountersDb.doc(myEncounterDoc.uid));

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
      //if (characterData.isJoinedInEncounter)
      if (await QuerryIsCharacterIsInAnyEncounter(t, characterData.uid))
        throw ("You cannot explore while in combat!");

      //Ulozime ze si prozkoumal tuto lokaci pokud byla neprozkoumana
      if (!characterData.exploredPositions.locations.includes(destinationLocationId))
        characterData.exploredPositions.locations.push(destinationLocationId);

      // await t.get(encountersWhereCallerIsCombatant).then(querry => {
      //   if (querry.size > 0)
      //     throw "Cant travel when in combat!";
      // });

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