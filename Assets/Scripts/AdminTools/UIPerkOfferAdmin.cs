using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.Events;
using System.Drawing;
using simplestmmorpg.adminToolsData;
using simplestmmorpg.data;
using System.Drawing.Drawing2D;
using Unity.VisualScripting;
using Image = UnityEngine.UI.Image;

public class UIPerkOfferAdmin : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    public FirebaseCloudFunctionSO FirebaseCloudFunctionSO;
    public PrefabFactory PrefabFactory;

    public TMP_InputField PriceTimeInput;
    //  public TextMeshProUGUI StockLeftText;
    // public TextMeshProUGUI RewardTypeText;

    public TMP_InputField CurseAmountInput;
    //  public TextMeshProUGUI SpecialEffectText;

    public TMP_Dropdown RarityDropdown;

    public TMP_InputField ChargesInput;
    public TMP_InputField ReccurenceInGameDaysInput;
    public TMP_InputField RewardAfterSpecificGameDayInput;
    public TMP_InputField RewardAtSpecificGameDayInput;
    public TMP_InputField StockLeftInput;
    public TMP_InputField StockClaimedInput;
    public TMP_InputField ChanceToSpawnInput;

    public GameObject UIRandomEquipAdminPrefab;
    public GameObject UIItemWithIdAdminPrefab;
    public GameObject UIContentItemPrefab;
    public GameObject UIPerkSpecialEffectPrefab;
    //  public GameObject CurseGO;
    //public GameObject ChoosenGO;

    public Transform RewardsParent;
    public Transform PerkSpecialEffectsParent;

    public GameObject ClaimButton;

    public TMP_Dropdown PerkType;
    public Image BackImage;
    //public GameObject Model;
    public PerkOfferDefinitionAdmin Data;

    public UnityAction<UIPerkOfferAdmin> OnRemoveThisPerk;
    public UnityAction<UIPerkOfferAdmin> OnDuplicateThisPerk;
    // public UnityAction<UIContentItem> OnContentItemClicked;


    //  public UIItemIdChooser UIItemIdChooser;

    private bool IsRarePerk = false;
    //public void Awake()
    //{
    //    UIItemIdChooser.OnItemsToAddSelected += OnChooserItemsSelected;
    //}

    public void OnPerkTypeValueChanged(int _value)
    {
        SetupDataByPerkType(_value);
    }

    private void SetupDataByPerkType(int _perkType)
    {
        switch (_perkType)
        {
            case 0: //instant

                Data.charges = -1;
                Data.isInstantReward = true;
                Data.recurrenceInGameDays = -1;
                Data.rewardAfterSpecificGameDay = -1;
                Data.rewardAtSpecificGameDay = -1;

                break;
            case 1: //Recurring

                Data.charges = 1;
                Data.isInstantReward = false;
                Data.recurrenceInGameDays = 1;
                Data.rewardAfterSpecificGameDay = -1;
                Data.rewardAtSpecificGameDay = -1;

                break;

            case 2: //Reward at specific game day

                Data.charges = -1;
                Data.isInstantReward = false;
                Data.recurrenceInGameDays = -1;
                Data.rewardAfterSpecificGameDay = -1;
                Data.rewardAtSpecificGameDay = 1;

                break;

            case 3: //Reward after specific game day

                Data.charges = -1;
                Data.isInstantReward = false;
                Data.recurrenceInGameDays = -1;
                Data.rewardAfterSpecificGameDay = 1;
                Data.rewardAtSpecificGameDay = -1;

                break;

            default:
                break;
        }
        Refresh();
    }



    private void OnChooserItemsSelected(List<UISelectableEntry> _selectedItems)
    {

        foreach (var itemToAdd in _selectedItems)
        {
            if (itemToAdd.GetUid() == "EQUIP")
            {
                RandomEquip newItem = new RandomEquip();
                newItem.rarity = Utils.RARITY.COMMON;
                newItem.mLevel = 1;
                newItem.equipSlotId = Utils.EQUIP_SLOT_ID.ANY;

                Data.rewardsRandomEquip.Add(newItem);
            }
            else
            {
                ItemIdWithAmountAdmin newItem = new ItemIdWithAmountAdmin();
                newItem.amount = 1;
                newItem.itemId = itemToAdd.GetUid();

                Data.rewardsGenerated.Add(newItem);
            }
        }
        Refresh();
    }



    public void Setup(PerkOfferDefinitionAdmin _data, bool _isRarePerk)
    {
        Data = _data;
        if (Data.isInstantReward)
            PerkType.SetValueWithoutNotify(0);// SetupDataByPerkType(0);
        else if (Data.recurrenceInGameDays != -1)
            PerkType.SetValueWithoutNotify(1);// SetupDataByPerkType(1);
        else if (Data.rewardAtSpecificGameDay != -1)
            PerkType.SetValueWithoutNotify(2);//SetupDataByPerkType(2);
        else if (Data.rewardAfterSpecificGameDay != -1)
            PerkType.SetValueWithoutNotify(3);//SetupDataByPerkType(3);

        IsRarePerk = _isRarePerk;
        Refresh();
    }



    public void Refresh()
    {

        //  CurseGO.SetActive(Data.curseCount > 0);


        PriceTimeInput.text = (Data.timePrice.ToString());
        CurseAmountInput.text = (Data.curseCount.ToString());
        //  SuccessPriceInput.text = (Data.successPrice.ToString());

        Utils.DestroyAllChildren(PerkSpecialEffectsParent);

        foreach (var item in Data.specialEffectId)
        {
            var content = PrefabFactory.CreateGameObject<UIPerkSpecialEffect>(UIPerkSpecialEffectPrefab, PerkSpecialEffectsParent);
            content.Setup(item);
            content.OnRemoveClicked += OnSpecialEffectRemoveClicked;
            ////  content.OnClicked += OnContentClicked;
        }

        Utils.DestroyAllChildren(RewardsParent);

        foreach (var item in Data.rewards)
        {
            var content = PrefabFactory.CreateGameObject<UIContentItem>(UIContentItemPrefab, RewardsParent);
            content.SetData(item.GetContent());
            ////  content.OnClicked += OnContentClicked;
        }

        foreach (var item in Data.rewardsGenerated)
        {
            var content = PrefabFactory.CreateGameObject<UIItemIdWithAmountAdmin>(UIItemWithIdAdminPrefab, RewardsParent);
            content.SetData(item);
            content.OnRemoveClicked += RemoveItem;
            //  content.OnClicked += OnContentClicked;
        }

        foreach (var item in Data.rewardsRandomEquip)
        {
            var equip = PrefabFactory.CreateGameObject<UIRandomEquipAdmin>(UIRandomEquipAdminPrefab, RewardsParent);
            equip.SetData(item);
            equip.OnRemoveClicked += RemoveItem;
            //  equip.OnClicked += OnRewardRandomEquipClicked;
        }

        if (Data.isInstantReward)
        {
            ChargesInput.gameObject.SetActive(false);
            ReccurenceInGameDaysInput.gameObject.SetActive(false);
            RewardAfterSpecificGameDayInput.gameObject.SetActive(false);
            RewardAtSpecificGameDayInput.gameObject.SetActive(false);

        }
        else if (Data.recurrenceInGameDays > 0)
        {

            ChargesInput.gameObject.SetActive(true);
            ReccurenceInGameDaysInput.gameObject.SetActive(true);
            RewardAfterSpecificGameDayInput.gameObject.SetActive(false);
            RewardAtSpecificGameDayInput.gameObject.SetActive(false);
        }
        else if (Data.rewardAfterSpecificGameDay > 0)
        {
            ChargesInput.gameObject.SetActive(false);
            ReccurenceInGameDaysInput.gameObject.SetActive(false);
            RewardAfterSpecificGameDayInput.gameObject.SetActive(true);
            RewardAtSpecificGameDayInput.gameObject.SetActive(false);
        }
        else if (Data.rewardAtSpecificGameDay > 0)
        {
            ChargesInput.gameObject.SetActive(false);
            ReccurenceInGameDaysInput.gameObject.SetActive(false);
            RewardAfterSpecificGameDayInput.gameObject.SetActive(false);
            RewardAtSpecificGameDayInput.gameObject.SetActive(true);
        }

        RewardAfterSpecificGameDayInput.text = Data.rewardAfterSpecificGameDay.ToString();
        RewardAtSpecificGameDayInput.text = Data.rewardAtSpecificGameDay.ToString();
        ReccurenceInGameDaysInput.text = Data.recurrenceInGameDays.ToString();
        ChargesInput.text = Data.charges.ToString();

        StockLeftInput.gameObject.SetActive(true);//IsRarePerk);
        StockClaimedInput.gameObject.SetActive(false);//IsRarePerk);
        ChanceToSpawnInput.gameObject.SetActive(IsRarePerk);

        ChanceToSpawnInput.text = Data.chanceToSpawn.ToString();
        StockLeftInput.text = Data.stockLeft.ToString();
        StockClaimedInput.text = "0"; Data.stockClaimed = 0;// Data.stockClaimed.ToString();

        RarityDropdown.value = Utils.GetRarityIndex(Data.rarity.ToString());

        var color = Utils.GetRarityColor(Data.rarity.ToString());
        color.a = 0.5f;
        BackImage.color = color;

        //if (Data.stockLeft == -1)
        //    StockLeftText.SetText("Unlimited stock");
        //else
        //    StockLeftText.SetText("Rewards left in stock : " + Data.stockLeft.ToString());
        //foreach (var item in Data.rewardsRecurring)
        //{
        //    var content = PrefabFactory.CreateGameObject<UIContentItem>(ContentContainerPrefab, RewardsRecrringParent);
        //    content.SetData(item);
        //    content.OnClicked += OnContentClicked;
        //}

        //ClaimButton.SetActive(AccountDataSO.CharacterData.IsQuestCompleted(Data));

    }

    public void OnRarityDropDownValueChanged(int _value)
    {
        Data.rarity = Utils.GetRarityByIndex(_value);
    }

    private void OnSpecialEffectRemoveClicked(UIPerkSpecialEffect _item)
    {
        Data.specialEffectId.Remove(_item.Data);
        Refresh();
    }

    public void RewardAfterSpecificGameDayInputValueChanged(string _value)
    {
        Data.rewardAfterSpecificGameDay = int.Parse(_value);
    }

    public void RewardAtSpecificGameDayInputValueChanged(string _value)
    {
        Data.rewardAtSpecificGameDay = int.Parse(_value);
    }

    public void ReccurenceInGameDaysInputValueChanged(string _value)
    {
        Data.recurrenceInGameDays = int.Parse(_value);
    }

    public void ChargesInputInputValueChanged(string _value)
    {
        Data.charges = int.Parse(_value);
    }

    public void CurseCountInputInputValueChanged(string _value)
    {
        Data.curseCount = int.Parse(_value);
    }

    //public void SuccessPriceInputValueChanged(string _value)
    //{
    //    Data.successPrice = int.Parse(_value);
    //}

    public void PriceTimeInputInputValueChanged(string _value)
    {
        Data.timePrice = int.Parse(_value);
    }


    public void StockLeftInputInputValueChanged(string _value)
    {
        Data.stockLeft = int.Parse(_value);
    }

    public void StockClaimedInputInputValueChanged(string _value)
    {
        Data.stockClaimed = int.Parse(_value);
    }


    public void ChanceToSpawnInputInputValueChanged(string _value)
    {
        Data.chanceToSpawn = float.Parse(_value);
    }



    public void RemoveItem(UIRandomEquipAdmin _item)
    {
        Data.rewardsRandomEquip.Remove(_item.Data);
        Refresh();
    }

    public void RemoveItem(UIItemIdWithAmountAdmin _item)
    {
        Data.rewardsGenerated.Remove(_item.Data);
        Refresh();
    }

    public void AddItemClicked()
    {
        AdminToolsManager.instance.ShowItemChooserItems(OnChooserItemsSelected);
        //   UIItemIdChooser.Show();
    }


    public void RemoveThisClicked()
    {
        OnRemoveThisPerk?.Invoke(this);

    }

    public void OnDuplicateClicked()
    {
        OnDuplicateThisPerk?.Invoke(this);

    }

    public void AddSpecialEffectClicked()
    {
        SimpleTally effect = new SimpleTally();
        effect.count = 0;
        effect.id = Utils.PERK_SPECIAL_EFFECT.ENEMY_ALL_ADD_HEALTH;

        Data.specialEffectId.Add(effect);
        Refresh();
    }

    //// Start is called before the first frame update
    //public void ChooseClicked()
    //{
    //    OnChooseClicked?.Invoke(this);
    //    // FirebaseCloudFunctionSO.ClaimQuestgiverReward(Data.id);
    //    // Hide();
    //}

    //public void Save()
    //{

    //    //Data.dropCountMax = int.Parse(DropCountMaxInput.text);
    //    //Data.dropCountMin = int.Parse(DropCountMinInput.text);
    //    foreach (var item in List)
    //    {
    //        item.Save();
    //    }
    //}


}
