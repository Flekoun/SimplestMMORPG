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

public class ListenOnInbox : MonoBehaviour
{

    public AccountDataSO AccountDataSO;
    private ListenerRegistration listenerRegistration;
    private ListenerRegistration listenerRegistrationPlayerInbox;

    public void Awake()
    {
        AccountDataSO.OnCharacterLoadedFirstTime += StartListeningOnCharacterInbox;
        AccountDataSO.OnPlayerDataLoadedFirstTime += StartListeningOnPlayerInbox;
    }

    public void StartListeningOnCharacterInbox()
    {

        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        listenerRegistration = db.Collection("inbox").WhereEqualTo("recipientUid", AccountDataSO.CharacterData.uid).Listen(snapshot =>

    {
        AccountDataSO.SetCharacterInboxData(snapshot);
        Debug.Log("New data on INBOX recieved");// + JsonConvert.SerializeObject(AccountDataSO.VendorsData, Formatting.Indented));

    });


    }

    public void StartListeningOnPlayerInbox()
    {

        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

        listenerRegistrationPlayerInbox = db.Collection("inboxPlayer").WhereEqualTo("recipientUid", AccountDataSO.PlayerData.uid).Listen(snapshot =>

        {
            AccountDataSO.SetPlayerInboxData(snapshot);
            Debug.Log("New data on plyer INBOX recieved");// + JsonConvert.SerializeObject(AccountDataSO.VendorsData, Formatting.Indented));

        });

    }

    public void StopListening()
    {
        listenerRegistration?.Stop();
        listenerRegistrationPlayerInbox?.Stop();
    }

    public void OnDestroy()
    {
        listenerRegistration?.Stop();
        listenerRegistrationPlayerInbox?.Stop();

    }


}
