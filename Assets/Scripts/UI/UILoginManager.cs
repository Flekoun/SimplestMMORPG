using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class UILoginManager : MonoBehaviour
{
    public FirebaseGoogleLogin FirebaseGoogleLogin;
    public FirebaseAuthenticate FirebaseAuth;
    public TextMeshProUGUI ErrorMessageText;
    public TMP_InputField EmailInput;
    public TMP_InputField PassInput;

    public void LoginAsAnonymous()
    {
        FirebaseAuth.LoginAsAnonymous(result =>
        {
            if (!result)
                ErrorMessageText.SetText("Cant login as anonymous");
            else
            {
                OnUserLoggedInAsAnonymous.Invoke();
                //   UIManager.instance.ShowMainScreen();
            }
        });
    }

    public void LoginAsGooglePlay()
    {
        FirebaseAuth.LoginAsPlayGames( result =>
        {
            if (!result)
                ErrorMessageText.SetText("Cant login with google play");
            else
            {
                OnUserLoggedWithPass.Invoke();
                //   UIManager.instance.ShowMainScreen();
            }
        });

    }

    public void LoginAsGoogle()
    {
        FirebaseGoogleLogin.GoogleSignInClick(result =>
        {
            if (!result)
                ErrorMessageText.SetText("Cant login with google");
            else
            {
                OnUserLoggedInAsGoogle.Invoke();
                //   UIManager.instance.ShowMainScreen();
            }
        });


    }

    public void LoginWithPassAndMail()
    {

        FirebaseAuth.LoginWithPassword(EmailInput.text, PassInput.text, result =>
        {
            if (!result)
                ErrorMessageText.SetText("Cant login as with pass");
            else
            {
                OnUserLoggedWithPass.Invoke();
                //   UIManager.instance.ShowMainScreen();
            }
        });
    }


    public void LinkToMail()
    {

        FirebaseAuth.Link(EmailInput.text, PassInput.text, result =>
        {
            if (!result)
                ErrorMessageText.SetText("Cant link as with pass");
            else
            {
                OnUserMailLinked.Invoke();
                // UIManager.instance.ShowMainScreen();
            }
        });
    }

    public UnityEvent OnUserLoggedInAsAnonymous;
    public UnityEvent OnUserLoggedInAsGooglePlay;
    public UnityEvent OnUserLoggedInAsGoogle;
    public UnityEvent OnUserLoggedWithPass;
    public UnityEvent OnUserMailLinked;



}
