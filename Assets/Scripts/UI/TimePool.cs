using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using UnityEngine;

public class TimePool : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    public FirebaseCloudFunctionSO FirebaseCloudFunctionSO;
    public GameObject Model;
    public void Awake()
    {
        AccountDataSO.OnCharacterDataChanged += OnCharacterDataChanged;
    }

    public void OnCharacterDataChanged()
    {

        double hoursPassedSinceLastClaim = Utils.SecondsToHours(Utils.GetTimePassedSinceDateInSeconds(AccountDataSO.CharacterData.timestamps.lastClaimTime));
        //Debug.Log("hoursPassedSinceLastClaim: " + hoursPassedSinceLastClaim);
        //Debug.Log("minutes: " + Utils.SecondsToMinutes(Utils.GetTimePassedSinceDateInSeconds(AccountDataSO.CharacterData.timestamps.lastClaimTime)));
        Model.SetActive(hoursPassedSinceLastClaim > 1);
    }

    public void ClaimTimePool()
    {
        const int FATIGUE_RECOVERED_PER_HOUR = 10;
        const int TIME_GAINED_PER_HOUR = 3;
     //   const int MAX_FATIGUE = 90;
        const int MAX_TRAVEL_TIME = 48;

        double hoursPassedSinceLastClaim = Utils.SecondsToHours(Utils.GetTimePassedSinceDateInSeconds(AccountDataSO.CharacterData.timestamps.lastClaimTime));

        int timeGained = Mathf.FloorToInt((float)hoursPassedSinceLastClaim) * TIME_GAINED_PER_HOUR;
        int fatigueRecovered = Mathf.FloorToInt((float)hoursPassedSinceLastClaim) * FATIGUE_RECOVERED_PER_HOUR;

        if (fatigueRecovered > AccountDataSO.CharacterData.currency.fatigue)
            fatigueRecovered = AccountDataSO.CharacterData.currency.fatigue;

        if (AccountDataSO.CharacterData.currency.time + timeGained > MAX_TRAVEL_TIME)
            timeGained = MAX_TRAVEL_TIME - AccountDataSO.CharacterData.currency.time;


        //console.log("You gained as much as: " + timeGained + " time");
        // console.log("You recover as much as : " + fatigueRecovered + "% fatigue");

        UIManager.instance.SpawnPromptPanel("Do you want to claim rested bonus?\n You will recover " + Utils.ColorizeGivenText(fatigueRecovered.ToString()+"%", Color.yellow) + " Fatigue and " + Utils.ColorizeGivenText(timeGained.ToString(), Color.yellow) + "</color> Travel time", "Claim rest bonus", ClaimPool, null);
    }


    private void ClaimPool()
    {
        FirebaseCloudFunctionSO.ClaimTimePool();

    }
}
