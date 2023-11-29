using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using UnityEngine;
using TMPro;

public class NewDay : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    public FirebaseCloudFunctionSO FirebaseCloudFunctionSO;
    public TextMeshProUGUI NextDayTimerText;
    public TextMeshProUGUI GameDayText;
    public TextMeshProUGUI DaysLeftText;

    public GameObject Model;

    public void Awake()
    {
        AccountDataSO.OnCharacterDataChanged += Refresh;
        AccountDataSO.OnGlobalMetadataChanged += Refresh;
    }

    public void Refresh()
    {
        GameDayText.SetText("Day " + AccountDataSO.GlobalMetadata.gameDay);
        DaysLeftText.SetText("Ends in "+(AccountDataSO.GlobalMetadata.seasonDurationDays - AccountDataSO.GlobalMetadata.gameDay).ToString() + " days");

        Model.SetActive(false);
        //Model.SetActive(AccountDataSO.CharacterData.lastClaimedGameDay < AccountDataSO.GlobalMetadata.gameDay);
    }

    public void OnEnable()
    {
        InvokeRepeating("RefreshNextRestTime", 1, 1);
    }

    public void OnDisable()
    {
        CancelInvoke();
    }

    public void ClaimNewDay()
    {
      
        var window = UIManager.instance.SpawnPromptPanel("New day ready!\n" + "Your Time will be fully restored", "New Day", Claim, null);
        window.SetAcceptButtonText("Claim!");
        window.SetDeclineButtonText("Not now");

        //const int FATIGUE_RECOVERED_PER_HOUR = 10;
        //const int TIME_GAINED_PER_HOUR = 3;

        //double hoursPassedSinceLastClaim = Utils.SecondsToHours(Utils.GetTimePassedSinceDateInSeconds(AccountDataSO.CharacterData.timestamps.lastClaimTime));

        //int timeGained = Mathf.FloorToInt((float)hoursPassedSinceLastClaim) * TIME_GAINED_PER_HOUR;
        //int fatigueRecovered = Mathf.FloorToInt((float)hoursPassedSinceLastClaim) * FATIGUE_RECOVERED_PER_HOUR;

        //if (fatigueRecovered > AccountDataSO.CharacterData.currency.fatigue)
        //    fatigueRecovered = AccountDataSO.CharacterData.currency.fatigue;

        //if (AccountDataSO.CharacterData.currency.time + timeGained > AccountDataSO.CharacterData.currency.timeMax)
        //    timeGained = AccountDataSO.CharacterData.currency.timeMax - AccountDataSO.CharacterData.currency.time;


        ////console.log("You gained as much as: " + timeGained + " time");
        //// console.log("You recover as much as : " + fatigueRecovered + "% fatigue");

        //UIManager.instance.SpawnPromptPanel("Do you want to claim rested bonus?\n You will recover " + Utils.ColorizeGivenText(fatigueRecovered.ToString()+"%", Color.yellow) + " Fatigue and " + Utils.ColorizeGivenText(timeGained.ToString(), Color.yellow) + "</color> Travel time", "Claim rest bonus", ClaimPool, null);
    }

    private void RefreshNextRestTime()
    {
        NextDayTimerText.SetText( "Next day in "  + Utils.ConvertTimestampToReadableString(AccountDataSO.GlobalMetadata.nextGameDayTimestamp) );
       // yield return new WaitForSecondsRealtime(1);
    }

    private void Claim()
    {
        FirebaseCloudFunctionSO.ClaimNewDay();
    }

}
