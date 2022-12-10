using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Firebase.Firestore;

using Unity.VisualScripting;
using UnityEngine;


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
        public List<ContentItem> GetInventoryItemsSimple();
        public List<Equip> GetItemsEquip();
    }

    [Serializable]
    [FirestoreData]
    public class Vendor
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
        public ContentContainer content { get; set; }


    }


    [Serializable]
    [FirestoreData]
    public class ContentContainer
    {
        [field: SerializeField]
        [FirestoreProperty]
        public string contentType { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public Equip contentEquip { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public ContentItem contentItem { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public ContentCurrency contentCurrency { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public ContentFood contentFood { get; set; }


        public Content GetContent()
        {
            if (contentType == Utils.CONTENT_TYPE.EQUIP)
                return contentEquip;
            else if (contentType == Utils.CONTENT_TYPE.ITEM)
                return contentItem;
            else if (contentType == Utils.CONTENT_TYPE.CURRENCY)
                return contentCurrency;
            else if (contentType == Utils.CONTENT_TYPE.FOOD)
                return contentFood;


            Debug.LogError("Content has not correct content as content or content is empty!!");
            return null;
        }

    }
}
