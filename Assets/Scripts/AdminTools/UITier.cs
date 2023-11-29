using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using simplestmmorpg.adminToolsData;
using System.Security.Policy;
using UnityEngine.Events;
using System;

public class UITier : MonoBehaviour
{
    public ImageIdDefinitionSOSet AllImageIdDefinitionSOSet;
    public PrefabFactory PrefabFactory;
    public GameObject UIPerkOfferPrefab;

    public UIItemIdChooser UIEnemyIdChooser;
    public GameObject EnemyPotraitPrefab;
    public Transform EnemyParent;
    public Transform Parent;

    public UnityAction<UITier> OnRemoveTierClicked;
    // private List<UIPerkOfferAdmin> List = new List<UIPerkOfferAdmin>();
    public TierMonstersDefinition Data;

    public void Awake()
    {
        UIEnemyIdChooser.OnItemsToAddSelected += OnAddEnemyClicked;
    }

    // Start is called before the first frame update
    public void Setup(TierMonstersDefinition _item)
    {
        Data = _item;
        Refresh();
    }

    private void Refresh()
    {
        Utils.DestroyAllChildren(EnemyParent);
        foreach (var enemyId in Data.enemies)
        {
            var UIItem = PrefabFactory.CreateGameObject<UIPortrait>(EnemyPotraitPrefab, EnemyParent);
            UIItem.SetPortrait(enemyId, "");
            UIItem.EnableAsButton();
            UIItem.OnClicked += OnEnemyPortraitClicked;
        }
        //   EnemyPortrait.sprite = AllImageIdDefinitionSOSet.GetDefinitionById(Utils.DescriptionsMetadata.GetEnemyMetadata(_item.enemyId).imageId).Image;

        Utils.DestroyAllChildren(Parent);
        //List.Clear();
        foreach (var perkOffer in Data.perkOffers)
        {
            var UIItem = PrefabFactory.CreateGameObject<UIPerkOfferAdmin>(UIPerkOfferPrefab, Parent);

            UIItem.Setup(perkOffer, false);
            UIItem.OnRemoveThisPerk += OnPerkRemoveClicked;
            //  List.Add(UIItem);
        }


    }

    public void AddPerkClicked()
    {
        //PerkOfferDefinitionAdmin newPerk = new PerkOfferDefinitionAdmin();
        //newPerk.charges = -1;
        //newPerk.curseCount = 0;
        //newPerk.isInstantReward = true;
        //newPerk.rarity = RARITY.COMMON;
        //newPerk.recurrenceInGameDays = -1;
        //newPerk.restictionProfession = new List<simplestmmorpg.data.SimpleTally>();
        //newPerk.restrictionClass = new List<simplestmmorpg.data.SimpleTally>();
        //newPerk.rewardAfterSpecificGameDay = -1;
        //newPerk.rewardAtSpecificGameDay = -1;
        //newPerk.rewards = new List<simplestmmorpg.data.ContentContainer>();
        //newPerk.rewardsGenerated = new List<ItemIdWithAmountAdmin>();
        //newPerk.rewardsRandomEquip = new List<simplestmmorpg.data.RandomEquip>();
        //newPerk.specialEffectId = new List<simplestmmorpg.data.SimpleTally>();
        //newPerk.stockLeft = -1;
        //newPerk.stockClaimed = 0;
        //newPerk.timePrice = 1;
        //newPerk.uid = System.Guid.NewGuid().ToString();
        //newPerk.chanceToSpawn = 1;

        Data.perkOffers.Add(PerkOfferDefinitionAdmin.FactoryNewPerk());

        Refresh();
    }


    public void AddEnemyClicked()
    {
        UIEnemyIdChooser.Show();
    }

    public void OnAddEnemyClicked(List<UISelectableEntry> _enemies)
    {
        foreach (var item in _enemies)
        {
            Data.enemies.Add(item.GetUid());

        }

        Refresh();
    }


    public void OnPerkRemoveClicked(UIPerkOfferAdmin _item)
    {
        Data.perkOffers.Remove(_item.Data);
        Refresh();
    }
    public void OnEnemyPortraitClicked(UIPortrait _item)
    {
        Data.enemies.Remove(_item.portraitId);
        Refresh();
    }

    public void RemoveThisClicked()
    {
        OnRemoveTierClicked?.Invoke(this);
    }


}
