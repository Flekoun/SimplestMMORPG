using System;
using System.Collections.Generic;
using simplestmmorpg.adminToolsData;
using simplestmmorpg.data;
using UnityEngine;

public class UIInternalDefinitionsPanel : MonoBehaviour
{
    [Header("General")]
    public FirebaseCloudFunctionSO_Admin FirebaseCloudFunctionSO_Admin;
    public AccountDataSO AccountDataSO;
    public ListenOnInternalDefinitions ListenOnInternalDefinitions;

    public PrefabFactory PrefabFactory;
    public GameObject DetailChooserGO;
    public GameObject DetailsGO;

    public GameObject MonstersSoloDetail;
    public GameObject DungeonDetail;
    public ContentFitterRefresh ContentFitterRefresh;

    private ACTIVE_DETAIL ActiveDetail = ACTIVE_DETAIL.NONE;
    private PointOfInterestInternalDefinition DataOfEditedInternalDefintionEntry = null;



    [Header("Monsters Solo")]
    public Transform TiersParent;
    public Transform ParentPerksRare;
    public Transform MonstersSoloButtonsParent;
    //   public GameObject TiersPanelGO;
    public GameObject UIPerkOfferPrefab;
    public GameObject UIInternalDefinitionMonstersButtonPrefab;
    public GameObject UITierPrefab;


    [Header("Dungeon")]
    public Transform DungeonParent;
    public GameObject UIInternalDefinitionDungeonPrefab;

    //private List<UITier> List = new List<UITier>();


    private enum ACTIVE_DETAIL
    {
        NONE,
        MONSTERS_SOLO,
        DUNGEON
    }

    public void Awake()
    {
        AdminToolsManager.instance.OnInternalDefinitionChanged += Refresh;

    }



    public void Show()
    {
        ListenOnInternalDefinitions.StartListening();

    }

    public void ShowDungeonDetail()
    {

        DetailChooserGO.gameObject.SetActive(false);
        DetailsGO.gameObject.SetActive(true);
        ActiveDetail = ACTIVE_DETAIL.DUNGEON;


        Utils.DestroyAllChildren(DungeonParent);

        foreach (var def in AdminToolsManager.instance.InternalDefinition.DUNGEON)//AdminToolsManager.instance.Tiers.tiers)
        {
            var UIItem = PrefabFactory.CreateGameObject<UIInternalDefinitionsDungeon>(UIInternalDefinitionDungeonPrefab, DungeonParent);
            UIItem.Setup(def);
            //UIItem.onm += OnRemoveTier;
        }


        ContentFitterRefresh.RefreshContentFitters();

        DungeonDetail.gameObject.SetActive(true);
    }



    public void ShowMonstersSoloDetail()
    {
        DetailChooserGO.gameObject.SetActive(false);
        DetailsGO.gameObject.SetActive(true);
        ActiveDetail = ACTIVE_DETAIL.MONSTERS_SOLO;


        Utils.DestroyAllChildren(TiersParent);
        Utils.DestroyAllChildren(MonstersSoloButtonsParent);
        Utils.DestroyAllChildren(ParentPerksRare);


        foreach (var poiDef in AdminToolsManager.instance.InternalDefinition.MONSTER_SOLO)//AdminToolsManager.instance.Tiers.tiers)
            SpawnMonstersButton(poiDef);

        if (DataOfEditedInternalDefintionEntry != null)
            SpawnChoosenMonstersDefinitions();

        ContentFitterRefresh.RefreshContentFitters();

        MonstersSoloDetail.gameObject.SetActive(true);
    }

    private void SpawnChoosenMonstersDefinitions()
    {

        // SpawnPoIButtons();

        foreach (var item in DataOfEditedInternalDefintionEntry.monsters.tiers)
        {
            var UIItem = PrefabFactory.CreateGameObject<UITier>(UITierPrefab, TiersParent);

            UIItem.Setup(item);
            UIItem.OnRemoveTierClicked += OnRemoveTier;
        }

        foreach (var item in AdminToolsManager.instance.InternalDefinition.GetRarePerksbyId(DataOfEditedInternalDefintionEntry.monsters.perkOffersRareId).perks)
        {
            var UIItem = PrefabFactory.CreateGameObject<UIPerkOfferAdmin>(UIPerkOfferPrefab, ParentPerksRare);

            UIItem.Setup(item, true);
            UIItem.OnRemoveThisPerk += OnRemoveRarePerk;
            UIItem.OnDuplicateThisPerk += OnDuplicateRarePerk;
        }
    }

    private void OnDuplicateRarePerk(UIPerkOfferAdmin arg0)
    {

        var perk = PerkOfferDefinitionAdmin.Duplicate(arg0.Data);
        AdminToolsManager.instance.InternalDefinition.GetRarePerksbyId(DataOfEditedInternalDefintionEntry.monsters.perkOffersRareId).perks.Add(perk);
        Refresh();

    }

    private void SpawnMonstersButton(PointOfInterestInternalDefinition poiDef)
    {
        var UIItem = PrefabFactory.CreateGameObject<UIInternalDefinitionsMonstersButton>(UIInternalDefinitionMonstersButtonPrefab, MonstersSoloButtonsParent);
        UIItem.Setup(poiDef);
        UIItem.OnClicked += OnMonstersButtonClicked;
    }

    private void OnMonstersButtonClicked(UIInternalDefinitionsMonstersButton _poiButton)
    {
        DataOfEditedInternalDefintionEntry = _poiButton.Data;
        Refresh();


    }


    private void Refresh()
    {


        if (ActiveDetail == ACTIVE_DETAIL.MONSTERS_SOLO)
        {
            ShowMonstersSoloDetail();
        }

        else if (ActiveDetail == ACTIVE_DETAIL.DUNGEON)
        {
            ShowDungeonDetail();
        }
        else
        {
            DetailChooserGO.gameObject.SetActive(true);
            DetailsGO.gameObject.SetActive(false);
        }




    }







    public void Hide()
    {
        ActiveDetail = ACTIVE_DETAIL.NONE;
        MonstersSoloDetail.gameObject.SetActive(false);
        DungeonDetail.gameObject.SetActive(false);
        DetailChooserGO.SetActive(false);
        DetailsGO.SetActive(false);
        ListenOnInternalDefinitions.StopListening();
    }

    public void SaveClicked()
    {

        FirebaseCloudFunctionSO_Admin.SaveInternalDefinitionsMapGenerator(AdminToolsManager.instance.InternalDefinition);

    }
    public void AdddTierClicked()
    {
        var newTier = new TierMonstersDefinition();
        newTier.enemies = new List<string>();
        newTier.entryTimePrice = 0;
        newTier.perkOffers = new List<PerkOfferDefinitionAdmin>();
        // AdminToolsManager.instance.Tiers.tiers.Add(newTier);
        DataOfEditedInternalDefintionEntry.monsters.tiers.Add(newTier);
        Refresh();
    }

    public void AdddRarePerkClicked()
    {
        var perk = PerkOfferDefinitionAdmin.FactoryNewPerk();
        perk.stockLeft = 10;
        perk.rarity = Utils.RARITY.UNCOMMON;
        perk.rarePerkGroupId = DataOfEditedInternalDefintionEntry.monsters.perkOffersRareId;
        AdminToolsManager.instance.InternalDefinition.GetRarePerksbyId(DataOfEditedInternalDefintionEntry.monsters.perkOffersRareId).perks.Add(perk);
        //  DataOfEditedPoIDefintion.perkOffersRare.Add(perk);
        Refresh();
    }

    private void OnRemoveTier(UITier _item)
    {
        DataOfEditedInternalDefintionEntry.monsters.tiers.Remove(_item.Data);
        //  AdminToolsManager.instance.Tiers.tiers.Remove(_item.Data);
        Refresh();
    }

    private void OnRemoveRarePerk(UIPerkOfferAdmin _item)
    {
        AdminToolsManager.instance.InternalDefinition.GetRarePerksbyId(DataOfEditedInternalDefintionEntry.monsters.perkOffersRareId).perks.Remove(_item.Data);
        //  DataOfEditedPoIDefintion.perkOffersRare.Remove(_item.Data);
        // AdminToolsManager.instance.Tiers.perkOffersRare.Remove(_item.Data);
        Refresh();
    }


    public void AdddPoIDefinitionClicked()
    {
        PointOfInterestInternalDefinition newDefinition = new PointOfInterestInternalDefinition();
        newDefinition.chanceToSpawn = 0.1f;
        newDefinition.chapelBless = new List<IdWithChance>();
        newDefinition.enemies = new List<IdWithChance>();
        newDefinition.floorMax = -1;
        newDefinition.floorMin = 0;
        newDefinition.id = "NEW_ITEM_PLACEHOLDER_ID";

        newDefinition.questgivers = new List<Questgiver>();
        //        newDefinition.rareEnemies = new List<RareEnemyTierDefinition>();
        newDefinition.specials = new List<string>();
        newDefinition.vendors = new List<Vendor>();
        newDefinition.monsters = new MonstersDefinition();
        newDefinition.monsters.perkOffersRareId = "PERKS_FLOOR_0";
        newDefinition.monsters.tiers = new List<TierMonstersDefinition>();
        newDefinition.monsters.partySize = 1;
        newDefinition.monsters.exploreTimePrice = 1;
        newDefinition.dungeon = null;

        AdminToolsManager.instance.InternalDefinition.MONSTER_SOLO.Add(newDefinition);


        Refresh();
    }
    //private void OnRemoveTier(UITier _item)
    //{

    //}
}
