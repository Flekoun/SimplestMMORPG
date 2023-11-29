using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using static UnityEngine.EventSystems.EventTrigger;
using UnityEngine.UI;
using System;

public class UIInnPanel : MonoBehaviour
{
    public FirebaseCloudFunctionSO FirebaseCloudFunctionSO;
    public AccountDataSO AccountDataSO;
    public PrefabFactory PrefabFactory;
    public Button CarriageButton;
    public Button BindButton;
    public Button HealthRestoreButton;
    //public Button ScavengePointsRestoreButton;
    //public UIPriceTimeLabel ScavengePointsRestorePrice;

    public UIPriceLabel CarriagePrice;
    public UIPriceLabel BindPrice;
    public UIPriceLabel HealthRestorePrice;
    public TextMeshProUGUI BindButtonDescriptionText;
    public TextMeshProUGUI HealthRestButtonDescriptionText;
    public TextMeshProUGUI CarriageButtonDescriptionText;
   // public TextMeshProUGUI ScavengePointsDescriptionText;
    public GameObject Model;

    private UIPortrait lastlyClickedEntry;

    public void Show()
    {
        AccountDataSO.OnCharacterDataChanged += Refresh;

        Refresh();

        Model.gameObject.SetActive(true);
    }


    public void Hide()
    {
        AccountDataSO.OnCharacterDataChanged -= Refresh;
        Model.gameObject.SetActive(false);
    }

    private void Refresh()
    {
        bool isThisYourHomeInn = Utils.ArePositionsSame(AccountDataSO.CharacterData.position, AccountDataSO.CharacterData.homeInn);

        double result;
        if ((AccountDataSO.CharacterData.stats.level + 1) > 20)
            result = Math.Pow(Math.E, (30 * Math.Log(100000) / 20));
        else
            result = Math.Pow(Math.E, ((AccountDataSO.CharacterData.innHealhRestsCount + 1) * Math.Log(100000) / 20));

        int carriagePrice = (int)Math.Round(result);

        CarriageButton.interactable = AccountDataSO.CharacterData.currency.gold >= carriagePrice && !isThisYourHomeInn;
        CarriagePrice.SetPrice(carriagePrice);


        if (isThisYourHomeInn)
            CarriageButtonDescriptionText.SetText("This is your home tavern");
        else
            CarriageButtonDescriptionText.SetText("Fast travel to your home tavern for a fee");



        if ((AccountDataSO.CharacterData.innHealhRestsCount + 1) > 10)
            result = Math.Pow(Math.E, (30 * Math.Log(100000) / 10));
        else
            result = Math.Pow(Math.E, ((AccountDataSO.CharacterData.innHealhRestsCount + 1) * Math.Log(100000) / 10));

        int healthRestorePrice = (int)Math.Round(result);

        bool hasGold = AccountDataSO.CharacterData.currency.gold >= healthRestorePrice;
        bool hasWounds = AccountDataSO.CharacterData.stats.currentHealth < AccountDataSO.CharacterData.stats.totalMaxHealth - AccountDataSO.CharacterData.stats.healthBlockedByFatigue;

        HealthRestoreButton.interactable = hasGold && hasWounds;
        HealthRestorePrice.SetPrice(healthRestorePrice);

        if (!hasWounds)
            HealthRestButtonDescriptionText.SetText("You are at full health");
        else
            HealthRestButtonDescriptionText.SetText("Restores 20 Health");



        //int bidPrice = AccountDataSO.CharacterData.stats.level * 10;
        //BindButton.interactable = AccountDataSO.CharacterData.currency.gold > bidPrice;
        //  BindPrice.SetPrice(bidPrice);

        int bindPrice = (int)Math.Round(Math.Pow(Math.E, ((AccountDataSO.CharacterData.stats.level) * Math.Log(10000) / 20)));

        BindPrice.SetPrice(bindPrice);

        if (AccountDataSO.CharacterData.position.pointOfInterestId == AccountDataSO.CharacterData.homeInn.pointOfInterestId &&
            AccountDataSO.CharacterData.position.locationId == AccountDataSO.CharacterData.homeInn.locationId)
        {
            BindButtonDescriptionText.SetText("You are bound to this Tavern");
            BindButton.interactable = false;
        }
        else
        {
            BindButton.interactable = true;
            BindButtonDescriptionText.SetText("Fast travel & Revive at this Tavern");

        }


        //ScavengePointsRestoreButton.interactable = AccountDataSO.CharacterData.currency.time >= AccountDataSO.OtherMetadataData.constants.SCAVENGE_POINT_PURCHASE_COST;
        //ScavengePointsRestorePrice.SetPrice(AccountDataSO.OtherMetadataData.constants.SCAVENGE_POINT_PURCHASE_COST);
        //ScavengePointsDescriptionText.SetText("Gain <color=\"yellow\">" + AccountDataSO.OtherMetadataData.constants.SCAVENGE_POINT_PURCHASE_AMOUNT + "</color> Scavenge points");
    }

    public void BindClicked()
    {
        FirebaseCloudFunctionSO.InnBind();

    }

    //public async void ScavengePurchaseClicked()
    //{
    //    var restult = await FirebaseCloudFunctionSO.ScavengePointsPurchase();
    //    if (restult.Result)
    //    {
    //        UIManager.instance.ImportantMessage.ShowMesssage("You gained " + AccountDataSO.OtherMetadataData.constants.SCAVENGE_POINT_PURCHASE_AMOUNT + " Scavenge Points!");
    //    }
    //}

    public void CarrigeClicked()
    {
        FirebaseCloudFunctionSO.InnCarriage();
    }

    public void RestHealthClicked()
    {
        FirebaseCloudFunctionSO.InnHealthRestore();
    }


}
