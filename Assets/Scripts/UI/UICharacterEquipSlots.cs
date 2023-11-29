using System.Collections;
using System.Collections.Generic;
using simplestmmorpg.data;
using UnityEngine;
using UnityEngine.Events;

public class UICharacterEquipSlots : MonoBehaviour
{

    public List<UIEquipSlotItem> EquipSlots;
    public UnityAction<UIEquipSlotItem> OnEquipSlotClicked;
    private CharacterData Data;

    public void Setup(CharacterData _data)
    {
        Debug.Log("VOLAM setup : " + _data.characterName);
        Data = _data;

        foreach (var slot in EquipSlots)
            slot.SetCharacter(Data);
    }

    public void Start()
    {
        foreach (var equipSlot in EquipSlots)
            equipSlot.OnSlotClicked += OnSlotClicked;

    }

    private void OnSlotClicked(UIEquipSlotItem _equipSlot)
    {
        OnEquipSlotClicked?.Invoke(_equipSlot);
    }

    public UIEquipSlotItem GetCorrespondingEquipSlot(Equip _equip)
    {
        foreach (var equipSlot in EquipSlots)
        {
            if (equipSlot.EquipSlotDefinition.EquipSlotId == _equip.equipSlotId) // a ma to ten samy equip slot jako to na co sem klikl
                return equipSlot;
        }

        return null;
    }


}
