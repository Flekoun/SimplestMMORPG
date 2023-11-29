using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using simplestmmorpg.data;

public class UIEquipQualityUpgradePanel : MonoBehaviour
{
    public PrefabFactory PrefabFactory;
    public AccountDataSO AccountDataSO;
    public FirebaseCloudFunctionSO FirebaseCloudFunctionSO;
    public UIInventory UIInventoryPanel;
    public UICharacterEquipSlots UICharacterEquipSlots;
    public UIEquipDetail UIEquipDetail_SelectedItem;
    public UIEquipDetail UIEquipDetail_SelectedItemNextQuality;
    public Button UpgradeButton;

    public GameObject ContentItemPrefab;
    public Transform QualityMaterialsRequirementsParent;
    // public Button UnequipButton;

    public ContentFitterRefresh ContentFitterRefresh;

    public GameObject Model;

    // private bool changesToEquipMade = false;
    private Equip selectedEquip;
    //private UIEquipSlotItem choosenSlot;


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

        UICharacterEquipSlots.OnEquipSlotClicked += OnSlotClicked;

    }

    private void OnHideClicked()
    {
        UIEquipDetail_SelectedItem.Hide();
        UIEquipDetail_SelectedItemNextQuality.Hide();
        UpgradeButton.gameObject.SetActive(false);
    }

    private void Refresh()
    {
        //    changesToEquipMade = false;
        UIInventoryPanel.Refresh(AccountDataSO.CharacterData.inventory.content);

        UICharacterEquipSlots.Setup(AccountDataSO.CharacterData);
    }


    public void Show()
    {
        Model.gameObject.SetActive(true);
        Refresh();
    }

    private void OnInventoryItemClicked(UIContentItem _item)
    {
        EquipClicked((Equip)_item.GetData());
    }


    private void OnSlotClicked(UIEquipSlotItem _equipSlot)
    {
        EquipClicked(_equipSlot.GetEquip());
    }

    private void EquipClicked(Equip _equip)
    {
        selectedEquip = _equip;
        UpgradeButton.gameObject.SetActive(true);

        UIEquipDetail_SelectedItem.Show(_equip);

        Utils.DestroyAllChildren(QualityMaterialsRequirementsParent);

        if (_equip.quality < _equip.qualityMax)
        {
            UIEquipDetail_SelectedItemNextQuality.Show(_equip, _equip.quality + 1);
            UIEquipDetail_SelectedItemNextQuality.gameObject.SetActive(true);


            foreach (var mat in _equip.qualityUpgradeMaterials[_equip.quality].materialsNeeded)
            {
                var item = PrefabFactory.CreateGameObject<UIContentItem>(ContentItemPrefab, QualityMaterialsRequirementsParent);
                item.SetData(mat);
                item.SetAmountOwned(AccountDataSO.CharacterData.inventory.GetAmountOfItemsInInventory(mat.itemId));
            }
        }
        else
        {
            UIEquipDetail_SelectedItemNextQuality.Hide();
            UIEquipDetail_SelectedItemNextQuality.gameObject.SetActive(false);
        }




        ContentFitterRefresh.RefreshContentFitters();
    }




    // Update is called once per frame
    public void Close()
    {
        Model.gameObject.SetActive(false);
        OnHideClicked();
        selectedEquip = null;
        //    chooosenInventoryItem = null;
        //  choosenSlot = null;
    }

    public void OnDestroy()
    {
        UIInventoryPanel.OnContentItemClicked -= OnInventoryItemClicked;
    }




    public async void UpgradeClicked() // v detailu itemu kliknu na equip
    {
        var result = await FirebaseCloudFunctionSO.upgradeEquipQuality(selectedEquip.uid);
        if (result.Result)
        {
            UIEquipDetail_SelectedItem.Hide();
            UIEquipDetail_SelectedItemNextQuality.Hide();
            Utils.DestroyAllChildren(QualityMaterialsRequirementsParent);
            UIManager.instance.ImportantMessage.ShowMesssage("Equipment Upgraded!");
            UpgradeButton.gameObject.SetActive(false);
        }
    }




}
