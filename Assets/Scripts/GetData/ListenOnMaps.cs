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

public class ListenOnMaps : MonoBehaviour
{

    public AccountDataSO AccountDataSO;



    public string skillsMetadataDataPath
    {
        get
        {
            return "_metadata_coreDefinitions/Maps";
        }
    }

    private ListenerRegistration listenerRegistration;

    public void Awake()
    {
        AccountDataSO.OnCharacterLoadedFirstTime += StartListening;
    }

    public void StartListening()
    {

        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

        listenerRegistration = db.Document(skillsMetadataDataPath).Listen(snapshot =>   
        {

            AccountDataSO.SetMaps(snapshot);
            Debug.Log("New Data for Maps recieved " + JsonConvert.SerializeObject(snapshot, Formatting.Indented));

        });


        Debug.Log("Starting to listen on Maps....");
    }



    public void OnDestroy()
    {
        if (listenerRegistration != null)
            StopListening();


    }

    public void StopListening()
    {
        listenerRegistration.Stop();
    }


}
