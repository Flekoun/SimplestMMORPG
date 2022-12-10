using System.Collections;
using System.Collections.Generic;
using simplestmmorpg.data;
using UnityEngine;

public class CharacterLevelUpListener : MonoBehaviour
{

    public AccountDataSO AccountDataSO;


    // Start is called before the first frame update
    void Awake()
    {
        // AccountDataSO.OnPlayerDataLoadedFirstTime += OnPlayerLoadedFirstTimeChanged;
        AccountDataSO.OnCharacterDataChanged_OldData += OnCharacterDataChanged_OldData;
    }



    // Update is called once per frame
    void OnCharacterDataChanged_OldData(CharacterData _oldData)
    {
        if (_oldData == null) return;

        if (_oldData.stats.level < AccountDataSO.CharacterData.stats.level)
        {
            var window = UIManager.instance.SpawnPromptPanel("Level " + AccountDataSO.CharacterData.stats.level + " reached! \n Fatigue restored!", null, null);
            window.HideDeclineButton();
            window.SetAcceptButtonText("Great!");

        }

    }
}
