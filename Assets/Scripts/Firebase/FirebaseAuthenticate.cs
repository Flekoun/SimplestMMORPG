using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SocialPlatforms;
using System.Threading.Tasks;

public class FirebaseAuthenticate : MonoBehaviour
{
    Firebase.Auth.FirebaseAuth auth;
    Firebase.Auth.FirebaseUser user;
    //Start is called before the first frame update

    public UnityAction OnSignedInEvent;
    public UnityAction OnSignedOutEvent;


    void Awake()
    {


        Debug.Log("Setting up Firebase Auth");

        auth = Firebase.Auth.FirebaseAuth.DefaultInstance;


        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);


    }

    //internal void ProcessAuthentication(SignInStatus status)
    //{
    //    if (status == SignInStatus.Success)
    //    {
    //        Debug.Log("vse vypada ok ");
    //        // Continue with Play Games Services
    //    }
    //    else
    //    {
    //        Debug.Log("nemas zadny play games account!");
    //        PlayGamesPlatform.Instance.ManuallyAuthenticate(ManualSignIn);
    //        // Disable your integration with Play Games Services or show a login button
    //        // to ask users to sign-in. Clicking it should call
    //        // PlayGamesPlatform.Instance.ManuallyAuthenticate(ProcessAuthentication).
    //    }
    //}

    //internal void ManualSignIn(SignInStatus status)
    //{
    //    Debug.Log("ManualSignIn : " + status);
    //}

    public void Start()
    {
       // PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);

    }



    void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
     
        if (auth.CurrentUser != user)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;
            if (!signedIn && user != null)
            {
                Debug.Log("Signed out " + user.UserId);
                OnSignedOut.Invoke();
                if (OnSignedOutEvent != null)
                    OnSignedOutEvent.Invoke();
            }
            user = auth.CurrentUser;
            if (signedIn)
            {
                Debug.Log("Signed in " + user.UserId);
                OnSignedIn.Invoke();
                if (OnSignedInEvent != null)
                    OnSignedInEvent.Invoke();
            }
        }
    }

    public void LoginAsAnonymous(Action<bool> _onAnonymousLoginResult)
    {


        FirebaseAuth auth = FirebaseAuth.DefaultInstance;

        auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInAnonymouslyAsync was canceled.");
                _onAnonymousLoginResult.Invoke(false);
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SignInAnonymouslyAsync encountered an error: " + task.Exception);
                _onAnonymousLoginResult.Invoke(false);
                return;
            }

            FirebaseUser newUser = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})", newUser.DisplayName, newUser.UserId);


            //foreach (var info in newUser.ProviderData)
            //{
            //    Debug.Log("aaa");
            //    Debug.Log("providerID: " + info.ProviderId);
            //    Debug.Log("providerID: " + info.UserId);
            //}
            //Debug.Log("aaa");

            _onAnonymousLoginResult.Invoke(true);
        });
    }
    private Action<bool> onGooglePlayLoginResult;
    public void LoginAsPlayGames(Action<bool> _onGooglePlayLoginResult)
    {
        onGooglePlayLoginResult = _onGooglePlayLoginResult;

        Debug.Log("jdu na to1");
        PlayGamesPlatform.Activate();

        PlayGamesPlatform.Instance.Authenticate(
             (SignInStatus status) =>
             {
                 Debug.Log("status:" + status);
             }
             );

        Debug.Log("jdu na to2");
        Social.localUser.Authenticate((bool success) =>
        {
            Debug.Log("success:" + success);
            PlayGamesPlatform.Instance.RequestServerSideAccess(false, (string result) =>
                {
                    string autCode = result;
                    Debug.Log("string:" + result);





                    Firebase.Auth.FirebaseAuth auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
                    Firebase.Auth.Credential credential =
                        Firebase.Auth.PlayGamesAuthProvider.GetCredential(autCode);
                    auth.SignInWithCredentialAsync(credential).ContinueWith(task =>
                    {
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
                        Debug.LogFormat("User signed in successfully: {0} ({1})", newUser.DisplayName, newUser.UserId);


                        if (onGooglePlayLoginResult != null)
                            onGooglePlayLoginResult.Invoke(true);
                    });





                });

            //PlayGamesPlatform.Instance.RequestServerSideAccess(false, OnRequestServerSideAccess);


        });
        Debug.Log("jdu na to3");
    }



    public void LoginWithPassword(string _email, string _pass, UnityAction<bool> _onLoginResult)
    {
        FirebaseAuth auth = FirebaseAuth.DefaultInstance;

        auth.SignInWithEmailAndPasswordAsync(_email, _pass).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInEmaillyAsync was canceled.");
                _onLoginResult.Invoke(false);
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SignInEmailAsync encountered an error: " + task.Exception);
                _onLoginResult.Invoke(false);
                return;
            }

            user = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})", user.DisplayName, user.UserId);

            _onLoginResult.Invoke(true);
        });
    }




    //public void LinkTest()
    //{
    //   Link("falcon.land@seznam.cz", "1234kokosak",null);
    //}

    public void Link(string email, string password, Action<bool> _onEmailLoginResult)
    {
        // (Anonymous user is signed in at that point.)

        // 1. Create the email and password credential, to upgrade the
        // anonymous user.
        var credential = EmailAuthProvider.GetCredential(email, password);

        // 2.Links the credential to the currently signed in user
        // (the anonymous user).

        FirebaseAuth.DefaultInstance.CurrentUser.LinkWithCredentialAsync(credential).ContinueWith(task =>
        {

            if (task.IsCanceled || task.IsFaulted)
            {

                Debug.LogError("LinkWithCredentialAsync was canceled or failed:" + task.Exception);
                if (_onEmailLoginResult != null)
                    _onEmailLoginResult.Invoke(false);

                return;
            }



            user = task.Result;
            Debug.LogFormat("Credentials successfully linked to Firebase user: {0} ({1})", user.DisplayName, user.UserId);

            if (_onEmailLoginResult != null)
                _onEmailLoginResult.Invoke(true);
        }


        );


        // // Gather data for the currently signed in User.
        //string currentUserId = auth.CurrentUser.UserId;
        //string currentEmail = auth.CurrentUser.Email;
        //string currentDisplayName = auth.CurrentUser.DisplayName;
        //System.Uri currentPhotoUrl = auth.CurrentUser.PhotoUrl;

        //// Sign in with the new credentials.
        //auth.SignInWithCredentialAsync(credential).ContinueWith(task => {
        //    if (task.IsCanceled)
        //    {
        //        Debug.LogError("SignInWithCredentialAsync was canceled.");
        //        return;
        //    }
        //    if (task.IsFaulted)
        //    {
        //        Debug.LogError("SignInWithCredentialAsync encountered an error: " + task.Exception);
        //        return;
        //    }

        //    Firebase.Auth.FirebaseUser newUser = task.Result;
        //    Debug.LogFormat("User signed in successfully: {0} ({1})",
        //        newUser.DisplayName, newUser.UserId);

        //    // TODO: Merge app specific details using the newUser and values from the
        //    // previous user, saved above.
        //});

    }


















    public void Logout()
    {
        auth.SignOut();
    }

    void OnDestroy()
    {
        auth.StateChanged -= AuthStateChanged;
        auth = null;
    }

    public UnityEvent OnSignedIn;
    public UnityEvent OnSignedOut;
}
