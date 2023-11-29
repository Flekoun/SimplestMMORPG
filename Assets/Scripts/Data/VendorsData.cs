using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Firebase.Firestore;

using Unity.VisualScripting;
using UnityEngine;
using static Utils;


namespace simplestmmorpg.data
{

    /// <summary>
    ///Defakto tyhle tandemove veci, tedy itemy a equip se vyskytuji ruzne v db. V inventari charakteru jako seznam veci co ma. Vendor v jeho BaseContent goodsech to ma jako co prodva atd...
    ///V UI clientovi to ale lze casto zobrazit jednotne jako proste seznam veci a proto mam tenhle interface ktery to spojuje
    ///update:
    ///ale nakonec to vypada ze steje neni kde to moc pouzit, vendor musi pouzit vlastni zobrazovac proto ma navic sell price svuj a ne toho itemu napr
    /// </summary>
    public interface IHasListOfItems
    {
        public List<Content> GetContent();
        public List<Equip> GetEquip();
    }

    [Serializable]
    [FirestoreData]
    public class Vendor
    {
        [field: SerializeField]
        [FirestoreProperty]
        public string id { get; set; }


        [field: SerializeField]
        [FirestoreProperty]
        public List<VendorGood> goods { get; set; }


    }


    [Serializable]
    [FirestoreData]
    public class VendorGood
    {
        [field: SerializeField]
        [FirestoreProperty]
        public string uid { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int sellPrice { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string currencyType { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public ContentContainer content { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public ItemIdWithAmount contentGenerated { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public RandomEquip contentRandomEquip { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int stockTotal { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int stockTotalLeft { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int stockPerCharacter { get; set; }



    }


    [Serializable]
    [FirestoreData]
    public class ContentContainer
    {
        //[field: SerializeField]
        //[FirestoreProperty]
        //public string contentType { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public Equip contentEquip { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public Content content { get; set; }

        //[field: SerializeField]
        //[FirestoreProperty]
        //public ContentCurrency contentCurrency { get; set; }

        //[field: SerializeField]
        //[FirestoreProperty]
        //public ContentFood contentFood { get; set; }


        public IContentDisplayable GetContent()
        {
            if (contentEquip != null)
                return contentEquip;
            else if (content != null)
                return content;



            Debug.LogError("Content has not correct content as content or content is empty!!");
            return null;
        }

        public void SetContent(IContentDisplayable _data)
        {

            if (_data is Equip)
                contentEquip = _data as Equip;
            else
                content = _data as Content;
        }

    }

    /// <summary>
    /// Reprezentuje item ktery bude teprve vygenerovany po jeho spawnu, kde dostane sell price a stack size a pripadne dalsi atributy
    /// podle toho co to je.
    /// 1)Maji to enemy v droptablu (ktere klient nevidi a nezobrazuje)
    /// 2)Maji to vendory, ktere client ukazuje a musim to specialne zobrazovat
    /// nektroloval sem ale asi i crafting a quality upgrade definice ne?
    /// NOTE : dedim z Contentu...ten ma stejny atributy akorta jich ma vic, a vim jak jej zobrazovat v UI....takze snad ok dokud nebude UI po me chtit veci ktere namam v ItemDropDefinici?
    ///
    /// Uklada do database vsechno z IContentDisplayable kdyz bych ho chtel pouzit v AdminTools, proto mam druhy specialne pro AdminTools
    /// </summary>
    [Serializable]
    [FirestoreData]
    public class ItemIdWithAmount : IContentDisplayable
    {

        [field: SerializeField]
        [FirestoreProperty]
        public string itemId { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int amount { get; set; }

        public string uid { get; set; }

        public string rarity { get { return Utils.RARITY.COMMON; } set { } }
        public int sellPrice { get; set; }
        public string currencyType { get; set; }
        public int stackSize { get; set; }
        public int quality { get; set; }
        public int qualityMax { get; set; }
        public CustomData customData { get; set; }
        public string contentType { get { return Utils.CONTENT_TYPE.GENERATED; } set { } }
        public string expireDate { get; set; }


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

        public virtual IContentDisplayable CopySelf()
        {
            return this.MemberwiseClone() as IContentDisplayable;
        }
    }




}
