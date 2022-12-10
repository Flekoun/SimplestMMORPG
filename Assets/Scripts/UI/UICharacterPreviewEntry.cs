using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using simplestmmorpg.data;
using UnityEngine.Events;

public class UICharacterPreviewEntry : UISelectableEntry
{
    public TextMeshProUGUI CharacterNameText;
    public TextMeshProUGUI CharacterClassText;
    public TextMeshProUGUI CharacterLevelText;
    public UIPortrait Portrait;

    public UnityAction<UICharacterPreviewEntry> OnClicked;
    public CharacterPreview Data;

    public override string GetUid()
    {
        return Data.uid;
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


        Portrait.SetPortrait(Data.portrait);


    }
}
