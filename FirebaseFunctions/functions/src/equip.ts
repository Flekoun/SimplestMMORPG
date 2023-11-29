
// [START import]
import * as functions from "firebase-functions";
import { characterDocumentConverter, CharacterDocument, CONTENT_TYPE, CHARACTER_CLASS, CURRENCY_ID, SimpleTally, generateContentContainer, ContentContainer, rollForRandomItem, randomIntFromInterval, getCurrentDateTimeInMillis, validateCallerBulletProof } from ".";
import { PROFESSION } from "./crafting";
import { firestoreAutoId } from "./general2";
import { RareEffectDefinition, Skill, AmountWithSpanAndRarity, SkillDefinition, SkillDefinitions, SkillBonusEffectDefinition, ConvertSkillDefinitionToSkill, BuffBonusEffectDefinition } from "./skills";
const admin = require('firebase-admin');
//const { getFirestore, Timestamp, FieldValue } = require('firebase-admin/firestore');
//const { FieldValue } = require('firebase-admin/firestore');
// // [END import]


export const enum FOOD_EFFECT {
  INCREASE_MAX_HP = "INCREASE_MAX_HP",
  INCREASE_MAX_MANA = "INCREASE_MAX_MANA",
  INCREASE_CRIT_CHANCE = "INCREASE_CRIT_CHANCE",
  INCREASE_MANA_REGEN = "INCREASE_MANA_REGEN",
  INCREASE_RESISTENCE = "INCREASE_RESISTENCE",
}


export const enum RARE_EFFECT {
  BAG_SIZE = "BAG_SIZE",
  MAX_HEALTH = "MAX_HEALTH",
  CRIT_CHANCE = "CRIT_CHANCE",
  HP_REGEN = "HP_REGEN",
  MAX_MANA = "MAX_MANA",
  MANA_REGEN = "MANA_REGEN",
  DEFENSE = "DEFENSE",
  DAMAGE_POWER = "DAMAGE_POWER",
  RESISTANCE = "RESISTANCE",
  HEALING_POWER = "HEALING_POWER",

  //Warrior
  SLAM_DAMAGE = "SLAM_DAMAGE",
}



export const enum ITEMS {

  //special
  SATOSHIUM = "SATOSHIUM",
  TRAINING_TOKEN = "TRAINING_TOKEN",
  REPUTATION = "REPUTATION",
  MAGIC_KEY = "MAGIC_KEY",
  TOWN_PORTAL = "TOWN_PORTAL",

  //quest items
  DEATH_MEDAILON = "DEATH_MEDAILON",
  RITUAL_DAGGER = "RITUAL_DAGGER",
  //junk
  OLD_BOOT = "OLD_BOOT",
  //food
  ROTTEN_APPLE = "ROTTEN_APPLE",

  APPLE = "APPLE", //fruit? ORGANS
  BERRY = "BERRY",  //EGGS
  MEAT = "MEAT",
  BONE = "BONE",

  RIBEYE = "RIBEYE",
  APPLE_PIE = "APPLE_PIE",   // liver 
  OMELETTE = "OMELETTE",
  BONE_BROTH = "BONE_BROTH",



  FATIGUE_MEAL = "FATIGUE_MEAL",

  //universal
  BOAR_TUSK = "BOAR_TUSK",
  SCORPID_TAIL = "SCORPID_TAIL",

  //potions
  MINOR_HEALTH_POTION = "MINOR_HEALTH_POTION",
  HEALTH_POTION = "HEALTH_POTION",
  MINOR_MANA_POTION = "MINOR_MANA_POTION",
  MANA_POTION = "MANA_POTION",
  INTELLECT_POTION = "INTELLECT_POTION",
  CURSE_REMOVE_MEAL = "CURSE_REMOVE_MEAL",
  MAJOR_HEALTH_POTION = "MAJOR_HEALTH_POTION",

  //currencies 
  GOLD = "GOLD",
  SILVER = "SILVER",


  //money sink mats
  WOOD = "WOOD",
  PEAT = "PEAT",
  COAL = "COAL",
  CHARCOAL = "CHARCOAL",
  SALT = "SALT",


  //Profession mats
  COPPER_ORE = "COPPER_ORE",
  IRON_ORE = "IRON_ORE",
  VERITE_ORE = "VERITE_ORE",
  VALORITE_ORE = "VALORITE_ORE",

  MARIGOLD = "MARIGOLD",
  KINGSLEAF = "KINGSLEAF",


  //blacksmithing
  FORGE_ALLOY_BASE = "FORGE_ALLOY_BASE",

  //mining
  COPPER_INGOT = "COPPER_INGOT",
  IRON_INGOT = "IRON_INGOT",
  VERITE_INGOT = "VERITE_INGOT",
  VALORITE_INGOT = "VALORITE_INGOT",

  //herbalism
  MARIGOLD_INFUSION = "MARIGOLD_INFUSION",

  //alchemy
  HARDENING_OIL = "HARDENING_OIL",

  //recipes
  //alchemy
  HARDENING_OIL_RECIPE = "HARDENING_OIL_RECIPE",
  MINOR_HEALTH_POTION_RECIPE = "MINOR_HEALTH_POTION_RECIPE",
  HEALTH_POTION_RECIPE = "HEALTH_POTION_RECIPE",
  MINOR_MANA_POTION_RECIPE = "MINOR_MANA_POTION_RECIPE",
  MANA_POTION_RECIPE = "MANA_POTION_RECIPE",
  INTELLECT_POTION_RECIPE = "INTELLECT_POTION_RECIPE",
  CURSE_REMOVE_MEAL_RECIPE = "CURSE_REMOVE_MEAL_RECIPE",
  MAJOR_HEALTH_POTION_RECIPE = "MAJOR_HEALTH_POTION_RECIPE",

  //cooking
  APPLE_PIE_RECIPE = "APPLE_PIE_RECIPE",
  RIBEYE_RECIPE = "RIBEYE_RECIPE",
  BONE_BROTH_RECIPE = "BONE_BROTH_RECIPE",
  OMELETTE_RECIPE = "OMELETTE_RECIPE",

  FATIGUE_MEAL_RECIPE = "FATIGUE_MEAL_RECIPE",


  //blacksmithing
  EQUIP_RARE_RECIPE = "EQUIP_RARE_RECIPE",
  EQUIP_UNCOMMON_RECIPE = "EQUIP_UNCOMMON_RECIPE",
  EQUIP_COMMON_RECIPE = "EQUIP_COMMON_RECIPE",
  RING_1_RECIPE = "RING_1_RECIPE",
  BAG_1_RECIPE = "BAG_1_RECIPE",

  //herbalism
  MARIGOLD_INFUSION_RECIPE = "MARIGOLD_INFUSION_RECIPE",

  //mining
  COPPER_INGOT_RECIPE = "COPPER_INGOT_RECIPE",
  IRON_INGOT_RECIPE = "IRON_INGOT_RECIPE",
  VERITE_INGOT_RECIPE = "VERITE_INGOT_RECIPE",
  VALORITE_INGOT_RECIPE = "VALORITE_INGOT_RECIPE",
}



export interface IHasChanceToSpawn {
  chanceToSpawn: number
}

export class DropTable {
  constructor(
    public dropCountMax: number,
    public dropCountMin: number,
    public dropTableItems: DropTableItem[],

  ) { }

}



//defakto zakladni definice itemu v metadatach, ktera rika jaky item ma byt vygenerovany , tedy stane se z nej ContentItem  ( a jeho varianty)  v intentary nekoho
//u vendoru se toto taky pouziva aby client videt co cca si kupujes, jakou to ma raritu, portret , content type je tam asi zbytecny, ale je to dalsi info pro klienta
//je to defakto simpkeTally... akorat v db uz mam vsudeozne ten "itemId" a "amount" a ne "id"/"count"
export class ItemIdWithAmount {
  constructor(
    public itemId: string,
    public amount: number,
  ) { }
}

//nemusi byt vzdy vsechny value zadane, napr contentType neni treba u obyc itemu, ale kdyz zas dropuju equip tak se divam na contentType jestli je equip a rarity pouzivam na definici jakou rarity ma mit vygenerovany item...

export class DropTableItem extends ItemIdWithAmount implements IHasChanceToSpawn {
  constructor(
    public amount: number,
    public chanceToSpawn: number,
    //public contentType: string | null,
    public rarity: string | null,  //zadava se jen u EQUIP a udava jakou rarity ma dropnouty
    public itemId: string,

  ) { super(itemId, amount) }

}

export class Content {
  constructor(

    public uid: string,
    public itemId: string,
    public rarity: string,
    public sellPrice: number,
    public currencyType: string,
    public stackSize: number,
    public amount: number,
    public customData: CustomData | undefined,
    public contentType: string,
    public expireDate: string | undefined
    // tady asi pridat: 
    // public level : number,
    // public maxLevel : number, //-1 == neni max level?
  ) { }

}

export class CustomData {
  constructor(

    public integers: number[] | undefined,
    public strings: string[] | undefined,
    public simpleTally: SimpleTally[] | undefined,

  ) { }
}

export class Equip extends Content {

  constructor(
    public uid: string,
    public itemId: string,
    public displayName: string,
    public imageId: string,
    public sellPrice: number,
    public currencyType: string,
    public contentType: string,

    public equipSlotId: EQUIP_SLOT_ID,
    public rarity: string,
    public level: number,
    public iLevel: number,

    // public attributes: EquipAttributes,

    public skill: Skill,
    public rareEffects: RareEffect[],
    public quality: number,
    public qualityMax: number,
    public qualityUpgradeMaterials: QualityUpgradeMaterials[],
    public neverEquiped: boolean,
    public skillBonusEffects: SkillBonusEffect[],
    public buffBonusEffects: BuffBonusEffect[]

  ) { super(uid, itemId, rarity, sellPrice, currencyType, 1, 1, undefined, contentType, undefined) }

}


// [qualityUpgradeMats]
export class QualityUpgradeMaterials {

  constructor(
    public materialsNeeded: ItemIdWithAmount[]
  ) { }

}

// [Skill]
export class RareEffect {

  constructor(
    public id: string,
    public amount: number
    // public rank: number,
  ) { }

}

export class SkillBonusEffect {

  constructor(
    public skillGroupId: string,
    public amount: number,
    public indexInArray: number,
    public id: string,
    public mathOperationType: string
  ) { }

}


export class BuffBonusEffect {

  constructor(
    public buffGroupId: string,
    public amount: number,
    public indexInArray: number,
    public id: string,
    public mathOperationType: string
  ) { }

}

export const enum MATH_OPERATION_TYPE {
  ADD = "ADD",
  SUBSTRACT = "SUBSTRACT"
}

export const enum RARITY {  //rarity jeste ovlivnuji jake stary a KOLIK bude na itemu , UNCOMMON 1-2 , RARE,EPIC- 2-4 
  COMMON = "COMMON",
  UNCOMMON = "UNCOMMON",
  RARE = "RARE",
  EPIC = "EPIC",
  LEGENDARY = "LEGENDARY",
  MYTHICAL = "MYTHICAL",
  ARTIFACT = "ARTIFACT",
  ANY = "ANY"
}

export const enum ATTRIBUTE_ID {
  STRENGTH = "STRENGTH",
  AGILITY = "AGILITY",
  INTELLECT = "INTELLECT",
  STAMINA = "STAMINA",
  SPIRIT = "SPIRIT",
}

//export const BAG_SLOTS_MAX = 4;
//export const RING_SLOTS_MAX = 2;

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
  BAG = "BAG",
  EARRING = "EARRING",
  CHARM = "CHARM"


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
  EQUIP_SLOT_ID.WRIST,
  EQUIP_SLOT_ID.EARRING,
  EQUIP_SLOT_ID.CHARM

];





export function shuffleArray(array) {
  // Used like so
  //var arr = [2, 11, 37, 42];
  //shuffle(arr);
  //console.log(arr);

  let currentIndex = array.length, randomIndex;

  // While there remain elements to shuffle.
  while (currentIndex != 0) {

    // Pick a remaining element.
    randomIndex = Math.floor(Math.random() * currentIndex);
    currentIndex--;

    // And swap it with the current element.
    [array[currentIndex], array[randomIndex]] = [
      array[randomIndex], array[currentIndex]];
  }

  return array;

}

//equip level - level ktery bude mit nahodne vygenerovany equip....napr u dropu z enemy chci aby mel level enemyho z kterho vypadl
//Momentalne pocet dropnutych itemu je linearni random cislo from dropCountMin to dropCountMax
export function generateDropFromDropTable(_dropTables: DropTable[], _equipLevel: number, _skillDefinitions: SkillDefinitions | null): ContentContainer[] {

  let droppedItems: ContentContainer[] = [];

  //let ItemDrops: InventoryItemSimple[] = [];
  //const LOOP_LOCK_PREVENTION_MAX_ROLLS = 100;
  // let loopCount = 0;

  let rolledItem: IHasChanceToSpawn | null = null;

  //let dropCount = 0;

  console.log("START-----Drop z droptable -----");

  //A rolluju dokud nenaplnim minimalny pocet vyzadovanych dropu, pokud je 0 , tak to zkusim jen jednou a konec nic nedroplo? Ok..
  //loopCount = 0;
  //dropCount = 0;




  // this.dropTable.forEach(dropTable => {
  _dropTables.forEach(async dropTable => {

    let dropCount = Math.floor(dropTable.dropCountMin + (dropTable.dropCountMax - dropTable.dropCountMin + 1) * (1 - Math.sqrt(1 - Math.random())));

    for (let i = 0; i < dropCount; i++) {

      rolledItem = rollForRandomItem(dropTable.dropTableItems);

      if (rolledItem != null) {

        //  dropCount++;
        console.log("rolled item je : " + (rolledItem as DropTableItem).itemId + " : id :" + (rolledItem as DropTableItem).itemId);

        // if ((rolledItem as DropTableItem).contentType != undefined) {
        if ((rolledItem as DropTableItem).itemId == CONTENT_TYPE.EQUIP) {
          if (_skillDefinitions == null)
            throw "You want to generate random equip but did not provide skill definitions. SERVER CODE ERROR!";

          const equip = generateEquip(_equipLevel, (rolledItem as DropTableItem).rarity!, EQUIP_SLOT_ID.ANY, CHARACTER_CLASS.ANY, _skillDefinitions);
          droppedItems.push(generateContentContainer(equip));
        }
        else
          droppedItems.push(generateContentContainer(generateContent((rolledItem as DropTableItem).itemId, (rolledItem as DropTableItem).amount)));
      }
      //   else
      //     droppedItems.push(generateContentContainer(generateContent((rolledItem as DropTableItem).itemId, (rolledItem as DropTableItem).amount)));
      // }


    }

    //console.log("KONEC-----loop probehlo tolikrát : " + loopCount);

  });
  console.log("droppedItems.lenght : " + droppedItems.length)
  return droppedItems;

}

//druhy zpusob jak toto delat je tady nacitat data z databaze....to je 1 read navic pokazde kdyz vytvarim food(nakup od vendora, loot z monster droptable)..takze asi ne?...ikdzy nejsou to moc caste eventy 
export function generateContent(_itemId: string, _amount: number): Content {
  //Tady tohle "generovani simple itemu jen nahrazuje 1 read z metadat kde by byly vsechny tyhle itemy? stoji to za to? neni to zadna logika pridana....."
  //let displayName: string = "";

  let customData: CustomData | undefined = undefined;

  let expireDate: string | undefined = undefined;
  let sellPrice: number = 0;
  let currencyType: string = CURRENCY_ID.GOLD;
  let stackSize: number = 1;
  // let imageId: string = "NONE";
  let rarity: RARITY = RARITY.COMMON;
  // let fatigueRecoveryBonus = 1;
  // let timeBonus = 1;
  let contentType = CONTENT_TYPE.ITEM;

  //SPECIAL
  if (_itemId == ITEMS.SATOSHIUM) { sellPrice = 0; stackSize = 10000000, rarity = RARITY.ARTIFACT, contentType = CONTENT_TYPE.ITEM }
  if (_itemId == ITEMS.TRAINING_TOKEN) { sellPrice = 0; stackSize = 10000000, rarity = RARITY.ARTIFACT, contentType = CONTENT_TYPE.ITEM }
  if (_itemId == ITEMS.REPUTATION) { sellPrice = 0; stackSize = 10000000, rarity = RARITY.ARTIFACT, contentType = CONTENT_TYPE.ITEM }
  if (_itemId == ITEMS.MAGIC_KEY) { sellPrice = 10000; stackSize = 1, rarity = RARITY.EPIC, contentType = CONTENT_TYPE.ITEM }
  if (_itemId == ITEMS.TOWN_PORTAL) { sellPrice = 50; stackSize = 5, rarity = RARITY.RARE, customData = new CustomData(undefined, undefined, undefined), contentType = CONTENT_TYPE.FOOD }

  //FOOD
  else if (_itemId == ITEMS.ROTTEN_APPLE) { sellPrice = 2; stackSize = 1, customData = new CustomData([1, 20], undefined, undefined), contentType = CONTENT_TYPE.FOOD_SUPPLY, expireDate = getCurrentDateTimeInMillis(24).toString() }

  else if (_itemId == ITEMS.APPLE) { sellPrice = 5; stackSize = 20, customData = new CustomData([1, 1], undefined, undefined), contentType = CONTENT_TYPE.FOOD_SUPPLY }
  else if (_itemId == ITEMS.BERRY) { sellPrice = 10; stackSize = 20, customData = new CustomData([1, 1], undefined, undefined), contentType = CONTENT_TYPE.FOOD_SUPPLY }//fatigueRecoveryBonus = 0, timeBonus = 3 }
  else if (_itemId == ITEMS.MEAT) { sellPrice = 10; stackSize = 20, customData = new CustomData([1, 30], undefined, undefined), contentType = CONTENT_TYPE.FOOD_SUPPLY }//fatigueRecoveryBonus = 0, timeBonus = 3 }
  else if (_itemId == ITEMS.BONE) { sellPrice = 10; stackSize = 20, customData = new CustomData([1, 6], undefined, undefined), contentType = CONTENT_TYPE.FOOD_SUPPLY }//fatigueRecoveryBonus = 0, timeBonus = 3 }


  else if (_itemId == ITEMS.APPLE_PIE) { sellPrice = 100; stackSize = 20, customData = new CustomData([1, 2], undefined, [new SimpleTally(FOOD_EFFECT.INCREASE_CRIT_CHANCE, 5)]), contentType = CONTENT_TYPE.FOOD_SUPPLY } //fatigueRecoveryBonus = 80, timeBonus = 0 }
  else if (_itemId == ITEMS.RIBEYE) { sellPrice = 100; stackSize = 20, customData = new CustomData([1, 40], undefined, [new SimpleTally(FOOD_EFFECT.INCREASE_MAX_HP, 20)]), contentType = CONTENT_TYPE.FOOD_SUPPLY } //fatigueRecoveryBonus = 80, timeBonus = 0 }
  else if (_itemId == ITEMS.OMELETTE) { sellPrice = 100; stackSize = 20, customData = new CustomData([1, 2], undefined, [new SimpleTally(FOOD_EFFECT.INCREASE_RESISTENCE, 3)]), contentType = CONTENT_TYPE.FOOD_SUPPLY } //fatigueRecoveryBonus = 80, timeBonus = 0 }
  else if (_itemId == ITEMS.BONE_BROTH) { sellPrice = 100; stackSize = 20, customData = new CustomData([1, 8], undefined, [new SimpleTally(FOOD_EFFECT.INCREASE_MAX_MANA, 1)]), contentType = CONTENT_TYPE.FOOD_SUPPLY } //fatigueRecoveryBonus = 80, timeBonus = 0 }

  else if (_itemId == ITEMS.FATIGUE_MEAL) { sellPrice = 200; stackSize = 20, customData = new CustomData([2], undefined, [new SimpleTally(FOOD_EFFECT.INCREASE_MAX_MANA, 1), new SimpleTally(FOOD_EFFECT.INCREASE_MANA_REGEN, 1), new SimpleTally(FOOD_EFFECT.INCREASE_MAX_HP, 30), new SimpleTally(FOOD_EFFECT.INCREASE_RESISTENCE, 4), new SimpleTally(FOOD_EFFECT.INCREASE_CRIT_CHANCE, 8)],), contentType = CONTENT_TYPE.FOOD_SUPPLY }
  else if (_itemId == ITEMS.CURSE_REMOVE_MEAL) { sellPrice = 500; stackSize = 20, customData = new CustomData([3, 1], undefined, undefined), contentType = CONTENT_TYPE.FOOD_SUPPLY }


  //else if (_itemId == ITEMS.BERRIES_DRIED) { sellPrice = 10; stackSize = 20, customData = new CustomData([1, 4], undefined, undefined), contentType = CONTENT_TYPE.FOOD_SUPPLY }//fatigueRecoveryBonus = 0, timeBonus = 3 }

  //SPECIAL
  //else if (_itemId == ITEMS.BEEF) { sellPrice = 100; stackSize = 1, customData = new CustomData([1, 10], undefined, [new SimpleTally(FOOD_EFFECT.INCREASE_MAX_HP, 10)]), contentType = CONTENT_TYPE.FOOD_SUPPLY } //fatigueRecoveryBonus = 80, timeBonus = 0 }
  //else if (_itemId == ITEMS.SALTED_RIBEYE) { sellPrice = 100; stackSize = 1, customData = new CustomData([1, 10], undefined, [new SimpleTally(FOOD_EFFECT.INCREASE_MAX_HP, 10)]), contentType = CONTENT_TYPE.FOOD_SUPPLY } //fatigueRecoveryBonus = 80, timeBonus = 0 }

  //fatigue
  //other



  //ITEMS
  else if (_itemId == ITEMS.BOAR_TUSK) { sellPrice = 5; stackSize = 20, contentType = CONTENT_TYPE.ITEM }
  else if (_itemId == ITEMS.OLD_BOOT) { sellPrice = 10; stackSize = 1, contentType = CONTENT_TYPE.ITEM }
  else if (_itemId == ITEMS.DEATH_MEDAILON) { sellPrice = 50; stackSize = 1, contentType = CONTENT_TYPE.ITEM }
  else if (_itemId == ITEMS.MARIGOLD) { sellPrice = 5; stackSize = 20, contentType = CONTENT_TYPE.ITEM }
  else if (_itemId == ITEMS.KINGSLEAF) { sellPrice = 5; stackSize = 20, contentType = CONTENT_TYPE.ITEM }
  else if (_itemId == ITEMS.SCORPID_TAIL) { sellPrice = 10; stackSize = 20, contentType = CONTENT_TYPE.ITEM }
  else if (_itemId == ITEMS.RITUAL_DAGGER) { sellPrice = 50; stackSize = 1, contentType = CONTENT_TYPE.ITEM }

  else if (_itemId == ITEMS.COPPER_ORE) { sellPrice = 5; stackSize = 20, contentType = CONTENT_TYPE.ITEM }
  else if (_itemId == ITEMS.IRON_ORE) { sellPrice = 10; stackSize = 20, contentType = CONTENT_TYPE.ITEM }
  else if (_itemId == ITEMS.VERITE_ORE) { sellPrice = 20; stackSize = 20, contentType = CONTENT_TYPE.ITEM }
  else if (_itemId == ITEMS.VALORITE_ORE) { sellPrice = 40; stackSize = 20, contentType = CONTENT_TYPE.ITEM }

  //CURRENCY
  else if (_itemId == CURRENCY_ID.GOLD) { sellPrice = 1; stackSize = 1000000, contentType = CONTENT_TYPE.CURRENCY }
  else if (_itemId == CURRENCY_ID.SILVER) { sellPrice = 1; stackSize = 1000000, contentType = CONTENT_TYPE.CURRENCY }



  //money sink mats
  else if (_itemId == ITEMS.WOOD) { sellPrice = 40; stackSize = 20, contentType = CONTENT_TYPE.ITEM }
  else if (_itemId == ITEMS.PEAT) { sellPrice = 80; stackSize = 20, contentType = CONTENT_TYPE.ITEM }
  else if (_itemId == ITEMS.COAL) { sellPrice = 160; stackSize = 20, contentType = CONTENT_TYPE.ITEM }
  else if (_itemId == ITEMS.CHARCOAL) { sellPrice = 320; stackSize = 20, contentType = CONTENT_TYPE.ITEM }
  else if (_itemId == ITEMS.SALT) { sellPrice = 100; stackSize = 20, contentType = CONTENT_TYPE.ITEM }


  //cooking


  //blacksmithing
  else if (_itemId == ITEMS.FORGE_ALLOY_BASE) { sellPrice = 20; stackSize = 20, contentType = CONTENT_TYPE.ITEM }
  //mining
  else if (_itemId == ITEMS.COPPER_INGOT) { sellPrice = 10; stackSize = 20, rarity = RARITY.UNCOMMON, contentType = CONTENT_TYPE.ITEM }
  else if (_itemId == ITEMS.IRON_INGOT) { sellPrice = 20; stackSize = 20, rarity = RARITY.RARE, contentType = CONTENT_TYPE.ITEM }
  else if (_itemId == ITEMS.VERITE_INGOT) { sellPrice = 40; stackSize = 20, rarity = RARITY.EPIC, contentType = CONTENT_TYPE.ITEM }
  else if (_itemId == ITEMS.VALORITE_INGOT) { sellPrice = 80; stackSize = 20, rarity = RARITY.LEGENDARY, contentType = CONTENT_TYPE.ITEM }

  //alchemy
  else if (_itemId == ITEMS.HARDENING_OIL) { sellPrice = 20; stackSize = 20, contentType = CONTENT_TYPE.ITEM }
  else if (_itemId == ITEMS.MINOR_HEALTH_POTION) { sellPrice = 15; stackSize = 5, customData = new CustomData([25, 0], undefined, undefined), contentType = CONTENT_TYPE.FOOD }
  else if (_itemId == ITEMS.HEALTH_POTION) { sellPrice = 40; stackSize = 5, customData = new CustomData([75, 0], undefined, undefined), contentType = CONTENT_TYPE.FOOD }
  else if (_itemId == ITEMS.MAJOR_HEALTH_POTION) { sellPrice = 160; stackSize = 5, customData = new CustomData([225, 0], undefined, undefined), contentType = CONTENT_TYPE.FOOD }
  else if (_itemId == ITEMS.MINOR_MANA_POTION) { sellPrice = 20; stackSize = 5, customData = new CustomData([2, 0], undefined, undefined), contentType = CONTENT_TYPE.FOOD }
  else if (_itemId == ITEMS.MANA_POTION) { sellPrice = 50; stackSize = 5, customData = new CustomData([4, 0], undefined, undefined), contentType = CONTENT_TYPE.FOOD }
  else if (_itemId == ITEMS.INTELLECT_POTION) { sellPrice = 60; stackSize = 5, customData = new CustomData([1, 0], undefined, undefined), contentType = CONTENT_TYPE.FOOD }

  //herbalism
  else if (_itemId == ITEMS.MARIGOLD_INFUSION) { sellPrice = 10; stackSize = 20, contentType = CONTENT_TYPE.ITEM }

  //RECIPES
  //alchemy
  else if (_itemId == ITEMS.MINOR_HEALTH_POTION_RECIPE) { sellPrice = 50; stackSize = 1, contentType = CONTENT_TYPE.RECIPE, rarity = RARITY.UNCOMMON, customData = new CustomData([0, 1], [ITEMS.MINOR_HEALTH_POTION_RECIPE], [new SimpleTally(PROFESSION.ALCHEMY, 0)]) }
  else if (_itemId == ITEMS.HEALTH_POTION_RECIPE) { sellPrice = 100; stackSize = 1, contentType = CONTENT_TYPE.RECIPE, rarity = RARITY.UNCOMMON, customData = new CustomData([0, 3], [ITEMS.HEALTH_POTION_RECIPE], [new SimpleTally(PROFESSION.ALCHEMY, 10)]) }
  else if (_itemId == ITEMS.MINOR_MANA_POTION_RECIPE) { sellPrice = 50; stackSize = 1, contentType = CONTENT_TYPE.RECIPE, rarity = RARITY.UNCOMMON, customData = new CustomData([0, 1], [ITEMS.MINOR_MANA_POTION_RECIPE], [new SimpleTally(PROFESSION.ALCHEMY, 5)]) }
  else if (_itemId == ITEMS.MANA_POTION_RECIPE) { sellPrice = 125; stackSize = 1, contentType = CONTENT_TYPE.RECIPE, rarity = RARITY.UNCOMMON, customData = new CustomData([0, 3], [ITEMS.MINOR_MANA_POTION_RECIPE], [new SimpleTally(PROFESSION.ALCHEMY, 15)]) }
  else if (_itemId == ITEMS.INTELLECT_POTION_RECIPE) { sellPrice = 150; stackSize = 1, contentType = CONTENT_TYPE.RECIPE, rarity = RARITY.UNCOMMON, customData = new CustomData([0, 5], [ITEMS.INTELLECT_POTION_RECIPE], [new SimpleTally(PROFESSION.ALCHEMY, 20)]) }
  else if (_itemId == ITEMS.MAJOR_HEALTH_POTION_RECIPE) { sellPrice = 1000; stackSize = 1, contentType = CONTENT_TYPE.RECIPE, rarity = RARITY.RARE, customData = new CustomData([0, 5], [ITEMS.HEALTH_POTION_RECIPE], [new SimpleTally(PROFESSION.ALCHEMY, 10)]) }


  //cooking
  else if (_itemId == ITEMS.APPLE_PIE_RECIPE) { sellPrice = 50; stackSize = 1, contentType = CONTENT_TYPE.RECIPE, rarity = RARITY.UNCOMMON, customData = new CustomData([0, 1], [ITEMS.APPLE_PIE_RECIPE], [new SimpleTally(PROFESSION.COOKING, 0)]) }
  else if (_itemId == ITEMS.OMELETTE_RECIPE) { sellPrice = 50; stackSize = 1, contentType = CONTENT_TYPE.RECIPE, rarity = RARITY.UNCOMMON, customData = new CustomData([0, 1], [ITEMS.OMELETTE_RECIPE], [new SimpleTally(PROFESSION.COOKING, 0)]) }
  else if (_itemId == ITEMS.BONE_BROTH_RECIPE) { sellPrice = 50; stackSize = 1, contentType = CONTENT_TYPE.RECIPE, rarity = RARITY.UNCOMMON, customData = new CustomData([0, 1], [ITEMS.BONE_BROTH_RECIPE], [new SimpleTally(PROFESSION.COOKING, 0)]) }
  else if (_itemId == ITEMS.RIBEYE_RECIPE) { sellPrice = 50; stackSize = 1, contentType = CONTENT_TYPE.RECIPE, rarity = RARITY.UNCOMMON, customData = new CustomData([0, 1], [ITEMS.RIBEYE_RECIPE], [new SimpleTally(PROFESSION.COOKING, 0)]) }

  else if (_itemId == ITEMS.FATIGUE_MEAL_RECIPE) { sellPrice = 100; stackSize = 1, contentType = CONTENT_TYPE.RECIPE, rarity = RARITY.RARE, customData = new CustomData([0, 5], [ITEMS.FATIGUE_MEAL_RECIPE], [new SimpleTally(PROFESSION.COOKING, 0)]) }
  else if (_itemId == ITEMS.CURSE_REMOVE_MEAL_RECIPE) { sellPrice = 200; stackSize = 1, contentType = CONTENT_TYPE.RECIPE, rarity = RARITY.RARE, customData = new CustomData([0, 8], [ITEMS.CURSE_REMOVE_MEAL_RECIPE], [new SimpleTally(PROFESSION.ALCHEMY, 20)]) }


  //blacksmithing
  else if (_itemId == ITEMS.RING_1_RECIPE) { sellPrice = 150; stackSize = 1, contentType = CONTENT_TYPE.RECIPE, rarity = RARITY.UNCOMMON, customData = new CustomData([0, 1], [ITEMS.RING_1_RECIPE], [new SimpleTally(PROFESSION.BLACKSMITHING, 5)]) }
  else if (_itemId == ITEMS.BAG_1_RECIPE) { sellPrice = 250; stackSize = 1, contentType = CONTENT_TYPE.RECIPE, rarity = RARITY.UNCOMMON, customData = new CustomData([0, 1], [ITEMS.BAG_1_RECIPE], [new SimpleTally(PROFESSION.BLACKSMITHING, 10)]) }
  else if (_itemId == ITEMS.EQUIP_COMMON_RECIPE) { sellPrice = 250; stackSize = 1, contentType = CONTENT_TYPE.RECIPE, rarity = RARITY.COMMON, customData = new CustomData([0, 1], [ITEMS.EQUIP_COMMON_RECIPE], undefined) }
  else if (_itemId == ITEMS.EQUIP_UNCOMMON_RECIPE) { sellPrice = 500; stackSize = 1, contentType = CONTENT_TYPE.RECIPE, rarity = RARITY.UNCOMMON, customData = new CustomData([0, 1], [ITEMS.EQUIP_UNCOMMON_RECIPE], undefined) }
  else if (_itemId == ITEMS.EQUIP_RARE_RECIPE) { sellPrice = 1000; stackSize = 1, contentType = CONTENT_TYPE.RECIPE, rarity = RARITY.RARE, customData = new CustomData([0, 1], [ITEMS.EQUIP_RARE_RECIPE], undefined) }


  else if (_itemId == ITEMS.COPPER_INGOT_RECIPE) { sellPrice = 150; stackSize = 1, contentType = CONTENT_TYPE.RECIPE, rarity = RARITY.UNCOMMON, customData = new CustomData([0, 1], [ITEMS.COPPER_INGOT_RECIPE], [new SimpleTally(PROFESSION.BLACKSMITHING, 0)]) }
  else if (_itemId == ITEMS.IRON_INGOT_RECIPE) { sellPrice = 300; stackSize = 1, contentType = CONTENT_TYPE.RECIPE, rarity = RARITY.UNCOMMON, customData = new CustomData([0, 3], [ITEMS.IRON_INGOT_RECIPE], [new SimpleTally(PROFESSION.BLACKSMITHING, 0)]) }
  else if (_itemId == ITEMS.VERITE_INGOT_RECIPE) { sellPrice = 600; stackSize = 1, contentType = CONTENT_TYPE.RECIPE, rarity = RARITY.UNCOMMON, customData = new CustomData([0, 5], [ITEMS.VERITE_INGOT_RECIPE], [new SimpleTally(PROFESSION.BLACKSMITHING, 0)]) }
  else if (_itemId == ITEMS.VALORITE_INGOT_RECIPE) { sellPrice = 1200; stackSize = 1, contentType = CONTENT_TYPE.RECIPE, rarity = RARITY.UNCOMMON, customData = new CustomData([0, 8], [ITEMS.VALORITE_INGOT_RECIPE], [new SimpleTally(PROFESSION.BLACKSMITHING, 0)]) }

  return new Content(firestoreAutoId(), _itemId, rarity, sellPrice, currencyType, stackSize, _amount, customData, contentType, expireDate);

}

function generateRandomName(_equip: Equip): string {
  const prefix: string[] = ["Godly", "Great", "Sturdy", "Rusty", "Mighty", "Iron", "Rusty", "Glorious", "Divine", "Majestic", "Robust", "Solid", "Supreme"]
  const name: string[] = ["Item", "Thing", "Equipment", "Object", "Stuff", "Construct"]
  const suffix: string[] = ["of the Wild", "of Silent", "of Hero", "of King", "of Beggar", "of Lord", "of Peasant"]

  return prefix[randomIntFromInterval(0, prefix.length - 1)] + " " + name[randomIntFromInterval(0, name.length - 1)] + " " + suffix[randomIntFromInterval(0, suffix.length - 1)]

}


export async function QuerryForSkillDefinitions(_transaction: any): Promise<SkillDefinitions> {

  const skillDefinitionsDb = admin.firestore().collection("_metadata_skills").doc("skillDefinitions");
  const skillDefinitionsDoc = await _transaction.get(skillDefinitionsDb);
  let skillDefinitionsData: SkillDefinitions = skillDefinitionsDoc.data();
  return skillDefinitionsData;
}

export function generateEquip(_mLevel: number, _rarity: string, _equipSlotId: string, _characterClass: string, _skillDefinitions: SkillDefinitions, _skillId?: string, _equipName?: string, _neverEquiped: boolean = true): Equip {

  console.log("rarity: " + _rarity + " _equipSlotId: " + _equipSlotId + " _characterClass : " + _characterClass);



  const equipMeta =
  {
    "equipSlotIds":
      [
        {
          "slotId": EQUIP_SLOT_ID.HEAD,
          "budgetMultiplier": 1,
          "imageId": "HEAD_1",
        },
        {
          "slotId": EQUIP_SLOT_ID.BODY,
          "budgetMultiplier": 1,
          "imageId": "BODY_1",
        },
        {
          "slotId": EQUIP_SLOT_ID.LEGS,
          "budgetMultiplier": 1,
          "imageId": "LEGS_1",
        },
        {
          "slotId": EQUIP_SLOT_ID.FEET,
          "budgetMultiplier": 0.77,
          "imageId": "FEET_1",
        },
        {
          "slotId": EQUIP_SLOT_ID.HANDS,
          "budgetMultiplier": 0.77,
          "imageId": "HANDS_1",
        },
        {
          "slotId": EQUIP_SLOT_ID.AMULET,
          "budgetMultiplier": 0.7,
          "imageId": "AMULET_1",
        },
        {
          "slotId": EQUIP_SLOT_ID.FINGER_1,
          "budgetMultiplier": 0.55,
          "imageId": "FINGER_1",
        },
        {
          "slotId": EQUIP_SLOT_ID.TRINKET,
          "budgetMultiplier": 0.7,
          "imageId": "TRINKET_1",
        },
        {
          "slotId": EQUIP_SLOT_ID.SHOULDER,
          "budgetMultiplier": 0.77,
          "imageId": "SHOULDER_1",
        },
        {
          "slotId": EQUIP_SLOT_ID.BACK,
          "budgetMultiplier": 0.55,
          "imageId": "BACK_1",
        },
        {
          "slotId": EQUIP_SLOT_ID.NECK,
          "budgetMultiplier": 0.55,
          "imageId": "NECK_1",
        },
        {
          "slotId": EQUIP_SLOT_ID.WRIST,
          "budgetMultiplier": 0.55,
          "imageId": "WRIST_1",
        },
        {
          "slotId": EQUIP_SLOT_ID.MAIN_HAND,
          "budgetMultiplier": 1,
          "imageId": "MAIN_HAND_1",
        },
        {
          "slotId": EQUIP_SLOT_ID.OFF_HAND,
          "budgetMultiplier": 0.55,
          "imageId": "OFF_HAND_1",
        },
        {
          "slotId": EQUIP_SLOT_ID.WAIST,
          "budgetMultiplier": 0.77,
          "imageId": "WAIST_1"
        },
        {
          "slotId": EQUIP_SLOT_ID.BAG,
          "budgetMultiplier": 0.55,
          "imageId": "BAG_1"
        },
        {
          "slotId": EQUIP_SLOT_ID.EARRING,
          "budgetMultiplier": 0.55,
          "imageId": "EARRING_1"
        },
        {
          "slotId": EQUIP_SLOT_ID.CHARM,
          "budgetMultiplier": 0.55,
          "imageId": "CHARM_1"
        }
      ],

  }


  let dummySkill: Skill = new Skill("", "", true, "", 0, [], undefined, true, true, true, "");
  let generatedEquip: Equip = new Equip(firestoreAutoId(), "EQUIP", "Item of Malakus", "NONE", 0, CURRENCY_ID.GOLD, CONTENT_TYPE.EQUIP, EQUIP_SLOT_ID.BODY, RARITY.COMMON, 0, 0, dummySkill, [], 0, 1, [], true, [], []);
  //Lowest iLevel = 6
  // Nejnizsi buget je Common level 1 ,0.55 muliplier equip slot, = 0.73333
  //Nejvyssi budget je Mythical level 60 , 1 multiplier slot = 61.7925 ( 25.04 pro mythical level 25)

  const iLevel = _mLevel + 5;
  let budget: number = 0;
  // if (_rarity == RARITY.COMMON) {
  //   budget = 0.3333 * (iLevel - 2);
  // }
  // else if (_rarity == RARITY.UNCOMMON) {
  //   budget = 0.5 * (iLevel - 2);
  // }
  // else if (_rarity == RARITY.RARE) {
  //   budget = 0.625 * (iLevel - 1.15);
  // }
  // else if (_rarity == RARITY.EPIC) {
  //   budget = 0.75 * (iLevel - 1.15);
  // }
  // else if (_rarity == RARITY.LEGENDARY) {
  //   budget = 0.9 * (iLevel - 1.15);
  // }
  // else if (_rarity == RARITY.MYTHICAL) {
  //   budget = 1.05 * (iLevel - 1.15);
  // }
  // else {
  //   budget = 1.2 * (iLevel - 1);
  // }
  generatedEquip.neverEquiped = _neverEquiped;
  generatedEquip.rarity = _rarity;
  generatedEquip.iLevel = iLevel;
  generatedEquip.level = _mLevel;
  generatedEquip.sellPrice = 30 * Math.exp((Math.log(6000) / 19) * (budget - 1));  //10 bronzu - 6 zlatych by to melo davat

  //Funkci mam videt v Desmosu
  //generatedEquip.qualityMax = Math.floor(1 + (5) * (1 - Math.sqrt(1 - Math.random())));
  generatedEquip.qualityMax = 1;
  // switch (_rarity) {
  //   case RARITY.COMMON: generatedEquip.qualityMax = 1; break;
  //   case RARITY.UNCOMMON: generatedEquip.qualityMax = 2; break;
  //   case RARITY.RARE: generatedEquip.qualityMax = 3; break;
  //   case RARITY.EPIC: generatedEquip.qualityMax = 4; break;
  //   case RARITY.LEGENDARY: generatedEquip.qualityMax = 5; break;
  //   default:
  //     break;
  // }


  //Nastavime QualityUpgradeMats
  let craftingMats: ItemIdWithAmount[] = [];



  if (generatedEquip.rarity == RARITY.COMMON) {
    craftingMats.push(new ItemIdWithAmount(ITEMS.WOOD, 1));
    craftingMats.push(new ItemIdWithAmount(ITEMS.COPPER_INGOT, 2));
  }
  else if (generatedEquip.rarity == RARITY.UNCOMMON) {
    craftingMats.push(new ItemIdWithAmount(ITEMS.PEAT, 1));
    craftingMats.push(new ItemIdWithAmount(ITEMS.COPPER_INGOT, 3));
    craftingMats.push(new ItemIdWithAmount(ITEMS.IRON_INGOT, 2));
  }
  else if (generatedEquip.rarity == RARITY.RARE) {
    craftingMats.push(new ItemIdWithAmount(ITEMS.COAL, 1));
    craftingMats.push(new ItemIdWithAmount(ITEMS.COPPER_INGOT, 4));
    craftingMats.push(new ItemIdWithAmount(ITEMS.IRON_INGOT, 3));
    craftingMats.push(new ItemIdWithAmount(ITEMS.VERITE_INGOT, 1));
  }
  else if (generatedEquip.rarity == RARITY.EPIC) {
    craftingMats.push(new ItemIdWithAmount(ITEMS.CHARCOAL, 1));
    craftingMats.push(new ItemIdWithAmount(ITEMS.IRON_INGOT, 4));
    craftingMats.push(new ItemIdWithAmount(ITEMS.VERITE_INGOT, 2));
    craftingMats.push(new ItemIdWithAmount(ITEMS.VALORITE_INGOT, 1));
  }
  else if (generatedEquip.rarity == RARITY.LEGENDARY) {
    craftingMats.push(new ItemIdWithAmount(ITEMS.CHARCOAL, 1));
    craftingMats.push(new ItemIdWithAmount(ITEMS.IRON_INGOT, 5));
    craftingMats.push(new ItemIdWithAmount(ITEMS.VERITE_INGOT, 3));
    craftingMats.push(new ItemIdWithAmount(ITEMS.VALORITE_INGOT, 2));
  }
  else {
    craftingMats.push(new ItemIdWithAmount(ITEMS.CHARCOAL, 1));
    craftingMats.push(new ItemIdWithAmount(ITEMS.IRON_INGOT, 1));
    craftingMats.push(new ItemIdWithAmount(ITEMS.VERITE_INGOT, 1));
    craftingMats.push(new ItemIdWithAmount(ITEMS.VALORITE_INGOT, 1));
  }

  for (let quality = 0; quality < generatedEquip.qualityMax; quality++)
    generatedEquip.qualityUpgradeMaterials.push(new QualityUpgradeMaterials(craftingMats));

  // let goldSinkMat = ITEMS.WOOD;
  // let craftingSMat = ITEMS.COPPER_INGOT;
  // let matMultiplier = 1;


  // if (generatedEquip.rarity == RARITY.COMMON) {
  //   goldSinkMat = ITEMS.WOOD;
  //   craftingSMat = ITEMS.COPPER_INGOT;
  //   matMultiplier = 1;
  // }
  // else if (generatedEquip.rarity == RARITY.UNCOMMON) {
  //   goldSinkMat = ITEMS.PEAT;
  //   craftingSMat = ITEMS.IRON_INGOT;
  //   matMultiplier = 2;
  // }
  // else if (generatedEquip.rarity == RARITY.RARE) {
  //   goldSinkMat = ITEMS.COAL;
  //   craftingSMat = ITEMS.VERITE_INGOT;
  //   matMultiplier = 4;
  // }
  // else if (generatedEquip.rarity == RARITY.EPIC) {
  //   goldSinkMat = ITEMS.CHARCOAL;
  //   craftingSMat = ITEMS.VALORITE_INGOT;
  //   matMultiplier = 8;
  // }
  // else if (generatedEquip.rarity == RARITY.LEGENDARY) {
  //   goldSinkMat = ITEMS.CHARCOAL;
  //   craftingSMat = ITEMS.VALORITE_INGOT;
  //   matMultiplier = 16;
  // }
  // else {
  //   goldSinkMat = ITEMS.CHARCOAL;
  //   craftingSMat = ITEMS.VALORITE_INGOT;
  //   matMultiplier = 32;
  // }

  // matMultiplier = 1;
  // for (let quality = 0; quality < generatedEquip.qualityMax; quality++) {

  //   //   switch (quality) {
  //   //     case 0:
  //   //       goldSinkMat = ITEMS.WOOD;
  //   //       craftingSMat = ITEMS.COPPER_INGOT; //na level 0 se nepouziva
  //   //       break;
  //   //     case 1:
  //   //       goldSinkMat = ITEMS.WOOD;
  //   //       craftingSMat = ITEMS.COPPER_INGOT;

  //   //       break;
  //   //     case 2:
  //   //       goldSinkMat = ITEMS.PEAT;
  //   //       craftingSMat = ITEMS.IRON_INGOT;

  //   //       break;
  //   //     case 3:
  //   //       goldSinkMat = ITEMS.COAL;
  //   //       craftingSMat = ITEMS.VERITE_INGOT;
  //   //       break;

  //   //     default:
  //   //       goldSinkMat = ITEMS.CHARCOAL;
  //   //       craftingSMat = ITEMS.VALORITE_INGOT;
  //   //       break;
  //   //   }


  //   let upgradeMatList: ItemIdWithAmount[] = [];

  //   upgradeMatList.push(new ItemIdWithAmount(goldSinkMat, (matMultiplier)));
  //   // if (quality > 0)
  //   upgradeMatList.push(new ItemIdWithAmount(craftingSMat, matMultiplier));

  //   // upgradeMatList.push(new ItemIdWithAmount(ITEMS.COPPER_INGOT, quality + 1));
  //   generatedEquip.qualityUpgradeMaterials.push(new QualityUpgradeMaterials(upgradeMatList));

  // }

  // const amount_vendorMat = Math.floor(Math.exp(iLevel / 5 - 1 + quality / 4));
  // const amount_professionMat = Math.floor(Math.exp(iLevel / 6 - 2 + quality / 4));

  // let upgradeMatList: ItemIdWithAmount[] = [];
  // if (amount_vendorMat > 0)
  //   upgradeMatList.push(new ItemIdWithAmount(ITEMS.COAL, amount_vendorMat));
  // if (amount_professionMat > 0) {
  //   if (Math.random() > 0.5)
  //     upgradeMatList.push(new ItemIdWithAmount(ITEMS.FORGE_ALLOY_BASE, amount_professionMat));
  //   else
  //     upgradeMatList.push(new ItemIdWithAmount(ITEMS.HARDENING_OIL, amount_professionMat));
  // }

  // if (upgradeMatList.length == 0)
  //   throw ("Error! No quality upgrade materials found for equip iLevel :" + iLevel);

  // let qualityUpgradeMaterialsRank1: QualityUpgradeMaterials = new QualityUpgradeMaterials(upgradeMatList);
  // generatedEquip.qualityUpgradeMaterials.push(qualityUpgradeMaterialsRank1);

  //}

  // console.log("_skillDefinitions.length :" + _skillDefinitions.items.length);
  // console.log("_skillDefinitions class :" + _skillDefinitions.items[0].characterClass);
  // console.log("_skillDefinitions class :" + _skillDefinitions.items[1].characterClass);

  //pokud sme nepedali jaky skill cheme...vygeneruju nahodny 
  if (_skillId == undefined) {
    //vyfiltruju skilly aby tam byly jen dane classy a rarity
    let filteredSkills: SkillDefinition[] = [];
    if (_characterClass == CHARACTER_CLASS.ANY)
      filteredSkills = _skillDefinitions.skills.filter(item => item.rarity == _rarity);//filteredSkills = _skillDefinitions.skills.filter(item => item.rarity == RARITY.COMMON);
    else
      filteredSkills = _skillDefinitions.skills.filter(item => item.characterClass == _characterClass && item.rarity == _rarity);//filteredSkills = _skillDefinitions.skills.filter(item => item.characterClass == _characterClass && item.rarity == RARITY.COMMON);//

    // console.log("filteredSkills.length :" + filteredSkills.length);

    //pridam skill
    let choosenSkill = rollForRandomItem(filteredSkills) as SkillDefinition;
    generatedEquip.skill = ConvertSkillDefinitionToSkill(choosenSkill, _skillDefinitions);//new Skill(choosenSkill.skillId, choosenSkill.skillGroupId, choosenSkill.singleUse, choosenSkill.characterClass, choosenSkill.manaCost, choosenSkill.quality, getBuffById(choosenSkill.buffId, _skillDefinitions), choosenSkill.validTarget_AnyAlly, choosenSkill.validTarget_Self, choosenSkill.validTarget_AnyEnemy, choosenSkill.rarity);

  }
  else { //rucne vyberem specificky skill ktery je vyzadovan
    let choosenSkill = _skillDefinitions.skills.find(skill => skill.skillId == _skillId);
    if (choosenSkill)
      generatedEquip.skill = ConvertSkillDefinitionToSkill(choosenSkill, _skillDefinitions);
    else
      throw ("could not find skill with id : " + _skillId);
  }
  //Nastavim jmeno
  if (_equipName == undefined)
    generatedEquip.displayName = generateRandomName(generatedEquip);
  else
    generatedEquip.displayName = _equipName;



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

  //alter budget by equip slot type
  budget *= choosenEquipSlot.budgetMultiplier;
  console.log("budet itemu je : " + budget + " jmeno itemu : " + generatedEquip.displayName);

  //pocty rare a skill efektu pro ruzne rarity
  let skillBonusEffectsCount = 1;
  let rareEffectsCount = 0;

  // let coinFlip = Math.random() <= 0.5;

  // if (_rarity == RARITY.COMMON) {
  //   skillBonusEffectsCount = 0
  //   rareEffectsCount = 0;
  // }
  // else if (_rarity == RARITY.UNCOMMON) {
  //   if (generatedEquip.equipSlotId == EQUIP_SLOT_ID.BAG) {
  //     skillBonusEffectsCount = 0;
  //     rareEffectsCount = 1;
  //   }
  //   else {

  //     skillBonusEffectsCount = coinFlip ? 1 : 0;
  //     rareEffectsCount = coinFlip ? 0 : 1;
  //   }
  // }
  // else if (_rarity == RARITY.RARE) {
  //   skillBonusEffectsCount = 1
  //   rareEffectsCount = 1;
  // }
  // else if (_rarity == RARITY.EPIC) {
  //   skillBonusEffectsCount = 1
  //   rareEffectsCount = 1;
  // }
  // else if (_rarity == RARITY.LEGENDARY) {
  //   skillBonusEffectsCount = 1
  //   rareEffectsCount = 1;
  // }

  switch (generatedEquip.equipSlotId) {
    case EQUIP_SLOT_ID.AMULET:
    case EQUIP_SLOT_ID.FINGER_1:
    case EQUIP_SLOT_ID.CHARM:
    case EQUIP_SLOT_ID.EARRING:
    case EQUIP_SLOT_ID.TRINKET:
      {
        skillBonusEffectsCount = 0;
        rareEffectsCount = 1;
        break;
      }
    case EQUIP_SLOT_ID.BACK:
    case EQUIP_SLOT_ID.BODY:
    case EQUIP_SLOT_ID.FEET:
    case EQUIP_SLOT_ID.HANDS:
    case EQUIP_SLOT_ID.LEGS:
    case EQUIP_SLOT_ID.HEAD:
    case EQUIP_SLOT_ID.MAIN_HAND:
    case EQUIP_SLOT_ID.OFF_HAND:
    case EQUIP_SLOT_ID.NECK:
    case EQUIP_SLOT_ID.SHOULDER:
    case EQUIP_SLOT_ID.WAIST:
    case EQUIP_SLOT_ID.WRIST:
      {
        skillBonusEffectsCount = 1;
        rareEffectsCount = 0;
        break;
      }
    case EQUIP_SLOT_ID.BAG:
      {
        skillBonusEffectsCount = 0;
        rareEffectsCount = 1;
        break;
      }
    default:
      break;
  }

  if (_rarity == RARITY.COMMON) {
    skillBonusEffectsCount = 0
    rareEffectsCount = 0;
  }


  //zkusime pridat skillBonusEffekt
  console.log("RESIM SKILL/BUFF BONUS EFFECT");


  //vyfiltrujem skilly podle clasy/rarity/slotu
  let filteredSkillsBonusDefinitions: SkillBonusEffectDefinition[] = [];
  for (const skillBonusEffect of _skillDefinitions.skillBonusEffects) {
    console.log("checkuju: " + skillBonusEffect.skillGroupId);
    //zkontrolujem raritu
    let rarityFilterPassed = false;
    for (const amount of skillBonusEffect.amounts) {
      if (amount.rarity == generatedEquip.rarity)
        rarityFilterPassed = true;

    }

    if (!rarityFilterPassed)
      continue;
    // //zkontrolujeme equip slot
    if (!(skillBonusEffect.equipSlots.includes(choosenEquipSlot.slotId) || skillBonusEffect.equipSlots.includes(EQUIP_SLOT_ID.ANY)))
      continue;

    //zkontrolujeme character class 
    if (!(skillBonusEffect.characterClass.includes(generatedEquip.skill.characterClass) || skillBonusEffect.characterClass.includes(CHARACTER_CLASS.ANY)))
      continue;

    filteredSkillsBonusDefinitions.push(skillBonusEffect);
  }

  //vyfiltrujem buffy podle clasy/rarity/slotu
  let filteredBuffBonusDefinitions: BuffBonusEffectDefinition[] = [];
  for (const buffBonusEffect of _skillDefinitions.buffBonusEffects) {
    //zkontrolujem raritu
    let rarityFilterPassed = false;
    for (const amount of buffBonusEffect.amounts) {
      if (amount.rarity == generatedEquip.rarity)
        rarityFilterPassed = true;

    }

    if (!rarityFilterPassed)
      continue;
    // //zkontrolujeme equip slot
    if (!(buffBonusEffect.equipSlots.includes(choosenEquipSlot.slotId) || buffBonusEffect.equipSlots.includes(EQUIP_SLOT_ID.ANY)))
      continue;

    //zkontrolujeme character class 
    if (!(buffBonusEffect.characterClass.includes(generatedEquip.skill.characterClass) || buffBonusEffect.characterClass.includes(CHARACTER_CLASS.ANY)))
      continue;

    filteredBuffBonusDefinitions.push(buffBonusEffect);
  }



  for (let i = 0; i < skillBonusEffectsCount; i++) {
    let randNumber = randomIntFromInterval(0, filteredSkillsBonusDefinitions.length + filteredBuffBonusDefinitions.length);

    if (randNumber <= filteredSkillsBonusDefinitions.length) {

      let rolledSkillBonus = rollForRandomItem(filteredSkillsBonusDefinitions) as SkillBonusEffectDefinition;
      if (rolledSkillBonus == null)
        continue;
      //dostanem amounts pro spravnou raritu
      let correctRarityAmountSkill = rolledSkillBonus.amounts.find((amount: AmountWithSpanAndRarity) => amount.rarity == _rarity);
      generatedEquip.skillBonusEffects.push(new SkillBonusEffect(rolledSkillBonus.skillGroupId, randomIntFromInterval(correctRarityAmountSkill!.min, correctRarityAmountSkill!.max), rolledSkillBonus.indexInArray, rolledSkillBonus.id, rolledSkillBonus.mathOperationType));
      filteredSkillsBonusDefinitions.splice(filteredSkillsBonusDefinitions.indexOf(rolledSkillBonus), 1);
    }
    else {
      let rolledBuffBonus = rollForRandomItem(filteredBuffBonusDefinitions) as BuffBonusEffectDefinition;
      if (rolledBuffBonus == null)
        continue;
      //dostanem amounts pro spravnou raritu
      let correctRarityAmountBuff = rolledBuffBonus.amounts.find((amount: AmountWithSpanAndRarity) => amount.rarity == _rarity);
      generatedEquip.buffBonusEffects.push(new BuffBonusEffect(rolledBuffBonus.buffGroupId, randomIntFromInterval(correctRarityAmountBuff!.min, correctRarityAmountBuff!.max), rolledBuffBonus.indexInArray, rolledBuffBonus.id, rolledBuffBonus.mathOperationType));
      filteredBuffBonusDefinitions.splice(filteredBuffBonusDefinitions.indexOf(rolledBuffBonus), 1);
    }

  }
  //zkusime pridat rare effect
  console.log("RESIM RARE EFFECT");

  for (let i = 0; i < rareEffectsCount; i++) {
    let filteredSkillsDefinitions: RareEffectDefinition[] = [];

    for (const rareEffect of _skillDefinitions.rareEffects) {
      //zkontrolujem raritu
      let rarityFilterPassed = false;
      for (const amount of rareEffect.amounts) {
        if (amount.rarity == generatedEquip.rarity)
          rarityFilterPassed = true;

        // if (!rarityFilterPassed)
        //   continue;

      }
      if (!rarityFilterPassed)
        continue;

      //zkontrolujeme equip slot
      if (!(rareEffect.equipSlots.includes(choosenEquipSlot.slotId) || rareEffect.equipSlots.includes(EQUIP_SLOT_ID.ANY)))
        continue;

      //zkontrolujeme character class 
      if (!(rareEffect.characterClass.includes(generatedEquip.skill.characterClass as CHARACTER_CLASS) || rareEffect.characterClass.includes(CHARACTER_CLASS.ANY)))
        continue;

      filteredSkillsDefinitions.push(rareEffect);
    }


    let rolledItem = rollForRandomItem(filteredSkillsDefinitions) as RareEffectDefinition;

    console.log("generatedEquip.skill.rarity :" + generatedEquip.skill.rarity);
    console.log("rolledItem:" + rolledItem.rareEffectId);
    //dostanem amounts pro spravnou raritu
    let correctRarityAmount = rolledItem.amounts.find((amount: AmountWithSpanAndRarity) => amount.rarity == _rarity);

    generatedEquip.rareEffects.push(new RareEffect(rolledItem.rareEffectId, randomIntFromInterval(correctRarityAmount!.min, correctRarityAmount!.max)));
  }



  // if (rarityFilterPassed)// muze se objevit na teto rarite itemu
  //   if (rareEffect.equipSlots.includes(choosenEquipSlot.slotId) || rareEffect.equipSlots.includes(EQUIP_SLOT_ID.ANY)) // muze se objevit na  vybranemu equip slotu
  //     if (rareEffect.characterClass.includes(generatedEquip.skill.characterClass as CHARACTER_CLASS) || rareEffect.characterClass.includes(CHARACTER_CLASS.ANY))// muze se objevit na tomto itemu pro danou classu
  //     {

  //       let rolledItem = rollForRandomItem(_skillDefinitions.rareEffects, true) as RareEffectDefinition;
  //       generatedEquip.rareEffects.push(new RareEffect(rolledItem.rareEffectId, rolledItem.amounts);
  //ok vse proslo, tak zkusim stesti a pridat rareeffekt
  // if (Math.random() < rareEffect.chanceToSpawn) {

  // //vyberu rank rareEfektu
  // let choosenRank = -1;
  // for (let i = 0; i < rareEffect.ranks.length; i++) {
  //   if (rareEffect.ranks[i].iLevelMax >= generatedEquip.iLevel && rareEffect.ranks[i].iLevelMin <= generatedEquip.iLevel) {
  //     choosenRank = i;
  //     break;
  //   }
  // }
  // if (choosenRank == -1)
  //   throw ("Cannot find valid rank for rare effect" + rareEffect.rareEffectId + " at iLevel :" + generatedEquip.iLevel.toString());

  // if (rareEffect.ranks[choosenRank].price < budget) //mam dost budgetu na to pridat teto rare effect
  // {
  //   console.log("pridavaam rare efekt : " + rareEffect.rareEffectId);
  //   if (rareEffect.rareEffectId != RARE_EFFECT.BAG_SIZE) {

  //     //vygeneruju nahodny max health a prepisu tim amount
  //     rareEffect.ranks[choosenRank].amounts[0] = randomIntFromInterval(rareEffect.ranks[choosenRank].amounts[0], rareEffect.ranks[choosenRank].amounts[1]);
  //     //smazu zbytecny amount
  //     rareEffect.ranks[choosenRank].amounts.pop();

  //   }

  //   generatedEquip.rareEffects.push(new RareEffect(rareEffect.rareEffectId, rareEffect.ranks[choosenRank].amounts, choosenRank + 1));

  //   console.log("buget je : " + budget);
  //   budget -= rareEffect.ranks[choosenRank].price;
  //   console.log("po pridani efektu buget je : " + budget + " protoze efekt stoji : " + rareEffect.ranks[choosenRank].price);

  //   if (budget < 0)
  //     throw ("Trying to add a rare effect with higher price than item budget - rareEffectId : " + rareEffect.rareEffectId + " - price : " + rareEffect.ranks[choosenRank].price + " - item budget :" + (budget + rareEffect.ranks[choosenRank].price) + " - item iLevel : " + generatedEquip.iLevel);
  // }

  //  break; //jen 1 effekt je ted mozne pridat....
  // }
  // }

  //}



  //ted zatim proste projdu kazdy rare effect hoidm si kostkou a pridam ho pokud to projde....1 JE MAX
  //zamicham pole aby se hazelo prvni vzdy na jine effekty


  // do {
  //   //najdu nahodny atribut
  //   const attributeToAdd = attributePricesMeta.attributePrices[randomIntFromInterval(0, attributePricesMeta.attributePrices.length - 1)];

  //   //pripocitam ho do equipu
  //      generatedEquip.attributes.setAttributeById(attributeToAdd.attributeId, 1);

  //   //snizim budget
  //   budget -= attributeToAdd.price;

  // } while (budget > 0);



  return generatedEquip;

}



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
      let callerCharacterData: CharacterDocument = callerCharacterDoc.data();

      validateCallerBulletProof(callerCharacterData, context);


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


exports.upgradeEquipQuality = functions.https.onCall(async (data, context) => {

  const callerCharacterUid = data.characterUid;
  const equipToUpgradeUid = data.equipToUpgradeUid; //Array of uid strings

  const callerCharacterDb = admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);
  //TODO : check na to jestli jsi vubec na nejake FORGE

  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {

      const callerCharacterDoc = await t.get(callerCharacterDb);

      let callerCharacterData: CharacterDocument = callerCharacterDoc.data();


      let equipToUpgrade = callerCharacterData.getEquipFromInventoryOfEquipment(equipToUpgradeUid);
      //sebereme requirement resources

      for (const materialNeeded of equipToUpgrade.qualityUpgradeMaterials[equipToUpgrade.quality].materialsNeeded) {
        callerCharacterData.removeContentFromInventoryById(materialNeeded.itemId, materialNeeded.amount);
      }

      if (equipToUpgrade.quality == equipToUpgrade.qualityMax)
        throw ("Equip :" + equipToUpgradeUid + " is already at maximum quality");

      //upgradneme equip
      equipToUpgrade.quality++;

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