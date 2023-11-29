using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Firebase.Firestore;

using Unity.VisualScripting;
using UnityEngine;


namespace simplestmmorpg.data
{



    [Serializable]
    [FirestoreData]
    public class IdWithChance
    {
        [field: SerializeField]
        [FirestoreProperty]
        public string id { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public float chanceToSpawn { get; set; }

    }


    [Serializable]
    [FirestoreData]
    public class SimpleStringDictionary
    {
        [field: SerializeField]
        [FirestoreProperty]
        public string string1 { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string string2 { get; set; }

    }

    //[Serializable]
    //[FirestoreData]
    //public class RareEnemyTierDefinition
    //{
    //    [field: SerializeField]
    //    [FirestoreProperty]
    //    public List<string> enemies { get; set; }

    //    [field: SerializeField]
    //    [FirestoreProperty]
    //    public float chanceToSpawn { get; set; }

    //}



}
