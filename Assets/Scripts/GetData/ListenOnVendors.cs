using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using Firebase.Functions;
using Newtonsoft.Json;
using RoboRyanTron.Unite2017.Variables;
using simplestmmorpg.playerData;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ListenOnVendors : MonoBehaviour
{

    public AccountDataSO AccountDataSO;
    private ListenerRegistration listenerRegistration;


    //public void Awake()
    //{
    //    AccountDataSO.OnCharacterLoadedFirstTime += StartListening;
    //}

    public void StartListening(string _locationId, string _zoneId)
    {

        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

        listenerRegistration = db.Collection("vendors").WhereEqualTo("position.locationId", _locationId).WhereEqualTo("position.zoneId", _zoneId).Listen(snapshot =>

     {
         AccountDataSO.SetVendorsData(snapshot);
         Debug.Log("New data on VENDORS recieved : " + JsonConvert.SerializeObject(AccountDataSO.VendorsData, Formatting.Indented));
         OnListenerStarted.Invoke();

     });
        Debug.Log("Starting to listen on Vendors Results...");

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
