using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

public class ListenOnQuestgivers : MonoBehaviour
{

    public AccountDataSO AccountDataSO;
    private ListenerRegistration listenerRegistrationOnWorldPosition;
    // private ListenerRegistration listenerRegistrationOnAllOffersIPutOnAuction;
    // private ListenerRegistration listenerRegistrationOnAllOffersIBiddedOn;
    public List<QuestgiverMeta> Questgivers = new List<QuestgiverMeta>();

    public UnityAction<List<QuestgiverMeta>> OnQuestgiversAtCharacterWorldPositionChanged;
    //public UnityAction<List<AuctionOffer>> OnAllOffersIPutOnAuctionChanged;
    //  public UnityAction<List<AuctionOffer>> OnnAllOffersIBiddedOnChanged;

    //public void Awake()
    //{
    //    AccountDataSO.OnCharacterLoadedFirstTime += StartListening;
    //}

    public void StartListeningOnQuestgiversAtCharacterPosition()
    {
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

        listenerRegistrationOnWorldPosition = db.Collection("_metadata_questgivers").WhereEqualTo("position.locationId", AccountDataSO.CharacterData.position.locationId).WhereEqualTo("position.zoneId", AccountDataSO.CharacterData.position.zoneId).Listen(snapshot =>
       {

           Questgivers.Clear();
           foreach (var newItem in snapshot)
           {
               var freshEntry = newItem.ConvertTo<QuestgiverMeta>();
               Questgivers.Add(freshEntry);
           }

           ////vyfiltruju questgivery ktere jsem uz splnil pryc.....

           //for (int i = Questgivers.Count - 1; i >= 0; i--)
           //{
           //    if (AccountDataSO.CharacterData.questgiversClaimed.Contains(Questgivers[i].uid))
           //        Questgivers.RemoveAt(i);
           //}



           if (OnQuestgiversAtCharacterWorldPositionChanged != null)
               OnQuestgiversAtCharacterWorldPositionChanged.Invoke(Questgivers);

           Debug.Log("New data on QUESTGIVERS AT WORLD POSITION recieved : " + JsonConvert.SerializeObject(AccountDataSO.VendorsData, Formatting.Indented));
           //  OnListenerStarted.Invoke();

       });
        Debug.Log("Starting to listen on Questgivers...");

    }



    public void StopListeningOnQuestgivcersAtCharacterWorldPosition()
    {
        listenerRegistrationOnWorldPosition.Stop();
    }

    //public void StopListeningOnAllOffersIPutOnAuction()
    //{
    //    listenerRegistrationOnAllOffersIPutOnAuction.Stop();
    //}

    //public void StopListeningOnAllOffersIBiddedOn()
    //{
    //    listenerRegistrationOnAllOffersIBiddedOn.Stop();
    //}


    //public void OnDestroy()
    //{
    //    if (listenerRegistrationOnAllAuctions != null)
    //        listenerRegistrationOnAllAuctions.Stop();

    //    if (listenerRegistrationOnAllOffersIPutOnAuction != null)
    //        listenerRegistrationOnAllOffersIPutOnAuction.Stop();

    //    if (listenerRegistrationOnAllOffersIBiddedOn != null)
    //        listenerRegistrationOnAllOffersIBiddedOn.Stop();

    //}
    //  public UnityEvent OnListenerStarted;


}
