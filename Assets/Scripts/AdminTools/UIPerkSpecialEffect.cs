using System.Collections;
using System.Collections.Generic;
using simplestmmorpg.data;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using System;

public class UIPerkSpecialEffect : MonoBehaviour
{
    public SimpleTally Data;

    public TMP_Dropdown SpecialEffectIdDropDown;
    public TMP_InputField SpecialEffectAmountInput;

    public UnityAction<UIPerkSpecialEffect> OnRemoveClicked;
    public void Setup(SimpleTally _data)
    {
        Data = _data;
        SpecialEffectIdDropDown.value = Utils.GetIndexByPerkSpecialEffectIdBy(Data.id);
        SpecialEffectAmountInput.text = Data.count.ToString();

    }

    public void OnPerkSpecialEffectDropDownValueChanged(int _value)
    {
        Data.id = Utils.GetPerkSpecialEffectIdByIndex(_value);
    }


    public void OnAmountValueChanged(string _value)
    {
        Data.count = int.Parse(_value);
    }

    public void RemoveClicked()
    {
        OnRemoveClicked?.Invoke(this);

    }

    internal int OnPerkSpecialEffectDropDownValueChanged()
    {
        throw new NotImplementedException();
    }
}
