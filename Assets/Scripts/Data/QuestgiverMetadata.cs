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
    public class QuestgiverMeta 
    {
        [field: SerializeField]
        [FirestoreProperty]
        public string uid { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string displayName { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public WorldPosition position { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int minLevel { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int qLevel { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public bool hasExpireDate { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string expireDate { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public SimpleTally[] killsRequired { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public SimpleTally[] itemsRequired { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<QuestgiverRewardsMeta> rewards { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<QuestgiverRewardRandomEquipsMeta> rewardsRandomEquip { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<string> prereqQuests { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<string> prereqExploredPointsOfInterest { get; set; }

        


    }

    [Serializable]
    [FirestoreData]
    public class QuestgiverRewardsMeta
    {
        [field: SerializeField]
        [FirestoreProperty]
        public List<string> characterClassIds { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public ContentContainer content { get; set; }

    }

    [Serializable]
    [FirestoreData]
    public class QuestgiverRewardRandomEquipsMeta
    {
        [field: SerializeField]
        [FirestoreProperty]
        public string rarity { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string equipSlotId { get; set; }

    }


    [Serializable]
    [FirestoreData]
    public class SimpleTally
    {
        [field: SerializeField]
        [FirestoreProperty]
        public int count { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string id { get; set; }

    }



}
