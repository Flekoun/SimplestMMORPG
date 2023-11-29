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

public class ListenOnLivePointOfInterestTiers : MonoBehaviour
{

    //public string ZoneId = "DUNOTAR";
    //public string LocationId = "VALLEY_OF_TRIALS";


    private ListenerRegistration ListenerRegistration = null;


    public void StartListening(string _zoneId, string _locationId, string _pointOfInterest)
    {

        //if (ListenerRegistration != null)
        //    return;

        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

        ListenerRegistration = db.Collection("_metadata_zones").Document(_zoneId).Collection("locations").Document(_locationId).Collection("pointsOfInterest").Document(_pointOfInterest).Collection("definitions").Document("TIERS").Listen(snapshot =>
        {
            AdminToolsManager.instance.SetServerData(snapshot);

        });

        //ListenerRegistration = db.Collection("_internal_defintions").Document("MAP_GENERATOR").Listen(snapshot =>
        //{
        //    AdminToolsManager.instance.SetTiersData(snapshot);

        //});


    }



    public void StopListening()
    {
        ListenerRegistration?.Stop();


    }



}
