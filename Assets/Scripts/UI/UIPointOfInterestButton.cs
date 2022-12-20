using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using simplestmmorpg.data;
using UnityEngine.Events;

public class UIPointOfInterestButton : MonoBehaviour
{
    public ImageIdDefinitionSOSet AllImageIdDefinitionSOSet;
    public AccountDataSO AccountDataSO;
    public FirebaseCloudFunctionSO FirebaseCloudFunctionSO;
    public PrefabFactory PrefabFactory;
    //public PointOfInterestIdDefinitionSOSet AllPointOfInterestIdDefinitionSOSet;
    public Image PortraitImage;
    public TextMeshProUGUI NameText;
    public UIPortrait PlayerPortrait;
    public Transform PartyMembersParent;
    public GameObject PortraitPrefab;
    //public UILineRenderer UILineRenderer;
    public PointOfInterest Data;
    // public PointOfInterestIdDefinition pointOfInterestDefinition;
    public Sprite UnexploredSprite;
    public Sprite ExploredSpriteDefault;
    public Button ExploreButton;
    public GameObject Model;
    public UnityAction<UIPointOfInterestButton> OnClicked;
    public HoldButton HoldButton;
    // Start is called before the first frame update
    private bool IsPlayerOnThisPoI = false;
    private BaseDescriptionMetadata metadata = null;
    //public void Awake()
    //{
    //    AccountDataSO.OnCharacterDataChanged += Refresh;
    //    AccountDataSO.OnPartyDataChanged += Refresh;
    //}


    public void OnEnable()
    {
        AccountDataSO.OnCharacterDataChanged += Refresh;
        AccountDataSO.OnPartyDataChanged += Refresh;
    }

    public void OnDisable()
    {
        AccountDataSO.OnCharacterDataChanged -= Refresh;
        AccountDataSO.OnPartyDataChanged -= Refresh;
    }


    public void SetData(PointOfInterest _data)
    {
        Data = _data;
        Refresh();

    }



    public void Show(bool _show)
    {
        Model.SetActive(_show);//AccountDataSO.CharacterData.IsPositionExplored(Data));
    }

    private void Refresh()
    {
        if (this == null)
        {
            Debug.LogWarning("divne ze se toto deje");
            return;
        }

        metadata = Utils.GetMetadataForPointOfInterest(Data.id);


        //if (AllPointOfInterestIdDefinitionSOSet.GetDefinitionById(Data.id) != null)
        //{

        //   pointOfInterestDefinition = AllPointOfInterestIdDefinitionSOSet.GetDefinitionById(Data.id) as PointOfInterestIdDefinition;

        if (AccountDataSO.IsPositionExplored(Data.id))//AccountDataSO.CharacterData.IsPositionExplored(Data.id))
        {
            if (metadata != null)
                PortraitImage.sprite = AllImageIdDefinitionSOSet.GetDefinitionById(metadata.imageId).Image;
            else
                PortraitImage.sprite = ExploredSpriteDefault;

        }
        else
            PortraitImage.sprite = UnexploredSprite;


        this.gameObject.transform.localPosition = Data.screenPosition.ToVector2();// pointOfInterestDefinition.position;

        if (AccountDataSO.CharacterData.position.pointOfInterestId == Data.id)
        {
            PlayerPortrait.SetPortrait(AccountDataSO.CharacterData.characterPortrait);
            PlayerPortrait.gameObject.SetActive(true);
        }
        else
            PlayerPortrait.gameObject.SetActive(false);


        IsPlayerOnThisPoI = AccountDataSO.CharacterData.position.pointOfInterestId == Data.id;//&& AccountDataSO.CharacterData.position.zoneId == ZoneDef.Id;


        //Spawn party members portraits
        Utils.DestroyAllChildren(PartyMembersParent);
        if (AccountDataSO.IsInParty())
        {
            foreach (var member in AccountDataSO.PartyData.partyMembers)
            {
                if (member.position.pointOfInterestId == Data.id)//&& member.position.zoneId == ZoneDef.Id)
                {
                    if (member.uid != AccountDataSO.CharacterData.uid)
                    {
                        PrefabFactory.CreateGameObject<UIPortrait>(PortraitPrefab, PartyMembersParent).SetPortrait(member.characterPortrait);
                    }
                }
            }
        }

        //   }


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


    public void TravelToThisPoI()
    {
        FirebaseCloudFunctionSO.PointOfInterestTravel(Data.id);
    }

    public void RefreshButtonDisplay(UIPointOfInterestButton _selectedButton, int _totalTravelTime)
    {
        this.DisplayButtonAsNormal();

        if (this == _selectedButton)
        {
            if (_selectedButton.Data.id != AccountDataSO.CharacterData.position.pointOfInterestId) //jen pokud to neni lokace na ktere jsem ukazu a buttonu travel time posledni
                DisplayButtonAsTravelTimeToThisPoI(_totalTravelTime);
        }
    }

    private void DisplayButtonAsTravelTimeToThisPoI(int _travelTime)
    {
        HoldButton.SetFunctional(true);
        NameText.color = Color.yellow;
        NameText.SetText("Travel Here (" + _travelTime.ToString() + ")");
    }

    private void DisplayButtonAsNormal()
    {

        if (IsPlayerOnThisPoI)
        {
            if (Data.pointOfInterestType == Utils.POI_TYPE.ENCOUNTER)
            {
                HoldButton.SetFunctional(true);
                ExploreButton.interactable = true;
                NameText.color = Color.green;
                if (metadata != null)
                    NameText.SetText("Explore " + metadata.title.GetText() + "(" + Data.exploreTimePrice + ")");
                else
                    NameText.SetText("Explore (" + Data.exploreTimePrice + ")");
            }
            else
            {
                ExploreButton.interactable = false;
                NameText.color = Color.white;
                if (metadata != null)
                    NameText.SetText(metadata.title.GetText());
                else
                    NameText.SetText("Explored");
            }
        }
        else
        {
            if (AccountDataSO.IsPositionExplored(Data.id))
            {
                HoldButton.SetFunctional(false);
                NameText.color = Color.white;
                if (metadata != null)
                    NameText.SetText(metadata.title.GetText());
                else
                    NameText.SetText("Explored");
            }
            else
            {
                HoldButton.SetFunctional(false);
                NameText.color = Color.gray;
                NameText.SetText("Unexplored");
            }
        }

    }


}
