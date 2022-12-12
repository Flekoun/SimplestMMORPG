using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Auth;
using TMPro;
using UnityEngine;

public class UISettingsPanel : MonoBehaviour
{

    public GameObject Model;
    public TextMeshProUGUI emailText;
    public TextMeshProUGUI VersionText;
    public FirebaseAuthenticate FirebaseAuthenticate;
    public GameObject LinkGoogleButtonGO;

    public void Show()
    {
        Model.gameObject.SetActive(true);
        string email = FirebaseAuthenticate.GetUser().Email;
        Debug.Log("email: " + email);
        Debug.Log("UserId: " + FirebaseAuthenticate.GetUser().UserId);
        if (String.IsNullOrEmpty(email))
        {
            LinkGoogleButtonGO.SetActive(true);
            email = "You use Guest account. Please link Google account so you can retrieve it later.";
        }
        else
        {
            email = Utils.ColorizeGivenText("Successfuly linked to Gmail: ", Color.green) + email; 
            LinkGoogleButtonGO.SetActive(false);
        }

        emailText.SetText(email);

        VersionText.SetText("v"+Application.version);
    }


    // Update is called once per frame
    public void Hide()
    {
        Model.gameObject.SetActive(false);
    }



}
