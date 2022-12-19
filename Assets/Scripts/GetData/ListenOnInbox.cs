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

public class ListenOnInbox : MonoBehaviour
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
        ListenerRegistration listenerRegistration = db.Collection("inbox").WhereEqualTo("characterRecipientUid", AccountDataSO.CharacterData.uid).Listen(snapshot =>

     {
         AccountDataSO.SetInboxData(snapshot);
         Debug.Log("New data on INBOX recieved");// + JsonConvert.SerializeObject(AccountDataSO.VendorsData, Formatting.Indented));
         OnListenerStarted.Invoke();

     });
        Debug.Log("Starting to listen on Inbox Results...");

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
    public UnityEvent OnListenerStarted;


}
