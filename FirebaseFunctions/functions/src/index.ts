


import * as functions from "firebase-functions";
import { Equip, EQUIP_SLOT_ID, RARITY, EquipAttributes, ContentItem, Content, ContentCurrency, ContentFood, generateFood, ITEMS } from "./equip";
import { firestoreAutoId } from "./general2";
import * as firebase from 'firebase-admin';
import { Skill, convertSkillToGiveniLevel, Combatskill } from "./skills";
import { CombatBuff, CombatStats, ENCOUNTER_CONTEXT } from "./encounter";
import { EncounterResult, EncounterResultEnemy } from "./encounterResult";
import { LOC, POI, ZONE } from "./worldMap";


//import { Equip, EquipAttributes, EQUIP_SLOT_ID, RARITY } from "./equip";
//import { InventoryBag, CharacterDocument, CharacterStats, Currency, firestoreAutoId, Inventory, CharacterEquip } from "./general2";
//import { convertSkillToGiveniLevel, Skill } from "./skills";

// [START import]
// The Cloud Functions for Firebase SDK to create Cloud Functions and set up triggers.
const admin = require('firebase-admin');
admin.initializeApp();
//admin.firestore().settings({ignoreUndefinedProperties:true}); //to sem tady pridal ja abych mohl mit "undefined" fieldy (takove ty co pouzivam jen programove a pak je mazu a davam undefined abych je nemel v DB ulozene), jinak to hazelo error
// [END import]

//Je to maximalni pocet karet v decku. Melo by to odpovidat presne equipu. Pokud mam 20 equipu a kazdy 1 skill. Pak je to 20.
export const MAX_DECK_SIZE = 24;
export const MAX_FATIGUE = 90;
export const MAX_TRAVEL_TIME = 48;

//Instantni zabiti nepratel v encounterech
export const INSTAKILL = false;

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
  return (firebase.firestore.Timestamp.now().toMillis() + (_hoursToAdd * 3600000)).toString();
}

export function getCurrentDateTimeVersionSecondsAdd(_secondsToAdd: number): string {
  return (firebase.firestore.Timestamp.now().toMillis() + (_secondsToAdd * 1000)).toString();
}

export function getCurrentDateTimeInMillis(_hoursToAdd: number): number {
  return (firebase.firestore.Timestamp.now().toMillis() + (_hoursToAdd * 3600000));
}

export function getMillisPassedSinceTimestamp(_timestamp: string): number {
  return (getCurrentDateTimeInMillis(0) - Number.parseInt(_timestamp));
}

function containsWhitespace(str) {
  return /\s/.test(str);
}


async function createCharacter(_transaction: any, _characterUid: string, _userUid: string, _characterName: string, _characterClass: string): Promise<CharacterDocument> {

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

  // const nonCaseSensitiveCharacterName = characterName.toLowerCase();
  // console.log("nonCaseSensitiveCharacterName: " + nonCaseSensitiveCharacterName);

  //zkontrolujem unikatnost jmena (ignorujem casesensitivitu)
  const characterNamesDb = admin.firestore().collection('characterNames').doc(characterName);
  const characterNamesDoc = await _transaction.get(characterNamesDb);
  let characterNamesData: CharacterNameEntry = characterNamesDoc.data();
  if (characterNamesData != undefined) throw "Name already exists! Please choose different one.";



  console.log("Creating character - uid :" + _characterUid + " userUid: " + _userUid + " characterName: " + characterName + " class: " + _characterClass);
  let stats = new CharacterStats(0, 0, 400, 1, 10, 1, 20, 20, 20, 20, 20);


  const worldPosition: WorldPosition = new WorldPosition(ZONE.DUNOTAR, LOC.VALLEY_OF_TRIALS, POI.VALLEY_OF_TRIALS_PLAINS);

  let bags: InventoryBag[] = [];
  bags.push(new InventoryBag(firestoreAutoId(), "STARTING_BAG", 16));
  const inventory = new Inventory(bags, 16, 16, []);
  inventory.content.push(new ContentContainer(CONTENT_TYPE.FOOD, undefined, undefined, undefined, generateFood(ITEMS.APPLE, 5)));
  const currency = new Currency(1000, 0, 500, 48, 0);
  const timestamps = new Timestamps(getCurrentDateTime(0));
  let startingSkill: Skill = convertSkillToGiveniLevel("SLAM", 1);
  const startingBody = new Equip(firestoreAutoId(), "EQUIP", "Apprentice Shirt", "BODY_1", 1, EQUIP_SLOT_ID.BODY, RARITY.COMMON, 1, 1, new EquipAttributes(0, 0, 0, 0, 0, 1, 10), startingSkill);
  const startingHead = new Equip(firestoreAutoId(), "EQUIP", "Apprentice Cap", "HEAD_1", 1, EQUIP_SLOT_ID.HEAD, RARITY.COMMON, 1, 1, new EquipAttributes(0, 0, 0, 0, 0, 1, 10), startingSkill);
  const startingLegs = new Equip(firestoreAutoId(), "EQUIP", "Apprentice Pants", "LEGS_1", 1, EQUIP_SLOT_ID.LEGS, RARITY.COMMON, 1, 1, new EquipAttributes(0, 0, 0, 0, 0, 1, 10), startingSkill);

  let characterPortrait = "CHARACTER_PORTRAIT_0";
  if (_characterClass == CHARACTER_CLASS.WARLOCK) {
    characterPortrait = "CHARACTER_PORTRAIT_WARLOCK";
    stats = new CharacterStats(0, 0, 400, 1, 10, 1, 21, 20, 19, 22, 18);
  }
  else if (_characterClass == CHARACTER_CLASS.WARRIOR) {
    characterPortrait = "CHARACTER_PORTRAIT_WARRIOR";
    stats = new CharacterStats(0, 0, 400, 1, 10, 1, 22, 19, 22, 19, 18);
  }
  else {
    characterPortrait = "CHARACTER_PORTRAIT_SHAMAN";
    stats = new CharacterStats(0, 0, 400, 1, 10, 1, 19, 21, 20, 21, 19);
  }

  let characteEquip: Equip[] = [];
  // const startingCharacterEquip = new CharacterEquip(EQUIP_SLOT_ID.BODY, startingEquip);
  characteEquip.push(startingBody);
  characteEquip.push(startingHead);
  characteEquip.push(startingLegs);

  let exploredPosition: ExploredPositions = new ExploredPositions([], []);
  exploredPosition.locations.push(LOC.VALLEY_OF_TRIALS);
  exploredPosition.pointsOfInterest.push(POI.VALLEY_OF_TRIALS_PLAINS);

  return new CharacterDocument(_characterUid, _userUid, characterName, _characterClass, inventory, characteEquip, currency, stats, worldPosition, [], [], timestamps, characterPortrait, exploredPosition);

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

export function validateCallerBulletProof(_characterData: CharacterDocument, _context: functions.https.CallableContext) {

  //jediny uplne cheat proof je jeste ziskat charakter dokument za ktery se vydavam z toho dokuemntu ziskaat userUid(playerUid) a zkontrolovat ze odpovidat conterxt.UserUid
  //tak jedine si muzu byt 100% jisty ze ten clovek co vola je i vlastnikem toho charakteru...
  const userUid = _characterData.userUid;

  if (userUid == undefined) {
    console.log("player add UserUid to the client when calling CloudFucntion so I can check for validity of user! ");
    return;
  }

  console.log("player s uid :" + _context.auth?.uid + " vola metodu a tvrdi ze je : " + userUid);
  if (userUid != _context.auth?.uid)
    throw "User caller mismatach!";
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
}


export const enum CURRENCY_ID {
  SILVER = "SILVER",
  GOLD = "GOLD",
  TIME = "TIME",
  FATIGUE = "FATIGUE"

}

export const enum CHARACTER_CLASS {
  WARRIOR = "WARRIOR",
  WARLOCK = "WARLOCK",
  MAGE = "MAGE",
  SHAMAN = "SHAMAN",
  ANY = "ANY",
}



//[ContentContainer]
export class ContentContainer {
  constructor(
    public contentType: string,// ITEM, EQUIP, CURRENCY....?
    public contentItem: ContentItem | undefined,
    public contentEquip: Equip | undefined,
    public contentCurrency: ContentCurrency | undefined,
    public contentFood: ContentFood | undefined,
  ) { }

  getItem(): Content {
    switch (this.contentType) {
      case CONTENT_TYPE.EQUIP: if (this.contentEquip != undefined) return this.contentEquip; else throw ("ERROR: There is no Equip entry when there should be!");
      case CONTENT_TYPE.ITEM: if (this.contentItem != undefined) return this.contentItem; else throw ("ERROR: There is no Item entry when there should be!");
      case CONTENT_TYPE.CURRENCY: if (this.contentCurrency != undefined) return this.contentCurrency; else throw ("ERROR: There is no Currency entry when there should be!");
      case CONTENT_TYPE.FOOD: if (this.contentFood != undefined) return this.contentFood; else throw ("ERROR: There is no Food entry when there should be!");


      default: throw ("ERROR : Invalid Item type in BaseContent!");
    }
  }
}
//[ContentContainer]

//[World Position]
export class WorldPosition {
  constructor(
    public zoneId: string,
    public locationId: string,
    public pointOfInterestId: string,
  ) { }
}
//[World Position]

//[Monster Kill]
export class SimpleTally {
  constructor(
    public id: string,
    public count: number,
  ) { }
}
//[Monster Kill]


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

export class ExploredPositions {
  constructor(
    public pointsOfInterest: string[],
    public locations: string[],

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
    public timestamps: Timestamps,
    public characterPortrait: string,
    public exploredPositions: ExploredPositions
    // public isJoinedInEncounter: boolean,
  ) { }

  grantStatsForLevelUp() {

    switch (this.characterClass) {
      case CHARACTER_CLASS.WARRIOR:
        this.stats.intellect += this.stats.level % 2 != 0 ? 1 : 0; //pouze kazdy lichy level
        this.stats.agility += this.stats.level % 2 == 0 ? 1 : 0; //pouze kazdy sudy level
        this.stats.stamina += 1
        this.stats.strength += 2;
        this.stats.spirit += 1;
        break;
      case CHARACTER_CLASS.WARLOCK:
        this.stats.intellect += 2;
        this.stats.agility += this.stats.level % 2 == 0 ? 1 : 0; //pouze kazdy sudy level
        this.stats.stamina += 1
        this.stats.strength += this.stats.level % 2 != 0 ? 1 : 0; //pouze kazdy lichy level
        this.stats.spirit += 1;
        break;
      case CHARACTER_CLASS.SHAMAN:
        this.stats.intellect += 1;
        this.stats.agility += 1
        this.stats.stamina += 1
        this.stats.strength += 1
        this.stats.spirit += 1;
        break;


      default:
        this.stats.intellect += 1;
        this.stats.agility += 1;
        this.stats.stamina += 1;
        this.stats.strength += 1;
        this.stats.spirit += 1;
        break;
    }

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

  equipEquipment(_equipToEquipUids: string[]) {

    let equipToEquip: Equip[] = [];

    //Vsechen equip co mam na sobe dam do inventare
    this.equipment.forEach(equip => {
      this.addContentToInventory(new ContentContainer(CONTENT_TYPE.EQUIP, undefined, equip, undefined, undefined), false, true)
    });

    //Ulozim equip ktery si chci equiupnout
    for (const content of this.inventory.content) {
      console.log("checkuju jesti si tento equip chci nasadit:  " + content.getItem().uid);
      if (_equipToEquipUids.includes(content.getItem().uid)) {

        if ((content.getItem() as Equip).level > this.stats.level)
          throw (content.getItem() as Equip).displayName + " level is too high!";

        // if ((content.getItem() as Equip).skill.characterClass != this.characterClass)
        // throw (content.getItem() as Equip).displayName + " item is not for your class!";

        equipToEquip.push(content.getItem() as Equip);
        console.log("jo chci tak pushuju:  " + content.getItem().uid);
      }
    }

    console.log("tohle si chces equipnout na sebe: ")
    equipToEquip.forEach(element => {
      console.log(element.displayName + " : " + element.uid);

    });


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

  }

  getInventoryContent(_equipToGetUid: string): ContentContainer {

    let foundContent: ContentContainer | null = null;

    for (var i = this.inventory.content.length - 1; i >= 0; i--) {
      if (_equipToGetUid == this.inventory.content[i].getItem().uid) {
        foundContent = this.inventory.content[i];
      }
    }

    if (foundContent == null)
      throw ("Could not find content with UID :" + _equipToGetUid + " in your inventory!");

    return foundContent;
  }


  removeContentFromInventory(_contentToRemoveUid: string, _amount: number) {
    let foundCountent = false;
    for (var i = this.inventory.content.length - 1; i >= 0; i--) {
      if (_contentToRemoveUid == this.inventory.content[i].getItem().uid) {

        if (this.inventory.content[i].getItem().amount < _amount)
          throw ("Not enough of UID :" + _contentToRemoveUid + " in your inventory! You have : " + this.inventory.content[i].getItem().amount + " but you want to remove " + _amount);

        this.inventory.content[i].getItem().amount -= _amount;

        if (this.inventory.content[i].getItem().amount == 0) {
          this.inventory.content.splice(i, 1);
          this.inventory.capacityLeft++;
        }
        foundCountent = true;
      }
    }

    if (!foundCountent)
      throw ("Could not find content with UID :" + _contentToRemoveUid + " in your inventory!");
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
      throw ("You dont have enought " + _contentToRemoveId + " items in your inventory! You need : " + _amountToRemove + "but you have only: " + (_amountToRemove - amoutOfItemsLeftToRemove));
  }




  addContentToInventory(_contentToAdd: ContentContainer, _generateNewUid: boolean, _ignoreCapacity: boolean) { //defakto vzdycky chci generovat novy Uid,jedine kdyz swapuju itemy mezi inventarem a charakterem a ocekavam ze ty itemy zas najdu podle id tak davam false

    //Hack abych ziskal pristup k meetodatm BaseContentu bez toho abych musel pouzivat WithConvertor
    _contentToAdd = new ContentContainer(_contentToAdd.contentType, _contentToAdd.contentItem, _contentToAdd.contentEquip, _contentToAdd.contentCurrency, _contentToAdd.contentFood);


    console.log("Davam item : " + _contentToAdd.getItem().itemId);
    //pokud je ten content co chci dat do inventare currency, tak to nedam do inventare ale zvednu hraci jeho cucrrency
    if (_contentToAdd.contentType == CONTENT_TYPE.CURRENCY) {
      console.log("Mam tu nejakou currency : " + (_contentToAdd.getItem() as ContentCurrency).itemId);
      this.addCurrency((_contentToAdd.getItem() as ContentCurrency).itemId, (_contentToAdd.getItem() as ContentCurrency).amount);
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

        this.inventory.content.push(new ContentContainer(_contentToAdd.contentType, _contentToAdd.contentItem, _contentToAdd.contentEquip, _contentToAdd.contentCurrency, _contentToAdd.contentFood));
        this.inventory.capacityLeft--;
        console.log("Item " + _contentToAdd.getItem().itemId + " added to inventory!");
      }
      else
        throw "Cant add new simple item " + _contentToAdd.getItem().itemId + " inventory is full!";
    }
  }

  addCurrency(_currencyType: string, _amount: number) {
    switch (_currencyType) {
      case CURRENCY_ID.GOLD: this.addGold(_amount); break;
      case CURRENCY_ID.SILVER: this.addSilver(_amount); break;
      case CURRENCY_ID.TIME: this.addTime(_amount); break;
      case CURRENCY_ID.FATIGUE: this.addFatigue(_amount); break;
      default:
        break;
    }
  }

  subCurrency(_currencyType: string, _amount: number) {
    switch (_currencyType) {
      case CURRENCY_ID.GOLD: this.subGold(_amount); break;
      case CURRENCY_ID.SILVER: this.subSilver(_amount); break;
      case CURRENCY_ID.TIME: this.subTime(_amount); break;
      case CURRENCY_ID.FATIGUE: this.subFatigue(_amount); break;

      default:
        break;
    }
  }

  subSilver(_amount: number) {
    if (this.currency.silver >= _amount)
      this.currency.silver -= _amount;
    else
      throw "Not enough Silver! You have " + this.currency.silver + " but want to subsctract " + _amount;
  }


  subGold(_amount: number) {
    if (this.currency.gold >= _amount)
      this.currency.gold -= _amount;
    else
      throw "Not enough Gold! You have " + this.currency.gold + " but want to subsctract " + _amount;
  }

  subTime(_amount: number) {
    if (this.currency.time >= _amount)
      this.currency.time -= _amount;
    else
      throw "Not enough Time! You have " + this.currency.time + " but want to subsctract " + _amount;
  }

  subFatigue(_amount: number) {
    // if (this.currency.fatigue >= _amount)
    this.currency.fatigue -= _amount;

    if (this.currency.fatigue < 0)
      this.currency.fatigue = 0;
    //throw "Not enough Time! You have " + this.currency.time + " but want to subsctract " + _amount;
  }


  addFatigue(_amount: number) {
    if (_amount + this.currency.fatigue > MAX_FATIGUE)
      this.currency.fatigue = MAX_FATIGUE;
    else
      this.currency.fatigue += _amount;
  }

  addTime(_amount: number) {
    if (_amount + this.currency.time > MAX_TRAVEL_TIME)
      this.currency.time = MAX_TRAVEL_TIME;
    else
      this.currency.time += _amount;
  }

  addSilver(_amount: number) {
    this.currency.silver += _amount;
  }

  addGold(_amount: number) {
    this.currency.gold += _amount;
  }

  converEquipToCombatSkills(): Combatskill[] {
    let combatSkills: Combatskill[] = [];

    this.equipment.forEach(equip => {
      // const totals: StatsSkill = skill.getStatsTotal();

      let combatBuff: CombatBuff | undefined = undefined;
      if (equip.skill.buff != undefined)
        combatBuff = new CombatBuff(equip.skill.buff.buffId, equip.skill.buff.durationTurns, equip.skill.buff.durationTurns, equip.skill.buff.amounts, equip.skill.buff.rank)

      combatSkills.push(new Combatskill(equip.skill.skillId, equip.skill.characterClass, equip.skill.manaCost, equip.skill.amounts, false, -1, equip.skill.rank, combatBuff));

    });

    var combatSkillsLength = combatSkills.length;
    //skilly z prazdnych equip slotu nahradim "punch" skilem
    for (let index = 0; index < MAX_DECK_SIZE - combatSkillsLength; index++) {
      combatSkills.push(new Combatskill("PUNCH", CHARACTER_CLASS.ANY, 10, [10], false, -1, 1, undefined));
    }

    return combatSkills;
  }

  converStatsToCombatStats(): CombatStats {

    let totalAgility: number = this.stats.agility;
    this.equipment.forEach(_equip => { totalAgility += _equip.attributes.agility; });

    let totalStrength: number = this.stats.strength;
    this.equipment.forEach(_equip => { totalStrength += _equip.attributes.strength; });

    let totalStamina: number = this.stats.stamina;
    this.equipment.forEach(_equip => { totalStamina += _equip.attributes.stamina; });

    let totalIntellect: number = this.stats.intellect;
    this.equipment.forEach(_equip => { totalIntellect += _equip.attributes.intellect; });

    let totalSpirit: number = this.stats.spirit;
    this.equipment.forEach(_equip => { totalSpirit += _equip.attributes.spirit; });

    let totalMana = totalIntellect * this.stats.manaMultiplier;
    let totalHealth = totalStamina * this.stats.healthMultiplier;

    //Aplikuju fatigue penalty
    totalMana = Math.round((totalMana / 100) * (100 - this.currency.fatigue));
    totalHealth = Math.round((totalHealth / 100) * (100 - this.currency.fatigue));

    return new CombatStats(totalMana, totalMana, totalHealth, totalHealth, totalStamina, totalIntellect, totalAgility, totalSpirit, totalStrength, 0);
  }



  giveExp(_encounterResultToGiveExpFrom: EncounterResult) {

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

    const characterLevel = this.stats.level;

    const TotalEXPReward = getExpAmountFromEncounterForGivenCharacterLevel(_encounterResultToGiveExpFrom.enemies, characterLevel);
    console.log("Total exp gained: " + TotalEXPReward);
    this.stats.exp += TotalEXPReward;


    //ZKORNTROLUJU JESTLI SEM NEDOSSTAL LEVEL UP!

    // Diff(CL) =       0,  for    CL <= 28
    // Diff(CL) =       1,     CL  = 29
    // Diff(CL) =       3,     CL  = 30
    // Diff(CL) =       6,     CL  = 31
    // Diff(CL) = 5 x (CL-30), CL >= 32, <=59

    const Diff = 0;
    //const Diff = 5x(characterLevel-30);

    // const ExpNeededForGivenLevel = ((8 * characterLevel) + Diff) * ((5 * characterLevel) + 45);

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

      // this.stats.intellect += 1;
      // this.stats.agility += 1;
      // this.stats.stamina += 1;
      // this.stats.strength += 1;
      // this.stats.spirit += 1;

      this.subCurrency(CURRENCY_ID.FATIGUE, 100);
      // this.stats.manaMax = this.stats.intellect * 15;
      //   this.stats.healthMax = this.stats.stamina * 10;

    }
  }

}

export class CharacterStats {
  constructor(

    public exp: number,
    public expNeededToReachLastLevel: number,
    public expNeededToReachNextLevel: number,
    public level: number,
    //  public healthMax: number,
    //  public manaMax: number,
    public healthMultiplier: number,
    public manaMultiplier: number,
    public stamina: number,
    public spirit: number,
    public strength: number,
    public intellect: number,
    public agility: number
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
    public silver: number,
    public food: number,
    public time: number,
    public fatigue: number,
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
      timestamps: character.timestamps,
      characterPortrait: character.characterPortrait,
      exploredPositions: character.exploredPositions
      // isJoinedInEncounter: character.isJoinedInEncounter

    };
  },
  fromFirestore: (snapshot: any, options: any) => {
    const data = snapshot.data(options);


    let contentRemade: ContentContainer[] = [];
    data.inventory.content.forEach(content => {
      contentRemade.push(new ContentContainer(content.contentType, content.contentItem, content.contentEquip, content.contentCurrency, content.contentFood));
    });

    let bagsRemade: InventoryBag[] = [];
    data.inventory.bags.forEach(bag => {
      bagsRemade.push(new InventoryBag(bag.uid, bag.itemId, bag.capacity));
    });


    let inventoryRemade = new Inventory(bagsRemade, data.inventory.capacityMax, data.inventory.capacityLeft, contentRemade);


    return new CharacterDocument(data.uid, data.userUid, data.characterName, data.characterClass, inventoryRemade, data.equipment, data.currency, data.stats, data.position, data.monsterKills, data.questgiversClaimed, data.timestamps, data.characterPortrait, data.exploredPositions);//, data.isJoinedInEncounter);
    // return new CharacterDocument(data.uid, data.userUid, data.characterName, data.characterClass, data.inventory, data.equipment, data.currency, data.stats, data.position, data.monsterKills, data.questgiversClaimed);
  }
}
// [Character]




export class PlayerData {
  constructor(
    public uid: string,
    public playerName: string,
    public country: string,
    public gems: number,
    public characters: CharacterPreview[]

  ) { }
}

export class CharacterPreview {
  constructor
    (
      public uid: string,
      public name: string,
      public characterClass: string,
      public level: number,
      public portrait: string
    ) { }
}

export async function QuerryHasCharacterAnyUnclaimedEncounterResult(_transaction: any, _characterUid: string): Promise<boolean> {
  const encounterDb = admin.firestore().collection("encounterResults").where("combatantsWithUnclaimedRewardsList", "array-contains", _characterUid);

  //najdu jestli mas nejaky unclaimnuty encounter Result
  let hasUnclaimedEncounterResult = false;
  await _transaction.get(encounterDb).then(querry => {
    hasUnclaimedEncounterResult = querry.size > 0
  });

  return hasUnclaimedEncounterResult
}

export async function QuerryIfCharacterIsInAnyEncounter(_transaction: any, _characterUid: string): Promise<boolean> {
  const encounterDb = admin.firestore().collection("encounters").where("combatantList", "array-contains", _characterUid);

  //najdu jestli si v nejakem encounteru clenem
  let participatingInEncounter = false;
  await _transaction.get(encounterDb).then(querry => {
    participatingInEncounter = querry.size > 0
  });

  return participatingInEncounter
}


export async function QuerryIfCharacterIsWatcherInAnyDungeonEncounter(_transaction: any, _characterUid: string): Promise<boolean> {
  const encounterDb = admin.firestore().collection("encounters").where("watchersList", "array-contains", _characterUid).where("encounterContext","==",ENCOUNTER_CONTEXT.DUNGEON);

  //najdu jestli si v nejakem encounteru clenem
  let participatingInEncounter = false;
  await _transaction.get(encounterDb).then(querry => {
    participatingInEncounter = querry.size > 0
  });

  return participatingInEncounter
}


//creates Player and starting Character for him
exports.createPlayer = functions.auth.user().onCreate(async (user) => {

  // get user data from the auth trigger
  const userUid = user.uid; // The UID of the user.

  // const characterRef = admin.firestore().collection("characters").doc();
  const playeref = admin.firestore().collection("players").doc(userUid);

  //const characterUid: string = characterRef.id;


  // set account  doc  
  const playerData = {
    uid: playeref.id,
    playerName: userUid,
    country: "US",
    gems: 10,
    characters: []
  }


  return playeref.set(playerData);
});


class CharacterNameEntry {
  constructor(public playerUid: string) { }
}

exports.createCharacter = functions.https.onCall(async (data, context) => {


  //const encounterResultUid = data.encounterResultUid;
  const callerPlayerUid = data.playerUid;
  const characterName = data.characterName;
  const characterClass = data.characterClass;

  const characterNamesDb = admin.firestore().collection("characterNames");
  const characterRef = admin.firestore().collection("characters").doc();
  const playerRef = admin.firestore().collection("players").doc(callerPlayerUid);

  const characterUid: string = characterRef.id;

  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {
      const playerDoc = await t.get(playerRef);
      let playerdata: PlayerData = playerDoc.data();


      const newCharacter = await createCharacter(t, characterUid, callerPlayerUid, characterName, characterClass);

      const characterPreview = new CharacterPreview(newCharacter.uid, newCharacter.characterName, newCharacter.characterClass, newCharacter.stats.level, newCharacter.characterPortrait);

      playerdata.characters.push(characterPreview);


      const characterNameEntry = new CharacterNameEntry(playerdata.uid);

      t.set(characterNamesDb.doc(newCharacter.characterName), JSON.parse(JSON.stringify(characterNameEntry)));
      t.set(playerRef, JSON.parse(JSON.stringify(playerdata)), { merge: true });
      t.set(characterRef, JSON.parse(JSON.stringify(newCharacter)));

      return "OK";
    });




    console.log('Transaction success', result);
    return result;
  } catch (e) {
    console.log('Transaction failure:', e);
    throw new functions.https.HttpsError("aborted", "Error : " + e);
  }
});


exports.claimTimePool = functions.https.onCall(async (data, context) => {

  const FATIGUE_RECOVERED_PER_HOUR = 10;
  const TIME_GAINED_PER_HOUR = 3;
  const callerCharacterUid = data.characterUid;

  const characterDb = await admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);

  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {

      const characterDoc = await t.get(characterDb);
      let characterData: CharacterDocument = characterDoc.data();

      const currentTime = getCurrentDateTimeInMillis(0);
      const hoursPassed = millisToHours(currentTime - Number.parseInt(characterData.timestamps.lastClaimTime));
      const wholeHoursPassed = Math.floor(hoursPassed);
      const lefoverAfterRoundingInMillis = hourstoMillis(hoursPassed - wholeHoursPassed);

      console.log("Hours passed since you claimed Time pool : " + hoursPassed);
      const timeGained = wholeHoursPassed * TIME_GAINED_PER_HOUR;
      const fatigueRecovered = wholeHoursPassed * FATIGUE_RECOVERED_PER_HOUR;
      console.log("You gained as much as: " + timeGained + " time");
      console.log("You recover as much as : " + fatigueRecovered + "% fatigue");

      if (wholeHoursPassed >= 1) {
        characterData.addCurrency(CURRENCY_ID.TIME, timeGained);
        characterData.subCurrency(CURRENCY_ID.FATIGUE, fatigueRecovered);
        characterData.timestamps.lastClaimTime = (currentTime - lefoverAfterRoundingInMillis).toString();
        t.set(characterDb, JSON.parse(JSON.stringify(characterData)), { merge: true });
      }

      return "OK";
    });


    console.log('Transaction success', result);
    return result;
  } catch (e) {
    console.log('Transaction failure:', e);
    throw new functions.https.HttpsError("aborted", "Error : " + e);
  }


});

exports.eatFood = functions.https.onCall(async (data, context) => {

  const callerCharacterUid = data.characterUid;
  const consumableUid = data.consumanleUid;

  const characterDb = await admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);

  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {

      const characterDoc = await t.get(characterDb);
      let characterData: CharacterDocument = characterDoc.data();


      const foodToConsume: ContentFood = characterData.getInventoryContent(consumableUid).getItem() as ContentFood;

      characterData.subCurrency(CURRENCY_ID.FATIGUE, foodToConsume.fatigueRecoveryBonus);
      characterData.addCurrency(CURRENCY_ID.TIME, foodToConsume.timeBonus);

      characterData.removeContentFromInventory(foodToConsume.uid, 1);

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
        if (character.uid == characterToDeleteUid) {
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


  // [END allAdd]