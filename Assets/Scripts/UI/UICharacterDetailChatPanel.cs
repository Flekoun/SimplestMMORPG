using System.Collections;
using System.Collections.Generic;
using simplestmmorpg.realtimeDatabaseData;
using TMPro;
using UnityEngine;

public class UICharacterDetailChatPanel : MonoBehaviour
{
    public FirebaseCloudFunctionSO FirebaseCloudFunctionSO;
    public TextMeshProUGUI CharacterNameText;
    public TextMeshProUGUI CharacterLevelAndClassText;
    public UIChatEntry Data;
    public UIChatMessageSpawner UIChatMessageSpawner;
    public UICharacterEquipPanel UICharacterEquipPanel;
    public QueryData QueryData;
    public GameObject Model;

    public void Awake()
    {
        UIChatMessageSpawner.OnChatEntryClicked += Show;
    }

    public void Show(UIChatEntry _data)
    {
        Model.SetActive(true);
        Data = _data;
        CharacterNameText.SetText(Data.Data.characterName);
        CharacterLevelAndClassText.SetText("Level " + Data.Data.characterLevel);
    }

    public async void InviteToParty()
    {
        var result = await FirebaseCloudFunctionSO.SendPartyInvite(Data.Data.characterUid);
        if (result.Result)
        {
            UIManager.instance.ImportantMessage.ShowMesssage("Invite sent to " + Data.Data.characterName);
            Model.SetActive(false);
        }
    }



    public async void CharacterInfo()
    {
        UICharacterEquipPanel.Show(await QueryData.GetCharacterData(Data.Data.characterUid));
    }

    public void Hide()
    {
        Model.SetActive(false);
    }
}
