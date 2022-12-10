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

    public InboxItem Data;


    public void Awake()
    {
        UIInboxItemsSpawner.OnUIEntryClicked += OnInboxEntryClicked;
        AccountDataSO.OnInboxDataChanged += Hide;
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
        UIContentItem.SetData(Data.content.GetContent());
        Model.SetActive(true);
    }


    public void Hide()
    {
        Model.SetActive(false);
    }


    public void ClaimButtonClicked()
    {
        FirebaseCloudFunctionSO.ClaimInboxItem(Data.uid);
    }
}
