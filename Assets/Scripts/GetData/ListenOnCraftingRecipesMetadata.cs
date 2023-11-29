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

public class ListenOnCraftingRecipesMetadata : MonoBehaviour
{
    public AccountDataSO AccountDataSO;

    public string path
    {
        get
        {
            return "_metadata_coreDefinitions/CraftingRecipes";
        }
    }

    public void Awake()
    {
        AccountDataSO.OnClientVersionMatch += OnClientVersionMatch;
    }

    private void OnClientVersionMatch(bool _match)
    {
        if (_match)
            StartListening();
        else
            Debug.Log("VERSION_MISMATCH");
    }


    private List<ListenerRegistration> listenerRegistrations = new List<ListenerRegistration>();


    private void StartListening()
    {
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

        ListenerRegistration listenerRegistration = db.Document(path).Listen(snapshot =>   
        {
            AccountDataSO.SetCraftingRecipesMetadata(snapshot);
            Debug.Log("New Data for Crafting Recipes  recieved ");

        });

        listenerRegistrations.Add(listenerRegistration);


        Debug.Log("Starting to listen on craftingRecipes Metadata....");
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
