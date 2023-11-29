using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using static UnityEngine.EventSystems.EventTrigger;
using UnityEngine.UI;
using System;

public class UIRestPanel : MonoBehaviour
{
    public FirebaseCloudFunctionSO FirebaseCloudFunctionSO;
    public AccountDataSO AccountDataSO;
    public PrefabFactory PrefabFactory;
    public Button LightRestButton;
    public Button DeepRestButton;
    public TextMeshProUGUI DeepRestSupplyCostText;

    public UIPriceTimeLabel LightRestTimePriceLabel;
    public UIPriceTimeLabel DeepRestTimePriceLabel;
    public GameObject Model;


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
        LightRestTimePriceLabel.SetPrice(12);
        DeepRestTimePriceLabel.SetPrice(8);

        var cost = AccountDataSO.OtherMetadataData.constants.restSupplyLimitBase + ((AccountDataSO.CharacterData.stats.level - 1) * AccountDataSO.OtherMetadataData.constants.restSupplyLimitIncrement);
        DeepRestSupplyCostText.SetText(cost.ToString());
       
    }

    public void DeepRestClicked()
    {
        //    FirebaseCloudFunctionSO.RestDeep();

    }

    //public void LightRestClicked()
    //{
    //    FirebaseCloudFunctionSO.RestLight();
    //}




}
