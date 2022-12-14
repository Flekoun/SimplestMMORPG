using System.Collections;
using System.Collections.Generic;
using simplestmmorpg.realtimeDatabaseData;
using TMPro;
using UnityEngine;

public class UICharacterDetailChatPanel : MonoBehaviour
{
    public FirebaseCloudFunctionSO FirebaseCloudFunctionSO;
    public TextMeshProUGUI CharacterNameText;
    public UIChatEntry Data;
    public UIChatMessageSpawner UIChatMessageSpawner;

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
    }

    public void InviteToParty()
    {
        FirebaseCloudFunctionSO.SendPartyInvite(Data.Data.characterUid);
        UIManager.instance.ImportantMessage.ShowMesssage("Invite sent to " + Data.Data.characterName);
        Model.SetActive(false);
    }

    public void Hide()
    {
        Model.SetActive(false);
    }
}
