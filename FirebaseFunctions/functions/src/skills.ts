
// [START import]
import * as functions from "firebase-functions";
import { CHARACTER_CLASS } from ".";
import { CombatBuff } from "./encounter";
import { AllEquipSlots, Equip, RARITY } from "./equip";
import { randomIntFromInterval } from "./general2";


const admin = require('firebase-admin');

// // [END import]


export const enum BUFF {
  SHIELD_WALL_BUFF = "SHIELD_WALL_BUFF",
  WEAKNESS_BUFF = "WEAKNESS_BUFF",
  REJUVENATION_BUFF = "REJUVENATION_BUFF",
  FRAGILITY_BUF = "FRAGILITY_BUFF"
}

export const enum SKILL {

  //Universal
  FIRST_AID = "FIRST_AID",//
  PUNCH = "PUNCH",
  SLAM = "SLAM",

  //Warrior
  EXECUTE = "EXECUTE",
  CLEAVE = "CLEAVE",
  SHIELD_WALL = "SHIELD_WALL",

  //Shaman
  REJUVENATION = "REJUVENATION",
  HEALING_WAVE = "HEALING_WAVE",
  CHAIN_LIGHTNING = "CHAIN_LIGHTNING",


  //Warlock
  SHADOWBOLT = "SHADOWBOLT",
  CURSE_OF_WEAKNESS = "CURSE_OF_WEAKNESS",
  LIFE_TAP = "LIFE_TAP"
  //Mana tap


}



// [Skill]
export class Buff {

  constructor(
    public buffId: string,
    public durationTurns: number,
    public amounts: number[],
    public rank: number
  ) { }

}


// [Skill]
export class Skill {

  constructor(
    //public uid: string,
    public skillId: string,
    public characterClass: string,
    public manaCost: number,
    public amounts: number[],
    //  public imageId : string,
    public rank: number,
    public buff: Buff | undefined
    // public equipSlot: number,

  ) { }

}

export class StatsSkill {
  constructor(
    public damage_A: number,
    public heal_A: number,
    //public manaCost: number
  ) { }
}



export class Combatskill extends Skill {
  constructor(
    public skillId: string,
    public characterClass: string,
    public manaCost: number,
    public amounts: number[],
    public alreadyUsed: boolean,
    public handSlotIndex: number, // mysleno na kterem indexu z tech karet v ruce ten skill je
    public rank: number,
    public buff: CombatBuff | undefined


  ) { super(skillId, characterClass, manaCost, amounts, rank, buff) }
}



const iLevelMaximum = 100;

//TADY TOHLE MAM ZATIM NATVRDO, slo by to cele proste dat do databaze, nicmene takhle nemusim volat dalsi read pokazde kdyz generuju equip a navic je to dostupne pro vsechny vsude v kodu, zajimava alternativa k metadatum ulozenym v DB???
export const SKILL_DEFINITIONS =
{
  "items":
    [
      {
        //Deals damage and heal yourself
        "skillId": SKILL.SLAM,
        "characterClass": CHARACTER_CLASS.ANY,

        "iLevelMin": 1,
        "iLevelMax": iLevelMaximum,
        "rarities": [RARITY.COMMON, RARITY.UNCOMMON, RARITY.RARE, RARITY.EPIC],
        "equipSlots": AllEquipSlots,

        //0-Damage
        //1-Heal
        "ranks":
          [
            {
              "manaCost": 8,
              "iLevelMin": 0,
              "iLevelMax": 10,
              "amounts": [10, 3],
            },
            {
              "manaCost": 10,
              "iLevelMin": 10,
              "iLevelMax": iLevelMaximum,
              "amounts": [16, 5],
            }
          ]
      },
      {
        //Deals damage and heal yourself
        "skillId": SKILL.EXECUTE,
        "characterClass": CHARACTER_CLASS.WARRIOR,
        "iLevelMin": 1,
        "iLevelMax": iLevelMaximum,
        "rarities": [RARITY.COMMON, RARITY.UNCOMMON, RARITY.RARE, RARITY.EPIC],  //KDYZ SEM TAM DAL TY ENUMY TAK ERROR V KONZOLI
        "equipSlots": ["BODY", "HEAD", "LEGS", "FINGER_1"],
        //0-Damage
        "ranks":
          [
            {
              "manaCost": 12,
              "iLevelMin": 0,
              "iLevelMax": 10,
              "amounts": [15],
            },
            {
              "manaCost": 15,
              "iLevelMin": 10,
              "iLevelMax": iLevelMaximum,
              "amounts": [25],
            }
          ]

      },
      {
        //Deals damage and heal yourself
        "skillId": SKILL.CLEAVE,
        "characterClass": CHARACTER_CLASS.WARRIOR,
        "iLevelMin": 1,
        "iLevelMax": iLevelMaximum,
        "rarities": [RARITY.COMMON, RARITY.UNCOMMON, RARITY.RARE, RARITY.EPIC],  //KDYZ SEM TAM DAL TY ENUMY TAK ERROR V KONZOLI
        "equipSlots": ["BODY", "HEAD", "LEGS", "FINGER_1"],
        //0-Damage
        "ranks":
          [
            {
              "manaCost": 20,

              "iLevelMin": 0,
              "iLevelMax": 10,
              "amounts": [10],
            },
            {
              "manaCost": 24,

              "iLevelMin": 10,
              "iLevelMax": iLevelMaximum,
              "amounts": [18],
            }
          ]

      },
      {
        "skillId": SKILL.FIRST_AID,
        "characterClass": CHARACTER_CLASS.ANY,
        "iLevelMin": 1,
        "iLevelMax": iLevelMaximum,
        "rarities": [RARITY.COMMON, RARITY.UNCOMMON, RARITY.RARE, RARITY.EPIC],  //KDYZ SEM TAM DAL TY ENUMY TAK ERROR V KONZOLI
        "equipSlots": ["BODY", "HEAD", "LEGS", "FINGER_1"],
        //0-Heal
        "ranks":
          [
            {
              "manaCost": 2,

              "iLevelMin": 0,
              "iLevelMax": 10,
              "amounts": [25],
            },
            {
              "manaCost": 3,

              "iLevelMin": 10,
              "iLevelMax": iLevelMaximum,
              "amounts": [35],
            }
          ]
      },
      {
        "skillId": SKILL.HEALING_WAVE,
        "characterClass": CHARACTER_CLASS.SHAMAN,
        "iLevelMin": 1,
        "iLevelMax": iLevelMaximum,
        "rarities": [RARITY.COMMON, RARITY.UNCOMMON, RARITY.RARE, RARITY.EPIC],
        "equipSlots": ["BODY", "HEAD", "LEGS", "FINGER_1"],
        //0-Heal
        "ranks":
          [
            {
              "manaCost": 5,
              "iLevelMin": 0,
              "iLevelMax": 10,
              //0-heal amount
              "amounts": [50],
            },
            {
              "manaCost": 7,
              "iLevelMin": 10,
              "iLevelMax": iLevelMaximum,
              "amounts": [80],
            }
          ]
      },
      {
        "skillId": SKILL.CHAIN_LIGHTNING,
        "characterClass": CHARACTER_CLASS.SHAMAN,
        "iLevelMin": 1,
        "iLevelMax": iLevelMaximum,
        "rarities": [RARITY.COMMON, RARITY.UNCOMMON, RARITY.RARE, RARITY.EPIC],
        "equipSlots": ["BODY", "HEAD", "LEGS", "FINGER_1"],
        //0-Heal
        "ranks":
          [
            {
              "manaCost": 10,
              "iLevelMin": 0,
              "iLevelMax": 10,
              //0-damage amount
              //1-jump reduction damage amount in %
              "amounts": [20, 0.25],
            },
            {
              "manaCost": 13,
              "iLevelMin": 10,
              "iLevelMax": iLevelMaximum,
              "amounts": [35, 0.25],
            }
          ]
      },
      {
        "skillId": SKILL.LIFE_TAP,
        "characterClass": CHARACTER_CLASS.WARLOCK,
        "iLevelMin": 1,
        "iLevelMax": iLevelMaximum,
        "rarities": [RARITY.COMMON, RARITY.UNCOMMON, RARITY.RARE, RARITY.EPIC],
        "equipSlots": ["BODY", "HEAD", "LEGS", "FINGER_1"],
        "ranks":
          [
            {
              "manaCost": 0,
              "iLevelMin": 0,
              "iLevelMax": 10,
              //(0)HP taken 
              //(1)Mana recieved
              "amounts": [30, 5]
            },
            {
              "manaCost": 0,

              "iLevelMin": 10,
              "iLevelMax": iLevelMaximum,

              "amounts": [50, 8]
            }
          ]

      },
      {
        //Deals damage and apply Weakness
        "skillId": SKILL.SHADOWBOLT,
        "characterClass": CHARACTER_CLASS.WARLOCK,

        "iLevelMin": 1,
        "iLevelMax": iLevelMaximum,
        "rarities": [RARITY.COMMON, RARITY.UNCOMMON, RARITY.RARE, RARITY.EPIC],  //KDYZ SEM TAM DAL TY ENUMY TAK ERROR V KONZOLI
        "equipSlots": ["BODY", "HEAD", "LEGS", "FINGER_1"],
        //0-damage
        "ranks":
          [
            {
              "manaCost": 15,
              "iLevelMin": 0,
              "iLevelMax": 10,
              //0-damage
              //1- chance to apply Weakness
              "amounts": [30, 0.25],
            },
            {
              "manaCost": 20,
              "iLevelMin": 10,
              "iLevelMax": iLevelMaximum,
              "amounts": [45, 0.25],
            }
          ],
        "buff": {
          "buffId": BUFF.FRAGILITY_BUF,
          "ranks":
            [
              {
                "iLevelMin": 0,
                "iLevelMax": 10,
                // 0 -all damage recieved increased by X%
                "amounts": [0.05],
                "durationTurns": 2,
              },
              {
                "iLevelMin": 10,
                "iLevelMax": iLevelMaximum,
                "amounts": [0.1],
                "durationTurns": 3,
              }
            ]
        }
      },
      {

        "skillId": SKILL.SHIELD_WALL,
        "characterClass": CHARACTER_CLASS.WARRIOR,
        "iLevelMin": 1,
        "iLevelMax": iLevelMaximum,
        "rarities": [RARITY.COMMON, RARITY.UNCOMMON, RARITY.RARE, RARITY.EPIC],  //KDYZ SEM TAM DAL TY ENUMY TAK ERROR V KONZOLI
        "equipSlots": ["BODY", "HEAD", "LEGS", "FINGER_1"],

        "ranks":
          [
            {
              "manaCost": 12,
              "iLevelMin": 0,
              "iLevelMax": iLevelMaximum,
              "amounts": [],
            }
          ],
        "buff": {
          "buffId": BUFF.SHIELD_WALL_BUFF,
          "ranks":
            [
              {
                "iLevelMin": 0,
                "iLevelMax": 10,
                // 0 -all incoming damage reduced by %X
                "amounts": [0.5],
                "durationTurns": 2,
              },
              {
                "iLevelMin": 10,
                "iLevelMax": iLevelMaximum,
                "amounts": [0.75],
                "durationTurns": 3,
              }
            ]
        }

      },
      {

        "skillId": SKILL.CURSE_OF_WEAKNESS,
        "characterClass": CHARACTER_CLASS.WARLOCK,
        "iLevelMin": 1,
        "iLevelMax": iLevelMaximum,
        "rarities": [RARITY.COMMON, RARITY.UNCOMMON, RARITY.RARE, RARITY.EPIC],  //KDYZ SEM TAM DAL TY ENUMY TAK ERROR V KONZOLI
        "equipSlots": ["BODY", "HEAD", "LEGS", "FINGER_1"],

        "ranks":
          [
            {
              "manaCost": 20,
              "iLevelMin": 0,
              "iLevelMax": iLevelMaximum,
              "amounts": [],
            }
          ],
        "buff": {
          "buffId": BUFF.WEAKNESS_BUFF,

          "ranks":
            [
              {
                "iLevelMin": 0,
                "iLevelMax": 10,
                // 0 -all damage dealt lowered by X%
                "amounts": [0.1],
                "durationTurns": 2,
              },
              {
                "iLevelMin": 10,
                "iLevelMax": iLevelMaximum,
                "amounts": [0.1],
                "durationTurns": 3,
              }
            ]
        }

      },
      {

        "skillId": SKILL.REJUVENATION,
        "characterClass": CHARACTER_CLASS.SHAMAN,
        "iLevelMin": 1,
        "iLevelMax": iLevelMaximum,
        "rarities": [RARITY.COMMON, RARITY.UNCOMMON, RARITY.RARE, RARITY.EPIC], //KDYZ SEM TAM DAL TY ENUMY TAK ERROR V KONZOLI
        "equipSlots": ["BODY", "HEAD", "LEGS", "FINGER_1"],

        "ranks":
          [
            {
              "manaCost": 20,
              "iLevelMin": 0,
              "iLevelMax": iLevelMaximum,
              "amounts": [],
            }
          ],
        "buff": {
          "buffId": BUFF.REJUVENATION_BUFF,
          "ranks":
            [
              {
                "iLevelMin": 0,
                "iLevelMax": 10,
                // 0 -heal amount
                "amounts": [35],
                "durationTurns": 5,
              },
              {
                "iLevelMin": 10,
                "iLevelMax": iLevelMaximum,
                "amounts": [50],
                "durationTurns": 5,
              }
            ]
        }

      },

    ]
}


export function addSkillToEquip(_equip: Equip, _characterClass: string) {
  //TODO: vyfiltrovat possible skilly podle equipu


  let choosenSkill = SKILL_DEFINITIONS.items[0];  //SLAM will be default skill when we reach more than 50 tries to get character specific skil.....
  //musime vybrat skill pro danou classu
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

export function convertSkillToGiveniLevel(_skillId: string, _iLevel: number): Skill {

  let skill: Skill = new Skill("", "", 0, [], 0, undefined);
  for (const skillDef of SKILL_DEFINITIONS.items) {

    if (skillDef.skillId == _skillId) {
      skill.characterClass = skillDef.characterClass;
      skill.skillId = skillDef.skillId;
      // skill.imageId = skillDef.imageId;

      //vyberu rank iLevelu equipu
      let choosenRank = -1;
      for (let i = 0; i < skillDef.ranks.length; i++) {
        if (skillDef.ranks[i].iLevelMax >= _iLevel && skillDef.ranks[i].iLevelMin <= _iLevel) {
          choosenRank = i;
          break;
        }
      }
      if (choosenRank == -1)
        throw ("Cannot find valid rank for " + _skillId + " at iLevel :" + _iLevel.toString());

      skill.rank = choosenRank + 1;
      skill.manaCost = skillDef.ranks[choosenRank].manaCost;
      skill.amounts = skillDef.ranks[choosenRank].amounts;

      if (skillDef.buff != undefined) {
        //vyberu rank Buffu podle ranku skillu(iLevelu equipu)
        let choosenRank = -1;
        for (let i = 0; i < skillDef.buff.ranks.length; i++) {
          if (skillDef.buff.ranks[i].iLevelMax >= _iLevel && skillDef.buff.ranks[i].iLevelMin <= _iLevel) {
            choosenRank = i;
            break;
          }
        }
        if (choosenRank == -1)
          throw ("Cannot find valid rank for Buff of " + _skillId + " at iLevel :" + _iLevel.toString());

        skill.buff = new Buff(skillDef.buff.buffId, skillDef.buff.ranks[choosenRank].durationTurns, skillDef.buff.ranks[choosenRank].amounts, choosenRank + 1);
      }


      break;
    }

  }
  return skill;

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