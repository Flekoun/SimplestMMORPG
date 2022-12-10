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
    public UILineMaker AllPathsUILineMaker;

    public UnityAction<UIPointOfInterestButton> OnUIEntryClicked;

    public List<UIPointOfInterestButton> EntryList = new List<UIPointOfInterestButton>();

    public void Awake()
    {
        AccountDataSO.OnMapsChanged += Refresh;
        //  AccountDataSO.OnEncounterDataChanged += CheckForAvailability;
        //  AccountDataSO.OnEncounterResultsDataChanged += CheckForAvailability;
    }

    public void OnDestroy()
    {
        AccountDataSO.OnMapsChanged -= Refresh;
        //  AccountDataSO.OnEncounterDataChanged -= CheckForAvailability;
        //   AccountDataSO.OnEncounterResultsDataChanged -= CheckForAvailability;
    }

    public void ShowPointOfInterestButton(BaseIdDefinition _locationDef)
    {
        foreach (var item in EntryList)
        {
            if (item.Data == _locationDef.Id)
                item.Show(true);
        }
    }

    public void Refresh()
    {
        EntryList.Clear();
        Utils.DestroyAllChildren(Parent);

        //pouze pokud ma lokace nejake PoI (tedy je to encounter lokace a ne mesto atd)....tak vygeneruju PoI
        if (AccountDataSO.MapsData.HasLocationAnyPointsOfInterest(AccountDataSO.CharacterData.position.locationId))
        {

            foreach (var vertex in AccountDataSO.MapsData.GetLocationById(AccountDataSO.CharacterData.position.locationId).dijkstraMap)
            {
                var entry = PrefabFactory.CreateGameObject<UIPointOfInterestButton>(UIEntryPrefab, Parent);
                entry.SetData(vertex.id);
                entry.OnClicked += UIEncounterEntryClicked;
                EntryList.Add(entry);
                entry.Show(false);
            }


        }
    }


    public void UIEncounterEntryClicked(UIPointOfInterestButton _data)
    {
        if (OnUIEntryClicked != null)
            OnUIEntryClicked.Invoke(_data);

    }
}
