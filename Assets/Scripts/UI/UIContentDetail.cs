using System.Collections;
using System.Collections.Generic;
using simplestmmorpg.data;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

public class UIContentDetail : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    public FirebaseCloudFunctionSO FirebaseCloudFunctionSO;
    public ImageIdDefinitionSOSet AllImageIdDefinitionSOSet;
    public TextMeshProUGUI DisplayNameText;
    public TextMeshProUGUI RarityText;
    public TextMeshProUGUI LevelText;
    public TextMeshProUGUI SatoshiumFiatValueText;
    public UIPriceLabel UIPriceLabel;
    public TextMeshProUGUI DescriptionText;
    public GameObject DescriptionGO;
    public GameObject SellPriceGO;
    public GameObject SatoshiumFiatValueGO;

    public Image PortraitImage;
    public GameObject SupplyCostGO;
    public TextMeshProUGUI SupplyCostText;
    public TextMeshProUGUI ExpireDayText;

    public GameObject ConsumeButtonGO;
    public TextMeshProUGUI ConsumeText;
    public UIPriceTimeLabel ConsumePrice;
    public UnityAction OnHideClicked;
    public UnityAction OnConsumeClicked;
    private IContentDisplayable Data;

    public void Show(IContentDisplayable _data)
    {
        Data = _data;
        DisplayNameText.SetText(Utils.ReplacePlaceholdersInTextWithDescriptionFromMetadata(_data.GetDisplayName()));
        Utils.ShowAllChildren(this.gameObject.transform, true);

        ConsumeButtonGO.gameObject.SetActive(false);

        SatoshiumFiatValueGO.SetActive(false);



        if (Data.contentType == Utils.CONTENT_TYPE.FOOD || Data.contentType == Utils.CONTENT_TYPE.RECIPE || Data.contentType == Utils.CONTENT_TYPE.CHEST)
        {
            Debug.Log("Data.contentType:" + Data.contentType);
            ConsumeButtonGO.gameObject.SetActive(true);

            int cost = 0;
            if (Data.customData?.integers?.Count >= 2)
            {
                cost = Mathf.RoundToInt(Data.customData.integers[1]);
            }

            if (Data.contentType == Utils.CONTENT_TYPE.FOOD)
                SetConsumeButtonText("Consume", cost);
            else if (Data.contentType == Utils.CONTENT_TYPE.RECIPE)
                SetConsumeButtonText("Learn", cost);
            else if (Data.contentType == Utils.CONTENT_TYPE.CHEST)
                SetConsumeButtonText("Open", cost);
        }
        else
            Debug.Log("Data.contentType 2:" + Data.contentType);
        //else
        //{
        //    ConsumeButtonGO.gameObject.SetActive(false);
        //}

        SellPriceGO.SetActive(_data.sellPrice > 0);
        UIPriceLabel.SetPrice((_data.sellPrice));

        DescriptionGO.SetActive(!string.IsNullOrWhiteSpace(_data.GetDescription()));
        DescriptionText.SetText(Utils.ReplacePlaceholdersInTextWithDescriptionFromMetadata(_data.GetDescription()));
        //   LevelText.SetText("Requires level " + Data.level.ToString());
        RarityText.gameObject.SetActive(Data.contentType != Utils.CONTENT_TYPE.GENERATED);
        RarityText.SetText(Utils.DescriptionsMetadata.GetRarities(Data.rarity).title.EN);
        RarityText.color = Utils.GetRarityColor(Data.rarity);
        PortraitImage.sprite = AllImageIdDefinitionSOSet.GetDefinitionById(Data.GetImageId()).Image;


        ExpireDayText.gameObject.SetActive(!string.IsNullOrEmpty(Data.expireDate));

        if (!string.IsNullOrEmpty(Data.expireDate))
        {
            ExpireDayText.SetText("Expires in " + Utils.ConvertTimestampToTimeLeft(Data.expireDate));

            if (Utils.GetTimeLeftToDateInSeconds(Data.expireDate) <= 0)
                ExpireDayText.SetText(Utils.ColorizeGivenText("Expired", Color.red));
        }
        if (Data.contentType == Utils.CONTENT_TYPE.FOOD_SUPPLY)
        {
            SupplyCostGO.SetActive(true);
            SupplyCostText.SetText(Data.customData.integers[0].ToString());
            DescriptionText.SetText("Rest effect: " + DescriptionText.text);

        }
        else
            SupplyCostGO.SetActive(false);

        if (Data.itemId == "SATOSHIUM")
        {
            SatoshiumFiatValueGO.SetActive(true);
            SellPriceGO.SetActive(false);
            SatoshiumFiatValueText.SetText(AccountDataSO.GlobalMetadata.GetPriceOfSatoshiumInFiat(Data.amount).ToString());
        }

    }

    public void Hide()
    {
        Utils.ShowAllChildren(this.gameObject.transform, false);
    }

    public void SetConsumeButtonText(string _title, int _timePrice)
    {
        ConsumePrice.gameObject.SetActive(_timePrice > 0);
        ConsumePrice.SetPrice(_timePrice);
        ConsumeText.SetText(_title);
    }

    public void HideButtonClicked()
    {
        if (OnHideClicked != null)
            OnHideClicked.Invoke();

        Hide();
    }

    public async void PoIForTeleportScrollChoosen(string _poiId)
    {
        var result = await FirebaseCloudFunctionSO.UseTeleportScroll(_poiId, Data.uid);

        if (result.Result)
        {
            UIManager.instance.ImportantMessage.ShowMesssage("Teleport successful!");

            //if (Data.amount == 1) //klikl sem na consume posledniho zradla tak hidnu ten detail 
            //    Hide();
        }
    }

    public async void ConsumeButtonClicked()
    {
        OnConsumeClicked?.Invoke();

        //if (Data.amount == 1) //klikl sem na consume posledniho zradla tak hidnu ten detail protoze nejspis nebude brzy co konzumovat (az se vrati update z DB)
        //{
        //    Hide();
        //}

        if (Data.itemId == "TOWN_PORTAL")
        {
            //var result = await FirebaseCloudFunctionSO.UseTeleportScroll(, Data.uid);

            //if (result.Result)
            //{
            //    UIManager.instance.ImportantMessage.ShowMesssage("You were teleported!");

            //    if (Data.amount == 1) //klikl sem na consume posledniho zradla tak hidnu ten detail 
            //        Hide();
            //}
            UIManager.instance.ShowPoIChooser(PoIForTeleportScrollChoosen, "Choose teleport location");

            Hide();

            UIManager.instance.ContextInfoPanel.HideContentContainerDetail();

        }
        else
        {
            var result = await FirebaseCloudFunctionSO.ConsumeConsumable(Data.uid);

            if (result.Result)
            {
                UIManager.instance.ImportantMessage.ShowMesssage("Item consumed!");

                if (Data.amount == 1) //klikl sem na consume posledniho zradla tak hidnu ten detail 
                    Hide();
            }
        }

    }

}
