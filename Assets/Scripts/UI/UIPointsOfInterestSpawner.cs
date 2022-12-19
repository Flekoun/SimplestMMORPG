using System.Collections;
using System.Collections.Generic;
using simplestmmorpg.data;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class UIPointsOfInterestSpawner : MonoBehaviour
{
    //  public PointOfInterestIdDefinitionSOSet AllPointOfInterestIdDefinitionSOSet;
    public AccountDataSO AccountDataSO;
    public PrefabFactory PrefabFactory;
    public Transform Parent;
    public GameObject UIEntryPrefab;
    public GameObject UIEntryPrefab_NoDescriptionData;
    public UILineMaker AllPathsUILineMaker;

    public UnityAction<UIPointOfInterestButton> OnUIEntryClicked;

    public List<UIPointOfInterestButton> EntryList = new List<UIPointOfInterestButton>();

    public void Awake()
    {
        AccountDataSO.OnLocationDataChanged += Refresh;
        //  AccountDataSO.OnEncounterDataChanged += CheckForAvailability;
        //  AccountDataSO.OnEncounterResultsDataChanged += CheckForAvailability;
    }

    public void OnDestroy()
    {
        AccountDataSO.OnLocationDataChanged -= Refresh;
        //  AccountDataSO.OnEncounterDataChanged -= CheckForAvailability;
        //   AccountDataSO.OnEncounterResultsDataChanged -= CheckForAvailability;
    }

    public void ShowPointOfInterestButton(ScreenPoisitionWihtId _locationDef)
    {
        foreach (var item in EntryList)
        {
            if (item.Data.id == _locationDef.id)
                item.Show(true);
        }
    }

    public void Refresh()
    {
        EntryList.Clear();
        Utils.DestroyAllChildren(Parent);


        foreach (var poi in AccountDataSO.LocationData.pointsOfInterest)//AccountDataSO.LocationData.dijkstraMap)
        {
            UIPointOfInterestButton entry = null;

            if (Utils.GetMetadataForPointOfInterest(poi.id) == null)
                entry = PrefabFactory.CreateGameObject<UIPointOfInterestButton>(UIEntryPrefab_NoDescriptionData, Parent);
            else
                entry = PrefabFactory.CreateGameObject<UIPointOfInterestButton>(UIEntryPrefab, Parent);

            entry.SetData(poi);
            entry.OnClicked += UIEncounterEntryClicked;
            EntryList.Add(entry);
            entry.Show(false);
        }


    }


    public void UIEncounterEntryClicked(UIPointOfInterestButton _data)
    {
        if (OnUIEntryClicked != null)
            OnUIEntryClicked.Invoke(_data);

    }

    public UIPointOfInterestButton GetPoIPlayerIsCurrentlyOn()
    {
        foreach (var item in EntryList)
        {
            if (item.Data.id == AccountDataSO.CharacterData.position.pointOfInterestId)
                return item;
        }

        return null;
    }
}
