using System;
using System.Collections.Generic;
using Firebase.Firestore;
using UnityEngine;


namespace simplestmmorpg.playerData
{
    [Serializable]
    [FirestoreData]
    public class CombatLog
    {
        [field: SerializeField]
        [FirestoreProperty]
        public string[] entries { get; set; }
    }


}

