using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using simplestmmorpg.data;
using System;
using UnityEngine.Events;
using System.Drawing;

public class UIPendingReward : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    public FirebaseCloudFunctionSO FirebaseCloudFunctionSO;
    public PrefabFactory PrefabFactory;

    public TextMeshProUGUI RewardTypeText;
    public UIPriceScavengePointsLabel UIPriceScavengePointsLabel;
    public UIPriceTimeLabel UIPriceScavengeTimePrice;


    public GameObject UIRandomEquipPrefab;
    public GameObject ContentContainerPrefab;

    public TextMeshProUGUI RecurrenceInGameDaysText;
    public GameObject RecurrenceInGameDaysGO;

    public GameObject ChargesGO;
    public GameObject ChargesInfiniteGO;
    public TextMeshProUGUI ChargesText;

    public Transform RewardsParent;

    public GameObject ClaimableGO;
    public Button ClaimButton;

    public PendingReward Data;

    // public UnityAction<UIPerkOffer> OnClicked;

    private bool isClaimable = false;
    //public UIQuestgiverSpawner UIQuestgiverSpawner;


    public void Awake()
    {
        //  UIQuestgiverSpawner.OnEntryClicked += OnQuestgiverEntryClicked;

    }

    //private void OnQuestgiverEntryClicked(UIPerkOffer _entry)
    //{
    //    Show(_entry.Data);
    //}

    public void Setup(PendingReward _data)
    {


        Data = _data;
        //   Model.SetActive(true);
        Refresh();
    }

    public bool IsClaimable()
    {
        return isClaimable;
    }

    public void Refresh()
    {

        // ClaimButton.GetComponent<Button>().interactable = !EncounterData.perkChoosen;



        Utils.DestroyAllChildren(RewardsParent);

        foreach (var item in Data.rewards)
        {
            var content = PrefabFactory.CreateGameObject<UIContentItem>(ContentContainerPrefab, RewardsParent);
            content.SetData(item.GetContent(), true);
        }

        foreach (var item in Data.rewardsGenerated)
        {
            var content = PrefabFactory.CreateGameObject<UIContentItem>(ContentContainerPrefab, RewardsParent);
            content.SetData(item, true);
        }

        foreach (var item in Data.rewardsRandomEquip)
        {
            var equip = PrefabFactory.CreateGameObject<UIRandomEquip>(UIRandomEquipPrefab, RewardsParent);
            equip.SetData(item);
            equip.OnClicked += OnRewardRandomEquipClicked;
        }

        RecurrenceInGameDaysGO.gameObject.SetActive(false);
        ChargesGO.gameObject.SetActive(true);
        RewardTypeText.SetText("");

        bool enoughtScavengePoints = AccountDataSO.CharacterData.currency.scavengePoints >= AccountDataSO.OtherMetadataData.constants.SCAVENGE_CLAIM_COST;
        bool enoughtTime = AccountDataSO.CharacterData.currency.time >= AccountDataSO.OtherMetadataData.constants.SCAVENGE_CLAIM_COST_TIME;



        ClaimButton.interactable = (enoughtTime || enoughtScavengePoints);
        UIPriceScavengePointsLabel.gameObject.SetActive(enoughtScavengePoints);
        UIPriceScavengeTimePrice.gameObject.SetActive(!enoughtScavengePoints);
        UIPriceScavengePointsLabel.SetPrice(AccountDataSO.OtherMetadataData.constants.SCAVENGE_CLAIM_COST);
        UIPriceScavengeTimePrice.SetPrice(AccountDataSO.OtherMetadataData.constants.SCAVENGE_CLAIM_COST_TIME);


        /*      if (Data.isInstantReward)
              {
                  //   RewardTypeText.SetText("Reward can be collected <color=\"yellow\">immediately</color> after victory");
                  isClaimable = true;
                  ClaimButton.gameObject.SetActive(true);
                  ClaimableGO.gameObject.SetActive(true);
              }
              else if (Data.recurrenceInGameDays > 0)
              {

                  isClaimable = AccountDataSO.CharacterData.IsPerkClaimable(AccountDataSO.GlobalMetadata.gameDay, this.Data);

                  if (isClaimable)
                      RewardTypeText.SetText("");
                  // RewardTypeText.SetText("Reward can be collected every <color=\"yellow\">" + Data.recurrenceInGameDays + "</color> game days.");
                  else
                      RewardTypeText.SetText("<color=\"red\">Available in " + ((Data.lastClaimGameDay + Data.recurrenceInGameDays) - AccountDataSO.GlobalMetadata.gameDay) + " game days </color>");
                  //  RewardTypeText.SetText("Reward can be collected every <color=\"yellow\">" + Data.recurrenceInGameDays + "</color> game days.\n<color=\"red\">" + ((Data.lastClaimGameDay + Data.recurrenceInGameDays) - AccountDataSO.GlobalMetadata.gameDay) + "</color> game day left");


                  ClaimButton.gameObject.SetActive(isClaimable);
                  ClaimableGO.gameObject.SetActive(isClaimable);

                  RecurrenceInGameDaysGO.gameObject.SetActive(true);
                  RecurrenceInGameDaysText.SetText(Data.recurrenceInGameDays + " Days");
              }
              else if (Data.rewardAfterSpecificGameDay > 0)
              {
                  RewardTypeText.SetText("Reward can be collected when season reaches <color=\"yellow\">" + Data.rewardAfterSpecificGameDay + "</color>. game days or more");

                  isClaimable = AccountDataSO.GlobalMetadata.gameDay >= Data.rewardAfterSpecificGameDay;

                  ClaimButton.gameObject.SetActive(isClaimable);
                  ClaimableGO.gameObject.SetActive(isClaimable);
              }
              else if (Data.rewardAtSpecificGameDay > 0)
              {
                  isClaimable = AccountDataSO.GlobalMetadata.gameDay == Data.rewardAtSpecificGameDay;

                  ClaimButton.gameObject.SetActive(isClaimable);
                  ClaimableGO.gameObject.SetActive(isClaimable);

                  RewardTypeText.SetText("Reward can be collected only at <color=\"yellow\">" + Data.rewardAfterSpecificGameDay + "</color>. game day of a season");
              }

              ChargesGO.gameObject.SetActive(!Data.isInstantReward);

              if (!Data.isInstantReward)
              {
                  if (Data.charges == -1)
                  {
                      // ChargesText.SetText("lasts for whole season");
                      ChargesInfiniteGO.gameObject.SetActive(true);
                      ChargesText.gameObject.SetActive(false);
                  }
              }
              else if (Data.charges > 0)
              {
                  ChargesText.SetText("can be collected < color =\"yellow\">" + Data.charges + "</color> times");
                  ChargesInfiniteGO.gameObject.SetActive(false);
                  ChargesText.gameObject.SetActive(true);
                  ChargesText.SetText(Data.charges - Data.chargesClaimed + " uses left");

              }

              //   StockLeftText.gameObject.SetActive(Data.stockLeft > -1);

              //foreach (var item in Data.rewardsRecurring)
              //{
              //    var content = PrefabFactory.CreateGameObject<UIContentItem>(ContentContainerPrefab, RewardsRecrringParent);
              //    content.SetData(item);
              //    content.OnClicked += OnContentClicked;
              //}

              //ClaimButton.SetActive(AccountDataSO.CharacterData.IsQuestCompleted(Data));
        */
    }

    private void OnRewardRandomEquipClicked(UIRandomEquip _item)
    {
        UIManager.instance.SpawnPromptPanel("You will get random " + Utils.ColorizeGivenText(_item.Data.rarity, Utils.GetRarityColor(_item.Data.rarity)) + " " + _item.Data.equipSlotId, "Random loot", null, null).HideDeclineButton();
    }





    // Start is called before the first frame update
    public async void ClaimClicked()
    {
        // OnClicked?.Invoke(this);
        var result = await FirebaseCloudFunctionSO.PendingRewardClaim(Data.uid);
        if (result.Result)
        {
            UIManager.instance.ImportantMessage.ShowMesssage("Loot Scavenged!");
        }
        // Hide();
    }




}
