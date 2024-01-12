using System.Collections;
using System.Collections.Generic;
using simplestmmorpg.data;
using UnityEngine;
using TMPro;
using UnityEngine.SocialPlatforms.Impl;
using Unity.VisualScripting;

public class UILeaderboards : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    public QueryData QueryData;
    //public UICharacterPreviewSpawner UICharacterPreviewSpawner;
    public UILeaderboardScoreEntrySpawner UILeaderboardScoreEntrySpawner;
    public UICharacterEquipPanel UICharacterEquipPanel;
    public TextMeshProUGUI TitleText;
    public GameObject LoadingGO;
    public TextMeshProUGUI TimeToResetText;
    public GameObject Model;
    public Transform LeaderboardEntiesContent;
    public UILeaderboardScoreEntry MyLeaderboardScoreEntry;
    private int Page = 0;
    private string LeaderboardId;
    private LeaderboardBaseData LeaderboardBaseData;
    private Coroutine TimeToResetCoroutine;
    public void Awake()
    {
        //UICharacterPreviewSpawner.OnEntryClicked += OnEntryClicked;
        //UICharacterPreviewSpawner.OnMoreDataClicked += FetchNextPageLevels;

        UILeaderboardScoreEntrySpawner.OnEntryClicked += OnEntryClicked;
        UILeaderboardScoreEntrySpawner.OnMoreDataClicked += FetchNextPage;
    }



    private async void FetchNextPage()
    {
        Page++;
        LoadingGO.SetActive(true);
        List<LeaderboardScoreEntry> list = await QueryData.GetLeaderboardEntries(LeaderboardId, true);
        UILeaderboardScoreEntrySpawner.Spawn(list, LeaderboardBaseData, AccountDataSO.OtherMetadataData.leaderboardsPageSize * Page, false);
        LoadingGO.SetActive(false);
    }

    private async void FetchFirstData()
    {
        LeaderboardBaseData = await QueryData.GetLeaderboardBaseData(LeaderboardId);

        if (Utils.DescriptionsMetadata.GetLeaderboardScoreTypeMetadata(LeaderboardBaseData.scoreType) != null)
            TitleText.SetText(Utils.DescriptionsMetadata.GetLeaderboardScoreTypeMetadata(LeaderboardBaseData.scoreType).title.GetText());
        else
            TitleText.SetText("UNKNOWN TITLE");

        if (LeaderboardBaseData == null)
            return;

        LeaderboardScoreEntry myLeaderboardEntry = await QueryData.GetMyLeaderboardEntry(LeaderboardId);

        MyLeaderboardScoreEntry.gameObject.SetActive(myLeaderboardEntry != null);

        if (myLeaderboardEntry != null)
            MyLeaderboardScoreEntry.SetData(myLeaderboardEntry, -1);

        List<LeaderboardScoreEntry> list = await QueryData.GetLeaderboardEntries(LeaderboardId);
        UILeaderboardScoreEntrySpawner.Spawn(list, LeaderboardBaseData, 0, true);
        LoadingGO.SetActive(false);

        if (LeaderboardBaseData.resetInterval == "SEASONAL")
        {
            TimeToResetText.SetText("Seasonal rewards");
        }
        else
            TimeToResetCoroutine = StartCoroutine(RepeatEverySecond(LeaderboardBaseData));

    }

    //private async void FetchFirstDataKills()
    //{
    //    List<LeaderboardScoreEntry> list = await QueryData.GetMonsterKillsLeaderboard();
    //    UILeaderboardScoreEntrySpawner.Spawn(list, true);
    //}

    //private async void FetchNextPageKills()
    //{
    //    List<LeaderboardScoreEntry> list = await QueryData.GetMonsterKillsLeaderboard(true);
    //    UILeaderboardScoreEntrySpawner.Spawn(list, false);
    //}

    //private async void OnEntryClicked(UICharacterPreviewEntry _entry)
    //{
    //    var character = await QueryData.GetCharacterData(_entry.Data.uid);
    //    UICharacterEquipPanel.Show(character);
    //}

    private async void OnEntryClicked(UILeaderboardScoreEntry _entry)
    {
        LoadingGO.SetActive(true);
        var character = await QueryData.GetCharacterData(_entry.Data.characterUid);
        UICharacterEquipPanel.Show(character);
        LoadingGO.SetActive(false);
    }

    public void ShowLeaderboardLevels()
    {

        LeaderboardId = "CHARACTER_LEVEL";
        Refresh();
    }

    public void ShowLeaderboardKills()
    {
        LeaderboardId = "MONSTER_KILLS";
        Refresh();
    }

    public void ShowLeaderboardItemsCrafted()
    {
        LeaderboardId = "ITEMS_CRAFTED";
        Refresh();
    }

    public void ShowLeaderboardDamageDone()
    {
        LeaderboardId = "DAMAGE_DONE";
        Refresh();
    }

    public void ShowLeaderboardHealingDone()
    {
        LeaderboardId = "HEALING_DONE";
        Refresh();
    }

    public void ShowLeaderboardEndgameDungeon()
    {
        LeaderboardId = "DUNGEON_ENDGAME_1";
        Refresh();
    }

    public void ShowLeaderboard(string _leaderboardId)
    {
        LeaderboardId = _leaderboardId;

        Refresh();
    }

    public void ShowPointOfInterestLeaderboard(string _leaderboardId, QueryData _queryData)
    {
        QueryData = _queryData;
        LeaderboardId = _leaderboardId;

        Refresh();
    }

    public void Refresh()
    {
        MyLeaderboardScoreEntry.gameObject.SetActive(false);
        TitleText.SetText("Refreshing...");
        TimeToResetText.SetText("--:--");
        LoadingGO.SetActive(true);
        Model.gameObject.SetActive(true);
        FetchFirstData();

    }
    public void Hide()
    {
        if (TimeToResetCoroutine != null)
            StopCoroutine(TimeToResetCoroutine);

        Utils.DestroyAllChildren(LeaderboardEntiesContent);
        Page = 0;
        Model.gameObject.SetActive(false);
    }



    IEnumerator RepeatEverySecond(LeaderboardBaseData _data)
    {

        if (LeaderboardBaseData != null)
        {
            while (true)
            {
                Debug.Log("LeaderboardBaseData.timestampNextReset:" + LeaderboardBaseData.timestampNextReset);

                TimeToResetText.SetText("resets in " + Utils.ConvertTimestampToReadableString(LeaderboardBaseData.timestampNextReset));
                yield return new WaitForSecondsRealtime(1);
            }
        }
        else yield return new WaitForSecondsRealtime(1); ;
    }
}
