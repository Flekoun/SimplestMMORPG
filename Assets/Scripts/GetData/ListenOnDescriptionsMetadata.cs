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

public class ListenOnDescriptionsMetadata : MonoBehaviour
{

    public AccountDataSO AccountDataSO;



    public string skillsMetadataDataPath
    {
        get
        {
            return "_metadata_coreDefinitions/Descriptions";
        }
    }

    private List<ListenerRegistration> listenerRegistrations = new List<ListenerRegistration>();


    public void StartListening()
    {

        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

        ListenerRegistration listenerRegistration = db.Document(skillsMetadataDataPath).Listen(snapshotSkillsMeta =>   //skills metadata
        {

            AccountDataSO.SetDescriptionsMetadata(snapshotSkillsMeta);
            Debug.Log("New Data for METADATA recieved " + JsonConvert.SerializeObject(snapshotSkillsMeta, Formatting.Indented));

        });

        listenerRegistrations.Add(listenerRegistration);



        Debug.Log("Starting to listen on Metadata....");
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
