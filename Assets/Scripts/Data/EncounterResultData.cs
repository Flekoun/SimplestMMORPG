using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Firebase.Firestore;
using simplestmmorpg.data;
using Unity.VisualScripting;
using UnityEngine;


namespace simplestmmorpg.data
{


    //public enum RARITY
    //{
    //    COMMON = "COMMON",
    //    UNCOMMON = "UNCOMMON",
    //    RARE = "RARE"
    //}

    //public enum ATTRIBUTE_ID
    //{
    //    STRENGTH = "STRENGTH",
    //    AGILITY = "AGILITY",
    //    INTELLECT = "INTELLECT",
    //    STAMINA = "STAMINA",
    //    SPIRIT = "SPIRIT",
    //}


    //public enum EQUIP_SLOT_ID
    //{
    //    HEAD = "HEAD",
    //    BODY = "BODY",
    //    LEGS = "LEGS",
    //    FINGER_1 = "FINGER_1",

    //}

    [Serializable]
    [FirestoreData]
    public class EncounterResult
    {

        [field: SerializeField]
        [FirestoreProperty]
        public string uid { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<EncounterResultEnemyData> enemies { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<string> combatantsList { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<string> combatantsWithUnclaimedRewardsList { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<string> combatantsWithUnchoosenWantedItemList { get; set; }

        //[field: SerializeField]
        //[FirestoreProperty]
        //public List<string> combatantsWhoAlreadyChoosenWantedItemList { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<EncounterResultCombatant> combatantsData { get; set; }


        [field: SerializeField]
        [FirestoreProperty]
        public int silver { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int wantItemChooseTimeAmount { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public bool wantItemPhaseFinished { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string expireDateWantItemPhase { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int turnsNumber { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public WorldPosition position { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<EncounterResultContentLoot> bonusLoot { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<EncounterResultContentLoot> dungeonLoot { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string dungeonFinished { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<PerkChoiceParticipant> perkChoices { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string foundBy { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public DungeonData dungeonData { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int tier { get; set; }




        public EncounterResultCombatant GetCombatantResultForUid(string _characterUid)
        {
            foreach (var item in combatantsData)
            {
                if (item.uid == _characterUid)
                    return item;
            }

            Debug.LogError("No combatant result with udi : " + _characterUid + " found!");
            return null;
        }

        public string GetWantTimerTimeLeftText()
        {

            int secondsLeft = GetWantTimerTimeLeft();


            if (secondsLeft > 0)
                return "<color=\"yellow\">" + Mathf.Ceil((float)secondsLeft) + "</color> sec left";
            else
                return "<color=\"yellow\">Collect your rewards</color>";

        }

        public int GetWantTimerTimeLeft()
        {
            double ExpireMilis = double.Parse(expireDateWantItemPhase);
            double NowInMilis = Utils.GetNowInMillis();


            double durationLeft = ExpireMilis - NowInMilis;

            double secondsLeft = durationLeft / 1000;


            return (int)secondsLeft;

        }

    }

    [Serializable]
    [FirestoreData]
    public class DungeonData
    {
        [field: SerializeField]
        [FirestoreProperty]
        public string dungeonId { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int tier { get; set; }


        [field: SerializeField]
        [FirestoreProperty]
        public bool isFinished { get; set; }


        [field: SerializeField]
        [FirestoreProperty]
        public bool isFinalDungeon { get; set; }


        [field: SerializeField]
        [FirestoreProperty]
        public bool isEndlessDungeon { get; set; }


    }

    [Serializable]
    [FirestoreData]
    public class EncounterResultEnemyData
    {
        [field: SerializeField]
        [FirestoreProperty]
        public string id { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string displayName { get; set; }


        [field: SerializeField]
        [FirestoreProperty]
        public int level { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<EncounterResultContentLoot> contentLoot { get; set; }

    }




    [Serializable]
    [FirestoreData]
    public class EncounterResultContentLoot
    {
        [field: SerializeField]
        [FirestoreProperty]
        public ContentContainer content { get; set; }


        [field: SerializeField]
        [FirestoreProperty]
        public List<EncounterResultCombatant> charactersWhoWantThis { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public EncounterResultCombatant characterWhoWillHaveThis { get; set; }


        public bool DoesAnyoneWillHaveThisItem()
        {
            if (characterWhoWillHaveThis == null) return false;
            Debug.Log(" characterWhoWillHaveThis.characterClass:" + characterWhoWillHaveThis.characterClass);
            //  else if (charactersWhoWantThis.Count == 0) return false;
            return true;
        }
    }


    [Serializable]
    [FirestoreData]
    public class EncounterResultCombatant
    {
        [field: SerializeField]
        [FirestoreProperty]
        public string uid { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string displayName { get; set; }


        [field: SerializeField]
        [FirestoreProperty]
        public string characterClass { get; set; }


        [field: SerializeField]
        [FirestoreProperty]
        public int level { get; set; }


        [field: SerializeField]
        [FirestoreProperty]
        public int expGainedEstimate { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int deckShuffleCount { get; set; }

        //[field: SerializeField]
        //[FirestoreProperty]
        //public List<SimpleTally> successSkillsRolled { get; set; }//ktery skill byl vylosovan a kolik ma  success slots

        //[field: SerializeField]
        //[FirestoreProperty]
        //public int successResult { get; set; }//soucet successu z narolovaneho equipu....pro snadnost...je to jen soucet



    }

}
