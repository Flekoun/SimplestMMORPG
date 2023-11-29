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
        public BaseDescriptionMetadataArray[] skills { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public BaseDescriptionMetadata[] enemies { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public BaseDescriptionMetadata[] quests { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public LocationDescriptionMetadata[] locations { get; set; }

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

        [field: SerializeField]
        [FirestoreProperty]
        public BaseDescriptionMetadata[] portraits { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public BaseDescriptionMetadata[] rareEffects { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public BaseDescriptionMetadata[] cratingRecipes { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public BaseDescriptionMetadata[] leaderboardScoreTypes { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public BaseDescriptionMetadata[] monsterSkillTypes { get; set; }



        [field: SerializeField]
        [FirestoreProperty]
        public BaseDescriptionMetadata[] perkSpecialEffects { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public BaseDescriptionMetadata[] skillBonusEffects { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public BaseDescriptionMetadata[] UI { get; set; }


        [field: SerializeField]
        [FirestoreProperty]
        public BaseDescriptionMetadata[] buffBonusEffects { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public BaseDescriptionMetadata[] pointsOfInterestRoomTypes { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public BaseDescriptionMetadata[] blesses { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public BaseDescriptionMetadata[] foodEffects { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public BaseDescriptionMetadata[] equipSlots { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public BaseDescriptionMetadata[] rarities { get; set; }




        public bool DoesDescriptionMetadataForIdExist(string _id)
        {
            foreach (var item in skills) if (item.id.Contains(_id)) return true;
            foreach (var item in locations) if (item.descriptionData.id == _id) return true;
            foreach (var item in quests) if (item.id == _id) return true;
            foreach (var item in pointsOfInterest) if (item.id == _id) return true;
            foreach (var item in items) if (item.id == _id) return true;
            foreach (var item in gatherables) if (item.id == _id) return true;
            foreach (var item in vendors) if (item.id == _id) return true;
            foreach (var item in trainers) if (item.id == _id) return true;
            foreach (var item in professions) if (item.id == _id) return true;
            foreach (var item in portraits) if (item.id == _id) return true;
            foreach (var item in rareEffects) if (item.id == _id) return true;
            foreach (var item in cratingRecipes) if (item.id == _id) return true;
            foreach (var item in leaderboardScoreTypes) if (item.id == _id) return true;
            foreach (var item in monsterSkillTypes) if (item.id == _id) return true;

            foreach (var item in perkSpecialEffects) if (item.id == _id) return true;
            foreach (var item in skillBonusEffects) if (item.id == _id) return true;
            foreach (var item in enemies) if (item.id == _id) return true;
            foreach (var item in UI) if (item.id == _id) return true;
            foreach (var item in buffBonusEffects) if (item.id == _id) return true;
            foreach (var item in pointsOfInterestRoomTypes) if (item.id == _id) return true;
            foreach (var item in blesses) if (item.id == _id) return true;
            foreach (var item in foodEffects) if (item.id == _id) return true;
            foreach (var item in equipSlots) if (item.id == _id) return true;
            foreach (var item in rarities) if (item.id == _id) return true;

            return false;
        }

        public BaseDescriptionMetadata_NoId GetDescriptionMetadataForId(string _id)
        {
            foreach (var item in skills) if (item.id.Contains(_id)) return item;
            foreach (var item in locations) if (item.descriptionData.id == _id) return item.descriptionData;
            foreach (var item in quests) if (item.id == _id) return item;
            foreach (var item in pointsOfInterest) if (item.id == _id) return item;
            foreach (var item in items) if (item.id == _id) return item;
            foreach (var item in gatherables) if (item.id == _id) return item;
            foreach (var item in vendors) if (item.id == _id) return item;
            foreach (var item in trainers) if (item.id == _id) return item;
            foreach (var item in professions) if (item.id == _id) return item;
            foreach (var item in portraits) if (item.id == _id) return item;
            foreach (var item in rareEffects) if (item.id == _id) return item;
            foreach (var item in cratingRecipes) if (item.id == _id) return item;
            foreach (var item in leaderboardScoreTypes) if (item.id == _id) return item;
            foreach (var item in monsterSkillTypes) if (item.id == _id) return item;

            foreach (var item in perkSpecialEffects) if (item.id == _id) return item;
            foreach (var item in skillBonusEffects) if (item.id == _id) return item;
            foreach (var item in enemies) if (item.id == _id) return item;
            foreach (var item in UI) if (item.id == _id) return item;
            foreach (var item in buffBonusEffects) if (item.id == _id) return item;
            foreach (var item in pointsOfInterestRoomTypes) if (item.id == _id) return item;
            foreach (var item in blesses) if (item.id == _id) return item;
            foreach (var item in foodEffects) if (item.id == _id) return item;
            foreach (var item in equipSlots) if (item.id == _id) return item;
            foreach (var item in rarities) if (item.id == _id) return item;

            Debug.LogWarning("No destription metadata with id : " + _id + " was found. Add it to collection <b>_metadatada_CoreDefinitions/Descriptions</b>");
            return null;
        }

        public BaseDescriptionMetadata_NoId GetUIMetadata(string _id)
        {
            foreach (var item in UI)
            {
                if (item.id == _id)
                    return item;
            }
            //  Debug.LogWarning("No point of interest with id : " + _id + " was found");
            return null;
        }

        public BaseDescriptionMetadata_NoId GetSkillBonusEffectsMetadata(string _id)
        {
            foreach (var item in skillBonusEffects)
            {
                if (item.id == _id)
                    return item;
            }
            //  Debug.LogWarning("No point of interest with id : " + _id + " was found");
            return null;
        }

        public BaseDescriptionMetadata_NoId GetBuffBonusEffectsMetadata(string _id)
        {
            foreach (var item in buffBonusEffects)
            {
                if (item.id == _id)
                    return item;
            }
            //  Debug.LogWarning("No point of interest with id : " + _id + " was found");
            return null;
        }

        public BaseDescriptionMetadata_NoId GetLeaderboardScoreTypeMetadata(string _id)
        {
            foreach (var item in leaderboardScoreTypes)
            {
                if (item.id == _id)
                    return item;
            }
            //  Debug.LogWarning("No point of interest with id : " + _id + " was found");
            return null;
        }

        public BaseDescriptionMetadata_NoId GetPointsOfInterestMetadata(string _id)
        {
            foreach (var item in pointsOfInterest)
            {
                if (item.id == _id)
                    return item;
            }
            //  Debug.LogWarning("No point of interest with id : " + _id + " was found");
            return null;
        }

        public LocationDescriptionMetadata GetLocationsMetadata(string _id)
        {
            foreach (var item in locations)
            {
                if (item.descriptionData.id == _id)
                    return item;
            }
            //  Debug.LogWarning("No locations with id : " + _id + " was found");
            return null;
        }

        public BaseDescriptionMetadata_NoId GetSkillMetadata(string _skillId)
        {
            foreach (var item in skills)
            {
                if (item.id.Contains(_skillId))

                    return item;
            }

            //   Debug.LogWarning("No skill with id : " + _skillId + " was found");
            return null;
        }

        public BaseDescriptionMetadata_NoId GetEnemyMetadata(string _id)
        {
            foreach (var item in enemies)
            {
                if (item.id == _id)
                    return item;
            }

            //    Debug.LogWarning("No enemy with id : " + _id + " was found");
            return null;
        }

        public BaseDescriptionMetadata_NoId GetQuestMetadata(string _id)
        {
            foreach (var item in quests)
            {
                if (item.id == _id)
                    return item;
            }

            //   Debug.LogWarning("No quest with id : " + _id + " was found");
            return null;
        }

        public BaseDescriptionMetadata_NoId GetItemsMetadata(string _id)
        {
            foreach (var item in items)
            {
                if (item.id == _id)
                    return item;
            }

            //   Debug.LogWarning("No item with id : " + _id + " was found");
            return null;
        }

        public BaseDescriptionMetadata_NoId GetGatherablesMetadata(string _id)
        {
            foreach (var item in gatherables)
            {
                if (item.id == _id)
                    return item;
            }

            //   Debug.LogWarning("No gatherable with id : " + _id + " was found");
            return null;
        }

        public BaseDescriptionMetadata_NoId GetVendorsMetadata(string _id)
        {
            foreach (var item in vendors)
            {
                if (item.id == _id)
                    return item;
            }

            //   Debug.LogWarning("No vendor with id : " + _id + " was found");
            return null;
        }

        public BaseDescriptionMetadata_NoId GetTrainersMetadata(string _id)
        {
            foreach (var item in trainers)
            {
                if (item.id == _id)
                    return item;
            }

            //  Debug.LogWarning("No trainer with id : " + _id + " was found");
            return null;
        }

        public BaseDescriptionMetadata_NoId GetProfessionMetadata(string _id)
        {
            foreach (var item in professions)
            {
                if (item.id == _id)
                    return item;
            }

            //   Debug.LogWarning("No profession with id : " + _id + " was found");
            return null;
        }

        public BaseDescriptionMetadata_NoId GetPortraitsMetadata(string _id)
        {
            foreach (var item in portraits)
            {
                if (item.id == _id)
                    return item;
            }

            //    Debug.LogWarning("No portrait with id : " + _id + " was found");
            return null;
        }

        public BaseDescriptionMetadata_NoId GetRareEffectMetadata(string _id)
        {
            foreach (var item in rareEffects)
            {
                if (item.id == _id)
                    return item;
            }

            //     Debug.LogWarning("No rareEffect with id : " + _id + " was found");
            return null;
        }


        public BaseDescriptionMetadata_NoId GetCratingRecipesMetadata(string _id)
        {
            foreach (var item in cratingRecipes)
            {
                if (item.id == _id)
                    return item;
            }

            //     Debug.LogWarning("No Crating Receipt with id : " + _id + " was found");
            return null;
        }

        public BaseDescriptionMetadata_NoId GetMonsterSkillTypesMetadata(string _id)
        {
            foreach (var item in monsterSkillTypes)
            {
                if (item.id == _id)
                    return item;
            }

            //     Debug.LogWarning("No Crating Receipt with id : " + _id + " was found");
            return null;
        }



        public BaseDescriptionMetadata_NoId GetPerkSpecialEffectsMetadata(string _id)
        {
            foreach (var item in perkSpecialEffects)
            {
                if (item.id == _id)
                    return item;
            }

            //     Debug.LogWarning("No Crating Receipt with id : " + _id + " was found");
            return null;
        }

        public BaseDescriptionMetadata_NoId GetPointsOfInterestRoomTypesMetadata(string _id)
        {
            foreach (var item in pointsOfInterestRoomTypes)
            {
                if (item.id == _id)
                    return item;
            }

            //     Debug.LogWarning("No Crating Receipt with id : " + _id + " was found");
            return null;
        }


        public BaseDescriptionMetadata_NoId GetBlessesMetadata(string _id)
        {
            foreach (var item in blesses)
            {
                if (item.id == _id)
                    return item;
            }

            return null;
        }

        public BaseDescriptionMetadata_NoId GetFoodEffectsMetadata(string _id)
        {
            foreach (var item in foodEffects)
            {
                if (item.id == _id)
                    return item;
            }

            return null;
        }

        public BaseDescriptionMetadata_NoId GetEquipSlots(string _id)
        {
            foreach (var item in equipSlots)
            {
                if (item.id == _id)
                    return item;
            }

            return null;
        }

        public BaseDescriptionMetadata_NoId GetRarities(string _id)
        {
            foreach (var item in rarities)
            {
                if (item.id == _id)
                    return item;
            }

            return null;
        }


    }


    [Serializable]
    [FirestoreData]
    public class BaseDescriptionMetadata : BaseDescriptionMetadata_NoId
    {
        [field: SerializeField]
        [FirestoreProperty]
        public string id { get; set; }

        //[field: SerializeField]
        //[FirestoreProperty]
        //public string imageId { get; set; }

        //[field: SerializeField]
        //[FirestoreProperty]
        //public LocalizedTexts title { get; set; }

        //[field: SerializeField]
        //[FirestoreProperty]
        //public LocalizedTexts description { get; set; }
    }


    public class BaseDescriptionMetadata_NoId
    {


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
    public class BaseDescriptionMetadataArray : BaseDescriptionMetadata_NoId
    {
        [field: SerializeField]
        [FirestoreProperty]
        public List<string> id { get; set; }

        //[field: SerializeField]
        //[FirestoreProperty]
        //public string imageId { get; set; }

        //[field: SerializeField]
        //[FirestoreProperty]
        //public LocalizedTexts title { get; set; }

        //[field: SerializeField]
        //[FirestoreProperty]
        //public LocalizedTexts description { get; set; }
    }



    [Serializable]
    [FirestoreData]
    public class LocationDescriptionMetadata
    {
        [field: SerializeField]
        [FirestoreProperty]
        public BaseDescriptionMetadata descriptionData { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string locationType { get; set; }

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

