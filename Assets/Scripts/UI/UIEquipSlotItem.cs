using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;
using simplestmmorpg.data;
using UnityEngine.TextCore.Text;

public class UIEquipSlotItem : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    public ImageIdDefinitionSOSet AllImageIdDefinitionSOSet;
    public EquipSlotDefinition EquipSlotDefinition;
    public UIContentItem UIInventoryItem;

    private Equip Data = null;

    public GameObject NoDataGO;
    public Image EquipSlotPortraitImage;
    public UnityAction<UIEquipSlotItem> OnSlotClicked;

    private CharacterData Character = null;

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

    public void SetCharacter(CharacterData _character)
    {
        Character = _character;
        Refresh();
    }

    public void Refresh()
    {
        if (Character == null)
            Debug.LogWarning("Cant refresh equip in slot for character as it is null!");

        bool dataFound = false;
        foreach (var item in Character.equipment)
        {
            if (item.equipSlotId == EquipSlotDefinition.EquipSlotId)
            {
                SetData(item);
                dataFound = true;
                break;
            }
        }

        NoDataGO.SetActive(!dataFound);

        Debug.Log("EquipSlotDefinition.EquipSlotId:" + EquipSlotDefinition.EquipSlotId);
        Debug.Log("EquipSlotDefinition.EquipSlotId2" + Utils.DescriptionsMetadata.GetEquipSlots(EquipSlotDefinition.EquipSlotId).imageId);
        Debug.Log("EquipSlotDefinition.EquipSlotId3" + AllImageIdDefinitionSOSet.GetDefinitionById(Utils.DescriptionsMetadata.GetEquipSlots(EquipSlotDefinition.EquipSlotId).imageId).Image);

        EquipSlotPortraitImage.sprite = AllImageIdDefinitionSOSet.GetDefinitionById(Utils.DescriptionsMetadata.GetEquipSlots(EquipSlotDefinition.EquipSlotId).imageId).Image;

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
        // Refresh(); toto dava null nenei jeste setup
        //  AccountDataSO.OnCharacterDataChanged += SetCurrentCharacter;
        UIInventoryItem.OnClicked += SlotClicked;
    }

    public void OnDisable()
    {
        //   AccountDataSO.OnCharacterDataChanged -= SetCurrentCharacter;
        UIInventoryItem.OnClicked -= SlotClicked;
    }

    public void RemoveEquipManualy()
    {
        AccountDataSO.CharacterData.equipment.Remove(Data);
        Data = null;
        Refresh();

    }

    public void AddEquipManualy(Equip _equip)
    {
        if (!AccountDataSO.CharacterData.equipment.Contains(_equip))
        {
            AccountDataSO.CharacterData.equipment.Add(_equip);
            Refresh();
        }

    }
}
