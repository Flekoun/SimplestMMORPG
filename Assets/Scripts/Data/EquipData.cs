using System;
using System.Collections.Generic;
using Firebase.Firestore;
using simplestmmorpg.data;

using UnityEngine;


namespace simplestmmorpg.data
{


    //TODO:  tady se zamyslet naco tu mam ten interfarce, pokud nema equio amount z databaze tak jestli by spis nemeli mti neco jako "isStackable" ty itemy atd? jestli by to nemelo byt neco spis jako stackable neboco, torchu to promyslet co tu delam a co je cilem?
    //public interface IInventoryItem
    //{
    //    public int GetAmount();
    //    public ItemSimple GetItem();
    //    public string GetImageId();

    //}


    [Serializable]
    [FirestoreData]
    public abstract class Content //: IInventoryItem//:ItemBase
    {
        [field: SerializeField]
        [FirestoreProperty]
        public string uid { get; set; }

        //[field: SerializeField]
        //[FirestoreProperty]
        //public string displayName { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int sellPrice { get; set; }

        //[field: SerializeField]
        //[FirestoreProperty]
        //public string imageId { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string itemId { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int stackSize { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int amount { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string rarity { get; set; }


        public abstract string GetContentType();

        public abstract string GetDisplayName();

        public abstract string GetImageId();


    }


    [Serializable]
    [FirestoreData]
    public class ContentItem : Content
    {

        [field: SerializeField]
        [FirestoreProperty]
        public int level { get; set; }

        public override string GetContentType()
        {
            return Utils.CONTENT_TYPE.ITEM;
        }

        public override string GetDisplayName()
        {
            if (Utils.GetMetadataForItem(this.itemId) == null)
                return "No Metadata for " + this.itemId;

            return Utils.GetMetadataForItem(this.itemId).title.GetText();
        }

        public override string GetImageId()
        {
            if (Utils.GetMetadataForItem(this.itemId) == null)
                return "No Metadata for " + this.itemId;

            return Utils.GetMetadataForItem(this.itemId).imageId;
        }
    }


    [Serializable]
    [FirestoreData]
    public class ContentFood : Content
    {

        [field: SerializeField]
        [FirestoreProperty]
        public int fatigueRecoveryBonus { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int timeBonus { get; set; }

        public override string GetContentType()
        {
            return Utils.CONTENT_TYPE.FOOD;
        }

        public override string GetDisplayName()
        {
            if (Utils.GetMetadataForItem(this.itemId) == null)
                return "No Metadata for " + this.itemId;

            return Utils.GetMetadataForItem(this.itemId).title.GetText();
        }

        public override string GetImageId()
        {
            if (Utils.GetMetadataForItem(this.itemId) == null)
                return "No Metadata for " + this.itemId;

            return Utils.GetMetadataForItem(this.itemId).imageId;
        }
    }


    [Serializable]
    [FirestoreData]
    public class ContentCurrency : Content
    {

        [field: SerializeField]
        [FirestoreProperty]
        public string currencyType { get; set; }

        public override string GetContentType()
        {
            return Utils.CONTENT_TYPE.CURRENCY;
        }

        public override string GetDisplayName()
        {
            if (Utils.GetMetadataForItem(this.itemId) == null)
                return "No Metadata for " + this.itemId;


            return Utils.GetMetadataForItem(this.itemId).title.GetText();
        }

        public override string GetImageId()
        {
            if (Utils.GetMetadataForItem(this.itemId) == null)
                return "No Metadata for " + this.itemId;

            return Utils.GetMetadataForItem(this.itemId).imageId;
        }
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

        [field: SerializeField]
        [FirestoreProperty]
        public EquipAttributesData attributes { get; set; }

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


        public override string GetContentType()
        {
            return Utils.CONTENT_TYPE.EQUIP;
        }

        public override string GetDisplayName()
        {
            return displayName;
        }

        public override string GetImageId()
        {
            return imageId;
        }
    }


    [Serializable]
    [FirestoreData]
    public class EquipAttributesData
    {

        [field: SerializeField]
        [FirestoreProperty]
        public int intellect { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int stamina { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int strength { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int agility { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int spirit { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int armor { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int durability { get; set; }


        public int GetAttributeAmount(Utils.EQUIP_ATTRIBUTES _attribute)
        {
            switch (_attribute)
            {
                case Utils.EQUIP_ATTRIBUTES.AGILITY: return agility;
                case Utils.EQUIP_ATTRIBUTES.SPIRIT: return spirit;
                case Utils.EQUIP_ATTRIBUTES.STRENGTH: return strength;
                case Utils.EQUIP_ATTRIBUTES.INTELLECT: return intellect;
                case Utils.EQUIP_ATTRIBUTES.STAMINA: return stamina;
                default:
                    {
                        Debug.LogError("Cannot find attribute : " + _attribute.ToString());
                        return 0;
                    }
            }
        }
    }

}