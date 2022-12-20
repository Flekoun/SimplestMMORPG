using System.Collections;
using System.Collections.Generic;
using simplestmmorpg.data;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//using simplestmmorpg.data;
using UnityEngine.Events;

public class UIVendorGoodEntry : UISelectableEntry
{
 //   public AccountDataSO AccountDataSO;
 //   public FirebaseCloudFunctionSO FirebaseCloudFunctionSO;
    public TextMeshProUGUI SellPriceText;
    public TextMeshProUGUI DisplayNameText;
    public UIContentItem UIInventoryItem;

    public VendorGood Data;
    public UnityAction<UIVendorGoodEntry> OnClicked;
    public UnityAction<UIContentItem> OnPortraitClicked;

    public void Awake()
    {
        UIInventoryItem.OnClicked += OnItemPortraitClicked;
    }

    public void OnDestroy()
    {
        UIInventoryItem.OnClicked -= OnItemPortraitClicked;
    }

    private void OnItemPortraitClicked(UIContentItem _item)
    {
        if (OnPortraitClicked != null)
            OnPortraitClicked.Invoke(_item);
    }

    public void SetData(VendorGood _data)
    {
        
        Data = _data;
        SellPriceText.SetText(Data.sellPrice.ToString());
        DisplayNameText.SetText(Data.content.GetContent().GetDisplayName());
        UIInventoryItem.SetData(Data.content.GetContent());

    }

    public void Clicked()
    {
        OnClicked.Invoke(this);
    }

    public override string GetUid()
    {
        return Data.uid;
    }

  
}
