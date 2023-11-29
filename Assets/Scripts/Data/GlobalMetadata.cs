using System;
using System.Collections.Generic;
using Firebase.Firestore;
using UnityEngine;


namespace simplestmmorpg.data
{

    [Serializable]
    [FirestoreData]
    public class GlobalMetadata
    {
        [field: SerializeField]
        [FirestoreProperty]
        public string serverVersion { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int gameDay { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string nextGameDayTimestamp { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int nextSeasonStartDelayInHours { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int seasonDurationDays { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string nextSeasonStartTimestamp { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int seasonNumber { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int BTC_USD_ExchangeRate { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public double SATOSHIUM_SATS_ExchangeRate { get; set; }



        public double GetPriceOfSatoshiumInFiat(int _satoshiumAmount)
        {
            double pricePerSatoshi = (double)BTC_USD_ExchangeRate / 100000000;
            //Debug.Log("pricePerSatoshi:" + pricePerSatoshi);
            //Debug.Log("_satoshiumAmount:" + _satoshiumAmount);
            //Debug.Log("BTC_USD_ExchangeRate:" + BTC_USD_ExchangeRate);
            //Debug.Log("SATOSHIUM_SATS_ExchangeRate:" + SATOSHIUM_SATS_ExchangeRate);

            //Debug.Log("s:" + Math.Round((SATOSHIUM_SATS_ExchangeRate * _satoshiumAmount) * pricePerSatoshi, 2));
            return Math.Round((SATOSHIUM_SATS_ExchangeRate * _satoshiumAmount) * pricePerSatoshi, 2);
        }

    }



}

