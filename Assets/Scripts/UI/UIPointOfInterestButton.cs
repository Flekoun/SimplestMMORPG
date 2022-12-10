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
    public PointOfInterestIdDefinitionSOSet AllPointOfInterestIdDefinitionSOSet;
    public Image PortraitImage;
    public TextMeshProUGUI NameText;
    public UIPortrait PlayerPortrait;
    public Transform PartyMembersParent;
    public GameObject PortraitPrefab;
    //public UILineRenderer UILineRenderer;
    public string Data;
    public PointOfInterestIdDefinition pointOfInterestDefinition;
    public Sprite UnexploredSprite;

    public GameObject Model;
    public UnityAction<UIPointOfInterestButton> OnClicked;
    // Start is called before the first frame update
    private bool IsPlayerOnThisPoI = false;

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


    public void SetData(string _id)
    {
        Data = _id;
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

        if (AllPointOfInterestIdDefinitionSOSet.GetDefinitionById(Data) != null)
        {
            //    Model.SetActive(AccountDataSO.CharacterData.IsPositionExplored(Data));

            pointOfInterestDefinition = AllPointOfInterestIdDefinitionSOSet.GetDefinitionById(Data) as PointOfInterestIdDefinition;
      
            if (AccountDataSO.CharacterData.IsPositionExplored(Data))
                PortraitImage.sprite = AllImageIdDefinitionSOSet.GetDefinitionById(Utils.GetMetadataForPointOfInterest(pointOfInterestDefinition.Id).imageId).Image;
            else
                PortraitImage.sprite = UnexploredSprite;
            // NameText.SetText(pointOfInterestDefinition.DisplayName);
            this.gameObject.transform.localPosition = pointOfInterestDefinition.position;

            if (AccountDataSO.CharacterData.position.pointOfInterestId == pointOfInterestDefinition.Id)
            {
                PlayerPortrait.SetPortrait(AccountDataSO.CharacterData.characterPortrait);
                PlayerPortrait.gameObject.SetActive(true);
            }
            else
                PlayerPortrait.gameObject.SetActive(false);


            IsPlayerOnThisPoI = AccountDataSO.CharacterData.position.pointOfInterestId == pointOfInterestDefinition.Id;//&& AccountDataSO.CharacterData.position.zoneId == ZoneDef.Id;


            //Spawn party members portraits
            Utils.DestroyAllChildren(PartyMembersParent);
            if (AccountDataSO.IsInParty())
            {
                foreach (var member in AccountDataSO.PartyData.partyMembers)
                {
                    if (member.position.pointOfInterestId == pointOfInterestDefinition.Id)//&& member.position.zoneId == ZoneDef.Id)
                    {
                        if (member.uid != AccountDataSO.CharacterData.uid)
                        {
                            PrefabFactory.CreateGameObject<UIPortrait>(PortraitPrefab, PartyMembersParent).SetPortrait(member.characterPortrait);
                        }
                    }
                }
            }

        }


    }

    private void SpawnPartyMemberPortraits()
    {

    }

    public void Clicked()
    {
        if (OnClicked != null)
            OnClicked.Invoke(this);
    }


    public void TravelToThisPoI()
    {
        FirebaseCloudFunctionSO.PointOfInterestTravel(pointOfInterestDefinition.Id);
    }

    public void RefreshButtonDisplay(UIPointOfInterestButton _selectedButton, int _totalTravelTime)
    {
        this.DisplayButtonAsNormal();

        if (this == _selectedButton)
        {
            if (_selectedButton.pointOfInterestDefinition.Id != AccountDataSO.CharacterData.position.pointOfInterestId) //jen pokud to neni lokace na ktere jsem ukazu a buttonu travel time posledni
                DisplayButtonAsTravelTimeToThisPoI(_totalTravelTime);
        }
    }

    private void DisplayButtonAsTravelTimeToThisPoI(int _travelTime)
    {
        NameText.color = Color.yellow;
        NameText.SetText("Travel Here (" + _travelTime.ToString() + ")");
    }

    private void DisplayButtonAsNormal()
    {

        if (IsPlayerOnThisPoI)
        {
            NameText.color = Color.green;
            NameText.SetText("Explore " + Utils.GetMetadataForPointOfInterest(pointOfInterestDefinition.Id).title.GetText());
        }
        else
        {
            if (AccountDataSO.CharacterData.IsPositionExplored(Data))
            {
                NameText.color = Color.white;
                NameText.SetText(Utils.GetMetadataForPointOfInterest(pointOfInterestDefinition.Id).title.GetText());
            }
            else
            {
                NameText.color = Color.gray;
                NameText.SetText("Unexplored");
            }
        }

    }


}
