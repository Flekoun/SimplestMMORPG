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

public class ListenOnGatherables : MonoBehaviour
{

    public AccountDataSO AccountDataSO;
    private ListenerRegistration listenerRegistration;


    public Action<List<Gatherable>> OnNewData;

    public void StartListeningOnPosition(WorldPosition _position)
    {
        StopListening();

        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        ListenerRegistration listenerRegistration = db.Collection("gatherables").WhereEqualTo("position.pointOfInterestId", _position.pointOfInterestId).WhereEqualTo("position.locationId", _position.locationId).WhereEqualTo("position.zoneId", _position.zoneId).Listen(snapshot =>

        {

            List<Gatherable> newData = new List<Gatherable>();

            foreach (var doc in snapshot)
            {
                var entry = doc.ConvertTo<Gatherable>();
                newData.Add(entry);
            }



            OnNewData?.Invoke(newData);


            Debug.Log("Starting to to new Gatherable PoI ..." + _position.pointOfInterestId);

        });


    }

    public void StopListening()
    {
        listenerRegistration?.Stop();
    }

    public void OnDestroy()
    {
        StopListening();

    }
    // public UnityEvent OnListenerStarted;


}
