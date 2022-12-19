using System;
using System.Collections.Generic;
using Firebase.Firestore;
using UnityEngine;


namespace simplestmmorpg.data
{


    [Serializable]
    [FirestoreData]
    public class DescriptionsMetadata
    {
        [field: SerializeField]
        [FirestoreProperty]
        public BaseDescriptionMetadata[] skills { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public BaseDescriptionMetadata[] enemies { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public BaseDescriptionMetadata[] quests { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public BaseDescriptionMetadata[] locations { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public BaseDescriptionMetadata[] pointsOfInterest { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public BaseDescriptionMetadata[] items { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public BaseDescriptionMetadata[] gatherables { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public BaseDescriptionMetadata[] vendors { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public BaseDescriptionMetadata[] trainers { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public BaseDescriptionMetadata[] professions { get; set; }

        public BaseDescriptionMetadata GetPointsOfInterestMetadata(string _id)
        {
            foreach (var item in pointsOfInterest)
            {
                if (item.id == _id)
                    return item;
            }
            Debug.LogWarning("No point of interest with id : " + _id + " was found");
            return null;
        }

        public BaseDescriptionMetadata GetLocationsMetadata(string _id)
        {
            foreach (var item in locations)
            {
                if (item.id == _id)
                    return item;
            }
            Debug.LogWarning("No locations with id : " + _id + " was found");
            return null;
        }

        public BaseDescriptionMetadata GetSkillMetadata(string _skillId)
        {
            foreach (var item in skills)
            {
                if (item.id == _skillId)
                    return item;
            }

            Debug.LogWarning("No skill with id : " + _skillId + " was found");
            return null;
        }

        public BaseDescriptionMetadata GetEnemyMetadata(string _id)
        {
            foreach (var item in enemies)
            {
                if (item.id == _id)
                    return item;
            }

            Debug.LogWarning("No enemy with id : " + _id + " was found");
            return null;
        }

        public BaseDescriptionMetadata GetQuestMetadata(string _id)
        {
            foreach (var item in quests)
            {
                if (item.id == _id)
                    return item;
            }

            Debug.LogWarning("No quest with id : " + _id + " was found");
            return null;
        }

        public BaseDescriptionMetadata GetItemsMetadata(string _id)
        {
            foreach (var item in items)
            {
                if (item.id == _id)
                    return item;
            }

            Debug.LogWarning("No item with id : " + _id + " was found");
            return null;
        }

        public BaseDescriptionMetadata GetGatherablesMetadata(string _id)
        {
            foreach (var item in gatherables)
            {
                if (item.id == _id)
                    return item;
            }

            Debug.LogWarning("No gatherable with id : " + _id + " was found");
            return null;
        }

        public BaseDescriptionMetadata GetVendorsMetadata(string _id)
        {
            foreach (var item in vendors)
            {
                if (item.id == _id)
                    return item;
            }

            Debug.LogWarning("No vendor with id : " + _id + " was found");
            return null;
        }

        public BaseDescriptionMetadata GetTrainersMetadata(string _id)
        {
            foreach (var item in trainers)
            {
                if (item.id == _id)
                    return item;
            }

            Debug.LogWarning("No trainer with id : " + _id + " was found");
            return null;
        }

        public BaseDescriptionMetadata GetProfessionMetadata(string _id)
        {
            foreach (var item in professions)
            {
                if (item.id == _id)
                    return item;
            }

            Debug.LogWarning("No profession with id : " + _id + " was found");
            return null;
        }
    }


    [Serializable]
    [FirestoreData]
    public class BaseDescriptionMetadata
    {
        [field: SerializeField]
        [FirestoreProperty]
        public string id { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string imageId { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public LocalizedTexts title { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public LocalizedTexts description { get; set; }
    }





    [Serializable]
    [FirestoreData]
    public class LocalizedTexts
    {
        [field: SerializeField]
        [FirestoreProperty]
        public string EN { get; set; }


        public string GetText()
        {
            if (Utils.ActiveLanguage == "EN")
            {
                return EN;
            }

            Debug.LogError("Unsupported language code : " + Utils.ActiveLanguage + " (english version of text is : " + EN + " )");
            return "";
        }

    }



}

