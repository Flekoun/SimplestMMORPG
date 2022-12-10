using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using simplestmmorpg.data;

public class UICharacterInventoryPanel : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    public TextMeshProUGUI CapacityText;
    
    public UIInventory UIInventory;
    public UIContentContainerDetail UIItemDetail;
   // public UIItemSimpleDetail UIItemSimpleDetail;

    public GameObject Model;

    public void Awake()
    {
        UIInventory.OnContentItemClicked += OnInventoryItemClicked;
    }
    // Start is called before the first frame update
    public void Show()
    {
        AccountDataSO.OnCharacterDataChanged += Refresh;
        Model.gameObject.SetActive(true);
        Refresh();
        // UIInventory.Show();
    }

    // Update is called once per frame
    public void Hide()
    {
        AccountDataSO.OnCharacterDataChanged -= Refresh;
        Model.gameObject.SetActive(false);

    }

    private void OnInventoryItemClicked(UIContentItem _inventoryItem)
    {
        UIItemDetail.Show(_inventoryItem.GetData());
    }

    private void Refresh()
    {
        UIInventory.Refresh(AccountDataSO.CharacterData.inventory.content);
        CapacityText.SetText(AccountDataSO.CharacterData.inventory.capacityMax - AccountDataSO.CharacterData.inventory.capacityLeft + "/" + AccountDataSO.CharacterData.inventory.capacityMax);
    }

}
