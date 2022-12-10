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

public class ListenOnOtherMetadata : MonoBehaviour
{
    public AccountDataSO AccountDataSO;

    public string path
    {
        get
        {
            return "_metadata_coreDefinitions/Other";
        }
    }

    private List<ListenerRegistration> listenerRegistrations = new List<ListenerRegistration>();


    public void StartListening()
    {
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

        ListenerRegistration listenerRegistration = db.Document(path).Listen(snapshot =>   
        {
            AccountDataSO.SetOtherMetadata(snapshot);
            Debug.Log("New Data for OTHER METADATA recieved " + JsonConvert.SerializeObject(snapshot, Formatting.Indented));

        });

        listenerRegistrations.Add(listenerRegistration);


        Debug.Log("Starting to listen on other Metadata....");
    }



    public void OnDestroy()
    {
        for (int i = listenerRegistrations.Count - 1; i >= 0; i--)
        {
            listenerRegistrations[i].Stop();
        }

        listenerRegistrations.Clear();


    }



}
