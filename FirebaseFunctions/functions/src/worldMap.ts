
// [START import]
import * as functions from "firebase-functions";

import { _databaseWithOptions } from "firebase-functions/v1/firestore";
import { CharacterDocument, characterDocumentConverter, checkForServerVersion, ContentContainer, CURRENCY_ID, getCurrentDateTime, HIGH_LEVEL_POI_FATIGUE_PENALTY, QuerryHasCharacterAnyUnclaimedEncounterResult, QuerryIfCharacterIsInAnyEncounterOnHisPosition, QuerryIfCharacterIsInCombatAtAnyEncounter, QuerryIfCharacterIsWatcherInAnyEncounterOnHisPosition, randomIntFromInterval, rollForRandomItem, TIME_PER_TRAVEL_POINT, validateCallerBulletProof, WorldPosition } from ".";
import { CombatEnemy, CombatMember, CombatStats, ENCOUNTER_CONTEXT, EncounterDocument, encounterDocumentConverter, EnemyDefinitionMoveSetSkill, EnemyDefinitions, EnemyDefinitionsConverter, joinCharacterToEncounter, TierMonstersDefinition, TierDungeonDefinition } from "./encounter";
import { IHasChanceToSpawn, ItemIdWithAmount } from "./equip";

import { Party } from "./party";
import { Questgiver, RandomEquip } from "./questgiver";

import { Vendor } from "./vendor";
import { firestoreAutoId } from "./general2";
import { PerksOffersRareData } from "./adminTools";
//import { CharacterDocument, characterDocumentConverter } from ".";

const admin = require('firebase-admin');
// // [END import]



export enum LOC {
  NONE = "NONE",
  VALLEY_OF_TRIALS = "VALLEY_OF_TRIALS",
  VILLAGE_OF_MALAKA = "VILLAGE_OF_MALAKA",
  DEEP_RAVINE = "DEEP_RAVINE",
  MALAKA_DUNGEON = "MALAKA_DUNGEON",

  SEASON_TEST = "SEASON_TEST"
}

export enum ZONE {
  DUNOTAR = "DUNOTAR",
}

export enum POI {
  NONE = "NONE",
  //VALLEY OF TRIALS
  POI_A1 = "POI_A1",
  VALLEY_OF_TRIALS_SCORPID_LAIR = "SCORPID_LAIR",
  VALLEY_OF_TRIALS_VILE_DEN = "VILE_DEN",
  VALLEY_OF_TRIALS_MUDDY_PLAINS = "MUDDY_PLAINS",
  VALLEY_OF_TRIALS_VILE_DEN_HIDEOUT = "VILE_DEN_HIDEOUT",
  VALLEY_OF_TRIALS_MALAKA_DUNGEON = "MALAKA_DUNGEON",
  //DEEP_RAVINE
  DEEP_RAVINE_ROCKY_PATH = "ROCKY_PATH",
  //MALAKA_DUNGEON
  MALAKA_DUNGEON_0 = "MALAKA_DUNGEON_0",
  MALAKA_DUNGEON_1 = "MALAKA_DUNGEON_1",
  MALAKA_DUNGEON_2 = "MALAKA_DUNGEON_2",
  MALAKA_DUNGEON_3 = "MALAKA_DUNGEON_3",
  MALAKA_DUNGEON_4 = "MALAKA_DUNGEON_4",
  MALAKA_DUNGEON_5 = "MALAKA_DUNGEON_5",
  MALAKA_DUNGEON_6 = "MALAKA_DUNGEON_6",
  MALAKA_DUNGEON_7 = "MALAKA_DUNGEON_7",
  //VILLAGE_OF_MALAKA
  VILLAGE_OF_MALAKA_MARKET = "VILLAGE_OF_MALAKA_MARKET",
  VILLAGE_OF_TOWNSQUARE = "VILLAGE_OF_MALAKA_TOWNSQUARE",
  VILLAGE_OF_MALAKA_GREEN_COVE = "VILLAGE_OF_MALAKA_GREEN_COVE",
  VILLAGE_OF_MALAKA_OLD_ANVIL = "VILLAGE_OF_MALAKA_OLD_ANVIL",
  //NEZAPOMEN PRIDAT I DO : getPoIForLocationId!! KDYZ PRIDAVAS NOVE

  POI_START = "POI_START"

}


export enum POI_SPECIALS {
  AUCTION_HOUSE = "AUCTION_HOUSE",
  MAILBOX = "MAILBOX",
  BARBER = "BARBER",
  DUNGEON_ENTRANCE = "DUNGEON_ENTRANCE",
  DUNGEON_EXIT = "DUNGEON_EXIT",
  FORGE = "FORGE",
  INN = "INN",
  VENDOR = "VENDOR", //nepouziva se jako special, je tu kvuli worldMapMemmory....at nemusim vytvaret nove "POI_SPECIAL_MEMMORY"
  QUESTGIVER = "QUESTGIVER", //nepouziva se jako special, je tu kvuli worldMapMemmory....at nemusim vytvaret nove "POI_SPECIAL_MEMMORY"
  CHAPEL = "CHAPEL",
  TREASURE = "TREASURE"
}

// export enum POI_TYPE {
//   ENCOUNTER = "ENCOUNTER",
//   DUNGEON = "DUNGEON",
//   TOWN = "TOWN"
// }

// export enum LOC_TYPE {
//   ENCOUNTERS = "ENCOUNTERS",
//   DUNGEON = "DUNGEON",
//   TOWN = "TOWN"
// }



export async function QuerryForPointOfInterestCharacterIsAt(_transaction: any, _character: CharacterDocument): Promise<PointOfInterest> {

  const pointOfInterestDb = admin.firestore().collection('_metadata_zones').doc(_character.position.zoneId).collection("locations").doc(_character.position.locationId).collection("pointsOfInterest").doc(_character.position.pointOfInterestId).withConverter(PointOfInterestConverter);//.doc(questgiverUid);
  const pointOfInterestDoc = await _transaction.get(pointOfInterestDb);
  let pointOfInterestData: PointOfInterest = pointOfInterestDoc.data();

  return pointOfInterestData;//.getQuestgiverById(questgiverUid);// questgiverDoc.data();


}




export function getLocationsForZoneId(_zoneId: string): string[] {

  let locList: string[] = [];

  switch (_zoneId) {
    case ZONE.DUNOTAR:
      locList.push(LOC.VALLEY_OF_TRIALS);
      locList.push(LOC.VILLAGE_OF_MALAKA);
      locList.push(LOC.DEEP_RAVINE);
      locList.push(LOC.MALAKA_DUNGEON);
      break;

    default:
      throw ("There is no location with ID : " + _zoneId);
  }
  return locList;

}

export function getPoIsForLocationId(_locId: string): string[] {

  let poiList: string[] = [];

  switch (_locId) {
    case LOC.VALLEY_OF_TRIALS:
      poiList.push(POI.POI_A1);
      poiList.push(POI.VALLEY_OF_TRIALS_SCORPID_LAIR);
      poiList.push(POI.VALLEY_OF_TRIALS_VILE_DEN);
      poiList.push(POI.VALLEY_OF_TRIALS_MUDDY_PLAINS);
      break;
    case LOC.VILLAGE_OF_MALAKA:
      poiList.push(POI.VILLAGE_OF_MALAKA_MARKET);
      poiList.push(POI.VILLAGE_OF_TOWNSQUARE);
      break;
    case LOC.MALAKA_DUNGEON:
      poiList.push(POI.MALAKA_DUNGEON_0);
      poiList.push(POI.MALAKA_DUNGEON_1);
      break;
    case LOC.DEEP_RAVINE:
      poiList.push(POI.DEEP_RAVINE_ROCKY_PATH);
      break;


    default:
      throw ("There is no location with ID : " + _locId);
      break;

  }
  return poiList;

}


export class MapLocation { //Chtel bych aby se to menovalo Location ale to uz je zabrane nejakou kktskou firebase clasou
  constructor(
    public locationType: string,
    public dijkstraMap: Dijkstra,
    public graveyard: string,
    public id: string,
    public perksRareOffers: PerksOffersRareData[]

  ) { }

  increaseStockClaimedForRarePerk(_rarePerkUid: string, _perkGroupId: string) {

    let perkOffersData = this.perksRareOffers.find(perkGroup => perkGroup.id == _perkGroupId);
    // for (const rarePerkOffersGroup of this.perksRareOffers) {

    let perk = perkOffersData?.perks.find(perkOffer => perkOffer.uid == _rarePerkUid);
    if (perk) {
      perk.stockClaimed++;
      return;
    }
    // }
    throw "cant find rare perk uid :" + _rarePerkUid;

  }
  // getPointOfInterestById(_id: string): PointOfInterest {

  //   for (const element of this.pointsOfInterest) {
  //     if (element.id == _id)
  //       return element;
  //   }

  //   throw "Could not find Point of Interest ID : " + _id;

  // }

}


export class Coordinates2DCartesian {
  constructor(
    public x: number,
    public y: number

  ) { }
}

// export class ScreenPositionWithId {
//   constructor(
//     public screenPosition: ScreenPosition,
//     public id: string
//   ) { }
// }


export class IdWithChance implements IHasChanceToSpawn {
  constructor(
    public id: string,
    public chanceToSpawn: number,
  ) { }
}



export class PointOfInterest {
  constructor(
    public id: string,
    public enemies: IdWithChance[],
    // public exploreTimePrice: number,
    public pointOfInterestType: number,
    public questgivers: Questgiver[],
    public vendors: Vendor[],
    // public trainers: Trainer[],
    public specials: string[],
    public worldPosition: WorldPosition,
    //  public maxCombatants: number,
    public typeId: string,
    public floorNumber: number,
    public dungeon: DungeonDefinitionPublic | null,
    public monsters: MonstersDefinitionPublic | null,
    public roomType: string
    //  public chapelBlessId: string | null

  ) { }



  getQuestgiverById(_id: string): Questgiver {

    for (const element of this.questgivers) {
      if (element.id == _id)
        return element;
    }

    throw "Could not find Questgiver with ID : " + _id;

  }


  getVendorById(_id: string): Vendor {

    for (const element of this.vendors) {
      if (element.id == _id)
        return element;
    }

    throw "Could not find Vendor with ID : " + _id;

  }

}


export const PointOfInterestConverter = {
  toFirestore: (_pointOfInterest: PointOfInterest) => {

    return {


      id: _pointOfInterest.id,
      enemies: _pointOfInterest.enemies,
      pointOfInterestType: _pointOfInterest.pointOfInterestType,
      questgivers: _pointOfInterest.questgivers,
      vendors: _pointOfInterest.vendors,
      // trainers: _pointOfInterest.trainers,
      specials: _pointOfInterest.specials,
      worldPosition: _pointOfInterest.worldPosition,
      typeId: _pointOfInterest.typeId,
      floorNumber: _pointOfInterest.floorNumber,
      dungeon: _pointOfInterest.dungeon,
      monsters: _pointOfInterest.monsters,
      roomType: _pointOfInterest.roomType
    };
  },
  fromFirestore: (snapshot: any, options: any) => {
    const data = snapshot.data(options);

    let questGivers: Questgiver[] = [];

    data.questgivers.forEach(questgiver => {
      questGivers.push(new Questgiver(questgiver.id, questgiver.minLevel, questgiver.qLevel, questgiver.killsRequired, questgiver.rewards, questgiver.hasExpireDate, questgiver.expireDate, questgiver.itemsRequired, questgiver.rewardsRandomEquip, questgiver.prereqQuests, questgiver.expRewardPerLevel, questgiver.rewardsGenerated));
    });

    let vendors: Vendor[] = [];

    data.vendors.forEach(vendor => {
      vendors.push(new Vendor(vendor.id, vendor.displayName, vendor.goods));
    });

    return new PointOfInterest(data.id, data.enemies, data.pointOfInterestType, questGivers, vendors, data.specials, data.worldPosition, data.typeId, data.floorNumber, data.dungeon, data.monsters, data.roomType);
  }
};



export class MonstersDefinitionPublic {
  constructor(

    public partySize: number,
    public exploreTimePrice: number,
    public tiersTotal: number

  ) { }
}

export class MonstersDefinitionServerOnly {
  constructor(
    public tiers: TierMonstersDefinition[],
    public perkOffersRareId: string
  ) { }
}



export class DungeonDefinitionPublic {
  constructor(

    public partySize: number,
    public entryPrice: number,
    public rewards: ContentContainer[], //tohle je presny content. Tedy equip nebo i bezny item s presne definovanyma vecma jako price a stackSize atd...defakto nevyuzivane..presnou kopii dostanes do inventare
    public rewardsRandomEquip: RandomEquip[], // tohle je nahodny equip....co to bude presne se dogeneruje
    public rewardsGenerated: ItemIdWithAmount[], //tohle jsou bezne itemy, kde vim ID a dogeneruje se u nich ty blbosti, stcksize a u jabka a lahvi kolik leci atd nez se pridaji do inventare a stane se z nich content container
    public isEndlessDungeon: boolean,
    public isFinalDungeon: boolean,
    public characterLevelMax: number,
    public characterLevelMin: number,
    public floorsTotal: number

  ) { }
}

export class DungeonDefinitionServerOnly {
  constructor(
    public tiers: TierDungeonDefinition[],
  ) { }
}

export class PointOfInterestServerDataDefinitions {

  constructor(

    public blesses: IdWithChance[],
    public dungeon: DungeonDefinitionServerOnly | null,
    public monsters: MonstersDefinitionServerOnly | null

  ) { }

  increaseStockClaimedForPerk(_perkUid: string) {
    if (this.monsters == null)
      return;

    for (const tier of this.monsters.tiers) {
      let perkToChange = tier.perkOffers.find(perkoffer => perkoffer.uid == _perkUid);
      if (perkToChange) {
        perkToChange.stockClaimed++;
        return;
      }

    }

    throw "can find perk with UID : " + _perkUid;

    // for (const tier of this.monsters.tiers) {
    //   if (tier.increaseStockClaimedForPerk(_perkUid))
    //     return;//nasli sme perk a zvedli stockUsed, koncime loop
    // }
    // throw "can find perk with UID : " + _perkUid;

  }

}


export const PointOfInterestServerDataDefinitionsConverter = {
  toFirestore: (_entry: PointOfInterestServerDataDefinitions) => {
    return {
      monsters: _entry.monsters,
      blesses: _entry.blesses,
      dungeon: _entry.dungeon
    };
  },
  fromFirestore: (snapshot: any, options: any) => {
    const data = snapshot.data(options);

    // let tiersNew: TierMonstersDefinition[] = [];
    // data.tiers.forEach(tier => {
    //   tiersNew.push(new TierMonstersDefinition(tier.entryTimePrice, tier.enemies, tier.perkOffers));
    // });

    // let mosnterDefServer :MonstersDefinitionServerOnly = new MonstersDefinitionServerOnly(tiersNew,data.m)

    return new PointOfInterestServerDataDefinitions(data.blesses, data.dungeon, data.monsters);
  }
}

//pouziva se pro lokace, obsahuje narozdil od sucheho DjiskraConvertoru i lokacion type a grafeyard
export const LocationConverter = {
  toFirestore: (_location: MapLocation) => {

    return {
      locationType: _location.locationType,
      dijkstraMap: _location.dijkstraMap,
      graveyard: _location.graveyard,
      perksRareOffers: _location.perksRareOffers,
      id: _location.id
    };
  },
  fromFirestore: (snapshot: any, options: any) => {
    const data = snapshot.data(options);

    // let pointsOnInterest: PointOfInterest[] = [];

    // data.pointsOfInterest.forEach(element => {
    //   pointsOnInterest.push(new PointOfInterest(element.id, element.enemies, element.exploreTimePrice, element.pointOfInterestType, element.questgivers, element.vendors, element.trainers, element.screenPosition, element.lootSpots, element.specials));
    // });

    let dijkstra = new Dijkstra();

    data.dijkstraMap.exportMap.forEach(vertex => {
      dijkstra.addVertex(vertex);
    });

    return new MapLocation(data.locationType, dijkstra, data.graveyard, data.id, data.perksRareOffers);
  }
};





export class NodeVertex {
  constructor(
    public idOfVertex: string,
    public weight: number,
  ) { }
}

// class Vertex {
//   id: string;
//   nodes: NodeVertex[];
//   weight: number;


//   constructor(theId: string, theNodes: NodeVertex[]) {//, theWeight: number) {
//     this.id = theId;
//     this.nodes = theNodes;
//     this.weight = 0;//theWeight
//   }
// }


export class VertexExport { //pouze pro export do konzole pak copy paste do databaze, tedy to co chci mit v databazi pak ulozene
  public id: string;
  public nodes: NodeVertex[];
  public screenPosition: Coordinates2DCartesian;
  public mapPosition: Coordinates2DCartesian;
  public type: number;

  constructor(theId: string, theNodes: NodeVertex[], theScreenPosition: Coordinates2DCartesian, theType: number, theMapPosition: Coordinates2DCartesian) {//, theWeight: number) {
    this.id = theId;
    this.nodes = theNodes;
    this.screenPosition = theScreenPosition;
    this.type = theType;
    this.mapPosition = theMapPosition;
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

export class Dijkstra {

  //jako tyhle 2 varianty tu podle me mam proto ze v Unita rada mapu jako export map, hezky list, kdezto na serveru zase rad v tom verticesIgnore kde je to mapa .....proste neumim sloucit ty 2 algoritmy na serveru a UI aby pouzivali stejnou strukturu dat jinak bych to tu nemusel mit vubec
  public exportMap: VertexExport[]; //Hlavni definice, toto editovat v DB , tohle si nacita UI a meni 
  public verticesIgnoreThis: any; //tohleje pro server na vypocet djikstry, vygeneruje se z export mapy pokazde, ignorovat nemenit v db. Nejlepe kdybych to db vubec neukladal ale to se mi ted nechce resit musel bych to vyhodit v tom "foFirestore:" 
  constructor() {
    //  this.vertices;
    this.verticesIgnoreThis = {};
    this.exportMap = [];//used for export only
  }


  addVertex(vertex: VertexExport): void {
    this.verticesIgnoreThis[vertex.id] = vertex;
    this.exportMap.push(new VertexExport(vertex.id, vertex.nodes, vertex.screenPosition, vertex.type, vertex.mapPosition));
  }

  // findPointsOfShortestWay(start: string, finish: string, weight: number): string[] {

  //   let nextVertex: string = finish;
  //   let arrayWithVertex: string[] = [];
  //   while (nextVertex !== start) {

  //     let minWeigth: number = Number.MAX_VALUE;
  //     let minVertex: string = "";

  //     for (let i of this.verticesIgnoreThis[nextVertex].nodes) {
  //       if (i.weight + this.verticesIgnoreThis[i.idOfVertex].weight < minWeigth) {
  //         minWeigth = this.verticesIgnoreThis[i.idOfVertex].weight;
  //         minVertex = i.idOfVertex;
  //       }
  //     }
  //     arrayWithVertex.push(minVertex);
  //     nextVertex = minVertex;
  //   }
  //   return arrayWithVertex;
  // }


  //   findShortestWay(start: string, finish: string): DijskraResult {//string[] {

  //     let result = new DijskraResult(0, []);
  //     let nodes: any = {};
  //     //  let visitedVertex: string[] = [];

  //     for (let i in this.verticesIgnoreThis) {

  //       if (this.verticesIgnoreThis[i].id === start) {
  //         this.verticesIgnoreThis[i].weight = 0;

  //       } else {
  //         this.verticesIgnoreThis[i].weight = Number.MAX_VALUE;
  //       }
  //       nodes[this.verticesIgnoreThis[i].id] = this.verticesIgnoreThis[i].weight;
  //     }

  //     while (Object.keys(nodes).length !== 0) {

  //       let sortedVisitedByWeight: string[] = Object.keys(nodes).sort((a, b) => this.verticesIgnoreThis[a].weight - this.verticesIgnoreThis[b].weight);



  //       let currentVertex: Vertex = this.verticesIgnoreThis[sortedVisitedByWeight[0]];
  //       for (let j of currentVertex.nodes) {

  //         const calculateWeight: number = currentVertex.weight + j.weight;
  //         //const calculateWeight: number =  j.weight;
  //         if (calculateWeight < this.verticesIgnoreThis[j.idOfVertex].weight) {
  //           this.verticesIgnoreThis[j.idOfVertex].weight = calculateWeight;
  //         }
  //       }
  //       delete nodes[sortedVisitedByWeight[0]];
  //     }
  //     const finishWeight: number = this.verticesIgnoreThis[finish].weight;
  //     let arrayWithVertex: string[] = this.findPointsOfShortestWay(start, finish, finishWeight).reverse();
  //     arrayWithVertex.push(finish, finishWeight.toString());


  //     result.nodesOnPath = arrayWithVertex;
  //     result.resultPathWeigth = finishWeight;
  //     //return arrayWithVertex;
  //     return result;
  //   }

  findShortestPath(start: string, finish: string): DijskraResult {
    const distances: { [key: string]: number } = {};
    const previous: { [key: string]: string | null } = {};
    const nodes: string[] = Object.keys(this.verticesIgnoreThis);
    const queue: string[] = [];

    for (const node of nodes) {
      if (node === start) {
        distances[node] = 0;
        queue.push(node);
      } else {
        distances[node] = Infinity;
      }
      previous[node] = null;
    }

    while (queue.length !== 0) {
      // Sort the nodes by distance
      queue.sort((a, b) => distances[a] - distances[b]);

      const current = queue.shift()!; // Take the node with the smallest distance

      if (current === finish) {
        // Finished: reconstruct the path
        const path: string[] = [];
        let vertex = finish;
        while (vertex !== null) {
          path.push(vertex);
          vertex = previous[vertex]!;
        }
        return new DijskraResult(distances[finish], path.reverse());
      }

      if (!isFinite(distances[current])) {
        break; // Stop if the smallest distance is Infinity
      }

      for (const neighbor of this.verticesIgnoreThis[current].nodes) {
        const alt = distances[current] + neighbor.weight;
        if (alt < distances[neighbor.idOfVertex]) {
          distances[neighbor.idOfVertex] = alt;
          previous[neighbor.idOfVertex] = current;
          queue.push(neighbor.idOfVertex);
        }
      }
    }

    // If we get here, there's no path between start and finish
    throw new Error(`No path found between ${start} and ${finish}`);
  }


}

export const DijkstraConverter = {
  toFirestore: (_dijkstra: Dijkstra) => {

    return {
      // dijkstraMap: _dijkstra.vertices,
      exportMap: _dijkstra.exportMap,
    };
  },
  fromFirestore: (snapshot: any, options: any) => {
    const data = snapshot.data(options);

    let dijkstra = new Dijkstra();

    data.dijkstraMap.exportMap.forEach(vertex => {
      dijkstra.addVertex(vertex);
    });

    return dijkstra;
  }
};

export function getStartingPointOfInterestForLocation(_locationId: string): string {
  switch (_locationId) {
    case LOC.VALLEY_OF_TRIALS: return POI.POI_A1;
    case LOC.VILLAGE_OF_MALAKA: return POI.VILLAGE_OF_MALAKA_MARKET;
    case LOC.DEEP_RAVINE: return POI.DEEP_RAVINE_ROCKY_PATH;
    case LOC.MALAKA_DUNGEON: return POI.MALAKA_DUNGEON_0;
    default:
      {
        throw console.log("Could not find starting point of interest for location : " + _locationId);
      }
  }
}


// function printWorldMap(_map: Dijkstra) {

//   console.log("-----------------------WORLD_MAP--------------------------");
//   // const worldMap = getWorldMap();
//   console.log("Toto ulozit do DB Maps jako WoldMapu : " + JSON.stringify(_map.exportMap));

//   for (let i in _map.vertices) {
//     _map.vertices[i].nodes.forEach(node => {
//       console.log((_map.vertices[i] as Vertex).id + " " + (node as NodeVertex).idOfVertex + " " + (node as NodeVertex).weight);
//     });
//   }
//   console.log("-----------------------WORLD_MAP--------------------------");

// }

// function printLocationMap(_map: Dijkstra) {

//   // console.log("-----------------------LOCATION_MAP_EXPORT--------------------------");
//   // //const locationMap = getLocationMap(_locationId);
//   // console.log("Tohle ulozit do DB Maps pro danou lokaci : " + JSON.stringify(_map.exportMap));

//   // for (let i in _map.exportMap) {
//   //   _map.exportMap[i].nodes.forEach(node => {
//   //     console.log((_map.exportMap[i] as VertexExport).id + " " + (node as NodeVertex).idOfVertex + " " + (node as NodeVertex).weight);
//   //   });
//   // }
//   // console.log("-----------------------LOCATION_MAP_EXPORT--------------------------");
//   console.log("-----------------------LOCATION_MAP--------------------------");
//   //const locationMap = getLocationMap(_locationId);
//   console.log("Tohle ulozit do DB Maps pro danou lokaci : " + JSON.stringify(_map.exportMap));

//   // for (let i in _map.verticesIgnoreThis) {
//   //   _map.verticesIgnoreThis[i].nodes.forEach(node => {
//   //     console.log((_map.verticesIgnoreThis[i]).id + " " + node.idOfVertex + " " + node.weight);
//   //   });
//   // }


//   for (let i in _map.verticesIgnoreThis) {
//     console.log("----" + (_map.verticesIgnoreThis[i]).id + "----");
//     _map.verticesIgnoreThis[i].nodes.forEach(node => {
//       console.log(node.idOfVertex + " " + node.weight);
//     });
//     console.log("----");
//   }
//   console.log("-----------------------LOCATION_MAP--------------------------");

// }


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

      checkForServerVersion(data);

      const characterDoc = await t.get(characterDb);
      let characterData: CharacterDocument = characterDoc.data();

      validateCallerBulletProof(characterData, context);

      //TODO: treba toto pridava read pokazde kdyz hrac cestuje, melo by to resit hlavne UI at to zamezi, takze tady ten check jen OBCAS a pripadne logovat cheatery
      //nebo ID na personal encounter davat rovnou do charakteru pokud chci stejne mit jen jeden svuj osobni....to by taky usetrilo tyhle ready a jen bych se mrkl eslu char ma nejaky personal enc...
      if (await QuerryIfCharacterIsInAnyEncounterOnHisPosition(t, characterData))
        throw ("Enemies are nearby, you cant travel!");

      if (await QuerryHasCharacterAnyUnclaimedEncounterResult(t, characterData))
        throw ("You must loot all corpses before travel!");

      const destinationLocationMetadataDb = admin.firestore().collection('_metadata_zones').doc(characterData.position.zoneId).collection("locations").doc(characterData.position.locationId);
      const destinationLocationMetadataEnemyStatsDb = destinationLocationMetadataDb.collection("definitions").doc("ENEMIES");
      const destinationPointOfInterestDb = destinationLocationMetadataDb.collection("pointsOfInterest").doc(destinationPointOfInterestId);//characterData.position.pointOfInterestId);

      const destinationLocationMetadataDoc = await t.get(destinationLocationMetadataDb.withConverter(LocationConverter));
      let destinationLocationMetadataData: MapLocation = destinationLocationMetadataDoc.data();

      const destinationPointOfInterestDoc = await t.get(destinationPointOfInterestDb);
      let destinationPointOfInterestData: PointOfInterest = destinationPointOfInterestDoc.data();


      // let destinationPointOfInterestMetadataData: PointOfInterest = destinationLocationMetadataData.getPointOfInterestById(destinationPointOfInterestId);



      var map = destinationLocationMetadataData.dijkstraMap; //getLocationMap(characterData.position.locationId);

      //zmenime mapu podle toho ktere lokace jsou explored a pouze ty a jejich sousedy budou v nove mape po ktere muzu cestovat


      // exploredMap.verticesIgnoreThis = [];

      // projdu vsechny vertexy ....pokud dany vertext neni prozkoumana, zvednu mu vahu na absurditu aby to simulovalo defakto neexistujici cestu...melo by to stacit aby to zamezilo jakemukoliv cheatovani,
      //ze se snazis cestovat na unreachable vertexy a dijktra je defakto ignoruje...zaroven to zamezi tomu abys pouzil neprozkoumany vertex jako "most" na cestovani...protoze ja chci abych mohl cestovat na
      //neexplornute vertexy ale jen ty co primo sousedi s explornutyma ale dal nikoliv...toto vsechno by tahle jedna vec mela zajistit. V klientovi to delam sloziteji...mozna zbytecne,...mozna tam chci
      //aby UI hezky ukazovalo "unreachable"vertexy a jinak ukazovalo ty "krajni" ktere muzu expplore ale nejsou explored jeste...
      const ABSURT_WEIGHT = 999;
      for (let i in map.verticesIgnoreThis) {
        let isExplored = false;
        for (const exploredPosition of characterData.exploredPositions) {
          if (map.verticesIgnoreThis[i].id === exploredPosition.pointOfInterestId) {
            isExplored = true;
          }
        }
        if (!isExplored) {
          // map.verticesIgnoreThis[i].nodes = [];
          for (let j in map.verticesIgnoreThis[i].nodes) {
            map.verticesIgnoreThis[i].nodes[j].weight = ABSURT_WEIGHT;
          }
        }
      }

      // let exploredMap: Dijkstra = new Dijkstra();

      // for (let i in map.verticesIgnoreThis) {
      //   for (const exploredPosition of characterData.exploredPositions) {
      //     if (map.verticesIgnoreThis[i].id === exploredPosition.pointOfInterestId) {
      //       //  if (!exploredMap.verticesIgnoreThis.includes(map.verticesIgnoreThis[i])) {
      //       exploredMap.verticesIgnoreThis[map.verticesIgnoreThis[i].id] = map.verticesIgnoreThis[i];
      //       //  }

      //       // Add its neighbors as well
      //       for (let j in map.verticesIgnoreThis[i].nodes) {

      //         for (let k in map.verticesIgnoreThis) {
      //           if (map.verticesIgnoreThis[k].id === map.verticesIgnoreThis[i].nodes[j].idOfVertex) {
      //             //   if (!exploredMap.verticesIgnoreThis.includes(map.verticesIgnoreThis[k])) {
      //             exploredMap.verticesIgnoreThis[map.verticesIgnoreThis[k].id] = map.verticesIgnoreThis[k];
      //             //    }
      //           }
      //         }
      //       }


      //     }
      //   }
      // }


      // // Store all vertex IDs for quick checking
      // let allVertexIds: string[] = [];
      // for (let i in exploredMap.verticesIgnoreThis) {
      //   allVertexIds.push(exploredMap.verticesIgnoreThis[i].id);
      // }
      // // Remove all mentions of neighbors not in exploreMap
      // for (let l in exploredMap.verticesIgnoreThis) {
      //   for (let i = exploredMap.verticesIgnoreThis[l].nodes.length - 1; i >= 0; i--) {
      //     if (!allVertexIds.includes(exploredMap.verticesIgnoreThis[l].nodes[i].idOfVertex)) {
      //       console.log("ostranuji: " + exploredMap.verticesIgnoreThis[l].nodes[i].idOfVertex + " z " + exploredMap.verticesIgnoreThis[l].id);
      //       exploredMap.verticesIgnoreThis[l].nodes.splice(i, 1);
      //       //  exploredMap.verticesIgnoreThis[l].nodes[i].weight = ABSURT_WEIGHT;
      //     }
      //   }
      // }



      // printLocationMap(exploredMap);
      // const resultTravel = exploredMap.findShortestWay(characterData.position.pointOfInterestId, destinationPointOfInterestId);

      //  printLocationMap(map);
      const resultTravel = map.findShortestPath(characterData.position.pointOfInterestId, destinationPointOfInterestId);

      console.log("Total travel time from :" + characterData.position.pointOfInterestId + " to : " + destinationPointOfInterestId + " is : " + resultTravel.resultPathWeigth);
      console.log("Nodes on path are: ");
      resultTravel.nodesOnPath.forEach(node => { console.log(node); });

      //TODO: checkuju tu nekde jestli jsou vsechny PoI po ceste explored?

      //nemam travel pointy ale mam dost casu na suplement
      let travelPointsConvertedToTime = (Math.floor(characterData.currency.travelPoints) * TIME_PER_TRAVEL_POINT);
      let fakeIncreasedTravelTime = 0;
      if (characterData.currency.travelPoints < resultTravel.resultPathWeigth &&
        (characterData.currency.time + travelPointsConvertedToTime) >= resultTravel.resultPathWeigth * TIME_PER_TRAVEL_POINT) {
        //     console.log("ANO: " + travelPointsConvertedToTime);
        characterData.subCurrency(CURRENCY_ID.TIME, (resultTravel.resultPathWeigth * TIME_PER_TRAVEL_POINT) - travelPointsConvertedToTime);
        fakeIncreasedTravelTime = (CURRENCY_ID.TRAVEL_POINTS, resultTravel.resultPathWeigth - (travelPointsConvertedToTime / TIME_PER_TRAVEL_POINT));
      }

      if (characterData.currency.travelPoints + fakeIncreasedTravelTime >= resultTravel.resultPathWeigth) {
        //Nastavime novou pozici a sebereme cas

        characterData.subCurrency(CURRENCY_ID.TRAVEL_POINTS, resultTravel.resultPathWeigth - fakeIncreasedTravelTime);
        characterData.position.pointOfInterestId = destinationPointOfInterestId;
      }
      else
        throw ("Not enough Time to travel! You have :" + characterData.currency.time + " but the travel needs : " + resultTravel.resultPathWeigth);



      /*  if (characterData.currency.travelPoints >= resultTravel.resultPathWeigth) {
          //Nastavime novou pozici a sebereme cas
          characterData.subCurrency(CURRENCY_ID.TRAVEL_POINTS, resultTravel.resultPathWeigth);
          characterData.position.pointOfInterestId = destinationPointOfInterestId;
        }
        else
          throw ("Not enough Time to travel! You have :" + characterData.currency.time + " but the travel needs : " + resultTravel.resultPathWeigth);
  */


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

              if (myPartyData.dungeonProgress != null)
                throw "You are in Dungeon. Cant travel now!";
            }
          });
        }
        else if (querry.size > 1)
          throw "You are more than in 1 party! How could this be? DATABASE ERROR!";
      });

      // if (destinationPointOfInterestData.pointOfInterestType == POI_TYPE.DUNGEON) { //jen pro dungeon PoI
      //   if (myPartyData != undefined) { //...kde si samozrejme v parte...zejo

      //     if (myPartyData.dungeonProgress == null)
      //       throw "How is it possible that you are in party, traveling in dungeon, but party has no dungeonProgress entry??";

      //     //pokud lokace nebyla jeste prozkoumana, ulozime ze je a vytvorime rovnou skupinu enemy
      //     if (!myPartyData.dungeonProgress.exploredPointsOnInterest.includes(destinationPointOfInterestId)) {

      //       //ulozime si do party ze byla lokace prozkoumana
      //       myPartyData.dungeonProgress.exploredPointsOnInterest.push(destinationPointOfInterestId);

      //       // tuto featuru spis natavit nejakym jinym flagem nez ze je to dungeon.....dungeon by mohl pak jit explorovat? naopak normal cocky by davaly hned enemy? 

      //       //ziskam z db staty o enemy v teto loakci
      //       const destinationLocationMetadataEnemyStatsDoc = await t.get(destinationLocationMetadataEnemyStatsDb.withConverter(EnemyDefinitionsConverter));
      //       let destinationLocationMetadataEnemyStatsData: EnemyDefinitions = destinationLocationMetadataEnemyStatsDoc.data();

      //       //vytvorime rovnou enemy groupu (v dungeonu se nebere nahodny ale vsechny a gnorujem uplne nejaky chanceToSpawn)
      //       let spawnedEnemies: CombatEnemy[] = [];
      //       destinationPointOfInterestData.enemies.forEach(enemy => {
      //         let enemyStats = destinationLocationMetadataEnemyStatsData.getEnemyById(enemy.id);
      //         let enemyHealth = randomIntFromInterval(enemyStats.healthMin, enemyStats.healthMax);
      //         let newEnemy = new CombatEnemy(firestoreAutoId(), enemy.id, new CombatStats(0, 0, enemyHealth, enemyHealth, enemyHealth, 0, 0, 0, 5, 0, 0, 0, 0, [], 0, [], 0), enemyStats.level, enemyStats.mLevel, enemyStats.moveSet, enemyStats.isRare, "", [], [], 0, new EnemyDefinitionMoveSetSkill([], 0, "", "", false, false, false, false, false, false), 0);
      //         newEnemy.nextSkill = newEnemy.chooseSkillToCast(0);
      //         spawnedEnemies.push(newEnemy);
      //       });

      //       var combatants: CombatMember[] = [];//combatants.push(new CombatMember(characterData.characterName, characterData.uid, characterData.characterClass, [], characterData.converSkillsToCombatSkills(), [], characterData.converStatsToCombatStats(), 0, 0, characterData.stats.level));
      //       var combatantList: string[] = []; //combatantList.push(callerCharacterUid);
      //       var watchersList: string[] = []; watchersList.push(callerCharacterUid);
      //       const expireDate = getCurrentDateTime(2);
      //       var maxCombatants: number = 5;
      //       var isFull: boolean = false;

      //       // //vygeneruju loot spoty 3 volby max 6 enemy = 18 spotu
      //       // let lootSpots: LootSpot[] = [];
      //       // for (let i = 0; i < 3 * 6; i++) {
      //       //   //nadohny loot spot vyberem
      //       //   const lootSpotDef = zonesLootSpotDefinitionsData.getLootSpotById((RollForRandomItem(pointOfInterest.enemies, false) as IdWithChance).id);
      //       //   lootSpots.push(lootSpotDef.convertToLootSpot(zonesEnemyDefinitionsStatsData));
      //       // }

      //       let dungeonEncounter: EncounterDocument = new EncounterDocument(encounterDb.doc().id, spawnedEnemies, combatants, combatantList, Math.random(), expireDate, callerCharacterUid, maxCombatants, watchersList, isFull, characterData.characterName, ENCOUNTER_CONTEXT.DUNGEON, characterData.position, 1, "Enemies in sight!\n", "0", myPartyData.uid, [], [], [], [], 0, [], false, 5);

      //       //pridam pripadne vsechny party membery do encounteru jako watchery
      //       myPartyData.partyMembersUidList.forEach(partyMemberUid => {
      //         if (!dungeonEncounter!.watchersList.includes(partyMemberUid))
      //           dungeonEncounter!.watchersList.push(partyMemberUid);
      //       });

      //       t.set(encounterDb.doc(dungeonEncounter.uid), JSON.parse(JSON.stringify(dungeonEncounter)), { merge: true });
      //     }
      //   }
      // }
      //else { //jen pokud ta PoI neni jeste explored, vytvorime rovnou enemy
      // if (destinationPointOfInterestData.pointOfInterestType == POI_TYPE.ENCOUNTER || destinationPointOfInterestData.pointOfInterestType == POI_TYPE.TOWN) {
      // console.log("characterData.hasExploredPosition(destinationPointOfInterestData.worldPosition) : " + characterData.hasExploredPosition(destinationPointOfInterestData.worldPosition));
      //   console.log("characterData.include : " + characterData.exploredPositions.includes(destinationPointOfInterestData.worldPosition));
      //onsole.log("destinationPointOfInterestData.worldPosition:" + destinationPointOfInterestData.id);
      // console.log("destinationPointOfInterestData.worldPosition:" + destinationPointOfInterestData.id);


      //Pokud je PoI CHAPEL a nema hrac pro ni zadne info, vytvorim jej
      if (destinationPointOfInterestData.specials.includes(POI_SPECIALS.CHAPEL)) {
        let chapelInfo = characterData.getChapelInfo(destinationPointOfInterestData.worldPosition);

        //navstivil si tuto lokaci poprve, nema jeste chapel info, vytvorime ji a vygenerujem nahodny bless a ulozime ti do charakteru
        if (chapelInfo == null) {

          const PointOfInterestServerDataDb = destinationPointOfInterestDb.collection("definitions").doc("SERVER_DATA").withConverter(PointOfInterestServerDataDefinitionsConverter);
          const PointOfInterestServerDataDoc = await t.get(PointOfInterestServerDataDb);
          let PointOfInterestServerDataData: PointOfInterestServerDataDefinitions = PointOfInterestServerDataDoc.data();


          //vyfiltrujem blessy ktere jsou jiz vygenerovane v jinych chapels
          PointOfInterestServerDataData.blesses = PointOfInterestServerDataData.blesses.filter(element => {
            return !characterData.chapelInfo.some(info => info.blessId == element.id);
          });

          //rollnu random bless v chapel pokud je to chapel
          let chapelBless: string | null = null;

          let rollResult = rollForRandomItem(PointOfInterestServerDataData.blesses);
          if (rollResult != null)
            chapelBless = (rollResult as IdWithChance).id;
          else
            throw "could not find aby blesses too chooe from for this PoI : " + destinationPointOfInterestData.worldPosition;

          characterData.addChapelInfo(destinationPointOfInterestData.worldPosition, chapelBless);

        }
      }



      if (!characterData.hasExploredPosition(destinationPointOfInterestData.worldPosition)) { //pokud neni jeste prozkoumana

        if (characterData.stats.level < destinationPointOfInterestData.floorNumber)
          characterData.addFatigue(HIGH_LEVEL_POI_FATIGUE_PENALTY);
        // else {

        characterData.exploredPositions.push(destinationPointOfInterestData.worldPosition);
        characterData.updateMemmoryMap(destinationPointOfInterestData);
        //}
      }
      else //pokud je prozkoumana, jen updatnem MemmoryMap
      {
        //updatneme memmoryMapu
        characterData.updateMemmoryMap(destinationPointOfInterestData);
      }
      //rollnem si jestli nahodou nebude ambush
      /*   let delta = destinationPointOfInterestData.floorNumber - characterData.stats.level;
        let roll = randomIntFromInterval(0, 3); //pro char level 1 je to : floor 2 = 25%, floor 3 = 50%, floor 4 = 75% floor 5+=100%
        let isAmbushed = (roll < delta);
        console.log("delta:" + delta + "roll:" + roll);
        if (!isAmbushed) {
          characterData.exploredPositions.push(destinationPointOfInterestData.worldPosition);
          characterData.updateMemmoryMap(destinationPointOfInterestData);
        }
        else {
          characterData.addFatigue(10);
          //ziskam z db staty o enemy v teto loakci
 
          /*  const destinationLocationMetadataEnemyStatsDoc = await t.get(destinationLocationMetadataEnemyStatsDb.withConverter(EnemyDefinitionsConverter));
            let destinationLocationMetadataEnemyStatsData: EnemyDefinitions = destinationLocationMetadataEnemyStatsDoc.data();
  
            console.log("You were ambushed!");
            let spawnedEnemies: CombatEnemy[] = [];
  
            const ambushGroups = [
              ["BOAR", "WOLF", "BOAR"], //floor1
              ["BOAR", "WOLF", "BOAR"],
              ["BANDIT", "WOLF", "BANDIT"],
              ["BANDIT", "WOLF", "BANDIT"],
              ["SCORPID_SMALL", "SCORPID_GRAY", "SCORPID_SMALL"],
              ["SCORPID_SMALL", "SCORPID_GRAY", "SCORPID_SMALL"],
              ["SCORPID_GRAY", "IMP_VILE", "SCORPID_GRAY"],
              ["SCORPID_GRAY", "IMP_VILE", "SCORPID_GRAY"],
              ["IMP_VILE", "IMP_VILE", "IMP_VILE"],
              ["IMP_VILE", "IMP_VILE", "IMP_VILE"],
              ["IMP_VILE", "SATYR", "IMP_VILE"],
              ["IMP_VILE", "SATYR", "IMP_VILE"],
              ["IMP_VILE", "SATYR", "IMP_VILE"],
              ["IMP_VILE", "SATYR", "IMP_VILE"],
              ["IMP_VILE", "SATYR", "IMP_VILE"],
              ["IMP_VILE", "SATYR", "IMP_VILE"],
              ["IMP_VILE", "SATYR", "IMP_VILE"],
              ["IMP_VILE", "SATYR", "IMP_VILE"],
              ["IMP_VILE", "SATYR", "IMP_VILE"],
              ["IMP_VILE", "SATYR", "IMP_VILE"],
              ["IMP_VILE", "SATYR", "IMP_VILE"],
              ["IMP_VILE", "SATYR", "IMP_VILE"],
              ["IMP_VILE", "SATYR", "IMP_VILE"]
  
            ]
  
  
            let ambushGroupIndex = Math.min(ambushGroups.length - 1, destinationPointOfInterestData.floorNumber - 1);
  
            ambushGroups[ambushGroupIndex].forEach(enemyId => {
              let enemyStats = destinationLocationMetadataEnemyStatsData.getEnemyById(enemyId);
              let enemyHealth = randomIntFromInterval(enemyStats.healthMin, enemyStats.healthMax);
              let newEnemy = new CombatEnemy(firestoreAutoId(), enemyId, new CombatStats(0, 0, enemyHealth, enemyHealth, enemyHealth, 0, 0, 0, 5, 0, 0, 0, 0, [], 0, [], 0), enemyStats.level, enemyStats.mLevel, enemyStats.moveSet, enemyStats.isRare, "", [], [], 0, new EnemyDefinitionMoveSetSkill([], 0, "", "", false, false, false, false, false, false), 0);
              newEnemy.nextSkill = newEnemy.chooseSkillToCast(0);
              spawnedEnemies.push(newEnemy);
            });
  
            var combatants: CombatMember[] = [];//combatants.push(new CombatMember( characterData.uid, characterData.characterName, characterData.uid, characterData.characterClass, [], characterData.converEquipToCombatSkills(), [], characterData.converRareEffectsAndSkillBonusEffectsToCombatStats(), 0, false, characterData.stats.level, 0, [], characterData.characterPortrait, [], 0, 0));
            var combatantList: string[] = []; //combatantList.push(callerCharacterUid);
            var watchersList: string[] = []; watchersList.push(callerCharacterUid);
            const expireDate = getCurrentDateTime(2);
            var maxCombatants: number = 1;
            var isFull: boolean = false;
  
            let encounter: EncounterDocument = new EncounterDocument(encounterDb.doc().id, spawnedEnemies, combatants, combatantList, Math.random(), expireDate, callerCharacterUid, maxCombatants, watchersList, isFull, characterData.characterName, ENCOUNTER_CONTEXT.PERSONAL, characterData.position, 1, "Enemies in sight!\n", "0", callerCharacterUid, [], [], 0, [], [], 5, [], [], destinationPointOfInterestData.typeId);
  
            await joinCharacterToEncounter(t, encounter, characterData);
  
  
            t.set(encounterDb.doc(encounter.uid), JSON.parse(JSON.stringify(encounter)), { merge: true });
           

    }
*/




      //  console.log("!characterData.exploredPositions.includes(destinationPointOfInterestData.worldPosition) : " + !characterData.hasExploredPosition(destinationPointOfInterestData.worldPosition));

      //ziskam z db staty o enemy v teto loakci
      // const destinationLocationMetadataEnemyStatsDoc = await t.get(destinationLocationMetadataEnemyStatsDb.withConverter(EnemyDefinitionsConverter));
      //   let destinationLocationMetadataEnemyStatsData: EnemyDefinitions = destinationLocationMetadataEnemyStatsDoc.data();


      // let spawnedEnemies: CombatEnemy[] = [];

      // let choosenEnemy = rollForRandomItem(destinationPointOfInterestMetadataData.enemies, false) as IdWithChance;
      // let enemyStats = destinationLocationMetadataEnemyStatsData.getEnemyById(choosenEnemy.id);
      // let enemyHealth = randomIntFromInterval(enemyStats.healthMin, enemyStats.healthMax);
      // let newEnemy = new CombatEnemy(firestoreAutoId(), choosenEnemy.id, new CombatStats(0, 0, enemyHealth, enemyHealth, 0, 0, 0, 0, 0, 0, 0, 0, 0), enemyStats.level, enemyStats.mLevel, enemyStats.moveSet, enemyStats.isRare, "", [], [], 0, new EnemyDefinitionMoveSetSkill([], 0, "", ""), 0);
      // newEnemy.nextSkill = newEnemy.chooseSkillToCast(0);
      // spawnedEnemies.push(newEnemy);
      //vytvorime rovnou enemy groupu (v dungeonu se nebere nahodny ale vsechny a gnorujem uplne nejaky chanceToSpawn)


      // if (destinationPointOfInterestData.enemies.length > 0) //poku jsou v POI definovani nejaci enemy, vytvorime hned encounter a v nem vsechny enemy, jinak nic
      // {
      //   console.log("jsou tu enemy");
      //   let spawnedEnemies: CombatEnemy[] = [];
      //   destinationPointOfInterestData.enemies.forEach(enemy => {
      //     let enemyStats = destinationLocationMetadataEnemyStatsData.getEnemyById(enemy.id);
      //     let enemyHealth = randomIntFromInterval(enemyStats.healthMin, enemyStats.healthMax);
      //     let newEnemy = new CombatEnemy(firestoreAutoId(), enemy.id, new CombatStats(0, 0, enemyHealth, enemyHealth, enemyHealth, 0, 0, 0, 5, 0, 0, 0, 0, [], 0, [], 0), enemyStats.level, enemyStats.mLevel, enemyStats.moveSet, enemyStats.isRare, "", [], [], 0, new EnemyDefinitionMoveSetSkill([], 0, "", "", false, false, false, false, false, false), 0);
      //     newEnemy.nextSkill = newEnemy.chooseSkillToCast(0);
      //     spawnedEnemies.push(newEnemy);
      //   });

      //   var combatants: CombatMember[] = [];//combatants.push(new CombatMember( characterData.uid, characterData.characterName, characterData.uid, characterData.characterClass, [], characterData.converEquipToCombatSkills(), [], characterData.converRareEffectsAndSkillBonusEffectsToCombatStats(), 0, false, characterData.stats.level, 0, [], characterData.characterPortrait, [], 0, 0));
      //   var combatantList: string[] = []; //combatantList.push(callerCharacterUid);
      //   var watchersList: string[] = []; watchersList.push(callerCharacterUid);
      //   const expireDate = getCurrentDateTime(2);
      //   var maxCombatants: number = 5;
      //   var isFull: boolean = false;

      //   let encounter: EncounterDocument = new EncounterDocument(encounterDb.doc().id, spawnedEnemies, combatants, combatantList, Math.random(), expireDate, callerCharacterUid, maxCombatants, watchersList, isFull, characterData.characterName, ENCOUNTER_CONTEXT.PERSONAL, characterData.position, 1, "Enemies in sight!\n", "0", callerCharacterUid, [], [], 0, [], false, 5);

      //   await joinCharacterToEncounter(t, encounter, characterData);

      //   if (myPartyData != undefined) {
      //     //pridam pripadne vsechny party membery do encounteru jako watchery
      //     myPartyData.partyMembersUidList.forEach(partyMemberUid => {
      //       if (!encounter!.watchersList.includes(partyMemberUid))
      //         encounter!.watchersList.push(partyMemberUid);
      //     });
      //   }
      //   t.set(encounterDb.doc(encounter.uid), JSON.parse(JSON.stringify(encounter)), { merge: true });
      // }
      // }





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



  const callerCharacterUid = data.characterUid;
  const destinationLocationId = data.locationId;

  console.log("callerCharacterUid: " + callerCharacterUid);
  console.log("destinationLocationId: " + destinationLocationId);

  const characterDb = await admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);
  const encountersDb = admin.firestore().collection('encounters');
  const partiesDb = admin.firestore().collection('parties')
  const myPartyDb = partiesDb.where("partyMembersUidList", "array-contains", callerCharacterUid);
  const worldMapDb = admin.firestore().collection('_metadata_zones').doc("DUNOTAR").withConverter(DijkstraConverter);
  //Tento querry VYZADUJE COMPOSITE INDEX , byl vytvoren v FIRESTORE!!!
  //const callerPersonalEncounterWithoutCombatants = encountersDb.where("foundByCharacterUid", "==", callerCharacterUid).where("encounterContext", "==", ENCOUNTER_CONTEXT.PERSONAL).where("combatantList", "==", []).withConverter(encounterDocumentConverter);
  const callerPersonalEncounterWithoutCombatants = encountersDb.where("foundByCharacterUid", "==", callerCharacterUid).where("combatantList", "==", []).withConverter(encounterDocumentConverter);

  try {

    const result = await admin.firestore().runTransaction(async (t: any) => {

      checkForServerVersion(data);

      const characterDoc = await t.get(characterDb);
      let characterData: CharacterDocument = characterDoc.data();

      //TODO: treba toto pridava read pokazde kdyz hrac cestuje, melo by to resit hlavne UI at to zamezi, takze tady ten check jen OBCAS a pripadne logovat cheatery
      //nebo ID na personal encounter davat rovnou do charakteru pokud chci stejne mit jen jeden svuj osobni....to by taky usetrilo tyhle ready a jen bych se mrkl eslu char ma nejaky personal enc...
      if (await QuerryIfCharacterIsWatcherInAnyEncounterOnHisPosition(t, characterData))
        throw ("Enemies are nearby, you cant travel!");

      if (await QuerryHasCharacterAnyUnclaimedEncounterResult(t, characterData))
        throw ("You must loot all corpses before travel!");


      // const destinationLocationMetadataDb = admin.firestore().collection('_metadata_zones').doc(characterData.position.zoneId).collection("locations").doc(destinationLocationId);

      // const destinationLocationMetadataDoc = await t.get(destinationLocationMetadataDb);
      // let destinationLocationMetadataData: MapLocation = destinationLocationMetadataDoc.data();


      //TODO:  nemel by byt problem moc profiltrovat world map dijkstru podle toho c characetdocument ma explored a nema
      //const resultTravel = getWorldMap().findShortestWay(characterData.position.locationId, destinationLocationId);

      const worldMapDoc = await t.get(worldMapDb);
      let worldMapData: Dijkstra = worldMapDoc.data();
      const resultTravel = worldMapData.findShortestPath(characterData.position.locationId, destinationLocationId);

      // printWorldMap(worldMapData);

      console.log("Total travel time from :" + characterData.position.locationId + " to : " + destinationLocationId + " is : " + resultTravel.resultPathWeigth);
      console.log("Nodes on path are: ");
      resultTravel.nodesOnPath.forEach(node => { console.log(node); });


      //nemam travel pointy ale mam dost casu na suplement
      // if (characterData.currency.travelPoints < resultTravel.resultPathWeigth && characterData.currency.time >= resultTravel.resultPathWeigth * TIME_PER_TRAVEL_POINT) {
      //   characterData.subCurrency(CURRENCY_ID.TIME, resultTravel.resultPathWeigth * TIME_PER_TRAVEL_POINT);
      //   characterData.addCurrency(CURRENCY_ID.TRAVEL_POINTS, resultTravel.resultPathWeigth);
      // }

      if (characterData.currency.travelPoints >= resultTravel.resultPathWeigth) {
        characterData.subCurrency(CURRENCY_ID.TRAVEL_POINTS, resultTravel.resultPathWeigth);
        //Nastavime novou pozici 
        characterData.position.locationId = destinationLocationId;
        characterData.position.pointOfInterestId = getStartingPointOfInterestForLocation(destinationLocationId);
      }
      else
        throw ("Not enough Time to travel! You have :" + characterData.currency.time + " but the travel needs : " + resultTravel.resultPathWeigth);


      //Kdyz si v boji nemuzes cestovat
      if (await QuerryIfCharacterIsInCombatAtAnyEncounter(t, characterData.uid))
        throw ("You cannot travel while in combat!");

      //Poku si v lokaci poprve a prave jsi ji prozkoumal ulozime ze si ji prozkoumal a jeji startovni pozici taky.
      if (!characterData.hasExploredPosition(destinationLocationId)) {
        let worldPosition: WorldPosition = new WorldPosition("DUNOTAR", destinationLocationId, getStartingPointOfInterestForLocation(destinationLocationId));
        characterData.exploredPositions.push(worldPosition);

        //   if (destinationLocationMetadataData.locationType != LOC_TYPE.DUNGEON)//..pokud  to neni dungeon tam se startovni pozice prozkovama v parte pri enter dungeonu
        // {
        // characterData.exploredPositions.pointsOfInterest.push(getStartingPointOfInterestForLocation(destinationLocationId));
        //  }
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
                if (element.uid == callerCharacterUid) {
                  element.position.locationId = destinationLocationId;
                  element.position.pointOfInterestId = characterData.position.pointOfInterestId; //point of interest taky protoze po ceste do nove lokace si na startovnim poi
                }
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