
// [START import]

import { _namespaceWithOptions } from "firebase-functions/v1/firestore";
import { CHARACTER_CLASS, randomIntFromInterval } from ".";
import { CombatEnemy, CombatEntity, CombatMember, EncounterDocument, MONSTER_SKILL_TYPES, } from "./encounter";



import { BUFF_GROUP, Combatskill, SKILL, SKILL_GROUP } from "./skills";
import { BLESS } from "./specials";
//import { UserDimensions } from "firebase-functions/v1/analytics";
//const admin = require('firebase-admin');

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


export function applySkillEffect(_caster: CombatMember, _encounter: EncounterDocument, _skillUsed: Combatskill, _targetUid: string) {


  //pokud combatFlow ma nejaky zaznam uz se bojovalo, takze nekdo odesel po startu boje, takze ok, at se bojuje dal
  // if (_encounter.combatantList.length < _encounter.maxCombatants && _encounter.combatFlow.length == 0)
  //   throw "Not enough players in combat! Need " + (_encounter.maxCombatants - _encounter.combatantList.length) + " more!";

  let ignoreManaPrice: boolean = false;
  if (_caster.characterClass != _skillUsed.characterClass && _skillUsed.characterClass != CHARACTER_CLASS.ANY)
    throw ("This skill is not castable by your hero class!");

  // if (_targetUid == "" && !_skillUsed.validTarget_Self)
  //   throw ("Choose target first!");

  //TODO: TADY BYCH MOHL PREDAVAT combatEntity a ne tu carovat s enemyTarget a combatMember
  //Zkusim najit Target mezi Enemy
  let targetFound = false;
  let target: CombatEntity | null = null;
  for (let index = 0; index < _encounter.enemies.length; index++) {

    if (_encounter.enemies[index].stats.health > 0 && _encounter.enemies[index].uid == _targetUid) {
      target = _encounter.enemies[index];
      targetFound = true;
    }
    //Kdyz uz iteruju vsechny enemy.....Zvysim threat vsem enemy o malinkato, hlavne proto aby na me vsichni targetnuli kdyz zacne boj at maji enemy nejaky target
    _encounter.enemies[index].addThreatForCombatant(_caster.characterUid, 1);
  }

  //Zkusim najit Target mezi Allies
  //let combatantTarget: CombatMember | null = null;
  if (!targetFound) {

    for (let index = 0; index < _encounter.combatants.length; index++) {

      if (_encounter.combatants[index].uid == _targetUid)
        target = _encounter.combatants[index];
    }
  }
  if (_skillUsed.validTarget_Self && target == _caster) {
    //everything ok
  }
  else {
    if (target == null && !_skillUsed.validTarget_Self) {
      throw ("Select your target first!");

    }
    else {

      if (target instanceof CombatEnemy && !_skillUsed.validTarget_AnyEnemy)//&& !_skillUsed.validTarget_Self)
        throw "Wrong target! Cant target enemy!";
      else if (target instanceof CombatMember) {
        if (!_skillUsed.validTarget_AnyAlly)//&& !_skillUsed.validTarget_Self)
          throw "Wrong target! Cant target ally!";

      }
    }
  }
  _encounter.addEntryToCombatLog(_caster.displayName + " casted {" + _skillUsed.skillGroupId + "}");
  let manaCostIncrease = 0;


  _caster.buffs.forEach(buff => {
    buff.applyBeforeAnySkillCastedByOwner(_caster, _encounter);
  });


  switch (_skillUsed.skillId) {

    case SKILL.PUNCH_1:
      {
        console.log("_skillUsed.skillGroupId _X :" + _skillUsed.skillGroupId);
        _encounter.dealDamageToCombatEntity(_caster, target!, _skillUsed.amounts[0], _skillUsed.skillGroupId, _encounter);
        manaCostIncrease = 1;
        break;

      }

    case SKILL.SLAM_1:
      {
        if (target!.hasBuff(BUFF_GROUP.BLEED_BUFF))
          _encounter.dealDamageToCombatEntity(_caster, target!, _skillUsed.amounts[0] + _skillUsed.amounts[1], _skillUsed.skillGroupId, _encounter);
        else
          _encounter.dealDamageToCombatEntity(_caster, target!, _skillUsed.amounts[0], _skillUsed.skillGroupId, _encounter);

        break;
      }
    // case SKILL.SLAM_2:
    //   {

    //     if (target!.hasBuff(BUFF_GROUP.BLEED_BUFF)) {
    //       _encounter.dealDamageToCombatEntity(_caster, target!, _skillUsed.amounts[0] + _skillUsed.amounts[1], _skillUsed.skillGroupId, _encounter);
    //       drawSkill(_caster, _skillUsed.amounts[2]);

    //     }
    //     else
    //       _encounter.dealDamageToCombatEntity(_caster, target!, _skillUsed.amounts[0], _skillUsed.skillGroupId, _encounter);

    //     break;
    //   }
    // case SKILL.SLAM_3:
    // case SKILL.SLAM_4:
    // case SKILL.SLAM_5:
    //   {

    //     if (target?.stats.healthMax == target?.stats.health)
    //       _caster.blockAmount += _skillUsed.amounts[3];

    //     if (target!.hasBuff(BUFF_GROUP.BLEED_BUFF)) {
    //       _encounter.dealDamageToCombatEntity(_caster, target!, _skillUsed.amounts[0] + _skillUsed.amounts[1], _skillUsed.skillGroupId, _encounter);
    //       drawSkill(_caster, _skillUsed.amounts[2]);
    //     }
    //     else
    //       _encounter.dealDamageToCombatEntity(_caster, target!, _skillUsed.amounts[0], _skillUsed.skillGroupId, _encounter);

    //     break;
    //   }
    case SKILL.BLOCK_1:
      {
        _caster.addBlock(_skillUsed.amounts[0], _encounter);

        _skillUsed.originalStats.amountsSkill[0] -= _skillUsed.amounts[1];
        break;
      }
    case SKILL.MYSTIC_SHIELD_1:
      {
        _caster.addBlock(_skillUsed.amounts[0], _encounter);

        _caster.addBuff(_skillUsed.buff!, _caster);
        break;
      }
    case SKILL.BARRIER_STRIKE_1:
      {
        _encounter.dealDamageToCombatEntity(_caster, target!, _skillUsed.amounts[0], _skillUsed.skillGroupId, _encounter);
        _caster.addBlock(_skillUsed.amounts[1], _encounter);

        break;
      }
    case SKILL.BUCKLER_1:
      {
        _caster.addBlock(_skillUsed.amounts[0], _encounter);

        break;
      }
    case SKILL.SHIELD_BASH_1:
      {
        _encounter.dealDamageToCombatEntity(_caster, target!, _caster.blockAmount, _skillUsed.skillGroupId, _encounter);

        break;
      }

    case SKILL.COUNTERSTRIKE_1:
      {
        _caster.addBuff(_skillUsed.buff!, _caster);
        break;
      }
    case SKILL.CRUSADER_1:
      {
        _caster.addBuff(_skillUsed.buff!, _caster);
        break;
      }
    case SKILL.DEFENSIVE_STANCE_1:
      {
        _caster.addBuff(_skillUsed.buff!, _caster);
        break;
      }
    case SKILL.HONEY_BADGER_1:
      {
        _caster.stats.resistanceTotal *= (1 + _skillUsed.amounts[0]);
        break;
      }
    case SKILL.REND_1:

      {
        _encounter.dealDamageToCombatEntity(_caster, target!, _skillUsed.amounts[0], _skillUsed.skillGroupId, _encounter);
        target!.addBuff(_skillUsed.buff!, _caster);

        break;
      }
    case SKILL.CLEAVE_1:

      {

        for (let index = 0; index < _encounter.enemies.length; index++) {
          if (_encounter.enemies[index].stats.health > 0)
            if (_encounter.enemies[index].targetUid == _caster.uid)
              _encounter.dealDamageToCombatEntity(_caster, _encounter.enemies[index], _skillUsed.amounts[0], _skillUsed.skillGroupId, _encounter);
        }
        break;
      }

    case SKILL.TAUNT_1:
      {
        _caster.addBlock(_skillUsed.amounts[0], _encounter);
        _encounter.forceEnemyToChangeTarget(_caster, target! as CombatEnemy, _caster, _skillUsed.amounts[1]);
        break;

      }

    case SKILL.FIRST_AID_1:
      {
        _encounter.giveHealthToCombatEntity(_caster, target!, _skillUsed.amounts[0], _skillUsed.skillGroupId);
        let drawnSkills = drawSkill(target! as CombatMember, _skillUsed.amounts[1]);
        drawnSkills.forEach(element => {
          element.manaCost = 0;
        });
        break;

      }

    case SKILL.SHIELD_WALL_1:
      {
        _caster.addBuff(_skillUsed.buff!, _caster);

        _caster.blockAmount = _caster.blockAmount + Math.round(_caster.blockAmount * _skillUsed.amounts[0]);
        break;
      }


    case SKILL.MORTAL_STRIKE_1:

      {
        _encounter.dealDamageToCombatEntity(_caster, target!, _skillUsed.amounts[0], _skillUsed.skillGroupId, _encounter);
        target!.addBuff(_skillUsed.buff!, _caster);

        break;
      }


    case SKILL.EXECUTE_1:
      {
        //muze se stat ze neco zvedne mana cost executu....proto toto
        let playerManaLeftForBonusDmg = _caster.stats.mana - _skillUsed.manaCost;
        if (playerManaLeftForBonusDmg < 0) //nemam dost many na to to vubec zakouzlit
          ignoreManaPrice = false; //at to zahlasi error ze nemam dost many...
        else {
          ignoreManaPrice = true;
          _caster.stats.mana = 0;
        }
        if (target!.stats.health / target!.stats.healthMax <= _skillUsed.amounts[1] || target!.hasBuff(BUFF_GROUP.BLEED_BUFF) || _encounter.getNumberOfEnemiesAlive() == 1) {
          _encounter.dealDamageToCombatEntity(_caster, target!, _skillUsed.amounts[0] + (playerManaLeftForBonusDmg * _skillUsed.amounts[2]), _skillUsed.skillGroupId, _encounter);
          _caster.stats.mana = 0;
        }
        else
          throw "Cannot be casted. Enemy has too much health or dont have Bleed buff and is not alone";
        break;
      }
    //-----WARLOCK-------
    case SKILL.SHADOWBOLT_1:
      {
        _encounter.dealDamageToCombatEntity(_caster, target!, _skillUsed.amounts[0], _skillUsed.skillGroupId, _encounter);
        if (!target!.hasBuff(_skillUsed.buff?.buffGroupId!)) {
          if (Math.random() <= _skillUsed.amounts[1])
            target!.addBuff(_skillUsed.buff!, _caster);

        }
        break;
      }
    case SKILL.SHADOWBOLT_2:
      {
        _encounter.dealDamageToCombatEntity(_caster, target!, _skillUsed.amounts[0], _skillUsed.skillGroupId, _encounter);
        if (!target!.hasBuff(_skillUsed.buff?.buffGroupId!)) {
          if (Math.random() <= _skillUsed.amounts[1]) {
            target!.addBuff(_skillUsed.buff!, _caster);

            let skillsWithManaCost = _caster.skillsInHand.filter(skill => skill.manaCost > 0)

            if (skillsWithManaCost.length > 0) {
              let choosenSkill = skillsWithManaCost[randomIntFromInterval(0, skillsWithManaCost.length - 1)];
              choosenSkill.manaCost -= _skillUsed.amounts[2];
              if (choosenSkill.manaCost < 0)
                choosenSkill.manaCost = 0;
            }
          }
        }
        break;
      }
    case SKILL.SHADOWBOLT_3:
      {
        _encounter.dealDamageToCombatEntity(_caster, target!, _skillUsed.amounts[0], _skillUsed.skillGroupId, _encounter);
        if (!target!.hasBuff(_skillUsed.buff?.buffGroupId!)) {
          let chanceToUse = _skillUsed.amounts[1];

          if (target! instanceof CombatEnemy) {
            if ((target! as CombatEnemy).targetUid != _caster.uid)
              chanceToUse += _skillUsed.amounts[3];
          }

          if (Math.random() <= chanceToUse) {
            target!.addBuff(_skillUsed.buff!, _caster);

            let skillsWithManaCost = _caster.skillsInHand.filter(skill => skill.manaCost > 0)

            if (skillsWithManaCost.length > 0) {
              let choosenSkill = skillsWithManaCost[randomIntFromInterval(0, skillsWithManaCost.length - 1)];
              choosenSkill.manaCost -= _skillUsed.amounts[2];
              if (choosenSkill.manaCost < 0)
                choosenSkill.manaCost = 0;
            }
          }
        }
        break;
      }
    case SKILL.CURSE_OF_WEAKNESS_2:
      {
        target!.addBuff(_skillUsed.buff!, _caster);
        break;
      }
    case SKILL.CURSE_OF_WEAKNESS_3:
      {
        target!.addBuff(_skillUsed.buff!, _caster);
        if (target! instanceof CombatEnemy) {
          console.log("ano jdu te neutralizovat: " + target.nextSkill.typeId);
          if (target.nextSkill.typeId == MONSTER_SKILL_TYPES.SKILL_TYPE_ATTACK_NORMAL) {
            console.log("ano jdu te neutralizovat 2");
            console.log(" target.nextSkill.amounts[0]:" + target.nextSkill.amounts[0]);
            target.nextSkill.amounts[0] = 0;
            console.log(" target.nextSkill.amounts[0]:" + target.nextSkill.amounts[0]);
          }
        }
        break;
      }
    // case SKILL.LIFE_TAP_1:
    //   {

    //     let amountToTake = _caster.stats.health * _skillUsed.amounts[0];
    //     _encounter.dealDamageToCombatEntity(_caster, _caster, amountToTake, _skillUsed.skillGroupId, _encounter, true);
    //     _caster.giveMana(_skillUsed.amounts[1]);
    //     break;
    //   }
    // case SKILL.LIFE_TAP_2:
    //   {

    //     let amountToTake = _caster.stats.health * _skillUsed.amounts[0];
    //     _encounter.dealDamageToCombatEntity(_caster, _caster, amountToTake, _skillUsed.skillGroupId, _encounter, true);
    //     _caster.giveMana(_skillUsed.amounts[1]);
    //     break;
    //   }
    case SKILL.LIFE_TAP_1:
    case SKILL.LIFE_TAP_2:
    case SKILL.LIFE_TAP_3:
      {

        let amountToTake = _caster.stats.health * _skillUsed.amounts[0];
        _encounter.dealDamageToCombatEntity(_caster, _caster, amountToTake, _skillUsed.skillGroupId, _encounter, true);
        _caster.giveMana(_skillUsed.amounts[1]);
        drawSkill(_caster, _skillUsed.amounts[2]);
        break;
      }
    case SKILL.SIPHON_LIFE_2:
      {
        target!.addBuff(_skillUsed.buff!, _caster);
        break;
      }
    case SKILL.SIPHON_LIFE_3:
      {
        target!.addBuff(_skillUsed.buff!, _caster);

        let result = _encounter.getAdjecentEnemiesOfEnemy(target!.uid);

        if (result[0] != null) {
          result[0].addBuff(_skillUsed.buff!, _caster);
        }
        if (result[1] != null) {
          result[1].addBuff(_skillUsed.buff!, _caster);
        }

        break;
      }
    case SKILL.CORRUPTION_1:
    case SKILL.CORRUPTION_2:
    case SKILL.CORRUPTION_3:
      {
        target!.addBuff(_skillUsed.buff!, _caster);
        break;
      }

    //SHAMAN
    case SKILL.REJUVENATION_1:
      {
        target!.addBuff(_skillUsed.buff!, _caster);
        break;
      }
    case SKILL.REJUVENATION_2:
      {
        target!.addBuff(_skillUsed.buff!, _caster);
        break;
      }
    case SKILL.REJUVENATION_3:
      {
        if (target!.hasBuff(BUFF_GROUP.REJUVENATION_BUFF))
          _encounter.giveHealthToCombatEntity(_caster, target!, _skillUsed.amounts[0], _skillUsed.skillGroupId);

        target!.addBuff(_skillUsed.buff!, _caster);
        break;
      }

    case SKILL.CHAIN_LIGHTNING_2:
    case SKILL.CHAIN_LIGHTNING_3:
      {

        let enemiesHitByLightingUid: string[] = [];
        let damage = _skillUsed.amounts[0];
        // let damageDiminish = damage * _skillUsed.amounts[1];

        //dame dmg primary targetu 100%
        _encounter.dealDamageToCombatEntity(_caster, target!, damage, _skillUsed.skillGroupId, _encounter);

        let enemyThatGotHitByLastJump: CombatEnemy | null = target! as CombatEnemy;

        for (let i = 0; i < _skillUsed.amounts[2]; i++) {

          enemiesHitByLightingUid.push(enemyThatGotHitByLastJump!.uid);
          //dalsi target snizime dmg a vyberem jiny nez ten co ted dostal blesk

          // damage -= damageDiminish;
          damage *= (1 - _skillUsed.amounts[1]);

          if (damage <= 0)
            break;

          enemyThatGotHitByLastJump = _encounter.getRandomEnemyExcludeEnemies(enemiesHitByLightingUid, true);
          if (enemyThatGotHitByLastJump != null) {
            _encounter.dealDamageToCombatEntity(_caster, enemyThatGotHitByLastJump, damage, _skillUsed.skillGroupId, _encounter);
          }
          else {
            break;
          }

        }
        break;
      }
    case SKILL.HEALING_WAVE_1:
    case SKILL.HEALING_WAVE_2:
    case SKILL.HEALING_WAVE_3:
      {

        _encounter.giveHealthToCombatEntity(_caster, target!, _skillUsed.amounts[0], _skillUsed.skillGroupId);

        break;
      }
    case SKILL.LIGHTNING_1:
      {

        let result = _encounter.dealDamageToCombatEntity(_caster, target!, _skillUsed.amounts[0], _skillUsed.skillGroupId, _encounter);

        if (result.hasDamageKilledIt) {
          _caster.skillsInHand.forEach(skill => {
            if (skill.skillGroupId == SKILL_GROUP.LIGHTNING)
              skill.manaCost = 0;
          });
        }
        break;
        break;
      }
    case SKILL.LIGHTNING_2:
      {

        let result = _encounter.dealDamageToCombatEntity(_caster, target!, _skillUsed.amounts[0], _skillUsed.skillGroupId, _encounter);

        if (result.hasDamageKilledIt || result.wasCriticalHit) {
          _caster.skillsInHand.forEach(skill => {
            if (skill.skillGroupId == SKILL_GROUP.LIGHTNING)
              skill.manaCost = 0;
          });
        }
        break;
      }
    case SKILL.LIGHTNING_3:
      {

        let result = _encounter.dealDamageToCombatEntity(_caster, target!, _skillUsed.amounts[0], _skillUsed.skillGroupId, _encounter);

        if (result.hasDamageKilledIt || result.wasCriticalHit) {
          _caster.skillsInHand.forEach(skill => {
            if (skill.skillGroupId == SKILL_GROUP.LIGHTNING || skill.skillGroupId == SKILL_GROUP.CHAIN_LIGHTNING)
              skill.manaCost = 0;
          });
        }
        break;
      }
    case SKILL.LIGHTNING_SHIELD_2:
    case SKILL.LIGHTNING_SHIELD_3:
      {
        target!.addBuff(_skillUsed.buff!, _caster);
        break;
      }
    case SKILL.BLOOD_LUST_1:
      {
        _caster!.addBuff(_skillUsed.buff!, _caster);
        break;
      }
    case SKILL.BLOOD_LUST_2:
      {
        if (_caster != target!)
          target!.addBuff(_skillUsed.buff!, _caster);

        _caster!.addBuff(_skillUsed.buff!, _caster);

        drawSkill(target! as CombatMember, _skillUsed.amounts[0]);
        drawSkill(_caster! as CombatMember, _skillUsed.amounts[0]);

        break;
      }
    case SKILL.BLOOD_LUST_3:
      {
        _encounter.combatants.forEach(combatant => {
          combatant!.addBuff(_skillUsed.buff!, _caster);
          drawSkill(combatant, _skillUsed.amounts[0]);
        });


        break;
      }

    //-----CURSES-------
    case SKILL.CURSE_BROKEN_LEG:
      {
        _encounter.dealDamageToCombatEntity(_caster, _caster, _skillUsed.amounts[0], _skillUsed.skillGroupId, _encounter);
        break;
      }



    default:
      // throw "Cannot find any skill with Id - " + _skillUsed.skillId;
      break;
  }


  if (_skillUsed.singleUse) {
    if (!_caster.hasBless(BLESS.LASTING_HAND))
      moveSkillToExhaustDeck(_caster, _skillUsed.uid);
    else {
      moveSkillToDiscardDeck(_caster, _skillUsed.uid);
      _skillUsed.originalStats.manaCost++;
      // if (Math.random() < 0.25) {
      //   moveSkillToExhaustDeck(_caster, _skillUsed.uid);
      // }
      // else
      //   moveSkillToDiscardDeck(_caster, _skillUsed.uid);
    }
  }
  else
    moveSkillToDiscardDeck(_caster, _skillUsed.uid);

  if (!ignoreManaPrice) {
    if (_caster.stats.mana < _skillUsed.manaCost)
      throw "Not enough Mana ! Mana cost - " + _skillUsed.manaCost + " you have - " + _caster.stats.mana;

    _caster.stats.mana -= _skillUsed.manaCost;


  }

  _skillUsed.originalStats.manaCost += manaCostIncrease;



  return true;
}

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

//vraci jestli doslo k shuffle
export function drawNewSkills(_skillsInHand: Combatskill[], _skillsDrawDeck: Combatskill[], _skillsDiscardDeck: Combatskill[], _drawCount: number): boolean {

  //const HAND_SIZE = 5;
  let shufflePerformed = false;
  let numberOfSkillsToBeDrawn = _drawCount;
  let newHand: Combatskill[] = [];

  //tirgguju efekty pri discardu karet v ruce
  _skillsInHand.forEach(skill => {
    switch (skill.skillId) {
      case SKILL.REJUVENATION_2:
      case SKILL.REJUVENATION_3:
        {
          if (skill.originalStats.manaCost > 0)
            skill.originalStats.manaCost--;
          break;
        }
      default:
        break;
    }
  });

  //move skills from hand to discard deck
  Object.assign(_skillsDiscardDeck, _skillsDiscardDeck.concat(_skillsInHand));

  //if draw deck has less than CARD_DRAW_COUNT skills 
  if (_skillsDrawDeck.length < _drawCount) {
    // put remaining skills from draw deck into your hand
    //newHand = _skillsDrawDeck.slice();
    numberOfSkillsToBeDrawn -= _skillsDrawDeck.length;
    newHand = _skillsDrawDeck.splice(0);
    shufflePerformed = true;
  }

  //pokud je draw deck prazdny
  if (_skillsDrawDeck.length == 0) {
    //move discard deck back to drawdeck
    Object.assign(_skillsDrawDeck, _skillsDiscardDeck);

    //shuffle draw deck
    shuffleArray(_skillsDrawDeck);

    //set all skills as not already played
    _skillsDrawDeck.forEach(element => { element.alreadyUsed = false; });

    //clear discard deck
    _skillsDiscardDeck.splice(0);
  }

  //draw cards from drawdeck
  newHand = newHand.concat(_skillsDrawDeck.slice(0, numberOfSkillsToBeDrawn));
  for (let index = 0; index < newHand.length; index++) {
    //  newHand[index].handSlotIndex = index;
    newHand[index].uid = firestoreAutoId();
  }

  //remove drawn cards from draw deck
  _skillsDrawDeck.splice(0, numberOfSkillsToBeDrawn);
  //save new hand
  _skillsInHand.splice(0, _skillsInHand.length - 1) //kdyz prvni nesmazu tak kdyz assignuju pole s mensim poctem elementu nez co je toto pole, tak se pocet elementu nezmensi!? takze treba 6 se assigne a 7. zustane...
  Object.assign(_skillsInHand, newHand);

  //obnovim jakekoliv modifikace  skillu na originalni
  _skillsInHand.forEach(skill => {
    Object.assign(skill.amounts, skill.originalStats.amountsSkill);
    if (skill.buff != undefined)
      Object.assign(skill.buff.amounts, skill.originalStats.amountsBuff);

    skill.manaCost = skill.originalStats.manaCost;

  });

  //ACID_BLISTERS CURSE
  let acitBlistersCurses = _skillsInHand.filter(skill => skill.skillId == SKILL.CURSE_ACID_BLISTER);
  if (acitBlistersCurses.length > 0) {
    _skillsInHand.forEach(skill => { skill.addDefense(acitBlistersCurses[0].amounts[0] * acitBlistersCurses.length * -1, true) });
  }

  //aplikuji CONFUSION CURSE
  let confusionCurses = _skillsInHand.filter(skill => skill.skillId == SKILL.CURSE_MANA_COST_INCREASE);

  //aplikuiju tuto kletbu pokud sem ji drawnul...
  if (confusionCurses.length > 0) {
    console.log("ok lizul sis :" + confusionCurses.length + " confusion");
    let skillsInHandExcludingCursesOrUnplaybleSkills = _skillsInHand.filter(skill => skill.skillGroupId != SKILL_GROUP.CURSE && skill.manaCost != -1);//(skill.validTarget_Self || skill.validTarget_AnyAlly || skill.validTarget_AnyEnemy));
    if (skillsInHandExcludingCursesOrUnplaybleSkills.length > 0) {
      for (let index = 0; index < confusionCurses.length; index++) {
        let skillToCripple = skillsInHandExcludingCursesOrUnplaybleSkills[randomIntFromInterval(0, skillsInHandExcludingCursesOrUnplaybleSkills.length - 1)];
        skillToCripple.manaCost++;
        console.log("zvedam manacost za confusion: " + skillToCripple.skillId + " mana :" + skillToCripple.manaCost);
      }

    }
  }


  return shufflePerformed;
}


export function moveSkillToDiscardDeck(_combatMember: CombatMember, _skillUid: string) {

  for (const iterator of _combatMember.skillsInHand) {
    if (iterator.uid == _skillUid) {
      _combatMember.skillsDiscardDeck.push(iterator);
      _combatMember.skillsInHand.splice(_combatMember.skillsInHand.indexOf(iterator), 1);
      break;
    }
  }

}

export function moveSkillToExhaustDeck(_combatMember: CombatMember, _skillUid: string) {

  for (const iterator of _combatMember.skillsInHand) {
    if (iterator.uid == _skillUid) {
      _combatMember.skillsExhaustDeck.push(iterator);
      _combatMember.skillsInHand.splice(_combatMember.skillsInHand.indexOf(iterator), 1);
      break;
    }
  }

}

//returns drawn skills
export function drawSkill(_combatMember: CombatMember, _amountToDraw: number): Combatskill[] {

  let numberOfSkillsToBeDrawn = _amountToDraw;

  //pokud je v draw decku min skillu nez kolik chci drawnout
  if (_combatMember.skillsDrawDeck.length < _amountToDraw) {
    numberOfSkillsToBeDrawn -= _combatMember.skillsDrawDeck.length;
    _combatMember.skillsInHand = _combatMember.skillsInHand.concat(_combatMember.skillsDrawDeck.splice(0));
  }

  //pokud je draw deck prazdny
  if (_combatMember.skillsDrawDeck.length == 0) {
    //move discard deck back to drawdeck
    Object.assign(_combatMember.skillsDrawDeck, _combatMember.skillsDiscardDeck);

    //shuffle draw deck
    shuffleArray(_combatMember.skillsDrawDeck);

    //set all skills as not already played
    _combatMember.skillsDrawDeck.forEach(element => { element.alreadyUsed = false; });

    //clear discard deck
    _combatMember.skillsDiscardDeck.splice(0);
  }

  //draw cards from drawdeck
  let drawnSkills = _combatMember.skillsDrawDeck.slice(0, numberOfSkillsToBeDrawn);
  _combatMember.skillsInHand = _combatMember.skillsInHand.concat(drawnSkills);

  for (let index = 0; index < _combatMember.skillsInHand.length; index++) {
    _combatMember.skillsInHand[index].uid = firestoreAutoId();
  }

  //remove drawn cards from draw deck
  _combatMember.skillsDrawDeck.splice(0, numberOfSkillsToBeDrawn);

  //ACID_BLISTERS CURSE
  let acitBlistersCurses = drawnSkills.filter(skill => skill.skillId == SKILL.CURSE_ACID_BLISTER);
  if (acitBlistersCurses.length > 0) {
    _combatMember.skillsInHand.forEach(skill => { skill.addDefense(acitBlistersCurses[0].amounts[0] * acitBlistersCurses.length * -1, true) });
  }

  //pokud sem drawnul zrovna tuto kletbu
  let confusionCurses = drawnSkills.filter(skill => skill.skillId == SKILL.CURSE_MANA_COST_INCREASE)
  if (confusionCurses.length > 0) {
    let skillsInHandExcludingCursesOrUnplaybleSkills = _combatMember.skillsInHand.filter(skill => skill.skillGroupId != SKILL_GROUP.CURSE && skill.manaCost != -1);//(skill.validTarget_Self || skill.validTarget_AnyAlly || skill.validTarget_AnyEnemy));
    if (skillsInHandExcludingCursesOrUnplaybleSkills.length > 0) {
      for (let index = 0; index < confusionCurses.length; index++) {
        skillsInHandExcludingCursesOrUnplaybleSkills[randomIntFromInterval(0, skillsInHandExcludingCursesOrUnplaybleSkills.length - 1)].manaCost++;

      }

    }
  }

  return drawnSkills;

}







// [END allAdd]