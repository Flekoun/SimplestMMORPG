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

    public GameObject LinkGoogleButtonGO;

    public void Show()
    {
        Model.gameObject.SetActive(true);
        string email = FirebaseAuth.DefaultInstance.CurrentUser.Email;
        Debug.Log("email: " + email);
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

    }


    // Update is called once per frame
    public void Hide()
    {
        Model.gameObject.SetActive(false);
    }



}
