using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using Firebase.Functions;
using RoboRyanTron.Unite2017.Variables;
using simplestmmorpg.data;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ListenOnParty : MonoBehaviour
{

    public AccountDataSO AccountDataSO;
    private ListenerRegistration listenerRegistration;


    public void Awake()
    {
        AccountDataSO.OnCharacterLoadedFirstTime += StartListeningOnParty;
    }

    public void StartListeningOnParty()
    {
        Debug.Log("Starting to listen on Party ...");

        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        ListenerRegistration listenerRegistration = db.Collection("parties").WhereArrayContains("partyMembersUidList", AccountDataSO.CharacterData.uid).Listen(snapshot =>

     {

         Debug.Log("New data from Listener recieved : " + this.gameObject.name);

         if (snapshot.Count > 1)
             Debug.LogError("You are in more than 1 party! How is this possible?!");

         if (snapshot.Count == 0)
             AccountDataSO.SetPartyData(null);

         foreach (var item in snapshot)
         {
             AccountDataSO.SetPartyData(item);
         }

         //         Debug.Log("New data on PARTY  recieved : " + snapshot .Count + " -- "+ JsonConvert.SerializeObject(AccountDataSO.PartyInviteData, Formatting.Indented));
         // OnListenerStarted.Invoke();

     });

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
