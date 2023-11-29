using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using simplestmmorpg.data;

public class UIFoodEffect : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    public ImageIdDefinitionSOSet AllImageIdDefinitionSOSet;
    public Image Portrait;
    public TextMeshProUGUI AmountText;
    public TooltipSpawner TooltipSpawner;
    private SimpleTally Data;

    //public void Awake()
    //{
    //    AccountDataSO.OnCharacterDataChanged += Refresh;
    //}

    public void Setup(SimpleTally _data)
    {
        Data = _data;

        AmountText.SetText(Data.count.ToString());
        Portrait.sprite = AllImageIdDefinitionSOSet.GetDefinitionById(Data.id, "FOOD_EFFECT").Image;
        TooltipSpawner.SetString(Data.id, new int[] { Data.count });

    }
}
