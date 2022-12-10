using System.Collections;
using System.Collections.Generic;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GetData : MonoBehaviour
{
    public TextMeshProUGUI LevelText;

   // public ListenerRegistration listenerRegistration;
    //[FirestoreData]
    //private struct CharacterData
    //{
    //    [FirestoreProperty]
    //    public string UserUid { get; set; }
    //    [FirestoreProperty]
    //    public string Uid { get; set; }
    //    [FirestoreProperty]
    //    public string Name { get; set; }
    //    [FirestoreProperty]
    //    public string CharacterClass { get; set; }
    //    [FirestoreProperty]
    //    public int Level { get; set; }


    //}

    public void Start()
    {

       

    }
    // Start is called before the first frame update
    public void GetDataTest()
    {

       

        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

        //listenerRegistration = db.Collection("playersData").Document("typek").Listen(callback =>
        //{
        //    PlayersData data = callback.ConvertTo<PlayersData>();
        //    LevelText.SetText(data.level.ToString());
        //}
        //);

        //  db.Collection("playersData").Document("typek").GetSnapshotAsync()
        Debug.Log("uid: " + FirebaseAuth.DefaultInstance.CurrentUser.UserId);
       db.Collection("playersData").WhereEqualTo("useruid",FirebaseAuth.DefaultInstance.CurrentUser.UserId).GetSnapshotAsync().ContinueWithOnMainThread((task) =>
        {
           // Debug.Log("Status: " + task.Status);

            if (task.IsCanceled)
            {
                Debug.LogError("task CANCELED: " + task.Exception);
            }
            if (task.IsFaulted)
            {
                Debug.LogError("task FAULTED: " + task.Exception);
            }
           

            if (task.Result.Count > 1)
            {
                Debug.LogError("WTF? How we can have more than 1 entry for user?");
                return;
            }

            foreach (DocumentSnapshot documentSnapshot in task.Result.Documents)
            {
             //   PlayersData data = documentSnapshot.ConvertTo<PlayersData>();
             //  LevelText.SetText(data.level.ToString());
            }

        });

    }




    // Update is called once per frame
    void Update()
    {

    }
}
