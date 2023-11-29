using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using Firebase.Firestore;

using Unity.VisualScripting;
using UnityEngine;


namespace simplestmmorpg.data
{


    [Serializable]
    [FirestoreData]
    public class AuctionOffer
    {
        [field: SerializeField]
        [FirestoreProperty]
        public string uid { get; set; }


        [field: SerializeField]
        [FirestoreProperty]
        public string sellerDisplayName { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string sellerUid { get; set; }


        [field: SerializeField]
        [FirestoreProperty]
        public ContentContainer content { get; set; }



        //[field: SerializeField]
        //[FirestoreProperty]
        //public string contentType { get; set; }

        //[field: SerializeField]
        //[FirestoreProperty]
        //public InventoryItemSimple contentItem { get; set; }

        //[field: SerializeField]
        //[FirestoreProperty]
        //public Equip contentEquip { get; set; }

        //[field: SerializeField]
        //[FirestoreProperty]
        //public int contentCurrencySilver { get; set; }


        [field: SerializeField]
        [FirestoreProperty]
        public bool hasBuyoutPrice { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int lastBidPrice { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int nextBidPrice { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int buyoutPrice { get; set; }


        [field: SerializeField]
        [FirestoreProperty]
        public string highestBidderUid { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string highestBidderDiplayName { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string expireDate { get; set; }

        public bool IsExpired()
        {
            double ExpireMilis = double.Parse(expireDate);
            double NowInMilis = Utils.GetNowInMillis();

            return (ExpireMilis - NowInMilis) <= 0;
        }


        //public string GetTimeLeft()
        //{
        //    double ExpireMilis = double.Parse(expireDate);
        //    double NowInMilis = Utils.GetNowInMillis();

        //    //Debug.Log("expire milis from server: "+ expireDate);
        //    //     Debug.Log("New milis on client: " + NowInMilis);

        //    double durationLeft = ExpireMilis - NowInMilis;

        //    //    Debug.Log("durationLeft : " + durationLeft);
        //    double minutesLeft = durationLeft / 60000;
        //    double hoursLeft = durationLeft / 3600000;


        //    //    Debug.Log("minutesLeft: " + minutesLeft + "hoursLeft: " + hoursLeft);

        //    if (minutesLeft < 0)
        //        return "Expired";
        //    if (minutesLeft < 2)
        //        return "Less 2 minutes left";
        //    if (minutesLeft < 10)
        //        return "Less 10 minutes left";
        //    else if (minutesLeft < 30)
        //        return "Less 30 minutes left";
        //    else if (minutesLeft < 60)
        //        return "Less than hour left";
        //    else
        //        return "Less than " + Mathf.Ceil((float)hoursLeft) + " hours left";




        //    // Return the time converted into UTC
        //    //    return DateTime.Parse(expireDate);
        //    // return DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(expireDate.ToString())).UtcDateTime;
        //}

        //public ItemSimple GetItem()
        //{
        //    if (contentType == Utils.AUCTION_CONTENT_TYPE.EQUIP)
        //        return contentEquip;
        //    else if (contentType == Utils.AUCTION_CONTENT_TYPE.ITEM)
        //        return contentItem;

        //    Debug.LogError("Action has not ItemSimple as content!!");
        //    return null;
        //}
    }
}
