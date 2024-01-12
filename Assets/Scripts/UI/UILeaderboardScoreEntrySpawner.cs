using System.Collections;
using System.Collections.Generic;
using simplestmmorpg.data;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class UILeaderboardScoreEntrySpawner : MonoBehaviour
{
    public PrefabFactory PrefabFactory;
    public AccountDataSO AccountDataSO;
    public Transform Parent;
    public GameObject CharacterListEntryPrefab;
    public GameObject RankRewardEntryPrefab;
    public GameObject MoreDataPrefab;
    public UnityAction<UILeaderboardScoreEntry> OnEntryClicked;
    //public UnityAction<UILeaderboardRankRewardEntry> OnRankRewardClicked;
    public UnityAction OnMoreDataClicked;

    private bool moreButtonWasSpawnedLastTime = false;

    private int LastRewardForRankShown_Max = 0;

    // Start is called before the first frame update
    public void Spawn(List<LeaderboardScoreEntry> _data, LeaderboardBaseData _baseData, int _startFrom, bool _destroyOldEntries = true)
    {
        if (_destroyOldEntries)
        {
            Utils.DestroyAllChildren(Parent);
            LastRewardForRankShown_Max = 0;
        }
        else if (moreButtonWasSpawnedLastTime)
            Destroy(Parent.GetChild(Parent.childCount - 1).gameObject);

        int rank = 0;
        foreach (var character in _data)
        {
            rank++;

            if (LastRewardForRankShown_Max < rank)
            {
                LeaderboardReward reward = _baseData.GetRewardForRank(rank);
                if (reward != null)
                {
                    var rankReward = PrefabFactory.CreateGameObject<UILeaderboardRankRewardEntry>(RankRewardEntryPrefab, Parent);
                    LastRewardForRankShown_Max = reward.rankMax;
                    rankReward.SetData(reward);
                }
            }

            var charPrev = PrefabFactory.CreateGameObject<UILeaderboardScoreEntry>(CharacterListEntryPrefab, Parent);
            charPrev.SetData(character, _startFrom + rank);
            charPrev.OnClicked += OnLeaderboardEntryClicked;
        }


        var button = PrefabFactory.CreateGameObject<Button>(MoreDataPrefab, Parent);
        button.onClick.AddListener(OnMoreDataClicked);
        moreButtonWasSpawnedLastTime = true;
    }


    private void OnLeaderboardEntryClicked(UILeaderboardScoreEntry _entry)
    {
        OnEntryClicked?.Invoke(_entry);
    }


}
