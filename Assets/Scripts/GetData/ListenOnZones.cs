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

    public string getPointOfInterest
    {
        get
        {
            return getLocation + "/pointsOfInterest/" + AccountDataSO.CharacterData.position.pointOfInterestId;
        }
    }

    private ListenerRegistration listenerRegistrationZone;
    private ListenerRegistration listenerRegistrationLocation;
    private ListenerRegistration listenerRegistrationPointOfInterest;



    public void Awake()
    {
        AccountDataSO.OnWorldLocationChanged += StartListeningOnNewLocation;
        AccountDataSO.OnWorldZoneChanged += StartListeningOnNewZone;
        AccountDataSO.OnWorldPointOfInterestChanged += StartListeningOnNewPointOfInterest;
        AccountDataSO.OnCharacterLoadedFirstTime += AfterCharacterIsLoadedFirstTime;
    }

    private void AfterCharacterIsLoadedFirstTime()
    {
        StartListeningOnNewLocation();
        StartListeningOnNewZone();
        StartListeningOnNewPointOfInterest();
    }

    public void StartListeningOnNewLocation()
    {

        StopListeningOnLocation();

        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

        listenerRegistrationLocation = db.Document(getLocation).Listen(snapshot =>
        {

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


    public void StartListeningOnNewPointOfInterest()
    {

        StopListeningOnPointOfInterest();

        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

        listenerRegistrationLocation = db.Document(getPointOfInterest).Listen(snapshot =>
        {

            AccountDataSO.SetPointOfInterest(snapshot);
            Debug.Log("Listening on new PointOfInterest " + AccountDataSO.CharacterData.position.pointOfInterestId);
        });

    }

    public void OnDestroy()
    {
        StopListeningOnZone();
        StopListeningOnLocation();
        StopListeningOnPointOfInterest();
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

    public void StopListeningOnPointOfInterest()
    {
        if (listenerRegistrationPointOfInterest != null)
            listenerRegistrationPointOfInterest.Stop();
    }


}
