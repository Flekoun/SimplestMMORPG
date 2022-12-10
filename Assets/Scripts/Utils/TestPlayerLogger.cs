using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPlayerLogger : MonoBehaviour
{
    public List<string> Player_Email;
    public List<string> Player_Password;

    private int playerNumber;
    public FirebaseAuthenticate FirebaseAuthenticate;
    // Start is called before the first frame update
    public void LoginPlayer(int _number)
    {
        playerNumber = _number;
        FirebaseAuthenticate.OnSignedOutEvent += PlayerSignedOut;
        FirebaseAuthenticate.Logout();
    }

    public void PlayerSignedOut()
    {
        FirebaseAuthenticate.OnSignedOutEvent -= PlayerSignedOut;
        FirebaseAuthenticate.LoginWithPassword(Player_Email[playerNumber], Player_Password[playerNumber],null);
    }

    // Update is called once per frame
    public void LogoutTest()
    {
        FirebaseAuthenticate.Logout();
        Application.Quit();
    }

    public void Awake()
    {

    }
}
