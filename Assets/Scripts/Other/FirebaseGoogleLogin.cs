using System.Collections;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Google;
using GooglePlayGames;
using Unity.Burst.Intrinsics;

using UnityEngine;
using UnityEngine.Events;

public class FirebaseGoogleLogin : MonoBehaviour
{

    //TOHLE VYTVORI NOVY! GOOLE account potrebuju spis link
    public string GoogleWebAPI = "41009291810-5jk1h55rvo66tgh4mg3dgmvp2muggfl3.apps.googleusercontent.com";

    private GoogleSignInConfiguration configuration;

    DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;
    FirebaseAuth auth;
    FirebaseUser user;



    public void Awake()
    {
        configuration = new GoogleSignInConfiguration
        {
            WebClientId = GoogleWebAPI,
            RequestIdToken = true
        };

    }

    void Start()
    {
        InitFirebase();
    }

    void InitFirebase()
    {
        auth = FirebaseAuth.DefaultInstance;

        //tohle pry pouzit na to jaky ma kde linkle ucty ale potrebuju email.....
        // auth.FetchProvidersForEmailAsync()
    }

    public void GoogleLinkClick()
    {
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;
        GoogleSignIn.Configuration.RequestEmail = true;

        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnGoogleAuthenticatedFinsihedLetsLink);

    }

    void OnGoogleAuthenticatedFinsihedLetsLink(Task<GoogleSignInUser> task)
    {
        if (task.IsFaulted)
        {
            Debug.LogError("Fault");
        }
        else if (task.IsCanceled)
        {
            Debug.LogError("Login canceled");
        }
        else
        {
            Credential credential = GoogleAuthProvider.GetCredential(task.Result.IdToken, null);


            auth.CurrentUser.LinkWithCredentialAsync(credential).ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("LinkWithCredentialAsync was canceled.");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.LogError("LinkWithCredentialAsync encountered an error: " + task.Exception);
                    //tad to zahlasi te uz mas linkly ucet kdyz zkousis link ale uz ses linkly
                    //TODO: tak klidne zkusit tady zavolat ten sign in? 
                  //  GoogleSignInClick();
                    return;
                }

                Firebase.Auth.FirebaseUser newUser = task.Result;
                Debug.LogFormat("Credentials successfully linked to Firebase user: {0} ({1})", newUser.DisplayName, newUser.UserId);

            });
        }
    }

    UnityAction<bool> OnFinished;
    public void GoogleSignInClick(UnityAction<bool> _onFinished)
    {
        OnFinished = _onFinished;

        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;
        GoogleSignIn.Configuration.RequestEmail = true;

        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnGoogleAuthenticatedFinsihed);

    }

    void OnGoogleAuthenticatedFinsihed(Task<GoogleSignInUser> task)
    {
        if (task.IsFaulted)
        {
            Debug.LogError("Fault");
        }
        else if (task.IsCanceled)
        {
            Debug.LogError("Login canceled");
        }
        else
        {
            Credential credential = GoogleAuthProvider.GetCredential(task.Result.IdToken, null);

            auth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("SignInWithCredentialsAsync was canceled!");
                    if (OnFinished != null)
                        OnFinished.Invoke(false);
                    return;
                }

                if (task.IsFaulted)
                {
                    Debug.LogError("SignInWithCredentialsAsync has error: " + task.Exception);
                    if (OnFinished != null)
                        OnFinished.Invoke(false);
                    return;
                }

                user = auth.CurrentUser;

                Debug.Log("Everyhting seems ok!");
                Debug.Log(user.DisplayName);
                Debug.Log(user.Email);

                if (OnFinished != null)
                    OnFinished.Invoke(true);
            });
        }
    }
    /*
    The call to LinkWithCredentialAsync will fail if the credentials are already linked to another user account.In this situation, you must handle merging the accounts and associated data as appropriate for your app:
    string currentUserId = auth.CurrentUser.UserId;
    string currentEmail = auth.CurrentUser.Email;
    string currentDisplayName = auth.CurrentUser.DisplayName;
    System.Uri currentPhotoUrl = auth.CurrentUser.PhotoUrl;

    // Sign in with the new credentials.
    auth.SignInWithCredentialAsync(credential).ContinueWith(task => {
        if (task.IsCanceled)
        {
            Debug.LogError("SignInWithCredentialAsync was canceled.");
            return;
        }
        if (task.IsFaulted)
        {
            Debug.LogError("SignInWithCredentialAsync encountered an error: " + task.Exception);
            return;
        }

        Firebase.Auth.FirebaseUser newUser = task.Result;
        Debug.LogFormat("User signed in successfully: {0} ({1})",
            newUser.DisplayName, newUser.UserId);

        // TODO: Merge app specific details using the newUser and values from the
        // previous user, saved above.
    });
    */
}
