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
    public UIInventory UIInventoyItemsToBuy;
    public UIInventory UIInventoyItemsToSell;
    public UIPriceLabel UIPriceLabelItemsToExchange;
    public UIVendorGoodsSpawner UIVendorGoodsSpawner;
    //  public UIContentContainerDetail UIItemDetail;
    public TextMeshProUGUI TitleText;
    public GameObject Model;
    //  public Button BuyButton;
    public Button TradeButton;
    public Vendor Data;

    private List<IContentDisplayable> ItemsToBuy;
    private List<IContentDisplayable> ItemsToSell;
    //  private List<string> equiToSellUids = new List<string>();
    // private List<string> equiToBuyUids = new List<string>();

    public void Awake()
    {
        UIInventoryPlayer.OnContentItemClicked += OnInventoryItemClickedPlayer;
        UIVendorGoodsSpawner.OnEntryClicked += OnVendorItemClicked;
        //    UIVendorGoodsSpawner.OnEntryPortraitClicked += OnVendorItemPortraitClicked;


        UIInventoyItemsToBuy.OnContentItemClicked += OnUIInventoryItemsToBuyItemClicked;
        UIInventoyItemsToSell.OnContentItemClicked += OnUIInventoryItemsToSellItemClicked;
    }


    public void Show(Vendor _data)
    {
        //        AccountDataSO.OnVendorsDataChanged += Refresh;
        AccountDataSO.OnCharacterDataChanged += Refresh;


        UIInventoryPlayer.ClearItemsSelected();
        // UIVendorGoodsSpawner.ClearItemsSelected();

        TradeButton.interactable = false;
        //SellButton.interactable = false;


        ItemsToBuy = new List<IContentDisplayable>();
        UIInventoyItemsToBuy.Refresh(ItemsToBuy);

        ItemsToSell = new List<IContentDisplayable>();
        UIInventoyItemsToSell.Refresh(ItemsToSell);

        UIPriceLabelItemsToExchange.SetPrice(0);


        Data = _data;
        Model.SetActive(true);
        Refresh();
    }

    public void Hide()
    {
        Model.SetActive(false);

        // AccountDataSO.OnVendorsDataChanged -= Refresh;
        AccountDataSO.OnCharacterDataChanged -= Refresh;

        foreach (var item in ItemsToSell)
            UIInventoryPlayer.AddItemOffline(item);

    }

    private void RefreshTradeButton()
    {
        TradeButton.interactable = UIInventoyItemsToSell.HasAnyItems() || UIInventoyItemsToBuy.HasAnyItems();
    }

    private void RefreshTradeBalance()
    {
        int totalTradeBalance = UIInventoyItemsToSell.GetValueOfAllItems() - UIInventoyItemsToBuy.GetValueOfAllItems();
        UIPriceLabelItemsToExchange.SetPrice(totalTradeBalance);

        if ((totalTradeBalance * -1) > AccountDataSO.CharacterData.currency.gold)
            UIPriceLabelItemsToExchange.SetColor(Color.red);
        else
            UIPriceLabelItemsToExchange.SetColor(Color.white);
    }


    public void Refresh()
    {
        UIInventoryPlayer.Refresh(new List<ContentContainer>(AccountDataSO.CharacterData.inventory.content));
        UIVendorGoodsSpawner.Refresh(Data);

        if (Utils.DescriptionsMetadata.DoesDescriptionMetadataForIdExist(Data.id))
            TitleText.SetText(Utils.DescriptionsMetadata.GetVendorsMetadata(Data.id).title.GetText());
        else
            TitleText.SetText(Data.id);

        RefreshTradeButton();
        RefreshTradeBalance();
        //        UIInventoryVendor.Refresh(Data);
    }

    public void OnUIInventoryItemsToBuyItemClicked(UIContentItem _item)
    {

        UIInventoyItemsToBuy.RemoveItemOffline(_item.Data, true);
        RefreshTradeButton();
        RefreshTradeBalance();
    }

    public void OnUIInventoryItemsToSellItemClicked(UIContentItem _item)
    {

        UIInventoyItemsToSell.RemoveItemOffline(_item.Data, true);
        UIInventoryPlayer.AddItemOffline(_item.Data, true);

        RefreshTradeButton();
        RefreshTradeBalance();
    }


    public void OnInventoryItemClickedPlayer(UIContentItem _item)
    {

        UIInventoyItemsToSell.AddItemOffline(_item.Data, true);
        UIInventoryPlayer.RemoveItemOffline(_item.Data, true);

        RefreshTradeButton();
        RefreshTradeBalance();
    }


    public void OnVendorItemClicked(UIVendorGoodEntry _item)
    {

        //UIItemDetail.Show(_item.UIInventoryItem.GetData());

        if (_item.Data.content != null)
        {
            //prepisu uid a cenu to co ma dany vendorgood, protoze vendor to prodava za sve ceny a ma vlastni uid
            _item.Data.content.content.uid = _item.Data.uid;
            _item.Data.content.content.sellPrice = _item.Data.sellPrice;

            UIInventoyItemsToBuy.AddItemOffline(_item.Data.content.GetContent());
        }
        else if (_item.Data.contentGenerated != null) //nemely by uz existovat
        {
            //tady musim doplnit UID a cenu z VendorGood, protoze ItemWithIdAndAmount je nema nastavene
            _item.Data.contentGenerated.uid = _item.Data.uid;
            _item.Data.contentGenerated.sellPrice = _item.Data.sellPrice;
            Debug.Log(" _item.Data.sellPrice: " + _item.Data.sellPrice);
            UIInventoyItemsToBuy.AddItemOffline(_item.Data.contentGenerated);
        }
        else if (_item.Data.contentRandomEquip != null)
        {
            //tady musim doplnit UID a cenu z VendorGood, protoze  je nema nastavene
            _item.Data.contentRandomEquip.uid = _item.Data.uid;
            _item.Data.contentRandomEquip.sellPrice = _item.Data.sellPrice;
            Debug.Log(" _item.Data.sellPrice: " + _item.Data.sellPrice);
            UIInventoyItemsToBuy.AddItemOffline(_item.Data.contentRandomEquip);
        }

        RefreshTradeButton();
        RefreshTradeBalance();
    }


    public void OnVendorItemPortraitClicked(UIContentItem _item)
    {
        //   UIManager.instance.ContextInfoPanel.ShowContentContainerDetail(_item.GetData());


        //if (_item.Data is ItemIdWithAmount)
        //{
        //    //tady musim doplnit UID a cenu z VendorGood, protoze ItemWithIdAndAmount je nema nastavene
        //    _item.Data.uid = _item.Data.uid;
        //    _item.Data.sellPrice = _item.Data.sellPrice;
        //    Debug.Log(" _item.Data.sellPrice: " + _item.Data.sellPrice);
        //    UIInventoyItemsToBuy.AddItemOffline(_item.Data);
        //}
        //else
        //{
        //    UIInventoyItemsToBuy.AddItemOffline(_item.Data);
        //}
        //RefreshTradeButton();
        //RefreshTradeBalance();
    }

    //public void Sell()
    //{

    //    FirebaseCloudFunctionSO.SellInventoryItems(UIInventoryPlayer.GetSelectedItemsUids());
    //    UIInventoryPlayer.ClearItemsSelected();

    //    // FirebaseCloudFunctionSO.SellInventoryItems(equiToSellUids);
    //    //   equiToSellUids.Clear();
    //}

    public async void Trade()
    {

        var result = await FirebaseCloudFunctionSO.TradeWithVendor(UIInventoyItemsToBuy.GetAllItemUids(), UIInventoyItemsToBuy.GetAllItemAmounts(), UIInventoyItemsToSell.GetAllItemUids(), UIInventoyItemsToSell.GetAllItemAmounts(), Data.id);

        if (result.Result)
        {
            UIManager.instance.ImportantMessage.ShowMesssage("Trade complete!");
            UIInventoyItemsToSell.RemoveAllItemsOffline();
            UIInventoyItemsToBuy.RemoveAllItemsOffline();
            UIPriceLabelItemsToExchange.SetPrice(0);

        }

        ///UIManager.instance.ImportantMessage.ShowMesssage("Trade complete!");

    }
}
