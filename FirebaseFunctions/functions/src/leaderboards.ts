
import * as functions from "firebase-functions";

// [START import]
import { CharacterDocument, CharacterPreview, ContentContainer, generateContentContainer, getCurrentDateTime, getCurrentDateTimeInMillis, hourstoMillis, millisToHours, WorldPosition } from ".";
import { EncounterResult, EncounterResultCombatant } from "./encounterResult";
import { generateContent, generateEquip, ItemIdWithAmount, QuerryForSkillDefinitions } from "./equip";
import { sendContentToInbox } from "./inbox";
import { RandomEquip } from "./questgiver";
import { LOC } from "./worldMap";
import { InboxItem, SetOperation } from "./utils";

const admin = require('firebase-admin');
// // [END import]

export enum LEADERBOARD {
  CHARACTER_LEVEL = "CHARACTER_LEVEL",
  MONSTER_KILLS = "MONSTER_KILLS",
  ITEMS_CRAFTED = "ITEMS_CRAFTED",
  HEALING_DONE = "HEALING_DONE",
  DAMAGE_DONE = "DAMAGE_DONE",
  DUNGEON_ENDGAME = "DUNGEON_ENDGAME",
  // LOCATION_VALLEY_OF_TRIALS = "LOCATION_VALLEY_OF_TRIALS",
  // LOCATION_VILLAGE_OF_MALAKA = "LOCATION_VILLAGE_OF_MALAKA",
  // LOCATION_DEEP_RAVINE = "LOCATION_DEEP_RAVINE",
  // LOCATION_MALAKA_DUNGEON = "LOCATION_MALAKA_DUNGEON",

}

export enum LEADERBOARD_SCORE_TYPE {
  CHARACTER_LEVEL = "CHARACTER_LEVEL",
  MONSTER_KILLS = "MONSTER_KILLS",
  DAMAGE_DONE = "DAMAGE_DONE",
  HEALING_DONE = "HEALING_DONE",
  ITEMS_CRAFTED = "ITEMS_CRAFTED",
  FLOOR_REACHED = "FLOOR_REACHED",
}

export enum LEADERBOARD_TYPE {
  GLOBAL = "GLOBAL",
  //LOCATION_BASED = "LOCATION_BASED",
}

export class LeaderboardScoreEntry {
  constructor(
    public character: CharacterPreview,
    public score: number

  ) { }
}

export class LeaderboardReward {
  constructor(
    public rankMin: number,
    public rankMax: number,

    public generatedContent: ItemIdWithAmount[] | undefined, //definice obyc itemu
    public content: ContentContainer[] | undefined, //specificky content/equip
    public randomEquip: RandomEquip[] | undefined, //random equip,


  ) { }
}


export class LeaderboardBaseData {
  constructor(
    public rewards: LeaderboardReward[],
    public resetInterval: string,
    public timestampNextReset: string,
    public scoreType: string,
    // public isSeasonal: boolean //seasonal leaderboardy trvaji celou sezonu a jsou rewardovane na konci sezonu


  ) { }

  getRewardForRank(_rank: number): LeaderboardReward | undefined {
    for (const reward of this.rewards) {
      if (_rank >= reward.rankMin && _rank <= reward.rankMax) {
        return reward;
      }
    }
    return undefined;
  }
}


export const LeaderboardBaseDataConverter = {
  toFirestore: (_entry: LeaderboardBaseData) => {
    return {
      rewards: _entry.rewards,
      resetInterval: _entry.resetInterval,
      timestampNextReset: _entry.timestampNextReset,
      scoreType: _entry.scoreType
    };
  },
  fromFirestore: (snapshot: any, options: any) => {
    const data = snapshot.data(options);
    return new LeaderboardBaseData(data.rewards, data.resetInterval, data.timestampNextReset, data.scoreType);
  }
};




const LEADERBOARDS_DEFINITIONS =
{
  "leaderboards":
    [
      {
        "leaderboardId": LEADERBOARD.MONSTER_KILLS,
        "leaderboardScoreType": LEADERBOARD_SCORE_TYPE.MONSTER_KILLS,
        "leaderboardType": LEADERBOARD_TYPE.GLOBAL,
        "location": LOC.NONE
      },
      {
        "leaderboardId": LEADERBOARD.CHARACTER_LEVEL,
        "leaderboardScoreType": LEADERBOARD_SCORE_TYPE.CHARACTER_LEVEL,
        "leaderboardType": LEADERBOARD_TYPE.GLOBAL,
        "location": LOC.NONE
      },
      {
        "leaderboardId": LEADERBOARD.ITEMS_CRAFTED,
        "leaderboardScoreType": LEADERBOARD_SCORE_TYPE.ITEMS_CRAFTED,
        "leaderboardType": LEADERBOARD_TYPE.GLOBAL,
        "location": LOC.NONE
      },
      {
        "leaderboardId": LEADERBOARD.HEALING_DONE,
        "leaderboardScoreType": LEADERBOARD_SCORE_TYPE.HEALING_DONE,
        "leaderboardType": LEADERBOARD_TYPE.GLOBAL,
        "location": LOC.NONE
      },
      {
        "leaderboardId": LEADERBOARD.DAMAGE_DONE,
        "leaderboardScoreType": LEADERBOARD_SCORE_TYPE.DAMAGE_DONE,
        "leaderboardType": LEADERBOARD_TYPE.GLOBAL,
        "location": LOC.NONE
      },
      {
        "leaderboardId": LEADERBOARD.DUNGEON_ENDGAME,
        "leaderboardScoreType": LEADERBOARD_SCORE_TYPE.FLOOR_REACHED,
        "leaderboardType": LEADERBOARD_TYPE.GLOBAL,
        "location": LOC.NONE
      },
      // {
      //   "leaderboardId": LEADERBOARD.LOCATION_DEEP_RAVINE,
      //   "leaderboardScoreType": LEADERBOARD_SCORE_TYPE.MONSTER_KILLS,
      //   "leaderboardType": LEADERBOARD_TYPE.LOCATION_BASED,
      //   "location": LOC.DEEP_RAVINE
      // },
      // {
      //   "leaderboardId": LEADERBOARD.LOCATION_VALLEY_OF_TRIALS,
      //   "leaderboardScoreType": LEADERBOARD_SCORE_TYPE.MONSTER_KILLS,
      //   "leaderboardType": LEADERBOARD_TYPE.LOCATION_BASED,
      //   "location": LOC.VALLEY_OF_TRIALS
      // },
      // {
      //   "leaderboardId": LEADERBOARD.LOCATION_VILLAGE_OF_MALAKA,
      //   "leaderboardScoreType": LEADERBOARD_SCORE_TYPE.MONSTER_KILLS,
      //   "leaderboardType": LEADERBOARD_TYPE.LOCATION_BASED,
      //   "location": LOC.VILLAGE_OF_MALAKA
      // },
      // {
      //   "leaderboardId": LEADERBOARD.LOCATION_MALAKA_DUNGEON,
      //   "leaderboardScoreType": LEADERBOARD_SCORE_TYPE.DAMAGE_DONE,
      //   "leaderboardType": LEADERBOARD_TYPE.LOCATION_BASED,
      //   "location": LOC.MALAKA_DUNGEON
      // },
    ],

}



export async function addMonsterKillsToLeaderboards_GET_SET(_transaction: any, _characterData: CharacterDocument, _position: WorldPosition, _amountToAdd: number) {


}

//Toto je trochu specialni, ze ,i staci character data a nemusim getovat entry, protoze score= level a jen ho prepisuju vetsim nic nescitam atd
export async function updateMyPortraitAtAllLeaderboards(_transaction: any, _character: CharacterDocument, portraitId: string) {

  const leaderboardsDb_CHARACTER_LEVEL = await admin.firestore().collection('leaderboards').doc(LEADERBOARD.CHARACTER_LEVEL).collection("season" + _character.seasonNumber).doc(_character.uid);
  const leaderboardsDb_MONSTER_KILLS = await admin.firestore().collection('leaderboards').doc(LEADERBOARD.MONSTER_KILLS).collection("season" + _character.seasonNumber).doc(_character.uid);
  const leaderboardsDb_ITEMS_CRAFTED = await admin.firestore().collection('leaderboards').doc(LEADERBOARD.ITEMS_CRAFTED).collection("season" + _character.seasonNumber).doc(_character.uid);
  const leaderboardsDb_DUNGEON_ENDGAME = await admin.firestore().collection('leaderboards').doc(LEADERBOARD.DUNGEON_ENDGAME).collection("season" + _character.seasonNumber).doc(_character.uid);
  //moje zaznamy v leaderboardech....zatim jen v character levle leaderboardu

  const leaderboardsDoc_CHARACTER_LEVEL = await _transaction.get(leaderboardsDb_CHARACTER_LEVEL);
  let leaderboardsData_CHARACTER_LEVEL: LeaderboardScoreEntry = leaderboardsDoc_CHARACTER_LEVEL.data();

  const leaderboardsDoc_MONSTER_KILLS = await _transaction.get(leaderboardsDb_MONSTER_KILLS);
  let leaderboardsData_MONSTER_KILLS: LeaderboardScoreEntry = leaderboardsDoc_MONSTER_KILLS.data();

  const leaderboardsDoc_ITEMS_CRAFTED = await _transaction.get(leaderboardsDb_ITEMS_CRAFTED);
  let leaderboardsData_ITEMS_CRAFTED: LeaderboardScoreEntry = leaderboardsDoc_ITEMS_CRAFTED.data();

  const leaderboardsDoc_DUNGEON_ENDGAME = await _transaction.get(leaderboardsDb_DUNGEON_ENDGAME);
  let leaderboardsData_DUNGEON_ENDGAME: LeaderboardScoreEntry = leaderboardsDoc_DUNGEON_ENDGAME.data();

  if (leaderboardsData_DUNGEON_ENDGAME != undefined) {
    leaderboardsData_DUNGEON_ENDGAME.character.portrait = portraitId
    _transaction.set(leaderboardsDb_DUNGEON_ENDGAME, JSON.parse(JSON.stringify(leaderboardsData_DUNGEON_ENDGAME)), { merge: true });
  }


  if (leaderboardsData_MONSTER_KILLS != undefined) {
    leaderboardsData_MONSTER_KILLS.character.portrait = portraitId
    _transaction.set(leaderboardsDb_MONSTER_KILLS, JSON.parse(JSON.stringify(leaderboardsData_MONSTER_KILLS)), { merge: true });
  }

  if (leaderboardsData_CHARACTER_LEVEL != undefined) {
    leaderboardsData_CHARACTER_LEVEL.character.portrait = portraitId
    _transaction.set(leaderboardsDb_CHARACTER_LEVEL, JSON.parse(JSON.stringify(leaderboardsData_CHARACTER_LEVEL)), { merge: true });
  }


  if (leaderboardsData_ITEMS_CRAFTED != undefined) {
    leaderboardsData_ITEMS_CRAFTED.character.portrait = portraitId
    _transaction.set(leaderboardsDb_ITEMS_CRAFTED, JSON.parse(JSON.stringify(leaderboardsData_ITEMS_CRAFTED)), { merge: true });
  }
}


//Toto je trochu specialni, ze ,i staci character data a nemusim getovat entry, protoze score= level a jen ho prepisuju vetsim nic nescitam atd
export async function setMyCharacterLevelLeaderboards(_transaction: any, _characterData: CharacterDocument): Promise<SetOperation> {

  const leaderboardsD_CHARACTER_LEVEL = admin.firestore().collection('leaderboards').doc(LEADERBOARD.CHARACTER_LEVEL).collection("season" + _characterData.seasonNumber);
  let preview = new CharacterPreview(_characterData.uid, _characterData.characterName, _characterData.characterClass, _characterData.stats.level, _characterData.characterPortrait, _characterData.userUid, _characterData.seasonNumber, _characterData.isRetired);
  let leaderboardScoreEntry: LeaderboardScoreEntry = new LeaderboardScoreEntry(preview, _characterData.stats.level);

  // _transaction.set(leaderboardsD_CHARACTER_LEVEL.doc(_characterData.uid), JSON.parse(JSON.stringify(leaderboardScoreEntry)), { merge: true });

  return {
    docRef: leaderboardsD_CHARACTER_LEVEL.doc(_characterData.uid),
    data: JSON.parse(JSON.stringify(leaderboardScoreEntry)),
    options: { merge: true }
  };
}

// export async function updateMyItemsCraftedLeaderboards(_transaction: any, _characterData: CharacterDocument, _newItemsCraftedAmount: number) {


//   let leaderboardDbMyEntryDb = admin.firestore().collection('leaderboards').doc(LEADERBOARD.ITEMS_CRAFTED).collection("leaderboard").doc(_characterData.uid);
//   let characterPreview = new CharacterPreview(_characterData.uid, _characterData.characterName, _characterData.characterClass, _characterData.stats.level, _characterData.characterPortrait);
//   let leaderboardScoreEntry: LeaderboardScoreEntry = new LeaderboardScoreEntry(characterPreview, 0);

//   //leaderboardDbMyEntryDbs.push(leaderboardDbMyEntryDb);

//   let leaderboardsDbMyEntryDoc = await _transaction.get(leaderboardDbMyEntryDb);
//   if (leaderboardsDbMyEntryDoc.data() != undefined) {
//     leaderboardScoreEntry = leaderboardsDbMyEntryDoc.data();
//   }
//   leaderboardScoreEntry.score += _newItemsCraftedAmount;// amountToAdd;


//   await _transaction.set(leaderboardDbMyEntryDb, JSON.parse(JSON.stringify(leaderboardScoreEntry)), { merge: true });
// }


export async function setScoreToLeaderboard(_transaction: any, _characterData: CharacterDocument, _leaderboardId: string, _amountToSet: number): Promise<SetOperation> {


  let leaderboardDbMyEntryDb = admin.firestore().collection('leaderboards').doc(_leaderboardId).collection("season" + _characterData.seasonNumber).doc(_characterData.uid);
  let characterPreview = new CharacterPreview(_characterData.uid, _characterData.characterName, _characterData.characterClass, _characterData.stats.level, _characterData.characterPortrait, _characterData.userUid, _characterData.seasonNumber, _characterData.isRetired);
  let leaderboardScoreEntry: LeaderboardScoreEntry = new LeaderboardScoreEntry(characterPreview, _amountToSet);

  // tady je divne ze set dela get ? naco?

  // let leaderboardsDbMyEntryDoc = await _transaction.get(leaderboardDbMyEntryDb);
  // if (leaderboardsDbMyEntryDoc.data() != undefined) {
  //   leaderboardScoreEntry = leaderboardsDbMyEntryDoc.data();
  // }
  // leaderboardScoreEntry.score = _amountToSet;// amountToAdd;

  return {
    docRef: leaderboardDbMyEntryDb,
    data: JSON.parse(JSON.stringify(leaderboardScoreEntry)),
    options: { merge: true }
  };

  //await _transaction.set(leaderboardDbMyEntryDb, JSON.parse(JSON.stringify(leaderboardScoreEntry)), { merge: true });
}



export async function incrementScoreToLeaderboard(_transaction: any, _characterData: CharacterDocument, _leaderboardId: string, _amountToAdd: number): Promise<SetOperation> {


  let leaderboardDbMyEntryDb = admin.firestore().collection('leaderboards').doc(_leaderboardId).collection("season" + _characterData.seasonNumber).doc(_characterData.uid);
  let characterPreview = new CharacterPreview(_characterData.uid, _characterData.characterName, _characterData.characterClass, _characterData.stats.level, _characterData.characterPortrait, _characterData.userUid, _characterData.seasonNumber, _characterData.isRetired);
  let leaderboardScoreEntry: LeaderboardScoreEntry = new LeaderboardScoreEntry(characterPreview, 0);


  let leaderboardsDbMyEntryDoc = await _transaction.get(leaderboardDbMyEntryDb);
  if (leaderboardsDbMyEntryDoc.data() != undefined) {
    leaderboardScoreEntry = leaderboardsDbMyEntryDoc.data();
  }
  leaderboardScoreEntry.score += _amountToAdd;// amountToAdd;

  return {
    docRef: leaderboardDbMyEntryDb,
    data: JSON.parse(JSON.stringify(leaderboardScoreEntry)),
    options: { merge: true }
  };

  //await _transaction.set(leaderboardDbMyEntryDb, JSON.parse(JSON.stringify(leaderboardScoreEntry)), { merge: true });
}

//TODO: pripadne jen predavat uid charakteru, a tahle metoda pokud nenajde zaznam mohla ziskat charakter, pokud najde nemusi ho ziskavat a pouzije zaznam z leaderboards
//docela brutalni...pokazde kdyz dokoncis encounter tak tato metoda udela klidne 3x get a 3x set....to je mazec
export async function updateMyMonsterKillsAndDamageDoneLeaderboards_GET_SET(_transaction: any, _encounterResult: EncounterResult, _encounterResultMyEntry: EncounterResultCombatant, _characterData: CharacterDocument) {

  let leaderboardsIds: string[] = [];
  let leaderboardsAmountsToAdd: number[] = [];
  let amountToAdd: number = 0;
  //vyberu spravne leaderboardy
  LEADERBOARDS_DEFINITIONS.leaderboards.forEach(leaderboardDefinition => {

    if (leaderboardDefinition.leaderboardScoreType == LEADERBOARD_SCORE_TYPE.MONSTER_KILLS) {
      amountToAdd = _encounterResult.enemies.length;

      if (amountToAdd > 0) {
        if (leaderboardDefinition.leaderboardType == LEADERBOARD_TYPE.GLOBAL) {
          leaderboardsIds.push(leaderboardDefinition.leaderboardId);
          leaderboardsAmountsToAdd.push(amountToAdd);
        }
      }
      // else if (leaderboardDefinition.leaderboardType == LEADERBOARD_TYPE.LOCATION_BASED) {
      //   if (leaderboardDefinition.location == _encounterResult.position.locationId) {
      //     leaderboardsIds.push(leaderboardDefinition.leaderboardId);
      //     leaderboardsAmountsToAdd.push(amountToAdd);
      //   }
      // }
    }
    else if (leaderboardDefinition.leaderboardScoreType == LEADERBOARD_SCORE_TYPE.DAMAGE_DONE) {
      amountToAdd = _encounterResultMyEntry.damageDone;

      if (amountToAdd > 0) {
        if (leaderboardDefinition.leaderboardType == LEADERBOARD_TYPE.GLOBAL) {

          leaderboardsIds.push(leaderboardDefinition.leaderboardId);
          leaderboardsAmountsToAdd.push(amountToAdd);
        }
      }
    }
    else if (leaderboardDefinition.leaderboardScoreType == LEADERBOARD_SCORE_TYPE.HEALING_DONE) {
      amountToAdd = _encounterResultMyEntry.healingDone;

      if (amountToAdd > 0) {
        if (leaderboardDefinition.leaderboardType == LEADERBOARD_TYPE.GLOBAL) {

          leaderboardsIds.push(leaderboardDefinition.leaderboardId);
          leaderboardsAmountsToAdd.push(amountToAdd);
        }
      }
    }
    // else if (leaderboardDefinition.leaderboardType == LEADERBOARD_TYPE.LOCATION_BASED) {
    //   if (leaderboardDefinition.location == _encounterResult.position.locationId) {
    //     leaderboardsIds.push(leaderboardDefinition.leaderboardId);
    //     leaderboardsAmountsToAdd.push(amountToAdd);
    //   }
    // }

  });



  let leaderboardScoreEntries: LeaderboardScoreEntry[] = [];
  //var leaderboardDbMyEntryDbs: any[] = [];

  for (let i = 0; i < leaderboardsIds.length; i++) {

    let leaderboardDbMyEntryDb = admin.firestore().collection('leaderboards').doc(leaderboardsIds[i]).collection("season" + _characterData.seasonNumber).doc(_characterData.uid);
    let characterPreview = new CharacterPreview(_characterData.uid, _characterData.characterName, _characterData.characterClass, _characterData.stats.level, _characterData.characterPortrait, _characterData.userUid, _characterData.seasonNumber, _characterData.isRetired);
    let leaderboardScoreEntry: LeaderboardScoreEntry = new LeaderboardScoreEntry(characterPreview, 0);

    //leaderboardDbMyEntryDbs.push(leaderboardDbMyEntryDb);

    let leaderboardsDbMyEntryDoc = await _transaction.get(leaderboardDbMyEntryDb);
    if (leaderboardsDbMyEntryDoc.data() != undefined) {
      leaderboardScoreEntry = leaderboardsDbMyEntryDoc.data();
    }
    leaderboardScoreEntry.score += leaderboardsAmountsToAdd[i]// amountToAdd;
    leaderboardScoreEntries.push(leaderboardScoreEntry);


  }




  for (let index = 0; index < leaderboardsIds.length; index++) {
    // console.log(leaderboardScoreEntries.length);
    // console.log((leaderboardScoreEntries[index]));
    await _transaction.set(admin.firestore().collection('leaderboards').doc(leaderboardsIds[index]).collection("season" + _characterData.seasonNumber).doc(_characterData.uid), JSON.parse(JSON.stringify(leaderboardScoreEntries[index])), { merge: true });
  }

}

export async function awardSeasonalLeaderboardRewards(_leaderboardId: string, _seasonNumber: number) {


  const batch = admin.firestore().batch();
  const leaderboardDb = await admin.firestore().collection('leaderboards').doc(_leaderboardId).withConverter(LeaderboardBaseDataConverter);
  const top100leaderboardEntriesDb = await leaderboardDb.collection("season" + _seasonNumber).orderBy("score", "desc").limit(100);

  try {
    await admin.firestore().runTransaction(async (t: any) => {

      const leaderboardDoc = await t.get(leaderboardDb);
      const leaderboardData: LeaderboardBaseData = leaderboardDoc.data();
      const top100LeaderboardEntriesSnapshot = await t.get(top100leaderboardEntriesDb);

      let rank = 0;
      // await top100LeaderboardEntriesSnapshot.then(querry => {
      //   querry.docs.forEach(doc => {
      for (const doc of top100LeaderboardEntriesSnapshot.docs) {
        rank++;
        let entry: LeaderboardScoreEntry = doc.data();
        let reward = leaderboardData.getRewardForRank(rank);
        //PROTOZE generateContent prasacky slouzi serveru jako referencni hodnoty, ktere se v AdminTools rozkopirujou na content a pripadne pozmeni....udeluje se jen Content potom. Generated vubec
        if (reward != null && reward.content != null) {
          for (const content of reward.content) {
            const inboxDb = admin.firestore().collection('inboxPlayer').doc();
            const inboxEntry = new InboxItem(inboxDb.id, entry.character.playerUid, content, "Season " + _seasonNumber + " reward", "Your hero " + entry.character.name + " placed at " + rank + ". place in " + _leaderboardId + " leaderboard! Here is your reward!", getCurrentDateTime(480));
            batch.set(inboxDb, JSON.parse(JSON.stringify(inboxEntry))); // Update the document in batch
            //});
          }
        }
      }
    });

    return await batch.commit()
      .then(() => {
        console.log("Batch update completed.");
        console.log("Reward given for leaderboard -" + _leaderboardId);
        return "Batch update successful";
      })
      .catch((e) => {
        console.error("Batch update failed: ", e);
        throw new functions.https.HttpsError("aborted", "Batch update failed: " + e);
      });
  } catch (e) {
    console.log('Transaction failure:', e);
    throw new functions.https.HttpsError("aborted", "Transaction failure: " + e);
  }
}
//     batch.commit() // Commit the batch
//       .then(() => {
//         console.log("Batch update completed.");
//         console.log("Reward given for leaderboard -" + _leaderboardId);
//       })
//       .catch((e) => {
//         console.error("Batch update failed: ", e);
//         throw new functions.https.HttpsError("aborted", "season leaderboard award Batch update failed: " + e);
//       });
//   });

// });



// console.log('Transaction success', result);
// return result;
//   } catch (e) {
//   console.log('Transaction failure:', e);
//   throw new functions.https.HttpsError("aborted", "Error : " + e);
// }

// }

// const leaderboardSnapshot = await leaderboardDb.get();

// leaderboardSnapshot.forEach(async doc => {

//   let leaderboard: LeaderboardBaseData = doc.data();

//   //ziskam dokumenty TOP 100 hracu a ulozim si je
//   const top100leaderboardEntriesDb = leaderboardDb.collection("leaderboard").orderBy("score", "desc").limit(100);
//   const top100LeaderboardEntries: LeaderboardScoreEntry[] = [];

//   await t.get(top100leaderboardEntriesDb).then(querry => {
//     // let rank = 0;
//     querry.docs.forEach(async doc => {
//       //   rank++;
//       let entry: LeaderboardScoreEntry = doc.data();
//       top100LeaderboardEntries.push(new LeaderboardScoreEntry(new CharacterPreview(entry.character.uid, entry.character.name, entry.character.characterClass, entry.character.level, entry.character.portrait), entry.score));
//     });
//   });

//   batch.update(doc.ref, JSON.parse(JSON.stringify(leaderboard)), { merge: true }); // Update the document in batch
// });

// return batch.commit() // Commit the batch
//   .then(() => {
//     console.log("Batch update completed.");
//     console.log("Reward given for leaderboard - Number of effected characters: ", leaderboardSnapshot.size);
//   }
//   )
//   .catch(err => console.error("Error: ", err));


// }


export async function checkForLeaderboardReset(_leaderboardId: string) {

  //  console.log('checking for leaderboard expiration : ' + _leaderboardId);
  const leaderboardDb = await admin.firestore().collection('leaderboards').doc(_leaderboardId).withConverter(LeaderboardBaseDataConverter);
  const allleaderboardEntriesDb = await leaderboardDb.collection("leaderboard");
  const top100leaderboardEntriesDb = await leaderboardDb.collection("leaderboard").orderBy("score", "desc").limit(100);

  try {
    const result = await admin.firestore().runTransaction(async (t: any) => {


      const leaderboardDoc = await t.get(leaderboardDb);
      const leaderboardData: LeaderboardBaseData = leaderboardDoc.data();



      //zjsitime jestli leaderboard expiroval
      const currentTime = getCurrentDateTimeInMillis(0);
      const leaderboardExpired = currentTime + 600000 > Number.parseInt(leaderboardData.timestampNextReset);  //pridam 10 minut pro jistotu aby se to neschazeno v uplne stejny cas...


      if (leaderboardExpired) {
        console.log('LEADERBOARD EXPIRED! : ' + _leaderboardId);

        //nastavime kdy bude expirovat priste podle jeho intervali

        if (leaderboardData.resetInterval == "30MINUTES") {
          leaderboardData.timestampNextReset = (Number.parseInt(leaderboardData.timestampNextReset) + (hourstoMillis(1) / 2)).toString(); //zatim dam jen 30 min
        }
        else if (leaderboardData.resetInterval == "DAY") {
          leaderboardData.timestampNextReset = (Number.parseInt(leaderboardData.timestampNextReset) + (hourstoMillis(24))).toString();
        }
        else if (leaderboardData.resetInterval == "3DAYS") {
          leaderboardData.timestampNextReset = (Number.parseInt(leaderboardData.timestampNextReset) + (hourstoMillis(24 * 3))).toString();
        }
        else if (leaderboardData.resetInterval == "7DAYS") {
          leaderboardData.timestampNextReset = (Number.parseInt(leaderboardData.timestampNextReset) + (hourstoMillis(24 * 7))).toString();
        }
        else if (leaderboardData.resetInterval == "10DAYS") {
          leaderboardData.timestampNextReset = (Number.parseInt(leaderboardData.timestampNextReset) + (hourstoMillis(24 * 10))).toString();
        }
        else if (leaderboardData.resetInterval == "14DAYS") {
          leaderboardData.timestampNextReset = (Number.parseInt(leaderboardData.timestampNextReset) + (hourstoMillis(24 * 14))).toString();
        }
        else if (leaderboardData.resetInterval == "30DAYS") {

          leaderboardData.timestampNextReset = (Number.parseInt(leaderboardData.timestampNextReset) + (hourstoMillis(24 * 30))).toString();
        }


        console.log("NextExpirationDate: " + leaderboardData.timestampNextReset);

        //ziskam dokumenty TOP 100 hracu a ulozim si je
        const top100LeaderboardEntries: LeaderboardScoreEntry[] = [];
        await t.get(top100leaderboardEntriesDb).then(querry => {
          // let rank = 0;
          querry.docs.forEach(async doc => {
            //   rank++;
            let entry: LeaderboardScoreEntry = doc.data();
            top100LeaderboardEntries.push(new LeaderboardScoreEntry(new CharacterPreview(entry.character.characterUid, entry.character.name, entry.character.characterClass, entry.character.level, entry.character.portrait, entry.character.playerUid, entry.character.seasonNumber, entry.character.isRetired), entry.score));
          });
        });

        //smazu leaderboard  
        await t.get(allleaderboardEntriesDb).then(snapshot => {
          snapshot.forEach(doc => {
            t.delete(doc.ref);
          });
        });

        //poslu kazemu z nich odmeny do inboxu
        for (let i = 0; i < top100LeaderboardEntries.length; i++) {

          let rank = i + 1;
          let reward = leaderboardData.getRewardForRank(rank);

          if (reward != undefined) {

            console.log("udeluji odmenu hraci na ranku :" + rank + " je to :" + top100LeaderboardEntries[i].character.name + "(" + top100LeaderboardEntries[i].character.characterUid + ") dostane reward z roptylu: " + reward.rankMin + "-" + reward.rankMax);

            if (reward.content != undefined) {
              reward.content.forEach(content => {
                sendContentToInbox(t, content, top100LeaderboardEntries[i].character.characterUid, "Reward from " + _leaderboardId + " Leaderboards", "You ended at " + rank + ". place in " + _leaderboardId + " leaderboard! Here is your reward!");
              });
            }

            if (reward.generatedContent != undefined) {
              reward.generatedContent.forEach(content => {
                sendContentToInbox(t, generateContentContainer(generateContent(content.itemId, content.amount)), top100LeaderboardEntries[i].character.characterUid, "Reward from " + _leaderboardId + " Leaderboards", "You ended at " + rank + ". place in " + _leaderboardId + " leaderboard! Here is your reward!");
              });
            }

            if (reward.randomEquip != undefined) {
              let skillDefinitions = await QuerryForSkillDefinitions(t);
              reward.randomEquip.forEach(content => {
                sendContentToInbox(t, generateContentContainer(generateEquip(content.mLevel, content.rarity, content.equipSlotId, top100LeaderboardEntries[i].character.characterClass, skillDefinitions)), top100LeaderboardEntries[i].character.characterUid, "Reward from " + _leaderboardId + " Leaderboards", "You ended at " + rank + ". place in " + _leaderboardId + " leaderboard! Here is your reward!");
              });
            }

          }
        }

        // //smazu leaderboard  
        // t.get(allleaderboardEntriesDb).then(snapshot => {
        //   snapshot.forEach(doc => {
        //     t.delete(doc.ref);
        //   });
        // });

        await t.set(leaderboardDb, JSON.parse(JSON.stringify(leaderboardData)));

      }
      else
        console.log('LEADERBOARD HAS NOT EXPIRED YET ! :' + _leaderboardId + " current time(with shift) : " + (currentTime + 600000) + " expiration time: " + leaderboardData.timestampNextReset + " time to reset(hours) : " + millisToHours(Number.parseInt(leaderboardData.timestampNextReset) - currentTime));

      return "leaderboard awards awarded " + _leaderboardId;
    });


    console.log('Transaction success', result);
    return result;
  } catch (e) {
    console.log('Transaction failure:', e);
    throw new functions.https.HttpsError("aborted", "Error : " + e);
  }

}



// [END allAdd]