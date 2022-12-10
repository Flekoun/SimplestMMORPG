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
        public int gems { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<CharacterPreview> characters { get; set; }

    }



    [Serializable]
    [FirestoreData]
    public class CharacterPreview
    {
        [field: SerializeField]
        [FirestoreProperty]
        public string uid { get; set; }

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

    }
}

