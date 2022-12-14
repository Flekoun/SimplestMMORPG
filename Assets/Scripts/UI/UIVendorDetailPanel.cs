using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using simplestmmorpg.data;


public class UIVendorDetailPanel : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    public FirebaseCloudFunctionSO FirebaseCloudFunctionSO;
    public UIInventory UIInventoryPlayer;
    public UIVendorGoodsSpawner UIVendorGoodsSpawner;
    public UIContentContainerDetail UIItemDetail;
    public TextMeshProUGUI TitleText;
    public GameObject Model;
    public Button BuyButton;
    public Button SellButton;
    public Vendor Data;

    //  private List<string> equiToSellUids = new List<string>();
    // private List<string> equiToBuyUids = new List<string>();

    public void Awake()
    {
        UIInventoryPlayer.OnContentItemClicked += OnInventoryItemClickedPlayer;
        UIVendorGoodsSpawner.OnEntryClicked += OnVendorItemClicked;
        UIVendorGoodsSpawner.OnEntryPortraitClicked += OnVendorItemPortraitClicked;
    }


    public void Show(Vendor _data)
    {
        AccountDataSO.OnVendorsDataChanged += Refresh;
        AccountDataSO.OnCharacterDataChanged += Refresh;


        UIInventoryPlayer.ClearItemsSelected();
        UIVendorGoodsSpawner.ClearItemsSelected();

        BuyButton.interactable = false;
        SellButton.interactable = false;

        Data = _data;
        Model.SetActive(true);
        Refresh();
    }

    public void Hide()
    {
        Model.SetActive(false);

        AccountDataSO.OnVendorsDataChanged -= Refresh;
        AccountDataSO.OnCharacterDataChanged -= Refresh;

    }


    public void Refresh()
    {
        UIInventoryPlayer.Refresh(AccountDataSO.CharacterData.inventory.content);
        UIVendorGoodsSpawner.Refresh(Data);
        TitleText.SetText(Data.displayName);
        //        UIInventoryVendor.Refresh(Data);
    }

    public void OnInventoryItemClickedPlayer(UIContentItem _item)
    {
        SellButton.interactable = UIInventoryPlayer.IsAnyItemSelected();
    }


    public void OnVendorItemClicked(UIVendorGoodEntry _item)
    {
        BuyButton.interactable = UIVendorGoodsSpawner.IsAnyItemSelected();
        //  UIItemDetail.Show(_item.Data.content.GetContent());

    }


    public void OnVendorItemPortraitClicked(UIContentItem _item)
    {
        UIItemDetail.Show(_item.GetData());

    }


    public void Sell()
    {

        FirebaseCloudFunctionSO.SellInventoryItems(UIInventoryPlayer.GetSelectedItemsUids());
        UIInventoryPlayer.ClearItemsSelected();

        // FirebaseCloudFunctionSO.SellInventoryItems(equiToSellUids);
        //   equiToSellUids.Clear();
    }

    public void Buy()
    {
        FirebaseCloudFunctionSO.BuyVendorItems(UIVendorGoodsSpawner.GetSelectedItemsUids(), Data.id);
        UIVendorGoodsSpawner.ClearItemsSelected();
    }
}
