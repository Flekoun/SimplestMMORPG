using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using simplestmmorpg.data;
using UnityEngine.Events;

public class UICharacterPreviewEntry : UISelectableEntry
{
    public FirebaseCloudFunctionSO FirebaseCloudFunctionSO;
    public TextMeshProUGUI CharacterNameText;
    public TextMeshProUGUI CharacterClassText;
    public TextMeshProUGUI CharacterLevelText;
    public TextMeshProUGUI CharacterSeasonText;
    public GameObject RetireButtonGO;
    public GameObject RetiredGO;

    public UIPortrait Portrait;

    public UnityAction<UICharacterPreviewEntry> OnClicked;
    public CharacterPreview Data;

    public override string GetUid()
    {
        return Data.characterUid;
    }

    public void SelectButtonClicked()
    {
        if (OnClicked != null)
            OnClicked.Invoke(this);
    }


    public void SetData(CharacterPreview _data)
    {

        Data = _data;

        CharacterNameText.SetText(_data.name);
        CharacterNameText.color = Utils.GetClassColor(Data.characterClass);
        CharacterClassText.SetText(_data.characterClass);
        CharacterLevelText.SetText("Level " + _data.level.ToString());
        CharacterSeasonText.SetText("Season " + _data.seasonNumber);
        Portrait.SetPortrait(Data.portrait, Data.characterClass);
        RetireButtonGO.SetActive(!_data.isRetired);
        RetiredGO.SetActive(_data.isRetired);

    }

    public void RetireCharacterClicked()
    {
        UIManager.instance.SpawnPromptPanel("Do you realy want to retire this character? It will no longer be playable. All artifacts from this character will be transfered to your player account.", async () =>

        {
            var result = await FirebaseCloudFunctionSO.retireCharacter(Data.characterUid);
            if (result.Result)
            {
                UIManager.instance.ImportantMessage.ShowMesssage("Character Retired!");
            }

        }, null);

    }
}
