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

public class ListenOnZones : MonoBehaviour
{

    public AccountDataSO AccountDataSO;

    public string getZone
    {
        get
        {
            return "_metadata_zones/" + AccountDataSO.CharacterData.position.zoneId ;
        }
    }

    public string getLocation
    {
        get
        {
            return getZone + "/locations/"+AccountDataSO.CharacterData.position.locationId;
        }
    }

    private ListenerRegistration listenerRegistrationZone;
    private ListenerRegistration listenerRegistrationLocation;


    public void Awake()
    {
        AccountDataSO.OnWorldLocationChanged += StartListeningOnNewLocation;
        AccountDataSO.OnWorldZoneChanged += StartListeningOnNewZone;
        AccountDataSO.OnCharacterLoadedFirstTime += AfterCharacterIsLoadedFirstTime;
    }

    private void AfterCharacterIsLoadedFirstTime()
    {
        StartListeningOnNewLocation();
        StartListeningOnNewZone();
    }

    public void StartListeningOnNewLocation()
    {

        StopListeningOnLocation();

        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

//        Debug.Log("getLocation: " + getLocation);
        listenerRegistrationLocation = db.Document(getLocation).Listen(snapshot =>
        {

  //          Debug.Log("LOCA DATA : " + snapshot.ToDictionary());
    //        Debug.Log("LOCA DATA : " + JsonConvert.SerializeObject(snapshot.ToDictionary(), Formatting.Indented));
            AccountDataSO.SetLocation(snapshot);
            Debug.Log("Listening on new Location " + AccountDataSO.CharacterData.position.locationId);

        });


    }

    public void StartListeningOnNewZone()
    {

        StopListeningOnZone();

        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

        listenerRegistrationZone = db.Document(getZone).Listen(snapshot =>
        {

            AccountDataSO.SetZone(snapshot);
            Debug.Log("Listening on new Zone " + AccountDataSO.CharacterData.position.zoneId);

        });


    }

    public void OnDestroy()
    {
        StopListeningOnZone();
        StopListeningOnLocation();
    }

    public void StopListeningOnZone()
    {
        if (listenerRegistrationZone != null)
            listenerRegistrationZone.Stop();
    }

    public void StopListeningOnLocation()
    {
        if (listenerRegistrationLocation != null)
            listenerRegistrationLocation.Stop();
    }


}
