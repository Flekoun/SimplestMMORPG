using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Firebase.Firestore;
using simplestmmorpg.data;
using UnityEngine;
using static Utils;

namespace simplestmmorpg.adminToolsData
{



    [Serializable]
    [FirestoreData]
    public class InternalDefinition
    {
        [field: SerializeField]
        [FirestoreProperty]
        public List<PointOfInterestInternalDefinition> START { get; set; }
        [field: SerializeField]
        [FirestoreProperty]
        public List<PointOfInterestInternalDefinition> MONSTER_SOLO { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<PointOfInterestInternalDefinition> TOWN { get; set; }
        [field: SerializeField]
        [FirestoreProperty]
        public List<PointOfInterestInternalDefinition> MERCHANT { get; set; }
        [field: SerializeField]
        [FirestoreProperty]
        public List<PointOfInterestInternalDefinition> QUEST { get; set; }
        [field: SerializeField]
        [FirestoreProperty]
        public List<PointOfInterestInternalDefinition> CHAPEL { get; set; }
        [field: SerializeField]
        [FirestoreProperty]
        public List<PointOfInterestInternalDefinition> DUNGEON { get; set; }
        [FirestoreProperty]
        public List<PointOfInterestInternalDefinition> ENDGAME { get; set; }
        [field: SerializeField]
        [FirestoreProperty]
        public InternalDefinitionOtherData OTHER_DATA { get; set; }


        public PerksOffersRareOtherData GetRarePerksbyId(string _id)
        {
            return OTHER_DATA.perksOffersRare.FirstOrDefault(item => item.id == _id);
        }

    }






    [Serializable]
    [FirestoreData]
    public class InternalDefinitionOtherData
    {
        [field: SerializeField]
        [FirestoreProperty]
        public List<PerksOffersRareOtherData> perksOffersRare { get; set; }


    }



    [Serializable]
    [FirestoreData]
    public class PerksOffersRareOtherData
    {
        [field: SerializeField]
        [FirestoreProperty]
        public string id { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<PerkOfferDefinitionAdmin> perks { get; set; }


    }

    [Serializable]
    [FirestoreData]
    public class PointOfInterestInternalDefinition
    {
        [field: SerializeField]
        [FirestoreProperty]
        public float chanceToSpawn { get; set; }


        [field: SerializeField]
        [FirestoreProperty]
        public List<IdWithChance> enemies { get; set; }



        [field: SerializeField]
        [FirestoreProperty]
        public string id { get; set; }




        [field: SerializeField]
        [FirestoreProperty]
        public List<Questgiver> questgivers { get; set; }

        //[field: SerializeField]
        //[FirestoreProperty]
        //public List<RareEnemyTierDefinition> rareEnemies { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<string> specials { get; set; }



        [field: SerializeField]
        [FirestoreProperty]
        public DungeonDefinition dungeon { get; set; }

        //nepouzivam
        //[field: SerializeField]
        //[FirestoreProperty]
        //public List<Trainer> trainers { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<Vendor> vendors { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int floorMin { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int floorMax { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<IdWithChance> chapelBless { get; set; }


        [field: SerializeField]
        [FirestoreProperty]
        public MonstersDefinition monsters { get; set; }
    }




    [Serializable]
    [FirestoreData]
    public class DungeonDefinition
    {


        [field: SerializeField]
        [FirestoreProperty]
        public int partySize { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int entryPrice { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<TierDungeonDefinition> tiers { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<ContentContainer> rewards { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<RandomEquip> rewardsRandomEquip { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<ItemIdWithAmount> rewardsGenerated { get; set; }


        [field: SerializeField]
        [FirestoreProperty]
        public bool isEndlessDungeon { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public bool isFinalDungeon { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int characterLevelMax { get; set; }


        [field: SerializeField]
        [FirestoreProperty]
        public int characterLevelMin { get; set; }
    }


    [Serializable]
    [FirestoreData]
    public class MonstersDefinition
    {

        [field: SerializeField]
        [FirestoreProperty]
        public int partySize { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int exploreTimePrice { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<TierMonstersDefinition> tiers { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string perkOffersRareId { get; set; }


    }


    [Serializable]
    [FirestoreData]
    public class TierDungeonDefinition
    {
        [field: SerializeField]
        [FirestoreProperty]
        public List<string> enemies { get; set; }

    }



    [Serializable]
    [FirestoreData]
    public class PointOfInterestServerDataDefinitions
    {
        [field: SerializeField]
        [FirestoreProperty]
        public List<TierMonstersDefinition> tiers { get; set; }

        //[field: SerializeField]
        //[FirestoreProperty]
        //public List<PerkOfferDefinitionAdmin> perkOffersRare { get; set; }
        [field: SerializeField]
        [FirestoreProperty]
        public List<PerkOfferDefinitionAdmin> perkOffersRareId { get; set; }


        //[field: SerializeField]
        //[FirestoreProperty]
        //public List<RareEnemyTierDefinition> rareEnemies { get; set; }


        [field: SerializeField]
        [FirestoreProperty]
        public List<IdWithChance> blesses { get; set; }
    }

    [Serializable]
    [FirestoreData]
    public class TierMonstersDefinition
    {
        [field: SerializeField]
        [FirestoreProperty]
        public List<string> enemies { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int entryTimePrice { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<PerkOfferDefinitionAdmin> perkOffers { get; set; }


        //[field: SerializeField]
        //[FirestoreProperty]
        //public List<IdWithChance> blesses { get; set; }
    }


    //pouzivam tuhle duplitni definici kvuli tomu ze chci pouzit jiny ItemIdWithAmount....protoze ten normalni pak uklada do database spoustu null hodnot co nechcni
    [Serializable]
    [FirestoreData]
    public class PerkOfferDefinitionAdmin
    {

        [field: SerializeField]
        [FirestoreProperty]
        public float chanceToSpawn { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string uid { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int timePrice { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int stockLeft { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int stockClaimed { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string rarity { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int curseCount { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<SimpleTally> restrictionClass { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<SimpleTally> restictionProfession { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<ContentContainer> rewards { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<RandomEquip> rewardsRandomEquip { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<ItemIdWithAmountAdmin> rewardsGenerated { get; set; }


        [field: SerializeField]
        [FirestoreProperty]
        public bool isInstantReward { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int recurrenceInGameDays { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int rewardAtSpecificGameDay { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int rewardAfterSpecificGameDay { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int charges { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<SimpleTally> specialEffectId { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string rarePerkGroupId { get; set; }


        //[field: SerializeField]
        //[FirestoreProperty]
        //public int successPrice { get; set; }


        public string GetDescription()
        {
            string result = "";
            foreach (var effect in specialEffectId)
            {


                string effectResult = "Unknown description";
                if (Utils.DescriptionsMetadata.GetPerkSpecialEffectsMetadata(effect.id) != null)
                {

                    var replacements = new List<(string key, string value)> { };

                    //int i = 0;
                    //foreach (var value in amounts)
                    //{
                    //    replacements.Add((i.ToString(), value.ToString()));
                    //    i++;
                    //}
                    replacements.Add(("0", effect.count.ToString()));

                    StringBuilder templateBuild = new StringBuilder(Utils.DescriptionsMetadata.GetPerkSpecialEffectsMetadata(effect.id).description.EN);
                    //  result = replacements.Aggregate(templateBuild, (s, t) => s.Replace($"{{{t.key}}}%", (int.Parse(t.value) * 100f).ToString())).ToString();

                    effectResult = replacements.Aggregate(templateBuild, (s, t) => s.Replace($"{{{t.key}}}%", "<color=\"yellow\">" + (((float.Parse(t.value)) * 100f).ToString()) + "%</color>")).ToString();
                    effectResult = replacements.Aggregate(templateBuild, (s, t) => s.Replace($"{{{t.key}}}", "<color=\"yellow\">" + t.value + "</color>")).ToString();

                }

                result += Utils.ReplacePlaceholdersInTextWithDescriptionFromMetadata(effectResult) + "\n";

            }
            return result;

        }

        public static PerkOfferDefinitionAdmin FactoryNewPerk()
        {
            PerkOfferDefinitionAdmin newPerk = new PerkOfferDefinitionAdmin();
            newPerk.charges = -1;
            newPerk.curseCount = 0;
            newPerk.isInstantReward = true;
            newPerk.rarity = RARITY.COMMON;
            newPerk.recurrenceInGameDays = -1;
            newPerk.restictionProfession = new List<simplestmmorpg.data.SimpleTally>();
            newPerk.restrictionClass = new List<simplestmmorpg.data.SimpleTally>();
            newPerk.rewardAfterSpecificGameDay = -1;
            newPerk.rewardAtSpecificGameDay = -1;
            newPerk.rewards = new List<simplestmmorpg.data.ContentContainer>();
            newPerk.rewardsGenerated = new List<ItemIdWithAmountAdmin>();
            newPerk.rewardsRandomEquip = new List<simplestmmorpg.data.RandomEquip>();
            newPerk.specialEffectId = new List<simplestmmorpg.data.SimpleTally>();
            newPerk.stockLeft = -1;
            newPerk.stockClaimed = 0;
            newPerk.timePrice = 1;
            newPerk.uid = System.Guid.NewGuid().ToString();
            newPerk.chanceToSpawn = 1;

            return newPerk;
        }

        public static PerkOfferDefinitionAdmin Duplicate(PerkOfferDefinitionAdmin _template)
        {
            PerkOfferDefinitionAdmin newPerk = new PerkOfferDefinitionAdmin();
            newPerk.charges = _template.charges;
            newPerk.curseCount = _template.curseCount;
            newPerk.isInstantReward = _template.isInstantReward;
            newPerk.rarity = _template.rarity;
            newPerk.recurrenceInGameDays = _template.recurrenceInGameDays;
            newPerk.restictionProfession = new List<simplestmmorpg.data.SimpleTally>();
            newPerk.restrictionClass = new List<simplestmmorpg.data.SimpleTally>();
            newPerk.rewardAfterSpecificGameDay = _template.rewardAfterSpecificGameDay;
            newPerk.rewardAtSpecificGameDay = _template.rewardAtSpecificGameDay;
            newPerk.rewards = new List<simplestmmorpg.data.ContentContainer>();
            newPerk.rarePerkGroupId = _template.rarePerkGroupId;
            newPerk.rewardsGenerated = new List<ItemIdWithAmountAdmin>();
            foreach (var item in _template.rewardsGenerated)
            {
                var itemNew = new ItemIdWithAmountAdmin();
                itemNew.amount = item.amount;
                itemNew.itemId = item.itemId;
                newPerk.rewardsGenerated.Add(itemNew);
            }
            newPerk.rewardsRandomEquip = new List<simplestmmorpg.data.RandomEquip>();
            foreach (var item in _template.rewardsRandomEquip)
            {
                var itemNew = new RandomEquip();
                itemNew.rarity = item.rarity;
                itemNew.equipSlotId = item.equipSlotId;
                itemNew.mLevel = item.mLevel;
                newPerk.rewardsRandomEquip.Add(itemNew);
            }
            newPerk.specialEffectId = new List<simplestmmorpg.data.SimpleTally>();
            foreach (var item in _template.specialEffectId)
            {
                var itemNew = new SimpleTally();
                itemNew.count = item.count;
                itemNew.id = item.id;
                newPerk.specialEffectId.Add(itemNew);
            }
            newPerk.stockLeft = _template.stockLeft;
            newPerk.stockClaimed = _template.stockClaimed;
            newPerk.timePrice = _template.timePrice;
            newPerk.uid = System.Guid.NewGuid().ToString();
            newPerk.chanceToSpawn = _template.chanceToSpawn;

            return newPerk;
        }

    }

    [Serializable]
    [FirestoreData]
    public class ItemIdWithAmountAdmin
    {

        [field: SerializeField]
        [FirestoreProperty]
        public string itemId { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int amount { get; set; }



        public string GetDescription()
        {

            //if (Utils.DescriptionsMetadata.GetItemsMetadata(this.itemId) != null)
            //    return Utils.GetMetadataForItem(this.itemId).description.GetText();
            if (Utils.DescriptionsMetadata.DoesDescriptionMetadataForIdExist(this.itemId))
                return Utils.DescriptionsMetadata.GetDescriptionMetadataForId(this.itemId).description.GetText();
            return string.Empty;

        }

        public string GetDisplayName()
        {
            if (Utils.DescriptionsMetadata.DoesDescriptionMetadataForIdExist(this.itemId))
                return Utils.DescriptionsMetadata.GetDescriptionMetadataForId(this.itemId).title.GetText();


            return string.Empty;//"No Metadata for " + this.GetItemId();

        }


        public string GetImageId()
        {


            if (Utils.DescriptionsMetadata.DoesDescriptionMetadataForIdExist(this.itemId))
                return Utils.DescriptionsMetadata.GetDescriptionMetadataForId(this.itemId).imageId;

            return string.Empty;//"No Metadata for " + this.GetItemId();

        }
    }


    //[Serializable]
    //[FirestoreData]
    //public class PerkOffer
    //{
    //    [field: SerializeField]
    //    [FirestoreProperty]
    //    public int charges { get; set; }

    //    [field: SerializeField]
    //    [FirestoreProperty]
    //    public int curseCount { get; set; }

    //    [field: SerializeField]
    //    [FirestoreProperty]
    //    public int fatiguePrice { get; set; }

    //    [field: SerializeField]
    //    [FirestoreProperty]
    //    public bool isInstantReward { get; set; }

    //    [field: SerializeField]
    //    [FirestoreProperty]
    //    public string rarity { get; set; }

    //    [field: SerializeField]
    //    [FirestoreProperty]
    //    public int recurrenceInGameDays { get; set; }

    //    [field: SerializeField]
    //    [FirestoreProperty]
    //    public List<object> restrictionClass { get; set; }

    //    [field: SerializeField]
    //    [FirestoreProperty]
    //    public List<object> restrictionProfession { get; set; }

    //    [field: SerializeField]
    //    [FirestoreProperty]
    //    public int rewardAfterSpecificGameDay { get; set; }

    //    [field: SerializeField]
    //    [FirestoreProperty]
    //    public int rewardAtSpecificGameDay { get; set; }

    //    [field: SerializeField]
    //    [FirestoreProperty]
    //    public List<object> rewards { get; set; }

    //    [field: SerializeField]
    //    [FirestoreProperty]
    //    public List<RewardsGenerated> rewardsGenerated { get; set; }

    //    [field: SerializeField]
    //    [FirestoreProperty]
    //    public List<RewardsRandomEquip> rewardsRandomEquip { get; set; }

    //    [field: SerializeField]
    //    [FirestoreProperty]
    //    public List<SpecialEffectID> specialEffectId { get; set; }

    //    [field: SerializeField]
    //    [FirestoreProperty]
    //    public int stockLeft { get; set; }

    //    [field: SerializeField]
    //    [FirestoreProperty]
    //    public int timePrice { get; set; }
    //}

    //[Serializable]
    //[FirestoreData]
    //public class RewardsGenerated
    //{
    //    [field: SerializeField]
    //    [FirestoreProperty]
    //    public int amount { get; set; }

    //    [field: SerializeField]
    //    [FirestoreProperty]
    //    public string itemId { get; set; }
    //}

    //[Serializable]
    //[FirestoreData]
    //public class RewardsRandomEquip
    //{
    //    [field: SerializeField]
    //    [FirestoreProperty]
    //    public string equipSlotId { get; set; }

    //    [field: SerializeField]
    //    [FirestoreProperty]
    //    public int mLevel { get; set; }

    //    [field: SerializeField]
    //    [FirestoreProperty]
    //    public string rarity { get; set; }
    //}

    //[Serializable]
    //[FirestoreData]
    //public class SpecialEffectID
    //{
    //    [field: SerializeField]
    //    [FirestoreProperty]
    //    public int count { get; set; }

    //    [field: SerializeField]
    //    [FirestoreProperty]
    //    public string id { get; set; }
    //}

}
