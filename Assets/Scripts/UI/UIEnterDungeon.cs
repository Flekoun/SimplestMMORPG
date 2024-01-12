using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIEnterDungeon : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    public FirebaseCloudFunctionSO FirebaseCloudFunctionSO;
    public PrefabFactory PrefabFactory;


    public GameObject DungeonRewardsPanelGO;
    public GameObject DungeonFloorsPanelGO;

    public TextMeshProUGUI PlayersEnteredText;
    public TextMeshProUGUI InfoText;
    public TextMeshProUGUI FloorReachedText;
    public TextMeshProUGUI LevelReqText;

    public Button ShowLeaderboardsButton;
    public Button EnterDungeonButton;
    public Button ExploreDungeonButton;

    public UISpawnGOCount UISpawnGOCount_DungeonFloors;
    public TextMeshProUGUI DungeonNameText;
    public TextMeshProUGUI PlayerCountText;
    public Transform DungeonRewardParent;
    public GameObject ContentContainerPrefab;
    public GameObject UIRandomEquipPrefab;

    public GameObject Model;

    public void Awake()
    {

    }

    public void Refresh()
    {
        if (AccountDataSO.PointOfInterestData.dungeon == null)
            return;

        LevelReqText.gameObject.SetActive(AccountDataSO.PointOfInterestData.dungeon.characterLevelMax > -1);
        LevelReqText.color = Color.gray;
        DungeonNameText.SetText(AccountDataSO.PointOfInterestData.typeId);

        PlayerCountText.SetText("" + AccountDataSO.PointOfInterestData.dungeon.partySize.ToString() + "-hero dungeon ");
        LevelReqText.SetText("Hero level " + AccountDataSO.PointOfInterestData.dungeon.characterLevelMin + " - " + AccountDataSO.PointOfInterestData.dungeon.characterLevelMax + " required");
        EnterDungeonButton.interactable = !AccountDataSO.CharacterData.HasFinishedDungeon(AccountDataSO.PointOfInterestData.typeId);

        ExploreDungeonButton.gameObject.SetActive(AccountDataSO.IsDungeonOpen());
        EnterDungeonButton.gameObject.SetActive(AccountDataSO.IsInParty() && !AccountDataSO.PartyData.HasPartyMemberGaveConsentToEnterDungeon(AccountDataSO.PointOfInterestData.typeId, AccountDataSO.CharacterData.uid));
        ShowLeaderboardsButton.gameObject.SetActive(AccountDataSO.PointOfInterestData.dungeon.isEndlessDungeon);
        DungeonRewardsPanelGO.gameObject.SetActive(!AccountDataSO.PointOfInterestData.dungeon.isEndlessDungeon);
        FloorReachedText.gameObject.SetActive(AccountDataSO.PointOfInterestData.dungeon.isEndlessDungeon);



        if (!AccountDataSO.IsInParty())
        {
            PlayersEnteredText.SetText("You need to in party of " + AccountDataSO.PointOfInterestData.dungeon.partySize.ToString() + " heroes to join this dungeon");
            PlayersEnteredText.color = Color.red;


        }
        else if (AccountDataSO.PartyData.GetNumberOfConsentsToEnterDungeon(AccountDataSO.PointOfInterestData.typeId) < AccountDataSO.PointOfInterestData.dungeon.partySize)
        {
            PlayersEnteredText.SetText("Waiting for heroes to enter dungeon " + AccountDataSO.PartyData.GetNumberOfConsentsToEnterDungeon(AccountDataSO.PointOfInterestData.typeId) + "/" + AccountDataSO.PointOfInterestData.dungeon.partySize);
            PlayersEnteredText.color = Color.yellow;
            // EnterDungeonButton.gameObject.SetActive(true);

        }
        else
        {

            PlayersEnteredText.SetText("Dungeon opened. You can now start exploring!");
            PlayersEnteredText.color = Color.green;
        }

        //}





        int tiersReached = 0;
        UISpawnGOCount_DungeonFloors.Spawn(tiersReached, 0, true); //jen abych to vymazal

        if (AccountDataSO.PartyData != null && AccountDataSO.PartyData.dungeonProgress != null)
            tiersReached = AccountDataSO.PartyData.dungeonProgress.tierReached;

        if (!AccountDataSO.PointOfInterestData.dungeon.isEndlessDungeon)
        {
            UISpawnGOCount_DungeonFloors.Spawn(tiersReached, 1, true);
            UISpawnGOCount_DungeonFloors.Spawn(AccountDataSO.PointOfInterestData.dungeon.floorsTotal - tiersReached, 0, false);
        }
        else
            FloorReachedText.SetText(tiersReached + " floors reached!");




        Utils.DestroyAllChildren(DungeonRewardParent);

        foreach (var item in AccountDataSO.PointOfInterestData.dungeon.rewards)
        {
            var content = PrefabFactory.CreateGameObject<UIContentItem>(ContentContainerPrefab, DungeonRewardParent);
            content.SetData(item.GetContent(), true);
            // content.OnClicked += OnContentClicked;
        }

        foreach (var item in AccountDataSO.PointOfInterestData.dungeon.rewardsRandomEquip)
        {
            var equip = PrefabFactory.CreateGameObject<UIRandomEquip>(UIRandomEquipPrefab, DungeonRewardParent);
            equip.SetData(item);
            // equip.OnClicked += OnRewardRandomEquipClicked;
        }


        foreach (var item in AccountDataSO.PointOfInterestData.dungeon.rewardsGenerated)
        {
            var content = PrefabFactory.CreateGameObject<UIContentItem>(ContentContainerPrefab, DungeonRewardParent);
            content.SetData(item, true);
            // content.OnClicked += OnContentClicked;
        }

        if (AccountDataSO.PointOfInterestData.dungeon.isEndlessDungeon)
        {
            PlayerCountText.SetText("Endless " + AccountDataSO.PointOfInterestData.dungeon.partySize.ToString() + "-hero trial ");
            InfoText.SetText("You have single try to venture as deep as you can. You will get rewarded at the end of season based on your placement in leaderboards!");
        }
        else
        {
            PlayerCountText.SetText("Treasure " + AccountDataSO.PointOfInterestData.dungeon.partySize.ToString() + "-hero dungeon ");
            InfoText.SetText("Only when you reach and defeat final boss. You will get a reward.");
        }

        if (AccountDataSO.PointOfInterestData.dungeon.isFinalDungeon)
        {
            PlayerCountText.SetText("Final " + AccountDataSO.PointOfInterestData.dungeon.partySize.ToString() + "-hero trial ");
            InfoText.SetText(InfoText.text + "\n<color=\"yellow\">After final trial, YOUR CHARACTER WILL RETIRE!</color>");
        }


        if (AccountDataSO.CharacterData.HasFinishedDungeon(AccountDataSO.PointOfInterestData.typeId))
        {
            InfoText.SetText("<color=\"red\">You have already finished this dungeon. You cant enter again!</color>");
            DungeonFloorsPanelGO.gameObject.SetActive(false);
        }

        if (AccountDataSO.PointOfInterestData.dungeon.characterLevelMax > -1)
        {
            if (AccountDataSO.CharacterData.stats.level > AccountDataSO.PointOfInterestData.dungeon.characterLevelMax || AccountDataSO.CharacterData.stats.level < AccountDataSO.PointOfInterestData.dungeon.characterLevelMin)
            {
                EnterDungeonButton.interactable = false;
                LevelReqText.color = Color.red;
            }
        }

    }


    //private void OnRewardRandomEquipClicked(UIRandomEquip _item)
    //{
    //    // UIManager.instance.SpawnPromptPanel("You will get random " + Utils.ColorizeGivenText(_item.Data.rarity, Utils.GetRarityColor(_item.Data.rarity)) + " " + _item.Data.equipSlotId, "Random loot", null, null).HideDeclineButton();
    //}


    //private void OnContentClicked(UIContentItem _item)
    //{
    //    //   UIManager.instance.ContextInfoPanel.ShowContentContainerDetail(_item.Data);
    //}

    public void Show()
    {
        //Show();
        AccountDataSO.OnPartyDataChanged += Refresh;
        Model.gameObject.SetActive(true);
        Refresh();

        //else
        //{

        //    // if (AccountDataSO.PartyData.dungeonProgress == null)
        //    //  {
        //    if (AccountDataSO.PartyData.partyLeaderUid == AccountDataSO.CharacterData.uid)
        //    {
        //        if (AccountDataSO.PartyData.AreAllPartyMembersOnSameLocation(AccountDataSO.CharacterData.position.locationId))
        //        {
        //            InfoText.SetText("You can enter the dungeon!");
        //            InfoText.color = Color.green;
        //            EnterDungeonButton.interactable = true;
        //        }
        //        else
        //        {
        //            InfoText.SetText("All party members must be on dungeon location before you can enter!");
        //            InfoText.color = Color.red;
        //            EnterDungeonButton.interactable = false;
        //        }
        //    }
        //    else
        //    {
        //        InfoText.SetText("Only party leader can order to enter the dungeon!");
        //        InfoText.color = Color.red;
        //        EnterDungeonButton.interactable = false;
        //    }
        //    //  }
        //    //   else
        //    //   {
        //    /////       Debug.Log("HIDUJU!!!!!!!!!!!!!!!");
        //    //       Hide();
        //    //   }
        //}

        //}
        //else
        //    Hide();
    }

    //public void Show()
    //{
    //    Model.gameObject.SetActive(true);
    //}

    public void Hide()
    {
        AccountDataSO.OnPartyDataChanged -= Refresh;
        Model.gameObject.SetActive(false);
    }


    public async void EnterDungeon()
    {
        if (AccountDataSO.PointOfInterestData.dungeon.isFinalDungeon)
        {
            UIManager.instance.SpawnPromptPanel("Do you realy want to enter the final dungeon trial? You wont be able to leave the dungeon or party until the trial is over. Once the trial is over your character will become unplayable, you will be able to create a new character.", async () =>
            {
                var result = await FirebaseCloudFunctionSO.EnterDungeon();

                if (result.Result)
                {
                    UIManager.instance.ImportantMessage.ShowMesssage("Final trial has begun!");
                }
            }, null);
        }

        else if (AccountDataSO.PointOfInterestData.dungeon.isEndlessDungeon)
        {
            UIManager.instance.SpawnPromptPanel("Do you realy want to enter the dungeon trial? You wont be able to leave the dungeon or party until the trial is over. You have only one try for the trial!", async () =>
             {
                 var result = await FirebaseCloudFunctionSO.EnterDungeon();

                 if (result.Result)
                 {
                     UIManager.instance.ImportantMessage.ShowMesssage("You have entered the dungeon!");
                 }
             }, null);
        }
        else
        {
            UIManager.instance.SpawnPromptPanel("Do you realy want to enter the dungeon?", async () =>
            {
                var result = await FirebaseCloudFunctionSO.EnterDungeon();

                if (result.Result)
                {
                    UIManager.instance.ImportantMessage.ShowMesssage("You have entered the dungeon!");
                }
            }, null);
        }



        // Hide();
    }

    public async void ExploreDungeon()
    {
        var result = await FirebaseCloudFunctionSO.ExploreDungeon();

        if (result.Result)
        {
            UIManager.instance.ImportantMessage.ShowMesssage("New floor explored!");
            Hide();

        }

        // Hide();
    }

    public void ExitDungeon()
    {
        FirebaseCloudFunctionSO.ExitDungeon();
        Hide();
    }


    public void ShowLeaderboards()
    {
        UIManager.instance.UILeaderboardsPanel.ShowLeaderboard(AccountDataSO.PointOfInterestData.typeId);
    }
}
