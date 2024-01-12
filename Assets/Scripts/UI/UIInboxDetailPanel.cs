using System.Collections;
using System.Collections.Generic;
using simplestmmorpg.data;
using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class UIInboxDetailPanel : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    public FirebaseCloudFunctionSO FirebaseCloudFunctionSO;
    public GameObject Model;
    public TextMeshProUGUI TitleText;
    public TextMeshProUGUI BodyText;
    public UIContentItem UIContentItem;
    public UIInboxItemsSpawner UIInboxItemsSpawner;
    public GameObject PerkOfferIndicatorGO;
    public InboxItem Data;


    public void Awake()
    {
        UIInboxItemsSpawner.OnUIEntryClicked += OnInboxEntryClicked;
        AccountDataSO.OnInboxDataCharacterChanged += Hide;
    }

    private void OnInboxEntryClicked(UIInboxItemEntry _entry)
    {
        Show(_entry.Data);
    }

    public void Show(InboxItem _data)
    {
        Data = _data;
        TitleText.SetText(Data.messageTitle);
        BodyText.SetText(Data.messageBody);
        PerkOfferIndicatorGO.SetActive(false);
        if (Data.content != null)
            UIContentItem.SetData(Data.content.GetContent());
        else if (Data.perkOffer != null)
        {
            UIContentItem.SetData(Data.perkOffer.rewards[0].GetContent());
            PerkOfferIndicatorGO.SetActive(true);
        }
        Model.SetActive(true);
    }


    public void Hide()
    {
        Model.SetActive(false);
    }


    public async void ClaimButtonClicked()
    {

        if (AccountDataSO.CharacterData != null && Data.recipientUid == AccountDataSO.CharacterData.uid)
        {
            var result = await FirebaseCloudFunctionSO.ClaimInboxItem(Data.uid);
            if (result.Result)
            {
                UIManager.instance.ImportantMessage.ShowMesssage("Inbox item claimed!");
                Hide();
            }
        }
        else if (AccountDataSO.PlayerData != null && Data.recipientUid == AccountDataSO.PlayerData.uid)
        {
            var result = await FirebaseCloudFunctionSO.ClaimPlayerInboxItem(Data.uid);
            if (result.Result)
            {
                UIManager.instance.ImportantMessage.ShowMesssage("Inbox item claimed!");
                Hide();
            }
        }


    }
}
