using System.Collections;
using System.Collections.Generic;
using Firebase;
using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;
using System.Threading.Tasks;
using UnityEngine.Events;
using Firebase.Extensions;

public class FirebaseStart : MonoBehaviour
{
    // Start is called before the first frame update
    public void Start()
    {
        Application.targetFrameRate =60 ;

        FirebaseApp app;
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
               
                // app = FirebaseApp.DefaultInstance;  //TODO: ZAKOMENTOVANO PROTOZE SE ZASEKAVALA UNITY...nejaky bug ci co tu maji mrkni na net https://stackoverflow.com/questions/69689474/firebase-makes-unity-hang
                Debug.Log("Firebase Ready");
                OnFirebaseInitialized.Invoke();

                // Set a flag here to indicate whether Firebase is ready to use by your app.
            }
            else
            {
                Debug.LogError(System.String.Format("Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                OnFirebaseInitializeFailed.Invoke();

                // Firebase Unity SDK is not safe to use here.
            }
        });


    }

    public UnityEvent OnFirebaseInitialized;
    public UnityEvent OnFirebaseInitializeFailed;

    public void OnDisable()
    {
        Firebase.FirebaseApp.DefaultInstance.Dispose();
    }

}
