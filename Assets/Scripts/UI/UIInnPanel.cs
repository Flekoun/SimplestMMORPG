using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using static UnityEngine.EventSystems.EventTrigger;
using UnityEngine.UI;
using System;
using UnityEngine.Events;

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
        //  bool isThisYourHomeInn = Utils.ArePositionsSame(AccountDataSO.CharacterData.position, AccountDataSO.CharacterData.homeInn);

        double result;
        //if ((AccountDataSO.CharacterData.stats.level + 1) > 20)
        //    result = Math.Pow(Math.E, (20 * Math.Log(100000) / 15));
        //else
        //    result = Math.Pow(Math.E, ((AccountDataSO.CharacterData.innHealhRestsCount + 1) * Math.Log(100000) / 15));

        result = Math.Pow(AccountDataSO.CharacterData.stats.level, 4);

        int carriagePrice = (int)Math.Round(result);

        CarriageButton.interactable = AccountDataSO.CharacterData.currency.gold >= carriagePrice;
        CarriagePrice.SetPrice(carriagePrice);


        //  if (isThisYourHomeInn)
        //      CarriageButtonDescriptionText.SetText("This is your home tavern");
        //   else
        CarriageButtonDescriptionText.SetText("Fast travel to other tavern for a fee");



        //if ((AccountDataSO.CharacterData.innHealhRestsCount + 1) > 10)
        //    result = Math.Pow(Math.E, (30 * Math.Log(100000) / 10));
        //else
        //    result = Math.Pow(Math.E, ((AccountDataSO.CharacterData.innHealhRestsCount + 1) * Math.Log(100000) / 10));

        //int healthRestorePrice = (int)Math.Round(result);

        //bool hasGold = AccountDataSO.CharacterData.currency.gold >= healthRestorePrice;
        bool hasWounds = AccountDataSO.CharacterData.stats.currentHealth < AccountDataSO.CharacterData.stats.totalMaxHealth - AccountDataSO.CharacterData.stats.healthBlockedByFatigue;
        bool hasFatigue = AccountDataSO.CharacterData.stats.healthBlockedByFatigue > 0;

        //HealthRestoreButton.interactable = hasGold && hasWounds;
        //HealthRestorePrice.SetPrice(healthRestorePrice);

        HealthRestoreButton.interactable = hasWounds || hasFatigue;

        if (!hasWounds && !hasFatigue)
            HealthRestButtonDescriptionText.SetText("You feel fit!");
        else
            HealthRestButtonDescriptionText.SetText("Replenish your body to its prime, but at the cost of your soul's silent torment.");



        //int bidPrice = AccountDataSO.CharacterData.stats.level * 10;
        //BindButton.interactable = AccountDataSO.CharacterData.currency.gold > bidPrice;
        //  BindPrice.SetPrice(bidPrice);

        int bindPrice = (int)Math.Round(Math.Pow(AccountDataSO.CharacterData.stats.level, 3));//(int)Math.Round(Math.Pow(Math.E, ((AccountDataSO.CharacterData.stats.level) * Math.Log(10000) / 20)));

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
            BindButtonDescriptionText.SetText("Forge a bond with this place, ensuring safe return in times of peril.");

        }


        //ScavengePointsRestoreButton.interactable = AccountDataSO.CharacterData.currency.time >= AccountDataSO.OtherMetadataData.constants.SCAVENGE_POINT_PURCHASE_COST;
        //ScavengePointsRestorePrice.SetPrice(AccountDataSO.OtherMetadataData.constants.SCAVENGE_POINT_PURCHASE_COST);
        //ScavengePointsDescriptionText.SetText("Gain <color=\"yellow\">" + AccountDataSO.OtherMetadataData.constants.SCAVENGE_POINT_PURCHASE_AMOUNT + "</color> Scavenge points");
    }

    public async void BindClicked()
    {
        var result = await FirebaseCloudFunctionSO.InnBind();
        if (result.Result)
        {
            UIManager.instance.ImportantMessage.ShowMesssage("Enjoy your stay, traveler!", 3);
        }
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
        UIManager.instance.ShowPoIChooser(OnPoIChooserFinished, "Choose destination tavern");
        Hide();
    }

    public void RestHealthClicked()
    {
        FirebaseCloudFunctionSO.InnHealthRestore();
    }

    public async void OnPoIChooserFinished(string _poIdId)
    {
        var result = await FirebaseCloudFunctionSO.InnCarriage(_poIdId);
        if (result.Result)
        {
            UIManager.instance.ImportantMessage.ShowMesssage("Carriage arrived!", 3);
        }
    }

}
