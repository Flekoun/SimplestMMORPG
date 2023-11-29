using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIExpBar : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    public UIProgressBar UIProgressBarExp;
    // Start is called before the first frame update
    void Awake()
    {
        AccountDataSO.OnCharacterDataChanged += Refresh;
    }

    // Update is called once per frame
    void Refresh()
    {
        UIProgressBarExp.SetValues(AccountDataSO.CharacterData.stats.expNeededToReachNextLevel - AccountDataSO.CharacterData.stats.expNeededToReachLastLevel, AccountDataSO.CharacterData.stats.exp - AccountDataSO.CharacterData.stats.expNeededToReachLastLevel);
    }
}
