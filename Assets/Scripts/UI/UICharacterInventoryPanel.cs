using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using simplestmmorpg.data;
using UnityEngine.UI;

public class UICharacterInventoryPanel : MonoBehaviour
{
    public FirebaseCloudFunctionSO FirebaseCloudFunctionSO;
    public AccountDataSO AccountDataSO;
    public TextMeshProUGUI CapacityText;

    public Button ToggleDropModeButton;
    public TextMeshProUGUI ToggleDropModeText;

    public Button DropButton;
    public UIInventory UIInventory;
    //public UIContentContainerDetail UIItemDetail;

    private UIContentItem SelectedItem;

    private bool isInDropMode = false;
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
        SetDropMode(false);
        // UIInventory.Show();
    }

    // Update is called once per frame
    public void Hide()
    {
        AccountDataSO.OnCharacterDataChanged -= Refresh;
        Model.gameObject.SetActive(false);
        RestartToAsNothingIsSelected();
    }

    private void RestartToAsNothingIsSelected()
    {
        //     UIManager.instance.ContextInfoPanel.HideContentContainerDetail();
        SelectedItem = null;
        DropButton.gameObject.SetActive(false);
        SetDropMode(false);
    }

    private void OnInventoryItemClicked(UIContentItem _inventoryItem)
    {
        UIManager.instance.ContextInfoPanel.ShowContentContainerDetail(_inventoryItem.GetData());
        SelectedItem = _inventoryItem;
        //DropButton.gameObject.SetActive(true);



        if (isInDropMode)
            DropButton.gameObject.SetActive(UIInventory.IsAnyItemSelected());
        else
            DropButton.gameObject.SetActive(false);

        if (isInDropMode && !UIInventory.IsAnyItemSelected())
            RestartToAsNothingIsSelected();

        ToggleDropModeButton.gameObject.SetActive(SelectedItem != null);
    }

    public void ToggleDropMode()
    {
        SetDropMode(!isInDropMode);
    }

    public void SetDropMode(bool _on)
    {
        isInDropMode = _on;
        UIInventory.ClearItemsSelected();
        UIInventory.MultiSelect = _on;
        UIInventory.UseItemSelectFeature = _on;
        isInDropMode = _on;

        if (_on)
        {
            ToggleDropModeButton.targetGraphic.color = Color.gray;
            if (SelectedItem != null)
            {
                UIInventory.AddAsSelected(SelectedItem);
                DropButton.gameObject.SetActive(true);
            }


        }
        else
        {
            DropButton.gameObject.SetActive(false);
            ToggleDropModeButton.targetGraphic.color = Color.white;
        }

    }



    public void DropClicked()
    {
        UIManager.instance.SpawnPromptPanel("By dropping the item, you will pernamently removed it from the inventory. Do you want to proceed?", () =>
        {
            FirebaseCloudFunctionSO.DropItem(UIInventory.GetSelectedItemsUids());
        }, null);

        // RestartToAsNothingIsSelected();
    }

    private void Refresh()
    {
        UIInventory.Refresh(AccountDataSO.CharacterData.inventory.content);
        CapacityText.SetText(AccountDataSO.CharacterData.inventory.capacityMax - AccountDataSO.CharacterData.inventory.capacityLeft + "/" + AccountDataSO.CharacterData.inventory.capacityMax);
        RestartToAsNothingIsSelected();
    }

}
