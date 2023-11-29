using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Firebase.Firestore;
using simplestmmorpg.data;

using UnityEngine;

using static Utils;


namespace simplestmmorpg.data
{


    public interface IContentDisplayable
    {

        public string uid { get; set; }

        public string itemId { get; set; }

        public string rarity { get; set; }

        public int sellPrice { get; set; }

        public string currencyType { get; set; }

        public int stackSize { get; set; }

        public int amount { get; set; }

        public int quality { get; set; }

        public int qualityMax { get; set; }

        public CustomData customData { get; set; }

        public string contentType { get; set; }

        public string expireDate { get; set; }

        public IContentDisplayable CopySelf();

        public string GetDescription();
        //{

        //    //if (Utils.DescriptionsMetadata.GetItemsMetadata(this.itemId) != null)
        //    //    return Utils.GetMetadataForItem(this.itemId).description.GetText();
        //    if (Utils.DescriptionsMetadata.DoesDescriptionMetadataForIdExist(this.itemId))
        //        return Utils.DescriptionsMetadata.GetDescriptionMetadataForId(this.itemId).description.GetText();
        //    return string.Empty;

        //}

        public string GetDisplayName();
        //{
        //    if (Utils.DescriptionsMetadata.DoesDescriptionMetadataForIdExist(this.itemId))
        //        return Utils.DescriptionsMetadata.GetDescriptionMetadataForId(this.itemId).title.GetText();


        //    return string.Empty;//"No Metadata for " + this.GetItemId();

        //}


        public string GetImageId();
        //{


        //    if (Utils.DescriptionsMetadata.DoesDescriptionMetadataForIdExist(this.itemId))
        //        return Utils.DescriptionsMetadata.GetDescriptionMetadataForId(this.itemId).imageId;

        //    return string.Empty;//"No Metadata for " + this.GetItemId();

        //}


    }


    [Serializable]
    [FirestoreData]
    public class Content : IContentDisplayable
    {

        [field: SerializeField]
        [FirestoreProperty]
        public string uid { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string itemId { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string rarity { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int sellPrice { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string currencyType { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int stackSize { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int amount { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int quality { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int qualityMax { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public CustomData customData { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string contentType { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string expireDate { get; set; }

        //TODO : Ano muze mit jest CustomData, ale clientovi je to k nicemu imo, bere info z descriptions....stejne by to nemohl pouzit napr pro jabko od vendora

        public virtual string GetDescription()
        {

            //if (Utils.DescriptionsMetadata.GetItemsMetadata(this.itemId) != null)
            //    return Utils.GetMetadataForItem(this.itemId).description.GetText();
            if (Utils.DescriptionsMetadata.DoesDescriptionMetadataForIdExist(this.itemId))
                return Utils.DescriptionsMetadata.GetDescriptionMetadataForId(this.itemId).description.GetText();
            return string.Empty;

        }

        public virtual string GetDisplayName()
        {
            if (Utils.DescriptionsMetadata.DoesDescriptionMetadataForIdExist(this.itemId))
                return Utils.DescriptionsMetadata.GetDescriptionMetadataForId(this.itemId).title.GetText();


            return string.Empty;//"No Metadata for " + this.GetItemId();

        }


        public virtual string GetImageId()
        {


            if (Utils.DescriptionsMetadata.DoesDescriptionMetadataForIdExist(this.itemId))
                return Utils.DescriptionsMetadata.GetDescriptionMetadataForId(this.itemId).imageId;

            return string.Empty;//"No Metadata for " + this.GetItemId();


        }
        public virtual IContentDisplayable CopySelf()
        {
            return this.MemberwiseClone() as IContentDisplayable;
        }



        //public Content CopySelf()
        //{
        //    Content newContent = new Content;
        //    newContent.amount = this.amount;
        //    newContent.contentType = this.contentType;

        //    if (this.customData != null)
        //    {
        //        CustomData data = new CustomData();
        //        if (this.customData.integers != null)
        //            data.integers = new List<double>(this.customData.integers);
        //        if (this.customData.strings != null)
        //            data.strings = new List<string>(this.customData.strings);
        //        if (this.customData.simpleTally != null)
        //            data.simpleTally = new List<SimpleTally>(this.customData.simpleTally);
        //    }

        //    newContent.expireDate = this.expireDate;
        //    newContent.itemId = this.itemId;
        //    newContent.
        //}
    }






    [Serializable]
    [FirestoreData]
    public class CustomData
    {
        [field: SerializeField]
        [FirestoreProperty]
        public List<float> integers { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<string> strings { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<SimpleTally> simpleTally { get; set; }

    }

    [Serializable]
    [FirestoreData]
    public class Equip : Content
    {



        [field: SerializeField]
        [FirestoreProperty]
        public string equipSlotId { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int iLevel { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int equipSet { get; set; }

        //[field: SerializeField]
        //[FirestoreProperty]
        //public EquipAttributesData attributes { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public Skill skill { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int level { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string displayName { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string imageId { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<RareEffect> rareEffects { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<QualityUpgradeMaterials> qualityUpgradeMaterials { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public bool neverEquiped { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<SkillBonusEffect> skillBonusEffects { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<BuffBonusEffect> buffBonusEffects { get; set; }

        //[field: SerializeField]
        //[FirestoreProperty]
        //public int successSlots { get; set; }


        public override string GetDisplayName()
        {
            return displayName;
        }

        public override string GetImageId()
        {
            return imageId;
        }

        public virtual IContentDisplayable CopySelf()
        {
            return this.MemberwiseClone() as IContentDisplayable;
        }


    }



    [Serializable]
    [FirestoreData]
    public class QualityUpgradeMaterials
    {
        [field: SerializeField]
        [FirestoreProperty]
        public List<ItemIdWithAmount> materialsNeeded { get; set; }

    }

    [Serializable]
    [FirestoreData]
    public class RareEffect
    {
        [field: SerializeField]
        [FirestoreProperty]
        public string id { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int rank { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int amount { get; set; }


        public string GetDescription()
        {
            string result = "Unknown description";
            if (Utils.DescriptionsMetadata.GetRareEffectMetadata(id) != null)
            {

                var replacements = new List<(string key, string value)> { };

                int i = 0;
                //     foreach (var value in amounts)
                //  {
                replacements.Add(("0", amount.ToString()));
                i++;
                //   }

                StringBuilder templateBuild = new StringBuilder(Utils.DescriptionsMetadata.GetRareEffectMetadata(id).description.EN);
                //  result = replacements.Aggregate(templateBuild, (s, t) => s.Replace($"{{{t.key}}}%", (int.Parse(t.value) * 100f).ToString())).ToString();

                result = replacements.Aggregate(templateBuild, (s, t) => s.Replace($"{{{t.key}}}%", "<color=\"yellow\">" + (((float.Parse(t.value)) * 100f).ToString()) + "%</color>")).ToString();
                result = replacements.Aggregate(templateBuild, (s, t) => s.Replace($"{{{t.key}}}", "<color=\"yellow\">" + t.value + "</color>")).ToString();

                result = Utils.ColorizeGivenText(result, Utils.GetRarityColor("UNCOMMON"));

            }

            return result;

        }

        public string GetTitle()
        {
            if (Utils.DescriptionsMetadata.GetRareEffectMetadata(id) != null)
                return Utils.DescriptionsMetadata.GetRareEffectMetadata(id).title.EN;
            else
                return id;

        }
    }




    [Serializable]
    [FirestoreData]
    public class SkillBonusEffect
    {
        [field: SerializeField]
        [FirestoreProperty]
        public string skillGroupId { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public float amount { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int indexInArray { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string id { get; set; }


        public string GetDescription()
        {
            string result = "Unknown description";
            if (Utils.DescriptionsMetadata.GetSkillBonusEffectsMetadata(id) != null)
            {

                var replacements = new List<(string key, string value)> { };

                //                int i = 0;
                //foreach (var value in amounts)
                // {
                replacements.Add(("0", amount.ToString()));
                //   i++;
                //}

                StringBuilder templateBuild = new StringBuilder(Utils.DescriptionsMetadata.GetSkillBonusEffectsMetadata(id).description.EN);
                //  result = replacements.Aggregate(templateBuild, (s, t) => s.Replace($"{{{t.key}}}%", (int.Parse(t.value) * 100f).ToString())).ToString();


                result = replacements.Aggregate(templateBuild, (s, t) => s.Replace($"{{{t.key}}}%", "<color=\"yellow\">" + (((float.Parse(t.value)) * 100f).ToString()) + "%</color>")).ToString();
                result = replacements.Aggregate(templateBuild, (s, t) => s.Replace($"{{{t.key}}}", "<color=\"yellow\">" + t.value + "</color>")).ToString();

                result = Utils.ColorizeGivenText(result, Utils.GetRarityColor("UNCOMMON"));

            }

            return result;

        }

        public string GetTitle()
        {
            if (Utils.DescriptionsMetadata.GetSkillBonusEffectsMetadata(id) != null)
                return Utils.DescriptionsMetadata.GetSkillBonusEffectsMetadata(id).title.EN;
            else
                return skillGroupId;

        }
    }

    [Serializable]
    [FirestoreData]
    public class BuffBonusEffect
    {
        [field: SerializeField]
        [FirestoreProperty]
        public string buffGroupId { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public float amount { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int indexInArray { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string id { get; set; }


        public string GetDescription()
        {
            string result = "Unknown description";
            if (Utils.DescriptionsMetadata.GetBuffBonusEffectsMetadata(id) != null)
            {

                var replacements = new List<(string key, string value)> { };

                //                int i = 0;
                //foreach (var value in amounts)
                // {
                replacements.Add(("0", amount.ToString()));
                //   i++;
                //}

                StringBuilder templateBuild = new StringBuilder(Utils.DescriptionsMetadata.GetBuffBonusEffectsMetadata(id).description.EN);
                //  result = replacements.Aggregate(templateBuild, (s, t) => s.Replace($"{{{t.key}}}%", (int.Parse(t.value) * 100f).ToString())).ToString();


                result = replacements.Aggregate(templateBuild, (s, t) => s.Replace($"{{{t.key}}}%", "<color=\"yellow\">" + (((float.Parse(t.value)) * 100f).ToString()) + "%</color>")).ToString();
                result = replacements.Aggregate(templateBuild, (s, t) => s.Replace($"{{{t.key}}}", "<color=\"yellow\">" + t.value + "</color>")).ToString();

                result = Utils.ColorizeGivenText(result, Utils.GetRarityColor("UNCOMMON"));

            }

            return result;

        }

        public string GetTitle()
        {
            if (Utils.DescriptionsMetadata.GetBuffBonusEffectsMetadata(id) != null)
                return Utils.DescriptionsMetadata.GetBuffBonusEffectsMetadata(id).title.EN;
            else
                return buffGroupId;

        }
    }

}