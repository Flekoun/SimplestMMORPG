using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
//using System.Threading.Tasks;
using Firebase.Database;
using Google.MiniJSON;
using simplestmmorpg.data;
using Unity.VisualScripting;
//using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEngine.UIElements.UxmlAttributeDescription;

public class RealtimeDatabasePresence : MonoBehaviour
{
    public class PresenceStatus
    {
        public string characterUid;
        public string state;
    }

    public AccountDataSO AccountDataSO;
    private DatabaseReference userStatusDatabaseRef;

    public void Awake()
    {
        AccountDataSO.OnCharacterLoadedFirstTime += Refresh;

    }

    private void ConnectToPresenceDatabaseAndListenForDiconnect()
    {
        //Nefacha mi timestamp a offline ukladni objektu , uklada se jako string ktery ma v sobe json.... je to kvuli tomu ze volam SetValue a ne SetRawJsonValueAsync
        PresenceStatus offline = new PresenceStatus();
        offline.state = "offline";
        offline.characterUid = AccountDataSO.CharacterData.uid;
        /// offline.LastSeen = ServerValue.Timestamp;
        string offlineJson = JsonUtility.ToJson(offline);


        PresenceStatus online = new PresenceStatus();
        online.state = "online";
        online.characterUid = AccountDataSO.CharacterData.uid;
        //online.LastSeen = ServerValue.Timestamp;
        string onlineJson = JsonUtility.ToJson(online);

        // If we are currently connected, then use the 'onDisconnect()'
        // method to add a set which will only trigger once this 
        // client has disconnected by closing the app, 
        // losing internet, or any other means.

        //TODO: TADY JE hezky ze se sice smaze zaznam muj ze statusu kdyz sem disconected ale uz se neprida kdyz sem connected...

        userStatusDatabaseRef.OnDisconnect().RemoveValue().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
//                Debug.Log("Presence setup Success!!");

                //    // We can now safely set ourselves as 'online' knowing that the
                //    // server will mark us as offline once we lose connection.
                userStatusDatabaseRef.SetRawJsonValueAsync(onlineJson);
            }
           // else Debug.Log("Presence setup Fail!");

        });
    }
    public void Refresh()
    {


        //Lets listen on players joining and leaving
        DatabaseReference statusRef = FirebaseDatabase.DefaultInstance.RootReference.Child("presenceStatus");

        // var userStatusFirestoreRef =  Firebase.Firestore.do .firestore().doc('/status/' + uid);


        statusRef.ChildAdded += CheckForNumberOfPlayersOnline;
        statusRef.ChildRemoved += CheckForNumberOfPlayersOnline;
        StartCoroutine(CheckForNumberOfPlayersOnline());


        // Create a reference to this user's specific status node.
        // This is where we will store data about being online/offline.
        //  var userStatusDatabaseRef = firebase.database().ref ('/status/' + uid);
        userStatusDatabaseRef = statusRef.Child(AccountDataSO.CharacterData.uid);




        // Create a reference to the special '.info/connected' path in 
        // Realtime Database. This path returns `true` when connected
        // and `false` when disconnected.
        //Tohle mi detekuje kdyz sem offline nebo online
        DatabaseReference connectedReference = FirebaseDatabase.DefaultInstance.RootReference.Child(".info").Child("connected");
        connectedReference.ValueChanged += ConnectedReference_ValueChanged;



        ConnectToPresenceDatabaseAndListenForDiconnect();

        //Tohle je jen test transakce ktera zveda pocet hracu o 1 pri loginu
        AddUserAndIncrementCount(FirebaseDatabase.DefaultInstance.GetReference("playersOnline"));

    }


    private void ConnectedReference_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        if (bool.Parse(e.Snapshot.GetRawJsonValue()))
        {
            Debug.Log("jsem connected");
            ConnectToPresenceDatabaseAndListenForDiconnect();
        }
        else
        {
            Debug.Log("jsem disconected");
        }
    }

    private IEnumerator CheckForNumberOfPlayersOnline()
    {
        var DBTask = FirebaseDatabase.DefaultInstance.RootReference.Child("presenceStatus").GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        DataSnapshot snapshot = DBTask.Result;
        AccountDataSO.SetOnlinePlayersCount(snapshot.ChildrenCount - 1); //-1 protoze je tam dummy jeden hrac
    }


    public void CheckForNumberOfPlayersOnline(object sender, ChildChangedEventArgs e)
    {

        StartCoroutine(CheckForNumberOfPlayersOnline());

    }

    //nepouzivam ted nikde, jakoze nezobrazuju atp....
    void AddUserAndIncrementCount(DatabaseReference _ref)
    {

        _ref.RunTransaction(usersData =>
            {
                Dictionary<string, object> users = usersData.Value as Dictionary<string, object>;
                if (users == null)
                { // firstTime
//                    Debug.Log("FIST TIME");
                    users = new Dictionary<string, object>();
                    users.Add("count", 1);
                    users.Add("playersList", new Dictionary<string, object>() { { AccountDataSO.CharacterData.uid, 1 } });
                }
                else
                {
              //      Debug.Log("MORE TIME");
                    // INCREMENT COUNT
                    users["count"] = int.Parse(users["count"].ToString()) + 1;

                    // ADD USER TO LIST
                    Dictionary<string, object> userList = users["playersList"] as Dictionary<string, object>;
                    if (!userList.ContainsKey(AccountDataSO.CharacterData.uid))
                    {
                        userList.Add(AccountDataSO.CharacterData.uid, 1);
                    }
                    else
                    {
                        userList[AccountDataSO.CharacterData.uid] = int.Parse(userList[AccountDataSO.CharacterData.uid].ToString()) + 1;
                    }
                    users["playersList"] = userList;
                }
                // END TRANSACTION

                usersData.Value = users;
                return TransactionResult.Success(usersData);
            }).ContinueWith(OnAddUserIncrementCountTask);
    }



    void OnAddUserIncrementCountTask(Task<DataSnapshot> task)
    {
        if (task.IsCompleted && !task.IsCanceled && !task.IsFaulted)
        {
            //Success
        }
        else
        {
            Debug.Log(task.IsFaulted + " - " + task.Exception.Message);
            // True - Exception of type 'System.AggregateException' was thrown.
        }
    }

}


