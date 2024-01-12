using System;
using System.Collections.Generic;
using Firebase.Firestore;
using UnityEngine;


namespace simplestmmorpg.data
{

    [Serializable]
    [FirestoreData]
    public class PlayerData
    {
        [field: SerializeField]
        [FirestoreProperty]
        public string uid { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string country { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string playerName { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int satoshi { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<SimpleTally> medals { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int reputation { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int fiatSpent { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<SimpleTally> heroUpgrades { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<string> heirloomUnlocks { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<CharacterPreview> characters { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public Inventory inventory { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<string> portraitsUnlocked { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string creationDate { get; set; }

    }



    [Serializable]
    [FirestoreData]
    public class CharacterPreview
    {
        [field: SerializeField]
        [FirestoreProperty]
        public string characterUid { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string playerUid { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string name { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string characterClass { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int level { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string portrait { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int seasonNumber { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public bool isSeasonInProgress { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public bool isRetired { get; set; }

    }
}

