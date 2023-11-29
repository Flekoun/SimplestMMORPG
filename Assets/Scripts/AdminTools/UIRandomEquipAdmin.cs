using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using simplestmmorpg.adminToolsData;
using UnityEngine.Events;
using simplestmmorpg.data;

public class UIRandomEquipAdmin : MonoBehaviour
{
    public ImageIdDefinitionSOSet AllImageIdDefinitionSOSet;

    public TMP_InputField MLevelInput;
    public TMP_Dropdown EquipSlotIdDropdown;
    public TMP_Dropdown RarityDropdown;
    public Image RarityImage;
    public Image PortraitImage;

    public RandomEquip Data;

    public UnityAction<UIRandomEquipAdmin> OnRemoveClicked;


    // Start is called before the first frame update
    public void SetData(RandomEquip _item)
    {
        Data = _item;

        RarityImage.color = Utils.GetRarityColor(_item.rarity.ToString());
        RarityDropdown.value = Utils.GetRarityIndex(_item.rarity.ToString());
        EquipSlotIdDropdown.value = Utils.GetIndexByEquipSlot(_item.equipSlotId);
        PortraitImage.sprite = AllImageIdDefinitionSOSet.GetDefinitionById("EQUIP").Image;

        MLevelInput.text = Data.mLevel.ToString();
    }

    public void OnRarityDropDownValueChanged(int _value)
    {
        Data.rarity = Utils.GetRarityByIndex(_value);
    }

    public void OnEquipSlotIdDropdownValueChanged(int _value)
    {
        Debug.Log("Utils.GetEquipSlotByIndex(_value);::" + Utils.GetEquipSlotByIndex(_value));
        Data.equipSlotId = Utils.GetEquipSlotByIndex(_value);
    }

    public void OnMLevelInputValueChanged(string _value)
    {
        Data.mLevel = int.Parse(_value);
    }


    public void RemoveClicked()
    {
        OnRemoveClicked.Invoke(this);
    }
}
