using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;
using simplestmmorpg.data;

public class UIEquipSlotItem : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    public EquipSlotDefinition EquipSlotDefinition;
    public UIContentItem UIInventoryItem;

    private Equip Data = null;

    public GameObject NoDataGO;
    public Image EquipSlotPortraitImage;
    public UnityAction<UIEquipSlotItem> OnSlotClicked;

    public bool IsSlotOccupied()
    {
        return Data != null;
    }

    public Equip GetEquip()
    {
        if (Data != null)
            return Data;
        else
        {
            Debug.LogError("EQUIP SLOT IS EMPTY!! Dont call this function only null you get");
            return null;
        }
    }

    public void SetCurrentCharacter()
    {

        SetCharacter(AccountDataSO.CharacterData);
    }

    public void SetCharacter(CharacterData _characterData)
    {
        bool dataFound = false;
        foreach (var item in _characterData.equipment)
        {
            if (item.equipSlotId == EquipSlotDefinition.EquipSlotId)
            {
                SetData(item);
                dataFound = true;
                break;
            }
        }

        NoDataGO.SetActive(!dataFound);

        EquipSlotPortraitImage.sprite = EquipSlotDefinition.EquipSlotImage;

    }

    // Start is called before the first frame update
    private void SetData(Equip _data)//, UIInventoryPanel _uiInventoryPanel)
    {
        Data = _data;
        UIInventoryItem.SetData(Data);


    }

    public void SlotClicked(UIContentItem _item)
    {
        if (OnSlotClicked != null)
            OnSlotClicked.Invoke(this);

    }

    public void OnEnable()
    {
        SetCurrentCharacter();
        AccountDataSO.OnCharacterDataChanged += SetCurrentCharacter;
        UIInventoryItem.OnClicked += SlotClicked;
    }

    public void OnDisable()
    {
        AccountDataSO.OnCharacterDataChanged -= SetCurrentCharacter;
        UIInventoryItem.OnClicked -= SlotClicked;
    }

    public void RemoveEquipManualy()
    {
        AccountDataSO.CharacterData.equipment.Remove(Data);
        Data = null;
        SetCurrentCharacter();

    }

    public void AddEquipManualy(Equip _equip)
    {
        if (!AccountDataSO.CharacterData.equipment.Contains(_equip))
        {
            AccountDataSO.CharacterData.equipment.Add(_equip);
            SetCurrentCharacter();
        }

    }
}
