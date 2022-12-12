
// [START import]

import * as functions from "firebase-functions";
import { CharacterDocument, characterDocumentConverter, CHARACTER_CLASS, ContentContainer, CONTENT_TYPE, CURRENCY_ID, getCurrentDateTime, getCurrentDateTimeVersionSecondsAdd, getExpAmountFromEncounterForGivenCharacterLevel, getMillisPassedSinceTimestamp, INSTAKILL, millisToSeconds, QuerryHasCharacterAnyUnclaimedEncounterResult, QuerryIsCharacterIsInAnyEncounter, SimpleTally, WorldPosition } from ".";
import { EncounterResult, EncounterResultContentLoot, EncounterResultEnemy, EncounterResultCombatant, RESULT_ITEM_WANT_DURATION_SECONDS } from "./encounterResult";
import { EQUIP_SLOT_ID, generateContentItemSimple, generateEquip, generateFood, RARITY } from "./equip";
import { applySkillEffect, drawNewSkills, EnemyMeta, firestoreAutoId, IHasChanceToSpawn, randomIntFromInterval, RollForRandomItem, shuffle } from "./general2";
import { Party } from "./party";
import { BUFF, Combatskill } from "./skills";
import { LOC, LocationMeta, LocationMetaConverter, POI } from "./worldMap";

const admin = require('firebase-admin');

//[END import]

export const TURN_DURATION_SECONDS = 45;

export enum ENCOUNTER_CONTEXT {
  PERSONAL = "PERSONAL",
  GROUP = "GROUP",
  DUNGEON = "DUNGEON",
  WORLD_BOSS = "WORLD_BOSS",
}


export class CombatEntity {
  constructor(
    public uid: string,
    // public enemyId: string,
    public displayName: string,
    public buffs: CombatBuff[],
    public stats: CombatStats,
    public level: number

  ) { }



  public addBuff(_buffToAdd: CombatBuff) {

    let buffReplaced = false;

    for (let i = 0; i < this.buffs.length; i++) {

      if (this.buffs[i].buffId == _buffToAdd.buffId) {  //uz mam tento buff na sobe

        if (_buffToAdd.rank >= this.buffs[i].rank) //jen pokud je to stejny nebo vetsi rank nez co uz mam, tak ho refreshnu
        {

          this.buffs.splice(i, 1, _buffToAdd);
          buffReplaced = true;
          break;
        }
        else {
          throw ("Target already has this buff with higher rank. Cant replace it");
          console.log("Target already has this buff with higher rank. Cant replace it. You wasted your mana");
          buffReplaced = true;
        }
      }
    }

    if (!buffReplaced)
      this.buffs.push(_buffToAdd);

  }

  public lowerTurnsLeftOnMyBuffs(_encounter: EncounterDocument) {

    for (var i = this.buffs.length - 1; i >= 0; i--) {
      this.buffs[i].applyEffectWhenTurnEnds(this, _encounter);
      if (this.buffs[i].lowerTurnsLeftAndCheckIfHasExpired()) {
        this.buffs.splice(i, 1);
      }
    }
  }

  public giveHealth(_amount: number) {
    this.stats.health += _amount;
    if (this.stats.health > this.stats.healthMax)
      this.stats.health = this.stats.healthMax;
  }

  public takeHealth(_amount: number): number {

    this.buffs.forEach(buff => { _amount = buff.applyEffectBeforeOwnerTakesDamage(_amount) });

    if (this.stats.health - _amount < 0) _amount = this.stats.health;
    this.stats.health -= _amount;

    return _amount;
  }
}

export class CombatLog {

  constructor(
    //  public entries : CombatEntry[]
    public entries: string[]
  ) { }

}



export class CombatEnemy extends CombatEntity {
  constructor(
    public uid: string,
    public enemyId: string,
   //public displayName: string,
    public stats: CombatStats,
    //  public healthMax: number,
    // public health: number,
    public damageAmountMin: number,
    public damageAmountMax: number,
    public level: number,
    public mLevel: number,

    public isRare: boolean,
    public dropTable: DropTable[],
    public targetUid: string | null,
    public threatMetter: SimpleTally[],
    public buffs: CombatBuff[],

  ) { super(uid, enemyId,buffs, stats, level) }

  public lowerTurnsLeftOnMyBuffs(_encounter: EncounterDocument) {

    super.lowerTurnsLeftOnMyBuffs(_encounter);

  }

  public giveHealth(_amount: number) {
    super.giveHealth(_amount);

  }

  public takeHealth(_amount: number): number {
    return super.takeHealth(_amount);

  }

  public addBuff(_buffToAdd: CombatBuff) {
    super.addBuff(_buffToAdd);


  }

  dealDamageToMyTarget(_encounter: EncounterDocument) {

    if (this.stats.health <= 0)
      return;

    if (this.targetUid == null)
      return;

    let target: CombatMember = _encounter.getCombatMemberByUid(this.targetUid);
    let myDamage = randomIntFromInterval(this.damageAmountMin, this.damageAmountMax);

    if (target.stats.health <= 0)
      return;


    this.buffs.forEach(buff => { myDamage = buff.applyEffectBeforeOwnerDealsDamage(myDamage) });
    //myDamage = Math.round(myDamage);
    // myDamage = target.takeHealth(myDamage)
    _encounter.addEntryToCombatLog("<b>" + this.displayName + "</b>" + " attacked " + "<b>" + target.displayName + "</b>");

    _encounter.dealDamageToCombatEntity(this, target, myDamage, "Enemy");
    // _encounter.addEntryToCombatLog(this.displayName + " hit " + target.displayName + " for <color=\"yellow\">" + myDamage + "</color> damage");

    // if (target.stats.health == 0) //combatMember Died
    // _encounter.addEntryToCombatLog("<b>" + target.displayName + "</b>" + "<color=\"red\"> Died !</color>");

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




  generateDropFromEnemy(): ContentContainer[] {

    let droppedItems: ContentContainer[] = [];

    //let ItemDrops: InventoryItemSimple[] = [];
    const LOOP_LOCK_PREVENTION_MAX_ROLLS = 100;
    let loopCount = 0;

    let rolledItem: IHasChanceToSpawn | null = null;

    let dropCount = 0;

    console.log("-----Drop z " + this.displayName + " -----");

    //A rolluju dokud nenaplnim minimalny pocet vyzadovanych dropu, pokud je 0 , tak to zkusim jen jednou a konec nic nedroplo? Ok..
    loopCount = 0;
    dropCount = 0;

    this.dropTable.forEach(dropTable => {
      dropCount = 0;
      loopCount = 0;
      do {

        if (loopCount < LOOP_LOCK_PREVENTION_MAX_ROLLS)
          loopCount++;
        else {

          console.log("LOOP LOCK PREVENTION TRIGGERED!! : generovani lootu for enemy :" + this.enemyId);
          //  throw "LOOP LOCK PREVENTION : generovani lootu pro encounter with random index : " + this.randomIndex + " for enemy :" + enemy.enemyId;
          break;

        }
        //zkusim jednou rolnout a vybrat nejaky item
        rolledItem = RollForRandomItem(dropTable.dropTableItems, true);

        //pokud to proslo a vybrany item se svou sanci na drop prosel, pridam si ho do dropu
        if (rolledItem != null) {
          dropCount++;
          console.log((rolledItem as DropTableItem).itemId);

          if ((rolledItem as DropTableItem).itemType == CONTENT_TYPE.ITEM) {
            const item = generateContentItemSimple((rolledItem as DropTableItem).itemId, (rolledItem as DropTableItem).amount);
            droppedItems.push(new ContentContainer((rolledItem as DropTableItem).itemType, item, undefined, undefined, undefined));
          }
          else if ((rolledItem as DropTableItem).itemType == CONTENT_TYPE.EQUIP) {
            const equip = generateEquip(this.mLevel, (rolledItem as DropTableItem).rarity, EQUIP_SLOT_ID.ANY, CHARACTER_CLASS.ANY);
            droppedItems.push(new ContentContainer((rolledItem as DropTableItem).itemType, undefined, equip, undefined, undefined));
          }
          else if ((rolledItem as DropTableItem).itemType == CONTENT_TYPE.FOOD) {
            const food = generateFood((rolledItem as DropTableItem).itemId, (rolledItem as DropTableItem).amount);
            droppedItems.push(new ContentContainer((rolledItem as DropTableItem).itemType, undefined, undefined, undefined, food));
          }
        }


      } while (dropCount < dropTable.dropCountMin || loopCount < dropTable.dropCountMax); //aspon "Max-krát" musím loopnout kdyz je max drop 3 a min 0, tak 3x ten loop at probehne!

      console.log("-----loop probehlo tolikrát : " + loopCount);

    });
    return droppedItems;

  }



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
    public characterPortrait: string

  ) { super(uid,displayName, buffs, stats, level) }

  public lowerTurnsLeftOnMyBuffs(_encounter: EncounterDocument) {
    super.lowerTurnsLeftOnMyBuffs(_encounter);

  }

  public giveHealth(_amount: number) {
    super.giveHealth(_amount);
  }

  public takeHealth(_amount: number): number {
    return super.takeHealth(_amount);
  }

  public addBuff(_buffToAdd: CombatBuff) {
    super.addBuff(_buffToAdd);
  }


  giveMana(_amount: number) {
    this.stats.mana += _amount;
    if (this.stats.mana > this.stats.manaMax)
      this.stats.mana = this.stats.manaMax;
  }

  regenMana(): number {
    let restoreAmount = this.stats.spirit;
    this.stats.mana += restoreAmount;

    if (this.stats.mana > this.stats.manaMax) {
      restoreAmount -= this.stats.mana - this.stats.manaMax;
      this.stats.mana = this.stats.manaMax;

    }

    return restoreAmount;
  }

  regenHealth(): number {
    let restoreAmount = this.stats.spirit / 10;
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
    // public uid: string,
    public buffId: string,
    public durationTurns: number,
    public turnsLeft: number,
    public amounts: number[],
    public rank: number
  ) { }

  lowerTurnsLeftAndCheckIfHasExpired(): boolean {
    this.turnsLeft--;
    return (this.turnsLeft < 0)

  }

  applyEffectBeforeOwnerTakesDamage(_damageAmount: number): number {

    switch (this.buffId) {
      case BUFF.SHIELD_WALL_BUFF:
        _damageAmount *= this.amounts[0];
        break;
      case BUFF.FRAGILITY_BUF:
        _damageAmount += _damageAmount * this.amounts[0];
        break;

      default:
        break;
    }

    return _damageAmount;
  }


  applyEffectBeforeOwnerDealsDamage(_damageAmount: number): number {

    switch (this.buffId) {
      case BUFF.WEAKNESS_BUFF:
        _damageAmount = _damageAmount - (this.amounts[0] * _damageAmount);
        break;

      default:
        break;
    }

    return _damageAmount;
  }


  applyEffectWhenTurnEnds(_ownerOfThisBuff: CombatEntity, _encounter: EncounterDocument) {

    switch (this.buffId) {
      case BUFF.REJUVENATION_BUFF:
        {

          //   _ownerOfThisBuff.giveHealth(this.amounts[0]);
          _encounter.giveHealthToAlly(_ownerOfThisBuff as CombatMember, _ownerOfThisBuff as CombatMember, this.amounts[0], this.buffId);
          // _encounter.addEntryToCombatLog(this.buffId +" healed " + _ownerOfThisBuff.displayName + " for <color=\"green\">" +this.amounts[0] + "</color>")
          //const entry = _target.displayName + " was healed for <color=\"yellow\">" + _amount + "</color> health by " + _caster.displayName + " (" + _skillUsed.skillId + ")";

        }
        break;

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
    public stamina: number,
    public intellect: number,
    public agility: number,
    public spirit: number,
    public strength: number,
    public armor: number,

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
    public foundByCharacterUid: string,
    public maxCombatants: number,
    public watchersList: string[],
    public isFull: boolean,
    public foundByName: string,
   // public chatLog: string,
    public encounterContext: ENCOUNTER_CONTEXT,
    public position: WorldPosition,
    // public restsPerTurn: number,
    //  public restsLeftUntilEndOfTurn: number,
    public turnNumber: number,
    public combatLog: string,
    public expireDateTurn: string

  ) { }

  getRandomEnemy(_enemyUidToExclude: string): CombatEnemy | null {

    let choosenEnemy: CombatEnemy | null = null;

    let loopCount = 0;
    do {
      loopCount++;
      choosenEnemy = this.enemies[randomIntFromInterval(0, this.enemies.length - 1)];
      if (choosenEnemy.uid == _enemyUidToExclude || choosenEnemy.stats.health <= 0)
        choosenEnemy = null;

      if (loopCount > 20)
        break;

    } while (choosenEnemy == null);

    return choosenEnemy;
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

  convertEnemiesToEncounterResultEnemies(): EncounterResultEnemy[] {

    let result: EncounterResultEnemy[] = [];

    this.enemies.forEach(enemy => {
      let encounterResultLoot: EncounterResultContentLoot[] = [];

      enemy.generateDropFromEnemy().forEach(content => {
        encounterResultLoot.push(new EncounterResultContentLoot(content, [], null))
      });

      result.push(new EncounterResultEnemy(enemy.enemyId, enemy.displayName, enemy.level, encounterResultLoot));
    });

    return result;
  }

  getSilverAmount(): number {

    let silverTotal: number = 0;

    this.enemies.forEach(enemy => {
      silverTotal += randomIntFromInterval(enemy.mLevel * 0.75 + 1, enemy.mLevel * 1.25);
    });

    silverTotal = silverTotal / (this.combatantList.length);
    return silverTotal;
  }

  checkForEndOfTurn() {

    //---TURN ENDED!---
    if (this.hasAllCombatMembersRested()) {
      this.addEntryToCombatLog("-----Turn Ended!-----");// + " after he casted " + _usedSkill.skillId);
      this.lowerTurnsLeftOnAllBuffsInEncounter(); //prvni aplikuju vsechny buffy
      this.allComabtMembersRestores(); //hraci si restornou zivot/manu a karty
      this.allEnemiesDealDamageToTheirTargets(); //...enemy zautoci uplne na konci kola
      this.turnNumber++;
      this.expireDateTurn = getCurrentDateTimeVersionSecondsAdd(TURN_DURATION_SECONDS);
      //a znovu zvedneme natavime hracum ze nejsou rested
      this.combatants.forEach(combatant => { combatant.hasRested = false });

    }

  }

  lowerTurnsLeftOnAllBuffsInEncounter() {

    this.combatants.forEach(combatant => {
      combatant.lowerTurnsLeftOnMyBuffs(this);
    });

    this.enemies.forEach(enemy => {
      enemy.lowerTurnsLeftOnMyBuffs(this);
    });

  }

  allComabtMembersRestores() {

    this.combatants.forEach(combatMember => {
      if (combatMember.stats.health > 0) { //jen pokud je nazivu
        drawNewSkills(combatMember.skillsInHand, combatMember.skillsDrawDeck, combatMember.skillsDiscardDeck);

        //Regen Spirit
        let healthRegenAmount = combatMember.regenHealth();
        let manaRegenAmount = combatMember.regenMana();
        if (manaRegenAmount > 0 || healthRegenAmount > 0)
          this.addEntryToCombatLog("<b>" + combatMember.displayName + "</b>" + " spirit restored " + "<color=\"yellow\">" + healthRegenAmount + "</color> health and <color=\"yellow\">" + manaRegenAmount + "</color> mana");

      }
    });
  }

  allEnemiesDealDamageToTheirTargets() {
    this.enemies.forEach(enemy => {
      enemy.dealDamageToMyTarget(this);//this.enemyDealDamageToItsTarget(enemy);
    });

  }

  giveHealthToAlly(_caster: CombatMember, _target: CombatMember, _amount: number, _sourceId: string) {
    if (_target.stats.health <= 0)
      return;

    // if (_target.stats.health + _amount > _target.stats.healthMax) _amount = _target.stats.healthMax - _target.stats.health;
    // _target.stats.health += _amount;
    // _amount = Math.round(_amount);

    _target.giveHealth(_amount);

    const entry = "<b>" + _target.displayName + "</b>" + " was healed for <color=\"green\">" + _amount + "</color>"; //health by " + _caster.displayName + " (" + _sourceId + ")";
    this.addEntryToCombatLog(entry);
    console.log(entry);
    //combatLot.entries.push(entry)

    //Zaloguju damage do STATS
    this.combatants.forEach(item => {
      if (item.characterUid == _caster.characterUid) { //najdu svuj combatant zazanam v encounter datech
        if (_amount > 0)
          item.healingDone += _amount;
        else
          console.log("Heal nemuze byt mensi nez 0!");
      }
    });

    //pridam threat 1:0,5 heal vsem enemy
    this.addThreatForCombatantOnAllEnemies(_caster.characterUid, _amount / 2);
    // _target.addThreatForCombatant(_attacker.characterUid, _amount);
  }


  dealDamageToCombatEntity(_attacker: CombatEntity, _target: CombatEntity, _amount: number, _sourceId: string) {
    if (_target.stats.health <= 0)
      return;

    let criticalStrike = "";
    //hodime si jestli nahodou nebude utok kriticky, zatim hloupy vypocet kdy 1 Agility dava 0.1% crit chance
    if (randomIntFromInterval(0, 1000) <= _attacker.stats.agility) {
      _amount *= 1.5;
      criticalStrike = "<color=#808080>(critical)</color>";
    }

    let dmgReductionAmount = "";
    //snizime zraneni o strength. Zatim stupidne 1 Strength snizuje 0.1%
    let reductionAmount = Math.round(_amount * (0.001 * _target.stats.strength));
    console.log("reductionAmount: " + reductionAmount);
    _amount -= reductionAmount;

    if (reductionAmount > 0)
      dmgReductionAmount = "<color=#808080>(" + reductionAmount.toString() + " damage mitigated)</color>";

    let totalAmount = _target.takeHealth(_amount);//Math.round(_target.takeHealth(_amount));

    //console.log("_target.name: " + _target.displayName + "_target.uid: " + _target.uid + " _attacker.name :" + _attacker.displayName);

    const entry = "<b>" + _target.displayName + "</b>" + " suffered <color=\"red\">" + totalAmount + "</color> damage " + criticalStrike + " " + dmgReductionAmount;// from " + _attacker.displayName ;//+ " (" + _sourceId + ")";
    this.addEntryToCombatLog(entry);
    console.log(entry);
    //  combatLog.entries.push(entry)

    if (_attacker instanceof CombatMember)
      //Zaloguju damage do STATS
      this.combatants.forEach(item => {
        if (item.characterUid == _attacker.characterUid) { //najdu svuj combatant zazanam v encounter datech
          if (totalAmount > 0)
            item.damageDone += totalAmount;
          else
            console.log("Damage nemuze byt mensi nez 0!");
        }
      });

    //pridam threat 1:1 dmg
    if (_target instanceof CombatEnemy && _attacker instanceof CombatMember)
      _target.addThreatForCombatant(_attacker.characterUid, totalAmount);

    if (_target.stats.health == 0 && _target instanceof CombatMember) //player Died
    {
      this.addEntryToCombatLog("<b>" + _target.displayName + "<color=\"red\"> Died !</color></b>");
      this.removeAllThreatForCombatantOnAllEnemies(_target.uid);
      (_target as CombatMember).hasRested = true;
      //   this.checkForEndOfTurn();

    }
    else if (_target.stats.health == 0 && _target instanceof CombatEnemy) //enemy died
      this.addEntryToCombatLog("<b>" + _target.displayName + "<color=\"green\"> Died !</color></b>");


  }

  getCombatMemberByUid(_combatMemberUid: string): CombatMember {
    for (const combatMember of this.combatants) {
      if (combatMember.characterUid == _combatMemberUid) {
        return combatMember;
      }
    }
    throw ("There is no combat member with UID :" + _combatMemberUid);


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

export class DropTable {
  constructor(
    public dropCountMax: number,
    public dropCountMin: number,
    public dropTableItems: DropTableItem[],

  ) { }

}

export class DropTableItem implements IHasChanceToSpawn {
  constructor(
    public itemId: string,
    public itemType: string,
    public chanceToSpawn: number,
    public amount: number,
    public rarity: RARITY,

  ) { }

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
      //chatLog: _encounter.chatLog,
      position: _encounter.position,
      uid: _encounter.uid,
      // restsPerTurn: _encounter.restsPerTurn,
      // restsLeftUntilEndOfTurn: _encounter.restsLeftUntilEndOfTurn,
      turnNumber: _encounter.turnNumber,
      combatLog: _encounter.combatLog,
      foundByCharacterUid: _encounter.foundByCharacterUid,
      expireDateTurn: _encounter.expireDateTurn
    };
  },
  fromFirestore: (snapshot: any, options: any) => {
    const data = snapshot.data(options);


    //  // pokud chci volat combatatns neco tak tady musim jak ve skills vytvorit nove classy combatantu a naplnit je hodnotama aby meli pristupne metody?!! Nejde to ...

    let combatants: CombatMember[] = [];

    data.combatants.forEach(combatMember => {

      let buffs: CombatBuff[] = [];
      combatMember.buffs.forEach(buff => {
        buffs.push(new CombatBuff(buff.buffId, buff.durationTurns, buff.turnsLeft, buff.amounts, buff.rank));
      });

      combatants.push(new CombatMember(combatMember.uid, combatMember.displayName, combatMember.characterUid, combatMember.characterClass, combatMember.skillsInHand, combatMember.skillsDrawDeck, combatMember.skillsDiscardDeck, combatMember.stats, combatMember.damageDone, combatMember.hasRested, combatMember.level, combatMember.healingDone, buffs, combatMember.characterPortrait));
    });

    let enemies: CombatEnemy[] = [];
    data.enemies.forEach(enemy => {

      let buffs: CombatBuff[] = [];
      enemy.buffs.forEach(buff => {
        buffs.push(new CombatBuff(buff.buffId, buff.durationTurns, buff.turnsLeft, buff.amounts, buff.rank));
      });


      enemies.push(new CombatEnemy(enemy.uid, enemy.enemyId, enemy.stats, enemy.damageAmountMin, enemy.damageAmountMax, enemy.level, enemy.mLevel, enemy.isRare, enemy.dropTable, enemy.targetUid, enemy.threatMetter, buffs));
    });
    //let combatMembers :CombatMember[]=[];
    // Object.assign(combatMembers, data.combatants)

    return new EncounterDocument(data.uid, enemies, combatants, data.combatantList, data.randomIndex, data.expireDate, data.foundByCharacterUid, data.maxCombatants, data.watchersList, data.isFull, data.foundByName, data.encounterContext, data.position, data.turnNumber, data.combatLog, data.expireDateTurn);
  }
};


//NEJVOLANEJSI FUNKCE 
// R - 1 , W- 1 ( + 1 delete / +1 create / -1 write, pokud encounter skonci)
exports.applySkillOnEncounter = functions.https.onCall(async (data, context) => {

  const encounterUid = data.encounterUid;
  const callerCharacterUid = data.characterUid;
  const skillToAttackWithId = data.skillSlotId;

  const targetUid = data.targetUid;

  const encounterDb = admin.firestore().collection('encounters').doc(encounterUid).withConverter(encounterDocumentConverter);
  const encounterResultsDb = admin.firestore().collection('encounterResults').doc();


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


      //Najdu skill ktery chces pouzit
      let skillToUse: Combatskill | undefined;


      myCombatEntry.skillsInHand.forEach(item => {
        if (item.handSlotIndex == skillToAttackWithId) {
          skillToUse = item;
          console.log("found a skill you want to apply : " + item.handSlotIndex + " id: " + item.skillId);
        }
      });

      if (skillToUse == undefined)
        throw "cannot find skill you want to use in your hand?! skill slot Id ! - " + skillToAttackWithId;


      if (skillToUse.alreadyUsed)
        throw "Skill already used! - " + skillToAttackWithId;

      if (myCombatEntry.stats.health <= 0)
        throw "You are dead. Cannot cast skill!";

      let combatLog: CombatLog = new CombatLog([]); //Currently Unused, vraci to defakto jako return co se stalo pri tomhle apply skillu, celkem neuzitecne
      //pouziju skill
      try {
        applySkillEffect(myCombatEntry, encounterData, skillToUse, targetUid, combatLog);
        // encounterData.chatLog += myCombatEntry.displayName + " casted " + skillToUse.skillId + "\n";
      } catch (error) {

        throw error;
      }
      skillToUse.alreadyUsed = true;

      //zvednu si pocet utoku o 1
      // encounterData.increaseAttackCountForCharacter(callerCharacterUid);

      //zkontroluju jestli nahodou nejsou vsichni enemy po smrti
      if (encounterData.checkIfAllEnemiesAreDead() || INSTAKILL) {

        const silverAmount = encounterData.getSilverAmount();

        let expireWantItemDate = getCurrentDateTimeVersionSecondsAdd(RESULT_ITEM_WANT_DURATION_SECONDS);
        let resultComatants: EncounterResultCombatant[] = [];
        let encounterResultEnemies = encounterData.convertEnemiesToEncounterResultEnemies();
        encounterData.combatants.forEach(combatant => { resultComatants.push(new EncounterResultCombatant(combatant.uid, combatant.displayName, combatant.characterClass, combatant.level, getExpAmountFromEncounterForGivenCharacterLevel(encounterResultEnemies, combatant.level))); });

        const wantItemPhaseFinished = encounterData.combatantList.length == 1; //kdyz si v boji sam, neni treba pak hlasovat o lootu
        const encounterResult = new EncounterResult(encounterResultsDb.id, encounterResultEnemies, encounterData.combatantList, encounterData.combatantList, encounterData.combatantList, resultComatants, silverAmount, wantItemPhaseFinished, encounterData.turnNumber, expireWantItemDate);
        t.set(encounterResultsDb, JSON.parse(JSON.stringify(encounterResult)));

        t.delete(encounterDb);
      }
      else {

        //aktualizuju encounter
        t.set(encounterDb, JSON.parse(JSON.stringify(encounterData)), { merge: true });
      }

      return combatLog;
    });




    console.log('Transaction success', result);
    return result;
  } catch (e) {
    console.log('Transaction failure:', e);
    throw new functions.https.HttpsError("aborted", "Error : " + e);
  }
});



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

async function joinCharacterToEncounter(_transaction: any, _encounterData: EncounterDocument, _callerCharacterData: CharacterDocument) {


  // if (_callerCharacterData.isJoinedInEncounter)
  if (await QuerryIsCharacterIsInAnyEncounter(_transaction, _callerCharacterData.uid))
    throw "You are already in combat! Cant join new one!";

  if (await QuerryHasCharacterAnyUnclaimedEncounterResult(_transaction, _callerCharacterData.uid))
    throw "You need to loot all corpses before joining another fight!";



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

  //hrac je v boji uz
  // _callerCharacterData.isJoinedInEncounter = true;

  _encounterData.addEntryToCombatLog("<b>" + _callerCharacterData.characterName + " has joined the fight!</b>");

  //Liznu si pocatecni skilly
  let skillsInHand: Combatskill[] = [];
  let skillsDrawDeck: Combatskill[] = shuffle(_callerCharacterData.converEquipToCombatSkills());
  let skillsDiscard: Combatskill[] = [];

  //shuffle(skillsDrawDeck);
  drawNewSkills(skillsInHand, skillsDrawDeck, skillsDiscard);

  //  encounterData.combatants.push(new CombatMember(characterData.characterName,, characterDoc.id, 0, 0));
  _encounterData.combatants.push(new CombatMember(_callerCharacterData.uid, _callerCharacterData.characterName, _callerCharacterData.uid, _callerCharacterData.characterClass, skillsInHand, skillsDrawDeck, skillsDiscard, _callerCharacterData.converStatsToCombatStats(), 0, false, _callerCharacterData.stats.level, 0, [], _callerCharacterData.characterPortrait));
  //   }
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


      await joinCharacterToEncounter(t, encounterData, callerCharacterData);



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

exports.explorePointOfInterest = functions.https.onCall(async (data, context) => {

  const MAX_ENEMIES = 6;

  const callerCharacterUid = data.characterUid;
  const pointOfInterestId = data.pointOfInterestId;

  const encounterDb = admin.firestore().collection('encounters');
  const myPartyDb = admin.firestore().collection('parties').where("partyMembersUidList", "array-contains", callerCharacterUid);
  const characterDb = admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);

  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {

      const characterDoc = await t.get(characterDb);
      let characterData: CharacterDocument = characterDoc.data();

      //  if (characterData.isJoinedInEncounter)
      if (await QuerryIsCharacterIsInAnyEncounter(t, characterData.uid))
        throw ("You cannot explore while in combat!");

      const allCallerPersonalEncounterOnHisPosition = admin.firestore().collection('encounters').where("position.zoneId", "==", characterData.position.zoneId).where("position.locationId", "==", characterData.position.locationId).where("foundByCharacterUid", "==", callerCharacterUid).where("encounterContext", "==", ENCOUNTER_CONTEXT.PERSONAL);
      const zonesDb = await admin.firestore().collection('_metadata_zones').doc(characterData.position.zoneId).collection("locations").doc(characterData.position.locationId).withConverter(LocationMetaConverter);

      //zkontroluje jestli uz mas personal encounter vytvoreny
      let personalEncounter: EncounterDocument | undefined;

      await t.get(allCallerPersonalEncounterOnHisPosition).then(querry => {
        if (querry.size > 1) {
          throw "How can you have more than 1 personal encounter! Database error!";
        }


        querry.docs.forEach(doc => {
          personalEncounter = doc.data();
        });
      });

      var locationMetaDoc = await t.get(zonesDb);
      var locationsMetaData: LocationMeta = locationMetaDoc.data();

      //vyberu nahodneho enemy z point of interest ... hodi error kdyz ho nenajdu, tedy dost spis pripad kdy hrac cheatuje a chce prozkoumat point of interest ktery neni v jeho lokaci
      const pointOfInterest = locationsMetaData.getPointOfInterestById(pointOfInterestId);
      const choosenEnemy = RollForRandomItem(pointOfInterest.enemies, false) as EnemyMeta;
      //Zaplatim explore time
      characterData.subCurrency(CURRENCY_ID.TIME, pointOfInterest.exploreTimePrice);

      const newEnemy = new CombatEnemy(firestoreAutoId(), choosenEnemy.enemyId, new CombatStats(0, 0, choosenEnemy.health, choosenEnemy.health, 0, 0, 0, 0, 0, 0), choosenEnemy.damageMin, choosenEnemy.damageMax, choosenEnemy.level, choosenEnemy.mLevel, choosenEnemy.isRare, choosenEnemy.dropTable, "", [], []);

      //pokud jeste nemam personal encounter, vytvorim jej
      if (personalEncounter == undefined) {

        let enemies: CombatEnemy[] = [];
        enemies.push(newEnemy);

        var combatants: CombatMember[] = [];//combatants.push(new CombatMember(characterData.characterName, characterData.uid, characterData.characterClass, [], characterData.converSkillsToCombatSkills(), [], characterData.converStatsToCombatStats(), 0, 0, characterData.stats.level));
        var combatantList: string[] = []; //combatantList.push(callerCharacterUid);
        var watchersList: string[] = []; watchersList.push(callerCharacterUid);
        const expireDate = getCurrentDateTime(2);
        var maxCombatants: number = 5;
        var isFull: boolean = false;//maxCombatants <= 0;
        //const position = new WorldPosition(zoneId, locationId);

        personalEncounter = new EncounterDocument(encounterDb.doc().id, enemies, combatants, combatantList, Math.random(), expireDate, callerCharacterUid, maxCombatants, watchersList, isFull, characterData.characterName, ENCOUNTER_CONTEXT.PERSONAL, characterData.position, 1, "Combat started!\n", "0");


        //pridam pripadne vsechny party membery do encounteru
        await t.get(myPartyDb).then(querry => {
          if (querry.size > 1)
            throw ("You are in more than 1 party! Database error!");

          querry.docs.forEach(doc => {

            let party: Party = doc.data();
            party.partyMembersUidList.forEach(partyMemberUid => {
              if (!personalEncounter!.watchersList.includes(partyMemberUid))
                personalEncounter!.watchersList.push(partyMemberUid);
            });
          });
        });
      }
      //pokud uz mam personal encounter, pridam do nej jen dalsiho enemy
      else {

        if (personalEncounter.combatantList.length > 0) // jen pokud jeste nikdo neni v combatu! nechceme tam pridavat enemy kdyz uz se bojuje!
          throw "Some is already attacking encounter found by you! Cant explore more enemies!";

        if (personalEncounter.enemies.length < MAX_ENEMIES)
          personalEncounter.enemies.push(newEnemy);
        else
          throw "Cant have more than " + MAX_ENEMIES + " enemies in encounter";
      }

      //Jen kvuli tomu ze explore bere explore time musim udelat dalsi save do db....
      t.set(characterDb, JSON.parse(JSON.stringify(characterData)), { merge: true });

      t.set(encounterDb.doc(personalEncounter.uid), JSON.parse(JSON.stringify(personalEncounter)), { merge: true });

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

  const FLEE_FATIGUE_PENALTY = 5;

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

      //hrac neni v boji uz
      //  callerCharacterData.isJoinedInEncounter = false;

      //dame penalty +  vsechen rest fatigue
      callerCharacterData.addFatigue(FLEE_FATIGUE_PENALTY);
      callerCharacterData.addFatigue(encounterData.turnNumber);



      //encounter z ktereho utikam jsem vytvoril ja sam, je to tedy muj personal encounter,takze ho muzu:
      //1) but smazat a lidem co bojujou uplne posrat boj, 
      //2) nebo musim cekat nez dobojujou a pak az muzu vytvaret dalsi sve personal encountery
      //*3) (to sem mi zda ted best) nenapadne se smazu jako Uid co nasel tento encounter, tim si otevru moznost hledat dalsi personal encountery a oni at si bojujou ten muj stary 
      if (encounterData.foundByCharacterUid == callerCharacterUid && encounterData.encounterContext == ENCOUNTER_CONTEXT.PERSONAL) {
        console.log("menim uid na founder fled");
        encounterData.foundByCharacterUid = "Founder fled the combat...";
      }

      encounterData.removeCharacterFromCombat(callerCharacterUid);

      encounterData.checkForEndOfTurn();

      //pokud odchazi zivy player co jeste nerestoval, tak ho restneme a checkneme jestli nahodou nema timto ukoncit 
      // let myCombatEntry = encounterData.getCombatMemberByUid(callerCharacterUid);
      // if (myCombatEntry.stats.health > 0 && myCombatEntry.hasRested == false) {
      //   myCombatEntry.hasRested = true;
      //   encounterData.checkForEndOfTurn();
      // }

      t.set(callerCharacterDb, JSON.parse(JSON.stringify(callerCharacterData)), { merge: true });

      //pokud jsem poslednu hrac co fleenul a nikdo tam uz neni, tak smazu ten encounter cely
      if (encounterData.combatantList.length == 0 && encounterData.encounterContext == ENCOUNTER_CONTEXT.PERSONAL)
        t.delete(encounterDb);
      else
        t.set(encounterDb, JSON.parse(JSON.stringify(encounterData)), { merge: true });





    });


    console.log('Transaction success', result);
    return result;
  } catch (e) {
    console.log('Transaction failure:', e);
    throw new functions.https.HttpsError("aborted", "Error : " + e);
  }

});

export async function CreateWorldBossEncounter(_position: WorldPosition) {
  //const callerCharacterUid = data.characterUid;
  const zoneId = _position.zoneId;
  const locationId = _position.locationId;
  //  const encounterType = data.encounterType;

  const MAX_NUMBER_OF_COMBATANTS = 10;

  let encounterDoc = admin.firestore().collection('encounters').doc();

  let worldBoosEncounterFound = false;
  await encounterDoc.where("position.zoneId", "==", zoneId).where("position.locationId", "==", locationId).where("encounterContext", "==", ENCOUNTER_CONTEXT.WORLD_BOSS).get().then(querry => {

    if (querry.size > 0) {
      worldBoosEncounterFound = true;
    }

  });

  console.log("worldBoosEncounterFound: " + worldBoosEncounterFound);

  if (!worldBoosEncounterFound) {
    var locationMetaDoc = await admin.firestore().collection('_metadata_zones').doc(zoneId).collection("locations").doc(locationId).get();
    var locationsMetaData: LocationMeta = locationMetaDoc.data();
    var pointOfInterestId = "MAIN";
    //ziskam nahodny encounter
    //TODO....
    //z nej vyberu nahodneho enemy
    const choosenEnemy = RollForRandomItem(locationsMetaData.getPointOfInterestById(pointOfInterestId).enemies, false) as EnemyMeta;

    var enemies: CombatEnemy[] = [];
    enemies.push(new CombatEnemy(firestoreAutoId(), "WORLD_BOSS_1", new CombatStats(0, 0, 3000, 3000, 0, 0, 0, 0, 0, 0), 50, 100, 10, 12, true, choosenEnemy.dropTable, "", [], []));


    var combatants: CombatMember[] = [];//combatants.push(new CombatMember(characterData.characterName, characterData.uid, [], characterData.converSkillsToCombatSkills(), [], characterData.converStatsToCombatStats(), 0, 0));
    var combatantList: string[] = []; //combatantList.push(callerCharacterUid);
    var watchersList: string[] = [];
    //var timestamp = admin.firestore.Timestamp.now();
    //const auctionDurationHours = 2 * 3600000;
    const expireDate = getCurrentDateTime(12);
    var maxCombatants: number = MAX_NUMBER_OF_COMBATANTS;
    var isFull: boolean = false;//maxCombatants <= 0;
    const position = new WorldPosition(zoneId, LOC.NONE, POI.NONE);

    var worldEncounter: EncounterDocument = new EncounterDocument(encounterDoc.id, enemies, combatants, combatantList, Math.random(), expireDate, "", maxCombatants, watchersList, isFull, "World Boss", ENCOUNTER_CONTEXT.WORLD_BOSS, position, 1, "Combat started!\n", "0");

    await encounterDoc.set(JSON.parse(JSON.stringify(worldEncounter)));

    return "World Encounter created!";
  }
  return "World Ecnounter already exists!";

}
