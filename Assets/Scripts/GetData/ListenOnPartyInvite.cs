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

public class ListenOnPartyInvite : MonoBehaviour
{

    public AccountDataSO AccountDataSO;
    private ListenerRegistration listenerRegistration;


    public void Awake()
    {
        AccountDataSO.OnCharacterLoadedFirstTime += StartListening;
    }

    public void StartListening()
    {
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        ListenerRegistration listenerRegistration = db.Collection("partyInvites").WhereEqualTo("invitedCharacterUid", AccountDataSO.CharacterData.uid).Listen(snapshot =>

     {

         if (snapshot.Count > 1)
             Debug.LogError("There is more than 1 party invite! How is this possible?!");

         if (snapshot.Count == 0)
             AccountDataSO.SetPartyInviteData(null);

         foreach (var item in snapshot)
         {
             AccountDataSO.SetPartyInviteData(item);
         }

         Debug.Log("New data on PARTY INVITE recieved : " + snapshot.Count + " -- " + JsonConvert.SerializeObject(AccountDataSO.PartyInviteData, Formatting.Indented));
         // OnListenerStarted.Invoke();

     });
        Debug.Log("Starting to listen on Party Invites...");

    }

    public void StopListening()
    {
        listenerRegistration.Stop();
    }

    public void OnDestroy()
    {
        if (listenerRegistration != null)
            listenerRegistration.Stop();

    }
    // public UnityEvent OnListenerStarted;


}
