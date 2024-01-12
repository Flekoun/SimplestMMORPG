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

public class ListenOnEnemyDropTables : MonoBehaviour
{

    //public string ZoneId = "DUNOTAR";
    //public string LocationId = "VALLEY_OF_TRIALS";


    private ListenerRegistration ListenerRegistration = null;


    public void StartListening(string _zoneId , string _locationId)
    {

        if (ListenerRegistration != null)
            return;

        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

        ListenerRegistration listenerRegistration = db.Collection("_metadata_dropTables").Document(_zoneId).Collection("locations").Document(_locationId).Listen(snapshot =>
        {
            AdminToolsManager.instance.SetEnemyDropTablesData(snapshot);

        });

        ListenerRegistration = listenerRegistration;

    }



    public void StopListening()
    {
        ListenerRegistration?.Stop();


    }



}
