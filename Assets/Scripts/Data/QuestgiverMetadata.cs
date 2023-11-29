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
    public class Questgiver
    {
        [field: SerializeField]
        [FirestoreProperty]
        public string id { get; set; }

        //[field: SerializeField]
        //[FirestoreProperty]
        //public string displayName { get; set; }

        //[field: SerializeField]
        //[FirestoreProperty]
        //public WorldPosition position { get; set; }

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
        public List<RewardClassSpecific> rewards { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<RandomEquip> rewardsRandomEquip { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<string> prereqQuests { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<WorldPosition> prereqExploredWorldPosition { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int expRewardPerLevel { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<ItemIdWithAmount> rewardsGenerated { get; set; }



    }

    [Serializable]
    [FirestoreData]
    public class RewardClassSpecific
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
    public class RandomEquip : IContentDisplayable
    {
        //public string uid { get => ""; set => throw new NotImplementedException(); }
        //public string itemId { get => ""; set => throw new NotImplementedException(); }
        //public int sellPrice { get => 0; set => throw new NotImplementedException(); }
        //public string currencyType { get => ""; set => throw new NotImplementedException(); }
        //public int stackSize { get => 1; set => throw new NotImplementedException(); }
        //public int amount { get => 1; set => throw new NotImplementedException(); }
        //public int quality { get => 0; set => throw new NotImplementedException(); }
        //public int qualityMax { get => 0; set => throw new NotImplementedException(); }
        //public CustomData customData { get => null; set => throw new NotImplementedException(); }
        //public string contentType { get => Utils.CONTENT_TYPE.GENERATED; set => throw new NotImplementedException(); }
        //public string expireDate { get => ""; set => throw new NotImplementedException(); }




        [field: SerializeField]
        [FirestoreProperty]
        public string rarity { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string equipSlotId { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int mLevel { get; set; }



        public string uid { get; set; }
        public string itemId { get; set; }
        //RandomEquip nema vubec definovany amount v DB, ale vzdy chci aby byl 1 defakto
        private int _amount = 1;
        public int amount { get { return _amount; } set { _amount = value; } }
        public int sellPrice { get; set; }
        public string currencyType { get; set; }
        public int stackSize { get; set; }
        public int quality { get; set; }
        public int qualityMax { get; set; }
        public CustomData customData { get; set; }
        public string contentType { get { return Utils.CONTENT_TYPE.RANDOM_EQUIP; } set { } }
        public string expireDate { get; set; }

        public IContentDisplayable CopySelf()
        {
            return this.MemberwiseClone() as IContentDisplayable;
        }

        public string GetDescription()
        {
            return "You will get random " + Utils.DescriptionsMetadata.GetEquipSlots(equipSlotId).description.EN + " equipment reward";
        }

        public string GetDisplayName()
        {
            return Utils.DescriptionsMetadata.GetEquipSlots(equipSlotId).title.EN;
        }

        public string GetImageId()
        {

            return Utils.DescriptionsMetadata.GetEquipSlots(equipSlotId).imageId;

        }
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

    [Serializable]
    [FirestoreData]
    public class SimpleTallyWithMax
    {
        [field: SerializeField]
        [FirestoreProperty]
        public int count { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int countMax { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string id { get; set; }

    }




}
