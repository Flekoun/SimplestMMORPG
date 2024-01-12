


import * as functions from "firebase-functions";
import { Equip, EQUIP_SLOT_ID, RARITY, Content, ITEMS, RARE_EFFECT, generateContent, ItemIdWithAmount, IHasChanceToSpawn, SkillBonusEffect, QuerryForSkillDefinitions, generateEquip, BuffBonusEffect, FOOD_EFFECT, generateDropFromDropTable } from "./equip";
import { firestoreAutoId } from "./general2";
//import * as firebase from 'firebase-admin';
import { Combatskill, SKILL, ConvertSkillToCombatSkill, SKILL_GROUP, CombatSkillOriginalStats, SkillDefinitions } from "./skills";
import { CombatStats, EncounterDocument, encounterDocumentConverter, ENCOUNTER_CONTEXT, DropTablesData } from "./encounter";
import { EncounterResult, EncounterResultEnemy } from "./encounterResult";
import { LOC, POI, PointOfInterest, POI_SPECIALS, ZONE, LocationConverter, MapLocation, PointOfInterestConverter } from "./worldMap";
import { Party, PartyConverter } from "./party";
import { PROFESSION } from "./crafting";
import { updateMyPortraitAtAllLeaderboards } from "./leaderboards";
import { PendingReward, PerkOfferDefinition } from "./perks";
import { BLESS } from "./specials";

import { Timestamp } from 'firebase-admin/firestore' //WORKAROUND...timestamp je jinak null a nejde ziskavat cas...predtim mu to fcahcalo a nemusel sem tu mit toto
import { InboxItem } from "./inbox";


//import { Equip, EquipAttributes, EQUIP_SLOT_ID, RARITY } from "./equip";
//import { InventoryBag, CharacterDocument, CharacterStats, Currency, firestoreAutoId, Inventory, CharacterEquip } from "./general2";
//import { convertSkillToGiveniLevel, Skill } from "./skills";

// [START import]
// The Cloud Functions for Firebase SDK to create Cloud Functions and set up triggers.
const admin = require('firebase-admin');
admin.initializeApp();


//admin.firestore().settings({ignoreUndefinedProperties:true}); //to sem tady pridal ja abych mohl mit "undefined" fieldy (takove ty co pouzivam jen programove a pak je mazu a davam undefined abych je nemel v DB ulozene), jinak to hazelo error
// [END import]

//Je to maximalni pocet karet v decku. Melo by to odpovidat presne equipu. Pokud mam 18 equipu a kazdy 1 skill. Pak je to 18.
export const MAX_DECK_SIZE = 18;

//maximalne mozny fatigue v %
export const MAX_FATIGUE = 99;

//kolik dostanes time za level-up
export const TIME_BONUS_PER_LEVEL_UP = 8;

//kolik se maximalne casu muze novy charakter mit
export const TIME_MAX = 24;
export const TRAVEL_POINTS_MAX = 30;
export const SCAVENGE_POINTS_MAX = 12;

//kolik se maximalne casu muze novy charakter mit
export const TIME_STARTING = 12;
export const TRAVEL_POINTS_STARTING = 8;
export const SCAVENGE_POINTS_STARTING = 6;

//kolil travel pointu a craft pointu regeneruje restovani
export const TRAVEL_POINTS_PER_REST = 0;
//export const CRAFT_POINTS_PER_REST = 1;

//kolik time stoji nahrazeni travel pointu
export const TIME_PER_TRAVEL_POINT = 1;

// kolik time stoji restnuti
export const REST_TIME_COST = 8;

// kolik food supplies ma kazdy na zacatku 
//export const REST_FOOD_SUPPLIES_LIMIT_STARTING = 3;

// kolik food supplies stoji deep rest
export const REST_FOOD_SUPPLIES_LIMIT_INCREMENT_PER_LEVEL = 0;



//kolik se dostane kazdy hrac Time za hodinu
export const TIME_INCREMENT_PER_SCHEDULED = 1;
//export const TRAVEL_POINTS_INCREMENT_PER_SCHEDULED = 0;//UNUSED
//export const SCAVENGE_POINTS_INCREMENT_PER_SCHEDULED = 1;

//kolik fatigue bere kazde zamichani baliku
export const DECK_SHUFFLE_FATIGUE_PENALTY = 0;

//extra fatigue penalty za to kdyz uteces z boje v procentech decimalnich
export const FLEE_FATIGUE_PENALTY = 5;

//extra fatigue penalty za to prozkoumavas high levelPoI
export const HIGH_LEVEL_POI_FATIGUE_PENALTY = 15;

//jaka je sance ze kdyz prozkoumavam novy tier dostanu do perku i rare perk z nabidky daneho PoI (1 = 100%)
export const CHANCE_OF_DRAWING_RARE_PERK_OFFER = 0.25;

//kolik % HP vezme shuffle deck (tato hodnota se vynasobi poctem kol)
export const DECK_SHUFFLE_MAX_HP_PENALTY = 0.00;

//sance pri explore encounter aby se spawnul i nejaky rare encounter (1 = 100%)
export const RARE_BOSS_SPAWN_CHANCE = 0.03;

//Instantni zabiti nepratel v encounterech
export const INSTAKILL = false;

//Loot spoty muze prozkoumat kdokoliv bez jakychkoliv restrikci
export const INGORE_LOOT_SPOT_RESTRICTIONS = false;

//cheaty
export const CHEATS_ENABLED = true;

//kolik time stoji explore encounteru
export const TIME_COST_TO_EXPLORE_POI = 3;

//export const SCAVENGE_CLAIM_COST = 1;
//export const SCAVENGE_CLAIM_ALL_COST = 3; //Unused
//export const SCAVENGE_CLAIM_COST_TIME = 4;
//export const SCAVENGE_CLAIM_ALL_COST_TIME = 12; //Unused

export const SATOSHIUM_LEADERBOARDS_COEFICIENT = 0.1 // Koeficient kterym se vynasobi satoshim v leaderboardech aby se dalo snadno menit dynamicky.

//Kontroluje se proti tomu co posle klient ( mel bych pridat jeste jeden secret do DB mozna kdybych jen menil data v DB ale tyhle cloud scripty zustaly stejne?)
export const SERVER_SECRET = "XXX15";

//exports.general = require('./general');
exports.general2 = require('./general2');
exports.equip = require('./equip');
exports.auction = require('./auction');
exports.questgiver = require('./questgiver');
exports.vendor = require('./vendor');
exports.inbox = require('./inbox');
exports.worldMap = require('./worldMap');
exports.encounter = require('./encounter');
exports.encounterResult = require('./encounterResult');
exports.scheduler = require('./scheduler');
exports.skills = require('./skills');
exports.party = require('./party');
exports.realtimeDatabase = require('./realtimeDatabase');
exports.gatherables = require('./gatherables');
exports.utils = require('./utils');
exports.rewardTree = require('./rewardTree');
exports.crafting = require('./crafting');
exports.leaderboards = require('./leaderboards');
exports.adminTools = require('./adminTools');
exports.perks = require('./perks');
exports.specials = require('./specials');


export function millisToHours(_millis: number) {
  return _millis / 3600000;
}

export function millisToSeconds(_millis: number) {
  return _millis / 1000;
}

export function hourstoMillis(_hours: number) {
  return Math.round(_hours * 3600000);
}

export function getCurrentDateTime(_hoursToAdd: number): string {
  return (Timestamp.now().toMillis() + (_hoursToAdd * 3600000)).toString();
}

export function getCurrentDateTimeVersionSecondsAdd(_secondsToAdd: number): string {
  return (Timestamp.now().toMillis() + (_secondsToAdd * 1000)).toString();
}

export function getCurrentDateTimeInMillis(_hoursToAdd: number): number {
  return (Timestamp.now().toMillis() + (_hoursToAdd * 3600000));
}

export function getMillisPassedSinceTimestamp(_timestamp: string): number {
  return (getCurrentDateTimeInMillis(0) - Number.parseInt(_timestamp));
}

export function getTimeLeftToDateInSeconds(_timestamp: string) {
  const expireMilis = parseFloat(_timestamp);
  const nowInMilis = Timestamp.now().toMillis();
  const durationLeft = expireMilis - nowInMilis;
  return durationLeft / 1000;
}

export function hasAllPropertiesDefined(data, YourClass) {
  const yourClass = new YourClass()
  return Object.keys(yourClass).every((key) => data[key] !== undefined)
}


function containsWhitespace(str) {
  return /\s/.test(str);
}

export function rollForRandomItems(_listOfStuff: IHasChanceToSpawn[], _numberOfItems: number): IHasChanceToSpawn[] {
  if (_listOfStuff.length === 0 || _numberOfItems <= 0) {
    console.log("Warning! No items to choose from or number of items requested is less than 1.");
    return [];
  }

  let result: IHasChanceToSpawn[] = [];
  let listOfStuffCopy = [..._listOfStuff]; // Make a copy to maintain the original list

  while (result.length < _numberOfItems && listOfStuffCopy.length > 0) {
    let sum = listOfStuffCopy.reduce((a, b) => a + b.chanceToSpawn, 0);
    let randomNum = Math.random() * sum;
    let total = 0;

    for (let i = 0; i < listOfStuffCopy.length; i++) {
      total += listOfStuffCopy[i].chanceToSpawn;
      if (randomNum <= total) {
        result.push(listOfStuffCopy[i]);
        listOfStuffCopy.splice(i, 1); // Remove the chosen item to avoid duplicates
        break; // Break the loop as we've found our item
      }
    }
  }

  if (result.length === 0) {
    console.log("Warning! rollForRandomItems is returning an empty array!");
  }

  return result;
}

export function rollForRandomItem(_listOfStuff: IHasChanceToSpawn[]): IHasChanceToSpawn | null {
  //let choosenIndex: number = -1;

  let sum = _listOfStuff.reduce((a, b) => a + b.chanceToSpawn, 0);
  let randomNum = Math.random() * sum;
  console.log("sum: " + sum + "randomNum: " + randomNum);
  let total = 0;
  for (let i = 0; i < _listOfStuff.length; i++) {
    // console.log("_listOfStuff[i].chanceToSpawn:" + _listOfStuff[i].chanceToSpawn);
    total += _listOfStuff[i].chanceToSpawn;
    // console.log("total:" + total);
    if (randomNum <= total) {
      //console.log("vracim: " + i);
      return _listOfStuff[i];
    }
  }
  console.log("WARINING! rollForRandomItem is returning null!");

  return null;

}

export function randomIntFromInterval(min, max) { // min and max included 
  if (Number.isInteger(min) && Number.isInteger(max)) {
    return Math.floor(Math.random() * (max - min + 1)) + min;
  } else {
    return Math.round((Math.random() * (max - min) + min) * 100) / 100;
    //  return Math.random() * (max - min) + min;
  }
  // return Math.floor(Math.random() * (max - min + 1) + min)
}

//returns true if they are same positions
export function compareWorldPosition(_worldPosition1: WorldPosition, _worldPosition2: WorldPosition): boolean {


  if (_worldPosition1.locationId == _worldPosition2.locationId &&
    _worldPosition1.zoneId == _worldPosition2.zoneId &&
    _worldPosition1.pointOfInterestId == _worldPosition2.pointOfInterestId
  ) {
    return true;

  }

  return false;
}

// export async function resetCharacter(_character: CharacterDocument, _transaction: any): Promise<boolean> {

//   const worldPosition: WorldPosition = new WorldPosition(ZONE.DUNOTAR, LOC.SEASON_TEST, POI.POI_START);
//   _character.position = worldPosition;
//   _character.exploredPositions = [];
//   _character.exploredPositions.push(worldPosition);

//   _character.stats = new CharacterStats(0, 0, 400, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

//   _character.inventory.content = [];
//   _character.inventory.content.push(generateContentContainer(generateContent(ITEMS.BERRY, 1)));

//   _character.currency.gold = _character.currency.gold * 0.5; //50% goldu se prenasi...
//   _character.currency.time = TIME_STARTING;
//   _character.currency.timeMax = TIME_MAX;
//   _character.currency.travelPoints = TRAVEL_POINTS_STARTING;
//   _character.currency.travelPointsMax = TRAVEL_POINTS_MAX;



//   _character.pointsOfInterestMaxTierReached = [];
//   _character.vendorInfo = [];
//   _character.blesses = [];
//   _character.curses = [];
//   // _character.questgiversClaimed = [];
//   // _character.treasuresClaimed = [];
//   // _character.chapelInfo = [];

//   _character.perks.forEach(perk => {
//     if (perk.recurrenceInGameDays != -1)
//       perk.lastClaimGameDay -= 1;

//   });
//   //zachovame vse co ma vetsi kvalitu nez 0
//   for (let i = _character.equipment.length - 1; i >= 0; i--) {
//     if (_character.equipment[i].quality == 0)
//       _character.equipment.splice(i, 1);
//   }



//   let skillDefinitions = await QuerryForSkillDefinitions(_transaction);

//   let genericEquip = generateEquip(1, RARITY.COMMON, EQUIP_SLOT_ID.BODY, CHARACTER_CLASS.WARRIOR, skillDefinitions);

//   let startingSkillPunch: Skill = ConvertSkillDefinitionToSkill(getSkillById(SKILL.PUNCH_1, skillDefinitions), skillDefinitions);
//   let startingSkillBlock: Skill = ConvertSkillDefinitionToSkill(getSkillById(SKILL.BLOCK_1, skillDefinitions), skillDefinitions);
//   let startingSkill_1: Skill = ConvertSkillDefinitionToSkill(getSkillById(SKILL.SLAM_1, skillDefinitions), skillDefinitions);


//   let characterPortrait = "CHARACTER_PORTRAIT_0";
//   let characterPortrait2 = "CHARACTER_PORTRAIT_0";

//   if (_character.characterClass == CHARACTER_CLASS.WARLOCK) {
//     startingSkill_1 = ConvertSkillDefinitionToSkill(getSkillById(SKILL.SHADOWBOLT_1, skillDefinitions), skillDefinitions);
//     characterPortrait = "CHARACTER_PORTRAIT_WARLOCK";
//     characterPortrait2 = "CHARACTER_PORTRAIT_WARLOCK_DEFAULT_1";

//     _character.stats.baseMana = 4;
//     _character.stats.baseManaRegen = 4;

//     _character.stats.baseHealth = 55;
//     _character.stats.skillDrawCount = 5;

//   }
//   else if (_character.characterClass == CHARACTER_CLASS.WARRIOR) {
//     startingSkill_1 = ConvertSkillDefinitionToSkill(getSkillById(SKILL.SLAM_1, skillDefinitions), skillDefinitions);
//     startingSkillBlock = ConvertSkillDefinitionToSkill(getSkillById(SKILL.BUCKLER_1, skillDefinitions), skillDefinitions);
//     characterPortrait = "CHARACTER_PORTRAIT_WARRIOR";
//     characterPortrait2 = "CHARACTER_PORTRAIT_WARRIOR_DEFAULT_1";

//     _character.stats.baseMana = 4;
//     _character.stats.baseManaRegen = 4;

//     _character.stats.baseHealth = 60;
//     _character.stats.skillDrawCount = 5;
//   }
//   else if (_character.characterClass == CHARACTER_CLASS.SHAMAN) {
//     startingSkill_1 = ConvertSkillDefinitionToSkill(getSkillById(SKILL.LIGHTNING_1, skillDefinitions), skillDefinitions);
//     characterPortrait = "CHARACTER_PORTRAIT_SHAMAN";
//     characterPortrait2 = "CHARACTER_PORTRAIT_SHAMAN_DEFAULT_1";

//     _character.stats.baseMana = 4;
//     _character.stats.baseManaRegen = 4;

//     _character.stats.baseHealth = 55;
//     _character.stats.skillDrawCount = 5;
//   }

//   _character.stats.baseCritChance = 5;
//   _character.stats.totalMaxHealth = _character.stats.baseHealth;
//   _character.stats.totalMaxMana = _character.stats.baseMana;
//   _character.stats.totalManaRegen = _character.stats.baseManaRegen;
//   _character.stats.currentHealth = _character.stats.baseHealth;



//   let upgradeMatList: ItemIdWithAmount[] = [];
//   upgradeMatList.push(new ItemIdWithAmount(ITEMS.WOOD, 1));
//   let qualityUpgradeMaterialsRank1: QualityUpgradeMaterials = new QualityUpgradeMaterials(upgradeMatList);
//   let qualityUpgradeMats: QualityUpgradeMaterials[] = [];
//   qualityUpgradeMats.push(qualityUpgradeMaterialsRank1);
//   const sellPrice = 5;
//   let startingBody = new Equip(firestoreAutoId(), "EQUIP", "Apprentice Shirt", "BODY_1", sellPrice, CONTENT_TYPE.EQUIP, EQUIP_SLOT_ID.BODY, RARITY.COMMON, 1, 1, startingSkill_1, [], 0, 5, genericEquip.qualityUpgradeMaterials, false, [], []);
//   const startingHead = new Equip(firestoreAutoId(), "EQUIP", "Apprentice Cap", "HEAD_1", sellPrice, CONTENT_TYPE.EQUIP, EQUIP_SLOT_ID.HEAD, RARITY.COMMON, 1, 1, startingSkill_1, [], 0, 5, genericEquip.qualityUpgradeMaterials, false, [], []);
//   const startingLegs = new Equip(firestoreAutoId(), "EQUIP", "Apprentice Pants", "LEGS_1", sellPrice, CONTENT_TYPE.EQUIP, EQUIP_SLOT_ID.LEGS, RARITY.COMMON, 1, 1, startingSkillBlock, [], 0, 5, genericEquip.qualityUpgradeMaterials, false, [], []);
//   const startingFeet = new Equip(firestoreAutoId(), "EQUIP", "Apprentice Boots", "FEET_1", sellPrice, CONTENT_TYPE.EQUIP, EQUIP_SLOT_ID.FEET, RARITY.COMMON, 1, 1, startingSkillBlock, [], 0, 5, genericEquip.qualityUpgradeMaterials, false, [], []);
//   const startingHands = new Equip(firestoreAutoId(), "EQUIP", "Apprentice Gloves", "HANDS_1", sellPrice, CONTENT_TYPE.EQUIP, EQUIP_SLOT_ID.HANDS, RARITY.COMMON, 1, 1, startingSkillBlock, [], 0, 5, genericEquip.qualityUpgradeMaterials, false, [], []);
//   const startingWaist = new Equip(firestoreAutoId(), "EQUIP", "Apprentice Belt", "WAIST_1", sellPrice, CONTENT_TYPE.EQUIP, EQUIP_SLOT_ID.WAIST, RARITY.COMMON, 1, 1, startingSkillBlock, [], 0, 5, genericEquip.qualityUpgradeMaterials, false, [], []);
//   const startingWrist = new Equip(firestoreAutoId(), "EQUIP", "Apprentice Wrist", "WRIST_1", sellPrice, CONTENT_TYPE.EQUIP, EQUIP_SLOT_ID.WRIST, RARITY.COMMON, 1, 1, startingSkillPunch, [], 0, 5, genericEquip.qualityUpgradeMaterials, false, [], []);
//   const startingShoulder = new Equip(firestoreAutoId(), "EQUIP", "Apprentice Shoulders", "SHOULDER_1", sellPrice, CONTENT_TYPE.EQUIP, EQUIP_SLOT_ID.SHOULDER, RARITY.COMMON, 1, 1, startingSkillPunch, [], 0, 5, genericEquip.qualityUpgradeMaterials, false, [], []);
//   const startingBack = new Equip(firestoreAutoId(), "EQUIP", "Apprentice Cape", "BACK_1", sellPrice, CONTENT_TYPE.EQUIP, EQUIP_SLOT_ID.BACK, RARITY.COMMON, 1, 1, startingSkillPunch, [], 0, 5, genericEquip.qualityUpgradeMaterials, false, [], []);
//   const startingNeck = new Equip(firestoreAutoId(), "EQUIP", "Apprentice Gorget", "NECK_1", sellPrice, CONTENT_TYPE.EQUIP, EQUIP_SLOT_ID.NECK, RARITY.COMMON, 1, 1, startingSkillPunch, [], 0, 5, genericEquip.qualityUpgradeMaterials, false, [], []);

//   const startingFinger = new Equip(firestoreAutoId(), "EQUIP", "Apprentice Ring", "FINGER_1", sellPrice, CONTENT_TYPE.EQUIP, EQUIP_SLOT_ID.FINGER_1, RARITY.COMMON, 1, 1, startingSkillPunch, [], 0, 5, genericEquip.qualityUpgradeMaterials, false, [], []);
//   const startingAmulet = new Equip(firestoreAutoId(), "EQUIP", "Apprentice Amulet", "AMULET_1", sellPrice, CONTENT_TYPE.EQUIP, EQUIP_SLOT_ID.AMULET, RARITY.COMMON, 1, 1, startingSkillPunch, [], 0, 5, genericEquip.qualityUpgradeMaterials, false, [], []);
//   const startingTrinket = new Equip(firestoreAutoId(), "EQUIP", "Apprentice Trinket", "TRINKET_1", sellPrice, CONTENT_TYPE.EQUIP, EQUIP_SLOT_ID.TRINKET, RARITY.COMMON, 1, 1, startingSkillPunch, [], 0, 5, genericEquip.qualityUpgradeMaterials, false, [], []);
//   const startingOffHand = new Equip(firestoreAutoId(), "EQUIP", "Apprentice Dagger", "OFF_HAND_1", sellPrice, CONTENT_TYPE.EQUIP, EQUIP_SLOT_ID.OFF_HAND, RARITY.COMMON, 1, 1, startingSkillPunch, [], 0, 5, genericEquip.qualityUpgradeMaterials, false, [], []);
//   const startingMainHand = new Equip(firestoreAutoId(), "EQUIP", "Apprentice Sword", "MAIN_HAND_1", sellPrice, CONTENT_TYPE.EQUIP, EQUIP_SLOT_ID.MAIN_HAND, RARITY.COMMON, 1, 1, startingSkillPunch, [], 0, 5, genericEquip.qualityUpgradeMaterials, false, [], []);
//   const startingBag = new Equip(firestoreAutoId(), "EQUIP", "Apprentice Bag", "BAG_1", sellPrice, CONTENT_TYPE.EQUIP, EQUIP_SLOT_ID.BAG, RARITY.COMMON, 1, 1, startingSkillBlock, [], 0, 5, genericEquip.qualityUpgradeMaterials, false, [], []);
//   const startingEarring = new Equip(firestoreAutoId(), "EQUIP", "Apprentice Earring", "EARRING_1", sellPrice, CONTENT_TYPE.EQUIP, EQUIP_SLOT_ID.EARRING, RARITY.COMMON, 1, 1, startingSkillBlock, [], 0, 5, genericEquip.qualityUpgradeMaterials, false, [], []);
//   const startingCharm = new Equip(firestoreAutoId(), "EQUIP", "Apprentice Charm", "CHARM_1", sellPrice, CONTENT_TYPE.EQUIP, EQUIP_SLOT_ID.CHARM, RARITY.COMMON, 1, 1, startingSkillBlock, [], 0, 5, genericEquip.qualityUpgradeMaterials, false, [], []);

//   let equipTmp: Equip | undefined;
//   if (!_character.equipment.find(item => item.equipSlotId == EQUIP_SLOT_ID.BODY))
//     _character.equipment.push(startingBody);
//   if (!_character.equipment.find(item => item.equipSlotId == EQUIP_SLOT_ID.HEAD))
//     _character.equipment.push(startingHead);
//   if (!_character.equipment.find(item => item.equipSlotId == EQUIP_SLOT_ID.LEGS))
//     _character.equipment.push(startingLegs);
//   if (!_character.equipment.find(item => item.equipSlotId == EQUIP_SLOT_ID.FEET))
//     _character.equipment.push(startingFeet);
//   if (!_character.equipment.find(item => item.equipSlotId == EQUIP_SLOT_ID.HANDS))
//     _character.equipment.push(startingHands);
//   if (!_character.equipment.find(item => item.equipSlotId == EQUIP_SLOT_ID.WAIST))
//     _character.equipment.push(startingWaist);
//   if (!_character.equipment.find(item => item.equipSlotId == EQUIP_SLOT_ID.WRIST))
//     _character.equipment.push(startingWrist);
//   if (!_character.equipment.find(item => item.equipSlotId == EQUIP_SLOT_ID.BACK))
//     _character.equipment.push(startingBack);
//   if (!_character.equipment.find(item => item.equipSlotId == EQUIP_SLOT_ID.SHOULDER))
//     _character.equipment.push(startingShoulder);
//   if (!_character.equipment.find(item => item.equipSlotId == EQUIP_SLOT_ID.SHOULDER))
//     _character.equipment.push(startingBack);
//   if (!_character.equipment.find(item => item.equipSlotId == EQUIP_SLOT_ID.NECK))
//     _character.equipment.push(startingNeck);
//   if (!_character.equipment.find(item => item.equipSlotId == EQUIP_SLOT_ID.FINGER_1))
//     _character.equipment.push(startingFinger);
//   if (!_character.equipment.find(item => item.equipSlotId == EQUIP_SLOT_ID.AMULET))
//     _character.equipment.push(startingAmulet);
//   if (!_character.equipment.find(item => item.equipSlotId == EQUIP_SLOT_ID.TRINKET))
//     _character.equipment.push(startingTrinket);
//   if (!_character.equipment.find(item => item.equipSlotId == EQUIP_SLOT_ID.OFF_HAND))
//     _character.equipment.push(startingOffHand);
//   if (!_character.equipment.find(item => item.equipSlotId == EQUIP_SLOT_ID.MAIN_HAND))
//     _character.equipment.push(startingMainHand);
//   if (!_character.equipment.find(item => item.equipSlotId == EQUIP_SLOT_ID.BAG))
//     _character.equipment.push(startingBag);
//   if (!_character.equipment.find(item => item.equipSlotId == EQUIP_SLOT_ID.EARRING))
//     _character.equipment.push(startingEarring);
//   if (!_character.equipment.find(item => item.equipSlotId == EQUIP_SLOT_ID.CHARM))
//     _character.equipment.push(startingCharm);




//   // Define a query to find documents with encounter.id = "AHOJ"
//   const allMyEncountersQuerry = admin.firestore().collection('encounters').where("foundByCharacterUid", "==", _character.uid);

//   // Fetch the documents matching the query
//   const snapshot = await _transaction.get(allMyEncountersQuerry);

//   // Run a transaction to delete the fetched documents
//   await snapshot.docs.forEach(doc => {
//     _transaction.delete(doc.ref);
//   });


//   return true;
// }



async function createCharacter(_transaction: any, _characterUid: string, _userUid: string, _characterName: string, _characterClass: string, _characterPortrait: string): Promise<CharacterDocument> {

  let characterName = _characterName;
  characterName = characterName.toLowerCase();
  characterName = characterName.charAt(0).toUpperCase() + characterName.slice(1);

  if (containsWhitespace(characterName))
    throw "Character name can be only single world without white spaces";

  if (characterName.length > 12 || characterName.length < 2)
    throw "Character name must have 2-12 characters";

  //specialni charaktery
  if (characterName.match(/[_\W0-9]/)) {
    throw "Character name can contain only non-accented alphabet characters";

  }


  //zkontrolujem unikatnost jmena (ignorujem casesensitivitu)
  const characterNamesDb = admin.firestore().collection('characterNames').doc(characterName);
  const characterNamesDoc = await _transaction.get(characterNamesDb);
  let characterNamesData: CharacterNameEntry = characterNamesDoc.data();
  if (characterNamesData != undefined) throw "Name already exists! Please choose different one.";

  //global data
  const globalDataDb = admin.firestore().collection('_metadata_coreDefinitions').doc("Global");
  const globalDataDoc = await _transaction.get(globalDataDb);
  let globalDataData: GlobalMetadata = globalDataDoc.data();


  console.log("Creating character - uid :" + _characterUid + " userUid: " + _userUid + " characterName: " + characterName + " class: " + _characterClass);
  let stats = new CharacterStats(0, 0, 400, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);


  const worldPosition: WorldPosition = new WorldPosition(ZONE.DUNOTAR, LOC.SEASON_TEST, POI.POI_START);

  const mapMemmoryEntry: WorldMapMemmoryEntry = new WorldMapMemmoryEntry(worldPosition, [POI_SPECIALS.INN, POI_SPECIALS.VENDOR], "START_NODE", 0, "START", 0);
  const MapMemmory: WorldMapMemmoryEntry[] = [];
  MapMemmory.push(mapMemmoryEntry);

  let bags: InventoryBag[] = [];
  bags.push(new InventoryBag(firestoreAutoId(), "STARTING_BAG", 16));
  const inventory = new Inventory(bags, 16, 16, []);
  inventory.content.push(generateContentContainer(generateContent(ITEMS.ROTTEN_APPLE, 1)));

  //inventory.content.push(generateContentContainer(generateContent(ITEMS.TOWN_PORTAL, 1)));
  //inventory.content.push(generateContentContainer(generateContent(ITEMS.MINOR_HEALTH_POTION, 3)));
  const currency = new Currency(10, TIME_STARTING, TIME_MAX, TRAVEL_POINTS_STARTING, TRAVEL_POINTS_MAX, SCAVENGE_POINTS_STARTING, SCAVENGE_POINTS_MAX, 0);
  const creationDate = getCurrentDateTime(0);


  // let genericEquip = generateEquip(1, RARITY.COMMON, EQUIP_SLOT_ID.BODY, CHARACTER_CLASS.WARRIOR, skillDefinitions);

  //let startingSkillPunch: Skill = ConvertSkillDefinitionToSkill(getSkillById(SKILL.PUNCH_1, skillDefinitions), skillDefinitions);
  let startingSkillBlock: string = SKILL.BLOCK_1;
  let startingSkill_1: string = SKILL.PUNCH_1;
  let startingSkill_2: string = SKILL.PUNCH_1;
  let startingSkill_3: string = SKILL.PUNCH_1;


  const proffesions: SimpleTallyWithMax[] = [];


  // let characterPortrait = "CHARACTER_PORTRAIT_0";
  // let characterPortrait2 = "CHARACTER_PORTRAIT_0";

  if (_characterClass == CHARACTER_CLASS.WARLOCK) {
    startingSkill_1 = SKILL.SHADOWBOLT_1;


    stats.baseMana = 4;
    stats.baseManaRegen = 4;
    stats.baseHealth = 90;
    stats.currentHealth = 90;
    stats.baseCritChance = 5;
    stats.healthBlockedByFatigue = 30;

    stats.skillDrawCount = 5;
    stats.restFoodLimit = 5;

  }
  else if (_characterClass == CHARACTER_CLASS.WARRIOR) {
    startingSkill_1 = SKILL.PUNCH_1;
    startingSkill_2 = SKILL.REND_1;
    startingSkill_3 = SKILL.SLAM_1;
    startingSkillBlock = SKILL.BUCKLER_1;


    stats.baseMana = 4;
    stats.baseManaRegen = 4;
    stats.baseCritChance = 5;
    stats.baseHealth = 90;
    stats.currentHealth = 90;
    stats.healthBlockedByFatigue = 30;
    //stats.healthBlockedByFatigue = 60;
    stats.skillDrawCount = 5;
    stats.restFoodLimit = 5;
  }
  else if (_characterClass == CHARACTER_CLASS.SHAMAN) {
    startingSkill_1 = SKILL.LIGHTNING_1;


    stats.baseMana = 4;
    stats.baseManaRegen = 4;
    stats.baseCritChance = 5;
    stats.baseHealth = 100;
    stats.currentHealth = 100;
    stats.healthBlockedByFatigue = 30;
    // stats.healthBlockedByFatigue = 50;
    stats.skillDrawCount = 5;
    stats.restFoodLimit = 5;
  }


  let characteEquip: Equip[] = [];



  let skillDefinitions = await QuerryForSkillDefinitions(_transaction);
  const startingBody = generateEquip(1, RARITY.COMMON, EQUIP_SLOT_ID.BODY, _characterClass, skillDefinitions, startingSkill_3, "Apprentice Shirt"); //    new Equip(firestoreAutoId(), "EQUIP", "Apprentice Shirt", "BODY_1", sellPrice, CONTENT_TYPE.EQUIP, EQUIP_SLOT_ID.BODY, RARITY.COMMON, 1, 1, startingSkill_3, [], 0, 5, genericEquip.qualityUpgradeMaterials, false, [], []);
  const startingHead = generateEquip(1, RARITY.COMMON, EQUIP_SLOT_ID.HEAD, _characterClass, skillDefinitions, startingSkill_2, "Apprentice Cap");//new Equip(firestoreAutoId(), "EQUIP", "Apprentice Cap", "HEAD_1", sellPrice, CONTENT_TYPE.EQUIP, EQUIP_SLOT_ID.HEAD, RARITY.COMMON, 1, 1, startingSkill_2, [], 0, 5, genericEquip.qualityUpgradeMaterials, false, [], []);
  const startingLegs = generateEquip(1, RARITY.COMMON, EQUIP_SLOT_ID.LEGS, _characterClass, skillDefinitions, startingSkillBlock, "Apprentice Pants");//new Equip(firestoreAutoId(), "EQUIP", "Apprentice Pants", "LEGS_1", sellPrice, CONTENT_TYPE.EQUIP, EQUIP_SLOT_ID.LEGS, RARITY.COMMON, 1, 1, startingSkillBlock, [], 0, 5, genericEquip.qualityUpgradeMaterials, false, [], []);
  const startingFeet = generateEquip(1, RARITY.COMMON, EQUIP_SLOT_ID.FEET, _characterClass, skillDefinitions, startingSkillBlock, "Apprentice Boots");//new Equip(firestoreAutoId(), "EQUIP", "Apprentice Boots", "FEET_1", sellPrice, CONTENT_TYPE.EQUIP, EQUIP_SLOT_ID.FEET, RARITY.COMMON, 1, 1, startingSkillBlock, [], 0, 5, genericEquip.qualityUpgradeMaterials, false, [], []);
  const startingHands = generateEquip(1, RARITY.COMMON, EQUIP_SLOT_ID.HANDS, _characterClass, skillDefinitions, startingSkillBlock, "Apprentice Gloves");//new Equip(firestoreAutoId(), "EQUIP", "Apprentice Gloves", "HANDS_1", sellPrice, CONTENT_TYPE.EQUIP, EQUIP_SLOT_ID.HANDS, RARITY.COMMON, 1, 1, startingSkillBlock, [], 0, 5, genericEquip.qualityUpgradeMaterials, false, [], []);
  const startingWaist = generateEquip(1, RARITY.COMMON, EQUIP_SLOT_ID.WAIST, _characterClass, skillDefinitions, startingSkillBlock, "Apprentice Belt");//new Equip(firestoreAutoId(), "EQUIP", "Apprentice Belt", "WAIST_1", sellPrice, CONTENT_TYPE.EQUIP, EQUIP_SLOT_ID.WAIST, RARITY.COMMON, 1, 1, startingSkillBlock, [], 0, 5, genericEquip.qualityUpgradeMaterials, false, [], []);
  const startingWrist = generateEquip(1, RARITY.COMMON, EQUIP_SLOT_ID.WRIST, _characterClass, skillDefinitions, startingSkill_1, "Apprentice Wrist");//new Equip(firestoreAutoId(), "EQUIP", "Apprentice Wrist", "WRIST_1", sellPrice, CONTENT_TYPE.EQUIP, EQUIP_SLOT_ID.WRIST, RARITY.COMMON, 1, 1, startingSkill_1, [], 0, 5, genericEquip.qualityUpgradeMaterials, false, [], []);
  const startingShoulder = generateEquip(1, RARITY.COMMON, EQUIP_SLOT_ID.SHOULDER, _characterClass, skillDefinitions, startingSkill_1, "Apprentice Shoulderpads");//new Equip(firestoreAutoId(), "EQUIP", "Apprentice Shoulders", "SHOULDER_1", sellPrice, CONTENT_TYPE.EQUIP, EQUIP_SLOT_ID.SHOULDER, RARITY.COMMON, 1, 1, startingSkill_1, [], 0, 5, genericEquip.qualityUpgradeMaterials, false, [], []);
  const startingBack = generateEquip(1, RARITY.COMMON, EQUIP_SLOT_ID.BACK, _characterClass, skillDefinitions, startingSkill_1, "Apprentice Cape");//new Equip(firestoreAutoId(), "EQUIP", "Apprentice Cape", "BACK_1", sellPrice, CONTENT_TYPE.EQUIP, EQUIP_SLOT_ID.BACK, RARITY.COMMON, 1, 1, startingSkill_1, [], 0, 5, genericEquip.qualityUpgradeMaterials, false, [], []);
  const startingNeck = generateEquip(1, RARITY.COMMON, EQUIP_SLOT_ID.NECK, _characterClass, skillDefinitions, startingSkill_1, "Apprentice Gorget");//new Equip(firestoreAutoId(), "EQUIP", "Apprentice Gorget", "NECK_1", sellPrice, CONTENT_TYPE.EQUIP, EQUIP_SLOT_ID.NECK, RARITY.COMMON, 1, 1, startingSkill_1, [], 0, 5, genericEquip.qualityUpgradeMaterials, false, [], []);

  const startingFinger = generateEquip(1, RARITY.COMMON, EQUIP_SLOT_ID.FINGER_1, _characterClass, skillDefinitions, startingSkill_1, "Apprentice Ring");//new Equip(firestoreAutoId(), "EQUIP", "Apprentice Ring", "FINGER_1", sellPrice, CONTENT_TYPE.EQUIP, EQUIP_SLOT_ID.FINGER_1, RARITY.COMMON, 1, 1, startingSkill_1, [], 0, 5, genericEquip.qualityUpgradeMaterials, false, [], []);
  const startingAmulet = generateEquip(1, RARITY.COMMON, EQUIP_SLOT_ID.AMULET, _characterClass, skillDefinitions, startingSkill_1, "Apprentice Amulet");//new Equip(firestoreAutoId(), "EQUIP", "Apprentice Amulet", "AMULET_1", sellPrice, CONTENT_TYPE.EQUIP, EQUIP_SLOT_ID.AMULET, RARITY.COMMON, 1, 1, startingSkill_1, [], 0, 5, genericEquip.qualityUpgradeMaterials, false, [], []);
  const startingTrinket = generateEquip(1, RARITY.COMMON, EQUIP_SLOT_ID.TRINKET, _characterClass, skillDefinitions, startingSkill_1, "Apprentice Trinket");//new Equip(firestoreAutoId(), "EQUIP", "Apprentice Trinket", "TRINKET_1", sellPrice, CONTENT_TYPE.EQUIP, EQUIP_SLOT_ID.TRINKET, RARITY.COMMON, 1, 1, startingSkill_1, [], 0, 5, genericEquip.qualityUpgradeMaterials, false, [], []);
  const startingOffHand = generateEquip(1, RARITY.COMMON, EQUIP_SLOT_ID.OFF_HAND, _characterClass, skillDefinitions, startingSkill_1, "Apprentice Dagger");//new Equip(firestoreAutoId(), "EQUIP", "Apprentice Dagger", "OFF_HAND_1", sellPrice, CONTENT_TYPE.EQUIP, EQUIP_SLOT_ID.OFF_HAND, RARITY.COMMON, 1, 1, startingSkill_1, [], 0, 5, genericEquip.qualityUpgradeMaterials, false, [], []);
  const startingMainHand = generateEquip(1, RARITY.COMMON, EQUIP_SLOT_ID.MAIN_HAND, _characterClass, skillDefinitions, startingSkill_1, "Apprentice Sword");//new Equip(firestoreAutoId(), "EQUIP", "Apprentice Sword", "MAIN_HAND_1", sellPrice, CONTENT_TYPE.EQUIP, EQUIP_SLOT_ID.MAIN_HAND, RARITY.COMMON, 1, 1, startingSkill_1, [], 0, 5, genericEquip.qualityUpgradeMaterials, false, [], []);
  const startingBag = generateEquip(1, RARITY.COMMON, EQUIP_SLOT_ID.BAG, _characterClass, skillDefinitions, startingSkillBlock, "Apprentice Trinket");//Equip(firestoreAutoId(), "EQUIP", "Apprentice Bag", "BAG_1", sellPrice, CONTENT_TYPE.EQUIP, EQUIP_SLOT_ID.BAG, RARITY.COMMON, 1, 1, startingSkillBlock, [], 0, 5, genericEquip.qualityUpgradeMaterials, false, [], []);
  const startingEarring = generateEquip(1, RARITY.COMMON, EQUIP_SLOT_ID.EARRING, _characterClass, skillDefinitions, startingSkillBlock, "Apprentice Earring");//new Equip(firestoreAutoId(), "EQUIP", "Apprentice Earring", "EARRING_1", sellPrice, CONTENT_TYPE.EQUIP, EQUIP_SLOT_ID.EARRING, RARITY.COMMON, 1, 1, startingSkillBlock, [], 0, 5, genericEquip.qualityUpgradeMaterials, false, [], []);
  const startingCharm = generateEquip(1, RARITY.COMMON, EQUIP_SLOT_ID.CHARM, _characterClass, skillDefinitions, startingSkillBlock, "Apprentice Charm");//new Equip(firestoreAutoId(), "EQUIP", "Apprentice Charm", "CHARM_1", sellPrice, CONTENT_TYPE.EQUIP, EQUIP_SLOT_ID.CHARM, RARITY.COMMON, 1, 1, startingSkillBlock, [], 0, 5, genericEquip.qualityUpgradeMaterials, false, [], []);


  characteEquip.push(startingBody);
  characteEquip.push(startingHead);
  characteEquip.push(startingLegs);
  characteEquip.push(startingFeet);
  characteEquip.push(startingHands);
  characteEquip.push(startingWaist);
  characteEquip.push(startingWrist);
  characteEquip.push(startingShoulder);
  characteEquip.push(startingBack);
  characteEquip.push(startingNeck);

  characteEquip.push(startingFinger);
  characteEquip.push(startingAmulet);
  characteEquip.push(startingTrinket);
  characteEquip.push(startingOffHand);
  characteEquip.push(startingMainHand);
  characteEquip.push(startingBag);
  characteEquip.push(startingEarring);
  characteEquip.push(startingCharm);

  let exploredPositions: WorldPosition[] = [];
  exploredPositions.push(worldPosition);


  let createdCharacter = new CharacterDocument(_characterUid, _userUid, characterName, _characterClass, inventory, characteEquip, currency, stats, worldPosition, [], [], creationDate, _characterPortrait, exploredPositions, proffesions, [], 0, [], [], 0, worldPosition, MapMemmory, [], [], [], [], [], [], [], globalDataData.seasonNumber, false);
  createdCharacter.recalculateCharacterStats();
  return createdCharacter;

  //return new CharacterDocument("",[],new CharacterStats(0,0,0,0,0,0,0,0,0,0),"","",new Currency(0,0,0),"",new Inventory([],0,0,[],[]));
}

export function validateCaller(_data: any, _context: functions.https.CallableContext) {

  //no to je pekny ale client by mohl posilat userUid toho hrace ktereho fejkuje, sice nevim jak by ho ziskal protoze jedine kde je exposed je v characterDocumentu? NO PRAVE nema jak ho ziskat prece?

  //jediny uplne cheat proof je jeste ziskat charakter dokument za ktery se vydavam z toho dokuemntu ziskaat userUid(playerUid) a zkontrolovat ze odpovidat conterxt.UserUid
  //tak jedine si muzu byt 100% jisty ze ten clovek co vola je i vlastnikem toho charakteru...
  const userUid = _data.userUid;

  if (userUid == undefined) {
    console.log("player add UserUid to the client when calling CloudFucntion so I can check for validity of user! ");
    return;
  }

  console.log("player s uid :" + _context.auth?.uid + " vola metodu a tvrdi ze je : " + userUid);
  if (userUid != _context.auth?.uid)
    throw "User caller mismatach!";
}

// export function validateCallerBulletProof(_characterData: CharacterDocument, _context: functions.https.CallableContext) {

//   const userUid = _characterData.userUid;

//   if (userUid == undefined) {
//     console.log("player add UserUid to the client when calling CloudFucntion so I can check for validity of user! ");
//     return;
//   }

//   console.log("player s uid :" + _context.auth?.uid + " vola metodu a tvrdi ze je : " + userUid);
//   if (userUid != _context.auth?.uid)
//     throw "User caller mismatach!";
// }


export function validateCallerBulletProof(_characterData: CharacterDocument, _context: functions.https.CallableContext) {



  // console.log("player s uid :" + _context.auth?.uid + " vola metodu a tvrdi ze vlastni charakter : " + _characterData.userUid);
  // if (_characterData.userUid != _context.auth?.uid)
  //   throw "User caller mismatach! You dont own this character!!!";

  if (_characterData.isRetired)
    throw "This character is retired. You cant do anything with it!";

  if (_characterData.isRetired)
    throw "This character is retired. You cant do anything with it!";
}

export function checkForServerVersion(_data: any) {

  const serverSecret = _data.serverSecret;

  if (serverSecret != SERVER_SECRET)
    throw ("Old client version detected! Please update game from appstore!");
}


export function getExpAmountFromEncounterForGivenCharacterLevel(_encounterResultEnemies: EncounterResultEnemy[], _characterLevel: number): number {


  let TotalEXPReward: number = 0;
  _encounterResultEnemies.forEach(enemy => {

    let EXPFromEnemy = (_characterLevel * 5) + 45;

    //Odmena za enemy s vyssim level nez jsi ty
    if (enemy.level > _characterLevel) {
      EXPFromEnemy = (EXPFromEnemy) * (1 + 0.05 * (enemy.level - _characterLevel));
    }
    //Penalty za enemy s nizsim level nez jsi ty
    else if (enemy.level < _characterLevel) {
      // ZD = 5, when Char Level = 1 - 7
      // ZD = 6, when Char Level = 8 - 9
      // ZD = 7, when Char Level = 10 - 11
      // ZD = 8, when Char Level = 12 - 15
      // ZD = 9, when Char Level = 16 - 19
      // ZD = 11, when Char Level = 20 - 29
      // ZD = 12, when Char Level = 30 - 39
      // ZD = 13, when Char Level = 40 - 44
      // ZD = 14, when Char Level = 45 - 49
      // ZD = 15, when Char Level = 50 - 54
      // ZD = 16, when Char Level = 55 - 59
      // ZD = 17, when Char Level = 60 - 79
      const ZD = 5; //TODO TOHLE SE MENI S LEVELEM CHARAKTERU
      EXPFromEnemy = (EXPFromEnemy) * (1 - (_characterLevel - enemy.level) / ZD)
    }
    if (EXPFromEnemy < 0)
      EXPFromEnemy = 0;

    console.log("Enemy " + enemy.displayName + "(level " + enemy.level + ") gave you " + EXPFromEnemy);
    TotalEXPReward += EXPFromEnemy;
  });

  return TotalEXPReward;

}

export const enum CONTENT_TYPE {
  ITEM = "ITEM",
  EQUIP = "EQUIP",
  CURRENCY = "CURRENCY",
  FOOD = "FOOD",
  RECIPE = "RECIPE",
  FOOD_SUPPLY = "FOOD_SUPPLY",
  CHEST = "CHEST",
}


export const enum CURRENCY_ID {
  SILVER = "SILVER",
  GOLD = "GOLD",
  TIME = "TIME",
  FATIGUE = "FATIGUE",
  TRAVEL_POINTS = "TRAVEL_POINTS",
  SCAVENGE_POINTS = "SCAVENGE_POINTS",
  MONSTER_ESSENCE = "MONSTER_ESSENCE"
  //  SATOSHIUM = "SATOSHIUM"

}

export const enum CHARACTER_CLASS {
  WARRIOR = "WARRIOR",
  WARLOCK = "WARLOCK",
  MAGE = "MAGE",
  SHAMAN = "SHAMAN",
  ANY = "ANY",
}


export function generateContentContainer(_content: Content): ContentContainer {

  if (_content.contentType == CONTENT_TYPE.EQUIP) {
    return new ContentContainer(undefined, _content as Equip);
  }
  else {
    console.log(_content.rarity + " : je to item");
    return new ContentContainer(_content, undefined);
  }
}

//[ContentContainer]
export class ContentContainer {
  constructor(
    public content: Content | undefined,
    public contentEquip: Equip | undefined,
  ) { }

  getItem(): Content {

    if (this.content != undefined)
      return this.content;
    else if (this.contentEquip != undefined)
      return this.contentEquip;

    throw ("ERROR : Invalid Item type in BaseContent!");

  }
  // getItem(): Content {
  //   switch (this.contentType) {
  //     case CONTENT_TYPE.EQUIP: if (this.contentEquip != undefined) return this.contentEquip; else throw ("ERROR: There is no Equip entry when there should be!");
  //     case CONTENT_TYPE.ITEM: if (this.contentItem != undefined) return this.contentItem; else throw ("ERROR: There is no Item entry when there should be!");
  //     case CONTENT_TYPE.CURRENCY: if (this.contentCurrency != undefined) return this.contentCurrency; else throw ("ERROR: There is no Currency entry when there should be!");
  //     case CONTENT_TYPE.FOOD: if (this.contentFood != undefined) return this.contentFood; else throw ("ERROR: There is no Food entry when there should be!");


  //     default: throw ("ERROR : Invalid Item type in BaseContent!");
  //   }
  // }
}
//[ContentContainer]



export class OtherMetadata {
  constructor(
    public possiblePortraits: PossiblePortraits[],
    public whatIsNew: string,
  ) { }
}




export class PossiblePortraits {
  constructor(
    public classId: string,
    public portraits: string[],
  ) { }
}

//[World Position]
export class WorldPosition {
  constructor(
    public zoneId: string,
    public locationId: string,
    public pointOfInterestId: string,
  ) { }
}
//[World Position]

export class SimpleTally {
  constructor(
    public id: string,
    public count: number,
  ) { }
}

export class SimpleStringDictionary {
  constructor(
    public string1: string,
    public string2: string,
  ) { }
}

export class SimpleTallyWithMax {
  constructor(
    public id: string,
    public count: number,
    public countMax: number,
  ) { }
}



export class InventoryBag {
  constructor(
    public uid: string,
    public itemId: string,
    public capacity: number,
  ) { }

}

export class Inventory {
  constructor(
    public bags: InventoryBag[],
    public capacityMax: number,
    public capacityLeft: number,
    //public itemsSimple: InventoryItemSimple[],
    // public itemsEquip: Equip[]
    public content: ContentContainer[]

  ) { }
}

// export class WorldPosition {
//   constructor(
//     public pointsOfInterest: string[],
//     public locations: string[],

//   ) { }
// }

export class GlobalMetadata {
  constructor(
    public gameDay: number,
    public nextSeasonStartDelayInHours: number,
    public nextGameDayTimestamp: string,
    public nextSeasonStartTimestamp: string,
    public seasonDurationDays: number,
    public seasonNumber: number,
    public isSeasonInProgress: boolean

  ) { }
}

export class WorldMapMemmoryEntry {
  constructor(
    public worldPosition: WorldPosition,
    public specialPointsOfInterest: string[],
    public typeId: string,
    public tiersCount: number,
    public roomType: string,
    public partySize: number,
  ) { }
}

export class ChapelInfo {
  constructor(
    public worldPosition: WorldPosition,
    public blessId: string,
    public used: boolean
  ) { }
}


export class VendorInfo {
  constructor(
    public vendorId: string,
    public goodsPurchased: ItemIdWithAmount[],
  ) { }
}

// [Character]
export class CharacterDocument {
  constructor(
    public uid: string,
    public userUid: string,
    public characterName: string,
    public characterClass: string,
    public inventory: Inventory,
    public equipment: Equip[],
    public currency: Currency,
    public stats: CharacterStats,
    public position: WorldPosition,
    public monsterKills: SimpleTally[],
    public questgiversClaimed: string[],
    public creationDate: string,
    public characterPortrait: string,
    public exploredPositions: WorldPosition[],
    public professions: SimpleTallyWithMax[],
    public craftingRecipesUnlocked: string[],
    public lastClaimedGameDay: number,
    public pointsOfInterestMaxTierReached: SimpleTally[],
    public pendingRewards: PendingReward[],
    public innHealhRestsCount: number,
    public homeInn: WorldPosition,
    public worldMapMemmory: WorldMapMemmoryEntry[],
    public foodEffects: SimpleTally[],
    public curses: Combatskill[],
    //public chapelsUsed: WorldPosition[],
    public blesses: string[],
    public chapelInfo: ChapelInfo[],
    public vendorInfo: VendorInfo[],
    public treasuresClaimed: WorldPosition[],
    public dungeonsFinished: string[],
    public seasonNumber: number,
    public isRetired: boolean
    // public rewardTreeRewardsClaimed: string[],

  ) { }



  // async querryForMyParty(_transaction: any): Promise<Party | null> {

  //   const myPartyDb = admin.firestore().collection('parties').where("partyMembersUidList", "array-contains", this.uid).withConverter(PartyConverter);

  //   let myPartyData: Party | null = null;

  //   //ziskam tvoji partu
  //   await _transaction.get(myPartyDb).then(querry => {
  //     if (querry.size == 1) {
  //       querry.docs.forEach(doc => {
  //         myPartyData = doc.data();
  //       });
  //     }
  //     else if (querry.size > 1)
  //       throw "You are more than in 1 party! How could this be? DATABASE ERROR!";
  //   });



  //   return myPartyData;
  // }

  addPendingReward(_perkDefinition: PerkOfferDefinition, _skillDefinitions: SkillDefinitions) {
    if (_perkDefinition.recurrenceInGameDays > -1) //pokud je to recuring perk
    {
      console.log("je ok");
      if (_perkDefinition.rewards.length == 1) //pokud ma jednu rewardu protoze scavenge spoty maji jen 1 reward?
      {
        console.log("je ok 2");
        //zkusim najit jestli stejny reward uz nemas abych ho kdyztak jen zvedl
        for (const pendingReward of this.pendingRewards) {
          if (pendingReward.recurrenceInGameDays > -1 && pendingReward.rewards.length == 1 && pendingReward.rewards[0].content!.itemId == _perkDefinition.rewards[0].content!.itemId) {
            //nasel sem ze uz mas stejny typ recuring rewardu, jen zvednu mnostvi teda..
            pendingReward.rewards[0].content!.amount += _perkDefinition.rewards[0].content!.amount;
            return;
          }
        }
        console.log("je ok 3");
        //pokud to byl recuring a jeste ho nemas, vytvorim jej jako prvniho zastupce
        this.pendingRewards.push(new PendingReward(firestoreAutoId(), _perkDefinition.rarity, _perkDefinition.rewards, _perkDefinition.rewardsRandomEquip, _perkDefinition.rewardsGenerated, _perkDefinition.isInstantReward, _perkDefinition.recurrenceInGameDays, _perkDefinition.rewardAtSpecificGameDay, _perkDefinition.rewardAfterSpecificGameDay, _perkDefinition.charges, -1, 0));
        return;
      }
    }
    console.log("neni, divne");
    //pokud to neni recurring, dam ti ho hned do inventory
    let reward = new PendingReward(firestoreAutoId(), _perkDefinition.rarity, _perkDefinition.rewards, _perkDefinition.rewardsRandomEquip, _perkDefinition.rewardsGenerated, _perkDefinition.isInstantReward, _perkDefinition.recurrenceInGameDays, _perkDefinition.rewardAtSpecificGameDay, _perkDefinition.rewardAfterSpecificGameDay, _perkDefinition.charges, -1, 0);

    reward.resolve(this, 1, _skillDefinitions);
    //  this.pendingRewards.push(new PendingReward(firestoreAutoId(), _perkDefinition.rarity, _perkDefinition.rewards, _perkDefinition.rewardsRandomEquip, _perkDefinition.rewardsGenerated, _perkDefinition.isInstantReward, _perkDefinition.recurrenceInGameDays, _perkDefinition.rewardAtSpecificGameDay, _perkDefinition.rewardAfterSpecificGameDay, _perkDefinition.charges, -1, 0));

  }

  removeFoodEffectsAll() {

    if (this.foodEffects)
      this.foodEffects.forEach(foodEffect => {
        switch (foodEffect.id) {
          case FOOD_EFFECT.INCREASE_MAX_HP:
            this.stats.otherBonusHealth += foodEffect.count;
            break;
          case FOOD_EFFECT.INCREASE_MAX_MANA:
            this.stats.otherBonusMana += foodEffect.count;
            break;
          case FOOD_EFFECT.INCREASE_MANA_REGEN:
            this.stats.otherBonusManaRegen += foodEffect.count;
            break;
          case FOOD_EFFECT.INCREASE_CRIT_CHANCE:
            this.stats.otherBonusCritChance += foodEffect.count;
            break;
          case FOOD_EFFECT.INCREASE_RESISTENCE:
            this.stats.otherBonusResistence += foodEffect.count;
            break;

          default:
            break;
        }
      });

    this.recalculateCharacterStats();

    this.foodEffects = [];
  }

  addFoodEffect(_foodEffect: SimpleTally) {

    this.foodEffects.push(_foodEffect);

    switch (_foodEffect.id) {
      case FOOD_EFFECT.INCREASE_MAX_HP:
        this.stats.otherBonusHealth += _foodEffect.count;
        break;
      case FOOD_EFFECT.INCREASE_MAX_MANA:
        this.stats.otherBonusMana += _foodEffect.count;
        break;

      default:
        break;
    }

    this.recalculateCharacterStats();
  }


  removeRandomCurse(_count: number) {

    for (let index = 0; index < _count; index++) {
      if (this.curses.length > 0) {
        this.curses.splice(randomIntFromInterval(0, this.curses.length - 1), 1);
      }

    }

  }
  getStockPurchasedForGivenVendorGood(_vendorId: string, _vendorGoodId: string): number {

    //this.vendorInfo.find(info => info.vendorId == _vendorId )
    // Find the VendorInfo entry for the given vendorId
    const vendor = this.vendorInfo.find(v => v.vendorId === _vendorId);

    // If the vendor exists, find the good by its id and return its amount
    if (vendor) {
      const good = vendor.goodsPurchased.find(g => g.itemId === _vendorGoodId);
      if (good) {
        return good.amount;
      }
    }

    // Return 0 if vendor or good is not found
    return 0;
  }

  addStockPurchasedForGivenVendorGood(_vendorId: string, _vendorGoodId: string, _amount: number) {

    // Find the VendorInfo entry for the given vendorId
    let vendor = this.vendorInfo.find(v => v.vendorId === _vendorId);

    // If the vendor exists
    if (vendor) {
      // Find the good by its id
      let good = vendor.goodsPurchased.find(g => g.itemId === _vendorGoodId);

      // If the good exists, increment its amount
      if (good) {
        good.amount += _amount;
      }
      // Else, add the new good with the given amount
      else {
        vendor.goodsPurchased.push({
          itemId: _vendorGoodId,
          amount: _amount
        });
      }
    }
    // If vendor does not exist, create a new vendor entry and add the good
    else {
      const newVendorInfo = new VendorInfo(_vendorId, [
        {
          itemId: _vendorGoodId,
          amount: _amount
        }
      ]);
      this.vendorInfo.push(newVendorInfo);
    }
  }

  addChapelInfo(_worldPosition: WorldPosition, _blessId: string) {

    for (const info of this.chapelInfo) {
      if (compareWorldPosition(info.worldPosition, _worldPosition)) {
        throw "Already have chapelInfo for this postion! " + _worldPosition;
      }
    }

    this.chapelInfo.push(new ChapelInfo(_worldPosition, _blessId, false));
  }

  getChapelInfo(_worldPosition: WorldPosition): ChapelInfo | null {

    for (const info of this.chapelInfo) {
      if (compareWorldPosition(info.worldPosition, _worldPosition)) {
        return info;
      }
    }

    return null;
  }

  addBless(_blessId: string) {

    if (!this.blesses.includes(_blessId)) {
      this.blesses.push(_blessId);
      console.log("pridavam bless: " + _blessId);

      if (_blessId == BLESS.BEHEMOND) {
        this.stats.baseHealth += 30;
        this.recalculateCharacterStats();
      }
      else if (_blessId == BLESS.GLASS_CANNON) {
        this.stats.baseHealth -= 60;
        //        this.stats.baseResistence = -5;
        this.stats.baseManaRegen += 1;
        this.stats.baseMana += 1;
        this.recalculateCharacterStats();
      }
      else if (_blessId == BLESS.FOOD_LIMIT_INCREASE) {
        this.stats.restFoodLimit++;
      }
      else if (_blessId == BLESS.ASSASSIN) {
        this.stats.baseHealth -= 30;
        this.stats.baseCritChance = 5;
        this.recalculateCharacterStats();
      }



    }
    else
      throw "You already have " + _blessId + " bless!";
  }

  hasBless(_blessId: string): boolean {
    return (this.blesses.includes(_blessId));
  }


  hasCurse(_curseId: string): boolean {
    throw ("WTF toto je kravina ne?");
    return (this.curses.find(curse => curse.skillId == SKILL.CURSE_MANA_COST_INCREASE) != undefined)
  }

  addCurse(_curseId: Combatskill) {
    this.curses.push(_curseId);
  }

  hasExploredPosition(_worldPosition: WorldPosition): boolean {

    for (const position of this.exploredPositions) {
      if (position.locationId == _worldPosition.locationId &&
        position.zoneId == _worldPosition.zoneId &&
        position.pointOfInterestId == _worldPosition.pointOfInterestId
      ) {
        return true;

      }
    }

    return false;
  }

  updateMemmoryMap(_pointOfInterest: PointOfInterest) {

    //updatneme memmoryMapu
    let memmoryEntry = this.worldMapMemmory.find((memmory: WorldMapMemmoryEntry) => memmory.worldPosition.locationId == _pointOfInterest.worldPosition.locationId && memmory.worldPosition.zoneId == _pointOfInterest.worldPosition.zoneId && memmory.worldPosition.pointOfInterestId == _pointOfInterest.worldPosition.pointOfInterestId);
    //manualne pridam do specials vendory a quetsgivery
    let memmorySpecials = _pointOfInterest.specials;
    console.log("memmorySpecials mnozstvi : " + memmorySpecials.length);
    for (let i = 0; i < _pointOfInterest.vendors.length; i++) {
      console.log("pridavam vendora " + i);
      memmorySpecials.push(POI_SPECIALS.VENDOR);
    }

    for (let i = 0; i < _pointOfInterest.questgivers.length; i++) {
      if (!this.questgiversClaimed.includes(_pointOfInterest.questgivers[i].id)) {
        memmorySpecials.push(POI_SPECIALS.QUESTGIVER);
        break;
      }
    }


    if (memmoryEntry != undefined)
      memmoryEntry.specialPointsOfInterest = memmorySpecials;
    else {
      if (_pointOfInterest.monsters != null)
        this.worldMapMemmory.push(new WorldMapMemmoryEntry(_pointOfInterest.worldPosition, memmorySpecials, _pointOfInterest.typeId, _pointOfInterest.monsters.tiersTotal, _pointOfInterest.roomType, _pointOfInterest.monsters.partySize));
      else
        this.worldMapMemmory.push(new WorldMapMemmoryEntry(_pointOfInterest.worldPosition, memmorySpecials, _pointOfInterest.typeId, 0, _pointOfInterest.roomType, 0));
    }
  }

  //vraci kolik HP jsem "overhealnul"
  giveHealth(_amount: number): number {

    this.stats.currentHealth += _amount;

    let currentMaxHealth = this.stats.totalMaxHealth - this.stats.healthBlockedByFatigue;

    if (this.stats.currentHealth > currentMaxHealth) {
      let overhealAmount = this.stats.currentHealth - currentMaxHealth;
      this.stats.currentHealth = currentMaxHealth
      return overhealAmount;
    }
    else
      return 0;

  }

  learnRecipe(_recipe: Content) {
    //manuscript 
    // strings - 0 - recipe to learn
    // simpleTally - 0 - professionId  : professionAmountNeeded


    if (this.craftingRecipesUnlocked.includes(_recipe.customData!.strings![0])) {
      throw ("You have already learnt recipe  :" + _recipe.customData!.strings![0]);
    }
    else
      this.craftingRecipesUnlocked.push(_recipe.customData!.strings![0]);

    // let profession = this.getProfessionById(_recipe.customData!.simpleTally![0].id);

    // if (profession == null)
    //   throw ("You dont have profession :" + _recipe.customData!.simpleTally![0].id);
    // else if (profession.count < _recipe.customData!.simpleTally![0].count) {
    //   throw ("Not enough profession skill :" + _recipe.customData!.simpleTally![0].count + " needed, you have : " + profession.count);
    // }
    // else if (this.craftingRecipesUnlocked.includes(_recipe.customData!.strings![0])) {
    //   throw ("You have already learnt recipe  :" + _recipe.customData!.strings![0]);
    // }
    // else {
    //   this.craftingRecipesUnlocked.push(_recipe.customData!.strings![0]);
    // }


  }

  addProfession(_professionId: string, _maxProfession: number) {
    if (this.getProfessionById(_professionId) == null) {
      switch (_professionId) {
        case PROFESSION.ALCHEMY:
          this.craftingRecipesUnlocked.push(ITEMS.HARDENING_OIL_RECIPE);
          break;
        // case PROFESSION.BLACKSMITHING:
        //   this.craftingRecipesUnlocked.push(ITEMS.FORGE_ALLOY_BASE_RECIPE);
        //   break;
        case PROFESSION.HERBALISM:
          this.craftingRecipesUnlocked.push(ITEMS.MARIGOLD_INFUSION_RECIPE);
          break;
        case PROFESSION.MINING:
          this.craftingRecipesUnlocked.push(ITEMS.COPPER_INGOT_RECIPE);
          break;

        default:
          break;
      }

      this.professions.push(new SimpleTallyWithMax(_professionId, 0, _maxProfession));
    }
  }

  getProfessionById(_professionId: string): SimpleTallyWithMax | undefined {

    for (const profession of this.professions) {
      if (profession.id == _professionId) {
        return profession;
      }
    }

    return undefined;
  }



  increaseProfessionSkill(_professionId: string, _amount: number) {

    let prof = this.getProfessionById(_professionId);
    if (prof != null) {
      if (prof.countMax <= prof.count + _amount)
        prof.count = prof.countMax;
      else
        prof.count += _amount;
    }
  }

  grantStatsForLevelUp() {

    switch (this.characterClass) {
      case CHARACTER_CLASS.WARRIOR:
        {
          this.stats.baseHealth += 5;
          this.stats.totalMaxHealth += 5;
          break;
        }
      case CHARACTER_CLASS.SHAMAN:
        {
          this.stats.baseHealth += 3;
          this.stats.totalMaxHealth += 3;
          break;
        }
      case CHARACTER_CLASS.WARLOCK:
        {
          this.stats.baseHealth += 3;
          this.stats.totalMaxHealth += 3;
          break;
        }
      default:
        {
          this.stats.baseHealth += 3;
          this.stats.totalMaxHealth += 3;
          break;
        }
    }
    // switch (this.characterClass) {
    //   case CHARACTER_CLASS.WARRIOR:
    //     this.stats.intellect += 0
    //     this.stats.agility += 0
    //     this.stats.stamina += this.stats.level % 3 != 0 ? 1 : 0; //pouze kazdy lichy level
    //     this.stats.strength += this.stats.level % 3 == 0 ? 1 : 0; //pouze kazdy sudy level
    //     // this.stats.spirit += this.stats.level % 4 == 0 ? 1 : 0; //pouze kazdy 4 level
    //     break;
    //   case CHARACTER_CLASS.WARLOCK:
    //     this.stats.intellect += this.stats.level % 2 != 0 ? 1 : 0;
    //     this.stats.agility += 0
    //     this.stats.stamina += this.stats.level % 4 == 0 ? 1 : 0;
    //     this.stats.strength += 0
    //     //  this.stats.spirit += this.stats.level % 4 == 0 ? 1 : 0;
    //     break;
    //   case CHARACTER_CLASS.SHAMAN:
    //     this.stats.intellect += this.stats.level % 3 != 0 ? 1 : 0;
    //     this.stats.agility += 0
    //     this.stats.stamina += this.stats.level % 5 == 0 ? 1 : 0;
    //     this.stats.strength += this.stats.level % 5 == 0 ? 1 : 0;
    //     //  this.stats.spirit += this.stats.level % 5 == 0 ? 1 : 0;
    //     break;


    //   default:
    //     this.stats.intellect += 0;
    //     this.stats.agility += 0;
    //     this.stats.stamina += 0;
    //     this.stats.strength += 0;
    //     //  this.stats.spirit += 0;
    //     break;
    // }

  }

  incrementMaxTierReachedForPointOfInterest(_pointOfIterestId: string) {

    for (const pointOfInterest of this.pointsOfInterestMaxTierReached) {
      if (pointOfInterest.id == _pointOfIterestId) {
        pointOfInterest.count++;
        return;
      }
    }

    //pokud nenajdeme dany zaznam, vytvorime ho a dame tier reached na 0
    this.pointsOfInterestMaxTierReached.push(new SimpleTally(_pointOfIterestId, 0));

    // throw ("could not find entry of max reach tier for PoI :" + _pointOfIterestId);
  }

  getMaxTierReachedForPointOfInterest(_pointOfIterestId: string): number {

    for (const pointOfInterest of this.pointsOfInterestMaxTierReached) {
      if (pointOfInterest.id == _pointOfIterestId) {
        return pointOfInterest.count;
      }
    }

    //pokud nenajdeme dany zaznam, vytvorime ho a dame tier reached na -1
    this.pointsOfInterestMaxTierReached.push(new SimpleTally(_pointOfIterestId, -1));
    return -1;
  }

  getNumberOfItemsInInventory(_itemId: string): number {
    let itemCount = 0;
    this.inventory.content.forEach(content => {
      if (content.getItem().itemId == _itemId)
        itemCount += content.getItem().amount;
    });
    return itemCount;
  }



  isOnSameWorldPosition(_position: WorldPosition): boolean {
    return (_position.locationId == this.position.locationId && _position.zoneId == this.position.zoneId)

  }

  recordMonsterKill(_monsterId: string) {

    for (const element of this.monsterKills) {
      if (element.id == _monsterId) {
        element.count++;
        console.log("added record  for  : " + _monsterId + " total kills now : " + element.count);
        return;
      }
    }

    console.log("didnt find entry for  : " + _monsterId);
    this.monsterKills.push(new SimpleTally(_monsterId, 1));
    // }
  }



  // upgradeEquipmentQuality(_equipToUpgradeUid: string) {

  //   // let equipToUpgrade: Equip | undefined;

  //   // for (const equip of this.equipment) {
  //   //   if (equip.uid == _equipToUpgradeUid) {

  //   //     equipToUpgrade = equip;
  //   //     break;
  //   //   }
  //   // }

  //   // if (equipToUpgrade == undefined) {
  //   //   for (const content of this.inventory.content) {
  //   //     if (content.getItem().uid == _equipToUpgradeUid) {
  //   //       equipToUpgrade = content.contentEquip;
  //   //       break;
  //   //     }
  //   //   }
  //   // }

  //   // if (equipToUpgrade == undefined)
  //   //   throw ("Could not find equip :" + _equipToUpgradeUid + " for upgrade");


  //   // if (equipToUpgrade.quality == equipToUpgrade.qualityMax)
  //   //   throw ("Equip :" + _equipToUpgradeUid + " is already at maximum quality");

  //   this.getEquipFromInventoryOfEquipment(_equipToUpgradeUid).quality++;
  //  // equipToUpgrade.quality++;

  // }

  recalculateCharacterStats() {

    this.stats.totalMaxHealth = this.stats.baseHealth + this.stats.equipBonusHealth + this.stats.otherBonusHealth;
    this.stats.totalMaxMana = this.stats.baseMana + this.stats.equipBonusMana + this.stats.otherBonusMana;

    this.stats.totalManaRegen = this.stats.baseManaRegen + this.stats.otherBonusManaRegen + this.stats.equipBonusManaRegen;
    this.stats.totalCritChance = this.stats.baseCritChance + this.stats.otherBonusCritChance + this.stats.equipBonusCritChance;
    this.stats.totalDamagePower = this.stats.baseDamagePower + this.stats.otherBonusDamagePower + this.stats.equipBonusDamagePower;
    this.stats.totalDefense = this.stats.baseDefense + this.stats.otherBonusDefense + this.stats.equipBonusDefense;
    this.stats.totalResistence = this.stats.baseResistence + this.stats.otherBonusResistence + this.stats.equipBonusResistence;
    this.stats.totalHealthRegen = this.stats.baseHealthRegen + this.stats.otherBonusHealthRegen + this.stats.equipBonusHealthRegen;


    if (this.stats.currentHealth > this.stats.totalMaxHealth)
      this.stats.currentHealth = this.stats.totalMaxHealth;


    //jeste musim zakomponovat fatiigue

    //prepocitam kolik HP je blokovano fatigue
    // console.log("this.stats.totalMaxHealth:" + this.stats.totalMaxHealth);
    // this.stats.healthBlockedByFatigue = this.stats.totalMaxHealth - Math.round(this.stats.totalMaxHealth * ((100 - this.currency.fatigue) / 100));
    //console.log("healthBlockedByFatigue:" + this.stats.healthBlockedByFatigue);

    //musim snizit currentHealth pokud je vetsi nez kolik dovoli nve fatigue limit
    const maxHpWithFatiguePenalty = this.stats.totalMaxHealth - this.stats.healthBlockedByFatigue;
    if (this.stats.currentHealth > maxHpWithFatiguePenalty)
      this.stats.currentHealth = maxHpWithFatiguePenalty;
  }



  equipEquipment(_equipToEquipUids: string[]) {

    let equipToEquip: Equip[] = [];

    //Vsechen equip co mam na sobe dam do inventare
    this.equipment.forEach(equip => {
      this.addContentToInventory(generateContentContainer(equip), false, true)

      //zrusime bag size rare effekty
      equip.rareEffects.forEach(rareEffect => {
        if (rareEffect.id == RARE_EFFECT.BAG_SIZE) {
          this.inventory.capacityMax -= rareEffect.amount;
        }
      });

    });

    //Ulozim equip ktery si chci equiupnout
    for (const content of this.inventory.content) {
      console.log("checkuju jesti si tento equip chci nasadit:  " + content.getItem().uid);
      if (_equipToEquipUids.includes(content.getItem().uid)) {


        if ((content.getItem() as Equip).level > this.stats.level)
          throw (content.getItem() as Equip).displayName + " level is too high!";

        //nastavim equipu ze uz jej nekdo equipnul
        (content.getItem() as Equip).neverEquiped = false;

        equipToEquip.push(content.getItem() as Equip);
        console.log("jo chci tak pushuju: " + content.getItem().uid + " " + (content.getItem() as Equip).equipSlotId);

        //pridame bag size rare effekty
        (content.getItem() as Equip).rareEffects.forEach(rareEffect => {
          if (rareEffect.id == RARE_EFFECT.BAG_SIZE) {
            console.log("Pridavam bag size : " + rareEffect.amount);
            this.inventory.capacityMax += rareEffect.amount;
          }
        });


      }
    }



    //Smazu z inventare equip ktey sem si nasadil
    const itemsCount = this.inventory.content.length - 1;
    for (var i = itemsCount; i >= 0; i--) {
      if (_equipToEquipUids.includes(this.inventory.content[i].getItem().uid)) {
        this.inventory.content.splice(i, 1);
      }
    }

    //a equipnu si ho
    this.equipment.splice(0, this.equipment.length);
    Object.assign(this.equipment, equipToEquip);

    this.inventory.capacityLeft = this.inventory.capacityMax - this.inventory.content.length;



    //sectu rare efekty z equipu a zatim updatnu teda jen max health, ostatni netreba pro character sheet v UI
    //TODO :jako mohl bych skoncit u totho ze tady scitam vsechny rare staty do agregovanych nejakych property, ale to by uz character musel teda mit pro kazdy rare efekt
    //i svuj atribud kde ulozi agregovanou hodnotu, ted to planuju mit az v combat statech u encounteru ale mozna by to davalo symsl to mit uz tady?
    let maxHealthBonus: number = 0;
    let maxManaBonus: number = 0;
    let regenManaBonus: number = 0;
    let critBonus: number = 0;
    let damagePowerBonus: number = 0;
    let defenseBonus: number = 0;
    let resistenceBonus: number = 0;
    let regenHpBonus: number = 0;

    this.equipment.forEach(_equip => {
      _equip.rareEffects.forEach(rareEffect => {
        switch (rareEffect.id) {
          case RARE_EFFECT.MAX_HEALTH:
            maxHealthBonus += rareEffect.amount;
            break;
          case RARE_EFFECT.MAX_MANA:
            maxManaBonus += rareEffect.amount;
            break;
          case RARE_EFFECT.MANA_REGEN:
            regenManaBonus += rareEffect.amount;
            break;
          case RARE_EFFECT.CRIT_CHANCE:
            critBonus += rareEffect.amount;
            break;
          case RARE_EFFECT.DAMAGE_POWER:
            damagePowerBonus += rareEffect.amount;
            break;
          case RARE_EFFECT.DEFENSE:
            defenseBonus += rareEffect.amount;
            break;
          case RARE_EFFECT.RESISTANCE:
            resistenceBonus += rareEffect.amount;
            break;
          case RARE_EFFECT.HP_REGEN:
            regenHpBonus += rareEffect.amount;
            break;

          // case RARE_EFFECT.BAG_SIZE:
          //   toto stejne nefacha nejaky to debugnout dela to null, proc?
          //   this.inventory.capacityMax += rareEffect.amount;
          //   break;

          default:
            break;
        }
      });
    });


    this.stats.equipBonusHealth = maxHealthBonus;
    this.stats.equipBonusMana = maxManaBonus;
    this.stats.equipBonusManaRegen = regenManaBonus;
    this.stats.equipBonusCritChance = critBonus;
    this.stats.equipBonusDamagePower = damagePowerBonus;
    this.stats.equipBonusDefense = defenseBonus;
    this.stats.equipBonusResistence = resistenceBonus;
    this.stats.equipBonusDamagePower = damagePowerBonus;
    this.stats.equipBonusHealthRegen = regenHpBonus;

    this.recalculateCharacterStats();
    // this.stats.totalMaxHealth = this.stats.baseHealth + maxHealthBonus;
    // if (this.stats.currentHealth > this.stats.totalMaxHealth)
    //   this.stats.currentHealth = this.stats.totalMaxHealth;


    // this.stats.totalMaxMana = this.stats.baseMana + maxManaBonus;
    // this.stats.totalManaRegen = this.stats.baseManaRegen + regenManaBonus;

    // //jeste musim zakomponovat fatiigue

    // //prepocitam kolik HP je blokovano fatigue
    // // console.log("this.stats.totalMaxHealth:" + this.stats.totalMaxHealth);
    // this.stats.healthBlockedByFatigue = this.stats.totalMaxHealth - Math.round(this.stats.totalMaxHealth * ((100 - this.currency.fatigue) / 100));
    // // console.log("healthBlockedByFatigue:" + this.stats.healthBlockedByFatigue);
    // //musim snizit currentHealth pokud je vetsi nez kolik dovoli nve fatigue limit

    // const maxHpWithFatiguePenalty = this.stats.totalMaxHealth - this.stats.healthBlockedByFatigue;
    // if (this.stats.currentHealth > maxHpWithFatiguePenalty)
    //   this.stats.currentHealth = maxHpWithFatiguePenalty;



  }

  getInventoryContent(_equipToGetUid: string): ContentContainer {

    let foundContent: ContentContainer | null = null;

    for (var i = this.inventory.content.length - 1; i >= 0; i--) {
      if (_equipToGetUid == this.inventory.content[i].getItem().uid) {
        foundContent = this.inventory.content[i];
      }
    }

    if (foundContent == null)
      throw ("getInventoryContent : Could not find content with UID :" + _equipToGetUid + " in your inventory!");

    return foundContent;
  }


  getEquipFromInventoryOfEquipment(_equipToGetUid: string): Equip {

    let equipToReturn: Equip | undefined;

    for (const equip of this.equipment) {
      if (equip.uid == _equipToGetUid) {

        equipToReturn = equip;
        break;
      }
    }

    if (equipToReturn == undefined) {
      for (const content of this.inventory.content) {
        if (content.getItem().uid == _equipToGetUid) {
          equipToReturn = content.contentEquip;
          break;
        }
      }
    }

    if (equipToReturn == undefined)
      throw ("Could not find equip :" + _equipToGetUid + " for upgrade");


    if (equipToReturn.quality == equipToReturn.qualityMax)
      throw ("Equip :" + _equipToGetUid + " is already at maximum quality");

    return equipToReturn;

  }


  removeContentFromInventory(_contentToRemoveUid: string, _amount: number) {
    let foundCountent = false;
    // console.log("_contentToRemoveUid : " + _contentToRemoveUid);
    for (var i = this.inventory.content.length - 1; i >= 0; i--) {
      //  console.log("checking : " + this.inventory.content[i].getItem().uid);
      if (_contentToRemoveUid == this.inventory.content[i].getItem().uid) {
        //   console.log("found : " + this.inventory.content[i].getItem().uid);
        if (_amount == -1)
          _amount = this.inventory.content[i].getItem().amount;

        if (this.inventory.content[i].getItem().amount < _amount)
          throw ("Not enough of UID :" + _contentToRemoveUid + " in your inventory! You have : " + this.inventory.content[i].getItem().amount + " but you want to remove " + _amount);

        this.inventory.content[i].getItem().amount -= _amount;

        if (this.inventory.content[i].getItem().amount == 0) {
          this.inventory.content.splice(i, 1);
          this.inventory.capacityLeft++;
        }
        foundCountent = true;
        break;
      }
    }

    if (!foundCountent)
      throw ("removeContentFromInventory : Could not find content with UID :" + _contentToRemoveUid + " in your inventory!");
  }


  removeContentFromInventoryById(_contentToRemoveId: string, _amountToRemove: number) {

    let amoutOfItemsLeftToRemove = _amountToRemove;
    for (var i = this.inventory.content.length - 1; i >= 0; i--) {
      if (_contentToRemoveId == this.inventory.content[i].getItem().itemId) {

        //v tom stacku je dost itemu na remove co potrebuju
        if (amoutOfItemsLeftToRemove <= this.inventory.content[i].getItem().amount) {
          this.inventory.content[i].getItem().amount -= amoutOfItemsLeftToRemove;
          amoutOfItemsLeftToRemove = 0;
        }
        else {   //v tom stacku neni dost itemu na remove co potrebuju
          amoutOfItemsLeftToRemove -= this.inventory.content[i].getItem().amount;
          this.inventory.content[i].getItem().amount = 0;

        }
        //  this.inventory.content[i].getItem().amount -= _amount;

        if (this.inventory.content[i].getItem().amount == 0) {
          this.inventory.content.splice(i, 1);
          this.inventory.capacityLeft++;
        }
        if (amoutOfItemsLeftToRemove == 0)
          break;
      }
    }

    if (amoutOfItemsLeftToRemove > 0)
      throw ("You dont have enought " + _contentToRemoveId + " items in your inventory! You need : " + _amountToRemove + " but you have only: " + (_amountToRemove - amoutOfItemsLeftToRemove));
  }




  addContentToInventory(_contentToAdd: ContentContainer, _generateNewUid: boolean, _ignoreCapacity: boolean) { //defakto vzdycky chci generovat novy Uid,jedine kdyz swapuju itemy mezi inventarem a charakterem a ocekavam ze ty itemy zas najdu podle id tak davam false

    //Hack abych ziskal pristup k meetodatm BaseContentu bez toho abych musel pouzivat WithConvertor
    _contentToAdd = new ContentContainer(_contentToAdd.content, _contentToAdd.contentEquip);


    console.log("Davam item : " + _contentToAdd.getItem().itemId);
    //pokud je ten content co chci dat do inventare currency, tak to nedam do inventare ale zvednu hraci jeho cucrrency
    if (_contentToAdd.getItem().contentType == CONTENT_TYPE.CURRENCY) {
      console.log("Mam tu nejakou currency : " + _contentToAdd.getItem().itemId);
      this.addCurrency(_contentToAdd.getItem().itemId, _contentToAdd.getItem().amount);
      return;
    }


    let itemAddedToExistingStack: boolean = false;

    for (const content of this.inventory.content) {
      if (content.getItem().itemId == _contentToAdd.getItem().itemId) {
        if (content.getItem().stackSize - content.getItem().amount >= _contentToAdd.getItem().amount) {
          content.getItem().amount += _contentToAdd.getItem().amount;
          itemAddedToExistingStack = true;
          console.log("Existing stack of " + _contentToAdd.getItem().itemId + " increased!");
          break;
        }
      }

    }
    if (!itemAddedToExistingStack) {
      if (this.inventory.capacityLeft > 0 || _ignoreCapacity) {
        if (_generateNewUid)
          _contentToAdd.getItem().uid = firestoreAutoId(); // TOHLE podle me je dobre delat vzdy a nicemu to nevadi. Kdovi co je to za item a jake ma uid, jestli originalni nebo ne (napriklad itemy koupene od vendroa maji vsechny stejny uid a nekde se novy musi vygenerovat)

        this.inventory.content.push(new ContentContainer(_contentToAdd.content, _contentToAdd.contentEquip));
        this.inventory.capacityLeft--;
        console.log("Item " + _contentToAdd.getItem().itemId + " added to inventory!");
      }
      else
        throw "Cant add new simple item " + _contentToAdd.getItem().itemId + " inventory is full!";
    }
  }

  addCurrency(_currencyType: string, _amount: number) {
    if (_amount == undefined) {
      throw "Data error! currency amount is null! Try again later!";
    }
    switch (_currencyType) {
      case CURRENCY_ID.GOLD: this.addGold(_amount); break;
      case CURRENCY_ID.MONSTER_ESSENCE: this.addMonsterEssence(_amount); break;
      case CURRENCY_ID.TIME: this.addTime(_amount); break;
      case CURRENCY_ID.FATIGUE: this.addFatigueFlat(_amount); break;
      case CURRENCY_ID.TRAVEL_POINTS: this.addTravelPoints(_amount); break;
      case CURRENCY_ID.SCAVENGE_POINTS: this.addScavengePoints(_amount); break;
      default:
        break;
    }
  }

  subCurrency(_currencyType: string, _amount: number) {
    if (_amount == undefined) {
      throw "Data error! currency amount is null! Try again later!";
    }
    switch (_currencyType) {
      case CURRENCY_ID.GOLD: this.subGold(_amount); break;
      case CURRENCY_ID.MONSTER_ESSENCE: this.subMonsterEssence(_amount); break;
      case CURRENCY_ID.TIME: this.subTime(_amount); break;
      case CURRENCY_ID.FATIGUE: this.subFatigueFlat(_amount); break;
      case CURRENCY_ID.TRAVEL_POINTS: this.subTravelPoints(_amount); break;
      case CURRENCY_ID.SCAVENGE_POINTS: this.subScavengePoints(_amount); break;

      default:
        break;
    }
  }

  // subSilver(_amount: number) {
  //   if (this.currency.silver >= _amount)
  //     this.currency.silver -= _amount;
  //   else
  //     throw "Not enough Silver! You have " + this.currency.silver + " but want to subsctract " + _amount;
  // }


  subGold(_amount: number) {
    if (this.currency.gold >= _amount)
      this.currency.gold -= _amount;
    else
      throw "Not enough Gold! You have " + this.currency.gold + " but want to subsctract " + _amount;

    this.currency.gold = Math.round(this.currency.gold);
  }



  subMonsterEssence(_amount: number) {
    if (this.currency.monsterEssence >= _amount)
      this.currency.monsterEssence -= _amount;
    else
      throw "Not enough Monster Essence! You have " + this.currency.monsterEssence + " but want to subsctract " + _amount;

    this.currency.monsterEssence = Math.round(this.currency.monsterEssence);
  }

  subTime(_amount: number) {
    if (this.currency.time >= _amount)
      this.currency.time -= _amount;
    else
      throw "Not enough Time! You have " + this.currency.time + " but want to subsctract " + _amount;
  }

  subTravelPoints(_amount: number) {
    if (this.currency.travelPoints >= _amount)
      this.currency.travelPoints -= _amount;
    else
      throw "Not enough TravelPoints! You have " + this.currency.travelPoints + " but want to subsctract " + _amount;
  }


  subScavengePoints(_amount: number) {
    if (this.currency.scavengePoints >= _amount)
      this.currency.scavengePoints -= _amount;
    else
      throw "Not enough Scavenge Points! You have " + this.currency.scavengePoints + " but want to subsctract " + _amount;
  }


  subFatiguePercentage(_percentageInDecimalFormat: number) {
    // if (this.currency.fatigue >= _amount)
    let amountToTake = Math.round(this.stats.totalMaxHealth * ((_percentageInDecimalFormat) / 100));
    this.subFatigueFlat(amountToTake);

  }

  subFatigueFlat(_amount: number) {
    // if (this.currency.fatigue >= _amount)
    this.stats.healthBlockedByFatigue -= _amount;


    if (this.stats.healthBlockedByFatigue < 0)
      this.stats.healthBlockedByFatigue = 0;

    // this.stats.healthBlockedByFatigue = this.stats.totalMaxHealth - Math.round(this.stats.totalMaxHealth * ((100 - this.currency.fatigue) / 100));

    //throw "Not enough Time! You have " + this.currency.time + " but want to subsctract " + _amount;
  }


  addFatiguePercentage(_percentageInDecimalFormat: number) {
    this.addFatigueFlat(Math.round(this.stats.totalMaxHealth * ((_percentageInDecimalFormat) / 100)));

    // this.stats.healthBlockedByFatigue = this.stats.totalMaxHealth - Math.round(this.stats.totalMaxHealth * ((100 - this.currency.fatigue) / 100));
    // console.log("fatigue ted blokuje " + this.stats.healthBlockedByFatigue + " healtu");


    //musim snizit currentHealth pokud je vetsi nez kolik dovoli nve fatigue limit
    // const maxHpWithFatiguePenalty = this.stats.totalMaxHealth - this.stats.healthBlockedByFatigue;
    // console.log("max healthu kdyz odectu fatiguje je teda " + maxHpWithFatiguePenalty);
    // if (this.stats.currentHealth > maxHpWithFatiguePenalty) {
    //   console.log("a tyy mas vic current health : " + this.stats.currentHealth + " nez kolik je prave max healthu s fatigue dovolebych, tak curent helath nastavim na to max hp s fatigue penalty");
    //   this.stats.currentHealth = maxHpWithFatiguePenalty;
    // }

  }

  addFatigueFlat(_amount: number) {
    console.log("pridavas " + _amount + "/ fatigue");
    if (_amount + this.stats.healthBlockedByFatigue >= this.stats.totalMaxHealth)
      this.stats.healthBlockedByFatigue = this.stats.totalMaxHealth - 1;
    else
      this.stats.healthBlockedByFatigue += _amount;

    // this.stats.healthBlockedByFatigue = this.stats.totalMaxHealth - Math.round(this.stats.totalMaxHealth * ((100 - this.currency.fatigue) / 100));
    // console.log("fatigue ted blokuje " + this.stats.healthBlockedByFatigue + " healtu");


    //musim snizit currentHealth pokud je vetsi nez kolik dovoli nve fatigue limit
    // const maxHpWithFatiguePenalty = this.stats.totalMaxHealth - this.stats.healthBlockedByFatigue;
    // console.log("max healthu kdyz odectu fatiguje je teda " + maxHpWithFatiguePenalty);
    // if (this.stats.currentHealth > maxHpWithFatiguePenalty) {
    //   console.log("a tyy mas vic current health : " + this.stats.currentHealth + " nez kolik je prave max healthu s fatigue dovolebych, tak curent helath nastavim na to max hp s fatigue penalty");
    //   this.stats.currentHealth = maxHpWithFatiguePenalty;
    // }

  }

  getMaxHealthWithFatigueBlocked(): number {
    return this.stats.totalMaxHealth - this.stats.healthBlockedByFatigue;
  }

  addScavengePoints(_amount: number) {
    if (_amount + this.currency.scavengePoints > this.currency.scavengePointsMax)
      this.currency.scavengePoints = this.currency.scavengePointsMax;
    else
      this.currency.scavengePoints += _amount;
  }

  addTravelPoints(_amount: number) {
    if (_amount + this.currency.travelPoints > this.currency.travelPointsMax)
      this.currency.travelPoints = this.currency.travelPointsMax;
    else
      this.currency.travelPoints += _amount;
  }

  addTime(_amount: number) {
    if (_amount + this.currency.time > this.currency.timeMax)
      this.currency.time = this.currency.timeMax;
    else
      this.currency.time += _amount;
  }


  addMonsterEssence(_amount: number) {
    this.currency.monsterEssence += _amount;
    this.currency.monsterEssence = Math.round(this.currency.monsterEssence);
  }

  addGold(_amount: number) {
    this.currency.gold += _amount;
    this.currency.gold = Math.round(this.currency.gold);
  }

  // addSatoshium(_amount: number) {
  //   this.currency. += _amount;
  // }

  converEquipToCombatSkills(): Combatskill[] {
    let combatSkills: Combatskill[] = [];


    this.equipment.forEach(equip => {
      let convertedSkill = ConvertSkillToCombatSkill(equip.skill, equip.quality);
      if (this.hasBless(BLESS.LASTING_HAND))
        convertedSkill.manaCost++;


      combatSkills.push(convertedSkill);
    });


    let combatSkillsLength = combatSkills.length;

    // skilly z prazdnych equip slotu nahradim "punch" skilem
    for (let index = 0; index < MAX_DECK_SIZE - combatSkillsLength; index++) {
      combatSkills.push(new Combatskill("UNPREPARED", CHARACTER_CLASS.ANY, -1, [], false, firestoreAutoId(), undefined, SKILL_GROUP.UNPREPARED, false, false, false, false, RARITY.COMMON, new CombatSkillOriginalStats(-1, [], null), 0));
    }



    return combatSkills;
  }



  converRareEffectsAndSkillBonusEffectsToCombatStats(): CombatStats {

    //sectu rare efekty

    // let critBonus: number = 0;
    // let hpRegenBonus: number = 0;
    // let regenMana: number = 0;
    // let defense: number = 0;
    // let damagePower: number = 0;
    // let healingPowerTotal: number = 0;
    // let resistanceTotal: number = 0;
    let summedBonusSkillEffects: SkillBonusEffect[] = []
    let summedBonusBuffEffects: BuffBonusEffect[] = []

    let sums: { [id: string]: { skillGroupId: string; amount: number; indexInArray: number, mathOperationType: string } } = {};
    let sumsBuff: { [id: string]: { buffGroupId: string; amount: number; indexInArray: number, mathOperationType: string } } = {};

    this.equipment.forEach(_equip => {

      // Create an intermediate object to sum the amounts and retain the index for each skillGroupId
      _equip.skillBonusEffects.forEach(skill => {

        if (!sums[skill.id]) {
          sums[skill.id] = { skillGroupId: skill.skillGroupId, amount: skill.amount, indexInArray: skill.indexInArray, mathOperationType: skill.mathOperationType };
        } else {
          sums[skill.id].amount += skill.amount;
        }


      });

      _equip.buffBonusEffects.forEach(buff => {

        if (!sumsBuff[buff.id]) {
          sumsBuff[buff.id] = { buffGroupId: buff.buffGroupId, amount: buff.amount, indexInArray: buff.indexInArray, mathOperationType: buff.mathOperationType };
        } else {
          sumsBuff[buff.id].amount += buff.amount;
        }

      });


      //TODO:  PROC TO TU DELAM NA 2 mistech. MAX MANA a HP mam uz sectene a special efekty scitam extra?

      /* _equip.rareEffects.forEach(rareEffect => {
         switch (rareEffect.id) {
           // ZROVNA MAX HEALTH JE SPECIAL A UZ MAM ULOZENY PRI ZMENE EQUIPU
           // case RARE_EFFECT.MAX_HEALTH:  
           //   maxHealthBonus += rareEffect.amounts[0];
           //   break;
           case RARE_EFFECT.CRIT_CHANCE:
             critBonus += rareEffect.amount;
             break;
           case RARE_EFFECT.HP_REGEN:
             hpRegenBonus += rareEffect.amount;
             break;
 
           // case RARE_EFFECT.MAX_MANA:
           //   maxMana += rareEffect.amount;
           //   break;
 
           // case RARE_EFFECT.MANA_REGEN:
           //   regenMana += rareEffect.amount;
           //   break;
 
           case RARE_EFFECT.DEFENSE:
             defense += rareEffect.amount;
             break;
 
           case RARE_EFFECT.DAMAGE_POWER:
             damagePower += rareEffect.amount;
             break;
 
           case RARE_EFFECT.RESISTANCE:
             resistanceTotal += rareEffect.amount;
             break;
 
           case RARE_EFFECT.HEALING_POWER:
             healingPowerTotal += rareEffect.amount;
             break;
 
           default:
             break;
         }
        });
          */
    });


    // Convert this object back to an array
    summedBonusSkillEffects = Object.entries(sums).map(([id, { skillGroupId, amount, indexInArray, mathOperationType }]) => ({ skillGroupId, amount, indexInArray, id, mathOperationType }));
    summedBonusBuffEffects = Object.entries(sumsBuff).map(([id, { buffGroupId, amount, indexInArray, mathOperationType }]) => ({ buffGroupId, amount, indexInArray, id, mathOperationType }));


    // console.log(summedBonusSkillEffects.length + " summedBonusSkillEffects lengt");
    // console.log(summedBonusSkillEffects); // this will print the new array with summed amounts

    //let maxHealth = this.stats.totalMaxHealth;
    //  let maxHealthAfterFatiguePenalty = Math.round((maxHealth / 100) * (100 - this.currency.fatigue));

    let currentHealth = this.stats.currentHealth;

    // if (currentHealth > maxHealthAfterFatiguePenalty)
    //    currentHealth = maxHealthAfterFatiguePenalty;

    let startingMana = this.stats.totalManaRegen;
    if (startingMana > this.stats.totalMaxMana)
      startingMana = this.stats.totalMaxMana;

    return new CombatStats(this.stats.totalMaxMana, startingMana, this.getMaxHealthWithFatigueBlocked(), currentHealth, currentHealth, this.stats.healthBlockedByFatigue, 0, this.stats.totalHealthRegen, this.stats.totalCritChance, this.stats.totalDamagePower, this.stats.totalResistence, this.stats.totalDefense, 0, summedBonusSkillEffects, this.stats.skillDrawCount, summedBonusBuffEffects, this.stats.totalManaRegen);
  }


  giveExp(_amount: number): boolean {

    const characterLevel = this.stats.level;
    this.stats.exp += _amount;

    const Diff = 0;

    let newLevel: number = 0;
    let ExpNeededForGivenLevel = 0;
    for (let level = 0; level < 60; level++) {
      //pridat tady vpocty toho DIFF u level >28
      ExpNeededForGivenLevel += ((8 * level) + Diff) * ((5 * level) + 45);
      if (this.stats.exp < ExpNeededForGivenLevel) {
        newLevel = level;
        console.log("your XP " + this.stats.exp + " is less than " + ExpNeededForGivenLevel + " (level " + (level + 1) + ") therefore your level is " + newLevel + " ");

        break;
      }
    }

    if (newLevel > characterLevel) {
      console.log("LEVEL UP!");
      //you gained a level up!
      this.stats.level = newLevel;
      this.stats.expNeededToReachLastLevel = this.stats.expNeededToReachNextLevel;
      this.stats.expNeededToReachNextLevel = ExpNeededForGivenLevel;
      this.grantStatsForLevelUp();

      // this.subCurrency(CURRENCY_ID.FATIGUE, 100);
      this.addCurrency(CURRENCY_ID.TIME, TIME_BONUS_PER_LEVEL_UP);

      return true;

    }
    return false;

  }



  giveExpFromEncounterResult(_encounterResultToGiveExpFrom: EncounterResult): boolean {

    // const characterLevel = this.stats.level;

    // let TotalEXPReward: number = 0;
    // _encounterResultToGiveExpFrom.enemies.forEach(enemy => {

    //   let EXPFromEnemy = (characterLevel * 5) + 45;

    //   //Odmena za enemy s vyssim level nez jsi ty
    //   if (enemy.level > characterLevel) {
    //     EXPFromEnemy = (EXPFromEnemy) * (1 + 0.05 * (enemy.level - characterLevel));
    //   }
    //   //Penalty za enemy s nizsim level nez jsi ty
    //   else if (enemy.level < characterLevel) {
    //     // ZD = 5, when Char Level = 1 - 7
    //     // ZD = 6, when Char Level = 8 - 9
    //     // ZD = 7, when Char Level = 10 - 11
    //     // ZD = 8, when Char Level = 12 - 15
    //     // ZD = 9, when Char Level = 16 - 19
    //     // ZD = 11, when Char Level = 20 - 29
    //     // ZD = 12, when Char Level = 30 - 39
    //     // ZD = 13, when Char Level = 40 - 44
    //     // ZD = 14, when Char Level = 45 - 49
    //     // ZD = 15, when Char Level = 50 - 54
    //     // ZD = 16, when Char Level = 55 - 59
    //     // ZD = 17, when Char Level = 60 - 79
    //     const ZD = 5; //TODO TOHLE SE MENI S LEVELEM CHARAKTERU
    //     EXPFromEnemy = (EXPFromEnemy) * (1 - (characterLevel - enemy.level) / ZD)
    //   }
    //   console.log("Enemy " + enemy.displayName + "(level " + enemy.level + ") gave you " + EXPFromEnemy);
    //   TotalEXPReward += EXPFromEnemy;
    // });
    let characterLevel = this.stats.level;
    return this.giveExp(getExpAmountFromEncounterForGivenCharacterLevel(_encounterResultToGiveExpFrom.enemies, characterLevel));


    //ZKORNTROLUJU JESTLI SEM NEDOSSTAL LEVEL UP!

    // Diff(CL) =       0,  for    CL <= 28
    // Diff(CL) =       1,     CL  = 29
    // Diff(CL) =       3,     CL  = 30
    // Diff(CL) =       6,     CL  = 31
    // Diff(CL) = 5 x (CL-30), CL >= 32, <=59

  }

}

export class CharacterStats {
  constructor(

    public exp: number,
    public expNeededToReachLastLevel: number,
    public expNeededToReachNextLevel: number,
    public level: number,

    public baseHealth: number,
    public baseMana: number,

    public baseManaRegen: number,
    public baseHealthRegen: number,
    public baseCritChance: number,
    public baseResistence: number,
    public baseDefense: number,
    public baseDamagePower: number,


    //aktualni mnostvi zivota charakteru
    public currentHealth: number,
    public healthBlockedByFatigue: number,
    public skillDrawCount: number,
    public restFoodLimit: number,


    //equip bonus
    public equipBonusHealth: number,
    public equipBonusMana: number,

    public equipBonusManaRegen: number,
    public equipBonusHealthRegen: number,
    public equipBonusCritChance: number,
    public equipBonusResistence: number,
    public equipBonusDefense: number,
    public equipBonusDamagePower: number,

    //other bonus
    public otherBonusHealth: number,
    public otherBonusMana: number,

    public otherBonusManaRegen: number,
    public otherBonusHealthRegen: number,
    public otherBonusCritChance: number,
    public otherBonusResistence: number,
    public otherBonusDefense: number,
    public otherBonusDamagePower: number,

    //total with everything bonus
    public totalMaxHealth: number,
    public totalMaxMana: number,

    public totalManaRegen: number,
    public totalHealthRegen: number,
    public totalCritChance: number,
    public totalResistence: number,
    public totalDefense: number,
    public totalDamagePower: number,
  ) {


  }
}

export class Timestamps {
  constructor(
    public lastClaimTime: string,

  ) { }
}

export class Currency {
  constructor(
    public gold: number,
    //public silver: number,
    // public food: number,
    public time: number,
    public timeMax: number,
    //public fatigue: number,
    public travelPoints: number,
    public travelPointsMax: number,
    public scavengePoints: number,
    public scavengePointsMax: number,
    public monsterEssence: number
  ) { }
}

export const characterDocumentConverter = {
  toFirestore: (character: CharacterDocument) => {
    return {
      inventory: character.inventory,
      equipment: character.equipment,
      stats: character.stats,
      uid: character.uid,
      userUid: character.userUid,
      characterName: character.characterName,
      characterClass: character.characterClass,
      currency: character.currency,
      monsterKills: character.monsterKills,
      questgiversClaimed: character.questgiversClaimed,
      position: character.position,
      creationDate: character.creationDate,
      characterPortrait: character.characterPortrait,
      exploredPositions: character.exploredPositions,
      professions: character.professions,
      craftingRecipesUnlocked: character.craftingRecipesUnlocked,
      lastClaimedGameDay: character.lastClaimedGameDay,
      pointsOfInterestMaxTierReached: character.pointsOfInterestMaxTierReached,
      pendingRewards: character.pendingRewards,
      innHealhRestsCount: character.innHealhRestsCount,
      homeInn: character.homeInn,
      worldMapMemmory: character.worldMapMemmory,
      foodEffects: character.foodEffects,
      curses: character.curses,
      // chapelsUsed: character.chapelsUsed,
      blesses: character.blesses,
      chapelInfo: character.chapelInfo,
      vendorInfo: character.vendorInfo,
      treasuresClaimed: character.treasuresClaimed,
      dungeonsFinished: character.dungeonsFinished,
      seasonNumber: character.seasonNumber,
      isRetired: character.isRetired
      // rewardTreeRewardsClaimed: character.rewardTreeRewardsClaimed
      // isJoinedInEncounter: character.isJoinedInEncounter

    };
  },
  fromFirestore: (snapshot: any, options: any) => {
    const data = snapshot.data(options);


    let contentRemade: ContentContainer[] = [];
    data.inventory.content.forEach(content => {
      contentRemade.push(new ContentContainer(content.content, content.contentEquip));
    });

    let bagsRemade: InventoryBag[] = [];
    data.inventory.bags.forEach(bag => {
      bagsRemade.push(new InventoryBag(bag.uid, bag.itemId, bag.capacity));
    });


    let inventoryRemade = new Inventory(bagsRemade, data.inventory.capacityMax, data.inventory.capacityLeft, contentRemade);


    let perksRemade: PendingReward[] = [];
    for (var perk of data.pendingRewards) {
      perksRemade.push(new PendingReward(perk.uid, perk.rarity, perk.rewards, perk.rewardsRandomEquip, perk.rewardsGenerated, perk.isInstantReward, perk.recurrenceInGameDays, perk.rewardAtSpecificGameDay, perk.rewardAfterSpecificGameDay, perk.charges, perk.lastClaimGameDay, perk.chargesClaimed));
    }

    return new CharacterDocument(data.uid, data.userUid, data.characterName, data.characterClass, inventoryRemade, data.equipment, data.currency, data.stats, data.position, data.monsterKills, data.questgiversClaimed, data.creationDate, data.characterPortrait, data.exploredPositions, data.professions, data.craftingRecipesUnlocked, data.lastClaimedGameDay, data.pointsOfInterestMaxTierReached, perksRemade, data.innHealhRestsCount, data.homeInn, data.worldMapMemmory, data.foodEffects, data.curses, data.blesses, data.chapelInfo, data.vendorInfo, data.treasuresClaimed, data.dungeonsFinished, data.seasonNumber, data.isRetired);//, data.isJoinedInEncounter);
    // return new CharacterDocument(data.uid, data.userUid, data.characterName, data.characterClass, data.inventory, data.equipment, data.currency, data.stats, data.position, data.monsterKills, data.questgiversClaimed);
  }
}
// [Character]



export class CharacterPreview {
  constructor
    (
      public characterUid: string,
      public name: string,
      public characterClass: string,
      public level: number,
      public portrait: string,
      public playerUid: string,
      public seasonNumber: number,
      public isRetired: boolean
    ) { }
}


export class PlayerData {
  constructor(
    public uid: string,
    public playerName: string,
    public country: string,
    public satoshi: number,
    public fiatSpent: number,
    public reputation: number,
    public medals: SimpleTally[],
    public heroUpgrades: SimpleTally[],
    public heirloomUnlocks: string[],
    public characters: CharacterPreview[],
    public inventory: Inventory,
    public portraitsUnlocked: string[],
    public creationDate: string


  ) { }


  addSatoshi(_amount: number) {
    this.satoshi += _amount;
    this.satoshi = Math.round(this.satoshi);
  }
  subSatoshi(_amount: number) {
    if (this.satoshi >= _amount)
      this.satoshi -= _amount;
    else
      throw "Not enough Satoshi! You have " + this.satoshi + " but want to subsctract " + _amount;

    this.satoshi = Math.round(this.satoshi);
  }


  addReputation(_amount: number) {
    this.reputation += _amount;
    this.reputation = Math.round(this.reputation);
  }
  subReputation(_amount: number) {
    if (this.reputation >= _amount)
      this.reputation -= _amount;
    else
      throw "Not enough Satoshi! You have " + this.reputation + " but want to subsctract " + _amount;

    this.reputation = Math.round(this.reputation);
  }


  removeContentFromInventory(_contentToRemoveUid: string, _amount: number) {
    let foundCountent = false;
    // console.log("_contentToRemoveUid : " + _contentToRemoveUid);
    for (var i = this.inventory.content.length - 1; i >= 0; i--) {
      //  console.log("checking : " + this.inventory.content[i].getItem().uid);
      if (_contentToRemoveUid == this.inventory.content[i].getItem().uid) {
        //   console.log("found : " + this.inventory.content[i].getItem().uid);
        if (_amount == -1)
          _amount = this.inventory.content[i].getItem().amount;

        if (this.inventory.content[i].getItem().amount < _amount)
          throw ("Not enough of UID :" + _contentToRemoveUid + " in your inventory! You have : " + this.inventory.content[i].getItem().amount + " but you want to remove " + _amount);

        this.inventory.content[i].getItem().amount -= _amount;

        if (this.inventory.content[i].getItem().amount == 0) {
          this.inventory.content.splice(i, 1);
          this.inventory.capacityLeft++;
        }
        foundCountent = true;
        break;
      }
    }

    if (!foundCountent)
      throw ("removeContentFromInventory : Could not find content with UID :" + _contentToRemoveUid + " in your inventory!");
  }


  removeContentFromInventoryById(_contentToRemoveId: string, _amountToRemove: number) {

    let amoutOfItemsLeftToRemove = _amountToRemove;
    for (var i = this.inventory.content.length - 1; i >= 0; i--) {
      if (_contentToRemoveId == this.inventory.content[i].getItem().itemId) {

        //v tom stacku je dost itemu na remove co potrebuju
        if (amoutOfItemsLeftToRemove <= this.inventory.content[i].getItem().amount) {
          this.inventory.content[i].getItem().amount -= amoutOfItemsLeftToRemove;
          amoutOfItemsLeftToRemove = 0;
        }
        else {   //v tom stacku neni dost itemu na remove co potrebuju
          amoutOfItemsLeftToRemove -= this.inventory.content[i].getItem().amount;
          this.inventory.content[i].getItem().amount = 0;

        }
        //  this.inventory.content[i].getItem().amount -= _amount;

        if (this.inventory.content[i].getItem().amount == 0) {
          this.inventory.content.splice(i, 1);
          this.inventory.capacityLeft++;
        }
        if (amoutOfItemsLeftToRemove == 0)
          break;
      }
    }

    if (amoutOfItemsLeftToRemove > 0)
      throw ("You dont have enought " + _contentToRemoveId + " items in your inventory! You need : " + _amountToRemove + " but you have only: " + (_amountToRemove - amoutOfItemsLeftToRemove));
  }

  addContentToInventory(_contentToAdd: ContentContainer, _generateNewUid: boolean, _ignoreCapacity: boolean) { //defakto vzdycky chci generovat novy Uid,jedine kdyz swapuju itemy mezi inventarem a charakterem a ocekavam ze ty itemy zas najdu podle id tak davam false

    //Hack abych ziskal pristup k meetodatm BaseContentu bez toho abych musel pouzivat WithConvertor
    _contentToAdd = new ContentContainer(_contentToAdd.content, _contentToAdd.contentEquip);


    console.log("Davam item : " + _contentToAdd.getItem().itemId);

    if (_contentToAdd.getItem().contentType == CONTENT_TYPE.CURRENCY) {
      console.log("Mam tu nejakou currency : " + _contentToAdd.getItem().itemId);
      throw ("Only characters can have currencies. Not Players");
      //this.addCurrency(_contentToAdd.getItem().itemId, _contentToAdd.getItem().amount);
      return;
    }

    if (_contentToAdd.getItem().itemId == ITEMS.REPUTATION) {
      this.reputation += _contentToAdd.getItem().amount;
      return;
    }


    let itemAddedToExistingStack: boolean = false;

    for (const content of this.inventory.content) {
      if (content.getItem().itemId == _contentToAdd.getItem().itemId) {
        if (content.getItem().stackSize - content.getItem().amount >= _contentToAdd.getItem().amount) {
          content.getItem().amount += _contentToAdd.getItem().amount;
          itemAddedToExistingStack = true;
          console.log("Existing stack of " + _contentToAdd.getItem().itemId + " increased!");
          break;
        }
      }

    }
    if (!itemAddedToExistingStack) {
      if (this.inventory.capacityLeft > 0 || _ignoreCapacity) {
        if (_generateNewUid)
          _contentToAdd.getItem().uid = firestoreAutoId(); // TOHLE podle me je dobre delat vzdy a nicemu to nevadi. Kdovi co je to za item a jake ma uid, jestli originalni nebo ne (napriklad itemy koupene od vendroa maji vsechny stejny uid a nekde se novy musi vygenerovat)

        this.inventory.content.push(new ContentContainer(_contentToAdd.content, _contentToAdd.contentEquip));
        this.inventory.capacityLeft--;
        console.log("Item " + _contentToAdd.getItem().itemId + " added to inventory!");
      }
      else
        throw "Cant add new simple item " + _contentToAdd.getItem().itemId + " inventory is full!";
    }
  }


  // addSatoshium(_amount: number) {
  //   this.currencies.satoshium += _amount;
  // }
}

export const PlayerDataConverter = {
  toFirestore: (playerData: PlayerData) => {
    return {
      uid: playerData.uid,
      playerName: playerData.playerName,
      country: playerData.country,
      satoshi: playerData.satoshi,
      fiatSpent: playerData.fiatSpent,
      reputation: playerData.reputation,
      medals: playerData.medals,
      heroUpgrades: playerData.heroUpgrades,
      heirloomUnlocks: playerData.heirloomUnlocks,
      characters: playerData.characters,
      inventory: playerData.inventory,
      portraitsUnlocked: playerData.portraitsUnlocked,
      creationDate: playerData.creationDate
    };
  },

  fromFirestore: (snapshot: any, options: any) => {
    const data = snapshot.data(options);


    let contentRemade: ContentContainer[] = [];
    data.inventory.content.forEach(content => {
      contentRemade.push(new ContentContainer(content.content, content.contentEquip));
    });

    let bagsRemade: InventoryBag[] = [];
    data.inventory.bags.forEach(bag => {
      bagsRemade.push(new InventoryBag(bag.uid, bag.itemId, bag.capacity));
    });


    let inventoryRemade = new Inventory(bagsRemade, data.inventory.capacityMax, data.inventory.capacityLeft, contentRemade);

    return new PlayerData(data.uid, data.playerName, data.country, data.satoshi, data.fiatSpent, data.reputation, data.medals, data.heroUpgrades, data.heirloomUnlocks, data.characters, inventoryRemade, data.portraitsUnlocked, data.creationDate);
  }
}

export async function UpdateCharacterLocationInHisParty(_transaction: any, _characterUid: string, _position: WorldPosition, _updateInstatnly: boolean): Promise<Party | undefined> {

  const partiesDb = admin.firestore().collection('parties')
  const myPartyDb = partiesDb.where("partyMembersUidList", "array-contains", _characterUid);

  //pokud jsi v parte updatnu tvoji lokaci i tam
  let myPartyData: Party | undefined;
  await _transaction.get(myPartyDb).then(querry => {
    if (querry.size == 1) {
      querry.docs.forEach(doc => {
        myPartyData = doc.data();
        if (myPartyData != undefined) {
          //najdu svuj zaznam v parte a updatnu lokaci
          myPartyData.partyMembers.forEach(element => {
            if (element.uid == _characterUid)
              element.position = _position;
          });
        }
      });
    }
    else if (querry.size > 1)
      throw "You are more than in 1 party! How could this be? DATABASE ERROR!";
  });

  if (myPartyData != undefined && _updateInstatnly) {
    _transaction.set(partiesDb.doc(myPartyData.uid), JSON.parse(JSON.stringify(myPartyData)), { merge: true });
  }
  return myPartyData;

}

export async function QuerryHasCharacterAnyUnclaimedEncounterResult(_transaction: any, _character: CharacterDocument): Promise<boolean> {
  const encounterDb = admin.firestore().collection("encounterResults").where("combatantsWithUnclaimedRewardsList", "array-contains", _character.uid).where("position.zoneId", "==", _character.position.zoneId).where("position.locationId", "==", _character.position.locationId).where("position.pointOfInterestId", "==", _character.position.pointOfInterestId);
  //najdu jestli mas nejaky unclaimnuty encounter Result
  let hasUnclaimedEncounterResult = false;
  await _transaction.get(encounterDb).then(querry => {
    hasUnclaimedEncounterResult = querry.size > 0
  });
  return hasUnclaimedEncounterResult
}

export async function QuerryIfCharacterIsInCombatAtAnyEncounter(_transaction: any, _characterUid: string): Promise<boolean> {
  const encounterDb = admin.firestore().collection("encounters").where("combatantList", "array-contains", _characterUid);

  //najdu jestli si v nejakem encounteru clenem
  let participatingInEncounter = false;
  await _transaction.get(encounterDb).then(querry => {
    participatingInEncounter = querry.size > 0
  });

  return participatingInEncounter
}


export async function QuerryIfCharacterIsInCombatAtDungeonEncounter(_transaction: any, _characterUid: string): Promise<boolean> {
  const encounterDb = admin.firestore().collection("encounters").where("encounterContext", "==", ENCOUNTER_CONTEXT.DUNGEON).where("combatantList", "array-contains", _characterUid);

  //najdu jestli si v nejakem encounteru clenem
  let participatingInEncounter = false;
  await _transaction.get(encounterDb).then(querry => {
    participatingInEncounter = querry.size > 0
  });

  return participatingInEncounter
}

export async function QuerryForCharactersCombatEncounter(_transaction: any, _characterUid: string): Promise<EncounterDocument | undefined> {
  const encounterDb = admin.firestore().collection("encounters").where("combatantList", "array-contains", _characterUid).withConverter(encounterDocumentConverter);

  let encounter: EncounterDocument | undefined;
  await _transaction.get(encounterDb).then(querry => {

    querry.docs.forEach(doc => {
      encounter = doc.data();
    });

    if (querry.size > 1)
      throw ("How can you be in more than 1 encounter!? DATABASE ERROR!");
  });

  return encounter
}


export async function QuerryIfCharacterIsWatcherInAnyEncounterOnHisPosition(_transaction: any, _character: CharacterDocument): Promise<boolean> {

  const allCallerPersonalEncountersOnHisPosition = admin.firestore().collection('encounters').where("watchersList", "array-contains", _character.uid).where("position.zoneId", "==", _character.position.zoneId).where("position.locationId", "==", _character.position.locationId).where("position.pointOfInterestId", "==", _character.position.pointOfInterestId);//.where("encounterContext", "==", ENCOUNTER_CONTEXT.PERSONAL);

  //najdu jestli si v nejakem encounteru clenem
  let participatingInEncounter = false;
  await _transaction.get(allCallerPersonalEncountersOnHisPosition).then(querry => {
    participatingInEncounter = querry.size > 0
  });

  return participatingInEncounter
}


export async function QuerryIfCharacterIsInAnyEncounterOnHisPosition(_transaction: any, _character: CharacterDocument): Promise<boolean> {

  const allCallerPersonalEncountersOnHisPosition = admin.firestore().collection('encounters').where("combatantList", "array-contains", _character.uid).where("position.zoneId", "==", _character.position.zoneId).where("position.locationId", "==", _character.position.locationId).where("position.pointOfInterestId", "==", _character.position.pointOfInterestId);//.where("encounterContext", "==", ENCOUNTER_CONTEXT.PERSONAL);

  //najdu jestli si v nejakem encounteru clenem
  let participatingInEncounter = false;
  await _transaction.get(allCallerPersonalEncountersOnHisPosition).then(querry => {
    participatingInEncounter = querry.size > 0
  });

  return participatingInEncounter
}

export async function QuerryIfCharacterIsWatcherInAnyDungeonEncounter(_transaction: any, _characterUid: string): Promise<boolean> {
  const encounterDb = admin.firestore().collection("encounters").where("watchersList", "array-contains", _characterUid).where("encounterContext", "==", ENCOUNTER_CONTEXT.DUNGEON);

  //najdu jestli si v nejakem encounteru clenem
  let participatingInEncounter = false;
  await _transaction.get(encounterDb).then(querry => {
    participatingInEncounter = querry.size > 0
  });

  return participatingInEncounter
}


export async function QuerryForParty(_transaction: any, characterUid: string): Promise<Party | null> {

  const myPartyDb = admin.firestore().collection('parties').where("partyMembersUidList", "array-contains", characterUid).withConverter(PartyConverter);

  let myPartyData: Party | null = null;

  //ziskam tvoji partu
  await _transaction.get(myPartyDb).then(querry => {
    if (querry.size == 1) {
      querry.docs.forEach(doc => {
        myPartyData = doc.data();

      });
    }
    else if (querry.size > 1)
      throw "You are more than in 1 party! How could this be? DATABASE ERROR!";
  });


  return myPartyData;
}



export async function HasPartyAnyDungeonEncounter(_transaction: any, _partyUid: string): Promise<boolean> {
  const encounterDb = admin.firestore().collection("encounters").where("foundByCharacterUid"/*"foundByPartyUid"*/, "==", _partyUid).where("encounterContext", "==", ENCOUNTER_CONTEXT.DUNGEON);

  let exists = false;
  await _transaction.get(encounterDb).then(querry => {
    exists = querry.size > 0
  });

  return exists
}






//creates Player 
exports.createPlayer = functions.auth.user().onCreate(async (user) => {

  // get user data from the auth trigger
  const userUid = user.uid; // The UID of the user.

  // const characterRef = admin.firestore().collection("characters").doc();
  const playeref = admin.firestore().collection("players").doc(userUid);

  let inventory = new Inventory([], 99, 99, []);
  inventory.content.push(generateContentContainer(generateContent(ITEMS.TRAINING_TOKEN, 3)));

  let ulockedPortraits: string[] = [];
  ulockedPortraits.push("CHARACTER_PORTRAIT_DEFAULT");
  ulockedPortraits.push("CHARACTER_PORTRAIT_1");
  ulockedPortraits.push("CHARACTER_PORTRAIT_2");
  ulockedPortraits.push("CHARACTER_PORTRAIT_3");
  ulockedPortraits.push("CHARACTER_PORTRAIT_4");
  ulockedPortraits.push("CHARACTER_PORTRAIT_WARRIOR");
  ulockedPortraits.push("CHARACTER_PORTRAIT_WARRIOR_1");
  ulockedPortraits.push("CHARACTER_PORTRAIT_WARRIOR_DEFAULT_1");
  ulockedPortraits.push("CHARACTER_PORTRAIT_WARRIOR_DEFAULT_1");
  ulockedPortraits.push("CHARACTER_PORTRAIT_WARLOCK");
  ulockedPortraits.push("CHARACTER_PORTRAIT_SHAMAN");

  // ulockedPortraits.push("CHARACTER_PORTRAIT_WARLOCK_DEFAULT_1");
  // ulockedPortraits.push("CHARACTER_PORTRAIT_WARRIOR_DEFAULT_1");
  // ulockedPortraits.push("CHARACTER_PORTRAIT_SHAMAN_DEFAULT_1");

  let player: PlayerData = new PlayerData(playeref.id, userUid, "UNKNOWN", 0, 0, 0, [], [], [], [], inventory, ulockedPortraits, getCurrentDateTime(0));

  return playeref.set(JSON.parse(JSON.stringify(player)));
});


class CharacterNameEntry {
  constructor(public playerUid: string) { }
}

exports.createCharacter = functions.https.onCall(async (data, context) => {


  //const encounterResultUid = data.encounterResultUid;
  const callerPlayerUid = data.playerUid;
  const characterName = data.characterName;
  const characterClass = data.characterClass;
  const characterPortrait = data.characterPortrait;

  const characterNamesDb = admin.firestore().collection("characterNames");
  const characterRef = admin.firestore().collection("characters").doc();
  const playerRef = admin.firestore().collection("players").doc(callerPlayerUid).withConverter(PlayerDataConverter);
  const globalDataDB = await admin.firestore().collection('_metadata_coreDefinitions').doc("Global");

  const characterUid: string = characterRef.id;

  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {
      const playerDoc = await t.get(playerRef);
      let playerdata: PlayerData = playerDoc.data();

      switch (characterClass) {
        case CHARACTER_CLASS.WARRIOR:
          playerdata.removeContentFromInventoryById(ITEMS.TRAINING_TOKEN, 0);
          break;
        case CHARACTER_CLASS.SHAMAN:
        case CHARACTER_CLASS.WARLOCK:
          playerdata.removeContentFromInventoryById(ITEMS.TRAINING_TOKEN, 5);
          break;
        case CHARACTER_CLASS.MAGE:
          playerdata.removeContentFromInventoryById(ITEMS.TRAINING_TOKEN, 20);
          break;


        default:
          throw "No character class :" + characterClass + " exists!";
      }

      if (!playerdata.portraitsUnlocked.includes(characterPortrait))
        throw "You dont have porait : " + characterPortrait + " unlocked!";


      const globalDataDoc = await t.get(globalDataDB);
      let globalData: GlobalMetadata = globalDataDoc.data();
      const newCharacter = await createCharacter(t, characterUid, callerPlayerUid, characterName, characterClass, characterPortrait);


      //dam nahodny scavenge point
      let skillDefinitions = await QuerryForSkillDefinitions(t);
      const locationDb = admin.firestore().collection('_metadata_zones').doc(newCharacter.position.zoneId).collection("locations").doc(newCharacter.position.locationId);
      var locationDoc = await t.get(locationDb.withConverter(LocationConverter));
      var locationData: MapLocation = locationDoc.data();

      let rarePerks = rollForRandomItems(locationData.perksRareOffers.find(item => item.id == "STARTING_SCAVENGE_SPOTS")!.perks, 1) as PerkOfferDefinition[];
      console.log("rarePerks:" + rarePerks[0].uid);
      newCharacter.addPendingReward(rarePerks[0], skillDefinitions);



      if (globalData.isSeasonInProgress) {
        const characterPreview = new CharacterPreview(newCharacter.uid, newCharacter.characterName, newCharacter.characterClass, newCharacter.stats.level, newCharacter.characterPortrait, playerdata.uid, globalData.seasonNumber, false);

        playerdata.characters.push(characterPreview);

        //presuneme artefakty z playera do charakteru
        for (const content of playerdata.inventory.content) {
          if (content.getItem().rarity == RARITY.ARTIFACT)
            newCharacter.inventory.content.push(generateContentContainer(generateContent(content.getItem().itemId, content.getItem().amount)));
        }
        //a smazeme z playera
        for (const content of playerdata.inventory.content) {
          if (content.getItem().rarity == RARITY.ARTIFACT)
            playerdata.inventory.content.splice(playerdata.inventory.content.indexOf(content));
        }

        const characterNameEntry = new CharacterNameEntry(playerdata.uid);

        newCharacter.lastClaimedGameDay = -1;// globalData.gameDay;


        t.set(characterNamesDb.doc(newCharacter.characterName), JSON.parse(JSON.stringify(characterNameEntry)));
        t.set(playerRef, JSON.parse(JSON.stringify(playerdata)), { merge: true });
        t.set(characterRef, JSON.parse(JSON.stringify(newCharacter)));
      }
      else
        throw "Season is not in progress. Wait for next season to start!";

      return "OK";
    });




    console.log('Transaction success', result);
    return result;
  } catch (e) {
    console.log('Transaction failure:', e);
    throw new functions.https.HttpsError("aborted", "Error : " + e);
  }
});




// exports.scavengePointsPurchase = functions.https.onCall(async (data, context) => {

//   const callerCharacterUid = data.characterUid;

//   const characterDb = await admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);
//   //const globalDataDB = await admin.firestore().collection('_metadata_coreDefinitions').doc("Global");

//   try {
//     const result = await admin.firestore().runTransaction(async (t: any) => {

//       const characterDoc = await t.get(characterDb);
//       let characterData: CharacterDocument = characterDoc.data();
//       validateCallerBulletProof(characterData, context);


//       characterData.subTime(SCAVENGE_POINT_PURCHASE_COST);
//       characterData.addScavengePoints(SCAVENGE_POINT_PURCHASE_AMOUNT);

//       // characterData.subCurrency(CURRENCY_ID.FATIGUE, 100);
//       t.set(characterDb, JSON.parse(JSON.stringify(characterData)), { merge: true });

//       return "OK";
//     });


//     console.log('Transaction success', result);
//     return result;
//   } catch (e) {
//     console.log('Transaction failure:', e);
//     throw new functions.https.HttpsError("aborted", "Error : " + e);
//   }


// });


exports.restDeep = functions.https.onCall(async (data, context) => {

  const callerCharacterUid = data.characterUid;

  const foodSuppliesUids: string[] = data.foodSuppliesUids;
  const foodSuppliesAmounts: number[] = data.foodSuppliesAmounts;

  const characterDb = await admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);

  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {

      const characterDoc = await t.get(characterDb);
      let characterData: CharacterDocument = characterDoc.data();
      validateCallerBulletProof(characterData, context);

      let party = await QuerryForParty(t, characterData.uid);
      if (party != null)
        if (party.dungeonProgress != null)
          throw "You cant rest in Dungeon! Its too dangerous!";

      characterData.subTime(REST_TIME_COST);
      characterData.addTravelPoints(TRAVEL_POINTS_PER_REST);

      //odstranim stare food effekty
      characterData.removeFoodEffectsAll();
      //odstranim food supplies
      let totalFoodSupplies: number = 0;

      for (var i = foodSuppliesUids.length - 1; i >= 0; i--) {
        for (var j = characterData.inventory.content.length - 1; j >= 0; j--) {
          if (foodSuppliesUids[i] == characterData.inventory.content[j].getItem().uid) {
            let currentItem = characterData.inventory.content[j].getItem();

            if (!currentItem.customData || !currentItem.customData?.integers)
              throw "Database error : food supply you are trying to conusme does not have food supply ammount set";

            totalFoodSupplies += characterData.inventory.content[j].getItem().customData!.integers![0] * foodSuppliesAmounts[i];

            switch (currentItem.itemId) {

              case ITEMS.ROTTEN_APPLE:
                characterData.giveHealth(currentItem.customData!.integers![1] * foodSuppliesAmounts[i]);
                break;
              case ITEMS.MEAT:
                characterData.giveHealth(currentItem.customData!.integers![1] * foodSuppliesAmounts[i]);
                break;
              case ITEMS.APPLE:
                if (characterData.hasBless(BLESS.FOOD_LIMIT_INCREASE))
                  throw "The mere scent of the organs almost made you vomit"
                characterData.subFatigueFlat(currentItem.customData!.integers![1] * foodSuppliesAmounts[i]);
                break;
              case ITEMS.BERRY:
                characterData.addScavengePoints(currentItem.customData!.integers![1] * foodSuppliesAmounts[i]);
                break;
              case ITEMS.BONE:
                characterData.addTravelPoints(currentItem.customData!.integers![1] * foodSuppliesAmounts[i]);
                break;

              case ITEMS.RIBEYE:
                characterData.giveHealth(currentItem.customData!.integers![1] * foodSuppliesAmounts[i]);
                break;
              case ITEMS.BONE_BROTH:
                characterData.addTravelPoints(currentItem.customData!.integers![1] * foodSuppliesAmounts[i]);
                break;
              case ITEMS.OMELETTE:
                characterData.addScavengePoints(currentItem.customData!.integers![1] * foodSuppliesAmounts[i]);
                break;
              case ITEMS.APPLE_PIE:
                if (characterData.hasBless(BLESS.FOOD_LIMIT_INCREASE))
                  throw "The mere scent of the organs almost made you vomit"
                characterData.subFatigueFlat(currentItem.customData!.integers![1] * foodSuppliesAmounts[i]);

                break;
              case ITEMS.CURSE_REMOVE_MEAL:
                characterData.removeRandomCurse(currentItem.customData!.integers![1] * foodSuppliesAmounts[i]);
                break;
              // case ITEMS.FATIGUE_MEAL:
              //   characterData.removeRafndomCurse(currentItem.customData!.integers![1]);
              //   break;
              // case ITEMS.RIBEYE:
              //   break;

              default:
                break;
            }

            //food ma definovy food effect, pridame food effekty
            if (currentItem.customData.simpleTally) {
              currentItem.customData.simpleTally.forEach(foodEffect => { characterData.addFoodEffect(foodEffect); });
            }

            //odstranim vse z inventory
            if (characterData.inventory.content[j].getItem().itemId)

              if (foodSuppliesAmounts[i] == characterData.inventory.content[j].getItem().amount) { //konzumuju vse
                characterData.inventory.content.splice(j, 1);
                characterData.inventory.capacityLeft++;
              }
              else
                characterData.inventory.content[j].getItem().amount -= foodSuppliesAmounts[i];

            break;
          }
        }
      }

      //   console.log("Total food supplies: " + totalFoodSupplies);

      const suppliesCost = characterData.stats.restFoodLimit + ((characterData.stats.level - 1) * REST_FOOD_SUPPLIES_LIMIT_INCREMENT_PER_LEVEL);
      if (totalFoodSupplies > suppliesCost)
        throw "Too many supplies. You want to eat :" + totalFoodSupplies + " but you can eat only up to :" + suppliesCost + "!";

      // characterData.giveHealth(25);


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


// exports.claimTimePool = functions.https.onCall(async (data, context) => {

//   const FATIGUE_RECOVERED_PER_HOUR = 10;
//   const TIME_GAINED_PER_HOUR = 3;
//   const callerCharacterUid = data.characterUid;

//   const characterDb = await admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);

//   try {
//     const result = await admin.firestore().runTransaction(async (t: any) => {

//       const characterDoc = await t.get(characterDb);
//       let characterData: CharacterDocument = characterDoc.data();

//       const currentTime = getCurrentDateTimeInMillis(0);
//       const hoursPassed = millisToHours(currentTime - Number.parseInt(characterData.timestamps.lastClaimTime));
//       const wholeHoursPassed = Math.floor(hoursPassed);
//       const lefoverAfterRoundingInMillis = hourstoMillis(hoursPassed - wholeHoursPassed);

//       console.log("Hours passed since you claimed Time pool : " + hoursPassed);
//       const timeGained = wholeHoursPassed * TIME_GAINED_PER_HOUR;
//       const fatigueRecovered = wholeHoursPassed * FATIGUE_RECOVERED_PER_HOUR;
//       console.log("You gained as much as: " + timeGained + " time");
//       console.log("You recover as much as : " + fatigueRecovered + "% fatigue");

//       if (wholeHoursPassed >= 1) {
//         characterData.addCurrency(CURRENCY_ID.TIME, timeGained);
//         characterData.subCurrency(CURRENCY_ID.FATIGUE, fatigueRecovered);
//         characterData.giveHealth(wholeHoursPassed * 10);
//         characterData.timestamps.lastClaimTime = (currentTime - lefoverAfterRoundingInMillis).toString();
//         t.set(characterDb, JSON.parse(JSON.stringify(characterData)), { merge: true });
//       }

//       return "OK";
//     });


//     console.log('Transaction success', result);
//     return result;
//   } catch (e) {
//     console.log('Transaction failure:', e);
//     throw new functions.https.HttpsError("aborted", "Error : " + e);
//   }


// });



exports.consumeConsumable = functions.https.onCall(async (data, context) => {

  const callerCharacterUid = data.characterUid;
  const consumableUid = data.consumanleUid;

  const characterDb = await admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);

  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {

      const characterDoc = await t.get(characterDb);
      let characterData: CharacterDocument = characterDoc.data();

      const itemToConsume = characterData.getInventoryContent(consumableUid).getItem();

      // if (itemToConsume.contentType != CONTENT_TYPE.FOOD)
      //   throw ("Only consumables can be consumed!");
      let myEncounter: EncounterDocument | undefined;

      if (itemToConsume.customData == undefined)
        throw ("Define custom data!");

      if (itemToConsume.expireDate != undefined)
        if (getTimeLeftToDateInSeconds(itemToConsume.expireDate) <= 0)
          throw "Cant consume. Item already expired!";


      if (itemToConsume.contentType == CONTENT_TYPE.FOOD) {

        switch (itemToConsume.itemId) {

          // case ITEMS.ROTTEN_APPLE:
          // case ITEMS.APPLE:
          // case ITEMS.BERRY:
          // case ITEMS.RIBEYE:
          // case ITEMS.APPLE_PIE:
          //   {
          //     myEncounter = await QuerryForCharactersCombatEncounter(t, callerCharacterUid);
          //     if (myEncounter != undefined)
          //       throw ("This cannot be consumed during combat!");

          //     //let overhealAmount = characterData.giveHealth(itemToConsume.customData.integers![0]);
          //     characterData.subCurrency(CURRENCY_ID.TIME, itemToConsume.customData.integers![1]);

          //     // if (overhealAmount > 0) {
          //     //   characterData.foodEffect = new SimpleTally(itemToConsume.itemId, overhealAmount);
          //     // }
          //     break;
          //   }
          case ITEMS.FATIGUE_PILL:
            {
              myEncounter = await QuerryForCharactersCombatEncounter(t, callerCharacterUid);
              if (myEncounter != undefined)
                throw ("This cannot be consumed during combat!");
              characterData.subCurrency(CURRENCY_ID.FATIGUE, itemToConsume.customData.integers![0]);
              characterData.stats.baseHealth -= itemToConsume.customData.integers![1]
              characterData.recalculateCharacterStats();
              break;
            }
          case ITEMS.HEALTH_POTION:
          case ITEMS.MINOR_HEALTH_POTION:
          case ITEMS.MAJOR_HEALTH_POTION:
            {
              myEncounter = await QuerryForCharactersCombatEncounter(t, callerCharacterUid);
              if (myEncounter == undefined)
                throw ("This can be consumed only during combat!");

              let myCombatEntry = myEncounter.getCombatMemberByUid(callerCharacterUid);
              if (myCombatEntry == null)
                throw "Cant find you in combat! ";

              myCombatEntry.addPotionUsed(itemToConsume.itemId);

              characterData.subCurrency(CURRENCY_ID.TIME, itemToConsume.customData.integers![1]);
              myEncounter.addEntryToCombatLog("<b>" + myCombatEntry.displayName + "</b> drinked <b>{" + itemToConsume.itemId + "}</b>");
              myEncounter.giveHealthToCombatEntity(myCombatEntry, myCombatEntry, itemToConsume.customData.integers![0], itemToConsume.itemId);

              //myCombatEntry.stats.leastHealth+=itemToConsume.customData.integers![0];

              t.set(admin.firestore().collection("encounters").doc(myEncounter.uid), JSON.parse(JSON.stringify(myEncounter)), { merge: true });

              break;
            }
          case ITEMS.INTELLECT_POTION:
            {
              myEncounter = await QuerryForCharactersCombatEncounter(t, callerCharacterUid);
              if (myEncounter == undefined)
                throw ("This can be consumed only during combat!");

              characterData.subCurrency(CURRENCY_ID.TIME, itemToConsume.customData.integers![1]);

              let myCombatEntry = myEncounter.getCombatMemberByUid(callerCharacterUid);
              if (myCombatEntry == null)
                throw "Cant find you in combat! ";

              myCombatEntry.addPotionUsed(itemToConsume.itemId);

              myEncounter.addEntryToCombatLog("<b>" + myCombatEntry.displayName + "</b> drinked <b>{" + itemToConsume.itemId + "}</b>");
              // myEncounter.getCombatMemberByUid(callerCharacterUid).stats.damagePowerTotal += itemToConsume.customData.integers![0];
              myCombatEntry.stats.manaMax += itemToConsume.customData.integers![0];
              myCombatEntry.stats.mana += itemToConsume.customData.integers![0];

              t.set(admin.firestore().collection("encounters").doc(myEncounter.uid), JSON.parse(JSON.stringify(myEncounter)), { merge: true });

              break;
            }
          // case ITEMS.CURSE_REMOVE_MEAL:
          //   {
          //     myEncounter = await QuerryForCharactersCombatEncounter(t, callerCharacterUid);
          //     if (myEncounter != undefined)
          //       throw ("This cannot be consumed during combat!");


          //     // for (let index = 0; index < itemToConsume.customData.integers![0]; index++) {
          //     //   characterData.curses.splice(randomIntFromInterval(0, characterData.curses.length - 1), 1);
          //     // }
          //     if (characterData.curses.length > 0)
          //       characterData.removeRandomCurse(1);
          //     else
          //       throw ("You dont have any curse");

          //     break;
          //   }
          case ITEMS.MANA_POTION:
          case ITEMS.MINOR_MANA_POTION:
            {
              myEncounter = await QuerryForCharactersCombatEncounter(t, callerCharacterUid);
              if (myEncounter == undefined)
                throw ("This can be consumed only during combat!");

              let myCombatEntry = myEncounter.getCombatMemberByUid(callerCharacterUid);
              if (myCombatEntry == null)
                throw "Cant find you in combat! ";
              myCombatEntry.addPotionUsed(itemToConsume.itemId);

              characterData.subCurrency(CURRENCY_ID.TIME, itemToConsume.customData.integers![1]);
              myEncounter.addEntryToCombatLog("<b>" + myCombatEntry.displayName + "</b> drinked <b>{" + itemToConsume.itemId + "}</b>");
              myCombatEntry.giveMana(itemToConsume.customData.integers![0]);
              t.set(admin.firestore().collection("encounters").doc(myEncounter.uid), JSON.parse(JSON.stringify(myEncounter)), { merge: true });

              break;
            }
          // case ITEMS.TOWN_PORTAL:
          //   {
          //     myEncounter = await QuerryForCharactersCombatEncounter(t, callerCharacterUid);
          //     if (myEncounter != undefined)
          //       throw ("This cannot be consumed during combat!");

          //     characterData.position = characterData.homeInn;

          //     break;
          //   }

          default:
            throw ("There is no definition of effect for consumable item : " + itemToConsume.itemId);
        }
      }
      else if (itemToConsume.contentType == CONTENT_TYPE.RECIPE) {
        if (itemToConsume.customData.integers)
          characterData.subCurrency(CURRENCY_ID.TIME, itemToConsume.customData.integers![1]);

        characterData.learnRecipe(itemToConsume);
      }
      else if (itemToConsume.contentType == CONTENT_TYPE.CHEST) {

        if (itemToConsume.itemId == ITEMS.CHEST_RUBY) {
          //remove key 
          characterData.removeContentFromInventoryById(ITEMS.MAGIC_KEY, 1);
        }

        const dropTablesDb = admin.firestore().collection('_metadata_dropTables').doc(characterData.position.zoneId).collection("locations").doc(characterData.position.locationId);
        const dropTablesDoc = await t.get(dropTablesDb);
        let dropTablesData: DropTablesData = dropTablesDoc.data();

        var dropTableForChest = dropTablesData.chestDropTables.find(dropTableGroup => dropTableGroup.id == itemToConsume.itemId);
        if (!dropTableForChest)
          throw "Cant find a drop table for chest : " + itemToConsume.itemId;

        let skillDefinitions = await QuerryForSkillDefinitions(t);

        let drop = generateDropFromDropTable(dropTableForChest.dropTables, characterData.stats.level, skillDefinitions, characterData.characterClass);
        drop.forEach(content => {
          characterData.addContentToInventory(content, true, true);
        });
      }
      else
        throw ("Item does not exist as consumable item: " + itemToConsume.itemId);

      characterData.removeContentFromInventory(itemToConsume.uid, 1);

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


exports.dropItem = functions.https.onCall(async (data, context) => {

  const callerCharacterUid = data.characterUid;
  //const contentUid = data.contentUid;
  const contentUids: string[] = data.contentUids;

  const characterDb = await admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);

  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {

      const characterDoc = await t.get(characterDb);
      let characterData: CharacterDocument = characterDoc.data();

      contentUids.forEach(element => {
        characterData.removeContentFromInventory(element, -1);

      });

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

//ASI UNUSED, potret se vybira na zacatku cely barber a vse je nanic
exports.changeCharacterPortrait = functions.https.onCall(async (data, context) => {

  const callerCharacterUid = data.characterUid;
  const portraitId = data.portraitId;


  const characterDb = await admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);
  const OtherMetadataDb = await admin.firestore().collection('_metadata_coreDefinitions').doc("Other");

  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {



      const characterDoc = await t.get(characterDb);
      let characterData: CharacterDocument = characterDoc.data();

      const OtherMetadataDoc = await t.get(OtherMetadataDb);
      let OtherMetadataData: OtherMetadata = OtherMetadataDoc.data();

      // if (!characterData.portraitsUnlocked.includes(portraitId))
      //   throw ("You dont own " + portraitId);

      //TODO: k tomuto by nikdy nemelo ale dojit, kdyz budu ukladat unlocked portraity do charakeru pres cloudscript tak tam nikdy nemuze byt blby portrait a test nahore by zachyti zase fake snahy o portait ktery nemam
      let portraitValid = false;
      for (const charcaterPortraits of OtherMetadataData.possiblePortraits) {
        if (charcaterPortraits.classId == characterData.characterClass) {
          charcaterPortraits.portraits.includes(portraitId);
          portraitValid = true;
          break;
        }
      }
      if (!portraitValid)
        throw (portraitId + " is not valid portrait for your class!");


      //muj charakter
      characterData.characterPortrait = portraitId;

      //Moje character preview v playerovi
      const playerDb = await admin.firestore().collection('players').doc(characterData.userUid);
      const playerDoc = await t.get(playerDb);
      let playerData: PlayerData = playerDoc.data();

      for (const character of playerData.characters) {
        if (character.characterUid == callerCharacterUid) {
          character.portrait = portraitId;
          break;
        }
      }


      //muj zaznam v parte..
      const myPartyDb = admin.firestore().collection('parties').where("partyMembersUidList", "array-contains", callerCharacterUid).withConverter(PartyConverter);

      let myPartyData: Party | undefined;
      await t.get(myPartyDb).then(querry => {
        if (querry.size == 1) {
          querry.docs.forEach(doc => {
            myPartyData = doc.data();

            if (myPartyData != null && myPartyData != undefined)
              myPartyData.getPartyMemberByUid(callerCharacterUid)!.characterPortrait = portraitId;

          });
        }
        else if (querry.size > 1)
          throw "You are more than in 1 party! How could this be? DATABASE ERROR!";
      });


      //moje zaznamy v leaderboardech...
      await updateMyPortraitAtAllLeaderboards(t, characterData, portraitId);
      // const leaderboardsDb = await admin.firestore().collection('leaderboards').doc("CHARACTER_LEVEL").collection("leaderboard").doc(characterData.uid);
      // const leaderboardsDoc = await t.get(leaderboardsDb);
      // let leaderboardsData: LeaderboardScoreEntry = leaderboardsDoc.data();
      // if (leaderboardsData != undefined) {
      //   leaderboardsData.character.portrait = portraitId
      //   t.set(leaderboardsDb, JSON.parse(JSON.stringify(leaderboardsData)), { merge: true });
      // }



      t.set(characterDb, JSON.parse(JSON.stringify(characterData)), { merge: true });
      if (myPartyData != undefined)
        t.set(admin.firestore().collection('parties').doc(myPartyData.uid), JSON.parse(JSON.stringify(myPartyData)), { merge: true });

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



exports.deleteCharacter = functions.https.onCall(async (data, context) => {

  const characterToDeleteUid = data.characterToDeleteUid;
  const characterDb = admin.firestore().collection("characters").doc(characterToDeleteUid);

  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {
      const characterDoc = await t.get(characterDb);
      let characterData: CharacterDocument = characterDoc.data();

      validateCallerBulletProof(characterData, context);

      const playerDb = admin.firestore().collection("players").doc(characterData.userUid);
      const playerDoc = await t.get(playerDb);
      let playerData: PlayerData = playerDoc.data();

      for (const character of playerData.characters) {
        if (character.characterUid == characterToDeleteUid) {
          playerData.characters.splice(playerData.characters.indexOf(character), 1);
          break;
        }
      }


      t.set(playerDb, JSON.parse(JSON.stringify(playerData)), { merge: true });

      t.delete(characterDb);

      return "OK";
    });




    console.log('Transaction success', result);
    return result;
  } catch (e) {
    console.log('Transaction failure:', e);
    throw new functions.https.HttpsError("aborted", "Error : " + e);
  }
});


exports.checkForIntegrityOfCharacterData = functions.https.onCall(async (data, context) => {

  const characterToCheckUid = data.characterToCheckUid;
  const characterDb = admin.firestore().collection("characters").doc(characterToCheckUid);
  const globalDataDB = await admin.firestore().collection('_metadata_coreDefinitions').doc("Global");

  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {
      const characterDoc = await t.get(characterDb);
      let characterData: CharacterDocument = characterDoc.data();


      const globalDataDoc = await t.get(globalDataDB);
      let globalData: GlobalMetadata = globalDataDoc.data();

      if (globalData.seasonNumber != characterData.seasonNumber)
        throw ("This character's season has already ended." + "Current season is " + globalData.seasonNumber + " but character season is" + characterData.seasonNumber);
      else if (!globalData.isSeasonInProgress) {
        //TODO:  character flagnout aby byl unplable...asi neco jeste jineho nez isRetired...alre treba isFromLastSeason....jde o to ze nechci pri kazde blbosti ziskavat global data abych cehckoval esli sezona skoncila, tak si to tady flagnout a oak uz je to v charakteru?
        //nebo do charakteru ulozit nejaky timstamp kdy je charakter expired spis
        throw ("Season not in progress. Wait for new season to start!");
      }

      validateCallerBulletProof(characterData, context);


      let issueFound = false;

      if (characterData.professions == undefined) {
        issueFound = true;
        console.log("Repairing Character....");
        characterData.professions = [];
        //  characterData.professions.push(new SimpleTally(PROFESSION.HERBALISM, 0));
        //characterData.professions.push(new SimpleTally(PROFESSION.MINING, 0));

      }

      if (issueFound)
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


exports.sellInventoryItems = functions.https.onCall(async (data, context) => {

  const callerCharacterUid = data.characterUid;
  const inventoryItemsToSellUids: string[] = data.inventoryItemsToSellEquipUids;

  const characterDb = await admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);

  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {

      const characterDoc = await t.get(characterDb);
      let characterData: CharacterDocument = characterDoc.data();



      let totalSellPrice: number = 0;

      for (var i = characterData.inventory.content.length - 1; i >= 0; i--) {
        if (inventoryItemsToSellUids.includes(characterData.inventory.content[i].getItem().uid)) {
          totalSellPrice += characterData.inventory.content[i].getItem().sellPrice * characterData.inventory.content[i].getItem().amount;
          characterData.inventory.content.splice(i, 1);
          characterData.inventory.capacityLeft++;
        }
      }

      console.log("Total sell price: " + totalSellPrice);
      characterData.addCurrency(CURRENCY_ID.GOLD, totalSellPrice);

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



exports.retireCharacter = functions.https.onCall(async (data, context) => {

  // await RetireCharacter(data.characterUid, context);

  const callerCharacterUid = data.characterUid;
  const batch = admin.firestore().batch();
  const characterDb = await admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);
  // const retiredcharacterDb = await admin.firestore().collection('charactersRetired');
  try {

    const result = await admin.firestore().runTransaction(async (t: any) => {

      const characterDoc = await t.get(characterDb);
      let characterData: CharacterDocument = characterDoc.data();

      const playerDb = admin.firestore().collection("players").doc(characterData.userUid);
      const playerDoc = await t.get(playerDb);
      let playerData: PlayerData = playerDoc.data();

      validateCallerBulletProof(characterData, context);

      for (const inventoryItem of characterData.inventory.content) {
        if (inventoryItem.getItem().rarity == RARITY.ARTIFACT) {
          if (!inventoryItem.content)
            continue;
          const inboxDb = admin.firestore().collection('inboxPlayer').doc();
          const inboxEntry = new InboxItem(inboxDb.id, characterData.userUid, generateContentContainer(generateContent(inventoryItem.content.itemId, inventoryItem.content.amount)), undefined, "Retired hero belongings", "Here are belongings of your retired hero " + characterData.characterName, getCurrentDateTime(480));
          batch.set(inboxDb, JSON.parse(JSON.stringify(inboxEntry))); // Update the document in batch

          //aspon nastavim amout na 0 kdyby nahodou se neco stalo a ten charakter byl pristupny at neduplikuju veci....lepsi by to bylo uplne smazat ale to se mi ted nechce kodovat
          inventoryItem.getItem().amount = 0;
        }
      }

      //updatneme preview na retired
      playerData.characters.find(character => character.characterUid == characterData.uid)!.isRetired = true;

      //odstranim charakter z playera, takze ztrati link na nej ale v DB zustane
      // playerData.characters.splice(playerData.characters.indexOf(playerData.characters.find(character => character.characterUid == characterData.uid)!), 1);

      characterData.isRetired = true;

      //   await t.set(retiredcharacterDb.doc(), JSON.parse(JSON.stringify(characterData)), { merge: true });

      await t.set(playerDb, JSON.parse(JSON.stringify(playerData)), { merge: true });

      await t.set(characterDb, JSON.parse(JSON.stringify(characterData)), { merge: true });

      // await t.delete(characterDb);


      return await batch.commit()
        .then(() => {
          console.log("Batch update completed.");
          return "Batch update successful";
        })
        .catch((e) => {
          console.error("Batch update failed: ", e);
          throw new functions.https.HttpsError("aborted", "Batch update failed: " + e);
        });


    });


    console.log('Transaction success', result);
    return result;
  } catch (e) {
    console.log('Transaction failure:', e);
    throw new functions.https.HttpsError("aborted", "Error : " + e);
  }


});



exports.useTeleportScroll = functions.https.onCall(async (data, context) => {

  const callerCharacterUid = data.characterUid;
  const targetPoI = data.targetPoI;
  const teleportScrollUid = data.teleportScrollUid;
  const characterDb = await admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);

  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {

      const characterDoc = await t.get(characterDb);
      let characterData: CharacterDocument = characterDoc.data();

      const itemToConsume = characterData.getInventoryContent(teleportScrollUid).getItem();

      const destinationPoIDb = admin.firestore().collection('_metadata_zones').doc(characterData.position.zoneId).collection("locations").doc(characterData.position.locationId).collection("pointsOfInterest").doc(targetPoI);
      const destinationPoIDoc = await t.get(destinationPoIDb.withConverter(PointOfInterestConverter));
      let destinationPoIData: PointOfInterest = destinationPoIDoc.data();

      if (compareWorldPosition(destinationPoIData.worldPosition, characterData.position))
        throw ("You are already at this location!");


      if (!characterData.hasExploredPosition(destinationPoIData.worldPosition))
        throw ("You must first explore this position!");

      let myEncounter = await QuerryForCharactersCombatEncounter(t, callerCharacterUid);
      if (myEncounter)
        throw ("This cannot teleport during combat!");

      characterData.addFatiguePercentage(itemToConsume.customData!.integers![0]);
      characterData.removeContentFromInventory(itemToConsume.uid, 1);

      characterData.position = destinationPoIData.worldPosition;

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