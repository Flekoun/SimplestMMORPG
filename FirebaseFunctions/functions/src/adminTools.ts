
// [START import]
import * as functions from "firebase-functions";
import { ContentContainer, SATOSHIUM_LEADERBOARDS_COEFICIENT, WorldPosition, generateContentContainer, getCurrentDateTimeInMillis, rollForRandomItem } from ".";
import { DropTablesData, TierMonstersDefinition, TierDungeonDefinition } from "./encounter";
import { Dijkstra, IdWithChance, LocationConverter, MapLocation, PointOfInterest, PointOfInterestConverter, Coordinates2DCartesian, PointOfInterestServerDataDefinitions, DungeonDefinitionPublic, DungeonDefinitionServerOnly, MonstersDefinitionServerOnly, MonstersDefinitionPublic } from "./worldMap";
import { PerkOfferDefinition } from "./perks";
import { Questgiver, RandomEquip, RewardClassSpecific } from "./questgiver";
import { Vendor } from "./vendor";
import { IHasChanceToSpawn, ITEMS, ItemIdWithAmount, generateContent } from "./equip";
import { LeaderboardBaseData, LeaderboardReward } from "./leaderboards";

const admin = require('firebase-admin');
// // [END import]

enum ROOM_TYPE { NONE, MONSTER_SOLO, MONSTER_COOP, REST, TREASURE, TOWN, MERCHANT, MONSTER_ELITE, ENDGAME, START, QUEST, CHAPEL, DUNGEON }

export interface InternalDefinition {
  START: PointOfInterestInternalDefinition[];
  MONSTER_SOLO: PointOfInterestInternalDefinition[];
  TOWN: PointOfInterestInternalDefinition[];
  MERCHANT: PointOfInterestInternalDefinition[];
  QUEST: PointOfInterestInternalDefinition[];
  CHAPEL: PointOfInterestInternalDefinition[];
  TREASURE: PointOfInterestInternalDefinition[];
  DUNGEON: PointOfInterestInternalDefinition[];
  ENDGAME: PointOfInterestInternalDefinition[];
  OTHER_DATA: InternalDefinitionOtherData;
}

export interface InternalDefinitionOtherData {
  perksOffersRare: PerksOffersRareData[];
}

export interface PerksOffersRareData {
  id: string;
  perks: PerkOfferDefinition[];
}

export class DungeonDefinition {
  constructor(

    public partySize: number,
    public entryPrice: number,
    public tiers: TierDungeonDefinition[],
    public rewards: ContentContainer[], //tohle je presny content. Tedy equip nebo i bezny item s presne definovanyma vecma jako price a stackSize atd...defakto nevyuzivane..presnou kopii dostanes do inventare
    public rewardsRandomEquip: RandomEquip[], // tohle je nahodny equip....co to bude presne se dogeneruje
    public rewardsGenerated: ItemIdWithAmount[], //tohle jsou bezne itemy, kde vim ID a dogeneruje se u nich ty blbosti, stcksize a u jabka a lahvi kolik leci atd nez se pridaji do inventare a stane se z nich content container
    public isEndlessDungeon: boolean,
    public isFinalDungeon: boolean,
    public characterLevelMax: number,
    public characterLevelMin: number

  ) { }
}


export class MonstersDefinition {
  constructor(

    public partySize: number,
    public exploreTimePrice: number,
    public tiers: TierMonstersDefinition[], //Momentalne se jne 1. zaznam pouziva na definici jaky enemy bude na POI a jaky perk reward bude mit leaderboard jeho
    public perkOffersRareId: string


  ) { }
}



export class PointOfInterestInternalDefinition implements IHasChanceToSpawn {
  constructor(
    public chanceToSpawn: number,
    public enemies: IdWithChance[],
    //public exploreTimePrice: number,
    public id: string,
    //  public perkOffersRareId: string,//PerkOfferDefinition[],
    public questgivers: Questgiver[],
    //public rareEnemies: RareEnemyTierDefinition[],
    public specials: string[],
    // public tiers: TierMonstersDefinition[],
    public dungeon: DungeonDefinition | null,
    public monsters: MonstersDefinition | null,
    //public trainers: Trainer[],
    public vendors: Vendor[],
    public floorMin: number,
    public floorMax: number,
    public chapelBless: IdWithChance[]

  ) { }
}

export class positionwithId {
  constructor(
    public uid: string,
    public characterRecipientUid: string,
    public content: ContentContainer,
    public messageTitle: string,
    public messageBody: string,

    public expireDate: string

  ) { }
}


exports.savePointOfInterestScreenPositionsForLocation = functions.https.onCall(async (data, context) => {

  const positions: Coordinates2DCartesian[] = JSON.parse(data.positions);
  const ids: string[] = JSON.parse(data.poiIds);
  // data["positions"] = positions;
  // data["poiIds"] = ids;
  const locationId = data.locationId;

  const locationDb = await admin.firestore().collection('_metadata_zones').doc("DUNOTAR").collection("locations").doc(locationId).withConverter(LocationConverter);

  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {

      const locationDoc = await t.get(locationDb);
      let locationData: MapLocation = locationDoc.data();

      console.log("positions " + positions);
      console.log("ids " + ids);


      for (let i = 0; i < locationData.dijkstraMap.exportMap.length; i++) {

        for (let j = 0; j < positions.length; j++) {
          if (ids[j] == locationData.dijkstraMap.exportMap[i].id) {
            locationData.dijkstraMap.exportMap[i].screenPosition.x = positions[j].x;
            locationData.dijkstraMap.exportMap[i].screenPosition.y = positions[j].y;
            console.log("Updated screen position of " + locationData.dijkstraMap.exportMap[i].id + " point of interest at " + locationId + " location to " + "x:" + positions[j].x + "y:" + positions[j].y);

          }
        }
      }


      t.set(locationDb, JSON.parse(JSON.stringify(locationData)), { merge: true });

      return "OK";
    });


    console.log('Transaction success', result);
    return result;
  } catch (e) {
    console.log('Transaction failure:', e);
    throw new functions.https.HttpsError("aborted", "Error : " + e);
  }


});


exports.saveDropTablesEnemy = functions.https.onCall(async (data, context) => {
  //const jsonString = data.dropTables

  const enemyDropTablesData: DropTablesData = JSON.parse(data.dropTables);
  const locationId = data.locationId;
  const zoneId = data.zoneId;

  const dropTablesDb = admin.firestore().collection('_metadata_dropTables').doc(zoneId).collection("locations").doc(locationId);

  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {

      t.set(dropTablesDb, enemyDropTablesData, { merge: true });

      return "OK";
    });


    console.log('Transaction success', result);
    return result;
  } catch (e) {
    console.log('Transaction failure:', e);
    throw new functions.https.HttpsError("aborted", "Error : " + e);
  }


});



exports.saveTiers = functions.https.onCall(async (data, context) => {
  //const jsonString = data.dropTables

  const pointOfInterestServerData: PointOfInterestServerDataDefinitions = JSON.parse(data.tiers);
  const pointOfInterestId = data.pointOfInterestId;
  const locationId = data.locationId;
  const zoneId = data.zoneId;


  const pointOfInterestDb = admin.firestore().collection('_metadata_zones').doc(zoneId).collection("locations").doc(locationId).collection("pointsOfInterest").doc(pointOfInterestId).withConverter(PointOfInterestConverter);
  const pointOfInterestServerDataDb = admin.firestore().collection('_metadata_zones').doc(zoneId).collection("locations").doc(locationId).collection("pointsOfInterest").doc(pointOfInterestId).collection("definitions").doc("SERVER_DATA");

  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {

      //updatnu tiersCount a character level 
      const pointOfInterestDoc = await t.get(pointOfInterestDb);
      let pointOfInterestData: PointOfInterest = pointOfInterestDoc.data();
      pointOfInterestData.monsters!.tiersTotal = pointOfInterestServerData.monsters!.tiers.length;

      // let minEnemyLevel: number = 0;
      // let maxEnemyLevel: number = 0;

      // tiersData.tiers.forEach(tier => {
      //   tier.enemies.forEach(enemy => {
      //     if (enemy)
      //   });
      // });

      //ulozim server data do definic
      t.set(pointOfInterestServerDataDb, pointOfInterestServerData, { merge: true });

      t.set(pointOfInterestDb, JSON.parse(JSON.stringify(pointOfInterestData)), { merge: true });

      return "OK";
    });


    console.log('Transaction success', result);
    return result;
  } catch (e) {
    console.log('Transaction failure:', e);
    throw new functions.https.HttpsError("aborted", "Error : " + e);
  }

});





exports.saveInternalDefinitionsMapGenerator = functions.https.onCall(async (data, context) => {
  //const jsonString = data.dropTables

  const internalData: InternalDefinition = JSON.parse(data.definition);


  const internalDb = admin.firestore().collection('_internal_definitions').doc("MAP_GENERATOR");


  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {


      //ulozim 
      t.set(internalDb, internalData, { merge: true });

      return "OK";
    });


    console.log('Transaction success', result);
    return result;
  } catch (e) {
    console.log('Transaction failure:', e);
    throw new functions.https.HttpsError("aborted", "Error : " + e);
  }


});



// exports.saveVendorGoods = functions.https.onCall(async (data, context) => {
//   //const jsonString = data.dropTables

//   const internalData: InternalDefinition = JSON.parse(data.definition);


//   const internalDefinitionsDb = admin.firestore().collection('_internal_definitions').doc("MAP_GENERATOR");

//   const internalDefinitionsDoc = await t.get(internalDefinitionsDb);
//   let internalDefinitionsData: InternalDefinition = internalDefinitionsDoc.data();


//   try {
//     const result = await admin.firestore().runTransaction(async (t: any) => {


//       //ulozim 
//       t.set(internalDb, internalData, { merge: true });

//       return "OK";
//     });


//     console.log('Transaction success', result);
//     return result;
//   } catch (e) {
//     console.log('Transaction failure:', e);
//     throw new functions.https.HttpsError("aborted", "Error : " + e);
//   }


// });



exports.generateLocationMap = functions.https.onCall(async (data, context) => {

  const newMap: Dijkstra = JSON.parse(data.dijkstra);

  // console.log(data.dijkstra);
  //console.log(newMap.exportMap[0].id);
  //console.log(newMap.exportMap[0].type);
  const locationId = data.locationId;
  const zoneId = data.zoneId;



  // const tiersOfChoosenPOIDb = admin.firestore().collection('_metadata_zones').doc(zoneId).collection("locations").doc(locationId).collection("pointsOfInterest").doc(pointOfInterestId).collection("definitions").doc("TIERS");

  // const pointOfInterestDb = admin.firestore().collection('_metadata_zones').doc(zoneId).collection("locations").doc(locationId).collection("pointsOfInterest").doc(pointOfInterestId).withConverter(PointOfInterestConverter);

  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {



      //updatnu mapu lokace
      const destinationLocationMetadataDb = admin.firestore().collection('_metadata_zones').doc(zoneId).collection("locations").doc(locationId);
      const destinationLocationMetadataDoc = await t.get(destinationLocationMetadataDb.withConverter(LocationConverter));
      let destinationLocationMetadataData: MapLocation = destinationLocationMetadataDoc.data();
      destinationLocationMetadataData.dijkstraMap = newMap;


      const internalDefinitionsDb = admin.firestore().collection('_internal_definitions').doc("MAP_GENERATOR");

      const internalDefinitionsDoc = await t.get(internalDefinitionsDb);
      let internalDefinitionsData: InternalDefinition = internalDefinitionsDoc.data();

      //console.log("internalDefinitionsData.: " + internalDefinitionsData.MONSTER_SOLO[0].perkOffersRare.length);
      const batch = admin.firestore().batch();

      // let maxPartySize = 0;//dat do db??? internal definic k ostatnimu?
      let QUEST_usedList: PointOfInterestInternalDefinition[] = [];
      let TOWN_usedList: PointOfInterestInternalDefinition[] = [];
      let MERCHANT_usedList: PointOfInterestInternalDefinition[] = [];
      let MONSTERSOLO_usedList: PointOfInterestInternalDefinition[] = [];
      let DUNGEON_usedList: PointOfInterestInternalDefinition[] = [];


      const leaderboardDb = admin.firestore().collection('leaderboards');//.withConverter(LeaderboardBaseDataConverter);







      //leaderbodum nastavim multiplier SATOHIUM a progeneruju rewardy 
      const leaderboardsQuerySnapshot = await t.get(leaderboardDb);

      leaderboardsQuerySnapshot.docs.forEach(doc => {
        let leaderboard: LeaderboardBaseData = doc.data();

        //aplikujem satoshium koeficient
        leaderboard.rewards.forEach(rewardInfo => {
          if (rewardInfo.generatedContent) {
            rewardInfo.content = [];
            rewardInfo.generatedContent.forEach(rewardContent => {

              let amount = rewardContent.amount;

              if (rewardContent.itemId == ITEMS.SATOSHIUM) {
                amount = Math.max(1, Math.round(SATOSHIUM_LEADERBOARDS_COEFICIENT * rewardContent.amount)); //1 je minimum, zaokrohlime nasobeni
              }
              rewardInfo.content?.push(generateContentContainer(generateContent(rewardContent.itemId, amount)));

            });
          }
        });

        console.log("fixju leaderboard: " + doc.id);
        t.set(leaderboardDb.doc(doc.id), JSON.parse(JSON.stringify(leaderboard)), { merge: true });
      });


      //vyplnim generated equip na konkretni podle definic
      internalDefinitionsData.MERCHANT.forEach(item => {

        item.vendors.forEach(vendor => {
          vendor.goods.forEach(good => {
            if (good.contentGenerated) {
              good.content = generateContentContainer(generateContent(good.contentGenerated.itemId, good.contentGenerated.amount));
              good.contentGenerated = undefined;
            }
          });
        });



        item.questgivers.forEach(questgiver => {
          if (questgiver.rewardsGenerated != null) {
            questgiver.rewardsGenerated.forEach(rewardGenerated => {
              questgiver.rewards.push(new RewardClassSpecific([], generateContentContainer(generateContent(rewardGenerated.itemId, rewardGenerated.amount))));

            });
          }
          questgiver.rewardsGenerated = undefined;

        });
      });

      internalDefinitionsData.START.forEach(item => {
        item.vendors.forEach(vendor => {
          vendor.goods.forEach(good => {
            if (good.contentGenerated) {
              good.content = generateContentContainer(generateContent(good.contentGenerated.itemId, good.contentGenerated.amount));
              good.contentGenerated = undefined;
            }
          });
        });

      });

      internalDefinitionsData.MONSTER_SOLO.forEach(item => {
        item.monsters?.tiers.forEach(tier => {
          tier.perkOffers.forEach(perkOffer => {
            perkOffer.rewardsGenerated.forEach(rewardGenerated => {
              perkOffer.rewards.push(generateContentContainer(generateContent(rewardGenerated.itemId, rewardGenerated.amount)));

            });
            perkOffer.rewardsGenerated = [];
          });
        });

      });


      internalDefinitionsData.DUNGEON.forEach(item => {
        item.dungeon?.rewardsGenerated.forEach(rewardGenerated => {
          item.dungeon?.rewards.push(generateContentContainer(generateContent(rewardGenerated.itemId, rewardGenerated.amount)));

        });
        item.dungeon!.rewardsGenerated = [];

      });


      internalDefinitionsData.OTHER_DATA.perksOffersRare.forEach(item => {

        item.perks.forEach(perk => {
          perk.rewardsGenerated.forEach(rewardGenerated => {
            perk.rewards.push(generateContentContainer(generateContent(rewardGenerated.itemId, rewardGenerated.amount)));

          });
          perk.rewardsGenerated = [];
        });


      });

      console.log("vertex count :" + newMap.exportMap.length);
      newMap.exportMap.forEach(vertex => {


        let internalDefinitionRef: PointOfInterestInternalDefinition = internalDefinitionsData.MONSTER_SOLO[0];

        // console.log("vertex.type :" + vertex.type + " vertex.id:" + vertex.id);

        switch (vertex.type) {

          case ROOM_TYPE.MONSTER_SOLO:
            {
              //  maxPartySize = 1;
              internalDefinitionRef = rollForRandomItem(filterFloors(internalDefinitionsData.MONSTER_SOLO, vertex.mapPosition.y)) as PointOfInterestInternalDefinition;
              MONSTERSOLO_usedList.push(internalDefinitionRef);

              break;
            }

          // case POI_TYPE.MONSTER_COOP:
          //   {
          //     maxPartySize = 2;
          //     internalDefinitionRef = rollForRandomItem(filterFloors(internalDefinitionsData.MONSTER_COOP, vertex.mapPosition.y)) as PointOfInterestInternalDefinition;
          //     break;
          //   }


          case ROOM_TYPE.TOWN:
            {
              internalDefinitionRef = rollForRandomItem(filterFloors(internalDefinitionsData.TOWN, vertex.mapPosition.y)) as PointOfInterestInternalDefinition;
              TOWN_usedList.push(internalDefinitionRef);
              break;
            }

          case ROOM_TYPE.MERCHANT:
            {
              internalDefinitionRef = rollForRandomItem(filterFloors(internalDefinitionsData.MERCHANT, vertex.mapPosition.y)) as PointOfInterestInternalDefinition;
              MERCHANT_usedList.push(internalDefinitionRef);


              break;
            }

          case ROOM_TYPE.QUEST:
            {
              internalDefinitionRef = rollForRandomItem(filterFloors(internalDefinitionsData.QUEST, vertex.mapPosition.y)) as PointOfInterestInternalDefinition;
              QUEST_usedList.push(internalDefinitionRef);
              break;
            }

          case ROOM_TYPE.CHAPEL:
            {
              ;
              internalDefinitionRef = rollForRandomItem(filterFloors(internalDefinitionsData.CHAPEL, vertex.mapPosition.y)) as PointOfInterestInternalDefinition;
              break;
            }

          case ROOM_TYPE.START:
            {
              internalDefinitionRef = rollForRandomItem(internalDefinitionsData.START) as PointOfInterestInternalDefinition;

              break;
            }

          case ROOM_TYPE.TREASURE:
            {
              internalDefinitionRef = rollForRandomItem(filterFloors(internalDefinitionsData.TREASURE, vertex.mapPosition.y)) as PointOfInterestInternalDefinition;
              break;

            }

          case ROOM_TYPE.DUNGEON:
            {
              internalDefinitionRef = rollForRandomItem(filterFloors(internalDefinitionsData.DUNGEON, vertex.mapPosition.y)) as PointOfInterestInternalDefinition;
              DUNGEON_usedList.push(internalDefinitionRef);
              break;

            }

          case ROOM_TYPE.ENDGAME:
            {
              internalDefinitionRef = rollForRandomItem(filterFloors(internalDefinitionsData.ENDGAME, vertex.mapPosition.y)) as PointOfInterestInternalDefinition;
              break;

            }

          default:
            break;

        }



        //vyfiltruju kde necchi udelovat duplicitni PoI
        internalDefinitionsData.QUEST = internalDefinitionsData.QUEST.filter(element => !QUEST_usedList.includes(element));
        internalDefinitionsData.MERCHANT = internalDefinitionsData.MERCHANT.filter(element => !MERCHANT_usedList.includes(element));
        internalDefinitionsData.MONSTER_SOLO = internalDefinitionsData.MONSTER_SOLO.filter(element => !MONSTERSOLO_usedList.includes(element));
        internalDefinitionsData.DUNGEON = internalDefinitionsData.DUNGEON.filter(element => !DUNGEON_usedList.includes(element));
        internalDefinitionsData.TOWN = internalDefinitionsData.TOWN.filter(element => !TOWN_usedList.includes(element));

        // //rollnu random bless v chapel pokd je to chapel
        // let chapelBless: string | null = null;

        // let rollResult = rollForRandomItem(internalDefinitionRef.chapelBless);
        // if (rollResult != null)
        //   chapelBless = (rollResult as IdWithChance).id;

        if (internalDefinitionRef == null)
          throw "Nenalezen zadny vhodny PoI pro " + (vertex.type) + " floor : " + vertex.mapPosition.y;

        let dungeonPublic: DungeonDefinitionPublic | null = null;
        let dungeonServer: DungeonDefinitionServerOnly | null = null;

        if (internalDefinitionRef.dungeon != null) {
          dungeonPublic = new DungeonDefinitionPublic(internalDefinitionRef.dungeon.partySize, internalDefinitionRef.dungeon.entryPrice, internalDefinitionRef.dungeon.rewards, internalDefinitionRef.dungeon.rewardsRandomEquip, internalDefinitionRef.dungeon.rewardsGenerated, internalDefinitionRef.dungeon.isEndlessDungeon, internalDefinitionRef.dungeon.isFinalDungeon, internalDefinitionRef.dungeon.characterLevelMax, internalDefinitionRef.dungeon.characterLevelMin, internalDefinitionRef.dungeon.tiers.length);
          dungeonServer = new DungeonDefinitionServerOnly(internalDefinitionRef.dungeon.tiers);
        }

        let monstersPublic: MonstersDefinitionPublic | null = null;
        let monstersServer: MonstersDefinitionServerOnly | null = null;

        if (internalDefinitionRef.monsters != null) {
          monstersPublic = new MonstersDefinitionPublic(internalDefinitionRef.monsters.partySize, internalDefinitionRef.monsters.exploreTimePrice, internalDefinitionRef.monsters.tiers.length);
          monstersServer = new MonstersDefinitionServerOnly(internalDefinitionRef.monsters.tiers, internalDefinitionRef.monsters.perkOffersRareId);
        }

        // let tierCout = internalDefinitionRef.tiers.length;

        let newPointOfInterest: PointOfInterest = new PointOfInterest(vertex.id, internalDefinitionRef.enemies, vertex.type, internalDefinitionRef.questgivers, internalDefinitionRef.vendors, internalDefinitionRef.specials, new WorldPosition(zoneId, locationId, vertex.id), internalDefinitionRef.id, vertex.mapPosition.y, dungeonPublic, monstersPublic, ROOM_TYPE[vertex.type]);
        const locationPointsOfInterestDb = admin.firestore().collection('_metadata_zones').doc(zoneId).collection("locations").doc(locationId).collection("pointsOfInterest").doc(newPointOfInterest.id);
        batch.set(locationPointsOfInterestDb, JSON.parse(JSON.stringify(newPointOfInterest)), { merge: true });

        const pointOfInterestServerDataDocRef = locationPointsOfInterestDb.collection("definitions").doc("SERVER_DATA");
        // //ziskame rareperky podleId
        // let perksOfferRareDef: PerkOfferDefinition[] | undefined;
        // perksOfferRareDef = internalDefinitionsData.OTHER_DATA.perksOffersRare.find(perkOfferrs => perkOfferrs.id == internalDefinitionRef.perkOffersRareId)?.perks;

        // if (!perksOfferRareDef)
        //   perksOfferRareDef = [];
        // else {
        //   perksOfferRareDef.forEach(perk => {
        //     //tady rucne ten rare perkum doplnim rareID, ktere zpetne pro kazdy perk odkazuje na to z ktere rarePerkId groupy se vzal...pak to pouzivam dal kdyz claimujes perky tak zse chci dohledat tu "materskou" definici
        //     //rare perku a zvednou claimed number, coz mi dost pomaha kdyz vim ne jen Uid perku ale i ke ktere groupe patril, navic vim ze ten samotny perk je rare perk (nese si ted to info v sobe)....
        //     //nicmene toto bych mohl fillovat i v UI, dkyz vytvarim ty rare perky....ale delam to ted tady no...
        //     perk.rarePerkGroupId = internalDefinitionRef.perkOffersRareId;
        //   });
        // }

        //pridame definice server dat
        let def: PointOfInterestServerDataDefinitions = new PointOfInterestServerDataDefinitions(internalDefinitionRef.chapelBless, dungeonServer, monstersServer);
        batch.set(pointOfInterestServerDataDocRef, JSON.parse(JSON.stringify(def)), { merge: true });



        //prekopireujem rarePerkOffery do lokace
        destinationLocationMetadataData.perksRareOffers = internalDefinitionsData.OTHER_DATA.perksOffersRare;

        //update  lokace
        //  batch2.set(destinationLocationMetadataDb, JSON.parse(JSON.stringify(destinationLocationMetadataData)), { merge: false });
        t.set(destinationLocationMetadataDb, JSON.parse(JSON.stringify(destinationLocationMetadataData)), { merge: false }); //merge false chceme vsechno prepsat novym 

      });
      console
        .log("JDiu na to");
      //vytvorim leaderboardy pro vsechny POI
      await MONSTERSOLO_usedList.forEach(async monsterSoloEnrty => {
        console.log("ahah:" + monsterSoloEnrty.id);
        let leaderbardReward = new LeaderboardReward(1, 1, undefined, undefined, undefined, monsterSoloEnrty.monsters?.tiers[0].perkOffers);
        let leaderboardEntry = new LeaderboardBaseData([leaderbardReward], "DAY", getCurrentDateTimeInMillis(24).toString(), "KILLS_ENCOUNTER");
        await t.set(leaderboardDb.doc(monsterSoloEnrty.id), JSON.parse(JSON.stringify(leaderboardEntry)), { merge: true });
      });
      console.log("Comituju");

      await batch.commit();

      return "OK";
    });


    console.log('Transaction success', result);
    return result;
  } catch (e) {
    console.log('Transaction failure:', e);
    throw new functions.https.HttpsError("aborted", "Error : " + e);
  }


});

function filterFloors(floorList: PointOfInterestInternalDefinition[], y: number): PointOfInterestInternalDefinition[] {

  let result: PointOfInterestInternalDefinition[] = [];

  for (let i = floorList.length - 1; i >= 0; i--) {

    let maxFloorCheckFailed: boolean = false;
    if (floorList[i].floorMax == -1) {
      maxFloorCheckFailed = false;
    }
    else if (floorList[i].floorMax < y)
      maxFloorCheckFailed = true;


    if (floorList[i].floorMin > y || maxFloorCheckFailed) {
      // console.log("filtruju pryc :" + floorList[i].id + "y:" + y + " floorList[i].floorMin:" + floorList[i].floorMin + "floorList[i].floorMax: " + floorList[i].floorMax);
      // floorList.splice(i, 1);
    }
    else {
      result.push(floorList[i]);
    }
  }
  return result;
}


exports.startNewSeason = functions.https.onCall(async (data, context) => {
  //const jsonString = data.dropTables

  const pointOfInterestServerData: PointOfInterestServerDataDefinitions = JSON.parse(data.tiers);
  const pointOfInterestId = data.pointOfInterestId;
  const locationId = data.locationId;
  const zoneId = data.zoneId;


  const pointOfInterestDb = admin.firestore().collection('_metadata_zones').doc(zoneId).collection("locations").doc(locationId).collection("pointsOfInterest").doc(pointOfInterestId).withConverter(PointOfInterestConverter);
  const pointOfInterestServerDataDb = admin.firestore().collection('_metadata_zones').doc(zoneId).collection("locations").doc(locationId).collection("pointsOfInterest").doc(pointOfInterestId).collection("definitions").doc("SERVER_DATA");

  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {

      //updatnu tiersCount a character level 
      const pointOfInterestDoc = await t.get(pointOfInterestDb);
      let pointOfInterestData: PointOfInterest = pointOfInterestDoc.data();
      pointOfInterestData.monsters!.tiersTotal = pointOfInterestServerData.monsters!.tiers.length;

      // let minEnemyLevel: number = 0;
      // let maxEnemyLevel: number = 0;

      // tiersData.tiers.forEach(tier => {
      //   tier.enemies.forEach(enemy => {
      //     if (enemy)
      //   });
      // });

      //ulozim server data do definic
      t.set(pointOfInterestServerDataDb, pointOfInterestServerData, { merge: true });

      t.set(pointOfInterestDb, JSON.parse(JSON.stringify(pointOfInterestData)), { merge: true });

      return "OK";
    });


    console.log('Transaction success', result);
    return result;
  } catch (e) {
    console.log('Transaction failure:', e);
    throw new functions.https.HttpsError("aborted", "Error : " + e);
  }


});

