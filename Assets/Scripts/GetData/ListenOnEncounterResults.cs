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
    //  public Action<List<Gatherable>> OnNewData;

    public void Awake()
    {
        AccountDataSO.OnWorldPointOfInterestChanged += StartListening;
        AccountDataSO.OnCharacterLoadedFirstTime += StartListening;
    }

    public void StartListening()
    {
        StopListening();

        AccountDataSO.EncounterResultsData.Clear();
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

        ListenerRegistration listenerRegistration = db.Collection("encounterResults").WhereArrayContains("combatantsWithUnclaimedRewardsList", AccountDataSO.CharacterData.uid).WhereEqualTo("position.locationId", AccountDataSO.CharacterData.position.locationId).WhereEqualTo("position.zoneId", AccountDataSO.CharacterData.position.zoneId).WhereEqualTo("position.pointOfInterestId", AccountDataSO.CharacterData.position.pointOfInterestId).Listen(snapshot =>

     {
         AccountDataSO.SetEncounterResultsData(snapshot);
         Debug.Log("New data on ENCOUNTER RESULTS recieved : " + JsonConvert.SerializeObject(AccountDataSO.EncountersData, Formatting.Indented));
         OnListenerStarted.Invoke();

     });
        Debug.Log("Starting to listen on new Encounter Results...");

    }

    public void StopListening()
    {
        listenerRegistration?.Stop();
    }

    public void OnDestroy()
    {
        StopListening();

    }

    public UnityEvent OnListenerStarted;


}
