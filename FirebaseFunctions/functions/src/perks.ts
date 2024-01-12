
// [START import]
import * as functions from "firebase-functions";
import { ContentContainer, CharacterDocument, characterDocumentConverter, generateContentContainer, SimpleTally, GlobalMetadata } from ".";


import { generateContent, generateEquip, IHasChanceToSpawn, ItemIdWithAmount, QuerryForSkillDefinitions } from "./equip";
import { RandomEquip } from "./questgiver";
import { SkillDefinitions } from "./skills";


const admin = require('firebase-admin');
// // [END import]

//tohle je definice perku ktere si hraci vybiraji pak? tohle je totiz ted definice ktera se zkopci do POI ktereho se tyka a pak se tam modifikuje takove to stockclaimed atd...
export class PerkOfferDefinition implements IHasChanceToSpawn {
  constructor(
    public uid: string,

    public timePrice: number,

    public rarity: string,
    public curseCount: number,

    public restrictionClass: SimpleTally[],
    public restictionProfession: SimpleTally[],

    public rewards: ContentContainer[], //tohle je presny content. Tedy equip nebo i bezny item s presne definovanyma vecma jako price a stackSize atd...defakto nevyuzivane..presnou kopii dostanes do inventare
    public rewardsRandomEquip: RandomEquip[], // tohle je nahodny equip....co to bude presne se dogeneruje
    public rewardsGenerated: ItemIdWithAmount[], //tohle jsou bezne itemy, kde vim ID a dogeneruje se u nich ty blbosti, stcksize a u jabka a lahvi kolik leci atd nez se pridaji do inventare a stane se z nich content container

    public isInstantReward: boolean, //pokud je instantni, udeli se hned po boji a pak zmizi
    public recurrenceInGameDays: number, //kazdych X dni lze ziskat rewardy z perku
    public rewardAtSpecificGameDay: number, //lze claimnout jen ve specificky game day , pak se znici
    public rewardAfterSpecificGameDay: number, //lze claimnout az kdyz se dosahne specifickeho dne, pak se znici
    public charges: number, //po vycerpani charges perkOffer zmizi

    public specialEffectId: SimpleTally[],
    public chanceToSpawn: number,
    public stockLeft: number, //neintuitivne je toto kolik je tam supply maximalne nazacatku (nechce s emi to prejmenovavat vsude v databazi)
    public stockClaimed: number, //kolik uz lidi claimnuli supply

    public rarePerkGroupId: string, //pokud se jedna o rare perk , tak ma tady vyplneno ke kteremu rare perk Id patri...vim pak ze se jedna o rare perk a muzu si dohledat jeho "matersky" rare perk kteremu pak snizim ty sockleft

    //neimplementoval
    public floorMin: number, //kde se muze objevit
    public floorMax: number,
    public countMax: number, // kolikrat se muze objeviT?
    public countMin: number
  ) { }
}


// //to co dostavaji hraci a maji v perk decku?
// export class PerkOffer {
//   constructor(
//     public uid: string,
//     public rarity: string,

//     public rewards: ContentContainer[], //tohle je presny content. Tedy equip nebo i bezny item s presne definovanyma vecma jako price a stackSize atd...defakto nevyuzivane..presnou kopii dostanes do inventare
//     public rewardsRandomEquip: RandomEquip[], // tohle je nahodny equip....co to bude presne se dogeneruje
//     public rewardsGenerated: ItemIdWithAmount[], //tohle jsou bezne itemy, kde vim ID a dogeneruje se u nich ty blbosti, stcksize a u jabka a lahvi kolik leci atd nez se pridaji do inventare a stane se z nich content container

//     public isInstantReward: boolean, //pokud je instantni, udeli se hned po boji a pak zmizi
//     public recurrenceInGameDays: number, //kazdych X dni lze ziskat rewardy z perku
//     public rewardAtSpecificGameDay: number, //lze claimnout jen ve specificky game day , pak se znici
//     public rewardAfterSpecificGameDay: number, //lze claimnout az kdyz se dosahne specifickeho dne, pak se znici
//     public charges: number, //po vycerpani charges perkOffer zmizi

//     //  public specialEffectId: SimpleTally[],

//     public lastClaimGameDay: number, //ktery den sem naposledy claimnul reward
//     public chargesClaimed: number  // kolikrat sem claimnul reward
//     //public successPrice :number 

//   ) { }
// }

export class PendingReward {
  constructor(
    public uid: string,
    public rarity: string,

    public rewards: ContentContainer[], //tohle je presny content. Tedy equip nebo i bezny item s presne definovanyma vecma jako price a stackSize atd...defakto nevyuzivane..presnou kopii dostanes do inventare
    public rewardsRandomEquip: RandomEquip[], // tohle je nahodny equip....co to bude presne se dogeneruje
    public rewardsGenerated: ItemIdWithAmount[], //tohle jsou bezne itemy, kde vim ID a dogeneruje se u nich ty blbosti, stcksize a u jabka a lahvi kolik leci atd nez se pridaji do inventare a stane se z nich content container

    public isInstantReward: boolean, //pokud je instantni, udeli se hned po boji a pak zmizi
    public recurrenceInGameDays: number, //kazdych X dni lze ziskat rewardy z perku
    public rewardAtSpecificGameDay: number, //lze claimnout jen ve specificky game day , pak se znici
    public rewardAfterSpecificGameDay: number, //lze claimnout az kdyz se dosahne specifickeho dne, pak se znici
    public charges: number, //po vycerpani charges perkOffer zmizi

    //  public specialEffectId: SimpleTally[],

    public lastClaimGameDay: number, //ktery den sem naposledy claimnul reward
    public chargesClaimed: number  // kolikrat sem claimnul reward
    //public successPrice :number 

  ) { }


  resolve(_character: CharacterDocument, _gameDay: number, skillDefinitions: SkillDefinitions) {

    this.grantRewards(_character, _gameDay, skillDefinitions);

    /*  if (this.isInstantReward) {
        this.grantRewards(_character, _gameDay, skillDefinitions);
        this.remove(_character);
      }
  
      else if (this.recurrenceInGameDays > 0) {
        //if (this.lastClaimGameDay == -1) this.lastClaimGameDay = 0;
        console.log(this.lastClaimGameDay);
        if (_gameDay >= this.lastClaimGameDay + this.recurrenceInGameDays) {// || this.lastClaimGameDay == -1) { //utekly dny nebo je to uplne novy recurrence perk
          this.grantRewards(_character, _gameDay, skillDefinitions);
  
          if (this.charges > 0) {
            if (this.chargesClaimed >= this.charges) {
              this.remove(_character);
            }
          }
        }
        // else
        // throw "You have already claimed this perk, you need to wait " + ((this.lastClaimGameDay + this.recurrenceInGameDays) - _gameDay) + "game day to claim again";
      }
  
      else if (this.rewardAtSpecificGameDay > 0) {
        if (this.rewardAtSpecificGameDay == _gameDay) {
          this.grantRewards(_character, _gameDay, skillDefinitions);
          this.remove(_character);
        }
      }
  
      else if (this.rewardAfterSpecificGameDay > 0) {
        if (this.rewardAfterSpecificGameDay <= _gameDay) {
          this.grantRewards(_character, _gameDay, skillDefinitions);
          this.remove(_character);
        }
      }
  
  */
  }



  // private remove(_character: CharacterDocument) {
  //   _character.pendingRewards = _character.pendingRewards.filter(item => item.uid !== this.uid);

  // }

  private grantRewards(_character: CharacterDocument, _gameDay: number, skillDefinitions: SkillDefinitions) {

    this.chargesClaimed++;
    this.lastClaimGameDay = _gameDay;
    for (const reward of this.rewards) {
      console.log("ok sem az tadyy");
      _character.addContentToInventory(reward, true, false);
    }

    if (this.rewardsRandomEquip.length > 0) {
      for (const reward of this.rewardsRandomEquip) {
        //Equip z perku je Bind on Pickup...nejde prodat na AH
        _character.addContentToInventory(generateContentContainer(generateEquip(reward.mLevel, reward.rarity, reward.equipSlotId, _character.characterClass, skillDefinitions, undefined, undefined, false)), true, false);
      }
    }

    for (const reward of this.rewardsGenerated) {
      _character.addContentToInventory(generateContentContainer(generateContent(reward.itemId, reward.amount)), true, false);
    }
  }
}


exports.pendingRewardClaim = functions.https.onCall(async (data, context) => {

  const callerCharacterUid = data.characterUid;
  const perkUid = data.perkUid; //kdyz je perk uid "" - znamena claimnout vsechny perky naraz

  const characterDb = await admin.firestore().collection('characters').doc(callerCharacterUid).withConverter(characterDocumentConverter);
  const globalDb = await admin.firestore().collection('_metadata_coreDefinitions').doc("Global");


  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {

      const characterDoc = await t.get(characterDb);
      let characterData: CharacterDocument = characterDoc.data();

      const globalDataDoc = await t.get(globalDb);
      let globalData: GlobalMetadata = globalDataDoc.data();

      if (globalData.gameDay <= characterData.lastClaimedGameDay)
        throw "You have already scavenged today!";

      characterData.lastClaimedGameDay = globalData.gameDay;

      let skillDefinitions = await QuerryForSkillDefinitions(t);

      if (perkUid != "") { //konkretni perk
        for (var perk of characterData.pendingRewards) {
          if (perkUid == perk.uid) {//|| perkUid == "") {
            try {
              // if (characterData.currency.scavengePoints >= SCAVENGE_CLAIM_COST)
              //   characterData.subScavengePoints(SCAVENGE_CLAIM_COST);
              // else
              //   characterData.subTime(SCAVENGE_CLAIM_COST_TIME);

              perk.resolve(characterData, globalData.gameDay, skillDefinitions);
            }
            catch (e) {
              throw e;
            }
            break;
            // if (perkUid != "")
            //   break;
          }
        }
      }
      else //all perky
      {
        if (characterData.pendingRewards.length == 0)
          throw "There is nothing to scavange";

        // if (characterData.currency.scavengePoints >= SCAVENGE_CLAIM_ALL_COST)
        //   characterData.subScavengePoints(SCAVENGE_CLAIM_ALL_COST);
        // else
        //   characterData.subTime(SCAVENGE_CLAIM_ALL_COST_TIME);

        for (var perk of characterData.pendingRewards) {
          try {
            perk.resolve(characterData, globalData.gameDay, skillDefinitions);
          }
          catch (e) {
            throw e;
          }

        }
      }

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

// export async function QuerryIncreasePerkStockClaimed_GET_ManualSET(_transaction: any, _perkDefinitionUid: string, _encounterResult: EncounterResult): Promise<{ tierDefinitions: any, dbPath: any }> {


//   const tiersDefinitionDb = admin.firestore().collection("_metadata_zones").doc(_encounterResult.position.zoneId).collection("locations").doc(_encounterResult.position.locationId).collection("pointsOfInterest").doc(_encounterResult.position.pointOfInterestId).collection("definitions").doc("TIERS").withConverter(TierDefinitionsConverter);
//   const tiersDefinitionDoc = await _transaction.get(tiersDefinitionDb);
//   let tiersDefinitionData: TierDefinitions = tiersDefinitionDoc.data();

//   if (tiersDefinitionData.increaseStockClaimedForPerk(_perkDefinitionUid)) //podarilo se nam najit perk mezi tiers data...koncime
//   {
//     return { tierDefinitions: tiersDefinitionData, dbPath: tiersDefinitionDb };
//   }
//   else //perk musi byt tedy mezi rare perky
//   {

//     const locationDefinitionDb = admin.firestore().collection("_metadata_zones").doc(_encounterResult.position.zoneId).collection("locations").doc(_encounterResult.position.locationId).withConverter(LocationConverter);
//     const locationDefinitionDoc = await _transaction.get(locationDefinitionDb);
//     let locationDefinitionData: MapLocation = locationDefinitionDoc.data();
//     locationDefinitionData.increaseStockClaimedForRarePerk()

//   }

// }



// [END allAdd]