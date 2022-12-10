using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using Firebase.Functions;
using Newtonsoft.Json;
using RoboRyanTron.Unite2017.Variables;
using simplestmmorpg.data;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ListenOnAuctionHouse : MonoBehaviour
{

    public AccountDataSO AccountDataSO;
    private ListenerRegistration listenerRegistrationOnAllAuctions;
    private ListenerRegistration listenerRegistrationOnAllOffersIPutOnAuction;
    private ListenerRegistration listenerRegistrationOnAllOffersIBiddedOn;
    public List<AuctionOffer> Offers = new List<AuctionOffer>();

    public UnityAction<List<AuctionOffer>> OnAllAuctionsChanged;
    public UnityAction<List<AuctionOffer>> OnAllOffersIPutOnAuctionChanged;
    public UnityAction<List<AuctionOffer>> OnnAllOffersIBiddedOnChanged;

    //public void Awake()
    //{
    //    AccountDataSO.OnCharacterLoadedFirstTime += StartListening;
    //}

    public void StartListeningOnAllAuctions()
    {
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

        listenerRegistrationOnAllAuctions = db.Collection("auctionHouse").WhereGreaterThan("expireDate", Utils.GetNowInMillis().ToString()).Limit(10).Listen(snapshot =>
        //  listenerRegistration = db.Collection("auctionHouse").Limit(10).Listen(snapshot =>

       {
           Offers.Clear();
           foreach (var newItem in snapshot)
           {
               var freshEntry = newItem.ConvertTo<AuctionOffer>();

               Offers.Add(freshEntry);

           }

           if (OnAllAuctionsChanged != null)
               OnAllAuctionsChanged.Invoke(Offers);

           Debug.Log("New data on AUCTION OFFERS recieved : " + JsonConvert.SerializeObject(AccountDataSO.VendorsData, Formatting.Indented));
           OnListenerStarted.Invoke();

       });
        Debug.Log("Starting to listen on Auction Offers...");

    }

    public void StartListeningOnAllOffersIPutOnAuction()
    {

        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

        listenerRegistrationOnAllOffersIPutOnAuction = db.Collection("auctionHouse").WhereEqualTo("sellerUid", AccountDataSO.CharacterData.uid).Listen(snapshot =>

       {
           Offers.Clear();
           foreach (var newItem in snapshot)
           {
               var freshEntry = newItem.ConvertTo<AuctionOffer>();

               Offers.Add(freshEntry);

           }

           if (OnAllOffersIPutOnAuctionChanged != null)
               OnAllOffersIPutOnAuctionChanged.Invoke(Offers);

           Debug.Log("New data on AUCTION OFFERS recieved : " + JsonConvert.SerializeObject(AccountDataSO.VendorsData, Formatting.Indented));
           OnListenerStarted.Invoke();

       });
        Debug.Log("Starting to listen on Auction Offers...");

    }


    public void StartListeningOnAllOffersIBiddedOn()
    {

        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

        listenerRegistrationOnAllOffersIBiddedOn = db.Collection("auctionHouse").WhereEqualTo("highestBidderUid", AccountDataSO.CharacterData.uid).Listen(snapshot =>

        {
            Offers.Clear();
            foreach (var newItem in snapshot)
            {
                var freshEntry = newItem.ConvertTo<AuctionOffer>();

                Offers.Add(freshEntry);

            }

            if (OnnAllOffersIBiddedOnChanged != null)
                OnnAllOffersIBiddedOnChanged.Invoke(Offers);

            Debug.Log("New data on AUCTION BIDDED ON recieved : " + JsonConvert.SerializeObject(AccountDataSO.VendorsData, Formatting.Indented));
            OnListenerStarted.Invoke();

        });
        Debug.Log("Starting to listen on Auction bidded on ...");

    }

    public void StopListeningOnAllAuctions()
    {
        listenerRegistrationOnAllAuctions.Stop();
    }

    public void StopListeningOnAllOffersIPutOnAuction()
    {
        listenerRegistrationOnAllOffersIPutOnAuction.Stop();
    }

    public void StopListeningOnAllOffersIBiddedOn()
    {
        listenerRegistrationOnAllOffersIBiddedOn.Stop();
    }


    public void OnDestroy()
    {
        if (listenerRegistrationOnAllAuctions != null)
            listenerRegistrationOnAllAuctions.Stop();

        if (listenerRegistrationOnAllOffersIPutOnAuction != null)
            listenerRegistrationOnAllOffersIPutOnAuction.Stop();

        if (listenerRegistrationOnAllOffersIBiddedOn != null)
            listenerRegistrationOnAllOffersIBiddedOn.Stop();

    }
    public UnityEvent OnListenerStarted;


}
