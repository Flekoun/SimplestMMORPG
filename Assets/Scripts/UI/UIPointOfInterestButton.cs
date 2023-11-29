using System;
using System.Collections;
using simplestmmorpg.data;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static Utils;

public class UIPointOfInterestButton : MonoBehaviour
{
    public ImageIdDefinitionSOSet AllImageIdDefinitionSOSet;
    public AccountDataSO AccountDataSO;
    public FirebaseCloudFunctionSO FirebaseCloudFunctionSO;
    public PrefabFactory PrefabFactory;
    //public PointOfInterestIdDefinitionSOSet AllPointOfInterestIdDefinitionSOSet;
    public Image PortraitImage;
    public Image RoomTypeImage;
    public TextMeshProUGUI NameText;
    // public TextMeshProUGUI TierText;
    public UIPriceTimeLabel TimePriceText;
    public Sprite MonsterProgressSprite;
    public Sprite MonsterProgressFinishedSprite;
    public Sprite MonsterProgressEliteSprite;
    public Sprite MonsterProgressFinishedEliteSprite;
    public UIQualityProgress UIMonsterTiersProgress; //Tiers
    public GameObject AmbushWarningGO;
    //    public TextMeshProUGUI CharacterLevelText;

    //   public UIPortrait PlayerPortrait;
    public Transform PartyMembersParent;
    public GameObject PortraitPrefab;
    // public GameObject InterestingStuffPrefab;
    public Transform InterestingStuffParent;

    public Transform EncountersMapParent;
    public Transform EncountersResultMapParent;
    //public UILineRenderer UILineRenderer;
    public WorldPosition WorldPosition;
    // public PointOfInterestIdDefinition pointOfInterestDefinition;
    public Sprite UnexploredSprite;
    public Sprite ExploredSpriteDefault;
    public Button ExploreButton;
    public GameObject Modelx;
    public HoldButton HoldButton;
    public GameObject NameTitle;
    public GameObject QuestGiverPrefab;
    public GameObject VendorMapPrefab;
    // public GameObject TrainerMapPrefab;
    public GameObject AuctionHousePrefab;
    public GameObject InboxPrefab;
    public GameObject BarberPrefab;
    public GameObject ForgePrefab;
    public GameObject InnPrefab;
    public GameObject ChapelPrefab;
    public GameObject TreasurePrefab;
    public GameObject DungeonEntrancePrefab;
    public GameObject DungeonExitPrefab;
    public GameObject EncounterEntryMapPrefab;
    public GameObject EncounterResultEntryMapPrefab;
    public GameObject WorldMapMemmoryEntryPrefab;

    public Color ColorActive;
    public Color ColorNormal;
    public Color ColorUnexplored;
    public Color ColorUnreachable;
    public Color ColorEnemy;
    //  public QueryData QueryData;

    public UnityAction<UIVendorEntry> OnVendorClicked;
    public UnityAction<UIQuestgiverEntry> OnQuestgiverClicked;
    public UnityAction<UITrainerEntry> OnTrainerClicked;
    public UnityAction<UIEncounterEntryMap> OnEncounterClicked;
    public UnityAction<UIEncounterResultEntry> OnEncounterResultClicked;
    public UnityAction<UIPointOfInterestButton> OnClicked;

    // Start is called before the first frame update
    private bool IsPlayerOnThisPoI = false;
    private BaseDescriptionMetadata_NoId metadata = null;
    private bool IsReachable = false;
    //public void Awake()
    //{
    //    AccountDataSO.OnCharacterDataChanged += Refresh;
    //    AccountDataSO.OnPartyDataChanged += Refresh;
    //}



    public void OnEnable()
    {
        AccountDataSO.OnCharacterDataChanged += Refresh;
        AccountDataSO.OnPartyDataChanged += Refresh;
        AccountDataSO.OnEncounterDataChanged += Refresh;
        AccountDataSO.OnRareEncounterDataChanged += Refresh;
        AccountDataSO.OnEncounterResultsDataChanged += RefreshEncounterResult;
    }

    public void OnDisable()
    {
        AccountDataSO.OnCharacterDataChanged -= Refresh;
        AccountDataSO.OnPartyDataChanged -= Refresh;
        AccountDataSO.OnEncounterDataChanged -= Refresh;
        AccountDataSO.OnRareEncounterDataChanged -= Refresh;
        AccountDataSO.OnEncounterResultsDataChanged -= RefreshEncounterResult;
    }


    public void SetData(WorldPosition _data)
    {
        //   Debug.Log("What the hack???");
        WorldPosition = _data;
        //  QueryData.Setup(WorldPosition.locationId, WorldPosition.pointOfInterestId);

        Refresh();
    }



    public void Show(bool _show)
    {
        //  Debug.Log("showing : " + this.Data.id + " " + _show);
        //if (!_show)
        //    Debug.Log("nicmene ted volam hide pro v show: " + this.WorldPosition.pointOfInterestId);
        //else
        //    Debug.Log("volam show pro v show: " + this.WorldPosition.pointOfInterestId);


        Modelx.SetActive(_show);//AccountDataSO.CharacterData.IsPositionExplored(Data));
    }

    private void OnQuestgiverEntryClicked(UIQuestgiverEntry _entry)
    {
        OnQuestgiverClicked?.Invoke(_entry);
    }


    private void OnTrainerEntryClicked(UITrainerEntry _entry)
    {
        OnTrainerClicked?.Invoke(_entry);
    }

    private void OnVendorEntryClicked(UIVendorEntry _entry)
    {
        OnVendorClicked?.Invoke(_entry);
    }

    private void OnEncounterEntryClicked(UIEncounterEntryMap _entry)
    {
        OnEncounterClicked?.Invoke(_entry);
    }
    private void OnEncounterResultEntryClicked(UIEncounterResultEntry _entry)
    {
        OnEncounterResultClicked?.Invoke(_entry);
    }


    private void Refresh()
    {
        if (this == null)
        {
            Debug.LogWarning("divne ze se toto deje");
            return;
        }

        if (AccountDataSO.LocationData.GetDijkstraMapVertexById(WorldPosition.pointOfInterestId) != null)
            this.transform.localPosition = AccountDataSO.LocationData.GetDijkstraMapVertexById(WorldPosition.pointOfInterestId).screenPosition.ToVector2(); //LocationDef.Position;



        //I am at this PoI?
        IsPlayerOnThisPoI = AccountDataSO.CharacterData.position.pointOfInterestId == WorldPosition.pointOfInterestId;//&& AccountDataSO.CharacterData.position.zoneId == ZoneDef.Id;


        Utils.DestroyAllChildren(InterestingStuffParent);

        metadata = Utils.DescriptionsMetadata.GetPointsOfInterestMetadata(WorldPosition.pointOfInterestId);


        //if (TimePriceText != null)
        //    TimePriceText.gameObject.SetActive(AccountDataSO.IsPositionExplored(WorldPosition) && IsPlayerOnThisPoI);
        TimePriceText.gameObject.SetActive(false);
        //if (AccountDataSO.PointOfInterestData.pointOfInterestType == (int)Utils.POI_TYPE.MONSTER_SOLO)//(AccountDataSO.PointOfInterestData.pointOfInterestType != (int)Utils.POI_TYPE.DUNGEON && AccountDataSO.PointOfInterestData.pointOfInterestType != (int)Utils.POI_TYPE.ENDGAME)
        //{
        if (IsPlayerOnThisPoI && AccountDataSO.PointOfInterestData.monsters != null && AccountDataSO.PointOfInterestData.monsters.tiersTotal > 0)
        {
            UIMonsterTiersProgress.gameObject.SetActive(true);

            if (AccountDataSO.PointOfInterestData.monsters.partySize > 1)
            {
                UIMonsterTiersProgress.Setup(AccountDataSO.CharacterData.GetMaxTierReachedForPointOfInterest(WorldPosition.pointOfInterestId) + 1, AccountDataSO.PointOfInterestData.monsters.tiersTotal, MonsterProgressFinishedEliteSprite, MonsterProgressEliteSprite);

            }
            else
            {
                UIMonsterTiersProgress.Setup(AccountDataSO.CharacterData.GetMaxTierReachedForPointOfInterest(WorldPosition.pointOfInterestId) + 1, AccountDataSO.PointOfInterestData.monsters.tiersTotal, MonsterProgressFinishedSprite, MonsterProgressSprite);

            }

        }
        else if (!IsPlayerOnThisPoI)
        {
            var memmory = AccountDataSO.CharacterData.GetWorldMapMemmoryForWorldPosition(WorldPosition);
            if (memmory != null)
            {
                UIMonsterTiersProgress.gameObject.SetActive(true);

                if (memmory.partySize > 1)
                {
                    UIMonsterTiersProgress.Setup(AccountDataSO.CharacterData.GetMaxTierReachedForPointOfInterest(WorldPosition.pointOfInterestId) + 1, memmory.tiersCount, MonsterProgressFinishedEliteSprite, MonsterProgressEliteSprite);

                }
                else
                {
                    UIMonsterTiersProgress.Setup(AccountDataSO.CharacterData.GetMaxTierReachedForPointOfInterest(WorldPosition.pointOfInterestId) + 1, memmory.tiersCount, MonsterProgressFinishedSprite, MonsterProgressSprite);

                }

                //if (memmory.tiersCount > 0)
                //{
                //    UIMonsterTiersProgress.gameObject.SetActive(true);
                //    UIMonsterTiersProgress.Setup(AccountDataSO.CharacterData.GetMaxTierReachedForPointOfInterest(WorldPosition.pointOfInterestId) + 1, memmory.tiersCount, MonsterProgressFinishedSprite, MonsterProgressSprite);
                //}
                //else
                //    UIMonsterTiersProgress.gameObject.SetActive(false);
            }
        }
        //}
        //else UIMonsterTiersProgress.gameObject.SetActive(false);
        //  TimePriceText.SetPrice(AccountDataSO.PointOfInterestData.exploreTimePrice);//+ AccountDataSO.CharacterData.GetMaxTierReachedForPointOfInterest(WorldPosition.pointOfInterestId) + 2));


        if (AccountDataSO.CharacterData.IsWorldPositionExplored(WorldPosition))//AccountDataSO.CharacterData.IsPositionExplored(Data.id))
        {

            //Debug.Log("WorldPosition: " + WorldPosition.pointOfInterestId);
            //Debug.Log("AccountDataSO.CharacterData.IsWorldPositionExplored(WorldPosition): " + AccountDataSO.CharacterData.IsWorldPositionExplored(WorldPosition));
            RoomTypeImage.gameObject.SetActive(false);
        }
        else
        {
            PortraitImage.sprite = UnexploredSprite;


            RoomTypeImage.gameObject.SetActive(true);

            if (AccountDataSO.LocationData.GetDijkstraMapVertexById(WorldPosition.pointOfInterestId) != null)
            {
                var poiDijkstraData = AccountDataSO.LocationData.GetDijkstraMapVertexById(WorldPosition.pointOfInterestId);
                RoomTypeImage.sprite = AllImageIdDefinitionSOSet.GetDefinitionById(Enum.GetName(typeof(ROOM_TYPE), poiDijkstraData.type), "POI_TYPE").Image;



            }




        }


        // this.gameObject.transform.localPosition = Data.screenPosition.ToVector2();// pointOfInterestDefinition.position;




        //Spawn party members portraits


        Utils.DestroyAllChildren(PartyMembersParent);
        if (AccountDataSO.IsInParty())
        {
            foreach (var member in AccountDataSO.PartyData.partyMembers)
            {
                if (member.position.pointOfInterestId == WorldPosition.pointOfInterestId)//&& member.position.zoneId == ZoneDef.Id)
                {
                    if (member.uid != AccountDataSO.CharacterData.uid)
                    {
                        var portrait = PrefabFactory.CreateGameObject<UIPortrait>(PortraitPrefab, PartyMembersParent);
                        portrait.SetPortrait(member.characterPortrait, member.characterClass);
                        portrait.SetName(member.displayName);
                    }
                }
            }
        }

        //spawn player portrait
        //I am at this PoI
        if (IsPlayerOnThisPoI)
        {
            //PlayerPortrait.SetPortrait(AccountDataSO.CharacterData.characterPortrait);
            //PlayerPortrait.SetName(AccountDataSO.CharacterData.characterName);
            //PlayerPortrait.gameObject.SetActive(true);
            var portrait = PrefabFactory.CreateGameObject<UIPortrait>(PortraitPrefab, PartyMembersParent);
            portrait.SetPortrait(AccountDataSO.CharacterData.characterPortrait, AccountDataSO.CharacterData.characterClass);
            portrait.SetName(AccountDataSO.CharacterData.characterName);

        }
        //else
        //    PlayerPortrait.gameObject.SetActive(false);

        //je prozkoumany PoI a jsem na nem, ukazu co o nem vim detailne, mam dokument ziskany 
        if (AccountDataSO.CharacterData.IsWorldPositionExplored(WorldPosition) && IsPlayerOnThisPoI)
        {
            ///   Utils.DestroyAllChildren(InterestingStuffParent);

            GameObject spawnedEntry = null;
            foreach (var item in AccountDataSO.PointOfInterestData.GetValidQuestGivers(AccountDataSO.CharacterData))
            {
                var entryUI = PrefabFactory.CreateGameObject<UIQuestgiverEntry>(QuestGiverPrefab, InterestingStuffParent);
                entryUI.SetData(item);
                entryUI.OnClicked += OnQuestgiverEntryClicked;
                //   entryUI.gameObject.GetComponentInChildren<UIInterestMapButton>().SetEnabled(AccountDataSO.CharacterData.position.pointOfInterestId == Data.id);
            }

            foreach (var item in AccountDataSO.PointOfInterestData.vendors)
            {

                var entryUI = PrefabFactory.CreateGameObject<UIVendorEntry>(VendorMapPrefab, InterestingStuffParent);
                entryUI.SetData(item);
                entryUI.OnClicked += OnVendorEntryClicked;
                entryUI.gameObject.GetComponentInChildren<UIInterestMapButton>().SetEnabled(IsPlayerOnThisPoI);
            }



            //foreach (var item in AccountDataSO.PointOfInterestData.trainers)
            //{
            //    var entryUI = PrefabFactory.CreateGameObject<UITrainerEntry>(TrainerMapPrefab, InterestingStuffParent);
            //    entryUI.SetData(item);
            //    entryUI.OnClicked += OnTrainerEntryClicked;
            //    entryUI.gameObject.GetComponentInChildren<UIInterestMapButton>().SetEnabled(IsPlayerOnThisPoI);
            //}

            foreach (var item in AccountDataSO.PointOfInterestData.specials)
            {
                //  var interestingItem = PrefabFactory.CreateGameObject<UIInterestingStuffEntry>(InterestingStuffPrefab, InterestingStuffParent);
                if (item == Utils.POI_SPECIALS.AUCTION_HOUSE)
                {
                    var entryUI = spawnedEntry = PrefabFactory.CreateGameObject(AuctionHousePrefab, InterestingStuffParent);
                    entryUI.gameObject.GetComponentInChildren<UIInterestMapButton>().SetEnabled(IsPlayerOnThisPoI);

                }
                //   interestingItem.SetAsAH();
                else if (item == Utils.POI_SPECIALS.MAILBOX)
                {
                    var entryUI = spawnedEntry = PrefabFactory.CreateGameObject(InboxPrefab, InterestingStuffParent);
                    entryUI.gameObject.GetComponentInChildren<UIInterestMapButton>().SetEnabled(IsPlayerOnThisPoI);

                }
                else if (item == Utils.POI_SPECIALS.BARBER)
                {
                    var entryUI = PrefabFactory.CreateGameObject(BarberPrefab, InterestingStuffParent);

                    entryUI.gameObject.GetComponentInChildren<UIInterestMapButton>().SetEnabled(IsPlayerOnThisPoI);

                }
                else if (item == Utils.POI_SPECIALS.INN)
                {
                    Debug.Log("INN");
                    var entryUI = PrefabFactory.CreateGameObject(InnPrefab, InterestingStuffParent);

                    entryUI.gameObject.GetComponentInChildren<UIInterestMapButton>().SetEnabled(IsPlayerOnThisPoI);

                }

                else if (item == Utils.POI_SPECIALS.CHAPEL)
                {
                    var entryUI = PrefabFactory.CreateGameObject(ChapelPrefab, InterestingStuffParent);

                    entryUI.gameObject.GetComponentInChildren<UIInterestMapButton>().SetEnabled(IsPlayerOnThisPoI);

                }

                else if (item == Utils.POI_SPECIALS.DUNGEON_ENTRANCE)
                {
                    var entryUI = PrefabFactory.CreateGameObject(DungeonEntrancePrefab, InterestingStuffParent);

                    entryUI.gameObject.GetComponentInChildren<UIInterestMapButton>().SetEnabled(IsPlayerOnThisPoI);

                }

                //else if (item == Utils.POI_SPECIALS.DUNGEON_EXIT)
                //{
                //    var entryUI = PrefabFactory.CreateGameObject(DungeonExitPrefab, InterestingStuffParent);

                //    entryUI.gameObject.GetComponentInChildren<UIInterestMapButton>().SetEnabled(IsPlayerOnThisPoI);

                //}

                else if (item == Utils.POI_SPECIALS.FORGE)
                {
                    var entryUI = PrefabFactory.CreateGameObject(ForgePrefab, InterestingStuffParent);

                    entryUI.gameObject.GetComponentInChildren<UIInterestMapButton>().SetEnabled(IsPlayerOnThisPoI);

                }
                else if (item == Utils.POI_SPECIALS.TREASURE)
                {
                    var entryUI = PrefabFactory.CreateGameObject(TreasurePrefab, InterestingStuffParent);

                    entryUI.gameObject.GetComponentInChildren<UIInterestMapButton>().SetEnabled(IsPlayerOnThisPoI);

                }

                //  interestingItem.SetAsMailbox();

            }

        }
        else //pokud mam nejaky memmory a mel bych mit tak ukazu co na nem si bylo
        {
            //   Utils.DestroyAllChildren(InterestingStuffParent);

            if (AccountDataSO.CharacterData.GetWorldMapMemmoryForWorldPosition(WorldPosition) != null)
            {
                foreach (var specialPoIMemmory in AccountDataSO.CharacterData.GetWorldMapMemmoryForWorldPosition(WorldPosition).specialPointsOfInterest)
                {
                    var memmoryUI = PrefabFactory.CreateGameObject<UIWorldMapMemmoryEntry>(WorldMapMemmoryEntryPrefab, InterestingStuffParent);
                    memmoryUI.Setup(specialPoIMemmory);
                }
            }
        }

        InterestingStuffParent.gameObject.SetActive(InterestingStuffParent.childCount > 0);

        Utils.DestroyAllChildren(EncountersMapParent);


        foreach (var item in AccountDataSO.EncountersData)
        {
            if (item.position.pointOfInterestId == WorldPosition.pointOfInterestId)
            {

                var entryUI = PrefabFactory.CreateGameObject<UIEncounterEntryMap>(EncounterEntryMapPrefab, EncountersMapParent);
                entryUI.SetEncounter(item);
                entryUI.OnClicked += OnEncounterEntryClicked;

                // ExploreButton.targetGraphic.color = ColorEnemy;
            }
        }

        RefreshEncounterResult();



        if (AccountDataSO.PointOfInterestData.monsters != null && (AccountDataSO.CharacterData.GetMaxTierReachedForPointOfInterest(WorldPosition.pointOfInterestId) + 1) >= AccountDataSO.PointOfInterestData.monsters.tiersTotal && IsPlayerOnThisPoI)
        {

            TimePriceText.gameObject.transform.gameObject.SetActive(false);
        }


        DisplayButtonAsNormal();

        ColoriseButton();
        //Refreshnu kameru na tuhle pozici

        //if (IsPlayerOnThisPoI)
        //{
        //    Vector3 pos1 = this.transform.localPosition;
        //    Vector3 pos2 = new Vector3((-1) * pos1.x, (-1) * pos1.y, pos1.z);
        //    //   ScrollContent.localPosition = pos2;

        //    if (ViewMoveCoroutine != null)
        //        StopCoroutine(ViewMoveCoroutine);

        //    ViewMoveCoroutine = StartCoroutine(MoveView(pos2));
        //}
        //else
        //    Debug.LogError("Jaktoze nejsem na zadnem PoI buttonu? Kde sem?");


    }

    private void RefreshEncounterResult()
    {
        Utils.DestroyAllChildren(EncountersResultMapParent);
        foreach (var item in AccountDataSO.EncounterResultsData)
        {
            if (item.position.pointOfInterestId == WorldPosition.pointOfInterestId)
            {
                var entryUI = PrefabFactory.CreateGameObject<UIEncounterResultEntry>(EncounterResultEntryMapPrefab, EncountersResultMapParent);
                entryUI.SetEncounter(item);
                entryUI.OnClicked += OnEncounterResultEntryClicked;

                // ExploreButton.targetGraphic.color = ColorEnemy;
            }
        }

        ColoriseButton();
    }

    private void SpawnPartyMemberPortraits()
    {

    }

    public void HoldFinished()
    {
        OnClicked?.Invoke(this);
    }

    public void Clicked()
    {
        if (!HoldButton.IsFunctional())
            OnClicked?.Invoke(this);
    }

    public async void ExploreThisPoI()
    {
        if (AccountDataSO.PointOfInterestData.pointOfInterestType == (int)Utils.ROOM_TYPE.MONSTER_SOLO)
            await FirebaseCloudFunctionSO.ExploreMonsters();
        //else if (AccountDataSO.PointOfInterestData.pointOfInterestType == (int)Utils.POI_TYPE.DUNGEON)
        //    await FirebaseCloudFunctionSO.EnterDungeon();

    }

    public void TravelToThisPoI()
    {
        FirebaseCloudFunctionSO.PointOfInterestTravel(WorldPosition.pointOfInterestId);
    }

    public void RefreshButtonDisplay(UIPointOfInterestButton _selectedButton, int _totalTravelTime)
    {
        this.DisplayButtonAsNormal();

        if (this == _selectedButton)
        {
            if (_selectedButton.WorldPosition.pointOfInterestId != AccountDataSO.CharacterData.position.pointOfInterestId) //jen pokud to neni lokace na ktere jsem ukazu a buttonu travel time posledni
                DisplayButtonAsTravelTimeToThisPoI(_totalTravelTime);
        }

        if (IsPlayerOnThisPoI)
            HoldButton.SetFunctional(true);
    }

    private void DisplayButtonAsTravelTimeToThisPoI(int _travelPointsCost)
    {
        HoldButton.SetFunctional(true);

        Debug.Log("_travelPointsCost:" + _travelPointsCost);
        int timePerTravelPoint = AccountDataSO.OtherMetadataData.constants.timePerTravelPoint;
        int playerTravelPointAmount = Mathf.FloorToInt(AccountDataSO.CharacterData.currency.travelPoints);

        int stockOfTravelTimeInsideTravelPoints = playerTravelPointAmount * timePerTravelPoint;
        int lefotverPriceInTimeToPay = (_travelPointsCost * timePerTravelPoint) - stockOfTravelTimeInsideTravelPoints;
        //    int pricePaidInFormOfTravelPoints = stockOfTravelTimeInsideTravelPoints / timePerTravelPoint;

        if (lefotverPriceInTimeToPay > 0)
        {
            //  NameText.color = Color.yellow;
            NameText.SetText("Travel Here (<color=\"lightblue\">" + playerTravelPointAmount.ToString() + "</color> + <color=\"yellow\">" + lefotverPriceInTimeToPay + "</color> ) ");
        }
        else
        {
            NameText.SetText("Travel Here (<color=\"lightblue\">" + _travelPointsCost.ToString() + "</color>)");
        }

        //if (_travelPointsCost <= AccountDataSO.CharacterData.currency.travelPoints)
        //    NameText.SetText("Travel Here (<color=\"lightblue\">" + _travelPointsCost.ToString() + "</color>)");
        //else
        //    NameText.SetText("Travel Here (<color=\"red\">" + _travelPointsCost.ToString() + "</color>)");

    }

    private void DisplayButtonAsNormal()
    {
        HoldButton.SetFunctional(false);


        if (IsPlayerOnThisPoI)
        {

            // if (AccountDataSO.PointOfInterestData.pointOfInterestType == Utils.POI_TYPE.ENCOUNTER && !AccountDataSO.EncountersContainsEncounterCreatedByMe())
            if (!AccountDataSO.EncountersContainsEncounterCreatedByMe())
            {

                Debug.Log("AccountDataSO.PointOfInterestData.roomType:" + AccountDataSO.PointOfInterestData.roomType);
                //HoldButton.SetFunctional(true);
                //ExploreButton.interactable = true;
                NameText.color = Color.green;
                NameText.SetText("");
                if (metadata != null && AccountDataSO.CharacterData.IsWorldPositionExplored(WorldPosition))
                    NameText.SetText(metadata.title.GetText());//+ "(" +( AccountDataSO.PointOfInterestData.exploreTimePrice + AccountDataSO.CharacterData.getMaxTierReachedForPointOfInterest(PointOfInterestId) + 1) + ")");
                else
                    NameText.SetText(Utils.DescriptionsMetadata.GetPointsOfInterestRoomTypesMetadata(AccountDataSO.PointOfInterestData.roomType).title.EN);//NameText.SetText(AccountDataSO.PointOfInterestData.typeId + "(" + AccountDataSO.PointOfInterestData.id + ")");// (" + (AccountDataSO.PointOfInterestData.exploreTimePrice + AccountDataSO.CharacterData.getMaxTierReachedForPointOfInterest(PointOfInterestId) + 1) + ")");
            }
            else
            {
                //ExploreButton.interactable = false;
                NameText.color = Color.white;
                NameText.SetText("");
                if (metadata != null)
                    NameText.SetText(metadata.title.GetText());
                else if (AccountDataSO.CharacterData.GetWorldMapMemmoryForWorldPosition(WorldPosition) != null)
                    NameText.SetText(Utils.DescriptionsMetadata.GetPointsOfInterestRoomTypesMetadata(AccountDataSO.PointOfInterestData.roomType).title.EN);
            }
        }
        else
        {
            if (AccountDataSO.CharacterData.IsWorldPositionExplored(WorldPosition))
            {
                //HoldButton.SetFunctional(false);
                NameText.color = Color.white;
                NameText.SetText("");
                if (metadata != null)
                    NameText.SetText(metadata.title.GetText());
                else if (AccountDataSO.CharacterData.GetWorldMapMemmoryForWorldPosition(WorldPosition) != null)
                {
                    Debug.Log("AccountDataSO.CharacterData.GetWorldMapMemmoryForWorldPosition(WorldPosition).typeId:" + AccountDataSO.CharacterData.GetWorldMapMemmoryForWorldPosition(WorldPosition).roomType);
                    NameText.SetText(Utils.DescriptionsMetadata.GetPointsOfInterestRoomTypesMetadata(AccountDataSO.CharacterData.GetWorldMapMemmoryForWorldPosition(WorldPosition).roomType).title.EN);//NameText.SetText(AccountDataSO.CharacterData.GetWorldMapMemmoryForWorldPosition(WorldPosition).typeId);
                }
            }
            else
            {


                NameText.color = Color.gray;


                if (AccountDataSO.LocationData.GetDijkstraMapVertexById(WorldPosition.pointOfInterestId) != null)
                {
                    var poiDijkstraData = AccountDataSO.LocationData.GetDijkstraMapVertexById(WorldPosition.pointOfInterestId);
                    //  RoomTypeImage.sprite = AllImageIdDefinitionSOSet.GetDefinitionById(Enum.GetName(typeof(ROOM_TYPE), poiDijkstraData.type), "POI_TYPE").Image;
                    NameText.SetText(Utils.DescriptionsMetadata.GetPointsOfInterestRoomTypesMetadata(Enum.GetName(typeof(ROOM_TYPE), poiDijkstraData.type)).title.EN);

                }


                //HoldButton.SetFunctional(false);

                //  NameText.SetText("Unexplored");
            }
        }

    }

    public void SetReachable(bool _isInteractable)
    {
        IsReachable = _isInteractable;

        ExploreButton.interactable = IsReachable;
        // HoldButton.SetFunctional(IsReachable);
        NameTitle.SetActive(IsReachable);

        ColoriseButton();
        RefreshAmbusDangerIndicator();
        //if (!IsReachable)
        //    ExploreButton.targetGraphic.color = ColorUnreachable;
        //else
        //    ExploreButton.targetGraphic.color = ColorUnexplored;
    }

    private void RefreshAmbusDangerIndicator()
    {
        var poiDijkstraData = AccountDataSO.LocationData.GetDijkstraMapVertexById(WorldPosition.pointOfInterestId);
        if (IsReachable)
            AmbushWarningGO.SetActive(!AccountDataSO.CharacterData.IsWorldPositionExplored(WorldPosition) && poiDijkstraData.mapPosition.y - AccountDataSO.CharacterData.stats.level >= 1);
    }

    private void ColoriseButton()
    {
        if (!IsReachable)
            ExploreButton.targetGraphic.color = ColorUnreachable;
        else
        {
            if (AccountDataSO.CharacterData.IsWorldPositionExplored(WorldPosition))
                ExploreButton.targetGraphic.color = ColorNormal;
            else
            {
                ExploreButton.targetGraphic.color = ColorUnexplored;
            }
        }

        if (EncountersResultMapParent.childCount > 0 || EncountersMapParent.childCount > 0)
            ExploreButton.targetGraphic.color = ColorEnemy;

        if (IsPlayerOnThisPoI)
            ExploreButton.targetGraphic.color = ColorActive;
    }

    //private IEnumerator MoveView(Vector3 targetPosition)
    //{
    //    float startTime = Time.time;
    //    while (Vector3.Distance(ScrollContent.localPosition, targetPosition) > 10 && (Time.time - startTime) < 1f)
    //    {
    //        //            Debug.Log(ScrollContent.localPosition);
    //        //           Debug.Log(targetPosition);
    //        ScrollContent.localPosition = Vector3.Lerp(ScrollContent.localPosition, targetPosition, 2f * Time.deltaTime);
    //        yield return null;
    //    }
    //}
    //public void ShowPointOfInterestLeaderboard()
    //{
    //    UIManager.instance.UILeaderboardsPanel.ShowPointOfInterestLeaderboard("MONSTER_KILLS", QueryData);
    //}
}
