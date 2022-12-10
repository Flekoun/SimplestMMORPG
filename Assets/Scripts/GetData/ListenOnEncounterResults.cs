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

public class ListenOnEncounterResults : MonoBehaviour
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

        ListenerRegistration listenerRegistration = db.Collection("encounterResults").WhereArrayContains("combatantsWithUnclaimedRewardsList", AccountDataSO.CharacterData.uid).Listen(snapshot =>

     {
         AccountDataSO.SetEncounterResultsData(snapshot);
         Debug.Log("New data on ENCOUNTER RESULTS recieved : " + JsonConvert.SerializeObject(AccountDataSO.EncountersData, Formatting.Indented));
         OnListenerStarted.Invoke();

     });
        Debug.Log("Starting to listen on Encounter Results...");

    }

    public void OnDestroy()
    {
        if (listenerRegistration != null)
            listenerRegistration.Stop();

    }
    public UnityEvent OnListenerStarted;


}
