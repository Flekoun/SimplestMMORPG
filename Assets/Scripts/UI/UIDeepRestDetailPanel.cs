using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using simplestmmorpg.data;
using DG.Tweening;


public class UIDeepRestDetailPanel : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    public FirebaseCloudFunctionSO FirebaseCloudFunctionSO;
    public UIInventory UIInventoryPlayer;
    public UIInventory UIInventoyFoodSuppliesToUse;
    public TextMeshProUGUI TotalSuppliesText;
    public UIPriceTimeLabel RestTimePriceLabel;
    public TextMeshProUGUI FoodEffectDesription;
    public GameObject Model;

    //  public Button BuyButton;
    public Button RestButton;

    private List<IContentDisplayable> ItemsToBuy;
    private List<IContentDisplayable> ItemsToSell;

    private int TotalSuppliesUsed = 0;
    private int SupplyLimit = 0;
    //  private List<string> equiToSellUids = new List<string>();
    // private List<string> equiToBuyUids = new List<string>();

    public void Awake()
    {
        UIInventoryPlayer.OnContentItemClicked += OnInventoryItemClickedPlayer;

        UIInventoyFoodSuppliesToUse.OnContentItemClicked += OnUIInventoryItemsToSellItemClicked;
    }


    public void Show()
    {
        SupplyLimit = AccountDataSO.CharacterData.stats.restFoodLimit + ((AccountDataSO.CharacterData.stats.level - 1) * AccountDataSO.OtherMetadataData.constants.restSupplyLimitIncrement);

        //        AccountDataSO.OnVendorsDataChanged += Refresh;
        AccountDataSO.OnCharacterDataChanged += Refresh;


        UIInventoryPlayer.ClearItemsSelected();
        // UIVendorGoodsSpawner.ClearItemsSelected();

        RestButton.interactable = false;
        //SellButton.interactable = false;

        ItemsToSell = new List<IContentDisplayable>();
        UIInventoyFoodSuppliesToUse.Refresh(ItemsToSell);

        TotalSuppliesText.SetText("0");

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

    //private void RefreshTradeButton()
    //{

    //}

    private void RefreshTotalSuppliesText()
    {
        TotalSuppliesUsed = UIInventoyFoodSuppliesToUse.GetFoodSupplyValueOfAllItems();

        // var supplyLimit = AccountDataSO.OtherMetadataData.constants.restSupplyLimitBase + ((AccountDataSO.CharacterData.stats.level - 1) * AccountDataSO.OtherMetadataData.constants.restSupplyLimitIncrement);

        TotalSuppliesText.SetText(TotalSuppliesUsed.ToString() + "/" + SupplyLimit);

        if (TotalSuppliesUsed > SupplyLimit)
            TotalSuppliesText.color = Color.red;
        else
            TotalSuppliesText.color = Color.green;

        FoodEffectDesription.SetText(GetFoodEffectDescription());



    }

    private string GetFoodEffectDescription()
    {
        string foodEffects = "";
        //  foodEffects += "+6 Travel points\n";

        foreach (var item in UIInventoyFoodSuppliesToUse.GetDescriptionsOfAllItems())
        {
            foodEffects += item + "\n";
        }

        return foodEffects;
    }

    public void Refresh()
    {




        UIInventoryPlayer.Refresh(new List<ContentContainer>(AccountDataSO.CharacterData.inventory.content));

        RestButton.interactable = AccountDataSO.CharacterData.currency.time >= AccountDataSO.OtherMetadataData.constants.deepRestTimeCost && TotalSuppliesUsed <= SupplyLimit;

        RestTimePriceLabel.SetPrice(AccountDataSO.OtherMetadataData.constants.deepRestTimeCost);

        RefreshTotalSuppliesText();
        //        UIInventoryVendor.Refresh(Data);
    }


    public void OnUIInventoryItemsToSellItemClicked(UIContentItem _item)
    {

        UIInventoyFoodSuppliesToUse.RemoveItemOffline(_item.Data, true);
        UIInventoryPlayer.AddItemOffline(_item.Data, true);

        RefreshTotalSuppliesText();
    }


    public void OnInventoryItemClickedPlayer(UIContentItem _item)
    {

        UIInventoyFoodSuppliesToUse.AddItemOffline(_item.Data, true);
        UIInventoryPlayer.RemoveItemOffline(_item.Data, true);

        RefreshTotalSuppliesText();
    }





    public async void RestClicked()
    {
        if (UIInventoyFoodSuppliesToUse.GetAllItemUids().Count == 0)
        {
            UIManager.instance.ImportantMessage.ShowMesssage("To rest is to refuel. No journey resumes on an empty stomach", 3);
        }
        else
        {
            var result = await FirebaseCloudFunctionSO.RestDeep(UIInventoyFoodSuppliesToUse.GetAllItemUids(), UIInventoyFoodSuppliesToUse.GetAllItemAmounts());

            if (result.Result)
            {
                UIManager.instance.ImportantMessage.ShowMesssage("Rested!");
                UIInventoyFoodSuppliesToUse.RemoveAllItemsOffline();

                Hide();

            }
        }
        ///UIManager.instance.ImportantMessage.ShowMesssage("Trade complete!");

    }
}
