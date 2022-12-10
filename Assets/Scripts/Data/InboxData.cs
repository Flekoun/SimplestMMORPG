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
    public class InboxItem
    {
        [field: SerializeField]
        [FirestoreProperty]
        public string uid { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public ContentContainer content { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string messageBody { get; set; }


        [field: SerializeField]
        [FirestoreProperty]
        public string messageTitle { get; set; }


        [field: SerializeField]
        [FirestoreProperty]
        public string expireDate { get; set; }

    }


}
