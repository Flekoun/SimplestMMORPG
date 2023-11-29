using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;


public class UIWorldMapLocationButton : MonoBehaviour
{
    public ImageIdDefinitionSOSet AllImageIdDefinitionSOSet;
    public FirebaseCloudFunctionSO FirebaseCloudFunctionSO;
    public AccountDataSO AccountDataSO;

    //public BaseDefinitionSOSet AllLocationsDefinitionSOSet;
    //public BaseDefinitionSOSet AllZonesDefinitionSOSet;

    public PrefabFactory PrefabFactory;
    // public LocationIdDefinition LocationDef;
    //   public ZoneIdDefinition ZoneDef;
    public TextMeshProUGUI LocationNameText;
    public Image LocationPotrait;
    public HoldButton HoldButton;
    //    public TextMeshProUGUI ButtonText;

    public GameObject PortraitPrefab;
    public Transform PartyMembersParent;

    public UIPortrait MarkerPortrait;
    public Sprite UnexploredSprite;
    public GameObject Model;

    public Image LocationTypeImage;

    public Sprite TownLocationSprite;
    public Sprite EncounterLocationSprite;
    public Sprite DungeonLocationSprite;

    public Image ButtonImage;
    public Color ColorActive;
    public Color ColorNormal;
    public Color ColorUnexplored;

    public UnityAction<UIWorldMapLocationButton> OnClicked;

    // private bool IsPlayerOnThisLocation = false;
    public string Data;

    public bool IsPlayerOnThisLocation()
    {
        return AccountDataSO.CharacterData.position.locationId == Data;
    }
    public void Awake()
    {
        AccountDataSO.OnCharacterDataChanged += Refresh;
        AccountDataSO.OnPartyDataChanged += RefreshPartyMemberPortraits;
    }


    public void OnDestroy()
    {
        AccountDataSO.OnCharacterDataChanged -= Refresh;
    }

    public void SetData(string _locationId)
    {
        Data = _locationId;

        if (AccountDataSO.CharacterData.IsLocationExplored(Data))
            LocationPotrait.sprite = AllImageIdDefinitionSOSet.GetDefinitionById(Utils.DescriptionsMetadata.GetLocationsMetadata(Data).descriptionData.imageId).Image;
        else
            LocationPotrait.sprite = UnexploredSprite;

        switch (Utils.DescriptionsMetadata.GetLocationsMetadata(Data).locationType)
        {
            case Utils.LOCATION_TYPE.DUNGEON: LocationTypeImage.sprite = DungeonLocationSprite; break;
            case Utils.LOCATION_TYPE.ENCOUNTERS: LocationTypeImage.sprite = EncounterLocationSprite; break;
            case Utils.LOCATION_TYPE.TOWN: LocationTypeImage.sprite = TownLocationSprite; break;

            default:
                break;
        }

        Refresh();
    }

    public void Show(bool _show)
    {
        Model.SetActive(_show);//AccountDataSO.CharacterData.IsPositionExplored(Data));
    }

    private void RefreshPartyMemberPortraits()
    {
        Utils.DestroyAllChildren(PartyMembersParent);

        if (AccountDataSO.IsInParty() && this != null)
        {
            foreach (var member in AccountDataSO.PartyData.partyMembers)
            {
                if (member.position.locationId == Data)//&& member.position.zoneId == ZoneDef.Id)
                {
                    if (member.uid != AccountDataSO.CharacterData.uid)
                    {
                        //   Debug.Log("PartyMembersParent: " + PartyMembersParent.name);
                        var portrait = PrefabFactory.CreateGameObject<UIPortrait>(PortraitPrefab, PartyMembersParent);
                        portrait.SetPortrait(member.characterPortrait, member.characterClass);
                        portrait.SetName(member.displayName);
                    }
                }
            }
        }
    }
    void Refresh()
    {

        RefreshPartyMemberPortraits();
        this.transform.localPosition = AccountDataSO.ZoneData.GetDijkstraMapVertexById(Data).screenPosition.ToVector2(); //LocationDef.Position;

        // IsPlayerOnThisLocation = AccountDataSO.CharacterData.position.locationId == LocationDef.Id;//&& AccountDataSO.CharacterData.position.zoneId == ZoneDef.Id;

        MarkerPortrait.gameObject.SetActive(IsPlayerOnThisLocation());
        MarkerPortrait.SetPortrait(AccountDataSO.CharacterData.characterPortrait, AccountDataSO.CharacterData.characterClass);
        MarkerPortrait.SetName(AccountDataSO.CharacterData.characterName);
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

    public void TravelToThisLocation()
    {
        FirebaseCloudFunctionSO.WorldMapTravel(Data);
    }

    public void RefreshButtonDisplay(UIWorldMapLocationButton _selectedButton, int _totalTravelTime)
    {
        this.DisplayButtonAsNormal();

        if (this == _selectedButton)
        {
            if (_selectedButton.Data != AccountDataSO.CharacterData.position.locationId) //jen pokud to neni lokace na ktere jsem ukazu a buttonu travel time posledni
            {
                DisplayButtonAsTravelTimeToThisLocation(_totalTravelTime);

            }
        }
    }

    private void DisplayButtonAsTravelTimeToThisLocation(int _travelTime)
    {
        HoldButton.SetFunctional(true);
        LocationNameText.color = Color.yellow;
        LocationNameText.SetText("Travel Here (" + _travelTime.ToString() + ")");
    }

    private void DisplayButtonAsNormal()
    {
        //        Debug.Log("------------Refreshing button :" + IsPlayerOnThisLocation() + "--------------");
        if (IsPlayerOnThisLocation())
        {
            HoldButton.SetFunctional(false);
            LocationNameText.color = Color.green;
            LocationNameText.SetText("Enter " + Utils.DescriptionsMetadata.GetLocationsMetadata(Data).descriptionData.title.GetText());
            ButtonImage.color = ColorActive;
        }
        else
        {
            if (AccountDataSO.CharacterData.IsLocationExplored(Data))
            {
                HoldButton.SetFunctional(false);
                LocationNameText.color = Color.white;
                LocationNameText.SetText(Utils.DescriptionsMetadata.GetLocationsMetadata(Data).descriptionData.title.GetText());
                ButtonImage.color = ColorNormal;
            }
            else
            {
                HoldButton.SetFunctional(false);
                LocationNameText.color = Color.gray;
                LocationNameText.SetText("Unexplored");
                ButtonImage.color = ColorUnexplored;
            }
        }

    }

    public void ShowLocationLeaderboard()
    {
        UIManager.instance.UILeaderboardsPanel.ShowLeaderboard("LOCATION_" + Data);
    }

    public UnityEvent OnOpenLocation;
}
