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
    public UICharacterEquipSlots UICharacterEquipSlots;
    public UIEquipDetail UIEquipDetail_SelectedItem;
    public UIEquipDetail UIEquipDetail_EquipedItemToCompare;
    public UICharacterInfoPanel UICharacterInfoPanel;
    public Button EquipButton;
    public Button UnequipButton;

    public ContentFitterRefresh ContentFitterRefresh;

    public GameObject Model;
    public CharacterData Data;


    private bool changesToEquipMade = false;
    private Equip chooosenInventoryItem;
    private UIEquipSlotItem choosenSlot;

    private bool IsMyCharacter()
    {
        return Data == AccountDataSO.CharacterData;
    }

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

        //foreach (var equipSlot in EquipSlots)
        //{
        //    equipSlot.OnSlotClicked += OnSlotClicked;
        //}

        UICharacterEquipSlots.OnEquipSlotClicked += OnSlotClicked;


    }

    private void OnHideClicked()
    {
        UIEquipDetail_SelectedItem.Hide();
        UIEquipDetail_EquipedItemToCompare.Hide();
        UIEquipDetail_EquipedItemToCompare.gameObject.SetActive(false);
        EquipButton.gameObject.SetActive(false);
        UnequipButton.gameObject.SetActive(false);
    }

    private void Refresh()
    {
        UIInventoryPanel.gameObject.SetActive(IsMyCharacter());
        changesToEquipMade = false;
        UIInventoryPanel.Refresh(Data.inventory.content);

        UICharacterEquipSlots.Setup(Data);
        //foreach (var slot in EquipSlots)
        //    slot.SetCharacter(Data);
    }

    public void ShowPlayerCharacter()
    {
        Show(AccountDataSO.CharacterData);
    }

    public void Show(CharacterData _data)
    {
        Data = _data;
        Model.gameObject.SetActive(true);
        Refresh();

        UICharacterInfoPanel.Show(Data);

    }



    private void OnInventoryItemClicked(UIContentItem _item)
    {
        if (!IsMyCharacter())
            return;

        EquipButton.gameObject.SetActive(true);
        UnequipButton.gameObject.SetActive(false);

        UIEquipSlotItem equipedCorrespondingGear = UICharacterEquipSlots.GetCorrespondingEquipSlot((Equip)_item.GetData());

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
        UIEquipDetail_EquipedItemToCompare.gameObject.SetActive(false);
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
        OnHideClicked();
        chooosenInventoryItem = null;
        choosenSlot = null;
    }

    public void OnDestroy()
    {
        UIInventoryPanel.OnContentItemClicked -= OnInventoryItemClicked;

        //foreach (var equipSlot in EquipSlots)
        //{
        //    equipSlot.OnSlotClicked -= OnSlotClicked;
        //}

    }

    //private UIEquipSlotItem GetCorrespondingEquipSlot(Equip _equip)
    //{
    //    foreach (var equipSlot in EquipSlots)
    //    {
    //        if (equipSlot.EquipSlotDefinition.EquipSlotId == _equip.equipSlotId) // a ma to ten samy equip slot jako to na co sem klikl
    //            return equipSlot;
    //    }

    //    return null;
    //}



    public void EquipClicked() // v detailu itemu kliknu na equip
    {
        if (!IsMyCharacter())
            return;


        if (chooosenInventoryItem.level > AccountDataSO.CharacterData.stats.level)
        {
            UIManager.instance.ImportantMessage.ShowMesssage("Your level is too low!");
            return;
        }

        //najdu si equip slot na doll
        UIEquipSlotItem equipedAlterntive = UICharacterEquipSlots.GetCorrespondingEquipSlot(chooosenInventoryItem);

        if (equipedAlterntive.IsSlotOccupied())//uz tam neco je vybaveneho
        {
            Debug.Log("JE OBSAZENY? PROHAZUJU :" + equipedAlterntive.GetEquip().uid+ " za " + chooosenInventoryItem.uid);
            //item z doll dam do inventare
            //ContentContainer dummyContent = new ContentContainer();
            //dummyContent.contentEquip = equipedAlterntive.GetEquip();

            UIInventoryPanel.AddItemOffline(equipedAlterntive.GetEquip(),true);
            equipedAlterntive.RemoveEquipManualy();


            //dam na doll kliknuty item
            equipedAlterntive.AddEquipManualy(chooosenInventoryItem);
            UIInventoryPanel.RemoveItemOffline(chooosenInventoryItem.uid, true);


        }
        else
        {
            Debug.Log("NIC TAM NENI");
            equipedAlterntive.AddEquipManualy(chooosenInventoryItem);
            UIInventoryPanel.RemoveItemOffline(chooosenInventoryItem.uid, true);

        }

        Debug.Log("Equip Changed");
        changesToEquipMade = true;
        UIEquipDetail_SelectedItem.Hide();
        UIEquipDetail_EquipedItemToCompare.Hide();
        UIEquipDetail_EquipedItemToCompare.gameObject.SetActive(false);
        UICharacterInfoPanel.Show(Data);

        EquipButton.gameObject.SetActive(false);
    }

    public void UnequipClicked()
    {
        if (!IsMyCharacter())
            return;


        if (choosenSlot.IsSlotOccupied())
        {
            //item z doll dam do inventare
            //ContentContainer dummyContent = new ContentContainer();
            //dummyContent.contentEquip = choosenSlot.GetEquip();

            UIInventoryPanel.AddItemOffline(choosenSlot.GetEquip(), true);
            choosenSlot.RemoveEquipManualy();


        }


        Debug.Log("Equip Changed");
        changesToEquipMade = true;
        UIEquipDetail_SelectedItem.Hide();

        UICharacterInfoPanel.Show(Data);
        UnequipButton.gameObject.SetActive(false);
    }

    // Update is called once per frame
    public void ApplyEquipChanges()
    {
        if (!IsMyCharacter())
            return;


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
