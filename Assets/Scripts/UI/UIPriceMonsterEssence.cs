using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Diagnostics;

public class UIPriceMonsterEssence : MonoBehaviour
{

    public AccountDataSO AccountDataSO;
    public TextMeshProUGUI PriceText;
    private int Price = 0;
    public void OnEnable()
    {
        AccountDataSO.OnCharacterDataChanged += Refresh;
        Refresh();
    }


    public void OnDisable()
    {
        AccountDataSO.OnCharacterDataChanged -= Refresh;
    }

    private void Refresh()
    {
        if (Price > AccountDataSO.CharacterData.currency.monsterEssence)
            PriceText.color = Color.red;
        else
            PriceText.color = Color.white;

        PriceText.SetText(Price.ToString());
    }

    public void SetPrice(int _price)
    {
        Price = _price;
        Refresh();
    }

    public void SetColor(Color _color)
    {
        PriceText.color = _color;
    }

}
