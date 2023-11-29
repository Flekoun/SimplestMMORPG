using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using simplestmmorpg.data;
using UnityEngine.Events;

public class UILeaderboardRankRewardEntry : MonoBehaviour
{

    public PrefabFactory PrefabFactory;
    public TextMeshProUGUI RankText;
    public Transform RewardsParent;
    //  public GameObject UIRandomEquipPrefab;
    public GameObject ContentContainerPrefab;
    // public UnityAction<UILeaderboardRankRewardEntry> OnClicked;
    public LeaderboardReward Data;



    //public void SelectButtonClicked()
    //{
    //    if (OnClicked != null)
    //        OnClicked.Invoke(this);
    //}

    public void Awake()
    {
    }


    public void SetData(LeaderboardReward _data)
    {
        Data = _data;

        RankText.SetText("Rank " + Data.rankMin.ToString() + " - " + Data.rankMax.ToString());


        Utils.DestroyAllChildren(RewardsParent);

        if (Data.content != null)
        {

            foreach (var item in Data.content)
            {
                var content = PrefabFactory.CreateGameObject<UIContentItem>(ContentContainerPrefab, RewardsParent);
                content.SetData(item.content, true);
                //  content.OnClicked += OnRewardClicked;

            }
        }

        if (Data.randomEquip != null)
        {
            foreach (var item in Data.randomEquip)
            {
                //      var equip = PrefabFactory.CreateGameObject<UIRandomEquip>(UIRandomEquipPrefab, RewardsParent);
                var content = PrefabFactory.CreateGameObject<UIContentItem>(ContentContainerPrefab, RewardsParent);
                content.SetData(item, true);
            }
        }

        //PROTOZE generateContent prasacky slouzi serveru jako referencni hodnoty, ktere se v AdminTools rozkopirujou na content a pripadne pozmeni....udeluje se jen Content potom. Generated vubec
        //if (Data.generatedContent != null)
        //{
        //    foreach (var item in Data.generatedContent)
        //    {
        //        var equip = PrefabFactory.CreateGameObject<UIContentItem>(ContentContainerPrefab, RewardsParent);
        //        equip.SetData(item);
        //        equip.OnClicked += OnRewardClicked;
        //    }

        //}
    }

    //private void OnRewardClicked(UIContentItem _item)
    //{
    //    UIManager.instance.ContextInfoPanel.ShowContentContainerDetail(_item.Data);
    //}



}