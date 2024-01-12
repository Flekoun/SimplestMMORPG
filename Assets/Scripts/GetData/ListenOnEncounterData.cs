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

public class ListenOnEncounterData : MonoBehaviour
{

    public AccountDataSO AccountDataSO;
    private ListenerRegistration listenerRegistration;

    //private string oldLocation = "";
    //private string oldZone = "";
    //private string oldPointOfInterestId = "";
    //public void Awake()
    //{
    //    AccountDataSO.OnCharacterLoadedFirstTime += StartListening;
    //}

    public void Awake()
    {
        AccountDataSO.OnWorldPointOfInterestChanged += StartListening;
        AccountDataSO.OnCharacterLoadedFirstTime += StartListening;

    }

    public void StartListening()
    {
        //if (oldLocation != _locationId || oldZone != _zoneId || oldPointOfInterestId != _pointOfInterestId) //zmenil jsem lokaci na ktere chci poslouchat
        //{
        StopListening();

        AccountDataSO.EncountersData.Clear();

        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

        //  listenerRegistration = db.Collection("encounters").WhereArrayContains("watchersList", AccountDataSO.CharacterData.uid).WhereEqualTo("position.locationId", AccountDataSO.CharacterData.position.locationId).WhereEqualTo("position.zoneId", AccountDataSO.CharacterData.position.zoneId).WhereEqualTo("position.pointOfInterestId", AccountDataSO.CharacterData.position.pointOfInterestId).Listen(snapshot =>
        listenerRegistration = db.Collection("encounters").WhereArrayContains("watchersList", AccountDataSO.CharacterData.uid).WhereEqualTo("position.locationId", AccountDataSO.CharacterData.position.locationId).WhereEqualTo("position.zoneId", AccountDataSO.CharacterData.position.zoneId).Listen(snapshot =>

        {
            AccountDataSO.SetEncounterData(snapshot);
            Debug.Log("Listener recieved new ENCOUNTERS data ");
            //            Debug.Log("Count: " + AccountDataSO.EncountersData.Count.ToString());
            OnListenerStarted.Invoke();

        });
        Debug.Log("Starting to listen on Encounters...");

        //oldLocation = _locationId;
        //oldZone = _zoneId;
        //oldPointOfInterestId = _pointOfInterestId;
        //   listenerRegistrations.Add(listenerRegistration);
        //}


    }


    public void OnDestroy()
    {
        StopListening();
    }
    public UnityEvent OnListenerStarted;


    public void StopListening()
    {
        listenerRegistration?.Stop();
    }
}
