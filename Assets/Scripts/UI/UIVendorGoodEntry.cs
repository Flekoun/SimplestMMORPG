using System.Collections;
using System.Collections.Generic;
using simplestmmorpg.data;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//using simplestmmorpg.data;
using UnityEngine.Events;

public class UIVendorGoodEntry : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    //   public FirebaseCloudFunctionSO FirebaseCloudFunctionSO;
    // public TextMeshProUGUI SellPriceText;
    public TextMeshProUGUI DisplayNameText;
    public UIContentItem UIInventoryItem;
    public UIPriceLabel UIPriceLabel;
    public TextMeshProUGUI TotalStockLeftText;
    public TextMeshProUGUI MyStockLeftText;
    public GameObject SoldOutGO;
    public Button Button;
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

    public void SetData(VendorGood _data, string _vendorId)
    {
        //   tady musim zohlednit ze vendor good muze byt jak content ktery je jasne dany anebo generated content ( ItemDropDefinition)
        Data = _data;

        UIPriceLabel.SetPrice(Data.sellPrice);
        if (Data.content != null)
        {
            DisplayNameText.SetText(Utils.ReplacePlaceholdersInTextWithDescriptionFromMetadata(Data.content.GetContent().GetDisplayName()));
            UIInventoryItem.SetData(Data.content.GetContent(), true);
        }
        else if (Data.contentGenerated != null)
        {
            DisplayNameText.SetText(Utils.ReplacePlaceholdersInTextWithDescriptionFromMetadata(Data.contentGenerated.GetDisplayName()));
            UIInventoryItem.SetData(Data.contentGenerated, true);
        }
        else if (Data.contentRandomEquip != null)
        {
            DisplayNameText.SetText(Utils.ReplacePlaceholdersInTextWithDescriptionFromMetadata(Data.contentRandomEquip.GetDisplayName()));
            UIInventoryItem.SetData(Data.contentRandomEquip, true);
        }

        int myStockLeft = (_data.stockPerCharacter - AccountDataSO.CharacterData.GetVendorGoodsPurchased(_vendorId, _data.uid));


        MyStockLeftText.gameObject.SetActive(true);
        if (_data.stockTotalLeft == -1)
        {
            TotalStockLeftText.gameObject.SetActive(false);
            TotalStockLeftText.SetText("(\u221E)");
        }
        else
        {
            TotalStockLeftText.gameObject.SetActive(true);
            TotalStockLeftText.SetText(_data.stockTotalLeft + " stock");
        }

        if (_data.stockPerCharacter == -1)
            // MyStockLeftText.gameObject.SetActive(false);
            MyStockLeftText.SetText("(\u221E)");
        else
        {
            // MyStockLeftText.gameObject.SetActive(true);
            MyStockLeftText.SetText(myStockLeft.ToString());
        }
        SoldOutGO.SetActive(myStockLeft == 0);
        Button.interactable = (myStockLeft > 0 || _data.stockPerCharacter == -1);

        if (myStockLeft == 0)
            MyStockLeftText.color = Color.red;
        else
            MyStockLeftText.color = Color.white;
        //if (myStockLeft == 0)
        //    this.gameObject.SetActive(false);


    }

    public void Clicked()
    {
        OnClicked.Invoke(this);
    }

    //public override string GetUid()
    //{
    //    return Data.uid;
    //}


}
