using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class UICharacterHealthProgress : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    public UIProgressBar UIHealthProgress;

    public void Awake()
    {

        AccountDataSO.OnCharacterDataChanged += Refresh;
    }
   
    public void Refresh()
    {
//        Debug.Log("Health progress zaregistroval zmenu character data :" + this.name);
        UIHealthProgress.SetValues(AccountDataSO.CharacterData.GetTotalHealth(true) - AccountDataSO.CharacterData.GetHealthTakenByFatiguePenalty(), AccountDataSO.CharacterData.stats.currentHealth, AccountDataSO.CharacterData.GetHealthTakenByFatiguePenalty());
    }

}
