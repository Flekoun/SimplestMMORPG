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
    public class Trainer
    {
        [field: SerializeField]
        [FirestoreProperty]
        public string id { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string professionHeTrains { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int professionMinAmountNeededToTrain { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int professionMaxTrainAmount { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int trainPrice { get; set; }

    }

}
