
// [START import]
import * as functions from "firebase-functions";
import { _namespaceWithOptions } from "firebase-functions/v1/firestore";
import { characterDocumentConverter, CharacterDocument, CHARACTER_CLASS } from ".";
import { CombatEnemy, CombatLog, CombatMember, DropTable, DropTableItem, EncounterDocument, } from "./encounter";


import { Combatskill, SKILL } from "./skills";
//import { UserDimensions } from "firebase-functions/v1/analytics";
const admin = require('firebase-admin');

//const { getFirestore, Timestamp, FieldValue } = require('firebase-admin/firestore');
//const { FieldValue } = require('firebase-admin/firestore');
// // [END import]

export const firestoreAutoId = (): string => {
  const CHARS = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789'

  let autoId = ''

  for (let i = 0; i < 20; i++) {
    autoId += CHARS.charAt(
      Math.floor(Math.random() * CHARS.length)
    )
  }
  return autoId
}

export function RollForRandomItem(_listOfStuff: IHasChanceToSpawn[], rollJustOnce: boolean): IHasChanceToSpawn | null {
  let choosenIndex: number = -1;

  const LOOP_LOCK_PREVENTION_MAX_ROLLS = 100;
  let loopCount = 0;

  do {
    if (loopCount < LOOP_LOCK_PREVENTION_MAX_ROLLS) {
      loopCount++
    }
    else {
      console.log("LOOP PREVENTION TRIGGERED! : " + _listOfStuff.length);
      break;
    }
    choosenIndex = randomIntFromInterval(0, _listOfStuff.length - 1);

    if (Math.random() > _listOfStuff[choosenIndex].chanceToSpawn) { //roll failed
      choosenIndex = -1
    }

  } while (choosenIndex == -1 && !rollJustOnce);

  if (choosenIndex == -1)
    return null;
  else
    return _listOfStuff[choosenIndex];
}

export function randomIntFromInterval(min, max) { // min and max included 
  return Math.floor(Math.random() * (max - min + 1) + min)
}

export function applySkillEffect(_caster: CombatMember, _encounter: EncounterDocument, _skillUsed: Combatskill, _targetUid, _combatLog: CombatLog) {


  if (_caster.characterClass != _skillUsed.characterClass && _skillUsed.characterClass != CHARACTER_CLASS.ANY)
    throw ("This skill is not castable by your hero class!");

  //TODO: TADY BYCH MOHL PREDAVAT combatEntity a ne tu carovat s enemyTarget a combatMember
  //Zkusim najit Target mezi Enemy
  let enemyTarget: CombatEnemy | null = null;
  for (let index = 0; index < _encounter.enemies.length; index++) {

    if (_encounter.enemies[index].stats.health > 0 && _encounter.enemies[index].uid == _targetUid)
      enemyTarget = _encounter.enemies[index];

    //Kdyz uz iteruju vsechny enemy.....Zvysim threat vsem enemy o malinkato, hlavne proto aby na me vsichni targetnuli kdyz zacne boj at maji enemy nejaky target
    _encounter.enemies[index].addThreatForCombatant(_caster.characterUid, 1);
  }

  //Zkusim najit Target mezi Allies
  let combatantTarget: CombatMember | null = null;
  if (enemyTarget == null) {

    for (let index = 0; index < _encounter.combatants.length; index++) {

      if (_encounter.combatants[index].uid == _targetUid)
        combatantTarget = _encounter.combatants[index];
    }
  }

  _encounter.addEntryToCombatLog(_caster.displayName + " casted " + _skillUsed.skillId);


  if (_skillUsed.skillId == SKILL.PUNCH) {

    if (enemyTarget == null)
      throw "Wrong target! You must target enemy !";

    if (_caster.stats.mana < _skillUsed.manaCost)
      throw "Not enough Mana ! Mana cost - " + _skillUsed.manaCost + " you have - " + _caster.stats.mana;

    _caster.stats.mana -= _skillUsed.manaCost;
    _encounter.dealDamageToCombatEntity(_caster, enemyTarget, _skillUsed.amounts[0], _skillUsed.skillId);
  }
  else if (_skillUsed.skillId == SKILL.SLAM) {

    if (enemyTarget == null)
      throw "Wrong target! You must target enemy !";

    if (_caster.stats.mana < _skillUsed.manaCost)
      throw "Not enough Mana ! Mana cost - " + _skillUsed.manaCost + " you have - " + _caster.stats.mana;

    _encounter.giveHealthToAlly(_caster, _caster, _skillUsed.amounts[1], _skillUsed.skillId);
    _caster.stats.mana -= _skillUsed.manaCost;
    _encounter.dealDamageToCombatEntity(_caster, enemyTarget, _skillUsed.amounts[0], _skillUsed.skillId);
  }
  else if (_skillUsed.skillId == SKILL.FIRST_AID) {
    if (_caster.stats.mana < _skillUsed.manaCost)
      throw "Not enough Mana ! Mana cost - " + _skillUsed.manaCost + " you have - " + _caster.stats.mana;

    _encounter.giveHealthToAlly(_caster, _caster, _skillUsed.amounts[0], _skillUsed.skillId);
    _caster.stats.mana -= _skillUsed.manaCost;
  }

  else if (_skillUsed.skillId == SKILL.EXECUTE) {

    if (enemyTarget == null)
      throw "Wrong target! You must target enemy !";

    if (enemyTarget.stats.health / enemyTarget.stats.healthMax <= 0.2)
      _encounter.dealDamageToCombatEntity(_caster, enemyTarget, _skillUsed.amounts[0] * 2, _skillUsed.skillId);

    else {
      if (_caster.stats.mana < _skillUsed.manaCost)
        throw "Not enough Mana ! Mana cost - " + _skillUsed.manaCost + " you have - " + _caster.stats.mana;
      _encounter.dealDamageToCombatEntity(_caster, enemyTarget, _skillUsed.amounts[0], _skillUsed.skillId);
      _caster.stats.mana -= _skillUsed.manaCost;
    }

  }
  else if (_skillUsed.skillId == SKILL.CLEAVE) {
    //   if (enemyTarget == null)
    //   throw "Wrong target! You must target enemy !";

    if (_caster.stats.mana < _skillUsed.manaCost)
      throw "Not enough Mana ! Mana cost - " + _skillUsed.manaCost + " you have - " + _caster.stats.mana;

    //enemyTarget.stats.health -= _skillUsed.amounts[0];
    for (let index = 0; index < _encounter.enemies.length; index++) {
      if (_encounter.enemies[index].stats.health > 0)
        if (_encounter.enemies[index].targetUid == _caster.uid)
          _encounter.dealDamageToCombatEntity(_caster, _encounter.enemies[index], _skillUsed.amounts[0], _skillUsed.skillId);
    }
    _caster.stats.mana -= _skillUsed.manaCost;
  }
  else if (_skillUsed.skillId == SKILL.SHIELD_WALL) {

    if (_caster.stats.mana < _skillUsed.manaCost)
      throw "Not enough Mana ! Mana cost - " + _skillUsed.manaCost + " you have - " + _caster.stats.mana;

    if (_skillUsed.buff == undefined)
      throw ("How is it possible that you are casting skill which should have Buff defined but has none?! " + _skillUsed.skillId);

    _caster.addBuff(_skillUsed.buff);

    _caster.stats.mana -= _skillUsed.manaCost;
  }
  else if (_skillUsed.skillId == SKILL.CURSE_OF_WEAKNESS) {
    if (enemyTarget == null)
      throw "Wrong target! You must target enemy !";

    if (_caster.stats.mana < _skillUsed.manaCost)
      throw "Not enough Mana ! Mana cost - " + _skillUsed.manaCost + " you have - " + _caster.stats.mana;

    if (_skillUsed.buff == undefined)
      throw ("How is it possible that you are casting skill which should have Buff defined but has none?! " + _skillUsed.skillId);

    enemyTarget.addBuff(_skillUsed.buff);

    _caster.stats.mana -= _skillUsed.manaCost;
  }
  else if (_skillUsed.skillId == SKILL.SHADOWBOLT) {
    if (enemyTarget == null)
      throw "Wrong target! You must target enemy !";

    if (_caster.stats.mana < _skillUsed.manaCost)
      throw "Not enough Mana ! Mana cost - " + _skillUsed.manaCost + " you have - " + _caster.stats.mana;

    if (_skillUsed.buff == undefined)
      throw ("How is it possible that you are casting skill which should have Buff defined but has none?! " + _skillUsed.skillId);

    _encounter.dealDamageToCombatEntity(_caster, enemyTarget, _skillUsed.amounts[0], _skillUsed.skillId);

    if (Math.random() <= _skillUsed.amounts[1])
      enemyTarget.addBuff(_skillUsed.buff);

    _caster.stats.mana -= _skillUsed.manaCost;
  }
  else if (_skillUsed.skillId == SKILL.REJUVENATION) {
    if (combatantTarget == null)
      throw "Wrong target! You must target ally!";

    if (_caster.stats.mana < _skillUsed.manaCost)
      throw "Not enough Mana ! Mana cost - " + _skillUsed.manaCost + " you have - " + _caster.stats.mana;

    if (_skillUsed.buff == undefined)
      throw ("How is it possible that you are casting skill which should have Buff defined but has none?! " + _skillUsed.skillId);

    combatantTarget.addBuff(_skillUsed.buff);

    _caster.stats.mana -= _skillUsed.manaCost;
  }
  else if (_skillUsed.skillId == SKILL.LIFE_TAP) {

    if (_caster.stats.mana < _skillUsed.manaCost)
      throw "Not enough Mana ! Mana cost - " + _skillUsed.manaCost + " you have - " + _caster.stats.mana;

    _encounter.dealDamageToCombatEntity(_caster, _caster, _skillUsed.amounts[0], _skillUsed.skillId);
    _caster.stats.mana -= _skillUsed.manaCost;
    _caster.giveMana(_skillUsed.amounts[1]);

  }
  else if (_skillUsed.skillId == SKILL.HEALING_WAVE) {
    if (combatantTarget == null)
      throw "Wrong target! You must target ally!";

    if (_caster.stats.mana < _skillUsed.manaCost)
      throw "Not enough Mana ! Mana cost - " + _skillUsed.manaCost + " you have - " + _caster.stats.mana;

    _caster.stats.mana -= _skillUsed.manaCost;
    _encounter.giveHealthToAlly(_caster, combatantTarget, _skillUsed.amounts[0], _skillUsed.skillId);

  }
  else if (_skillUsed.skillId == SKILL.CHAIN_LIGHTNING) {

    if (enemyTarget == null)
      throw "Wrong target! You must target enemy !";

    if (_caster.stats.mana < _skillUsed.manaCost)
      throw "Not enough Mana ! Mana cost - " + _skillUsed.manaCost + " you have - " + _caster.stats.mana;

    _caster.stats.mana -= _skillUsed.manaCost;
    //blesk preskoci na dalsi 2 enemy co nejsou stejni jako ten kdo dostal dmg a dmg se snizi o 25%
    let damage = _skillUsed.amounts[0];
    let damageDiminish = _skillUsed.amounts[0] * _skillUsed.amounts[1];

    //dame dmg primary targetu 100%
    _encounter.dealDamageToCombatEntity(_caster, enemyTarget, damage, _skillUsed.skillId);

    //75% dalsimu targetu
    damage -= damageDiminish;
    let nextEnemy = _encounter.getRandomEnemy(enemyTarget.uid);
    if (nextEnemy != null) {
      _encounter.dealDamageToCombatEntity(_caster, nextEnemy, damage, _skillUsed.skillId);

      //50% dalsimu targetu
      damage -= damageDiminish;
      nextEnemy = _encounter.getRandomEnemy(nextEnemy.uid);
      if (nextEnemy != null)
        _encounter.dealDamageToCombatEntity(_caster, nextEnemy, damage, _skillUsed.skillId);
    }
  }

  else
    throw "Cannot find any skill with Id - " + _skillUsed.skillId;

  return true;
}

export function shuffle(array) {
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

export function drawNewSkills(_skillsInHand: Combatskill[], _skillsDrawDeck: Combatskill[], _skillsDiscardDeck: Combatskill[]) {


  const HAND_SIZE = 6;
  let numberOfSkillsToBeDrawn = HAND_SIZE;
  let newHand: Combatskill[] = [];

  //move skills from hand to discard deck
  Object.assign(_skillsDiscardDeck, _skillsDiscardDeck.concat(_skillsInHand));

  //if draw deck has less than 6 skills 
  if (_skillsDrawDeck.length < HAND_SIZE) {
    // put remaining skills from draw deck into your hand
    newHand = _skillsDrawDeck.slice();
    numberOfSkillsToBeDrawn -= _skillsDrawDeck.length;
  }

  //pokud je draw deck prazdny
  if (_skillsDrawDeck.length == 0) {

    //move discard deck back to drawdeck
    Object.assign(_skillsDrawDeck, _skillsDiscardDeck);

    //shuffle draw deck
    shuffle(_skillsDrawDeck);

    //set all skills as not already played
    _skillsDrawDeck.forEach(element => { element.alreadyUsed = false; });

    //clear discard deck
    _skillsDiscardDeck.splice(0);
  }
  //draw cards from drawdeck
  newHand = newHand.concat(_skillsDrawDeck.slice(0, numberOfSkillsToBeDrawn));

  for (let index = 0; index < newHand.length; index++) {
    newHand[index].handSlotIndex = index;

  }
  //remove drawn cards from draw deck
  _skillsDrawDeck = _skillsDrawDeck.splice(0, numberOfSkillsToBeDrawn);
  //save new hand
  Object.assign(_skillsInHand, newHand);

}



export interface IHasChanceToSpawn {
  chanceToSpawn: number
}

//[Enemies Meta]
// export class EnemiesMeta {
//   constructor(
//     public enemies: EnemyMeta[]
//   ) { }
// }

export class EnemyMeta implements IHasChanceToSpawn {
  constructor(
    public enemyId: string,
   // public displayName: string,
    public chanceToSpawn: number,
    public health: number,
    public damageMin: number,
    public damageMax: number,
    public level: number,
    public mLevel: number,
    // public dropCountMin: number,
    // public dropCountMax: number,
    public isRare: boolean,
    public dropTable: DropTable[]
  ) { }//super(chanceToSpawn)}
}

//[Enemies Meta]

//!!! JEN PRO UKAZKU ....ZATIM NEPOUZITY
export class MineralMeta implements IHasChanceToSpawn {
  constructor(
    public id: string,
    public displayName: string,
    public chanceToSpawn: number,
    public dropCountMin: number,
    public dropCountMax: number,
    public dropTable: DropTableItem[],
    public level: number
  ) { }//super(chanceToSpawn)}


}



// [Encounter Mining]
export class EncounterMiningDocument {
  constructor(
    public ore: Ore,
    public participants: ParticipantMining[],
    public participantsList: string[],
    public randomIndex: number,
    public created: number,
    public foundByCharacterUid: string,
    public characterSlotsLeft: number,
    public watchersList: string[],
    public isFull: boolean
  ) { }
}

export class ParticipantMining {  //Z nejakeho duvodu nemuzu se dostat na metody tehle classy, moje podezreni je ze vzhledem k tomu ze je to array a convertuje se to nejak samo a nikde nevolam rucne new Combatant, tak to  neni  prava classa
  constructor(
    public characterName: string,
    public characterUid: string,
    public miningCount: number
  ) { }
}

export class Ore {
  constructor(
    public oreId: string,
    public oreContent: string[]
  ) { }
}


exports.sellInventoryItems = functions.https.onCall(async (data, context) => {

  const callerCharacterUid = data.characterUid;
  const inventoryItemsToSellUids: string[] = data.inventoryItemsToSellEquipUids;

  const characterDb = await admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);

  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {

      const characterDoc = await t.get(characterDb);
      let characterData: CharacterDocument = characterDoc.data();



      // // console.log("bag index : " + bagIndex);
      // for (let index = 0; index < bagItemsEquipIndexes.length; index++) {
      //   // console.log("selling item at index: " + bagItemsEquipIndexes[index]);
      //   //  console.log("sellprice index : " + characterData.inventory[0].itemsEquip[3].item.sellPrice);
      //   totalSellPrice += characterData.inventory[bagIndex].itemsEquip[bagItemsEquipIndexes[index]].item.sellPrice;
      //   characterData.inventory[bagIndex].itemsEquip[bagItemsEquipIndexes[index]].amount = -1; //UGH tak toto pouzivam jen jako flag, abych pak mohl promazat vsechny itemy z array a mezitim neresil ze se meni index po smazani
      // }

      let totalSellPrice: number = 0;

      for (var i = characterData.inventory.content.length - 1; i >= 0; i--) {
        if (inventoryItemsToSellUids.includes(characterData.inventory.content[i].getItem().uid)) {
          totalSellPrice += characterData.inventory.content[i].getItem().sellPrice;
          characterData.inventory.content.splice(i, 1);
          characterData.inventory.capacityLeft++;
        }
      }



      // for (var i = characterData.inventory.itemsEquip.length - 1; i >= 0; i--) {
      //   if (inventoryItemsToSellUids.includes(characterData.inventory.itemsEquip[i].uid)) {
      //     totalSellPrice += characterData.inventory.itemsEquip[i].sellPrice;
      //     characterData.inventory.itemsEquip.splice(i, 1);
      //     characterData.inventory.capacityLeft++;
      //   }
      // }
      // for (var i = characterData.inventory.itemsSimple.length - 1; i >= 0; i--) {
      //   if (inventoryItemsToSellUids.includes(characterData.inventory.itemsSimple[i].uid)) {
      //     totalSellPrice += characterData.inventory.itemsSimple[i].sellPrice * characterData.inventory.itemsSimple[i].amount;
      //     characterData.inventory.itemsSimple.splice(i, 1);
      //     characterData.inventory.capacityLeft++;
      //   }
      // }

      console.log("Total sell price: " + totalSellPrice);
      characterData.currency.silver += totalSellPrice;

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

exports.createMiningEncounter = functions.https.onCall(async (data, context) => {//1 R , 1 W

  var callerCharacterUid: string = data.characterUid;

  //checknu jestli uz nahodou nemas encounter vytvoreny
  var alreadyCreatedEncounter = await admin.firestore().collection('encounters_mining').where("foundByCharacterUid", "==", callerCharacterUid).get().then(querry => {
    // console.log("querry" + querry);
    if (querry.size > 0) {
      return true;
    }
    return false;
  });

  if (alreadyCreatedEncounter) {
    console.log("already created mining encounter!");
    return "already created mining encounter!";
  }

  var ore: Ore = new Ore("IRON_VEIN", ["IRON", "IRON", "IRON", "GOLD"]);


  var watchersList: string[] = []; watchersList.push(callerCharacterUid);
  var timestamp = admin.firestore.Timestamp.now();
  var characterSlotsLeft: number = 3;
  var isFull: boolean = characterSlotsLeft <= 0;

  //console.log("timestamp: " + timestamp);



  var encounterDoc: EncounterMiningDocument = new EncounterMiningDocument(ore, [], [], Math.random(), timestamp, callerCharacterUid, characterSlotsLeft, watchersList, isFull);

  await admin.firestore().collection('encounters_mining').add(JSON.parse(JSON.stringify(encounterDoc)));

  return "Encounter mining created!";

  //SHOULD ATTACK HERE AUTOMATICALY? OR JUST ADD ME TO COMBATANTS??

  //.set(encounterDb, JSON.parse(JSON.stringify(encounterData)), { merge: true });
  //vstupy
  //-typ encounteru (hidden stash, bandid camp, rare monster , atd.... )
  //-lokace (badlands---level 1-10, barrens 10-20)

  //zcheckovat jestli nejsem ucastnikem uz mnoha encounteru jinych??
  //zecheckuj esli ma dost staminy?
  // Vybrat nahodny encounter podle z daneho typu (pouzit metadata v databazi nebo natvrdo ve funkci?)
  //vytvorit do "encounters" novy encounter found by player
  // provede prvni utok hned - FightEncounter() prilepi si Listenery na nej a ostatni se tedy muzoz joinovat jde videt v "getRadomEncounterOfOtherPlayers"
  // seber staminu?
});

  // [END allAdd]