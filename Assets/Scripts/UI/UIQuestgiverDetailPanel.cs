using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using simplestmmorpg.data;
using System;

public class UIQuestgiverDetailPanel : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    public FirebaseCloudFunctionSO FirebaseCloudFunctionSO;
    public PrefabFactory PrefabFactory;
    public TextMeshProUGUI DisplayNameText;
    public TextMeshProUGUI DescriptionText;
    public TextMeshProUGUI KillListText;
    public TextMeshProUGUI XPRewardText;
    public UIContentContainerDetail UIContentContainerDetail;
    //public UIInventory UIInventoryRewards;

    public GameObject UIRandomEquipPrefab;
    public GameObject ContentContainerPrefab;
    public Transform RewardsParent;
    public GameObject ClaimButton;
    public GameObject Model;
    public Questgiver Data;

    public UIQuestgiverSpawner UIQuestgiverSpawner;
    //  private List<string> equiToSellUids = new List<string>();
    // private List<string> equiToBuyUids = new List<string>();

    public void Awake()
    {
        UIQuestgiverSpawner.OnEntryClicked += OnQuestgiverEntryClicked;

    }

    private void OnQuestgiverEntryClicked(UIQuestgiverEntry _entry)
    {
        Show(_entry.Data);
    }

    public void Show(Questgiver _data)
    {

        Data = _data;
        Model.SetActive(true);
        Refresh();
    }

    public void Hide()
    {
        Model.SetActive(false);
    }

    public void Refresh()
    {
        string killsNeeded = "";
        foreach (var item in Data.killsRequired)
        {
            Color textColor;
            string textToAdd = "";
            if (AccountDataSO.CharacterData.GetKillsForEnemyId(item.id) >= item.count)
            {
                textToAdd = " (completed)";
                textColor = Color.gray;
            }
            else
                textColor = Color.yellow;

            killsNeeded = killsNeeded + Utils.ColorizeGivenText("<b>" + Utils.DescriptionsMetadata.GetEnemyMetadata(item.id).title.GetText() + "</b> slain : " + AccountDataSO.CharacterData.GetKillsForEnemyId(item.id) + "/" + item.count + textToAdd + "\n", textColor);
        }

        foreach (var item in Data.itemsRequired)
        {
            Color textColor;
            string textToAdd = "";
            if (AccountDataSO.CharacterData.inventory.GetAmountOfItemsInInventory(item.id) >= item.count)
            {
                textToAdd = " (completed)";
                textColor = Color.gray;
            }
            else
                textColor = Color.yellow;

            killsNeeded = killsNeeded + Utils.ColorizeGivenText("<b>" + Utils.DescriptionsMetadata.GetItemsMetadata(item.id).title.GetText() + "</b> : " + AccountDataSO.CharacterData.inventory.GetAmountOfItemsInInventory(item.id) + "/" + item.count + textToAdd + "\n", textColor);
        }

        KillListText.SetText(killsNeeded);
        // DescriptionText.SetText(String.Format(Utils.ReplacePlaceholdersInTextWithDescriptionFromMetadata(Utils.GetMetadataForQuest(Data.id).description.GetText()), "<b>" + AccountDataSO.CharacterData.characterName + "</b>"));
        DescriptionText.SetText(Utils.ReplacePlaceholdersInTextWithDescriptionFromMetadata(Utils.DescriptionsMetadata.GetQuestMetadata(Data.id).description.GetText()));
        //DescriptionText.SetText(Utils.GetMetadataForQuest(Data.id).description.GetText());
        DisplayNameText.SetText(Utils.DescriptionsMetadata.GetQuestMetadata(Data.id).title.GetText());
        XPRewardText.SetText((Data.expRewardPerLevel * Data.qLevel).ToString());
        UIContentContainerDetail.Hide();

        //List<ContentContainer> rewardsAsContent = new List<ContentContainer>();
        //foreach (var item in Data.rewards)
        //{
        //    if (item.characterClassIds.Contains(AccountDataSO.CharacterData.characterClass))
        //        rewardsAsContent.Add(item.content);
        //}

        //UIInventoryRewards.Refresh(rewardsAsContent);

        Utils.DestroyAllChildren(RewardsParent);

        foreach (var item in Data.rewards)
        {
            if (item.characterClassIds.Contains(AccountDataSO.CharacterData.characterClass) || item.characterClassIds.Contains(Utils.CHARACTER_CLASS.ANY))
            {
                var content = PrefabFactory.CreateGameObject<UIContentItem>(ContentContainerPrefab, RewardsParent);
                content.SetData(item.content.GetContent());
                content.OnClicked += OnContentClicked;
            }
        }

        foreach (var item in Data.rewardsRandomEquip)
        {
            var equip = PrefabFactory.CreateGameObject<UIRandomEquip>(UIRandomEquipPrefab, RewardsParent);
            equip.SetData(item);
            equip.OnClicked += OnRewardRandomEquipClicked;
        }


        if (Data.rewardsGenerated != null)
        {
            foreach (var item in Data.rewardsGenerated)
            {
                var content = PrefabFactory.CreateGameObject<UIContentItem>(ContentContainerPrefab, RewardsParent);
                content.SetData(item);
                content.OnClicked += OnContentClicked;
            }
        }

        ClaimButton.SetActive(AccountDataSO.CharacterData.IsQuestCompleted(Data));

    }

    private void OnRewardRandomEquipClicked(UIRandomEquip _item)
    {
        UIManager.instance.SpawnPromptPanel("You will get random " + Utils.ColorizeGivenText(_item.Data.rarity, Utils.GetRarityColor(_item.Data.rarity)) + " " + _item.Data.equipSlotId, "Random loot", null, null).HideDeclineButton();
    }


    private void OnContentClicked(UIContentItem _item)
    {
        UIContentContainerDetail.Show(_item.GetData());
    }


    // Start is called before the first frame update
    public void ClaimQuestgiverReward()
    {
        FirebaseCloudFunctionSO.ClaimQuestgiverReward(Data.id);
        Hide();
    }




}
