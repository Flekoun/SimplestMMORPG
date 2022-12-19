using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;
using static UnityEditor.Progress;

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

    //    public TextMeshProUGUI ButtonText;

    public GameObject PortraitPrefab;
    public Transform PartyMembersParent;

    public UIPortrait MarkerPortrait;
    public Sprite UnexploredSprite;
    public GameObject Model;

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

       // LocationDef = AllLocationsDefinitionSOSet.GetDefinitionById(_locationId) as LocationIdDefinition;
        //ZoneDef = AllZonesDefinitionSOSet.GetDefinitionById(_zoneId) as ZoneIdDefinition;



        if (AccountDataSO.CharacterData.IsPositionExplored(Data))
        {
            LocationPotrait.sprite = AllImageIdDefinitionSOSet.GetDefinitionById(Utils.GetMetadataForLocation(Data).imageId).Image;
            //    LocationNameText.SetText(Utils.GetMetadataForLocation(LocationDef.Id).title.GetText());
        }
        else
        {
            LocationPotrait.sprite = UnexploredSprite;
            //  LocationNameText.SetText("<color=\"gray\">Unexplored</color>");
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
                        portrait.SetPortrait(member.characterPortrait);
                    }
                }
            }
        }
    }
    void Refresh()
    {

        RefreshPartyMemberPortraits();
        this.transform.localPosition = AccountDataSO.ZoneData.GetScreenPositionForLocationId(Data).ToVector2(); //LocationDef.Position;

        // IsPlayerOnThisLocation = AccountDataSO.CharacterData.position.locationId == LocationDef.Id;//&& AccountDataSO.CharacterData.position.zoneId == ZoneDef.Id;

        MarkerPortrait.gameObject.SetActive(IsPlayerOnThisLocation());
        MarkerPortrait.SetPortrait(AccountDataSO.CharacterData.characterPortrait);
    }

    public void Clicked()
    {
        if (OnClicked != null)
            OnClicked.Invoke(this);
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
                DisplayButtonAsTravelTimeToThisLocation(_totalTravelTime);
        }
    }

    private void DisplayButtonAsTravelTimeToThisLocation(int _travelTime)
    {
        LocationNameText.color = Color.yellow;
        LocationNameText.SetText("Travel Here (" + _travelTime.ToString() + ")");
    }

    private void DisplayButtonAsNormal()
    {
        //        Debug.Log("------------Refreshing button :" + IsPlayerOnThisLocation() + "--------------");
        if (IsPlayerOnThisLocation())
        {
            LocationNameText.color = Color.green;
            LocationNameText.SetText("Enter " + Utils.GetMetadataForLocation(Data).title.GetText());
        }
        else
        {
            if (AccountDataSO.CharacterData.IsPositionExplored(Data))
            {
                LocationNameText.color = Color.white;
                LocationNameText.SetText(Utils.GetMetadataForLocation(Data).title.GetText());
            }
            else
            {
                LocationNameText.color = Color.gray;
                LocationNameText.SetText("Unexplored");
            }
        }

    }

    public UnityEvent OnOpenLocation;
}
