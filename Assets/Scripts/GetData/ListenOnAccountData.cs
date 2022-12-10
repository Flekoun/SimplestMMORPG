using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using Firebase.Functions;
using Newtonsoft.Json;
using RoboRyanTron.Unite2017.Variables;

using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ListenOnAccountData : MonoBehaviour
{

    public AccountDataSO AccountDataSO;

    private bool isListening = false;
    //    public List<Action> OnDataRefreshed = new List<Action>();


    public string playerDataPath
    {
        get
        {
            return "players/" + FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        }
    }

    private ListenerRegistration listenerRegistration_Player;

    private ListenerRegistration listenerRegistration_Character;

    //public void Start()
    //{
    //    AccountDataSO.OnSkillsMetadataChanged += StartListening;
    //}

    public void StartListeningOnPlayer()
    {

        if (!isListening)
        {
            FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

            listenerRegistration_Player = db.Document(playerDataPath).Listen(snapshot =>   //player
            {
                AccountDataSO.SetPlayerData(snapshot);
                Debug.Log("New Data for PLAYER recieved : " + JsonConvert.SerializeObject(AccountDataSO.PlayerData, Formatting.Indented));



            });

            Debug.Log("Starting to listen on Character and Player....");

        }
        isListening = true;

       // OnListenerStarted.Invoke();

    }

    public void StartListeningOnCharacter(string _characterUid)
    {
        if (AccountDataSO.PlayerData == null)
        {
            Debug.LogError("You need to get Player data first!");
            return;
        }

        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

        listenerRegistration_Character = db.Collection("characters").Document(_characterUid).Listen(snapshot =>
        {
            AccountDataSO.SetCharacterData(snapshot);

      //      Debug.Log("New Data for CHARACTER recieved : " + JsonConvert.SerializeObject(AccountDataSO.CharacterData, Formatting.Indented));

           
        });


        Debug.Log("Starting to listen on Character ....");



    }

    public void StopListeningOnPlayer()
    {
        if (listenerRegistration_Player != null)
            listenerRegistration_Player.Stop();
    }

    public void StopListeningOnCharacter()
    {
        if (listenerRegistration_Character != null)
            listenerRegistration_Character.Stop();
    }



    public void OnDestroy()
    {
        StopListeningOnPlayer();
        StopListeningOnCharacter();

    }

    //public UnityEvent OnListenerStarted;


}
