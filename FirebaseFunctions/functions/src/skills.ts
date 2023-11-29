
// [START import]
import * as functions from "firebase-functions";
import { CharacterDocument, CHARACTER_CLASS, rollForRandomItem } from ".";
import { CombatBuff, CombatMember, EncounterDocument } from "./encounter";
import { BuffBonusEffect, IHasChanceToSpawn, MATH_OPERATION_TYPE, SkillBonusEffect } from "./equip";
import { firestoreAutoId } from "./general2";



const admin = require('firebase-admin');

// // [END import]



export const enum BUFF_GROUP {
  BLEED_BUFF = "BLEED_BUFF",
  REJUVENATION_BUFF = "REJUVENATION_BUFF",
  CRUSADER_BUFF = "CRUSADER_BUFF",
  DEFENSIVE_STANCE_BUFF = "DEFENSIVE_STANCE_BUFF",
  MYSTIC_SHIELD_BUFF = "MYSTIC_SHIELD_BUFF",
  MORTAL_WOUNDS_BUFF = "MORTAL_WOUNDS_BUFF",
}

export const enum BUFF {
  SHIELD_WALL_BUFF_1 = "SHIELD_WALL_BUFF_1",
  WEAKNESS_BUFF_1 = "WEAKNESS_BUFF_1",
  REJUVENATION_BUFF_1 = "REJUVENATION_BUFF_1",
  REJUVENATION_BUFF_2 = "REJUVENATION_BUFF_2",
  REJUVENATION_BUFF_3 = "REJUVENATION_BUFF_3",
  FRAGILITY_BUFF_1 = "FRAGILITY_BUFF_1",
  STRENGTH_BUFF = "STRENGTH_BUFF",
  BLEED_BUFF_1 = "BLEED_BUFF_1",
  CRUSADER_BUFF_1 = "CRUSADER_BUFF_1",
  DEFENSIVE_STANCE_BUFF_1 = "DEFENSIVE_STANCE_BUFF_1",
  MORTAL_WOUNDS_BUFF_1 = "MORTAL_WOUNDS_BUFF_1",

  MONSTER_STRENGTH_BUFF = "MONSTER_STRENGTH_BUFF",
  LIGHTNING_SHIELD_BUFF_2 = "LIGHTNING_SHIELD_BUFF_2",
  LIGHTNING_SHIELD_BUFF_3 = "LIGHTNING_SHIELD_BUFF_3",
  BLOOD_LUST_BUFF_1 = "BLOOD_LUST_BUFF_1",
  SIPHON_LIFE_BUFF_2 = "SIPHON_LIFE_BUFF_2",
  CORRUPTION_BUFF_1 = "CORRUPTION_BUFF_1",
  CORRUPTION_BUFF_2 = "CORRUPTION_BUFF_2",
  CORRUPTION_BUFF_3 = "CORRUPTION_BUFF_3",
  MYSTIC_SHIELD_BUFF_1 = "MYSTIC_SHIELD_BUFF_1",
  COUNTERSTRIKE_BUFF_1 = "COUNTERSTRIKE_BUFF_1"
}

export const enum SKILL_GROUP {

  //Special 
  UNPREPARED = "UNPREPARED",
  //Curses
  CURSE = "CURSE",

  //Any
  PUNCH = "PUNCH",
  FIRST_AID = "FIRST_AID",

  //Warrior
  SLAM = "SLAM",
  EXECUTE = "EXECUTE",
  BARRIER_STRIKE = "BARRIER_STRIKE",
  REND = "REND",
  BLOCK = "BLOCK",
  TAUNT = "TAUNT",
  BUCKLER = "BUCKLER",
  MORTAL_STRIKE = "MORTAL_STRIKE",

  //SHAMAN
  LIGHTNING = "LIGHTNING",
  CHAIN_LIGHTNING = "CHAIN_LIGHTNING"
}
export const enum SKILL {

  //Universal
  PUNCH_1 = "PUNCH_1",
  BLOCK_1 = "BLOCK_1",

  //Warrior
  //COMMON
  CLEAVE_1 = "CLEAVE_1",
  BUCKLER_1 = "BUCKLER_1",  //new
  SLAM_1 = "SLAM_1",
  REND_1 = "REND_1",
  BARRIER_STRIKE_1 = "BARRIER_STRIKE_1", //new
  COUNTERSTRIKE_1 = "COUNTERSTRIKE_1", //new

  //UNCOMMON
  EXECUTE_1 = "EXECUTE_1",
  FIRST_AID_1 = "FIRST_AID_1",
  TAUNT_1 = "TAUNT_1",
  SHIELD_BASH_1 = "SHIELD_BASH_1", //new
  MYSTIC_SHIELD_1 = "MYSTIC_SHIELD_1", //new

  //RARE
  SHIELD_WALL_1 = "SHIELD_WALL_1",
  DEFENSIVE_STANCE_1 = "DEFENSIVE_STANCE_1",
  CRUSADER_1 = "CRUSADER_1",

  //EPIC
  HONEY_BADGER_1 = "HONEY_BADGER_1",
  MORTAL_STRIKE_1 = "MORTAL_STRIKE_1",
  //FEED = "FEED" // deal damage if fatal increase you characer hp by 3 permanently!


  //Shaman
  REJUVENATION_1 = "REJUVENATION_1",
  REJUVENATION_2 = "REJUVENATION_2",
  REJUVENATION_3 = "REJUVENATION_3",
  HEALING_WAVE_1 = "HEALING_WAVE_1",
  HEALING_WAVE_2 = "HEALING_WAVE_2",
  HEALING_WAVE_3 = "HEALING_WAVE_3",
  CHAIN_LIGHTNING_2 = "CHAIN_LIGHTNING_2",
  CHAIN_LIGHTNING_3 = "CHAIN_LIGHTNING_3",
  LIGHTNING_1 = "LIGHTNING_1",
  LIGHTNING_2 = "LIGHTNING_2",
  LIGHTNING_3 = "LIGHTNING_3",
  LIGHTNING_SHIELD_2 = "LIGHTNING_SHIELD_2",
  LIGHTNING_SHIELD_3 = "LIGHTNING_SHIELD_3",
  BLOOD_LUST_1 = "BLOOD_LUST_1",
  BLOOD_LUST_2 = "BLOOD_LUST_2",
  BLOOD_LUST_3 = "BLOOD_LUST_3",
  //buff windfury, sance ze zakouzlis zakozlene kouzlo po 2.??

  //Warlock
  SHADOWBOLT_1 = "SHADOWBOLT_1",
  SHADOWBOLT_2 = "SHADOWBOLT_2",
  SHADOWBOLT_3 = "SHADOWBOLT_3",
  CURSE_OF_WEAKNESS_2 = "CURSE_OF_WEAKNESS_2",
  CURSE_OF_WEAKNESS_3 = "CURSE_OF_WEAKNESS_3",
  LIFE_TAP_1 = "LIFE_TAP_1",
  LIFE_TAP_2 = "LIFE_TAP_2",
  LIFE_TAP_3 = "LIFE_TAP_3",
  SIPHON_LIFE_2 = "SIPHON_LIFE_2",
  SIPHON_LIFE_3 = "SIPHON_LIFE_3",
  CORRUPTION_1 = "CORRUPTION_1",
  CORRUPTION_2 = "CORRUPTION_2",
  CORRUPTION_3 = "CORRUPTION_3",

  //CORRUPTION - one time spell, velka DOT? co kazdym kolem dava vic dmg?

  //Cruses
  CURSE_BROKEN_LEG = "CURSE_BROKEN_LEG",
  CURSE_DECAY = "CURSE_DECAY",
  CURSE_MANA_COST_INCREASE = "CURSE_MANA_COST_INCREASE", //neimplementovana
  CURSE_UNPREPARED = "CURSE_UNPREPARED" // curse za kazdy equip co ti chybi na sobe
}

// [Skill]
export class QualityAmounts {

  constructor(
    public amounts: number[]
  ) { }

}


// [Buff]
export class Buff {

  constructor(
    public buffId: string,
    public durationTurns: number,
    public quality: QualityAmounts[],
    public buffGroupId: string,
  ) { }

}

export class SkillDefinitions {

  constructor(
    //public uid: string,
    public skills: SkillDefinition[],
    public buffs: Buff[], //neni treba mit rozdeleny buff a buffdefinition protoze je to to same, co je v buffDefinition to je pak i v Buffu, az budu mit v databazi nejake extra data co pak nechci aby mel klient tak to rozstepim
    public curses: SkillDefinition[],
    public rareEffects: RareEffectDefinition[],
    public skillBonusEffects: SkillBonusEffectDefinition[],
    public buffBonusEffects: BuffBonusEffectDefinition[],
    public validTarget_AnyAlly: boolean,
    public validTarget_Self: boolean,
    public validTarget_AnyEnemy: boolean,
  ) { }


}


export class AmountWithSpanAndRarity {

  constructor(
    //public uid: string,
    public min: number,
    public max: number,
    public rarity: string
  ) { }


}

export class SkillBonusEffectDefinition {

  constructor(
    //public uid: string,
    public amounts: AmountWithSpanAndRarity[],
    public skillGroupId: string,
    public chanceToSpawn: number,
    public indexInArray: number,
    public characterClass: string[],
    public equipSlots: string[],
    public id: string,
    public mathOperationType: string
  ) { }

}



export class BuffBonusEffectDefinition {

  constructor(
    //public uid: string,
    public amounts: AmountWithSpanAndRarity[],
    public buffGroupId: string,
    public chanceToSpawn: number,
    public indexInArray: number,
    public characterClass: string[],
    public equipSlots: string[],
    public id: string,
    public mathOperationType: string
  ) { }

}

export class RareEffectDefinition implements IHasChanceToSpawn {

  constructor(
    //public uid: string,
    public rareEffectId: string,
    //   public price: number,//NEPOUZIVANE?
    public amounts: AmountWithSpanAndRarity[],
    public characterClass: string[],
    public equipSlots: string[],
    public chanceToSpawn: number,
  ) { }

}



// [Skill]
export class SkillDefinition implements IHasChanceToSpawn {

  constructor(
    //public uid: string,
    public skillId: string,
    public skillGroupId: string,
    public singleUse: boolean,
    public characterClass: string,
    public rarity: string,
    public equipSlots: string[],
    public manaCost: number,
    public quality: QualityAmounts[],
    public buffId: string,
    public chanceToSpawn: number,
    public validTarget_AnyAlly: boolean,
    public validTarget_Self: boolean,
    public validTarget_AnyEnemy: boolean,
    //public successSlots: number
  ) { }

}


// // [Curse]
// export class CurseDefinition implements IHasChanceToSpawn {

//   constructor(
//     //public uid: string,
//     public curseId: string,
//     public characterClass: string,
//     public singleUse: boolean,
//     public rarity: string,
//     public manaCost: number,
//     public amounts: number[],
//     public chanceToSpawn: number,
//     public validTarget_AnyAlly: boolean,
//     public validTarget_Self: boolean,
//     public validTarget_AnyEnemy: boolean
//   ) { }

// }
// export class BuffDefinition {

//   constructor(
//     public buffId: string,
//     public quality: QualityAmounts[],

//   ) { }

// }

//je vytvoren ze SkillDefintion a je to to co ma equip, z tohodle se vytvari combat skill
export class Skill {

  constructor(
    //public uid: string,
    public skillId: string,
    public skillGroupId: string,
    public singleUse: boolean,
    public characterClass: string,
    public manaCost: number,
    public quality: QualityAmounts[],
    public buff: Buff | undefined,
    public validTarget_AnyAlly: boolean,
    public validTarget_Self: boolean,
    public validTarget_AnyEnemy: boolean,
    public rarity: string,
    // public successSlots: number

  ) { }

}


// export class StatsSkill {
//   constructor(
//     public damage_A: number,
//     public heal_A: number,
//     //public manaCost: number
//   ) { }
// }

export class CombatSkillOriginalStats {
  constructor(
    public manaCost: number,
    public amountsSkill: number[],
    public amountsBuff: number[] | null
  ) { }
}



export class Combatskill {
  constructor(
    public skillId: string,
    public characterClass: string,
    public manaCost: number,
    public amounts: number[],
    public alreadyUsed: boolean,
    public uid: string,
    public buff: CombatBuff | undefined,
    public skillGroupId: string,
    public singleUse: boolean,
    public validTarget_AnyAlly: boolean,
    public validTarget_Self: boolean,
    public validTarget_AnyEnemy: boolean,
    public rarity: string,
    public originalStats: CombatSkillOriginalStats,
    public quality: number,


  ) { }//{ super(skillId, characterClass, manaCost, amounts, rank, buff) }


  addSkillBonus(_skillBonus: SkillBonusEffect) {

    if (_skillBonus.skillGroupId == this.skillGroupId) {

      if (_skillBonus.indexInArray > this.amounts.length - 1)  //rare efekt se nejspis snazi zvednout atribut ktery je dostupny az u vyssi rarity skillu, tim padem u tohodle skillu co ho nema jej ignoruju
        return;

      // console.log("_skillBonus.skillGroupId :" + _skillBonus.skillGroupId)
      if (_skillBonus.mathOperationType == MATH_OPERATION_TYPE.ADD)
        this.originalStats.amountsSkill[_skillBonus.indexInArray] += _skillBonus.amount;
      else if (_skillBonus.mathOperationType == MATH_OPERATION_TYPE.SUBSTRACT)
        this.originalStats.amountsSkill[_skillBonus.indexInArray] -= _skillBonus.amount;

      //console.log(" this.amounts[_skillBonus.indexInArray]" +  this.amounts[_skillBonus.indexInArray]);
    }
  }

  addBuffBonus(_buffBonus: BuffBonusEffect) {

    if (this.buff == undefined)
      return;
    if (_buffBonus.buffGroupId == this.buff.buffGroupId) {

      if (_buffBonus.indexInArray > this.buff.amounts.length - 1)  //rare efekt se nejspis snazi zvednout atribut ktery je dostupny az u vyssi rarity skillu, tim padem u tohodle skillu co ho nema jej ignoruju
        return;


      if (_buffBonus.mathOperationType == MATH_OPERATION_TYPE.ADD) {
        if (_buffBonus.indexInArray == -1) {
          this.buff.durationTurns += _buffBonus.amount;
          this.buff.turnsLeft += _buffBonus.amount;
        }
        else
          this.originalStats.amountsBuff![_buffBonus.indexInArray] += _buffBonus.amount;
      }
      else if (_buffBonus.mathOperationType == MATH_OPERATION_TYPE.SUBSTRACT) {
        if (_buffBonus.indexInArray == -1) {
          this.buff.durationTurns -= _buffBonus.amount;
          this.buff.turnsLeft -= _buffBonus.amount;
        }
        else
          this.originalStats.amountsBuff![_buffBonus.indexInArray] -= _buffBonus.amount;
      }
    }
  }

  addDmgPower(_dmgPower: number) {
    switch (this.skillGroupId) {
      case SKILL_GROUP.SLAM:
      case SKILL_GROUP.PUNCH:
      case SKILL_GROUP.BARRIER_STRIKE:
      case SKILL_GROUP.REND:
      case SKILL_GROUP.EXECUTE:
      case SKILL_GROUP.MORTAL_STRIKE:

        console.log("this.skillGroupId:" + this.skillGroupId + "adding:" + this.originalStats.amountsSkill[0] + " power : " + _dmgPower);

        this.originalStats.amountsSkill[0] += _dmgPower;
        break;


      default:
        break;
    }
  }

  addHealingPower(_amount: number) {
    switch (this.skillId) {
      // case SKILL.FIRST_AID:
      // case SKILL.HEALING_WAVE:

      //   this.amounts[0] += _amount;
      //   break;
      // case SKILL.SLAM:
      //   this.amounts[1] += _amount;
      //   break;

      // default:
      //   break;
    }

    if (this.buff != undefined) {
      switch (this.buff?.buffId) {
        case BUFF.REJUVENATION_BUFF_1:
        case BUFF.REJUVENATION_BUFF_2:
        case BUFF.REJUVENATION_BUFF_3:
          this.originalStats.amountsSkill[0] += _amount;
          break;

        default:
          break;
      }
    }
  }

  addDefense(_amount: number) {
    switch (this.skillGroupId) {
      case SKILL_GROUP.BLOCK:
      case SKILL_GROUP.BUCKLER:
      case SKILL_GROUP.TAUNT:
      case SKILL_GROUP.BARRIER_STRIKE:

        this.originalStats.amountsSkill[0] += _amount;
        break;


      default:
        break;
    }
  }

  onTurnEnded(_owner: CombatMember, _encounter: EncounterDocument) {
    switch (this.skillId) {
      case SKILL.CURSE_DECAY:

        _encounter.dealDamageToCombatEntity(_owner, _owner, this.amounts[0], this.skillId, _encounter);

        break;

      default:
        break;
    }
  }
}



export const iLEVEL_MAX = 100;
export const mLEVEL_MAX = 60;


export function getRandomCurseAsCombatSkill(_character: CharacterDocument, _skillDefinitions: SkillDefinitions, _quality: number): Combatskill {

  for (let i = _skillDefinitions.curses.length - 1; i >= 0; i--) {
    if (_skillDefinitions.curses[i].characterClass != CHARACTER_CLASS.ANY && _skillDefinitions.curses[i].characterClass != _character.characterClass)
      _skillDefinitions.curses.splice(i, 1);
  }

  let choosenCurse: SkillDefinition = rollForRandomItem(_skillDefinitions.curses) as SkillDefinition; // CURSE_DEFINITIONS.items[randomIntFromInterval(0, CURSE_DEFINITIONS.items.length - 1)];
  console.log("choosen curse :" + choosenCurse.skillId);

  return ConvertSkillToCombatSkill(ConvertSkillDefinitionToSkill(choosenCurse, _skillDefinitions), _quality);

}


export function getRandomCurse(_character: CharacterDocument, _skillDefinitions: SkillDefinitions, _quality: number): Skill {

  for (let i = _skillDefinitions.curses.length - 1; i >= 0; i--) {
    if (_skillDefinitions.curses[i].characterClass != CHARACTER_CLASS.ANY && _skillDefinitions.curses[i].characterClass != _character.characterClass)
      _skillDefinitions.curses.splice(i, 1);
  }

  let choosenCurse: SkillDefinition = rollForRandomItem(_skillDefinitions.curses) as SkillDefinition; // CURSE_DEFINITIONS.items[randomIntFromInterval(0, CURSE_DEFINITIONS.items.length - 1)];
  console.log("choosen curse :" + choosenCurse.skillId);

  return ConvertSkillDefinitionToSkill(choosenCurse, _skillDefinitions);

}



export function ConvertSkillDefinitionToSkill(_skillDefinition: SkillDefinition, _skillDefinitions: SkillDefinitions) {
  return new Skill(_skillDefinition.skillId, _skillDefinition.skillGroupId, _skillDefinition.singleUse, _skillDefinition.characterClass, _skillDefinition.manaCost, _skillDefinition.quality, getBuffById(_skillDefinition.buffId, _skillDefinitions), _skillDefinition.validTarget_AnyAlly, _skillDefinition.validTarget_Self, _skillDefinition.validTarget_AnyEnemy, _skillDefinition.rarity);

}

export function ConvertSkillToCombatSkill(_skillDefinition: Skill, _quality: number): Combatskill {

  let combatBuff: CombatBuff | undefined = undefined;


  let amountsBuffs: number[] | null = null;
  if (_skillDefinition.buff != undefined) {
    amountsBuffs = _skillDefinition.buff.quality[_quality].amounts.slice();
    combatBuff = new CombatBuff(_skillDefinition.buff.buffId, _skillDefinition.buff.durationTurns, _skillDefinition.buff.durationTurns, amountsBuffs, _skillDefinition.buff.buffGroupId, "")
  }
  if (_skillDefinition.quality[_quality] == null)
    throw ("Missing definition for quality : " + _quality + " _skillDefinition:" + _skillDefinition.skillId);

  let amounts = _skillDefinition.quality[_quality].amounts.slice();

  return new Combatskill(_skillDefinition.skillId, _skillDefinition.characterClass, _skillDefinition.manaCost, _skillDefinition.quality[_quality].amounts.slice(), false, firestoreAutoId(), combatBuff, _skillDefinition.skillGroupId, _skillDefinition.singleUse, _skillDefinition.validTarget_AnyAlly, _skillDefinition.validTarget_Self, _skillDefinition.validTarget_AnyEnemy, _skillDefinition.rarity, new CombatSkillOriginalStats(_skillDefinition.manaCost, amounts, amountsBuffs), _quality);
}


/*export function attachSkillToEquip(_equip: Equip, _characterClass: string, _skillDefinitions : SkillDefinition[]) {
  //TODO: vyfiltrovat possible skilly podle equipu

  // for (var i = _skillDefinitions.length - 1; i >= 0; i--) {
  //   if (!SKILL_DEFINITIONS.items[i].rarities.includes(_equip.rarity as RARITY) && !SKILL_DEFINITIONS.items[i].rarities.includes(RARITY.ANY)) {
  //     console.log("Vyradil sem skill: " + SKILL_DEFINITIONS.items[i].skillId + " nesplnuje podminky pro raritu : " + _equip.rarity);
  //     SKILL_DEFINITIONS.items.splice(SKILL_DEFINITIONS.items.indexOf(SKILL_DEFINITIONS.items[i]), 1);
  //   }
  //   // else
  //   //console.log("skill: " + SKILL_DEFINITIONS.items[i].skillId + " splnuje podminky pro raritu : " + _equip.rarity);
  // }

  let choosenSkill = SKILL_DEFINITIONS.items[0];  //SLAM will be default skill when we reach more than 50 tries to get character specific skil.....

  //musime vybrat skill pro danou classu
  //TODO : asi by bylo lepsi vyfiltrovat ten array podle characteru ne? a pak random ten skill? nez takhle idiotsky?
  if (_characterClass != CHARACTER_CLASS.ANY) {
    let loopCount = 0;
    let correctSkillFound = false;
    do {
      choosenSkill = SKILL_DEFINITIONS.items[randomIntFromInterval(0, SKILL_DEFINITIONS.items.length - 1)];

      if (choosenSkill.characterClass == _characterClass)
        correctSkillFound = true;

      loopCount++;
      if (loopCount > 50)
        correctSkillFound = true;

    } while (!correctSkillFound);
  }
  else
    choosenSkill = SKILL_DEFINITIONS.items[randomIntFromInterval(0, SKILL_DEFINITIONS.items.length - 1)];

  _equip.skill = convertSkillToGiveniLevel(choosenSkill.skillId, _equip.iLevel);
}
*/


export function getCurseById(_curseId: string, _skillDefinitions: SkillDefinitions): SkillDefinition {
  let foundItem: SkillDefinition | undefined;

  for (let i = 0; i < _skillDefinitions.curses.length; i++) {
    if (_skillDefinitions.curses[i].skillId === _curseId) {
      foundItem = _skillDefinitions.curses[i];
      break;
    }
  }
  if (foundItem) {
    return foundItem // this will print the item with id "SLAM"
  } else {
    throw ("No curse with id " + _curseId + " found.");
  }

}


export function getSkillById(_skillId: string, _skillDefinitions: SkillDefinitions): SkillDefinition {
  let foundItem: SkillDefinition | undefined;

  for (let i = 0; i < _skillDefinitions.skills.length; i++) {
    if (_skillDefinitions.skills[i].skillId === _skillId) {
      foundItem = _skillDefinitions.skills[i];
      break;
    }
  }
  if (foundItem) {
    return foundItem // this will print the item with id "SLAM"
  } else {
    throw ("No item with id " + _skillId + " found.");
  }

}



export function getBuffById(_buffId: string, _skillDefinitions: SkillDefinitions): Buff | undefined {
  let foundItem: Buff | undefined;
  if (_buffId == "")
    return undefined;

  for (let i = 0; i < _skillDefinitions.buffs.length; i++) {
    if (_skillDefinitions.buffs[i].buffId == _buffId) {
      foundItem = _skillDefinitions.buffs[i];
      break;
    }
  }
  if (foundItem) {
    return foundItem // this will print the item with id "SLAM"
  } else {
    throw ("No buff with id " + _buffId + " found.");
  }

}


exports.grantNewSkill = functions.https.onCall(async (data, context) => {

  var requestData = {
    encounterUid: data.encounterUid,
    characterUid: data.characterUid,
  }

  const characterToGetNewSkillDb = admin.firestore().collection('characters').doc(requestData.characterUid);
  console.log(characterToGetNewSkillDb);
});





// [END allAdd]