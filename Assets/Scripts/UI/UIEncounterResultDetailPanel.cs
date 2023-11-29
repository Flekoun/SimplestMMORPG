using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using simplestmmorpg.data;
using UnityEngine.UI;
using System;
using static UnityEngine.EventSystems.EventTrigger;

public class UIEncounterResultDetailPanel : UISelectableSpawner
{
    public AccountDataSO AccountDataSO;
    public FirebaseCloudFunctionSO FirebaseCloudFunctionSO;
    public PrefabFactory Factory;
    public GameObject Model;
    public EncounterResult Data;
    public Button ClaimRewardsButton;
    public Button ForceEndItemWantPhaseButton;
    public TextMeshProUGUI WaitingForPlayersToChooseWantedItemText;
    public TextMeshProUGUI WantItemTimerText;
    public TextMeshProUGUI FatiguePenaltyText;
    public UIEncountersResultSpawner UIEncounterResultSpawner;

    public UIContentContainerDetail UIContentContainerDetail;
    //public GameObject NeedthisItemButtonGO;

    public GameObject NoItemDropGO;
    public GameObject ChooseItemGO;
    public GameObject ChooseItemHintGO;

    public UIPriceLabel GoldReward;
    public TextMeshProUGUI XPText;

    public Transform EquipLootParent;
    public GameObject UIEquipPrefab;

    private bool IsShowing = false;
    public void Awake()
    {
        //  UIEncounterResultSpawner.OnUIEntryClicked += Show;
        UIContentContainerDetail.OnHideClicked += HideContentDetailClicked;
        UIContentContainerDetail.OnActionButtonClicked += OnActionButtonClicked;
        AccountDataSO.OnEncounterResultsDataChanged += ForceShowResult;
    }

    private void ForceShowResult()
    {
        if (IsShowing)
            return;

        for (int i = 0; i < AccountDataSO.EncounterResultsData.Count; i++)
        {
            Show(AccountDataSO.EncounterResultsData[i]);
        }
    }

    //public void OnDestroy()
    //{
    //    UIEncounterResultSpawner.OnUIEntryClicked -= Show;

    //}
    public void Show(EncounterResult _data)
    {
        IsShowing = true;
        Data = _data;

        AccountDataSO.OnEncounterResultsDataChanged += Refresh;
        Refresh();
        Model.SetActive(true);

    }

    public void Close()
    {
        IsShowing = false;
        AccountDataSO.OnEncounterResultsDataChanged -= Refresh;
        UIContentContainerDetail.Hide();
        Model.SetActive(false);
    }


    private void OnActionButtonClicked()
    {
        FirebaseCloudFunctionSO.SelectWantItemInEncounterResult(Data.uid, base.GetSelectedEntry().GetUid());
        ClearItemsSelected();

    }

    private void HideContentDetailClicked()
    {
        ClearItemsSelected();

    }


    private void Refresh()
    {

        if (!AccountDataSO.EncounterResultsData.Contains(Data)) //pokud neni muj encounter v EncounterListu, musim byt smaazany z databaze, asi encounter skoncil
        {
            Close();
        }

        GoldReward.SetPrice(Data.silver);
        XPText.SetText(Data.GetCombatantResultForUid(AccountDataSO.CharacterData.uid).expGainedEstimate.ToString());

        int deckShuffleCount = Data.GetCombatantResultForUid(AccountDataSO.CharacterData.uid).deckShuffleCount;

        FatiguePenaltyText.gameObject.SetActive(false);

        //  FatiguePenaltyText.SetText("You have suffered 1% Fatigue during fight");

        //if (deckShuffleCount == 0)
        //    FatiguePenaltyText.SetText("You have suffered no Fatigue during fight");
        //else
        //    FatiguePenaltyText.SetText("You have suffered " + deckShuffleCount  + "% Fatigue during fight ("+ deckShuffleCount+"x deck shuffled )");

        Utils.DestroyAllChildren(EquipLootParent);

        bool hasAnyItemDropped = false;
        foreach (var enemy in Data.enemies)
        {

            foreach (var equip in enemy.contentLoot)
            {
                var item = Factory.CreateGameObject<UIEncounterResultItem>(UIEquipPrefab, EquipLootParent);
                item.SetData(equip, Data, enemy);
                // if (Data.combatantsList.Count > 1) //vybirat jde jen kdyz nejsi sam 
                item.UIInventoryItem.OnClicked += OnResultItemClicked;


                hasAnyItemDropped = true;

            }
        }

        foreach (var bonusloot in Data.bonusLoot)
        {
            var item2 = Factory.CreateGameObject<UIEncounterResultItem>(UIEquipPrefab, EquipLootParent);
            item2.SetData(bonusloot, Data, null);
            item2.UIInventoryItem.OnClicked += OnResultItemClicked;
            hasAnyItemDropped = true;
        }

        foreach (var bonusloot in Data.dungeonLoot)
        {
            var item2 = Factory.CreateGameObject<UIEncounterResultItem>(UIEquipPrefab, EquipLootParent);
            item2.SetData(bonusloot, Data, null);
            item2.UIInventoryItem.OnClicked += OnResultItemClicked;
            hasAnyItemDropped = true;
        }



        NoItemDropGO.SetActive(!hasAnyItemDropped);

        //ukazu to vybirani a timer jen a pouze pokud vypadl nejaky item pro partu
        if (hasAnyItemDropped && Data.combatantsList.Count > 1)
        {
            ChooseItemGO.SetActive(true);
            ChooseItemHintGO.SetActive(true);

            WaitingForPlayersToChooseWantedItemText.SetText("Waiting for <color=\"yellow\">" + Data.combatantsWithUnchoosenWantedItemList.Count + "</color> heroes to desire loot");
            WaitingForPlayersToChooseWantedItemText.gameObject.SetActive(!Data.wantItemPhaseFinished);
            ClaimRewardsButton.gameObject.SetActive(Data.wantItemPhaseFinished);
            CancelInvoke();
            InvokeRepeating("RefreshTurnTimeLeft", 0f, 1f);

        }
        else
        {
            ChooseItemGO.SetActive(false);
            ChooseItemHintGO.SetActive(false);

            WaitingForPlayersToChooseWantedItemText.gameObject.SetActive(false);
            WantItemTimerText.gameObject.SetActive(false);
            ForceEndItemWantPhaseButton.gameObject.SetActive(false);

            ClaimRewardsButton.gameObject.SetActive(true);
        }
    }


    private void RefreshTurnTimeLeft()
    {
        WantItemTimerText.SetText(Data.GetWantTimerTimeLeftText());
        ForceEndItemWantPhaseButton.gameObject.SetActive(Data.GetWantTimerTimeLeft() <= 0 && !Data.wantItemPhaseFinished);

    }

    public void CollectRewardClicked()
    {
        FirebaseCloudFunctionSO.CollectEncounterResultReward(Data.uid);
    }

    public void ForceEndWantItemPhaseClicked()
    {
        FirebaseCloudFunctionSO.ForceEndWantItemPhase(Data.uid);
    }


    public void OnResultItemClicked(UIContentItem _item)
    {
        base.OnUISelectableItemClicked(_item);
        if (Data.combatantsList.Count > 1)
        {
            if (Data.combatantsWithUnchoosenWantedItemList.Contains(AccountDataSO.CharacterData.uid))
            {
                UIContentContainerDetail.ShowActionButton(true);


                if (IsAnyItemSelected())
                    UIContentContainerDetail.Show(_item.GetData());
                else
                    UIContentContainerDetail.Hide();
            }
        }
        else if (Data.combatantsList.Count == 1)
        {
            UIContentContainerDetail.ShowActionButton(false);
            UIContentContainerDetail.Show(_item.GetData());
        }
        //if (IsAnyItemSelected())
        //{
        //    UIContentContainerDetail.Show(_item.GetData());
        //   // NeedthisItemButtonGO.SetActive(true);
        //}
        //else
        //{
        //    UIContentContainerDetail.Hide();
        //  //  NeedthisItemButtonGO.SetActive(false);
        //}
        // FirebaseCloudFunctionSO.SelectWantItemInEncounterResult(Data.uid, _item.GetData().uid);
    }




    //public void NeedSelectedItem()
    //{
    //    FirebaseCloudFunctionSO.SelectWantItemInEncounterResult(Data.uid, base.GetSelectedEntry().GetUid());//_item.GetData().uid);


    //}

}


