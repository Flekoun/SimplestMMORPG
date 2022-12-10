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

public class ListenOnEncounterData : MonoBehaviour
{

    public AccountDataSO AccountDataSO;
    private ListenerRegistration listenerRegistration;

    private string oldLocation = "";
    private string oldZone = "";
    //public void Awake()
    //{
    //    AccountDataSO.OnCharacterLoadedFirstTime += StartListening;
    //}

    public void StartListening(string _locationId, string _zoneId)
    {
        if (oldLocation != _locationId || oldZone != _zoneId) //zmenil jsem lokaci na ktere chci poslouchat
        {
            StopListening();

            AccountDataSO.EncountersData.Clear();

            FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

            listenerRegistration = db.Collection("encounters").WhereArrayContains("watchersList", AccountDataSO.CharacterData.uid).WhereEqualTo("position.locationId", _locationId).WhereEqualTo("position.zoneId", _zoneId).Listen(snapshot =>
            // ListenerRegistration listenerRegistration = db.Collection("encounters").WhereArrayContains("combatantList", AccountDataSO.CharacterData.uid).Listen(snapshot =>

                    {
                        AccountDataSO.SetEncounterData(snapshot);
                        Debug.Log("New data on ENCOUNTERS recieved : " + JsonConvert.SerializeObject(AccountDataSO.EncountersData, Formatting.Indented));
                        //            Debug.Log("Count: " + AccountDataSO.EncountersData.Count.ToString());
                        OnListenerStarted.Invoke();

                    });
            Debug.Log("Starting to listen on Encounters...");

            oldLocation = _locationId;
            oldZone = _zoneId;
            //   listenerRegistrations.Add(listenerRegistration);
        }


    }


    public void OnDestroy()
    {
        StopListening();
    }
    public UnityEvent OnListenerStarted;


    public void StopListening()
    {
        if (listenerRegistration != null)
            listenerRegistration.Stop();
    }
}
