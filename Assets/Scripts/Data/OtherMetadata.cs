using System;
using System.Collections.Generic;
using Firebase.Firestore;
using Unity.VisualScripting;
using UnityEngine;


namespace simplestmmorpg.data
{

    [Serializable]
    [FirestoreData]
    public class OtherMetadata
    {
        [field: SerializeField]
        [FirestoreProperty]
        public string whatIsNew { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int leaderboardsPageSize { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int auctionHousePageSize { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<PossiblePortraits> possiblePortraits { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public Constants constants { get; set; }

        public List<string> GetPossiblePortraits(string _classId)
        {
            List<string> portraits = new List<string>();

            foreach (var item in possiblePortraits)
            {
                if (item.classId == _classId || item.classId == Utils.CHARACTER_CLASS.ANY)
                    portraits.AddRange(item.portraits);
                //return item;
            }

            // Debug.LogError("Cant find possible portraits for Class Id : " + _classId);
            return portraits;
        }
    }

    [Serializable]
    [FirestoreData]
    public class PossiblePortraits
    {
        [field: SerializeField]
        [FirestoreProperty]
        public string classId { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<string> portraits { get; set; }



    }

    [Serializable]
    [FirestoreData]
    public class Constants
    {
        [field: SerializeField]
        [FirestoreProperty]
        public float deckShuffleMaxHpPenalty { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int timePerTravelPoint { get; set; }


        [field: SerializeField]
        [FirestoreProperty]
        public int restSupplyLimitIncrement { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int deepRestTimeCost { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int SCAVENGE_CLAIM_COST { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int SCAVENGE_CLAIM_ALL_COST { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int SCAVENGE_CLAIM_COST_TIME { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int SCAVENGE_CLAIM_ALL_COST_TIME { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int FLEE_FATIGUE_PENALTY { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int DECK_SHUFFLE_FATIGUE_PENALTY { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int HIGH_LEVEL_POI_FATIGUE_PENALTY { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int TIME_COST_TO_EXPLORE_POI { get; set; }


        
    }




}

