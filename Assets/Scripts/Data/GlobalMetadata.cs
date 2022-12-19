using System;
using System.Collections.Generic;
using Firebase.Firestore;
using UnityEngine;


namespace simplestmmorpg.data
{

    [Serializable]
    [FirestoreData]
    public class GlobalMetadata
    {
        [field: SerializeField]
        [FirestoreProperty]
        public string serverVersion { get; set; }

      
    }



}

