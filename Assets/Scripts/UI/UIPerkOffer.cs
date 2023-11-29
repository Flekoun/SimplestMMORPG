using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using simplestmmorpg.data;
using System;
using UnityEngine.Events;

public class UIPerkOffer : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    public FirebaseCloudFunctionSO FirebaseCloudFunctionSO;
    public PrefabFactory PrefabFactory;

    public UIPriceTimeLabel PriceTime;
    public TextMeshProUGUI StockLeftText;

    public TextMeshProUGUI ChargesText;
    public TextMeshProUGUI CurseAmountText;
    public TextMeshProUGUI SpecialEffectText;
    public Image RarityImage;
    public TooltipSpawner RewardTypeTooltip;
    public GameObject RewardTypeGO;
    public Image RewardTypeImage;
    public TextMeshProUGUI RewardTypeText;
    public UIPortrait Chooser_UIPortrait;
    //public Button ChooseButton;

    public Sprite RewardInstantSprite;
    public Sprite RewardRecurringSprite;
    public Sprite RewardGameDaySprite;
    //  public UIContentContainerDetail UIContentContainerDetail;

    public GameObject UIRandomEquipPrefab;
    public GameObject ContentContainerPrefab;
    public GameObject CurseGO;
    public GameObject OutOfStockGO;
    //   public GameObject ChoosenGO;

    public Transform RewardsParent;


    public GameObject ClaimButton;
    //public GameObject Model;
    public PerkOfferDefinition Data;
    private EncounterData EncounterData;

    public UnityAction<UIPerkOffer> OnChooseClicked;
    //public UnityAction<UIContentItem> OnContentItemClicked;
    //public UIQuestgiverSpawner UIQuestgiverSpawner;


    public void Awake()
    {
        //  UIQuestgiverSpawner.OnEntryClicked += OnQuestgiverEntryClicked;

    }

    //private void OnQuestgiverEntryClicked(UIPerkOffer _entry)
    //{
    //    Show(_entry.Data);
    //}

    public void Setup(PerkOfferDefinition _data, EncounterData _encounterData)
    {
        //if (_contentDetail != null)
        //    UIContentContainerDetail = _contentDetail;

        EncounterData = _encounterData;
        Data = _data;
        //   Model.SetActive(true);
        Refresh();
    }

    public void CheckForShowingChooserPortrait(EncounterData _data)
    {
        Chooser_UIPortrait.gameObject.SetActive(false);
        var result = _data.perkChoices.Find(item => item.choosenPerk != null && item.choosenPerk.uid == Data.uid);
        if (result != null)
        {
            Chooser_UIPortrait.SetPortrait(result.characterPortraitId, result.characterClass);
            Chooser_UIPortrait.gameObject.SetActive(true);
        }
    }

    public void Refresh()
    {


        ClaimButton.GetComponent<Button>().interactable = !EncounterData.IsPerkUidAmongPerkChoices(Data.uid) && Data.StockRemaining() > 0;
        //    ChoosenGO.SetActive(EncounterData.perkChoosen);
        CurseGO.SetActive(Data.curseCount > 0);
        //  ChooseButton.gameObject.SetActive(!EncounterData.perkChoosen);

        //        SuccessPriceText.SetText(Data.successPrice.ToString());
        //if (UIContentContainerDetail != null)
        //    UIContentContainerDetail.Hide();
        RarityImage.color = Utils.GetRarityColor(Data.rarity);
        PriceTime.SetPrice(Data.timePrice);
        CurseAmountText.SetText(Data.curseCount.ToString());

        SpecialEffectText.gameObject.SetActive(Data.specialEffectId.Count > 0);
        SpecialEffectText.SetText(Data.GetDescription());

        Utils.DestroyAllChildren(RewardsParent);

        foreach (var item in Data.rewards)
        {
            var content = PrefabFactory.CreateGameObject<UIContentItem>(ContentContainerPrefab, RewardsParent);
            content.SetData(item.GetContent(), true);
            //    content.OnClicked += OnContentClicked;
        }

        foreach (var item in Data.rewardsGenerated)
        {
            var content = PrefabFactory.CreateGameObject<UIContentItem>(ContentContainerPrefab, RewardsParent);
            content.SetData(item, true);
            //  content.OnClicked += OnContentClicked;
        }

        foreach (var item in Data.rewardsRandomEquip)
        {
            //var equip = PrefabFactory.CreateGameObject<UIRandomEquip>(UIRandomEquipPrefab, RewardsParent);
            //equip.SetData(item);
            //equip.OnClicked += OnRewardRandomEquipClicked;

            var equip = PrefabFactory.CreateGameObject<UIContentItem>(ContentContainerPrefab, RewardsParent);
            equip.SetData(item, true);
            //  equip.OnClicked += OnContentClicked;
        }
        RewardTypeGO.gameObject.SetActive(true);
        RewardTypeText.gameObject.SetActive(false);
        if (Data.isInstantReward)
        {
            RewardTypeGO.gameObject.SetActive(false);
            RewardTypeTooltip.SetString("UI_TOOLTIP_INSTANT_PERK_REWARD");
            RewardTypeImage.sprite = RewardInstantSprite;
            RewardTypeText.SetText("");
        }
        else if (Data.recurrenceInGameDays > 0)
        {
            //  RewardTypeTooltip.SetString("UI_TOOLTIP_SCAVENGE_PERK_REWARD", new int[] { Data.recurrenceInGameDays });
            RewardTypeTooltip.SetString("UI_TOOLTIP_SCAVENGE_PERK_REWARD");
            RewardTypeImage.sprite = RewardRecurringSprite;
            RewardTypeText.SetText(Data.recurrenceInGameDays.ToString());
            // var stringResult = Utils.ReplaceValuePlaceholderInStringWithValues(Utils.DescriptionsMetadata.GetUIMetadata("UI_TOOLTIP_RECURRING_PERK_REWARD").title.EN, new int[] { Data.recurrenceInGameDays });

            //    RewardTypeText.SetText("Reward can be collected every <color=\"yellow\">" + Data.recurrenceInGameDays + "</color> game days. Recurring reward.");
        }
        else if (Data.rewardAfterSpecificGameDay > 0)
        {

            RewardTypeText.gameObject.SetActive(true);
            if (AccountDataSO.GlobalMetadata.gameDay <= Data.rewardAfterSpecificGameDay)
            {
                RewardTypeTooltip.SetString("UI_TOOLTIP_AFTER_GAMEDAY_PERK_REWARD", new int[] { Data.rewardAfterSpecificGameDay, (Data.rewardAfterSpecificGameDay - AccountDataSO.GlobalMetadata.gameDay) });
                RewardTypeImage.sprite = RewardGameDaySprite;
                RewardTypeText.SetText(Data.rewardAfterSpecificGameDay.ToString());
            }
            else
            {

                RewardTypeTooltip.SetString("UI_TOOLTIP_AFTER_GAMEDAY_PERK_REWARD_COMPLETED", new int[] { Data.rewardAfterSpecificGameDay });
                RewardTypeImage.sprite = RewardGameDaySprite;
                RewardTypeText.SetText(Data.rewardAfterSpecificGameDay.ToString());
            }
        }
        else if (Data.rewardAtSpecificGameDay > 0)
        {
            if (AccountDataSO.GlobalMetadata.gameDay <= Data.rewardAtSpecificGameDay)
                RewardTypeText.SetText("Reward can be collected only at <color=\"yellow\">" + Data.rewardAtSpecificGameDay + "</color>th game day (" + (Data.rewardAtSpecificGameDay - AccountDataSO.GlobalMetadata.gameDay) + "game days left)");
            else
            {
                RewardTypeText.SetText("<color=\"red\">Reward can be collected only at " + Data.rewardAtSpecificGameDay + "th game day. Already Expired!</color>");
                ClaimButton.GetComponent<Button>().interactable = false;
            }
        }



        ChargesText.gameObject.SetActive(!Data.isInstantReward && Data.charges > 0);

        if (!Data.isInstantReward && Data.recurrenceInGameDays != -1)
        {
            if (Data.charges == -1)
            {
                ChargesText.SetText("lasts for whole season");
            }
            else if (Data.charges > 0)
            {
                ChargesText.SetText("Has <color=\"yellow\">" + Data.charges + "</color> charges");
            }
        }

        StockLeftText.gameObject.SetActive(Data.stockLeft > -1);
        //if (Data.stockLeft == -1)
        //    StockLeftText.SetText("Unlimited stock");
        //else
        StockLeftText.SetText("Stock Left : " + Data.StockRemaining().ToString() + "/" + Data.stockLeft.ToString());

        OutOfStockGO.SetActive(Data.StockRemaining() == 0);
        if (Data.StockRemaining() == 0)
            StockLeftText.color = Color.red;

        //foreach (var item in Data.rewardsRecurring)
        //{
        //    var content = PrefabFactory.CreateGameObject<UIContentItem>(ContentContainerPrefab, RewardsRecrringParent);
        //    content.SetData(item);
        //    content.OnClicked += OnContentClicked;
        //}

        //ClaimButton.SetActive(AccountDataSO.CharacterData.IsQuestCompleted(Data));

    }

    //private void OnRewardRandomEquipClicked(UIRandomEquip _item)
    //{
    //    UIManager.instance.SpawnPromptPanel("You will get random " + Utils.ColorizeGivenText(_item.Data.rarity, Utils.GetRarityColor(_item.Data.rarity)) + " " + _item.Data.equipSlotId, "Random loot", null, null).HideDeclineButton();
    //}


    //private void OnContentClicked(UIContentItem _item)
    //{
    //    //UIContentContainerDetail.Show(_item.GetData());
    //    OnContentItemClicked.Invoke(_item);
    //}


    // Start is called before the first frame update
    public void ChooseClicked()
    {
        OnChooseClicked?.Invoke(this);
        // FirebaseCloudFunctionSO.ClaimQuestgiverReward(Data.id);
        // Hide();
    }




}
