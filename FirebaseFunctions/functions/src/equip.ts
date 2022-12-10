
// [START import]
import * as functions from "firebase-functions";
import { characterDocumentConverter, CharacterDocument, ContentContainer, CONTENT_TYPE, CHARACTER_CLASS } from ".";
import { firestoreAutoId, randomIntFromInterval } from "./general2";
import { addSkillToEquip, Skill } from "./skills";
const admin = require('firebase-admin');
//const { getFirestore, Timestamp, FieldValue } = require('firebase-admin/firestore');
//const { FieldValue } = require('firebase-admin/firestore');
// // [END import]



export const enum ITEMS {

  //quest items
  DEATH_MEDAILON = "DEATH_MEDAILON",
  BOAR_TUSK = "BOAR_TUSK",
  //junk
  OLD_BOOT = "OLD_BOOT",
  //food
  RIBEYE = "RIBEYE",
  BERRY = "BERRY",
  APPLE = "APPLE",
}


export class Content {//implements ISellable {
  constructor(

    public uid: string,
    public itemId: string,
    // public displayName: string,
    public rarity: string,
    //  public level: number,
    public sellPrice: number,
    //  public imageId: string,
    public stackSize: number,
    public amount: number,
  ) { }

}

export class ContentCurrency extends Content {// implements IInventoryItem {
  constructor(

    //  public item: ItemSimple,
    public uid: string,
    public itemId: string,
    //  public displayName: string,
    public sellPrice: number,
    //  public imageId: string,
    public stackSize: number,
    public amount: number,
    public rarity: string,

    //public currencyType: string

  ) { super(uid, itemId, rarity, sellPrice, stackSize, amount) }

}


export class ContentFood extends Content {
  constructor(

    //  public item: ItemSimple,
    public uid: string,
    public itemId: string,
    // public displayName: string,
    public sellPrice: number,
    //   public imageId: string,
    public stackSize: number,
    public amount: number,
    public rarity: string,

    public fatigueRecoveryBonus: number,
    public timeBonus: number

  ) { super(uid, itemId, rarity, sellPrice, stackSize, amount) }

}

export class ContentItem extends Content {// implements IInventoryItem {
  constructor(

    //  public item: ItemSimple,
    public uid: string,
    public itemId: string,
    //public displayName: string,
    public sellPrice: number,
    // public imageId: string,
    public stackSize: number,
    public amount: number,
    public rarity: string,

    public level: number,

  ) { super(uid, itemId, rarity, sellPrice, stackSize, amount) }

}



export class Equip extends Content {

  constructor(
    public uid: string,
    public itemId: string,
    public displayName: string,
    public imageId: string,
    public sellPrice: number,

    public equipSlotId: EQUIP_SLOT_ID,
    public rarity: string,
    public level: number,
    public iLevel: number,
    //  public equipSet: number,
    public attributes: EquipAttributes,

    public skill: Skill

  ) { super(uid, itemId, rarity, sellPrice, 1, 1) }

}

export class EquipAttributes {
  constructor(
    public intellect: number,
    public stamina: number,
    public strength: number,
    public agility: number,
    public spirit: number,
    public armor: number,
    public durability: number
  ) { }

  setAttributeById(_attributeId: ATTRIBUTE_ID, _amount: number) {
    if (_attributeId == ATTRIBUTE_ID.STRENGTH)
      this.strength += _amount;
    else if (_attributeId == ATTRIBUTE_ID.INTELLECT)
      this.intellect += _amount;
    else if (_attributeId == ATTRIBUTE_ID.SPIRIT)
      this.spirit += _amount;
    else if (_attributeId == ATTRIBUTE_ID.AGILITY)
      this.agility += _amount;
    else if (_attributeId == ATTRIBUTE_ID.STAMINA)
      this.stamina += _amount;
  }
}


export const enum RARITY {  //rarity jeste ovlivnuji jake stary a KOLIK bude na itemu , UNCOMMON 1-2 , RARE,EPIC- 2-4 
  COMMON = "COMMON",
  UNCOMMON = "UNCOMMON",
  RARE = "RARE",
  EPIC = "EPIC"
}

export const enum ATTRIBUTE_ID {
  STRENGTH = "STRENGTH",
  AGILITY = "AGILITY",
  INTELLECT = "INTELLECT",
  STAMINA = "STAMINA",
  SPIRIT = "SPIRIT",
}


export const enum EQUIP_SLOT_ID {
  ANY = "ANY",
  HEAD = "HEAD",
  BODY = "BODY",
  LEGS = "LEGS",
  FINGER_1 = "FINGER_1",
  HANDS = "HANDS",
  FEET = "FEET",
  AMULET = "AMULET",
  TRINKET = "TRINKET",
  WAIST = "WAIST",
  BACK = "BACK",
  WRIST = "WRIST",
  NECK = "NECK",
  SHOULDER = "SHOULDER",
  OFF_HAND = "OFF_HAND",
  MAIN_HAND = "MAIN_HAND",
}

export const AllEquipSlots = [
  EQUIP_SLOT_ID.BODY,
  EQUIP_SLOT_ID.HEAD,
  EQUIP_SLOT_ID.LEGS,
  EQUIP_SLOT_ID.FINGER_1,
  EQUIP_SLOT_ID.AMULET,
  EQUIP_SLOT_ID.BACK,
  EQUIP_SLOT_ID.FEET,
  EQUIP_SLOT_ID.HANDS,
  EQUIP_SLOT_ID.HEAD,
  EQUIP_SLOT_ID.MAIN_HAND,
  EQUIP_SLOT_ID.NECK,
  EQUIP_SLOT_ID.OFF_HAND,
  EQUIP_SLOT_ID.SHOULDER,
  EQUIP_SLOT_ID.TRINKET,
  EQUIP_SLOT_ID.WAIST,
  EQUIP_SLOT_ID.WRIST];


// export const enum EQUIP_SLOT_BUDGET_MULTIPLIER {
//   HEAD = 1,
//   CHEST = 1,
//   LEGS = 1,
//   FINGER_1 = 0.55
// }

// export const enum ATTRIBUTE_PRICE {
//   STRENGTH = 1,
//   CHEST = 1,
//   LEGS = 1,
//   FINGER_1 = 0.55
// }

export function generateContentItemSimple(_itemId: string, _amount: number): ContentItem {
  //Tady tohle "generovani simple itemu jen nahrazuje 1 read z metadat kde by byly vsechny tyhle itemy? stoji to za to? neni to zadna logika pridana teda he tu stack size a sell price...... ale ty image id a displayname jsou 
  //duplicitni s tim co je v metadatech....."
  //let displayName: string = "";
  let level: number = 0;
  let sellPrice: number = 0;
  let stackSize: number = 1;
  //let imageId: string = "NONE";
  let rarity: RARITY = RARITY.COMMON;
  if (_itemId == ITEMS.BOAR_TUSK) { sellPrice = 5; stackSize = 20 }
  else if (_itemId == ITEMS.OLD_BOOT) { sellPrice = 10; stackSize = 1 }
  else if (_itemId == ITEMS.DEATH_MEDAILON) { sellPrice = 50; stackSize = 1 }


  return new ContentItem(firestoreAutoId(), _itemId, sellPrice, stackSize, _amount, rarity, level);

}


export function generateFood(_itemId: string, _amount: number): ContentFood {
  //Tady tohle "generovani simple itemu jen nahrazuje 1 read z metadat kde by byly vsechny tyhle itemy? stoji to za to? neni to zadna logika pridana....."
  //let displayName: string = "";
  let sellPrice: number = 0;
  let stackSize: number = 1;
 // let imageId: string = "NONE";
  let rarity: RARITY = RARITY.COMMON;
  let fatigueRecoveryBonus = 1;
  let timeBonus = 1;

  if (_itemId == ITEMS.APPLE) {  sellPrice = 5;  stackSize = 20, fatigueRecoveryBonus = 5, timeBonus = 0 }
  else if (_itemId == ITEMS.BERRY) { sellPrice = 10; stackSize = 20, fatigueRecoveryBonus = 0, timeBonus = 3 }
  else if (_itemId == ITEMS.RIBEYE) {  sellPrice = 100; stackSize = 20, fatigueRecoveryBonus = 80, timeBonus = 0 }

  return new ContentFood(firestoreAutoId(), _itemId, sellPrice, stackSize, _amount, rarity, fatigueRecoveryBonus, timeBonus);

}

function generateRandomName(_equip: Equip): string {
  const prefix: string[] = ["Godly", "Great", "Sturdy", "Rusty", "Mighty", "Iron", "Rusty", "Glorious"]
  const name: string[] = ["Item", "Thing", "Equipment", "Object", "Stuff", "Construct"]
  const suffix: string[] = ["of the Wild", " of Silent", "of Hero", "of King", "of Beggar", "of Lord", "of Peasant"]

  return prefix[randomIntFromInterval(0, prefix.length - 1)] + " " + name[randomIntFromInterval(0, name.length - 1)] + " " + suffix[randomIntFromInterval(0, suffix.length - 1)]

}


export function generateEquip(_mLevel: number, _rarity: string, _equipSlotId: string, _characterClass: string): Equip {

  const attributePricesMeta =
  {
    "attributePrices":
      [
        {
          "attributeId": ATTRIBUTE_ID.STRENGTH,
          "price": 1,
        },
        {
          "attributeId": ATTRIBUTE_ID.AGILITY,
          "price": 1,
        },
        {
          "attributeId": ATTRIBUTE_ID.INTELLECT,
          "price": 1,
        },
        {
          "attributeId": ATTRIBUTE_ID.STAMINA,
          "price": 1,
        },
        {
          "attributeId": ATTRIBUTE_ID.SPIRIT,
          "price": 1,
        }
      ],

  }

  const equipMeta =
  {
    "equipSlotIds":
      [
        {
          "slotId": EQUIP_SLOT_ID.HEAD,
          "budgetMultiplier": 1,
          "imageId": "HEAD_1"
        },
        {
          "slotId": EQUIP_SLOT_ID.BODY,
          "budgetMultiplier": 1,
          "imageId": "BODY_1"
        },
        {
          "slotId": EQUIP_SLOT_ID.LEGS,
          "budgetMultiplier": 1,
          "imageId": "LEGS_1"
        },
        {
          "slotId": EQUIP_SLOT_ID.FEET,
          "budgetMultiplier": 0.77,
          "imageId": "FEET_1"
        },
        {
          "slotId": EQUIP_SLOT_ID.HANDS,
          "budgetMultiplier": 0.77,
          "imageId": "HANDS_1"
        },
        {
          "slotId": EQUIP_SLOT_ID.AMULET,
          "budgetMultiplier": 0.7,
          "imageId": "AMULET_1"
        },
        {
          "slotId": EQUIP_SLOT_ID.FINGER_1,
          "budgetMultiplier": 0.55,
          "imageId": "FINGER_1"
        },
        {
          "slotId": EQUIP_SLOT_ID.TRINKET,
          "budgetMultiplier": 0.7,
          "imageId": "TRINKET_1"
        },
        {
          "slotId": EQUIP_SLOT_ID.SHOULDER,
          "budgetMultiplier": 0.77,
          "imageId": "SHOULDER_1"
        }, {
          "slotId": EQUIP_SLOT_ID.BACK,
          "budgetMultiplier": 0.55,
          "imageId": "BACK_1"
        },
        {
          "slotId": EQUIP_SLOT_ID.NECK,
          "budgetMultiplier": 0.55,
          "imageId": "NECK_1"
        },
        {
          "slotId": EQUIP_SLOT_ID.WRIST,
          "budgetMultiplier": 0.55,
          "imageId": "WRIST_1"
        },
        {
          "slotId": EQUIP_SLOT_ID.MAIN_HAND,
          "budgetMultiplier": 1,
          "imageId": "MAIN_HAND_1"
        },
        {
          "slotId": EQUIP_SLOT_ID.OFF_HAND,
          "budgetMultiplier": 0.55,
          "imageId": "OFF_HAND_1"
        },
        {
          "slotId": EQUIP_SLOT_ID.WAIST,
          "budgetMultiplier": 0.77,
          "imageId": "WAIST_1"
        }
      ],

  }


  let dummySkill: Skill = new Skill("", "", 0, [], 0, undefined);
  let generatedEquip: Equip = new Equip(firestoreAutoId(), "EQUIP", "Item of Malakus", "NONE", 0, EQUIP_SLOT_ID.BODY, RARITY.COMMON, 0, 0, new EquipAttributes(0, 0, 0, 0, 0, 0, 0), dummySkill);

  const iLevel = _mLevel + 5;
  let budget: number = 0;
  if (_rarity == RARITY.UNCOMMON) {
    budget = 0.5 * iLevel - 2;

  }
  else {
    budget = 0.625 * iLevel - 1.15;
  }

  generatedEquip.rarity = _rarity;
  generatedEquip.iLevel = iLevel;
  generatedEquip.level = _mLevel;
  generatedEquip.sellPrice = iLevel * 9;

  addSkillToEquip(generatedEquip, _characterClass);

  //choose equip slot randomly if not specified
  let choosenEquipSlot = equipMeta.equipSlotIds[0];
  if (_equipSlotId == EQUIP_SLOT_ID.ANY) {
    choosenEquipSlot = equipMeta.equipSlotIds[randomIntFromInterval(0, equipMeta.equipSlotIds.length - 1)];
  }
  else {
    equipMeta.equipSlotIds.forEach(element => {
      if (element.slotId == _equipSlotId)
        choosenEquipSlot = element;
    });
  }


  generatedEquip.equipSlotId = choosenEquipSlot.slotId;
  generatedEquip.imageId = choosenEquipSlot.imageId;

  //alter budget
  budget *= choosenEquipSlot.budgetMultiplier;

  do {
    //najdu nahodny atribut
    const attributeToAdd = attributePricesMeta.attributePrices[randomIntFromInterval(0, attributePricesMeta.attributePrices.length - 1)];

    //pripocitam ho do equipu
    generatedEquip.attributes.setAttributeById(attributeToAdd.attributeId, 1);

    //snizim budget
    budget -= attributeToAdd.price;

  } while (budget > 0);

  //Nastavim jmeno
  generatedEquip.displayName = generateRandomName(generatedEquip);

  return generatedEquip;

}



exports.grantNewEquip = functions.https.onCall(async (data, context) => {


  const callerCharacterUid = data.characterUid;

  const callerCharacterDb = admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);


  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {

      const callerCharacterDoc = await t.get(callerCharacterDb);
      let callerCharacterData: CharacterDocument = callerCharacterDoc.data();


      for (let i = 0; i < 3; i++) {
        const content: ContentContainer = new ContentContainer(CONTENT_TYPE.EQUIP, undefined, generateEquip(randomIntFromInterval(1, 20), RARITY.UNCOMMON, EQUIP_SLOT_ID.ANY, CHARACTER_CLASS.ANY), undefined, undefined);
        callerCharacterData.addContentToInventory(content, true, false);
      }

      for (let i = 0; i < 3; i++) {
        const content: ContentContainer = new ContentContainer(CONTENT_TYPE.EQUIP, undefined, generateEquip(randomIntFromInterval(1, 20), RARITY.RARE, EQUIP_SLOT_ID.ANY, CHARACTER_CLASS.ANY), undefined, undefined);
        callerCharacterData.addContentToInventory(content, true, false);
      }

      for (let i = 0; i < 3; i++) {
        const content: ContentContainer = new ContentContainer(CONTENT_TYPE.EQUIP, undefined, generateEquip(randomIntFromInterval(1, 20), RARITY.EPIC, EQUIP_SLOT_ID.ANY, CHARACTER_CLASS.ANY), undefined, undefined);
        callerCharacterData.addContentToInventory(content, true, false);
      }

      t.set(callerCharacterDb, JSON.parse(JSON.stringify(callerCharacterData)), { merge: true });


    });


    console.log('Transaction success', result);
    return result;
  } catch (e) {
    console.log('Transaction failure:', e);
    throw new functions.https.HttpsError("aborted", "Error : " + e);
  }

});

exports.changeCharacterEquip = functions.https.onCall(async (data, context) => {

  // var requestData = {
  const callerCharacterUid = data.characterUid;
  const equipToEquip = data.equitToEquip; //Array of uid strings


  //const encounterResultDb = admin.firestore().collection('encountersResults').doc(encounterRewardUid);//.withConverter(encounterDocumentConverter);
  const callerCharacterDb = admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);


  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {


      //  const encounterResultDoc = await t.get(encounterResultDb);
      const callerCharacterDoc = await t.get(callerCharacterDb);

      //let encounterResultData: EncounterResult = encounterResultDoc.data();
      let callerCharacterData: CharacterDocument = callerCharacterDoc.data();

      equipToEquip.forEach(equipUid => {
        console.log("equipUid: " + equipUid);
      });
      callerCharacterData.equipEquipment(equipToEquip);

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