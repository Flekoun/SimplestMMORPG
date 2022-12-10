using System;
using System.Collections.Generic;
using Firebase.Firestore;
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

      
    }



}

