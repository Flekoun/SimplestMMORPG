
// [START import]

import * as functions from "firebase-functions";

import { CHARACTER_CLASS, CharacterDocument, characterDocumentConverter, compareWorldPosition, ContentContainer, DECK_SHUFFLE_FATIGUE_PENALTY, FLEE_FATIGUE_PENALTY, generateContentContainer, getCurrentDateTime, getCurrentDateTimeVersionSecondsAdd, getExpAmountFromEncounterForGivenCharacterLevel, getMillisPassedSinceTimestamp, INSTAKILL, millisToSeconds, PlayerData, QuerryForParty, QuerryHasCharacterAnyUnclaimedEncounterResult, QuerryIfCharacterIsInCombatAtAnyEncounter, QuerryIfCharacterIsWatcherInAnyEncounterOnHisPosition, randomIntFromInterval, rollForRandomItems, SimpleTally, UpdateCharacterLocationInHisParty, validateCallerBulletProof, WorldPosition } from ".";
import { EncounterResult, EncounterResultContentLoot, EncounterResultEnemy, EncounterResultCombatant, RESULT_ITEM_WANT_DURATION_SECONDS, DungeonData } from "./encounterResult";
import { BuffBonusEffect, DropTable, generateContent, generateDropFromDropTable, generateEquip, IHasChanceToSpawn, QuerryForSkillDefinitions, RARITY, shuffleArray, SkillBonusEffect } from "./equip";
import { applySkillEffect, drawNewSkills, firestoreAutoId } from "./general2";

import { Party } from "./party";
import { PerkOfferDefinition } from "./perks";
import { BUFF, Combatskill, getRandomCurseAsCombatSkill, SkillDefinitions, SKILL_GROUP, Buff, BUFF_GROUP } from "./skills";
import { LocationConverter, MapLocation, PointOfInterest, PointOfInterestServerDataDefinitions, PointOfInterestServerDataDefinitionsConverter } from "./worldMap";
import { BLESS } from "./specials";
import { InboxItem } from "./utils";


const admin = require('firebase-admin');

//[END import]

export const TURN_DURATION_SECONDS = 45;

export enum ENCOUNTER_CONTEXT {
  PERSONAL = "PERSONAL",
  DUNGEON = "DUNGEON",
  WORLD_BOSS = "WORLD_BOSS",
}

export enum PERK_SPECIAL_EFFECT {
  ENEMY_ALL_ADD_HEALTH = "ENEMY_ALL_ADD_HEALTH",
  ENEMY_RANDOM_ADD_HEALTH = "ENEMY_RANDOM_ADD_HEALTH",
  INCREASE_MANA_OF_ALL_SKILLS = "INCREASE_MANA_OF_ALL_SKILLS",
  DUPLICATE_ENEMY = "DUPLICATE_ENEMY"
}


export enum LOOT_SPOT {
  ORE_DEPOSIT = "ORE_DEPOSIT",
  DARK_LEGION_MINE = "DARK_LEGION_MINE",
  PLAINS_MEADOW = "PLAINS_MEADOW",
  PLAINS_MEADOW_RARE = "PLAINS_MEADOW_RARE",
}

export enum MONSTER_SKILLS {
  ATTACK_NORMAL = "ATTACK_NORMAL",
  BUFF_STRENGTH = "BUFF_STRENGTH",
  BLOCK_NORMAL = "BLOCK_NORMAL",
  INCREASE_STATS_STRENGTH = "INCREASE_STATS_STRENGTH",
  //MONSTER_ALLY_STRENGTH_BUFF_PERMANENT = "MONSTER_ALLY_STRENGTH_BUFF_PERMANENT",
  HEAL_BASIC = "HEAL_BASIC",
  REND = "REND"
}

export enum MONSTER_SKILL_TYPES {
  SKILL_TYPE_ATTACK_NORMAL = "SKILL_TYPE_ATTACK_NORMAL",
  SKILL_TYPE_BUFF = " SKILL_TYPE_BUFF",
  SKILL_TYPE_INCREASE_STATS = "SKILL_TYPE_INCREASE_STATS",
  SKILL_TYPE_BLOCK = "SKILL_TYPE_BLOCK",
  SKILL_TYPE_HEAL = "SKILL_TYPE_HEAL",
  SKILL_TYPE_ATTACK_MASS = "SKILL_TYPE_ATTACK_MASS",
  SKILL_TYPE_ATTACK_AND_DEBUFF = "SKILL_TYPE_ATTACK_AND_DEBUFF",
}


export function createCombatEnemyFromDefinition(_enemyId: string, _enemyDefinitions: EnemyDefinitions): CombatEnemy {

  console.log("_enemyId: " + _enemyId);
  const enemyStats = _enemyDefinitions.getEnemyById(_enemyId);
  const enemyHealth = randomIntFromInterval(enemyStats.healthMin, enemyStats.healthMax);
  const newEnemy = new CombatEnemy(firestoreAutoId(), _enemyId, new CombatStats(0, 0, enemyHealth, enemyHealth, enemyHealth, 0, 0, 0, 5, 0, 0, 0, 0, [], 0, [], 0), enemyStats.level, enemyStats.mLevel, enemyStats.moveSet, enemyStats.isRare, "", [], [], 0, new EnemyDefinitionMoveSetSkill([], 0, "", "", false, false, false, false, false, false), 0, enemyStats.monsterEssences);
  newEnemy.nextSkill = newEnemy.chooseSkillToCast(0);
  return newEnemy;
}

export class CombatEntity {
  constructor(
    public uid: string,
    // public enemyId: string,
    public displayName: string,
    public buffs: CombatBuff[],
    public stats: CombatStats,
    public level: number,
    public blockAmount: number


  ) { }

  public addBlock(_amount: number, _encounter: EncounterDocument) {
    this.blockAmount += _amount;

    if (this.hasBuff(BUFF_GROUP.CRUSADER_BUFF)) {
      let buff = this.getBuff(BUFF_GROUP.CRUSADER_BUFF);
      let enemy = _encounter.getRandomEnemy("");
      if (enemy != null && buff)
        _encounter.dealDamageToCombatEntity(this, enemy, buff.amounts[0], BUFF_GROUP.CRUSADER_BUFF, _encounter, false);
    }
  }

  public hasBuff(_buffGroupId: string): boolean {

    return this.buffs.find(buff => buff.buffGroupId == _buffGroupId) != undefined;
    // for (const buff of this.buffs) {
    //   if (buff.buffGroupId == _buffGroupId)
    //     return true;
    // }
    // return false;
  }

  public getBuff(_buffGroupId: string): CombatBuff | undefined {

    return this.buffs.find(buff => buff.buffGroupId == _buffGroupId);

  }


  public removeBuff(_buff: CombatBuff) {
    this.buffs.splice(this.buffs.indexOf(_buff), 1);
  }



  public addBuff(_buffToAdd: CombatBuff, _caster: CombatEntity) {

    _buffToAdd.creatorUid = _caster.uid;
    let buffReplaced = false;

    for (let i = 0; i < this.buffs.length; i++) {

      if (this.buffs[i].buffId == _buffToAdd.buffId) {  //uz mam tento buff na sobe

        this.buffs[i].applyEffectBeforeExpiration(this);  //imo toto co tu delam mozna neni dobre, obe muzu ignorovat? nebudou se trigrovat zapnuti/vypnuti bufu kdyz replacuju za ten samy?
        this.buffs.splice(i, 1, _buffToAdd);
        _buffToAdd.applyEffecWhenBuffApplied(this); //imo toto co tu delam mozna neni dobre, obe muzu ignorovat? nebudou se trigrovat zapnuti/vypnuti bufu kdyz replacuju za ten samy?

        buffReplaced = true;
        break;

      }
    }

    if (!buffReplaced) {

      this.buffs.push(_buffToAdd);
      _buffToAdd.applyEffecWhenBuffApplied(this);
    }

  }

  public lowerTurnsLeftOnMyBuffs(_encounter: EncounterDocument) {
    for (var i = this.buffs.length - 1; i >= 0; i--) {
      if (this.buffs[i] != null)
        this.buffs[i].applyEffectWhenTurnEnds(this, _encounter);

      //zavolam jen efekt ze buff expirnul...ale jeste ho neodstranuju...to prijde na radu az uplne na zacataku noveho kola...buff stale muze udelat efekt na zacatku noveho kola...
      if (this.buffs[i] != null) {
        if (this.buffs[i].lowerTurnsLeftAndCheckIfHasExpired()) {
          this.buffs[i].applyEffectBeforeExpiration(this);
          //  this.buffs.splice(i, 1);
        }
      }
    }
  }

  public applyEffectWhenNewRoundStartsOnMyBuffs(_encounter: EncounterDocument) {
    for (var i = this.buffs.length - 1; i >= 0; i--) {
      if (this.buffs[i] != null)
        this.buffs[i].applyEffectWhenNewRoundStartsOnMyBuffs(this, _encounter);

      //az tady odstranim buff uplne...na zacatku noveho kola po tom co aplikovat svuj efekt na zacatku kola a expire efekt pokud expirnul
      if (this.buffs[i] != null) {
        if (this.buffs[i].checkIfHasExpired()) {
          this.buffs.splice(i, 1);
        }
      }
    }
  }

  public giveHealth(_amount: number): number {

    if (this.stats.health + _amount > this.stats.healthMax) {
      _amount = this.stats.healthMax - this.stats.health;
      //  this.stats.health = this.stats.healthMax;
    }

    this.stats.health += _amount;

    return _amount;
  }

  public takeHealth(_amount: number, _source: CombatEntity, _encounter: EncounterDocument): number {

    this.buffs.forEach(buff => { _amount = buff.applyEffectBeforeOwnerLosesHealth(this, _amount, _source, _encounter) });

    if (this.stats.health - _amount < 0) _amount = this.stats.health;
    this.stats.health -= _amount;

    if (this.stats.health < this.stats.leastHealth)
      this.stats.leastHealth = this.stats.health;
    return _amount;
  }
}

export class CombatLog {

  constructor(
    //  public entries : CombatEntry[]
    public entries: string[]
  ) { }

}


export class EnemyDropTablesData {

  constructor(
    public enemyDropTables: EnemyDropTable[],
  ) { }

}


export class EnemyDropTable {

  constructor(
    public dropTables: DropTable[],
    public enemyId: string
  ) { }

}



export class EnemyDefinitions {

  constructor(
    public enemies: EnemyDefinition[]
  ) { }

  getEnemyById(_id: string): EnemyDefinition {

    for (const element of this.enemies) {
      if (element.enemyId == _id)
        return element;
    }

    throw "Could not find Definition Stat for Enemy Id : " + _id;

  }
}


export const EnemyDefinitionsConverter = {
  toFirestore: (_entry: EnemyDefinitions) => {
    return {
      enemies: _entry.enemies,
    };
  },
  fromFirestore: (snapshot: any, options: any) => {
    const data = snapshot.data(options);
    return new EnemyDefinitions(data.enemies);
  }
};






// export class RewardRecurring {
//   constructor(
//     public recurrenceInGameDays: number,
//     public content: ContentContainer,
//   ) { }

// }

export class TierDungeonDefinition {
  constructor(
    public enemies: string[],
  ) { }


}

export class TierMonstersDefinition {
  constructor(

    //public id: string,
    public entryTimePrice: number,
    public enemies: string[],
    public perkOffers: PerkOfferDefinition[],
  ) { }

  // increaseStockClaimedForPerk(_rarePerkUid: string): boolean {

  //   let foundElement = this.perkOffers.find(perkOffer => perkOffer.uid === _rarePerkUid);

  //   if (foundElement) {
  //     foundElement.stockClaimed++;
  //     return true;
  //   }
  //   return false;


  // }

}


export class RareEnemyTierDefinition implements IHasChanceToSpawn {
  constructor(

    public enemies: string[],
    public chanceToSpawn: number,
  ) { }
}





export class EnemyDefinition {

  constructor(
    public enemyId: string,
    public healthMax: number,
    public healthMin: number,
    public level: number,
    public mLevel: number,
    public isRare: boolean,
    public moveSet: EnemyDefinitionMoveSet[],
    public monsterEssences: number
  ) { }



}

export class EnemyDefinitionMoveSet {

  constructor(
    public skills: EnemyDefinitionMoveSetSkill[],
    public turnMax: number,
    public turnMin: number,
  ) { }
}

export class EnemyDefinitionMoveSetSkill {

  constructor(
    public amounts: number[],
    public changeToCast: number,
    public id: string,
    public typeId: string,
    public canChoooseAsTarget_AnyAlly: boolean,
    public canChooseAsTarget_Self: boolean,
    public canChooseAsTarget_Target: boolean,
    public canChooseAsTarget_AnyHero: boolean,
    public chooseAllHeroesAsTarget: boolean,
    public chooseAllEnemiesAsTarget: boolean,
    public repeatedCastLimit?: number,

  ) { }

}



export class CombatEnemy extends CombatEntity {
  constructor(
    public uid: string,
    public enemyId: string,
    public stats: CombatStats,
    public level: number,
    public mLevel: number,
    public moveSet: EnemyDefinitionMoveSet[],

    public isRare: boolean,
    public targetUid: string | null,
    public threatMetter: SimpleTally[],
    public buffs: CombatBuff[],
    public blockAmount: number,
    public nextSkill: EnemyDefinitionMoveSetSkill,
    public repeatedCastCount: number,
    public monsterEssences: number

  ) { super(uid, enemyId, buffs, stats, level, blockAmount) }

  public lowerTurnsLeftOnMyBuffs(_encounter: EncounterDocument) {
    super.lowerTurnsLeftOnMyBuffs(_encounter);
  }

  public applyEffectWhenNewRoundStartsOnMyBuffs(_encounter: EncounterDocument) {
    super.applyEffectWhenNewRoundStartsOnMyBuffs(_encounter);
  }

  public giveHealth(_amount: number) {
    return super.giveHealth(_amount);

  }




  public takeHealth(_amount: number, _source: CombatEntity, _encounter: EncounterDocument): number {
    return super.takeHealth(_amount, _source, _encounter);

  }

  public addBuff(_buffToAdd: CombatBuff, _caster: CombatEntity) {
    super.addBuff(_buffToAdd, _caster);
  }


  public hasBuff(_buffGroupId: string): boolean {
    return super.hasBuff(_buffGroupId);
  }


  getRandomSkill(skills: EnemyDefinitionMoveSetSkill[]): EnemyDefinitionMoveSetSkill | null {
    let sum = skills.reduce((a, b) => a + b.changeToCast, 0);
    let randomNum = Math.random() * sum;
    let total = 0;
    for (let i = 0; i < skills.length; i++) {
      total += skills[i].changeToCast;
      if (randomNum <= total) {
        return skills[i];
      }
    }

    return null;
  }


  chooseSkillToCast(turnNumber: number): EnemyDefinitionMoveSetSkill {

    //prvni zvednu pocet castu, abyc algo uz vedel ze to co chce vybrat bych kouzlil po X++ te
    this.repeatedCastCount++;

    //vyberu move z movesetu ktery odpovida danemu cilu kola
    let choosenMove: EnemyDefinitionMoveSet | undefined = undefined;
    for (const move of this.moveSet) {
      if (move.turnMax == -1) move.turnMax = 1000000; //-1 reprezentuje v DB jakoby "az do konce boje", tedy nekonecny turn number
      // console.log("move.turnMax :" + move.turnMax);
      // console.log("move.turnMin :" + move.turnMin);
      // console.log("turnNumber :" + turnNumber);
      if (turnNumber >= move.turnMin && turnNumber <= move.turnMax) {
        //  console.log("valid ");
        choosenMove = move;
        break;
      }
    }


    if (choosenMove == undefined)
      throw ("Cant find move for turn : " + turnNumber + " for enemy : " + this.enemyId);


    //vyfiltruju skilly ktere maji limit na to kolikrat muzou byt zakouzleny za sebou
    let validSkills: EnemyDefinitionMoveSetSkill[] = [];
    choosenMove.skills.forEach(skill => {
      let validSkill = true;
      //pokud ma skill definovany max repetice
      if (skill.repeatedCastLimit != undefined)
        //presahl pocet repetic
        if (this.repeatedCastCount >= skill.repeatedCastLimit)
          //a je to ten samy skill co sem kouzlil minule = tento skill presahl repetice , vyhodim ho z moznych skillu na vyber
          if (this.nextSkill.id == skill.id && this.nextSkill.changeToCast == skill.changeToCast && this.nextSkill.amounts[0] == skill.amounts[0])  // TODO : takhle idiotsky se snazim zjistit jestli se jedna o identicky skill....pridej tam asi nejake UID nebo nevim, pak jeste podle indexu v arrayi by to slo?
            validSkill = false;

      if (validSkill)
        validSkills.push(new EnemyDefinitionMoveSetSkill(skill.amounts, skill.changeToCast, skill.id, skill.typeId, skill.canChoooseAsTarget_AnyAlly, skill.canChooseAsTarget_Self, skill.canChooseAsTarget_Target, skill.canChooseAsTarget_AnyHero, skill.chooseAllHeroesAsTarget, skill.chooseAllEnemiesAsTarget, skill.repeatedCastLimit));
    });
    validSkills.forEach(element => {
      console.log("validSkill: " + element.id);

    });

    //vyberu nahodny skill
    let choosenSkill = this.getRandomSkill(validSkills);

    if (choosenSkill != null) {

      if (this.nextSkill.id == choosenSkill.id && this.nextSkill.changeToCast == choosenSkill.changeToCast && this.nextSkill.amounts[0] == choosenSkill.amounts[0])  // TODO : takhle idiotsky se snazim zjistit jestli se jedna o identicky skill....pridej tam asi nejake UID nebo nevim, pak jeste podle indexu v arrayi by to slo?
        console.log("je to stejny skill co minule, nenuluje");
      else
        this.repeatedCastCount = 0;


      return choosenSkill;
    }
    else
      throw ("Cant find any skill for turn : " + turnNumber + " for enemy : " + this.enemyId);


  }

  //jen metoda ktera zcheckuje jestli je nul, nemam pak tolik balastu v te dlouhe metode vyberu targetu
  addAsTarget(_target: CombatEntity | null, _whereToAdd: CombatEntity[]) {
    if (_target != null)
      if (_target.stats.health > 0)
        _whereToAdd.push(_target);
  }

  applySkillAndChooseNextOne(_encounter: EncounterDocument) {

    if (this.stats.health <= 0)
      return;

    if (this.targetUid == null)
      return;

    let agroTarget: CombatMember | null = _encounter.getCombatMemberByUid(this.targetUid);
    if (agroTarget != null)
      if (agroTarget.stats.health <= 0)
        agroTarget = null;

    let agroTargetUid = ""; if (agroTarget != null) agroTargetUid = agroTarget.uid;

    console.log("turn number: " + _encounter.turnNumber);

    //vybereme target podle pozadavku skillu
    let targets: CombatEntity[] = [];


    if (this.nextSkill.chooseAllEnemiesAsTarget || this.nextSkill.chooseAllHeroesAsTarget) {

      if (this.nextSkill.chooseAllEnemiesAsTarget && this.nextSkill.canChooseAsTarget_Self) {
        _encounter.enemies.forEach(enemy => {
          this.addAsTarget(enemy, targets);
        });
      }
      else if (this.nextSkill.chooseAllEnemiesAsTarget && !this.nextSkill.canChooseAsTarget_Self) {
        _encounter.enemies.forEach(enemy => {
          if (enemy.uid != this.uid)
            this.addAsTarget(enemy, targets);
        });
      }
      else if (this.nextSkill.chooseAllHeroesAsTarget && this.nextSkill.canChooseAsTarget_Target) {
        _encounter.combatants.forEach(hero => {
          this.addAsTarget(hero, targets);
        });
      }
      else if (this.nextSkill.chooseAllHeroesAsTarget && !this.nextSkill.canChooseAsTarget_Target) {
        _encounter.combatants.forEach(hero => {
          if (hero.uid != agroTargetUid)
            this.addAsTarget(hero, targets);
        });
      }
    }
    else {

      //asi by to slo udelat lip kdybych prvni podle tech flagu naplnil/odstranil potencionalni targety z nejakeho pole a pak nahodne vybral jednoho?
      if (this.nextSkill.canChooseAsTarget_Target && this.nextSkill.canChooseAsTarget_AnyHero && this.nextSkill.canChoooseAsTarget_AnyAlly && this.nextSkill.canChooseAsTarget_Self) {
        if (randomIntFromInterval(0, 1) == 0)
          this.addAsTarget(_encounter.getRandomHero(""), targets);
        else
          this.addAsTarget(_encounter.getRandomEnemy(""), targets);
      }
      else if (this.nextSkill.canChooseAsTarget_Target && this.nextSkill.canChooseAsTarget_AnyHero && this.nextSkill.canChoooseAsTarget_AnyAlly && !this.nextSkill.canChooseAsTarget_Self) {
        if (randomIntFromInterval(0, 1) == 0)
          this.addAsTarget(_encounter.getRandomHero(""), targets);
        else
          this.addAsTarget(_encounter.getRandomEnemy(this.uid), targets);
      }
      else if (this.nextSkill.canChooseAsTarget_Target && this.nextSkill.canChooseAsTarget_AnyHero && !this.nextSkill.canChoooseAsTarget_AnyAlly && this.nextSkill.canChooseAsTarget_Self) {
        this.addAsTarget(_encounter.getRandomHero(""), targets);

        if (randomIntFromInterval(0, _encounter.combatants) == 0)
          this.addAsTarget(this, targets);
      }
      else if (this.nextSkill.canChooseAsTarget_Target && this.nextSkill.canChooseAsTarget_AnyHero && !this.nextSkill.canChoooseAsTarget_AnyAlly && !this.nextSkill.canChooseAsTarget_Self) {
        this.addAsTarget(_encounter.getRandomHero(this.uid), targets);
      }
      else if (this.nextSkill.canChooseAsTarget_Target && !this.nextSkill.canChooseAsTarget_AnyHero && this.nextSkill.canChoooseAsTarget_AnyAlly && this.nextSkill.canChooseAsTarget_Self) {
        this.addAsTarget(_encounter.getRandomEnemy(""), targets);
        if (randomIntFromInterval(0, _encounter.enemies) == 0)
          this.addAsTarget(agroTarget, targets);
      }
      else if (this.nextSkill.canChooseAsTarget_Target && !this.nextSkill.canChooseAsTarget_AnyHero && this.nextSkill.canChoooseAsTarget_AnyAlly && !this.nextSkill.canChooseAsTarget_Self) {
        this.addAsTarget(_encounter.getRandomEnemy(this.uid), targets);
        if (randomIntFromInterval(0, _encounter.enemies) == 0)
          this.addAsTarget(agroTarget, targets);
      }
      else if (this.nextSkill.canChooseAsTarget_Target && !this.nextSkill.canChooseAsTarget_AnyHero && !this.nextSkill.canChoooseAsTarget_AnyAlly && this.nextSkill.canChooseAsTarget_Self) {
        if (randomIntFromInterval(0, 1) == 0)
          this.addAsTarget(agroTarget, targets);
        else
          this.addAsTarget(this, targets);
      }
      else if (this.nextSkill.canChooseAsTarget_Target && !this.nextSkill.canChooseAsTarget_AnyHero && !this.nextSkill.canChoooseAsTarget_AnyAlly && !this.nextSkill.canChooseAsTarget_Self) {
        this.addAsTarget(agroTarget, targets);
      }
      else if (!this.nextSkill.canChooseAsTarget_Target && this.nextSkill.canChooseAsTarget_AnyHero && this.nextSkill.canChoooseAsTarget_AnyAlly && this.nextSkill.canChooseAsTarget_Self) {

        if (randomIntFromInterval(0, 1) == 0) {
          this.addAsTarget(_encounter.getRandomHero(agroTargetUid), targets);
        }
        else
          this.addAsTarget(_encounter.getRandomEnemy(""), targets);
      }
      else if (!this.nextSkill.canChooseAsTarget_Target && this.nextSkill.canChooseAsTarget_AnyHero && this.nextSkill.canChoooseAsTarget_AnyAlly && !this.nextSkill.canChooseAsTarget_Self) {

        if (randomIntFromInterval(0, 1) == 0)
          this.addAsTarget(_encounter.getRandomHero(agroTargetUid), targets);
        else
          this.addAsTarget(_encounter.getRandomEnemy(this.uid), targets);
      }
      else if (!this.nextSkill.canChooseAsTarget_Target && this.nextSkill.canChooseAsTarget_AnyHero && !this.nextSkill.canChoooseAsTarget_AnyAlly && this.nextSkill.canChooseAsTarget_Self) {
        this.addAsTarget(_encounter.getRandomHero(agroTargetUid), targets);

        if (randomIntFromInterval(0, _encounter.combatants) == 0)
          this.addAsTarget(this, targets);
      }
      else if (!this.nextSkill.canChooseAsTarget_Target && this.nextSkill.canChooseAsTarget_AnyHero && !this.nextSkill.canChoooseAsTarget_AnyAlly && !this.nextSkill.canChooseAsTarget_Self) {
        this.addAsTarget(_encounter.getRandomHero(agroTargetUid), targets);
      }
      else if (!this.nextSkill.canChooseAsTarget_Target && !this.nextSkill.canChooseAsTarget_AnyHero && this.nextSkill.canChoooseAsTarget_AnyAlly && this.nextSkill.canChooseAsTarget_Self) {
        this.addAsTarget(_encounter.getRandomEnemy(""), targets);
      }
      else if (!this.nextSkill.canChooseAsTarget_Target && !this.nextSkill.canChooseAsTarget_AnyHero && this.nextSkill.canChoooseAsTarget_AnyAlly && !this.nextSkill.canChooseAsTarget_Self) {
        this.addAsTarget(_encounter.getRandomEnemy(this.uid), targets);
      }
      else if (!this.nextSkill.canChooseAsTarget_Target && !this.nextSkill.canChooseAsTarget_AnyHero && !this.nextSkill.canChoooseAsTarget_AnyAlly && this.nextSkill.canChooseAsTarget_Self) {
        this.addAsTarget(this, targets);
      }
      else if (!this.nextSkill.canChooseAsTarget_Target && !this.nextSkill.canChooseAsTarget_AnyHero && !this.nextSkill.canChoooseAsTarget_AnyAlly && !this.nextSkill.canChooseAsTarget_Self) {
        // targetNew = null;
      }

    }


    if (targets.length == 0)
      return;

    switch (this.nextSkill.id) {
      case MONSTER_SKILLS.ATTACK_NORMAL:
        {
          let myDamage = this.nextSkill.amounts[0];
          targets.forEach(target => {
            _encounter.dealDamageToCombatEntity(this, target, myDamage, this.nextSkill.id, _encounter);
          });
          //  _encounter.addEntryToCombatLog("<b>{" + this.displayName + "}</b>" + " attacked all Heroes");
          break;

        }

      case MONSTER_SKILLS.INCREASE_STATS_STRENGTH:
        {
          targets.forEach(target => {
            target.stats.damagePowerTotal += this.nextSkill.amounts[0];
            _encounter.addEntryToCombatLog("<b>{" + target.displayName + "}</b>" + " gained " + "<b>" + this.nextSkill.amounts[0] + " strength </b>");
            _encounter.combatFlow.push(new CombatFlowEntry(this.uid, target.uid, this.nextSkill.id, this.nextSkill.amounts[0], true, true));
          });
          break;

        }

      case MONSTER_SKILLS.BLOCK_NORMAL:
        {
          targets.forEach(target => {
            target.blockAmount += this.nextSkill.amounts[0];
            _encounter.addEntryToCombatLog("<b>{" + target.displayName + "}</b>" + " gained " + "<b>" + this.nextSkill.amounts[0] + " block </b>");
            _encounter.combatFlow.push(new CombatFlowEntry(this.uid, target.uid, this.nextSkill.id, this.nextSkill.amounts[0], true, true));
          });

          break;
        }
      case MONSTER_SKILLS.BUFF_STRENGTH:

        targets.forEach(target => {
          target.addBuff(new CombatBuff(BUFF.STRENGTH_BUFF, 10, 10, this.nextSkill.amounts, "", this.uid), this);
          _encounter.addEntryToCombatLog("<b>{" + target.displayName + "}</b>" + " gained " + "<b>{" + BUFF.STRENGTH_BUFF + "}</b>");
          _encounter.combatFlow.push(new CombatFlowEntry(this.uid, target.uid, this.nextSkill.id, this.nextSkill.amounts[0], true, true));
        });
        break;


      case MONSTER_SKILLS.HEAL_BASIC:

        targets.forEach(target => {
          let myHealAmount = this.nextSkill.amounts[0];
          _encounter.addEntryToCombatLog("<b>{" + this.displayName + "}</b>" + " healed " + "<b>{" + target.displayName + "}</b>");
          _encounter.giveHealthToCombatEntity(this, target, myHealAmount, this.nextSkill.id);
        });
        break;
      case MONSTER_SKILLS.REND:

        targets.forEach(target => {
          _encounter.dealDamageToCombatEntity(this, target, this.nextSkill.amounts[0], this.nextSkill.id, _encounter);
          target.addBuff(new CombatBuff(BUFF.BLEED_BUFF_1, 3, 3, [this.nextSkill.amounts[1]], BUFF_GROUP.BLEED_BUFF, this.uid), this);
        });

        break;

      default:
        break;
    }



    this.nextSkill = this.chooseSkillToCast(_encounter.turnNumber);


  }

  setThreatForCombatant(_combatantUid: string, _threatAmount: number) {

    let entryFound = false;
    this.threatMetter.forEach(threatEntry => {
      if (threatEntry.id == _combatantUid) {
        threatEntry.count = _threatAmount;
        entryFound = true;
      }
    });

    if (!entryFound)
      this.threatMetter.push(new SimpleTally(_combatantUid, _threatAmount));

    //TODO: Switch Targets - if some new guy has threat >110-130% of the guy who is target now.
    const target = this.getCombatantUidWithHighestThreat();
    if (target != null)
      this.targetUid = target.id;
    else
      this.targetUid = null;
  }


  addThreatForCombatant(_combatantUid: string, _threatAmount: number) {


    let entryFound = false;
    this.threatMetter.forEach(threatEntry => {
      if (threatEntry.id == _combatantUid) {
        threatEntry.count += _threatAmount;
        entryFound = true;
      }
    });

    if (!entryFound)
      this.threatMetter.push(new SimpleTally(_combatantUid, _threatAmount));

    //TODO: Switch Targets - if some new guy has threat >110-130% of the guy who is target now.
    const target = this.getCombatantUidWithHighestThreat();
    if (target != null)
      this.targetUid = target.id;
    else
      this.targetUid = null;

  }

  getCombatantUidWithHighestThreat(): SimpleTally | null {

    let guyWithHighestThreat: SimpleTally = new SimpleTally("", 0);
    let guyWithHigherThreatFound: boolean = false;
    this.threatMetter.forEach(threatEntry => {
      if (threatEntry.count > guyWithHighestThreat.count) {
        guyWithHigherThreatFound = true;
        guyWithHighestThreat = threatEntry;
      }
    });

    if (!guyWithHigherThreatFound)
      return null;
    else
      return guyWithHighestThreat;
  }



  //UNUSED,ZAKOMENTOVAT,SMAZAT!
  // generateDrop(_dropTables: DropTable[]): ContentContainer[] {

  //   let droppedItems: ContentContainer[] = [];

  //   //let ItemDrops: InventoryItemSimple[] = [];
  //   const LOOP_LOCK_PREVENTION_MAX_ROLLS = 100;
  //   let loopCount = 0;

  //   let rolledItem: IHasChanceToSpawn | null = null;

  //   let dropCount = 0;

  //   console.log("START-----Drop z " + this.displayName + " -----");

  //   //A rolluju dokud nenaplnim minimalny pocet vyzadovanych dropu, pokud je 0 , tak to zkusim jen jednou a konec nic nedroplo? Ok..
  //   loopCount = 0;
  //   dropCount = 0;

  //   // this.dropTable.forEach(dropTable => {
  //   _dropTables.forEach(dropTable => {

  //     dropCount = 0;
  //     loopCount = 0;
  //     do {

  //       if (loopCount < LOOP_LOCK_PREVENTION_MAX_ROLLS)
  //         loopCount++;
  //       else {

  //         console.log("LOOP LOCK PREVENTION TRIGGERED!! : generovani lootu for enemy :" + this.enemyId);
  //         //  throw "LOOP LOCK PREVENTION : generovani lootu pro encounter with random index : " + this.randomIndex + " for enemy :" + enemy.enemyId;
  //         break;

  //       }
  //       //zkusim jednou rolnout a vybrat nejaky item
  //       rolledItem = RollForRandomItem(dropTable.dropTableItems, true);

  //       //pokud to proslo a vybrany item se svou sanci na drop prosel, pridam si ho do dropu
  //       if (rolledItem != null) {

  //         dropCount++;
  //         console.log("rolled item je : " + (rolledItem as DropTableItem).itemId + " : type :" + (rolledItem as DropTableItem).contentType);

  //         if ((rolledItem as DropTableItem).contentType != undefined) {
  //           if ((rolledItem as DropTableItem).contentType == CONTENT_TYPE.EQUIP) {
  //             const equip = generateEquip(this.mLevel, (rolledItem as DropTableItem).rarity, EQUIP_SLOT_ID.ANY, CHARACTER_CLASS.ANY);
  //             droppedItems.push(generateContentContainer(equip));
  //           }
  //           else
  //             droppedItems.push(generateContentContainer(generateContent((rolledItem as DropTableItem).itemId, (rolledItem as DropTableItem).amount)));
  //         }
  //         else
  //           droppedItems.push(generateContentContainer(generateContent((rolledItem as DropTableItem).itemId, (rolledItem as DropTableItem).amount)));
  //       }


  //     } while (dropCount < dropTable.dropCountMin || loopCount < dropTable.dropCountMax); //aspon "Max-krát" musím loopnout kdyz je max drop 3 a min 0, tak 3x ten loop at probehne!

  //     console.log("KONEC-----loop probehlo tolikrát : " + loopCount);

  //   });
  //   console.log("droppedItems.lenght : " + droppedItems.length)
  //   return droppedItems;

  // }



}

export class CombatMember extends CombatEntity {

  constructor(
    public uid: string,
    public displayName: string,
    public characterUid: string,
    public characterClass: string,
    public skillsInHand: Combatskill[],//Skill[],
    public skillsDrawDeck: Combatskill[],
    public skillsDiscardDeck: Combatskill[],
    public stats: CombatStats,
    public damageDone: number,
    public hasRested: boolean,
    public level: number,
    public healingDone: number,
    public buffs: CombatBuff[],
    public characterPortrait: string,
    public skillsExhaustDeck: Combatskill[],
    public blockAmount: number,
    public deckShuffleCount: number,
    public potionsUsed: SimpleTally[],
    public blesses: string[],
    public hasAlreadyFinishedEncounterOfThisTier: boolean


  ) { super(uid, displayName, buffs, stats, level, blockAmount) }



  hasBless(_blessId: string): boolean {
    return (this.blesses.includes(_blessId));
  }

  getAllCursesInDeck(_setAlreadyUsedToFalse: boolean): Combatskill[] {

    let cursesIds: Combatskill[] = [];

    this.skillsInHand.forEach(skill => {
      if (skill.skillGroupId == SKILL_GROUP.CURSE)
        cursesIds.push(skill);
      if (_setAlreadyUsedToFalse)
        skill.alreadyUsed = false; //to delam aby proste byly ready na dalsi pouziti a neukladaly se nejake divne already used??
    });

    this.skillsDrawDeck.forEach(skill => {
      if (skill.skillGroupId == SKILL_GROUP.CURSE)
        cursesIds.push(skill);
      if (_setAlreadyUsedToFalse)
        skill.alreadyUsed = false; //to delam aby proste byly ready na dalsi pouziti a neukladaly se nejake divne already used??
    });

    this.skillsDiscardDeck.forEach(skill => {
      if (skill.skillGroupId == SKILL_GROUP.CURSE)
        cursesIds.push(skill);
      if (_setAlreadyUsedToFalse)
        skill.alreadyUsed = false; //to delam aby proste byly ready na dalsi pouziti a neukladaly se nejake divne already used??
    });

    // this.skillsExhaustDeck.forEach(skill => {
    //   if (skill.skillGroupId == SKILL_GROUP.CURSE)
    //     cursesIds.push(skill.skillId);
    // });

    return cursesIds;
  }

  addPotionUsed(_potionId: string) {
    for (const iterator of this.potionsUsed) {
      if (iterator.id == _potionId)
        throw "Cant drink this potion, you have already drank it in this combat";
    }
    this.potionsUsed.push(new SimpleTally(_potionId, 1))

  }


  hasAlreadyUsedPotion(_potionId: string): boolean {
    for (const iterator of this.potionsUsed) {
      if (iterator.id == _potionId)
        return true;
    }

    return false;
  }

  addBonusFromEquipToSkills() {
    this.skillsDrawDeck.forEach(skill => {

      for (const skillbonus of this.stats.skillBonusEffects) {
        skill.addSkillBonus(skillbonus);
      }

      for (const buffBonus of this.stats.buffBonusEffects) {
        skill.addBuffBonus(buffBonus);
      }

      skill.addDmgPower(this.stats.damagePowerTotal);
      skill.addDefense(this.stats.defenseTotal);
      skill.addHealingPower(this.stats.healingPowerTotal);
    });
  }


  turnEnded(_encounter: EncounterDocument) {
    for (const skillInHand of this.skillsInHand) {
      skillInHand.onTurnEnded(this, _encounter);
    }
  }

  public lowerTurnsLeftOnMyBuffs(_encounter: EncounterDocument) {
    super.lowerTurnsLeftOnMyBuffs(_encounter);

  }

  public giveHealth(_amount: number): number {
    return super.giveHealth(_amount);
  }

  public takeHealth(_amount: number, _source: CombatEntity, _encounter: EncounterDocument): number {
    return super.takeHealth(_amount, _source, _encounter);
  }

  public addBuff(_buffToAdd: CombatBuff, _caster: CombatEntity) {
    super.addBuff(_buffToAdd, _caster);
  }


  giveMana(_amount: number) {
    this.stats.mana += _amount;
    if (this.stats.mana > this.stats.manaMax)
      this.stats.mana = this.stats.manaMax;
  }

  regenMana(): number {
    let restoreAmount = this.stats.manaRegen;//this.stats.spirit;
    restoreAmount = Math.round(restoreAmount);
    this.stats.mana += restoreAmount;

    if (this.stats.mana > this.stats.manaMax) {
      restoreAmount -= this.stats.mana - this.stats.manaMax;
      this.stats.mana = this.stats.manaMax;

    }

    return restoreAmount;
  }

  regenHealth(_encounter: EncounterDocument): number {



    let restoreAmount = this.stats.hpRegenTotal//spirit / 10;
    restoreAmount = Math.round(restoreAmount);
    this.stats.health += restoreAmount;

    if (this.stats.health > this.stats.healthMax) {
      restoreAmount -= this.stats.health - this.stats.healthMax;
      this.stats.health = this.stats.healthMax;
    }

    return restoreAmount;
  }

}


export class CombatBuff {
  constructor(
    public buffId: string,
    public durationTurns: number,
    public turnsLeft: number,
    public amounts: number[],
    public buffGroupId: string,
    public creatorUid: string  // UID toho kdo buff vytvoril
  ) { }

  lowerTurnsLeftAndCheckIfHasExpired(): boolean {
    this.turnsLeft--;
    return (this.turnsLeft <= 0)
  }

  lowerTurnsLeft() {
    this.turnsLeft--;
  }


  checkIfHasExpired(): boolean {
    return (this.turnsLeft <= 0)
  }

  applyBeforeAnySkillCastedByOwner(_ownerOfThisBuff: CombatEntity, _encounter: EncounterDocument) {
    switch (this.buffId) {
      case BUFF.MYSTIC_SHIELD_BUFF_1:
        {
          _ownerOfThisBuff.removeBuff(this);
          break;
        }
      default:
        break;
    }
  }

  //nejake akce kdyz majitel buffu zemre
  applyEffecWhenOwnerDies(_ownerOfThisBuff: CombatEntity, _encounter: EncounterDocument) {

    switch (this.buffId) {
      case BUFF.CORRUPTION_BUFF_2:
      case BUFF.CORRUPTION_BUFF_3:
        {
          let caster = _encounter.getCombatEntityByUid(this.creatorUid);
          if (caster != null) {
            _encounter.giveHealthToCombatEntity(caster, caster, this.amounts[2], this.buffGroupId);
          }

          break;
        }
      default:
        break;
    }
  }

  //nejake akce tesne predtim nez buff zmizi...ale predtim nez hraci ztrati blocky, enemy uz blocky nemaji z minula budou teprve kouzlit
  applyEffectBeforeExpiration(_ownerOfThisBuff: CombatEntity,) {

    switch (this.buffId) {
      case BUFF.BLOOD_LUST_BUFF_1:
        _ownerOfThisBuff.stats.critChanceTotal -= this.amounts[0] * 100;
        break;

      default:
        break;
    }
  }

  //nejake akce hend pote co zacne nove kolo...tedy po tom co se restujou vsechny block vsem, hraci restujou atd...
  applyEffectWhenNewRoundStartsOnMyBuffs(_ownerOfThisBuff: CombatEntity, _encounter: EncounterDocument) {

    switch (this.buffGroupId) {

      case BUFF_GROUP.MYSTIC_SHIELD_BUFF:
        {
          _ownerOfThisBuff.addBlock(this.amounts[0], _encounter);
          break;
        }
      case BUFF_GROUP.DEFENSIVE_STANCE_BUFF:
        {
          _ownerOfThisBuff.addBlock(this.amounts[0], _encounter);
          break;
        }
      default:
        break;
    }
  }

  //nejake akce kdyz je buff pridan
  applyEffecWhenBuffApplied(_ownerOfThisBuff: CombatEntity,) {

    switch (this.buffId) {
      case BUFF.BLOOD_LUST_BUFF_1:
        _ownerOfThisBuff.stats.critChanceTotal += this.amounts[0] * 100;
        break;

      default:
        break;
    }
  }

  //toto se aplikuje vzdy ve chvili kdy mas dostat zraneni, tedy kdyz se ti ma snizit zivot z jakehokoliv duvodu
  applyEffectBeforeOwnerLosesHealth(_ownerOfThisBuff: CombatEntity, _damageAmount: number, _source: CombatEntity, _encounter: EncounterDocument): number {

    switch (this.buffId) {
      // case BUFF.SHIELD_WALL_BUFF_1:
      //   {
      //     console.log("_damageAmountA: " + _damageAmount);
      //     _damageAmount = Math.round(_damageAmount * (1 - this.amounts[0]));
      //     console.log("this.amounts[0]: " + this.amounts[0]);
      //     console.log("_damageAmountB: " + _damageAmount);
      //     break;
      //   }
      case BUFF.FRAGILITY_BUFF_1:
        {
          _damageAmount = _damageAmount + Math.round(_damageAmount * this.amounts[0]);
        }
        break;
      case BUFF.LIGHTNING_SHIELD_BUFF_2:
      case BUFF.LIGHTNING_SHIELD_BUFF_3:
        {
          if (_source != _ownerOfThisBuff) //nechci si davat dmg sam sobe
            _encounter.dealDamageToCombatEntity(_ownerOfThisBuff, _source, this.amounts[0], this.buffGroupId, _encounter);
        }
        break;
        // case BUFF.COUNTERSTRIKE_BUFF_1:
        //   {
        //     if (_source != _ownerOfThisBuff) //nechci si davat dmg sam sobe
        //     {
        //       _encounter.dealDamageToCombatEntity(_ownerOfThisBuff, _source, _damageAmount, this.buffGroupId, _encounter);
        //       _ownerOfThisBuff.removeBuff(this);
        //     }
        //   }
        break;

      default:
        break;
    }

    return _damageAmount;
  }

  //toto se aplikuje pred tim nez se aplikuje jakekoliv zraneni od combat entity...tedy pred tim nez se vypocita kolik si resistnul a vyblokoval blokem atd....
  applyEffectBeforeAnyDamageIsAppliedToOwner(_ownerOfThisBuff: CombatEntity, _damageAmount: number, _source: CombatEntity, _encounter: EncounterDocument): number {

    switch (this.buffId) {
      case BUFF.SHIELD_WALL_BUFF_1:
        {
          // console.log("_damageAmountA: " + _damageAmount);
          _damageAmount = Math.round(_damageAmount * (1 - this.amounts[0]));
          // console.log("this.amounts[0]: " + this.amounts[0]);
          // console.log("_damageAmountB: " + _damageAmount);
          break;
        }
      // case BUFF.FRAGILITY_BUFF_1:
      //   {
      //     _damageAmount = _damageAmount + Math.round(_damageAmount * this.amounts[0]);
      //   }
      //   break;
      // case BUFF.LIGHTNING_SHIELD_BUFF_2:
      // case BUFF.LIGHTNING_SHIELD_BUFF_3:
      //   {
      //     if (_source != _ownerOfThisBuff) //nechci si davat dmg sam sobe
      //       _encounter.dealDamageToCombatEntity(_ownerOfThisBuff, _source, this.amounts[0], this.buffGroupId, _encounter);
      //   }
      //   break;
      case BUFF.COUNTERSTRIKE_BUFF_1:
        {
          if (_source != _ownerOfThisBuff) //nechci si davat dmg sam sobe
          {
            _encounter.dealDamageToCombatEntity(_ownerOfThisBuff, _source, _damageAmount, this.buffGroupId, _encounter);
            _ownerOfThisBuff.removeBuff(this);
          }
        }
        break;

      default:
        break;
    }

    return _damageAmount;
  }

  // tohle musim volat pokazde nez owner udeli dmg. Vola se to ted v te combatEntity dealtdmg funkci
  applyEffectBeforeOwnerDealsDamage(_damageAmount: number): number {

    switch (this.buffId) {
      case BUFF.WEAKNESS_BUFF_1:
        _damageAmount = _damageAmount - (this.amounts[0] * _damageAmount);
        break;

      default:
        break;
    }

    return _damageAmount;
  }


  // tohle musim volat pokazde kdyz owner je healnuty. Vola se to ted v te combatEntity heal funkci
  applyEffectBeforeOwnerIsHealed(_healAmount: number): number {

    switch (this.buffId) {
      case BUFF.MORTAL_WOUNDS_BUFF_1:
        _healAmount = _healAmount - (this.amounts[0] * _healAmount);
        break;

      default:
        break;
    }

    return _healAmount;
  }


  applyEffectWhenTurnEnds(_ownerOfThisBuff: CombatEntity, _encounter: EncounterDocument) {

    switch (this.buffId) {
      case BUFF.REJUVENATION_BUFF_1:
      case BUFF.REJUVENATION_BUFF_2:
      case BUFF.REJUVENATION_BUFF_3:
        {
          let caster = _encounter.getCombatEntityByUid(this.creatorUid);
          if (caster != null) {
            _encounter.giveHealthToCombatEntity(caster, _ownerOfThisBuff, this.amounts[0], this.buffGroupId);
          }
          break;

        }
      case BUFF.STRENGTH_BUFF:
        {
          _ownerOfThisBuff.stats.damagePowerTotal += this.amounts[0];
          _encounter.addEntryToCombatLog("<b>{" + _ownerOfThisBuff.displayName + "}</b>" + " gained " + "<b>" + this.amounts[0] + " strength </b>");
          //pridam zaznam do CombatFlow
          _encounter.combatFlow.push(new CombatFlowEntry(_ownerOfThisBuff.uid, _ownerOfThisBuff.uid, this.buffId, this.amounts[0], true, true));
          break;

        }
      case BUFF.BLEED_BUFF_1:

        {
          let caster = _encounter.getCombatEntityByUid(this.creatorUid);
          if (caster != null) {
            _encounter.dealDamageToCombatEntity(caster, _ownerOfThisBuff, this.amounts[0], this.buffGroupId, _encounter)
          }
          break;

        }
      case BUFF.SIPHON_LIFE_BUFF_2:
        {
          let siphonCaster = _encounter.getCombatMemberByUid(this.creatorUid);

          if (siphonCaster != null) {
            let result = _encounter.dealDamageToCombatEntity(siphonCaster, _ownerOfThisBuff, this.amounts[0], this.buffGroupId, _encounter);
            _encounter.giveHealthToCombatEntity(siphonCaster, siphonCaster, result.totalAmount, this.buffGroupId);
          }

          break;

        }
      case BUFF.CORRUPTION_BUFF_1:
      case BUFF.CORRUPTION_BUFF_2:
      case BUFF.CORRUPTION_BUFF_3:
        {
          let caster = _encounter.getCombatEntityByUid(this.creatorUid);
          if (caster != null) {
            let turnsPassed = this.durationTurns - this.turnsLeft;
            let bonusDmg = turnsPassed * this.amounts[1];
            console.log("turnsPassed : " + turnsPassed + "bonusDmg:  " + bonusDmg);
            _encounter.dealDamageToCombatEntity(caster, _ownerOfThisBuff, this.amounts[0] + bonusDmg, this.buffGroupId, _encounter)

          }
          break;

        }
      // case BUFF.MYSTIC_SHIELD_BUFF_1:
      //   {
      //     _ownerOfThisBuff.blockAmount += this.amounts[0];
      //     break;
      //   }
      default:
        break;
    }

  }



}

export class CombatStats {
  constructor(
    public manaMax: number,
    public mana: number,
    public healthMax: number,
    public health: number,
    public leastHealth: number, //this keeps track of the least health player had during fight. Which is than used to transfer its value to character.
    public healthFatiguePenalty: number, // proste fatigue z charakteru, abych vedel v boji kdo ma kolik fatigue

    public activeBlockAmount: number,  //current active block amount. Zvetsuje se kartama hlavne a na kocni kole se nuluje.

    //tohle jsou optinal staty z equipu co muzu a nemusim mit
    public hpRegenTotal: number,
    public critChanceTotal: number,
    public damagePowerTotal: number, //zveda dmg vsech skillu
    public resistanceTotal: number, //snizuje dmg taken flat
    public defenseTotal: number, //pridava +defense , defense kartam
    public healingPowerTotal: number, //pridava healing amount 

    public skillBonusEffects: SkillBonusEffect[], //soucet vsech skill bonus efektu co ma equip abych pak mohl alternout combatskilly v boji

    //seznam a soucet vsech rareEffektu ktere ovlivnuji skilly

    public skillDrawCount: number,
    public buffBonusEffects: BuffBonusEffect[], //soucet vsech buff bonus efektu co ma equip abych pak mohl alternout combatskilly v boji

    public manaRegen: number

  ) { }
}


export class CombatFlowEntry {
  constructor(
    public caster: string,
    public target: string,
    public effectId: string,
    public amount: number,
    public isPositive: boolean,
    public isSpecialEffect: boolean
  ) { }
}


// export class LootSpot {
//   constructor(

//     public uid: string,
//     public id: string,

//     public restrictedClass: string[],
//     public restrictedProfession: SimpleTally[],

//     public priceDiscountClass: SimpleTally[],
//     public priceDiscountProfession: SimpleTally[],

//     public explorePrice: number,

//     public loot: ContentContainer[],
//     public enemy: CombatEnemy,

//     public amounts: number[],

//     public rarity: string,
//     public hasCurse: boolean
//   ) { }


// }


export class PerkChoiceParticipant {
  constructor(
    public characterUid: string,
    public choosenPerk: PerkOfferDefinition | null, // null se pouziva kdyz si uz dokoncil tier tak mas vybrany null per a nic nevybiras dal tim padem
    public characterPortraitId: string,
    public characterClass: string,
    public characterName: string
  ) { }
}

export class EncounterDocument {
  constructor(
    public uid: string,
    public enemies: CombatEnemy[],
    public combatants: CombatMember[],
    public combatantList: string[],
    public randomIndex: number,
    public expireDate: string,
    public foundByCharacterUid: string, // dungeon encounteru tu maji id party....
    public maxCombatants: number,
    public watchersList: string[],
    public isFull: boolean,
    public foundByName: string,
    // public chatLog: string,
    public encounterContext: string,
    public position: WorldPosition,
    // public restsPerTurn: number,
    //  public restsLeftUntilEndOfTurn: number,
    public turnNumber: number,
    public combatLog: string,
    public expireDateTurn: string,
    //public foundByPartyUid: string,
    public combatFlow: CombatFlowEntry[],
    //public bonusLoot :EncounterResultContentLoot
    public bonusLoot: ContentContainer[],
    // public lootSpots: LootSpot[],
    // public lootSpotsActive: LootSpot[],
    public curseCount: number,  //MOMENTALNE K NICMENU....kletby se davaji rovnou otmu kdo   vybral perk s kletbama a neni treba mit v encounteru nejaky pocet kledeb
    public perksOffers: PerkOfferDefinition[],
    public perkChoices: PerkChoiceParticipant[],
    public joinPrice: number,
    public perksOffersRare: PerkOfferDefinition[],
    public forbiddenCharacterUids: string[], //hraci v tomhle listu se nemuzou pridat do encounteru...pouzivam v dungeonech, kdyz surrendernes tak uz se nemuzes vratit aby sis nerestoval atp?(neni lepsi zakazat rest a cestovani kdyz si v dungeonu?)
    public typeOfOrigin: string, //z ktereho typeId daneho point of interestu byl encounter vytvoren....tedy defakto ID te internal definice
    public tier: number //pokud je encounter vytvoreny z monster encounter tak tier monstera, pokud dung tak asi floor dungu je tu,...

  ) { }

  //tohle neni optimalni, pouzivam jen proti zaseku, kdy je moc hracu a nemaji vybirat z dosti perku....takze je pak poustim hned do hry...nicmene mel bych je pustit jen kdyz uz neni zadny perk na vybrani...
  //...takze z tohodle pak spis udelat neo jako areThereAnyPerkLeftForChoice
  areThereEnoughPerksLeftForChoice(): boolean {

    return this.perksOffers.concat(this.perksOffersRare).filter(perkOffer => (perkOffer.stockLeft - perkOffer.stockClaimed > 0 || perkOffer.stockLeft == -1)).length >= this.combatants.length
  }


  // getChoosenPerks(): PerkOfferDefinition[] {
  //   let result: PerkOfferDefinition[] = [];

  //   this.perkChoices.forEach(perkChoice => {
  //     let perk = this.perksOffers.find(perkOffer => perkOffer.uid == perkChoice.perkUid);
  //     if (perk)
  //       result.push(perk)

  //     perk = this.perksOffersRare.find(perkOffer => perkOffer.uid == perkChoice.perkUid);
  //     if (perk)
  //       result.push(perk)

  //   });

  //   return result;
  // }

  hasEveryoneChoosenHisPerk() {
    return this.perkChoices.length == this.combatantList.length;
  }




  getNumberOfEnemiesAlive(): number {

    return this.enemies.filter(enemy => enemy.stats.health > 0).length;

  }

  getRandomEnemy(_enemyUidToExclude: string): CombatEnemy | null {

    let eligibleEnemies = this.enemies.filter(enemy => enemy.uid !== _enemyUidToExclude && enemy.stats.health > 0);

    if (eligibleEnemies.length === 0) {
      return null;
    }

    return eligibleEnemies[randomIntFromInterval(0, eligibleEnemies.length - 1)];
  }


  getAdjecentEnemiesOfEnemy(_enemyUid: string): [CombatEnemy | null, CombatEnemy | null] {

    const index = this.enemies.findIndex(enemy => enemy.uid === _enemyUid);

    if (index === -1) {
      throw _enemyUid + " does not exists in enemies list ";
    }

    const leftEnemy = index > 0 ? this.enemies[index - 1] : null;
    const rightEnemy = index < this.enemies.length - 1 ? this.enemies[index + 1] : null;

    return [leftEnemy, rightEnemy];
  }


  getRandomEnemyExcludeEnemies(_enemyUidToExclude: string[], _alsoExcludeDeadEnemies: boolean): CombatEnemy | null {


    let eligibleEnemies: CombatEnemy[] = [];

    if (_alsoExcludeDeadEnemies) {
      eligibleEnemies = this.enemies.filter(item1 => {
        return !_enemyUidToExclude.some(item2 => {
          return item1.uid === item2 && item1.stats.health > 0;
        });
      });
    }
    else {
      eligibleEnemies = this.enemies.filter(item1 => {
        return !_enemyUidToExclude.some(item2 => {
          return item1.uid === item2;
        });
      });

    }

    if (eligibleEnemies.length === 0) {
      return null;
    }

    return eligibleEnemies[randomIntFromInterval(0, eligibleEnemies.length - 1)];
  }



  getRandomHero(_heroUidToExclude: string): CombatMember | null {

    let eligibleHeroes = this.combatants.filter(enemy => enemy.uid !== _heroUidToExclude && enemy.stats.health > 0);

    if (eligibleHeroes.length === 0) {
      return null;
    }

    return eligibleHeroes[randomIntFromInterval(0, eligibleHeroes.length - 1)];
  }

  removeCharacterFromCombat(_characterUid: string) {

    for (var i = this.combatants.length - 1; i >= 0; i--) {
      if (this.combatants[i].characterUid == _characterUid) {
        this.removeAllThreatForCombatantOnAllEnemies(this.combatants[i].uid);
        this.addEntryToCombatLog(this.combatants[i].displayName + " <color=\"yellow\">Fleed</color> from combat!");

        this.combatants.splice(i, 1);
        break;
      }
    }
    this.combatantList.splice(this.combatantList.indexOf(_characterUid), 1);
    this.isFull = false;

  }

  hasAllCombatMembersRested(): boolean {

    for (const combatant of this.combatants) {
      if (combatant.hasRested == false && combatant.stats.health > 0)
        return false;

    }
    return true;
  }

  addThreatForCombatantOnAllEnemies(_combatantUid: string, _threatAmount: number) {
    this.enemies.forEach(enemy => { enemy.addThreatForCombatant(_combatantUid, _threatAmount); });
  }

  removeAllThreatForCombatantOnAllEnemies(_combatantUid: string) {
    this.enemies.forEach(enemy => { enemy.setThreatForCombatant(_combatantUid, 0); });
  }

  addEntryToCombatLog(_entry: string) {
    this.combatLog += _entry + "\n";
  }

  // addEntryToChatLog(_entry: string) {
  //   this.chatLog += _entry + "\n";
  // }

  convertEnemiesToEncounterResultEnemies(_dropTablesData: EnemyDropTablesData, _skillDefinitions: SkillDefinitions): EncounterResultEnemy[] {

    let result: EncounterResultEnemy[] = [];

    this.enemies.forEach(enemy => {
      let enemyDropTables: DropTable[] | null = null;
      //ziskam droptable pro enemy 
      for (const dropTable of _dropTablesData.enemyDropTables) {
        if (dropTable.enemyId == enemy.enemyId) {
          enemyDropTables = dropTable.dropTables;
        }
      }

      if (enemyDropTables == null)
        throw "Database error - Cant find drop table for enemy :" + enemy.enemyId;

      let encounterResultLoot: EncounterResultContentLoot[] = [];
      generateDropFromDropTable(enemyDropTables, enemy.mLevel, _skillDefinitions).forEach(content => {
        // enemy.generateDrop(enemyDropTables).forEach(content => {
        console.log("pushuje novy conent: " + content.getItem().itemId);
        encounterResultLoot.push(new EncounterResultContentLoot(content, [], null))
      });

      result.push(new EncounterResultEnemy(enemy.enemyId, enemy.displayName, enemy.level, encounterResultLoot, enemy.monsterEssences));
    });

    return result;
  }

  getSilverAmount(): number {

    let silverTotal: number = 0;

    this.enemies.forEach(enemy => {
      silverTotal += randomIntFromInterval(enemy.mLevel * 0.75 + 1, enemy.mLevel * 1.25) * 5;
    });

    silverTotal = silverTotal / (this.combatantList.length);
    return silverTotal;
  }

  checkForEndOfTurn() {

    //---TURN ENDED!---
    if (this.hasAllCombatMembersRested()) {
      this.addEntryToCombatLog("<color=\"lightblue\"><b>Turn Ended</b></color> (" + this.turnNumber + ")");// + " after he casted " + _usedSkill.skillId);

      this.resetsAllBlockEnemies(); //..nepratelum zustava block zakouzleny v tomto kole

      this.allEnemiesApplySkillAndChooseNewOne(); //...enemy pouziji skilly na konci kola
      this.lowerTurnsLeftOnAllBuffsInEncounter(); //prvni aplikuju vsechny buffy
      this.allComabtMembersRestores(); //hraci si restornou zivot/manu a karty
      this.resetsAllBlockCombatMembers(); //..vsechny blocky na 0
      this.turnNumber++;
      this.expireDateTurn = getCurrentDateTimeVersionSecondsAdd(TURN_DURATION_SECONDS);
      //a znovu zvedneme natavime hracum ze nejsou rested
      this.combatants.forEach(combatant => { combatant.hasRested = false });
      this.newRoundStarted() //momentalne se pouziva pro buffy, ktere delaji nejaky efekt jakmile jedno kolo skonci a chteji neco udelat na zacatku kola noveho

    }

  }

  resetsAllBlockCombatMembers() {

    this.combatants.forEach(combatant => {
      combatant.blockAmount = 0;
    });

  }

  resetsAllBlockEnemies() {

    this.enemies.forEach(enemy => {
      enemy.blockAmount = 0;
    });

  }

  lowerTurnsLeftOnAllBuffsInEncounter() {

    this.combatants.forEach(combatant => {
      combatant.lowerTurnsLeftOnMyBuffs(this);
    });

    this.enemies.forEach(enemy => {
      enemy.lowerTurnsLeftOnMyBuffs(this);
    });

  }

  newRoundStarted() {

    this.combatants.forEach(combatant => {
      combatant.applyEffectWhenNewRoundStartsOnMyBuffs(this);
    });

    this.enemies.forEach(enemy => {
      enemy.applyEffectWhenNewRoundStartsOnMyBuffs(this);
    });

  }

  allComabtMembersRestores() {

    this.combatants.forEach(combatMember => {

      //notifikuje kazdeho combaMembera ze tah skoncil at si udela svoje voodoo, treba notifikuje sve karty v ruce at udelaji efekty na konci kola.....teoreticky by toto melo i pak triggronout buffy, ale to mam asi extra nekde jeste
      combatMember.turnEnded(this);

      if (combatMember.stats.health > 0) { //jen pokud je nazivu

        //pokud shufflujem deck
        if (drawNewSkills(combatMember.skillsInHand, combatMember.skillsDrawDeck, combatMember.skillsDiscardDeck, combatMember.stats.skillDrawCount)) {
          combatMember.deckShuffleCount++;

          //vezmeme 1% max HP * pocet shufflu
          // let healthToTake = combatMember.stats.healthMax * combatMember.deckShuffleCount * DECK_SHUFFLE_MAX_HP_PENALTY;
          // if (combatMember.stats.health <= healthToTake)
          //   healthToTake = combatMember.stats.health - 1;

          // this.dealDamageToCombatEntity(combatMember, combatMember, Math.round(healthToTake), "DECK_SHUFFLED", this, true);


        }

        //regen HP
        let healthRegenAmount = this.giveHealthToCombatEntity(combatMember, combatMember, combatMember.stats.hpRegenTotal, "HP REGEN", false).totalAmount;
        if (healthRegenAmount > 0)
          this.addEntryToCombatLog("<b>" + combatMember.displayName + "</b>" + " restored <color=\"yellow\">" + healthRegenAmount + "</color> health");

        //Regen Mana
        let manaRegenAmount = combatMember.regenMana();
        if (manaRegenAmount > 0)
          this.addEntryToCombatLog("<b>" + combatMember.displayName + "</b>" + " restored <color=\"yellow\">" + manaRegenAmount + "</color> mana");
      }
    });
  }

  allEnemiesApplySkillAndChooseNewOne() {
    this.enemies.forEach(enemy => {
      enemy.applySkillAndChooseNextOne(this);//this.enemyDealDamageToItsTarget(enemy);
    });

  }

  // giveHealthToAlly(_caster: CombatMember, _target: CombatMember, _amount: number, _sourceId: string) {
  //   if (_target.stats.health <= 0)
  //     return;

  //   // if (_target.stats.health + _amount > _target.stats.healthMax) _amount = _target.stats.healthMax - _target.stats.health;
  //   // _target.stats.health += _amount;
  //   // _amount = Math.round(_amount);

  //   _amount = Math.round(_amount);

  //   _amount = _target.giveHealth(_amount);

  //   const entry = "<b>" + _target.displayName + "</b>" + " was healed for <color=\"green\">" + _amount + "</color>"; //health by " + _caster.displayName + " (" + _sourceId + ")";
  //   this.addEntryToCombatLog(entry);
  //   console.log(entry);
  //   //combatLot.entries.push(entry)

  //   //pridam zaznam do CombatFlow
  //   this.combatFlow.push(new CombatFlowEntry(_caster.uid, _target.uid, _sourceId, _amount, true, false));

  //   //Zaloguju damage do STATS
  //   this.combatants.forEach(item => {
  //     if (item.characterUid == _caster.characterUid) { //najdu svuj combatant zazanam v encounter datech
  //       if (_amount > 0)
  //         item.healingDone += _amount;
  //       else
  //         console.log("Heal nemuze byt mensi nez 0!");
  //     }
  //   });

  //   //pridam threat 1:0,5 heal vsem enemy
  //   this.addThreatForCombatantOnAllEnemies(_caster.characterUid, _amount / 2);
  //   // _target.addThreatForCombatant(_attacker.characterUid, _amount);
  // }

  giveHealthToCombatEntity(_caster: CombatEntity, _target: CombatEntity, _amount: number, _sourceId: string, _makeALogEntry: boolean = true): { totalAmount: number } {
    let totalAmount: number = 0;

    if (_target.stats.health <= 0 || _amount == 0)
      return { totalAmount };


    //aplikujem vsechny efekty z buffy pred tim nez aplikujem dmg...
    _target.buffs.forEach(buff => { _amount = buff.applyEffectBeforeOwnerIsHealed(_amount) });


    _amount = Math.round(_amount);
    _amount = _target.giveHealth(_amount);

    let entry = "";
    if (_target instanceof CombatEnemy)
      entry = "<b>{" + _target.displayName + "}</b>" + " was healed for <color=\"green\">" + _amount + "</color>" + " ({" + _sourceId + "})"; //health by " + _caster.displayName + " (" + _sourceId + ")";
    else
      entry = "<b>" + _target.displayName + "</b>" + " was healed for <color=\"green\">" + _amount + "</color>" + " ({" + _sourceId + "})"; //health by " + _caster.displayName + " (" + _sourceId + ")";

    if (_makeALogEntry)
      this.addEntryToCombatLog(entry);

    console.log(entry);
    //combatLot.entries.push(entry)

    //pridam zaznam do CombatFlow
    this.combatFlow.push(new CombatFlowEntry(_caster.uid, _target.uid, _sourceId, _amount, true, false));

    //Zaloguju damage do STATS
    if (_caster instanceof CombatMember) {
      this.combatants.forEach(item => {
        if (item.characterUid == _caster.characterUid) { //najdu svuj combatant zazanam v encounter datech
          if (_amount > 0)
            item.healingDone += _amount;
          else
            console.log("Heal nemuze byt mensi nez 0!");
        }
      });
    }
    //pridam threat 1:0,5 heal vsem enemy
    if (_caster instanceof CombatMember)
      this.addThreatForCombatantOnAllEnemies(_caster.characterUid, _amount / 2);

    totalAmount = _amount;
    return { totalAmount };
    // _target.addThreatForCombatant(_attacker.characterUid, _amount);
  }

  dealDamageToCombatEntity(_attacker: CombatEntity, _target: CombatEntity, _amount: number, _sourceId: string, _encounter: EncounterDocument, _ignoreAnyBonusDamage: boolean = false): { totalAmount: number, hasDamageKilledIt: boolean, wasCriticalHit: boolean } {
    let totalAmount: number = 0;
    let hasDamageKilledIt: boolean = false;
    let wasCriticalHit: boolean = false;

    if (_target.stats.health <= 0)
      return { totalAmount, hasDamageKilledIt, wasCriticalHit };

    //zvysime dmg o power - ted to je tu jen kvuli enemy kteri si zvysujou total dmg power...hrdinove to maji uz v tom skillu natvrdo convertnute a pridane pri joinuti do boje
    if (_attacker instanceof CombatEnemy && !_ignoreAnyBonusDamage)
      _amount += _attacker.stats.damagePowerTotal;

    //aby buffy jako weakness snizili dmg ktery chci udelit...
    if (!_ignoreAnyBonusDamage) {
      _attacker.buffs.forEach(buff => { _amount = buff.applyEffectBeforeOwnerDealsDamage(_amount) });
    }
    //hodime si jestli nahodou nebude utok kriticky , ted mam 5% base crit chance pro vsechny ....chtelo by to zase modifikovat podle levelu utocnika vs obrance , pripadne nejake defense a weapon skill co je ve wow to modifikuje...

    let criticalStrike = "";
    if (!_ignoreAnyBonusDamage) {
      if (randomIntFromInterval(0, 100) <= _attacker.stats.critChanceTotal) {
        _amount *= 1.5;
        criticalStrike = "<color=#808080>(critical)</color>";
        wasCriticalHit = true;
      }
    }

    //aplikujem vsechny efekty z buffy pred tim nez aplikujem dmg...
    _target.buffs.forEach(buff => { _amount = buff.applyEffectBeforeAnyDamageIsAppliedToOwner(_target, _amount, _attacker, _encounter) });


    let resistedAmount = 0;
    //snizime zraneni o resist
    if (!_ignoreAnyBonusDamage) {
      resistedAmount = _target.stats.resistanceTotal;

      _amount -= _target.stats.resistanceTotal;
      if (_amount < 0) {
        resistedAmount -= _amount * (-1);
        _amount = 0;
      }
    }

    //snizime zraneni o block
    let blockedAmount = _target.blockAmount;
    console.log("reductionAmount: " + blockedAmount);
    _amount -= blockedAmount;
    _amount = Math.round(_amount);
    _target.stats.activeBlockAmount = 0;

    if (_amount < 0) {
      _target.blockAmount = _amount * (-1);
      blockedAmount -= _target.blockAmount;
      _amount = 0;
    }
    else
      _target.blockAmount = 0;

    let dmgBlockedAmount = "";
    if (blockedAmount > 0)
      dmgBlockedAmount = "<color=#808080>(" + blockedAmount.toString() + " damage blocked)</color>";

    let resistedAmountText = "";
    if (resistedAmount > 0)
      resistedAmountText = "<color=#808080>(" + resistedAmount.toString() + " damage resisted)</color>";

    totalAmount = _target.takeHealth(_amount, _attacker, _encounter);//Math.round(_target.takeHealth(_amount));

    //console.log("_target.name: " + _target.displayName + "_target.uid: " + _target.uid + " _attacker.name :" + _attacker.displayName);
    let entry = "";
    if (_target instanceof CombatEnemy)
      entry = "<b>{" + _target.displayName + "}</b>" + " suffered <color=\"red\">" + totalAmount + "</color> damage " + " ({" + _sourceId + "})" + criticalStrike + " " + dmgBlockedAmount + " " + resistedAmountText;// from " + _attacker.displayName ;//+ " (" + _sourceId + ")";
    else
      entry = "<b>" + _target.displayName + "</b>" + " suffered <color=\"red\">" + totalAmount + "</color> damage " + " ({" + _sourceId + "})" + criticalStrike + " " + dmgBlockedAmount + " " + resistedAmountText;// from " + _attacker.displayName ;//+ " (" + _sourceId + ")";


    this.addEntryToCombatLog(entry);

    //pridam zaznam do CombatFlow
    console.log("_sourceId:" + _sourceId);
    this.combatFlow.push(new CombatFlowEntry(_attacker.uid, _target.uid, _sourceId, totalAmount, false, false));

    console.log(entry);
    //  combatLog.entries.push(entry)

    if (_attacker instanceof CombatMember) {
      //Zaloguju damage do STATS
      this.combatants.forEach(item => {
        if (item.characterUid == _attacker.characterUid) { //najdu svuj combatant zazanam v encounter datech
          if (totalAmount > 0)
            item.damageDone += totalAmount;
          else
            console.log("Damage nemuze byt mensi nez 0!");
        }
      });
    }
    //pridam threat 1:1 dmg
    if (_target instanceof CombatEnemy && _attacker instanceof CombatMember)
      _target.addThreatForCombatant(_attacker.characterUid, totalAmount);

    if (_target.stats.health == 0 && _target instanceof CombatMember) //player Died
    {
      this.addEntryToCombatLog("<b>" + _target.displayName + "<color=\"red\"> Died !</color></b>");
      this.removeAllThreatForCombatantOnAllEnemies(_target.uid);
      (_target as CombatMember).hasRested = true;
      //_target.buffs = [];
      hasDamageKilledIt = true;
      //   this.checkForEndOfTurn();

    }
    else if (_target.stats.health == 0 && _target instanceof CombatEnemy) //enemy died
    {
      this.addEntryToCombatLog("<b>{" + _target.displayName + "}<color=\"green\"> Died !</color></b>");
      // _target.buffs = [];
      hasDamageKilledIt = true;
    }

    if (hasDamageKilledIt) {
      _target.buffs.forEach(buff => {
        buff.applyEffecWhenOwnerDies(_target, _encounter);
      });
      _target.buffs = [];
    }


    return { totalAmount, hasDamageKilledIt, wasCriticalHit };
  }

  getCombatMemberByUid(_combatMemberUid: string): CombatMember | null {
    for (const combatMember of this.combatants) {
      if (combatMember.characterUid == _combatMemberUid) {
        return combatMember;
      }
    }
    return null;
    //throw ("There is no combat member with UID :" + _combatMemberUid);
  }


  getCombatEntityByUid(_combatEntityUid: string): CombatEntity | null {
    for (const combatMember of this.combatants) {
      if (combatMember.characterUid == _combatEntityUid) {
        return combatMember;
      }
    }
    for (const enemy of this.enemies) {
      if (enemy.uid == _combatEntityUid) {
        return enemy;
      }
    }
    return null;
    //throw ("There is no combat member with UID :" + _combatMemberUid);
  }




  forceEnemyToChangeTarget(_caster: CombatMember, _target: CombatEnemy, _whoToTarget: CombatMember, _amounOfAddedThreat: number) {

    if (_target.stats.health <= 0)
      return;

    let entry = "<b>{" + _target.displayName + "}</b>" + " was taunted";// from " + _attacker.displayName ;//+ " (" + _sourceId + ")";
    this.addEntryToCombatLog(entry);
    console.log(entry);

    //forcnu zmenu targetu
    _target.targetUid = _whoToTarget.uid;

    //pridam threat
    _target.addThreatForCombatant(_caster.characterUid, _amounOfAddedThreat);
  }



  //Najde moje Combatant data v encounteru...musim to delat takhle prohledavanim protoze neumim volat metody na Combatantovi samotnem, tak ho musim vzdy najit...
  //UNUSED!!
  increaseAttackCountForCharacter(_characterUid: string) {
    this.combatants.forEach(item => {
      // console.log("item " +item.characterUid);
      if (item.characterUid == _characterUid) {
        //  item.attacksCount++;
        //   console.log("vracim " +item.characterUid);
      }
    });

  }

  checkIfAllEnemiesAreDead(): boolean {
    let allEnemiesDead: boolean = true;
    for (let index = 0; index < this.enemies.length; index++) {
      if (this.enemies[index].stats.health > 0)
        allEnemiesDead = false;
    }
    return allEnemiesDead;
  }
}


export const encounterDocumentConverter = {
  toFirestore: (_encounter: EncounterDocument) => {


    return {
      enemies: _encounter.enemies,
      combatants: _encounter.combatants,
      combatantList: _encounter.combatantList,
      maxCombatants: _encounter.maxCombatants,
      watchersList: _encounter.watchersList,
      isFull: _encounter.isFull,
      foundByName: _encounter.foundByName,
      position: _encounter.position,
      uid: _encounter.uid,
      turnNumber: _encounter.turnNumber,
      combatLog: _encounter.combatLog,
      foundByCharacterUid: _encounter.foundByCharacterUid,
      expireDateTurn: _encounter.expireDateTurn,
      encounterContext: _encounter.encounterContext,
      // foundByPartyUid: _encounter.foundByPartyUid,
      combatFlow: _encounter.combatFlow,
      curseCount: _encounter.curseCount,
      perksOffers: _encounter.perksOffers,
      joinPrice: _encounter.joinPrice,
      perksOffersRare: _encounter.perksOffersRare,
      perkChoices: _encounter.perkChoices,
      forbiddenCharacterUids: _encounter.forbiddenCharacterUids,
      typeOfOrigin: _encounter.typeOfOrigin
    };
  },
  fromFirestore: (snapshot: any, options: any) => {
    const data = snapshot.data(options);


    //  // pokud chci volat combatatns neco tak tady musim jak ve skills vytvorit nove classy combatantu a naplnit je hodnotama aby meli pristupne metody?!! Nejde to ...

    let combatants: CombatMember[] = [];

    data.combatants.forEach(combatMember => {

      let buffs: CombatBuff[] = [];
      combatMember.buffs.forEach(buff => {
        buffs.push(new CombatBuff(buff.buffId, buff.durationTurns, buff.turnsLeft, buff.amounts, buff.buffGroupId, buff.creatorUid));
      });

      let skillsInHand: Combatskill[] = [];
      combatMember.skillsInHand.forEach(skillInHand => {

        let buff: CombatBuff | undefined;
        if (skillInHand.buff != undefined)
          buff = new CombatBuff(skillInHand.buff.buffId, skillInHand.buff.durationTurns, skillInHand.buff.turnsLeft, skillInHand.buff.amounts, skillInHand.buff.buffGroupId, skillInHand.buff.creatorUid);

        skillsInHand.push(new Combatskill(skillInHand.skillId, skillInHand.characterClass, skillInHand.manaCost, skillInHand.amounts, skillInHand.alreadyUsed, skillInHand.uid, buff, skillInHand.skillGroupId, skillInHand.singleUse, skillInHand.validTarget_AnyAlly, skillInHand.validTarget_Self, skillInHand.validTarget_AnyEnemy, skillInHand.rarity, skillInHand.originalStats, skillInHand.quality));
      });




      let skillsInDrawDeck: Combatskill[] = [];

      combatMember.skillsDrawDeck.forEach(skillDrawDeck => {
        let buff: CombatBuff | undefined;
        if (skillDrawDeck.buff != undefined)
          buff = new CombatBuff(skillDrawDeck.buff.buffId, skillDrawDeck.buff.durationTurns, skillDrawDeck.buff.turnsLeft, skillDrawDeck.buff.amounts, skillDrawDeck.buff.buffGroupId, skillDrawDeck.buff.creatorUid);

        skillsInDrawDeck.push(new Combatskill(skillDrawDeck.skillId, skillDrawDeck.characterClass, skillDrawDeck.manaCost, skillDrawDeck.amounts, skillDrawDeck.alreadyUsed, skillDrawDeck.uid, buff, skillDrawDeck.skillGroupId, skillDrawDeck.singleUse, skillDrawDeck.validTarget_AnyAlly, skillDrawDeck.validTarget_Self, skillDrawDeck.validTarget_AnyEnemy, skillDrawDeck.rarity, skillDrawDeck.originalStats, skillDrawDeck.quality));
      });

      let skillsDiscardDeck: Combatskill[] = [];
      combatMember.skillsDiscardDeck.forEach(skillInDiscardDeck => {
        let buff: CombatBuff | undefined;
        if (skillInDiscardDeck.buff != undefined)
          buff = new CombatBuff(skillInDiscardDeck.buff.buffId, skillInDiscardDeck.buff.durationTurns, skillInDiscardDeck.buff.turnsLeft, skillInDiscardDeck.buff.amounts, skillInDiscardDeck.buff.buffGroupId, skillInDiscardDeck.buff.creatorUid);

        skillsDiscardDeck.push(new Combatskill(skillInDiscardDeck.skillId, skillInDiscardDeck.characterClass, skillInDiscardDeck.manaCost, skillInDiscardDeck.amounts, skillInDiscardDeck.alreadyUsed, skillInDiscardDeck.uid, buff, skillInDiscardDeck.skillGroupId, skillInDiscardDeck.singleUse, skillInDiscardDeck.validTarget_AnyAlly, skillInDiscardDeck.validTarget_Self, skillInDiscardDeck.validTarget_AnyEnemy, skillInDiscardDeck.rarity, skillInDiscardDeck.originalStats, skillInDiscardDeck.quality));
      });

      let skillsExhaustDeck: Combatskill[] = [];
      combatMember.skillsExhaustDeck.forEach(skillInExhaustDeck => {
        let buff: CombatBuff | undefined;
        if (skillInExhaustDeck.buff != undefined)
          buff = new CombatBuff(skillInExhaustDeck.buff.buffId, skillInExhaustDeck.buff.durationTurns, skillInExhaustDeck.buff.turnsLeft, skillInExhaustDeck.buff.amounts, skillInExhaustDeck.buff.buffGroupId, skillInExhaustDeck.buff.creatorUid);

        skillsExhaustDeck.push(new Combatskill(skillInExhaustDeck.skillId, skillInExhaustDeck.characterClass, skillInExhaustDeck.manaCost, skillInExhaustDeck.amounts, skillInExhaustDeck.alreadyUsed, skillInExhaustDeck.uid, buff, skillInExhaustDeck.skillGroupId, skillInExhaustDeck.singleUse, skillInExhaustDeck.validTarget_AnyAlly, skillInExhaustDeck.validTarget_Self, skillInExhaustDeck.validTarget_AnyEnemy, skillInExhaustDeck.rarity, skillInExhaustDeck.originalStats, skillInExhaustDeck.quality));
      });

      combatants.push(new CombatMember(combatMember.uid, combatMember.displayName, combatMember.characterUid, combatMember.characterClass, skillsInHand, skillsInDrawDeck, skillsDiscardDeck, combatMember.stats, combatMember.damageDone, combatMember.hasRested, combatMember.level, combatMember.healingDone, buffs, combatMember.characterPortrait, skillsExhaustDeck, combatMember.blockAmount, combatMember.deckShuffleCount, combatMember.potionsUsed, combatMember.blesses, combatMember.hasAlreadyFinishedEncounterOfThisTier));
    });

    let enemies: CombatEnemy[] = [];
    data.enemies.forEach(enemy => {

      let buffs: CombatBuff[] = [];
      enemy.buffs.forEach(buff => {
        buffs.push(new CombatBuff(buff.buffId, buff.durationTurns, buff.turnsLeft, buff.amounts, buff.buffGroupId, buff.creatorUid));
      });


      enemies.push(new CombatEnemy(enemy.uid, enemy.enemyId, enemy.stats, enemy.level, enemy.mLevel, enemy.moveSet, enemy.isRare, enemy.targetUid, enemy.threatMetter, buffs, enemy.blockAmount, enemy.nextSkill, enemy.repeatedCastCount, enemy.monsterEssences));
    });
    //let combatMembers :CombatMember[]=[];
    // Object.assign(combatMembers, data.combatants)

    return new EncounterDocument(data.uid, enemies, combatants, data.combatantList, data.randomIndex, data.expireDate, data.foundByCharacterUid, data.maxCombatants, data.watchersList, data.isFull, data.foundByName, data.encounterContext, data.position, data.turnNumber, data.combatLog, data.expireDateTurn, data.combatFlow, data.bonusLoot, data.curseCount, data.perksOffers, data.perkChoices, data.joinPrice, data.perksOffersRare, data.forbiddenCharacterUids, data.typeOfOrigin, data.tier);
  }
};


//NEJVOLANEJSI FUNKCE 
// R - 1 , W- 1 ( + 1 delete / +1 create / -1 write, pokud encounter skonci)
exports.applySkillOnEncounter = functions.https.onCall(async (data, context) => {

  const encounterUid = data.encounterUid;
  const callerCharacterUid = data.characterUid;
  const uid = data.uid;

  const targetUid: string = data.targetUid;

  const encounterDb = admin.firestore().collection('encounters').doc(encounterUid).withConverter(encounterDocumentConverter);
  // const encounterResultsDb = admin.firestore().collection('encounterResults').doc();


  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {
      const encounterDoc = await t.get(encounterDb);
      let encounterData: EncounterDocument = encounterDoc.data();

      //Najdu jestli uz mas bojovy zaznamy
      let myCombatEntry: CombatMember | undefined;

      encounterData.combatants.forEach(item => {

        if (item.characterUid == callerCharacterUid) {
          myCombatEntry = item;
          console.log("found your combat entry ");
        }
      });

      if (myCombatEntry == undefined)
        throw "cannot find your combat entry in encounter! How can you try to attack?! character Id ! - " + callerCharacterUid;

      //tohle neni optimalni, pouzivam jen proti zaseku, kdy je moc hracu a nemaji vybirat z dosti perku....takze je pak poustim hned do hry...nicmene mel bych je pustit jen kdyz uz neni zadny perk na vybrani...
      //...takze z tohodle pak spis udelat neo jako areThereAnyPerkLeftForChoice
      if (encounterData.perkChoices.length < encounterData.combatants.length && encounterData.areThereEnoughPerksLeftForChoice())
        throw "Waiting for " + (encounterData.combatants.length - encounterData.perkChoices.length) + " more player to choose perk";


      //Najdu skill ktery chces pouzit
      let skillToUse: Combatskill | undefined;


      myCombatEntry.skillsInHand.forEach(item => {


        if (item.uid == uid) {
          skillToUse = item;
          console.log("found a skill you want to apply : " + item.uid + " id: " + item.skillId);
        }
      });

      if (skillToUse == undefined)
        throw "cannot find skill you want to use in your hand?! skill slot Id ! - " + uid;


      if (skillToUse.alreadyUsed)
        throw "Skill already used! - " + uid;

      if (myCombatEntry.stats.health <= 0)
        throw "You are dead. Cannot cast skill!";

      //let combatLog: CombatLog = new CombatLog([]); //Currently Unused, vraci to defakto jako return co se stalo pri tomhle apply skillu, celkem neuzitecne
      //pouziju skill
      try {
        applySkillEffect(myCombatEntry, encounterData, skillToUse, targetUid);//, combatLog);
        // encounterData.chatLog += myCombatEntry.displayName + " casted " + skillToUse.skillId + "\n";
      } catch (error) {

        throw error;
      }
      skillToUse.alreadyUsed = true;

      //zvednu si pocet utoku o 1
      // encounterData.increaseAttackCountForCharacter(callerCharacterUid);

      //zkontroluju jestli nahodou nejsou vsichni enemy po smrti
      if (encounterData.checkIfAllEnemiesAreDead() || INSTAKILL) {

        await createEncounterResult(encounterData, callerCharacterUid, t);


      }
      else {

        //aktualizuju encounter
        t.set(encounterDb, JSON.parse(JSON.stringify(encounterData)), { merge: true });
      }

      // return combatLog;
    });




    console.log('Transaction success', result);
    return result;
  } catch (e) {
    console.log('Transaction failure:', e);
    throw new functions.https.HttpsError("aborted", "Error : " + e);
  }
});


export async function createEncounterResult(_encounterData: EncounterDocument, _callerCharacterUid: string, t: any) {
  //zkontroluju jestli nahodou nejsou vsichni enemy po smrti
  if (_encounterData.checkIfAllEnemiesAreDead() || INSTAKILL) {

    const encounterResultsDb = admin.firestore().collection('encounterResults').doc();
    const encounterDb = admin.firestore().collection('encounters').doc(_encounterData.uid).withConverter(encounterDocumentConverter);

    //Predpokladam ze enemy ktere zabijim jsou na lokaci kde je encounter, prece musi ne?!
    const dropTablesDb = admin.firestore().collection('_metadata_dropTables').doc(_encounterData.position.zoneId).collection("locations").doc(_encounterData.position.locationId);
    const dropTablesDoc = await t.get(dropTablesDb);
    let dropTablesData: EnemyDropTablesData = dropTablesDoc.data();

    const silverAmount = _encounterData.getSilverAmount();

    let expireWantItemDate = getCurrentDateTimeVersionSecondsAdd(RESULT_ITEM_WANT_DURATION_SECONDS);
    let resultComatants: EncounterResultCombatant[] = [];
    // let encounterOwnerResultCombatant: EncounterResultCombatant | null = null;

    //OPTIMALIZACE :takze tady musim projit vsechny enemies a zjistit jestli nemaji random equip. Pokud ho totiz maji, tak potrebuju nacist z databaze data o skilech abych mohl random equip vyrobit
    //nicmene to chci zavolat jen 1x a jen v pripade ze nejaky enemy ma random equip loot. Proto ted musim projit tady vsechny enemy a jejich reward a zjistit jestli mam nebo nemam nacist z db info o skilech
    //NICMENE, to ted nedelam, protoze co dropnou generuju az v te funkci, takze ted tady POKAZDE nacitam info o skillech ikdyz ve vetsine priopadu neni VUBEC potreba! Ale co kdyz enemy opravdu dropne 
    //equip....smutne mam to proste blbe, mel bych prvni vygenereovat co chteji dropovat...
    let skillDefinitions = await QuerryForSkillDefinitions(t);
    let encounterResultEnemies = _encounterData.convertEnemiesToEncounterResultEnemies(dropTablesData, skillDefinitions);

    _encounterData.combatants.forEach(combatant => {

      let resultCombatant = new EncounterResultCombatant(combatant.uid, combatant.displayName, combatant.characterClass, combatant.level, getExpAmountFromEncounterForGivenCharacterLevel(encounterResultEnemies, combatant.level), combatant.damageDone, combatant.stats.leastHealth, combatant.deckShuffleCount, combatant.getAllCursesInDeck(true), combatant.healingDone, combatant.hasAlreadyFinishedEncounterOfThisTier);
      resultComatants.push(resultCombatant);

    });

    //preneseme bonus loot do resultu
    let bonusLootResult: EncounterResultContentLoot[] = [];
    _encounterData.bonusLoot.forEach(bonusLoot => {
      bonusLootResult.push(new EncounterResultContentLoot(bonusLoot, [], null));
    });


    let dungeonLoot: EncounterResultContentLoot[] = [];
    const wantItemPhaseFinished = _encounterData.combatantList.length == 1; //kdyz si v boji sam, neni treba pak hlasovat o lootu
    //prenesu jen vybrane perky do resultu
    const encounterResult = new EncounterResult(encounterResultsDb.id, encounterResultEnemies, _encounterData.combatantList, _encounterData.combatantList, _encounterData.combatantList, resultComatants, silverAmount, wantItemPhaseFinished, _encounterData.turnNumber, expireWantItemDate, _encounterData.position, bonusLootResult, dungeonLoot, _encounterData.perkChoices, _encounterData.foundByCharacterUid, null, _encounterData.tier);

    //pokud to byl dungeon encounter co sme dokoncili, tak jeste zvednem tier reached o 1 v parte 
    if (_encounterData.encounterContext == ENCOUNTER_CONTEXT.DUNGEON) {
      let myPartyData = await QuerryForParty(t, _callerCharacterUid);

      if (myPartyData != null) {
        if (myPartyData.dungeonProgress != null) {
          myPartyData.dungeonProgress.tierReached++;

          //ulozime dungeon data dungeon finished a hraci si to pak sam zapisou do svych charakeru jak claimnou reward
          encounterResult.dungeonData = new DungeonData(myPartyData.dungeonProgress.dungeonId, myPartyData.dungeonProgress.tierReached, false, myPartyData.dungeonProgress.isFinalDungeon, myPartyData.dungeonProgress.isEndlessDungeon);

          //pokud sme dokoncili dungeon, pridame dungeon rewardy a zapiseme hraci ze dungeon dokoncil
          if (myPartyData.dungeonProgress.tierReached == myPartyData.dungeonProgress.tiersMax) {

            // //preneseme dungeon rewardy do bonus loot do resultu
            let skillDefinitions = await QuerryForSkillDefinitions(t);
            //   for (const combatant of resultComatants) {
            myPartyData.dungeonProgress.rewards.forEach(reward => { bonusLootResult.push(new EncounterResultContentLoot(reward, [], null)); });
            myPartyData.dungeonProgress.rewardsRandomEquip.forEach(rewardRandom => { bonusLootResult.push(new EncounterResultContentLoot(generateContentContainer(generateEquip(rewardRandom.mLevel, rewardRandom.rarity, rewardRandom.equipSlotId, CHARACTER_CLASS.ANY, skillDefinitions)), [], null)); });
            myPartyData.dungeonProgress.rewardsGenerated.forEach(rewardGenerated => { bonusLootResult.push(new EncounterResultContentLoot(generateContentContainer(generateContent(rewardGenerated.itemId, rewardGenerated.amount)), [], null)); });

            //smazeme vlastne cely dungeon progress protoze sme jej dokoncili a poznacime do resultu
            myPartyData.dungeonProgress = null;
            myPartyData.dungeonEnterConsents = [];
            encounterResult.dungeonData.isFinished = true;

          }


          await t.set(admin.firestore().collection('parties').doc(myPartyData.uid), JSON.parse(JSON.stringify(myPartyData)));
        }

      }
    }

    await t.set(encounterResultsDb, JSON.parse(JSON.stringify(encounterResult)));

    t.delete(encounterDb);
  }
}

exports.forceRestEncounter = functions.https.onCall(async (data, context) => {

  //TODO:!! zkontrolovat ze hrac co chce restovat za druhe je member toho encounteru?!

  //const userUid = data.userUid;
  const encounterUid = data.encounterUid;
  const callerCharacterUid = data.characterUid;
  const forceRestOnThisCharacterUid = data.forceRestOnThisCharacterUid;

  console.log("callerCharacterUid :" + callerCharacterUid);

  const characterDb = admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);
  //const characterToForceRestOnDb = admin.firestore().collection('characters').doc(forceRestOnThisCharacterUid).withConverter(characterDocumentConverter);
  const encounterDb = admin.firestore().collection('encounters').doc(encounterUid).withConverter(encounterDocumentConverter);

  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {
      const encounterDoc = await t.get(encounterDb);
      let encounterData: EncounterDocument = encounterDoc.data();

      const characterDoc = await t.get(characterDb);
      let characterData: CharacterDocument = characterDoc.data();

      //const characterToForceRestOnDoc = await t.get(characterToForceRestOnDb);
      // let characterToForceRestOnData: CharacterDocument = characterToForceRestOnDoc.data();

      console.log("characterData.userUid :" + characterData.userUid);
      console.log("context.auth?.uid :" + context.auth?.uid);

      if (!encounterData.combatantList.includes(characterData.uid))
        throw "You are not joined to this ecnounter, you cant force rest on other players!";


      //Volas rest za jineho hrace!
      const secondsPassedOverTurnLimit = millisToSeconds(getMillisPassedSinceTimestamp(encounterData.expireDateTurn));
      if (secondsPassedOverTurnLimit > 0) {
        console.log("playerUid :" + context.auth?.uid + " is calling end of turn for player : " + characterData.characterName + " because Turn timer is overdue by : " + secondsPassedOverTurnLimit + " seconds");
      }
      else
        throw "Turn timer has not expired, yet. Cant rest for other players!";

      restEncounter(forceRestOnThisCharacterUid, encounterData);

      t.set(encounterDb, JSON.parse(JSON.stringify(encounterData)), { merge: true });

    });

    console.log('Transaction success', result);
    return result;
  } catch (e) {
    console.log('Transaction failure:', e);
    throw new functions.https.HttpsError("aborted", "Error : " + e);
  }
});

function restEncounter(_characterUid: string, _encounterData: EncounterDocument) {
  //Pridam kazemu enemy malinko threat na me, aby me targetnuli pokud nikoho netargetujou
  _encounterData.addThreatForCombatantOnAllEnemies(_characterUid, 1);

  let myCombatEntry = _encounterData.getCombatMemberByUid(_characterUid);

  if (myCombatEntry == undefined)
    throw "cannot find your combat entry in encounter! How can you try to end turn?! character Id ! - " + _characterUid;

  if (myCombatEntry.stats.health <= 0)
    throw "You are dead. Cannot rest!";

  if (myCombatEntry.hasRested)
    throw "You have already rested this turn!";

  //zvednu si pocet restu o 1
  myCombatEntry.hasRested = true;

  //Pridam zaznam o restu do chatLogu
  _encounterData.addEntryToCombatLog("<color=\"yellow\">" + myCombatEntry.displayName + " rested!</color>");

  //neni konec kola?
  _encounterData.checkForEndOfTurn();

}

//2. nejvolanejsi funkce
//R - 2 (ten druhy je jen kvuli chater detection... ) 
//W - 1
exports.restEncounter = functions.https.onCall(async (data, context) => {

  const encounterUid = data.encounterUid;
  const callerCharacterUid = data.characterUid;

  console.log("callerCharacterUid :" + callerCharacterUid);

  const characterDb = admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);
  const encounterDb = admin.firestore().collection('encounters').doc(encounterUid).withConverter(encounterDocumentConverter);

  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {
      const encounterDoc = await t.get(encounterDb);
      let encounterData: EncounterDocument = encounterDoc.data();

      const characterDoc = await t.get(characterDb);
      let characterData: CharacterDocument = characterDoc.data();

      console.log("characterData.userUid :" + characterData.userUid);
      console.log("context.auth?.uid :" + context.auth?.uid);


      //Volas rest za jineho hrace!
      if (characterData.userUid != context.auth?.uid)
        throw "You cant rest for other players!";

      if (!encounterData.combatantList.includes(characterData.uid))
        throw "You are not joined to this ecnounter, you cant rest!";

      restEncounter(callerCharacterUid, encounterData);

      t.set(encounterDb, JSON.parse(JSON.stringify(encounterData)), { merge: true });

    });

    console.log('Transaction success', result);
    return result;
  } catch (e) {
    console.log('Transaction failure:', e);
    throw new functions.https.HttpsError("aborted", "Error : " + e);
  }
});

export async function joinCharacterToEncounter(_transaction: any, _encounterData: EncounterDocument, _callerCharacterData: CharacterDocument) {


  if (_encounterData.maxCombatants == _encounterData.combatantList.length)
    throw "Encounter is full!";

  if (_callerCharacterData.currency.fatigue >= 90)
    throw "You are too fatigued to go to battle! You need a rest!";

  //zaplatime time za join 
  //if (_encounterData.foundByCharacterUid != _callerCharacterData.uid)
  // _callerCharacterData.addFatigue(_encounterData.joinPrice);

  // if (_encounterData.encounterContext == ENCOUNTER_CONTEXT.DUNGEON) {

  //   if (_callerCharacterData.dungeonsFinished.includes(_encounterData.typeOfOrigin))
  //     throw "You have already completed this dungeon. Cant join the fight!";
  // }

  if (_encounterData.forbiddenCharacterUids.includes(_callerCharacterData.uid))
    throw "You've been traumatized by your past involvement in this encounter. You cant join the fight.";

  if (_encounterData.enemies.length == 0)
    throw "There are no enemies. Cant join encounter!";
  // if (_callerCharacterData.isJoinedInEncounter)
  if (await QuerryIfCharacterIsInCombatAtAnyEncounter(_transaction, _callerCharacterData.uid))
    throw "You are already in combat! Cant join new one!";
  if (await QuerryHasCharacterAnyUnclaimedEncounterResult(_transaction, _callerCharacterData))
    throw "You need to loot all corpses before joining another fight!";

  if (!compareWorldPosition(_callerCharacterData.position, _encounterData.position))
    throw "You need to travel to the position of encounter to join!";



  //Najdu jestli uz jsi joinuty do boje, pokud ne, pridam te
  if (!_encounterData.combatantList.includes(_callerCharacterData.uid)) {
    console.log("didnt find you in combat, adding you... ");
    _encounterData.combatantList.push(_callerCharacterData.uid);
    //zcheckneme esli nejsme full
    if (_encounterData.maxCombatants == _encounterData.combatantList.length) {

      //Vymazu vsechni lidi co nejsou joinuti do boje z watcher listu
      _encounterData.watchersList = [];
      _encounterData.combatantList.forEach(combatantUid => {
        _encounterData.watchersList.push(combatantUid);
      });

      _encounterData.isFull = true;
    }
  }
  else
    throw "You have already joined this encounter ! - " + _encounterData.uid;


  _encounterData.addEntryToCombatLog("<b>" + _callerCharacterData.characterName + " has joined the fight!</b>");

  //Liznu si pocatecni skilly
  let skillsInHand: Combatskill[] = [];
  let skillsDrawDeck: Combatskill[] = _callerCharacterData.converEquipToCombatSkills();// shuffleArray(_callerCharacterData.converEquipToCombatSkills());
  let skillsDiscard: Combatskill[] = [];
  let skillsExhaust: Combatskill[] = [];

  //uz si dokoncil tento tier na danem POI?
  let alreadyFinishedThisTier = false;

  let maxTierReached = _callerCharacterData.pointsOfInterestMaxTierReached.find(tierReached => tierReached.id == _encounterData.position.pointOfInterestId)
  if (maxTierReached) {
    if (maxTierReached.count + 1 == _encounterData.tier)
      alreadyFinishedThisTier = true;
  }
  //vytvorim sebe jako Combat Membera
  console.log("joining character to encounter: " + _callerCharacterData);
  let combatMember: CombatMember = new CombatMember(_callerCharacterData.uid, _callerCharacterData.characterName, _callerCharacterData.uid, _callerCharacterData.characterClass, skillsInHand, skillsDrawDeck, skillsDiscard, _callerCharacterData.converRareEffectsAndSkillBonusEffectsToCombatStats(), 0, false, _callerCharacterData.stats.level, 0, [], _callerCharacterData.characterPortrait, skillsExhaust, 0, 0, [], _callerCharacterData.blesses, alreadyFinishedThisTier);

  //modifikuju skilly pripadnyma statama co mam
  combatMember.addBonusFromEquipToSkills();

  //pokud si uz tenhle encounter tier dokoncil, dam te rovnou do perkchoices s null perkem abys byl jakoby hotovej s vyberem, ale zadny perk nedostanes
  // if (combatMember.hasAlreadyFinishedEncounterOfThisTier)
  //   _encounterData.perkChoices.push(new PerkChoiceParticipant(_callerCharacterData.uid, null, _callerCharacterData.characterPortrait, _callerCharacterData.characterClass, _callerCharacterData.characterName));

  //pridam tve stavajici curses co mas do decku
  _callerCharacterData.curses.forEach(curse => {
    skillsDrawDeck.push(curse);
  });


  // //Pridam nove curses z encounteru
  // if (_encounterData.curseCount > 0 || _callerCharacterData.curses.length > 0) {
  //   let skillDefinitions = await QuerryForSkillDefinitions(_transaction);


  //   //pridam tve stavajici curses
  //   _callerCharacterData.curses.forEach(curse => {
  //     skillsDrawDeck.push(curse);
  //   });

  //   //pridam nove curse z encounteru
  //   for (let index = 0; index < _encounterData.curseCount; index++) {
  //     let curse = getRandomCurseAsCombatSkill(_callerCharacterData, skillDefinitions, 0);
  //     skillsDrawDeck.push(curse);
  //     _callerCharacterData.addCurse(curse);
  //   }

  // }

  //pridam te do combatantu
  _encounterData.combatants.push(combatMember);


  //aplikuju vsechny existujici perky na nove joinujiciho se hrace
  _encounterData.perksOffers.forEach(perkOffer => {
    perkOffer.specialEffectId.forEach(effectID => {
      switch (effectID.id) {
        case PERK_SPECIAL_EFFECT.INCREASE_MANA_OF_ALL_SKILLS:
          {
            skillsDrawDeck.forEach(skill => {
              skill.originalStats.manaCost += effectID.count;
            });
            break;
          }

        default:
          break;
      }
    });
  });

  // //zamicham si draw deck;
  shuffleArray(skillsDrawDeck);

  //Drawnu pocatecni skilly
  drawNewSkills(skillsInHand, skillsDrawDeck, skillsDiscard, _callerCharacterData.stats.skillDrawCount);

}

exports.joinEncounter = functions.https.onCall(async (data, context) => {


  const encounterUid = data.encounterUid;
  const callerCharacterUid = data.characterUid;

  if (encounterUid == "")
    throw "Not valid encounter Id ! - " + encounterUid;

  if (callerCharacterUid == "")
    throw "Not valid character Id ! - " + callerCharacterUid;

  const encounterDb = admin.firestore().collection('encounters').doc(encounterUid).withConverter(encounterDocumentConverter);
  const callerCharacterDb = admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);


  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {


      const encounterDoc = await t.get(encounterDb);
      const callerCharacterDoc = await t.get(callerCharacterDb);

      let encounterData: EncounterDocument = encounterDoc.data();
      let callerCharacterData: CharacterDocument = callerCharacterDoc.data();
      validateCallerBulletProof(callerCharacterData, context);

      // if (!encounterData.hasEveryoneChoosenHisPerk())
      //   throw "Not everyone has choosen his perk ,yet. Cant join encounter";

      await joinCharacterToEncounter(t, encounterData, callerCharacterData);

      encounterData.combatants[0].skillsDrawDeck.forEach(element => {
        if (element.amounts.length > 0)
          console.log(element.skillId + " amount:" + element.amounts[0])
      });
      // encounterData.combatants.forEach(combatant => {

      //   combatant.skillsDrawDeck.forEach(element => {
      //     if (element.skillGroupId == SKILL_GROUP.LIGHTNING)
      //       console.log("element.amounts[0]: " + element.amounts[0])
      //   });
      // });
      t.set(encounterDb, JSON.parse(JSON.stringify(encounterData)), { merge: true });
      t.set(callerCharacterDb, JSON.parse(JSON.stringify(callerCharacterData)), { merge: true });

    });


    console.log('Transaction success', result);
    return result;
  } catch (e) {
    console.log('Transaction failure:', e);
    throw new functions.https.HttpsError("aborted", "Error : " + e);
  }

});


exports.addSelfAsWatcher = functions.https.onCall(async (data, context) => {


  var characterUid = data.characterUid;
  var encounterUid = data.encounterUid;

  console.log('encounterUid', encounterUid);
  const encounterDb = admin.firestore().collection('encounters').doc(encounterUid).withConverter(encounterDocumentConverter);

  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {

      const encounterDoc = await t.get(encounterDb);
      let encounterData: EncounterDocument = encounterDoc.data();

      //pokud nejsem uz watcher pridam se
      if (encounterData.watchersList.includes(characterUid))
        throw ("Nemuzu te pridat. Uz jsi watcher na encounterId: " + encounterUid);

      encounterData.watchersList.push(characterUid);
      t.set(encounterDb, JSON.parse(JSON.stringify(encounterData)), { merge: true }); //Musim pouzit to stringyfy protoze jinak nemuzu nikde v kodu pouzivat "new Class", hlasi to firebase error....prej to koruptije trimestampy...
      return 'Pridal sem te jako Wathcera na encounter ' + encounterUid + ' : Transaction success!';
    });

    console.log('Transaction success', result);
    return result;
  } catch (e) {
    console.log('Transaction failure:', e);
    throw new functions.https.HttpsError("aborted", "Error : " + e);
  }




});




exports.chooseEncounterPerkOffer = functions.https.onCall(async (data, context) => {

  const callerCharacterUid = data.characterUid;
  const perkOfferUid = data.perkOfferUid;
  const encounterUid = data.encounterUid;
  //  const lootSpotId = data.lootSpotId;

  const characterDb = admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);
  const encounterDb = admin.firestore().collection('encounters').doc(encounterUid);

  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {



      const characterDoc = await t.get(characterDb);
      let characterData: CharacterDocument = characterDoc.data();

      // if (await QuerryIfCharacterIsInCombatAtAnyEncounter(t, characterData.uid))
      //   throw ("You cannot explore while in combat!");

      const encounterDoc = await t.get(encounterDb.withConverter(encounterDocumentConverter));
      let encounterData: EncounterDocument = encounterDoc.data();

      // if (encounterData.perkChoices.find(item => item.characterUid))
      //   throw ("Perk is already choosen!");

      //zkontroluju ze je to muj encounter, ja jediny vybiram sam sobe perk
      if (!encounterData.combatantList.includes(callerCharacterUid))
        throw ("You need to join the encounter to be able to choose perk!");

      if (encounterData.perkChoices.find(choice => choice.choosenPerk != null && choice.choosenPerk.uid == perkOfferUid)) //uz nekdo vybrat tento perk UID!
        throw "Other player already choosen this perk! Choose different one!";

      if (encounterData.perkChoices.find(choice => choice.characterUid == callerCharacterUid)) //ty uz sis vybiral perk!
        throw "You have already choosen perk!";


      let choosenPerk = encounterData.perksOffers.find(perk => perk.uid == perkOfferUid);

      if (choosenPerk) //je to tedy obyc perk offer
      {

      }
      else //v perk offerech encounteru sem ho nenasel, musi to byt teda rare perk flooru
      {
        choosenPerk = encounterData.perksOffersRare.find(perk => perk.uid == perkOfferUid);

        if (choosenPerk) {

        }
        else
          throw "Could not find per with UID : " + perkOfferUid + " among perks offered in this encounter!";
      }

      if (choosenPerk.stockLeft - choosenPerk.stockClaimed == 0)
        throw ("This perk is out of stock!");

      encounterData.perkChoices.push(new PerkChoiceParticipant(callerCharacterUid, choosenPerk, characterData.characterPortrait, characterData.characterClass, characterData.characterName));

      //Zaplatim explore time 
      // characterData.subCurrency(CURRENCY_ID.TIME, encounterData.perksOffers[0].timePrice);

      //dame curse
      // encounterData.curseCount += choosenPerk.curseCount;



      // //Pridam nove curses z encounteru
      // if (_encounterData.curseCount > 0 || _callerCharacterData.curses.length > 0) {
      //   let skillDefinitions = await QuerryForSkillDefinitions(_transaction);


      //   //pridam tve stavajici curses
      //   _callerCharacterData.curses.forEach(curse => {
      //     skillsDrawDeck.push(curse);
      //   });

      //pridam nove curse z encounteru
      if (choosenPerk.curseCount > 0) {
        let skillDefinitions = await QuerryForSkillDefinitions(t);
        for (let index = 0; index < choosenPerk.curseCount; index++) {
          let curse = getRandomCurseAsCombatSkill(characterData, skillDefinitions, 0);
          encounterData.getCombatMemberByUid(characterData.uid)?.skillsDrawDeck.push(curse);
          characterData.addCurse(curse);
        }
      }

      // }


      //dame special effekty
      for (const specialEffect of choosenPerk.specialEffectId) {

        switch (specialEffect.id) {
          case PERK_SPECIAL_EFFECT.ENEMY_ALL_ADD_HEALTH:
            {
              for (const enemy of encounterData.enemies) {
                // console.log("enemy:" + enemy.enemyId);
                enemy.stats.healthMax += specialEffect.count;
                enemy.giveHealth(specialEffect.count);
              }
              break;
            }
          case PERK_SPECIAL_EFFECT.ENEMY_RANDOM_ADD_HEALTH:
            {

              let enemy = encounterData.enemies[randomIntFromInterval(0, encounterData.enemies.length - 1)];

              enemy.stats.healthMax += specialEffect.count;
              enemy.giveHealth(specialEffect.count);

              break;
            }
          case PERK_SPECIAL_EFFECT.DUPLICATE_ENEMY:
            {
              for (let index = 0; index < specialEffect.count; index++) {
                let enemy = encounterData.enemies[randomIntFromInterval(0, encounterData.enemies.length - 1)];

                const locationEnemyDefinitionsDb = admin.firestore().collection('_metadata_zones').doc(characterData.position.zoneId).collection("locations").doc(characterData.position.locationId).collection("definitions").doc("ENEMIES");
                var zonesEnemyDefinitionsStatsDoc = await t.get(locationEnemyDefinitionsDb.withConverter(EnemyDefinitionsConverter));
                var zonesEnemyDefinitionsStatsData: EnemyDefinitions = zonesEnemyDefinitionsStatsDoc.data();

                encounterData.enemies.push(createCombatEnemyFromDefinition(enemy.enemyId, zonesEnemyDefinitionsStatsData));
              }

              break;
            }
          case PERK_SPECIAL_EFFECT.INCREASE_MANA_OF_ALL_SKILLS:
            {
              encounterData.combatants.forEach(combatant => {
                combatant.skillsDrawDeck.forEach(skill => { skill.originalStats.manaCost += specialEffect.count; });
                combatant.skillsInHand.forEach(skill => { skill.manaCost += specialEffect.count; }); //protoze combatanti uz maji drawnute skilly, v ruce, ktere nemaji zvysenou manu, musim manualne i je
                combatant.skillsInHand.forEach(skill => { skill.originalStats.manaCost += specialEffect.count; });
                combatant.skillsDiscardDeck.forEach(skill => { skill.originalStats.manaCost += specialEffect.count; });
                combatant.skillsExhaustDeck.forEach(skill => { skill.originalStats.manaCost += specialEffect.count; });


              });
              break;
            }
          default:
            break;
        }
      }

      t.set(encounterDb, JSON.parse(JSON.stringify(encounterData)), { merge: true });


      //Jen kvuli tomu ze explore bere explore time musim udelat dalsi save do db....
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

exports.exploreDungeon = functions.https.onCall(async (data, context) => {


  const callerCharacterUid = data.characterUid;
  //const pointOfInterestId = data.pointOfInterestId;

  const encountersDb = admin.firestore().collection('encounters');
  //const myPartyDb = admin.firestore().collection('parties').where("partyMembersUidList", "array-contains", callerCharacterUid);
  const characterDb = admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);

  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {

      const characterDoc = await t.get(characterDb);
      let characterData: CharacterDocument = characterDoc.data();
      validateCallerBulletProof(characterData, context);
      let myPartyData = await QuerryForParty(t, characterData.uid);

      if (myPartyData == null)
        throw "You are not in party. You cant explore dungeon!";

      if (myPartyData.dungeonProgress == null)
        throw "You have not opened any dungeon. You cant explore dungeon!";

      if (myPartyData.partyLeaderUid != characterData.uid)
        throw "Only party leader can explore a dungeon!";

      const allCallerDungeonEncounterOnHisPosition = admin.firestore().collection('encounters').where("position.zoneId", "==", characterData.position.zoneId).where("position.locationId", "==", characterData.position.locationId).where("position.pointOfInterestId", "==", characterData.position.pointOfInterestId).where("foundByCharacterUid", "==", myPartyData.uid).where("encounterContext", "==", ENCOUNTER_CONTEXT.DUNGEON);
      const locationDb = admin.firestore().collection('_metadata_zones').doc(characterData.position.zoneId).collection("locations").doc(characterData.position.locationId);//.withConverter(LocationConverter);
      const locationEnemyDefinitionsDb = locationDb.collection("definitions").doc("ENEMIES");


      // const locationLootSpotDefinitionsDb = locationDb.collection("definitions").doc("LOOT_SPOTS");
      const pointOfInterestDb = locationDb.collection("pointsOfInterest").doc(characterData.position.pointOfInterestId);
      const pointOfInterestServerDataDefinitionsDb = locationDb.collection("pointsOfInterest").doc(characterData.position.pointOfInterestId).collection("definitions").doc("SERVER_DATA");

      //nactu si data o PoI
      var pointOfInterestDoc = await t.get(pointOfInterestDb);
      const pointOfInterestData: PointOfInterest = pointOfInterestDoc.data();

      if (myPartyData.dungeonProgress.dungeonId != pointOfInterestData.typeId)
        throw "You have already opened a different dungeon then the one you are trying to explore!";


      //const pointOfInterest = locationsMetaData.getPointOfInterestById(pointOfInterestId);

      //nactu si info o lokaci...(pro doplneni rare perku...)
      // var locationDoc = await t.get(locationDb.withConverter(LocationConverter));
      // var locationData: MapLocation = locationDoc.data();

      //nactu si definice o Enemy stats
      var zonesEnemyDefinitionsStatsDoc = await t.get(locationEnemyDefinitionsDb.withConverter(EnemyDefinitionsConverter));
      var zonesEnemyDefinitionsStatsData: EnemyDefinitions = zonesEnemyDefinitionsStatsDoc.data();

      //nactu si definice o Server datech
      let pointOfInterestServerDataDefinitionsDoc = await t.get(pointOfInterestServerDataDefinitionsDb.withConverter(PointOfInterestServerDataDefinitionsConverter));
      let pointOfInterestServerDataDefinitionsData: PointOfInterestServerDataDefinitions = pointOfInterestServerDataDefinitionsDoc.data();



      if (pointOfInterestData.dungeon == null)
        throw ("Database error : you are trying to explore a dungeon where there are no data about dungeon!!");

      if (pointOfInterestServerDataDefinitionsData.dungeon == null)
        throw ("Database error : you are trying to explore a dungeon where there are no data about dungeon!");

      if (myPartyData.dungeonProgress.tierReached + 1 > pointOfInterestData.dungeon.floorsTotal)
        throw ("There are no more floors to explore at this dungeon. You have reached max floor :" + myPartyData.dungeonProgress.tierReached + " there are : " + pointOfInterestData.dungeon.floorsTotal + " floors");

      let pointOfInterestNextTierData = pointOfInterestServerDataDefinitionsData.dungeon.tiers[myPartyData.dungeonProgress.tierReached];


      await t.get(allCallerDungeonEncounterOnHisPosition).then(querry => {
        if (querry.size > 0) {
          throw "You already have some other dungeon encounter active! ";
        }

      });

      //musim mut dungeon exlored
      if (!characterData.hasExploredPosition(pointOfInterestData.worldPosition))
        throw "You need to explore the dungeon position first!";


      //vyrobime encounter
      var enemies: CombatEnemy[] = [];
      var combatants: CombatMember[] = [];
      var combatantList: string[] = [];
      var watchersList: string[] = [];
      const expireDate = getCurrentDateTime(2);
      var maxCombatants: number = pointOfInterestData.dungeon?.partySize;
      var isFull: boolean = false;

      myPartyData.partyMembers.forEach(partyMember => { watchersList.push(partyMember.uid); });

      //naplnime enemy podle toho jaci jsou v danem tieru
      for (const enemy of pointOfInterestNextTierData.enemies) { enemies.push(createCombatEnemyFromDefinition(enemy, zonesEnemyDefinitionsStatsData)); }

      let dungeonEncounter = new EncounterDocument(encountersDb.doc().id, enemies, combatants, combatantList, Math.random(), expireDate, myPartyData.uid, maxCombatants, watchersList, isFull, "Party", ENCOUNTER_CONTEXT.DUNGEON, characterData.position, 1, "Enemies in sight!\n", "0", [], [], 0, [], [], 0, [], [], pointOfInterestData.typeId, myPartyData.dungeonProgress.tierReached + 1);

      t.set(encountersDb.doc(dungeonEncounter.uid), JSON.parse(JSON.stringify(dungeonEncounter)), { merge: true });



      //Jen kvuli tomu ze explore bere explore time musim udelat dalsi save do db....
      // t.set(characterDb, JSON.parse(JSON.stringify(characterData)), { merge: true });


      return "OK";
    });


    console.log('Transaction success', result);
    return result;
  } catch (e) {
    console.log('Transaction failure:', e);
    throw new functions.https.HttpsError("aborted", "Error : " + e);
  }

});


exports.exploreMonsters = functions.https.onCall(async (data, context) => {

  //tady teda vybrat co bude explornuto...enemy nebo gathering resource pro ted nebo nic?
  //const MAX_ENEMIES = 6;
  // const CHANCE_FOR_BONUS_DROP_BASE = 0.5;
  // const CHANCE_FOR_BONUS_DROP_PENALTY_PER_BONUS_ITEM = 0.2;
  // const CHANCE_FOR_BONUS_DROP_BONUS_PER_ENEMY = 0.2;

  const callerCharacterUid = data.characterUid;
  //const pointOfInterestId = data.pointOfInterestId;

  const encounterDb = admin.firestore().collection('encounters');
  // const myPartyDb = admin.firestore().collection('parties').where("partyMembersUidList", "array-contains", callerCharacterUid);
  const characterDb = admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);
  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {
      const characterDoc = await t.get(characterDb);
      let characterData: CharacterDocument = characterDoc.data();
      validateCallerBulletProof(characterData, context);
      const locationDb = admin.firestore().collection('_metadata_zones').doc(characterData.position.zoneId).collection("locations").doc(characterData.position.locationId);//.withConverter(LocationConverter);
      const locationEnemyDefinitionsDb = locationDb.collection("definitions").doc("ENEMIES");

      // const locationLootSpotDefinitionsDb = locationDb.collection("definitions").doc("LOOT_SPOTS");
      const pointOfInterestDb = locationDb.collection("pointsOfInterest").doc(characterData.position.pointOfInterestId);
      const pointOfInterestServerDataDefinitionsDb = locationDb.collection("pointsOfInterest").doc(characterData.position.pointOfInterestId).collection("definitions").doc("SERVER_DATA");
      //nactu si data o PoI
      var pointOfInterestDoc = await t.get(pointOfInterestDb);
      const pointOfInterestData: PointOfInterest = pointOfInterestDoc.data();
      //const pointOfInterest = locationsMetaData.getPointOfInterestById(pointOfInterestId);

      //nactu si info o lokaci...(pro doplneni rare perku...)
      var locationDoc = await t.get(locationDb.withConverter(LocationConverter));
      var locationData: MapLocation = locationDoc.data();
      //nactu si definice o Enemy stats
      var zonesEnemyDefinitionsStatsDoc = await t.get(locationEnemyDefinitionsDb.withConverter(EnemyDefinitionsConverter));
      var zonesEnemyDefinitionsStatsData: EnemyDefinitions = zonesEnemyDefinitionsStatsDoc.data();
      //nactu si definice o Loot spotech
      // let zonesLootSpotDefinitionsDoc = await t.get(locationLootSpotDefinitionsDb.withConverter(LootSpotDefinitionsConverter));
      // let zonesLootSpotDefinitionsData: LootSpotDefinitions = zonesLootSpotDefinitionsDoc.data();

      //nactu si definice o Tierech
      let pointOfInterestServerDataDefinitionsDoc = await t.get(pointOfInterestServerDataDefinitionsDb.withConverter(PointOfInterestServerDataDefinitionsConverter));
      let pointOfInterestServerDataDefinitionsData: PointOfInterestServerDataDefinitions = pointOfInterestServerDataDefinitionsDoc.data();

      let nextTierNumber = characterData.getMaxTierReachedForPointOfInterest(characterData.position.pointOfInterestId) + 1

      if (pointOfInterestData.monsters == null)
        throw "Database error: there are no monsters public data defined on this Point of interesst!";

      if (pointOfInterestServerDataDefinitionsData.monsters == null)
        throw "Database error: there are no monsters server data defined on this Point of interesst!";

      if (pointOfInterestData.monsters.tiersTotal <= nextTierNumber)
        throw ("There are no more tiers to explore on this point of interest. You have reached max tier :" + nextTierNumber);

      let pointOfInterestNextTierData = pointOfInterestServerDataDefinitionsData.monsters.tiers[nextTierNumber];


      // if (await QuerryIfCharacterIsInCombatAtAnyEncounter(t, characterData.uid))
      //   throw ("You cannot explore while in combat!");

      if (await QuerryIfCharacterIsWatcherInAnyEncounterOnHisPosition(t, characterData))
        throw ("You have already some encounter on your position, cant explore more!");

      if (await QuerryHasCharacterAnyUnclaimedEncounterResult(t, characterData))
        throw ("You must loot all corpses before exploring!");

      // if (await QuerryIfCharacterIsWatcherInAnyEncounterOnHisPosition(t, characterData))
      //   throw ("Enemies are nearby. Cant explore!!");
      let founderId: string = characterData.uid;
      let foundBy: string = characterData.characterName;

      let myParty: Party | null = null;
      //je to MP encounter, musis byt v parte pred explorem
      if (pointOfInterestData.monsters.partySize > 1) {
        myParty = await QuerryForParty(t, characterData.uid);
        if (myParty == null)
          throw ("You need to be in party to explore this encounter.")
        else {
          founderId = myParty.uid;
          foundBy = "Party";
        }

      }

      //zkontroluje jestli uz mas personal encounter vytvoreny
      let personalEncounter: EncounterDocument | undefined;
      // let rareEncounter: EncounterDocument | undefined;

      // let callerMonsterEncountersOnThisPosition = admin.firestore().collection('encounters').where("position.zoneId", "==", characterData.position.zoneId).where("position.locationId", "==", characterData.position.locationId).where("position.pointOfInterestId", "==", characterData.position.pointOfInterestId).where("foundByCharacterUid", "==", founderId).where("encounterContext", "==", ENCOUNTER_CONTEXT.PERSONAL);


      // await t.get(callerMonsterEncountersOnThisPosition).then(querry => {
      //   if (querry.size > 1) {
      //     throw "How can you have more than 1 monster encounter! Database error!";
      //   }
      //   querry.docs.forEach(doc => {
      //     personalEncounter = doc.data();
      //   });
      // });

      // if (personalEncounter != undefined) {
      //   // if (personalEncounter.enemies.length == MAX_ENEMIES)
      //   //   throw "You cant fight with more than" + MAX_ENEMIES + " enemies";

      //   if (personalEncounter.combatantList.length > 0) // jen pokud jeste nikdo neni v combatu! nechceme tam pridavat enemy kdyz uz se bojuje!
      //     throw "Someone is already attacking encounter found by you! Cant explore more enemies!";

      // }

      //pokud jeste nemam personal encounter a lokaci si uz prozkoumal, vytvorim novy encounter
      if (personalEncounter == undefined && characterData.hasExploredPosition(pointOfInterestData.worldPosition)) {

        var combatants: CombatMember[] = [];//combatants.push(new CombatMember(characterData.characterName, characterData.uid, characterData.characterClass, [], characterData.converSkillsToCombatSkills(), [], characterData.converStatsToCombatStats(), 0, 0, characterData.stats.level));
        var combatantList: string[] = []; //combatantList.push(callerCharacterUid);
        var watchersList: string[] = []; watchersList.push(callerCharacterUid);

        //naplnime enemy podle toho jaci jsou v danem tieru
        var enemies: CombatEnemy[] = [];
        for (const enemy of pointOfInterestNextTierData.enemies) {
          enemies.push(createCombatEnemyFromDefinition(enemy, zonesEnemyDefinitionsStatsData));
        }
        const expireDate = getCurrentDateTime(2);
        var maxCombatants: number = pointOfInterestData.monsters.partySize;
        var isFull: boolean = false;//maxCombatants <= 0;
        //const position = new WorldPosition(zoneId, locationId);

        // let perkOffers: PerkOfferDefinition[] = pointOfInterestNextTierData.perkOffers;
        // // pointOfInterestTierDefinitionsData.takeAChanceForARarePerk(pointOfInterestNextTierData, CHANCE_OF_DRAWING_RARE_PERK_OFFER);

        // //doplnime rare perky tak aby byly vzdy 3 perky na vyber
        // let numberOfPerksToFill = 3 - pointOfInterestNextTierData.perkOffers.length;

        // for (let i = 0; i < numberOfPerksToFill; i++) {
        //   pointOfInterestTierDefinitionsData.takeAChanceForARarePerk(pointOfInterestNextTierData, 1);
        // }

        let rarePerks: PerkOfferDefinition[] = [];
        let rarePerksFound = locationData.perksRareOffers.find(item => item.id == pointOfInterestServerDataDefinitionsData.monsters!.perkOffersRareId)?.perks;
        if (rarePerksFound) {
          rarePerks = rarePerksFound;
          rarePerks = rollForRandomItems(rarePerks, nextTierNumber + 3) as PerkOfferDefinition[];
        }

        let tierPerks: PerkOfferDefinition[] = pointOfInterestNextTierData.perkOffers;
        //  let firstPerk = pointOfInterestNextTierData.perkOffers[0]; //ulozim si prvni perk, ten chci mit vzdy vylosovany
        //  pointOfInterestNextTierData.perkOffers[0].chanceToSpawn = 0; //nastavim mu sanci na spawn na 0 aby nebyl vybrani znova nahodne
        //rollForRandomItems(pointOfInterestNextTierData.perkOffers, pointOfInterestData.monsters.partySize) as PerkOfferDefinition[];
        //tierPerks[0] = firstPerk;//a rucne ho nastavim jako prvni perk
        personalEncounter = new EncounterDocument(encounterDb.doc().id, enemies, combatants, combatantList, Math.random(), expireDate, founderId, maxCombatants, watchersList, isFull, foundBy, ENCOUNTER_CONTEXT.PERSONAL, characterData.position, 1, "Enemies in sight!\n", "0", [], [], 0, tierPerks, [], pointOfInterestNextTierData.entryTimePrice, rarePerks, [], pointOfInterestData.typeId, nextTierNumber);


        // //ziskam svoju partu
        // let myPartyData: Party = new Party("", "", 0, [], [], null);
        // await t.get(myPartyDb).then(querry => {
        //   if (querry.size > 1)
        //     throw ("You are in more than 1 party! Database error!");
        //   querry.docs.forEach(doc => {
        //     myPartyData = doc.data();
        //   });
        // });
        // pokud je to MP encounter pridam pripadne vsechny party membery do encounteru
        if (myParty != null && pointOfInterestData.monsters.partySize > 1)
          myParty.partyMembersUidList.forEach(partyMemberUid => {
            if (!personalEncounter!.watchersList.includes(partyMemberUid))
              personalEncounter!.watchersList.push(partyMemberUid);
          });

      }
      //  else
      //  throw "You already has created your encounter on this point of interest!";


      // Ulozime ze si prozkoumal tuto lokaci pokud nebyla neprozkoumana
      // pokud je to normalni PoI tak do charakteru
      // if (pointOfInterestData.pointOfInterestType == POI_TYPE.ENCOUNTER || pointOfInterestData.pointOfInterestType == POI_TYPE.TOWN) {

      //   if (!characterData.exploredPositions.includes(pointOfInterestData.worldPosition))
      //     characterData.exploredPositions.push(pointOfInterestData.worldPosition);

      //updatneme memmoryMapu - pokazde kdyz exploruju se updatuje, a updatuje se i kdyz cestuju....je to zdanlive tady zbytecne ale je to tu kvuli tomu  ze kdyz zabiju guardiany tak pak dam poprve explore...nemuzu updatnout memory v kdyz claimuju result protoze neznam PointOfInterest...
      // characterData.updateMemmoryMap(pointOfInterestData);

      //    }

      if (personalEncounter != undefined)
        t.set(encounterDb.doc(personalEncounter.uid), JSON.parse(JSON.stringify(personalEncounter)), { merge: true });

      // if (rareEncounter != undefined)
      //   t.set(encounterDb.doc(rareEncounter.uid), JSON.parse(JSON.stringify(rareEncounter)), { merge: true });


      //Jen kvuli tomu ze explore bere explore time musim udelat dalsi save do db....
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


exports.fleeFromEncounter = functions.https.onCall(async (data, context) => {


  const encounterUid = data.encounterUid;
  const callerCharacterUid = data.characterUid;

  if (encounterUid == "")
    throw "Not valid encounter Id ! - " + encounterUid;

  if (callerCharacterUid == "")
    throw "Not valid character Id ! - " + callerCharacterUid;

  const encounterDb = admin.firestore().collection('encounters').doc(encounterUid).withConverter(encounterDocumentConverter);
  const callerCharacterDb = admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);

  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {


      const encounterDoc = await t.get(encounterDb);
      const callerCharacterDoc = await t.get(callerCharacterDb);

      let encounterData: EncounterDocument = encounterDoc.data();
      let callerCharacterData: CharacterDocument = callerCharacterDoc.data();

      let myPartyData = await QuerryForParty(t, callerCharacterData.uid);

      //dame penalty +  vsechen rest fatigue + health z boje prenesem
      let callerCombatMemberEntry = encounterData.getCombatMemberByUid(callerCharacterUid)

      if (callerCombatMemberEntry == null)
        throw "Cant find you int combat!";

      if (!callerCharacterData.hasBless(BLESS.UNWEARIED))
        callerCharacterData.addFatigue(callerCombatMemberEntry.deckShuffleCount * DECK_SHUFFLE_FATIGUE_PENALTY + FLEE_FATIGUE_PENALTY);

      if (callerCharacterData.hasBless(BLESS.BEHEMOND)) {
        //
        let skillDefinitions = await QuerryForSkillDefinitions(t);
        let curse = getRandomCurseAsCombatSkill(callerCharacterData, skillDefinitions, 0);
        callerCharacterData.addCurse(curse);

      }

      callerCharacterData.stats.currentHealth = callerCombatMemberEntry.stats.leastHealth;



      //encounter z ktereho utikam jsem vytvoril ja sam, je to tedy muj personal encounter,takze ho muzu:
      //1) but smazat a lidem co bojujou uplne posrat boj, 
      //2) nebo musim cekat nez dobojujou a pak az muzu vytvaret dalsi sve personal encountery
      //*3) (to sem mi zda ted best) nenapadne se smazu jako Uid co nasel tento encounter, tim si otevru moznost hledat dalsi personal encountery a oni at si bojujou ten muj stary 
      if (encounterData.foundByCharacterUid == callerCharacterUid && (encounterData.encounterContext == ENCOUNTER_CONTEXT.PERSONAL || encounterData.encounterContext == ENCOUNTER_CONTEXT.WORLD_BOSS)) {
        encounterData.foundByCharacterUid = "Founder fled the combat...";
      }


      encounterData.removeCharacterFromCombat(callerCharacterUid);

      encounterData.checkForEndOfTurn();


      // //pokud si byl v dungeonu, tak zustanes na PoI ale zakazu ti se znova pripojit do encouneru podruhe.
      // if (myPartyData != null && myPartyData.dungeonProgress != null) {
      //   encounterData.forbiddenCharacterUids.push(callerCharacterData.uid);
      // }
      // else {
      //   //pokud nejsi v dungeonu tak proste jen bez na svuj inn
      //   callerCharacterData.position = callerCharacterData.homeInn;
      //   await UpdateCharacterLocationInHisParty(t, callerCharacterData.uid, callerCharacterData.homeInn, true);

      // }



      //  t.set(callerCharacterDb, JSON.parse(JSON.stringify(callerCharacterData)), { merge: true });

      if (myPartyData != null && myPartyData.dungeonProgress != null) {

        //pokud si byl v dungeonu, tak zustanes na PoI ale zakazu ti se znova pripojit do encouneru podruhe.
        encounterData.forbiddenCharacterUids.push(callerCharacterData.uid);

        //pokud si utekl z endless dungeonu tak zazancim ze je teda finished, abys tam nemohl jit znova...
        if (myPartyData.dungeonProgress.isEndlessDungeon) {
          if (!callerCharacterData.dungeonsFinished.includes(myPartyData.dungeonProgress.dungeonId)) {
            callerCharacterData.dungeonsFinished.push(myPartyData.dungeonProgress.dungeonId);
          }
          //pokud je to final dungeon tak retirneme charakter
          if (myPartyData.dungeonProgress.isFinalDungeon) {
            //locknout charakter, mame hotovo, dohrano
            console.log("Jdeme retirnout charakter, fleenul si z final dungu");










            ////////NEUMIM UDELAT Z RETIRE FUNKCI PROTO  MUSIM DUPLIKOVAT REITRE TADY? FIX THIS

            const batch = admin.firestore().batch();
            const playerDb = admin.firestore().collection("players").doc(callerCharacterData.userUid);
            const playerDoc = await t.get(playerDb);
            let playerData: PlayerData = playerDoc.data();

            validateCallerBulletProof(callerCharacterData, context);

            for (const inventoryItem of callerCharacterData.inventory.content) {
              if (inventoryItem.getItem().rarity == RARITY.ARTIFACT) {
                if (!inventoryItem.content)
                  continue;
                const inboxDb = admin.firestore().collection('inboxPlayer').doc();
                const inboxEntry = new InboxItem(inboxDb.id, callerCharacterData.userUid, generateContentContainer(generateContent(inventoryItem.content.itemId, inventoryItem.content.amount)), "Retired hero belongings", "Here are belongings of your retired hero " + callerCharacterData.characterName, getCurrentDateTime(480));
                batch.set(inboxDb, JSON.parse(JSON.stringify(inboxEntry))); // Update the document in batch

                //aspon nastavim amout na 0 kdyby nahodou se neco stalo a ten charakter byl pristupny at neduplikuju veci....lepsi by to bylo uplne smazat ale to se mi ted nechce kodovat
                inventoryItem.getItem().amount = 0;
              }
            }

            //updatneme preview na retired
            playerData.characters.find(character => character.characterUid == callerCharacterData.uid)!.isRetired = true;

            //odstranim charakter z playera, takze ztrati link na nej ale v DB zustane
            // playerData.characters.splice(playerData.characters.indexOf(playerData.characters.find(character => character.characterUid == callerCharacterData.uid)!), 1);

            callerCharacterData.isRetired = true;

            //   await t.set(retiredcharacterDb.doc(), JSON.parse(JSON.stringify(characterData)), { merge: true });

            await t.set(playerDb, JSON.parse(JSON.stringify(playerData)), { merge: true });

            await t.set(callerCharacterDb, JSON.parse(JSON.stringify(callerCharacterData)), { merge: true });

            // await t.delete(characterDb);


            await batch.commit()
              .then(() => {
                console.log("Batch update completed.");
              })
              .catch((e) => {
                console.error("Batch update failed: ", e);
                throw new functions.https.HttpsError("aborted", "Batch update failed: " + e);
              });







          }

          //      t.set(callerCharacterDb, JSON.parse(JSON.stringify(callerCharacterData)), { merge: true });
        }
      }
      else if (callerCombatMemberEntry.stats.health == 0) {

        //dam ti random curse
        let skillDefinitions = await QuerryForSkillDefinitions(t);
        let curse = getRandomCurseAsCombatSkill(callerCharacterData, skillDefinitions, 0);
        callerCharacterData.addCurse(curse);

        //pokud nejsi v dungeonu tak pokud jsi byl mrtvy bez na svuj inn a dam ti 1HP
        callerCharacterData.stats.currentHealth = 1;
        callerCharacterData.position = callerCharacterData.homeInn;
        await UpdateCharacterLocationInHisParty(t, callerCharacterData.uid, callerCharacterData.homeInn, true);

      }


      //pokud jsem poslednu hrac co fleenul a nikdo tam uz neni, tak smazu ten encounter cely
      if (encounterData.combatantList.length == 0) {
        //vylecim vsechny enemy co nejsou mrtvi na 100% a smazu buffy
        encounterData.enemies.forEach(enemy => {
          if (enemy.stats.health > 0) {
            enemy.stats.health = enemy.stats.healthMax;
            enemy.buffs = [];
          }
        });
      }

      //pokud jsem poslednu hrac co fleenul a nikdo tam uz neni, tak smazu ten encounter cely
      /* if (encounterData.combatantList.length == 0) {
         //u personal accountu kdyz tam nikdo neni tak ho jen smazu
         if (encounterData.encounterContext == ENCOUNTER_CONTEXT.PERSONAL || encounterData.encounterContext == ENCOUNTER_CONTEXT.WORLD_BOSS) {
           t.delete(encounterDb);
         }
         //u dungeonu jeste zrusim cely dungeon progress , proste jste prohrali, vseci zdrhli
         else if (encounterData.encounterContext == ENCOUNTER_CONTEXT.DUNGEON) {
           if (myPartyData != null && myPartyData.dungeonProgress != null) {
 
             // if (myPartyData.dungeonProgress.isFinalDungeon) {
             //   //locknout charakter, mame hotovo, dohrano
             // }
 
             myPartyData.dungeonProgress = null;
             myPartyData.dungeonEnterConsents = [];
 
             t.set(admin.firestore().collection('parties').doc(myPartyData.uid), JSON.parse(JSON.stringify(myPartyData)), { merge: true });
 
           }
 
           t.delete(encounterDb);
         }
       }
       else {
         t.set(encounterDb, JSON.parse(JSON.stringify(encounterData)), { merge: true });
       }
 */
      callerCharacterData.recalculateCharacterStats();

      t.set(encounterDb, JSON.parse(JSON.stringify(encounterData)), { merge: true });

      t.set(callerCharacterDb, JSON.parse(JSON.stringify(callerCharacterData)), { merge: true });

    });


    console.log('Transaction success', result);
    return result;
  } catch (e) {
    console.log('Transaction failure:', e);
    throw new functions.https.HttpsError("aborted", "Error : " + e);
  }

});



exports.retreatFromEncounter = functions.https.onCall(async (data, context) => {


  const encounterUid = data.encounterUid;
  const callerCharacterUid = data.characterUid;

  if (encounterUid == "")
    throw "Not valid encounter Id ! - " + encounterUid;

  if (callerCharacterUid == "")
    throw "Not valid character Id ! - " + callerCharacterUid;

  const encounterDb = admin.firestore().collection('encounters').doc(encounterUid).withConverter(encounterDocumentConverter);
  // const callerCharacterDb = admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);

  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {


      const encounterDoc = await t.get(encounterDb);
      //  const callerCharacterDoc = await t.get(callerCharacterDb);

      let encounterData: EncounterDocument = encounterDoc.data();

      if (encounterData.foundByCharacterUid == callerCharacterUid)
        throw "You cant retreat from your encounter";

      // ja sem ten co vytvoril tento encounter a jdu pryc pred startem boje, tedy uplne encounter smazu protoze ostatni by stejne nemohli si vybrat perk a nikomu tim neuskodim ze nejsem v boji
      //a nikdo se do boje jeste nemohl pridat protoze neni vybrany perk
      if (encounterData.foundByCharacterUid == callerCharacterUid && (encounterData.encounterContext == ENCOUNTER_CONTEXT.PERSONAL || encounterData.encounterContext == ENCOUNTER_CONTEXT.WORLD_BOSS)) {

        if (encounterData.combatants.length == 0) //nikdo neni v boji logicky, neni totoiz vybrany perk a ja jako creator tuto metodu v UI muzu volat jen kdyz nejsem v boji a nevybral sem si perk
        {
          t.delete(encounterDb); //proste encounter smazu
        }
        else
          throw "DATABASE ERROR : How is it possible that you want to retreat from combat as a founder and there is already someone fighting there?!!"
      }
      //pokud je to cizi encounter, muzu se proste snadno smazat z watcheru bez postihu pro sebe nebo ostatni
      else if (encounterData.foundByCharacterUid != callerCharacterUid && (encounterData.encounterContext == ENCOUNTER_CONTEXT.PERSONAL || encounterData.encounterContext == ENCOUNTER_CONTEXT.WORLD_BOSS)) {
        encounterData.watchersList = encounterData.watchersList.filter(item => item != callerCharacterUid);
        t.set(encounterDb, JSON.parse(JSON.stringify(encounterData)), { merge: true });
      }


      // t.set(callerCharacterDb, JSON.parse(JSON.stringify(callerCharacterData)), { merge: true });

      // //pokud jsem poslednu hrac co fleenul a nikdo tam uz neni, tak smazu ten encounter cely
      // if (encounterData.combatantList.length == 0 && encounterData.encounterContext == ENCOUNTER_CONTEXT.PERSONAL)
      //   t.delete(encounterDb);
      // else
      //   t.set(encounterDb, JSON.parse(JSON.stringify(encounterData)), { merge: true });





    });


    console.log('Transaction success', result);
    return result;
  } catch (e) {
    console.log('Transaction failure:', e);
    throw new functions.https.HttpsError("aborted", e as string);
  }

});

// export async function CreateWorldBossEncounter(_position: WorldPosition) {
//   //const callerCharacterUid = data.characterUid;
//   const zoneId = _position.zoneId;
//   const locationId = _position.locationId;
//   //  const encounterType = data.encounterType;

//   const MAX_NUMBER_OF_COMBATANTS = 10;

//   let encounterDoc = admin.firestore().collection('encounters').doc();

//   let worldBoosEncounterFound = false;
//   await encounterDoc.where("position.zoneId", "==", zoneId).where("position.locationId", "==", locationId).where("encounterContext", "==", ENCOUNTER_CONTEXT.WORLD_BOSS).get().then(querry => {

//     if (querry.size > 0) {
//       worldBoosEncounterFound = true;
//     }

//   });

//   console.log("worldBoosEncounterFound: " + worldBoosEncounterFound);

//   if (!worldBoosEncounterFound) {
//     // var locationMetaDoc = await admin.firestore().collection('_metadata_zones').doc(zoneId).collection("locations").doc(locationId).get();
//     //var locationsMetaData: MapLocation = locationMetaDoc.data();
//     // var pointOfInterestId = "MAIN";
//     //ziskam nahodny encounter
//     //TODO....
//     //z nej vyberu nahodneho enemy
//     // const choosenEnemy = RollForRandomItem(locationsMetaData.getPointOfInterestById(pointOfInterestId).enemies, false) as EnemyMeta;

//     var enemies: CombatEnemy[] = [];
//     enemies.push(new CombatEnemy(firestoreAutoId(), "WORLD_BOSS_1", new CombatStats(0, 0, 3000, 3000, 0, 0, 0, 0, 0, 0, 0, 0, 0), 50, 100, 10, 12, true, "", [], [], 0));


//     var combatants: CombatMember[] = [];//combatants.push(new CombatMember(characterData.characterName, characterData.uid, [], characterData.converSkillsToCombatSkills(), [], characterData.converStatsToCombatStats(), 0, 0));
//     var combatantList: string[] = []; //combatantList.push(callerCharacterUid);
//     var watchersList: string[] = [];
//     //var timestamp = admin.firestore.Timestamp.now();
//     //const auctionDurationHours = 2 * 3600000;
//     const expireDate = getCurrentDateTime(12);
//     var maxCombatants: number = MAX_NUMBER_OF_COMBATANTS;
//     var isFull: boolean = false;//maxCombatants <= 0;
//     const position = new WorldPosition(zoneId, LOC.NONE, POI.NONE);

//     var worldEncounter: EncounterDocument = new EncounterDocument(encounterDoc.id, enemies, combatants, combatantList, Math.random(), expireDate, "", maxCombatants, watchersList, isFull, "World Boss", ENCOUNTER_CONTEXT.WORLD_BOSS, position, 1, "Combat started!\n", "0", "", []);

//     await encounterDoc.set(JSON.parse(JSON.stringify(worldEncounter)));

//     return "World Encounter created!";
//   }
//   return "World Ecnounter already exists!";

// }
