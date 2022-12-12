using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using simplestmmorpg.data;

public class UICharacterEquipPanel : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    public FirebaseCloudFunctionSO FirebaseCloudFunctionSO;
    public UIInventory UIInventoryPanel;
    public List<UIEquipSlotItem> EquipSlots;
    public UIEquipDetail UIEquipDetail_SelectedItem;
    public UIEquipDetail UIEquipDetail_EquipedItemToCompare;
    public UICharacterInfoPanel UICharacterInfoPanel;
    public Button EquipButton;
    public Button UnequipButton;
    public ContentFitterRefresh ContentFitterRefresh;

    public GameObject Model;
   // public CharacterData Data;


    private bool changesToEquipMade = false;
    private Equip chooosenInventoryItem;
    private UIEquipSlotItem choosenSlot;

    public void OnEnable()
    {
        AccountDataSO.OnCharacterDataChanged += Refresh;
    }

    public void OnDisable()
    {
        AccountDataSO.OnCharacterDataChanged -= Refresh;
    }


    public void Awake()
    {
        UIEquipDetail_SelectedItem.OnHideClicked += OnHideClicked;
    }


    public void Start()
    {
        UIInventoryPanel.OnContentItemClicked += OnInventoryItemClicked;

        foreach (var equipSlot in EquipSlots)
        {
            equipSlot.OnSlotClicked += OnSlotClicked;
        }


    }

    private void OnHideClicked()
    {
        UIEquipDetail_SelectedItem.Hide();
        UIEquipDetail_EquipedItemToCompare.Hide();
        EquipButton.gameObject.SetActive(false);
        UnequipButton.gameObject.SetActive(false);
    }

    private void Refresh()
    {
        changesToEquipMade = false;
        UIInventoryPanel.Refresh(AccountDataSO.CharacterData.inventory.content);
    }
    public void ShowPlayerCurrentCharacterInfo()
    {
        //  Data = AccountDataSO.CharacterData;
        Model.gameObject.SetActive(true);
        Refresh();

        // Show(AccountDataSO.CharacterData);
    }

    //public void Show(CharacterData _data)
    //{
    //    changesToEquipMade = false;
    //    Data = _data;
    //    Model.gameObject.SetActive(true);
    //}


    private void OnInventoryItemClicked(UIContentItem _item)
    {
        EquipButton.gameObject.SetActive(true);
        UnequipButton.gameObject.SetActive(false);

        UIEquipSlotItem equipedCorrespondingGear = GetCorrespondingEquipSlot((Equip)_item.GetData());

        if (equipedCorrespondingGear.IsSlotOccupied())
        {
            UIEquipDetail_EquipedItemToCompare.gameObject.SetActive(true);
            UIEquipDetail_EquipedItemToCompare.Show(equipedCorrespondingGear.GetEquip());
        }
        else
        {
            UIEquipDetail_EquipedItemToCompare.Hide();
            UIEquipDetail_EquipedItemToCompare.gameObject.SetActive(false);
        }

        chooosenInventoryItem = (Equip)_item.GetData();

        UIEquipDetail_SelectedItem.Show((Equip)_item.GetData());

        ContentFitterRefresh.RefreshContentFitters();
    }


    private void OnSlotClicked(UIEquipSlotItem _equipSlot)
    {
        UIEquipDetail_EquipedItemToCompare.Hide();
        UnequipButton.gameObject.SetActive(true);
        EquipButton.gameObject.SetActive(false);
        choosenSlot = _equipSlot;
        UIEquipDetail_SelectedItem.Show(_equipSlot.GetEquip());

        ContentFitterRefresh.RefreshContentFitters();
    }



    // Update is called once per frame
    public void Close()
    {
        Model.gameObject.SetActive(false);
    }

    public void OnDestroy()
    {
        UIInventoryPanel.OnContentItemClicked -= OnInventoryItemClicked;

        foreach (var equipSlot in EquipSlots)
        {
            equipSlot.OnSlotClicked -= OnSlotClicked;
        }

    }

    private UIEquipSlotItem GetCorrespondingEquipSlot(Equip _equip)
    {
        foreach (var equipSlot in EquipSlots)
        {
            if (equipSlot.EquipSlotDefinition.EquipSlotId == _equip.equipSlotId) // a ma to ten samy equip slot jako to na co sem klikl
                return equipSlot;
        }

        return null;
    }

    public void EquipClicked() // v detailu itemu kliknu na equip
    {
        if (chooosenInventoryItem.level>AccountDataSO.CharacterData.stats.level)
        {
            UIManager.instance.ImportantMessage.ShowMesssage("Your level is too low!");
            return;
        }

            //najdu si equip slot na doll
            UIEquipSlotItem equipedAlterntive = GetCorrespondingEquipSlot(chooosenInventoryItem);

        if (equipedAlterntive.IsSlotOccupied())//uz tam neco je vybaveneho
        {

            //item z doll dam do inventare
            ContentContainer dummyContent = new ContentContainer();
            dummyContent.contentEquip = equipedAlterntive.GetEquip();
            dummyContent.contentType = Utils.CONTENT_TYPE.EQUIP;

            UIInventoryPanel.AddItemOffline(dummyContent);
            equipedAlterntive.RemoveEquipManualy();


            //dam na doll kliknuty item
            equipedAlterntive.AddEquipManualy(chooosenInventoryItem);
            UIInventoryPanel.RemoveItemOffline(chooosenInventoryItem.uid);


        }
        else
        {
            equipedAlterntive.AddEquipManualy(chooosenInventoryItem);
            UIInventoryPanel.RemoveItemOffline(chooosenInventoryItem.uid);

        }

        Debug.Log("Equip Changed");
        changesToEquipMade = true;
        UIEquipDetail_SelectedItem.Hide();
        UIEquipDetail_EquipedItemToCompare.Hide();
        UICharacterInfoPanel.ShowPlayerCurrentCharacterInfo();

        EquipButton.gameObject.SetActive(false);
    }

    public void UnequipClicked()
    {
        if (choosenSlot.IsSlotOccupied())
        {
            //item z doll dam do inventare
            ContentContainer dummyContent = new ContentContainer();
            dummyContent.contentEquip = choosenSlot.GetEquip();
            dummyContent.contentType = Utils.CONTENT_TYPE.EQUIP;

            UIInventoryPanel.AddItemOffline(dummyContent);
            choosenSlot.RemoveEquipManualy();


        }


        Debug.Log("Equip Changed");
        changesToEquipMade = true;
        UIEquipDetail_SelectedItem.Hide();

        UICharacterInfoPanel.ShowPlayerCurrentCharacterInfo();
        UnequipButton.gameObject.SetActive(false);
    }

    // Update is called once per frame
    public void ApplyEquipChanges()
    {

        if (changesToEquipMade)
        {
            Debug.Log("applyingChanges");
            List<string> equipUids = new List<string>();
            foreach (var item in AccountDataSO.CharacterData.equipment)
            {
                Debug.Log("Equipnuty Equipu : " + item.uid + " ( " + item.displayName + ")");
                equipUids.Add(item.uid);
            }

            FirebaseCloudFunctionSO.ChangeEquip(equipUids);
        }
    }



}
