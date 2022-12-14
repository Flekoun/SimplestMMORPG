using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using simplestmmorpg.data;

public class UILocationEncounters : MonoBehaviour
{
    public PointOfInterestIdDefinitionSOSet AllPointOfInterestIdDefinitionSOSet;
    public FirebaseCloudFunctionSO FirebaseCloudFunctionSO;
    public AccountDataSO AccountDataSO;
    public UIQuestgiverSpawner UIQuestgiverSpawner;
    public UIPointsOfInterestSpawner UIPointsOfInterestSpawner;
    public GameObject Model;
    public ContentFitterRefresh ContentFitterRefresh;
    public ListenOnEncounterData ListenOnEncounterData;

    public GameObject QuestsPanel;
    public GameObject EncountersPanel;
    public GameObject EncounterRewardsPanel;

   // public UIEnterDungeon UIEnterDungeon;

    public Image BackgroundImage;
    public DijkstraMapMaker DijkstraMapMaker;

    private UIPointOfInterestButton selectedUIPointOfInterestButton = null;


    public void Awake()
    {
        AccountDataSO.OnWorldPositionChanged += OnWorldPositionChanged;
        AccountDataSO.OnWorldLocationChanged += OnWorldLocationChanged;
        UIPointsOfInterestSpawner.OnUIEntryClicked += OnPointOfInterestClicked;
        DijkstraMapMaker.OnVertexReachable += OnVertexReachable;

    }

    private void OnVertexReachable(BaseIdDefinition _vertexDef)
    {
        UIPointsOfInterestSpawner.ShowPointOfInterestButton(_vertexDef);
    }

    private void OnWorldLocationChanged()
    {

        ListenOnEncounterData.StartListening(AccountDataSO.CharacterData.position.locationId, AccountDataSO.CharacterData.position.zoneId);

    }

    private void OnWorldPositionChanged()
    {

        if (!AccountDataSO.MapsData.HasLocationAnyPointsOfInterest(AccountDataSO.CharacterData.position.locationId))
            return;

       

        selectedUIPointOfInterestButton = null;

        UIPointsOfInterestSpawner.Refresh();
        DijkstraMapMaker.Setup(AccountDataSO.MapsData.GetLocationById(AccountDataSO.CharacterData.position.locationId).dijkstraMap, AllPointOfInterestIdDefinitionSOSet);

        DijkstraMapMaker.ClearPlannedTravelPath();
        RefreshButtons();



    }
    // Start is called before the first frame update
    public void Show()
    {
        if (AccountDataSO.MapsData.GetLocationById(AccountDataSO.CharacterData.position.locationId) == null)
        {
            Debug.LogWarning("Tahle lokace nema mapu neni co ukazovat");
            return;
        }
        ListenOnEncounterData.StartListening(AccountDataSO.CharacterData.position.locationId, AccountDataSO.CharacterData.position.zoneId);

        AccountDataSO.OnEncounterDataChanged += Refresh;
        AccountDataSO.OnEncounterResultsDataChanged += Refresh;
        UIQuestgiverSpawner.OnRefreshed += Refresh;

        UIPointsOfInterestSpawner.Refresh();
        Model.gameObject.SetActive(true);

        DijkstraMapMaker.Setup(AccountDataSO.MapsData.GetLocationById(AccountDataSO.CharacterData.position.locationId).dijkstraMap, AllPointOfInterestIdDefinitionSOSet);

        RefreshButtons();

        Refresh();
    }

    public void Hide()
    {
        //   ListenOnEncounterData.StopListening();

        UIQuestgiverSpawner.OnRefreshed -= Refresh;
        AccountDataSO.OnEncounterDataChanged -= Refresh;
        AccountDataSO.OnEncounterResultsDataChanged -= Refresh;

        Model.gameObject.SetActive(false);
    }

    private void Refresh()
    {

        QuestsPanel.SetActive(UIQuestgiverSpawner.Data.Count > 0);
        EncountersPanel.SetActive(AccountDataSO.EncountersData.Count > 0);
        EncounterRewardsPanel.SetActive(AccountDataSO.EncounterResultsData.Count > 0);

        TryToFixScrollReckGlitches();
    }

    private void OnPointOfInterestClicked(UIPointOfInterestButton _entry)
    {


        //pokud PoI na kterou klikam uz je vybrana (tedy klikam po 2.) a neni to moje lokace, jdeme cestovat
        if (_entry == selectedUIPointOfInterestButton && _entry.pointOfInterestDefinition.Id != AccountDataSO.CharacterData.position.pointOfInterestId)
        {
            _entry.TravelToThisPoI();
        }

        // pokud je to lokace na ktere jsem, tak do ni jednoduce vlezem
        if (_entry.pointOfInterestDefinition.Id == AccountDataSO.CharacterData.position.pointOfInterestId)
        {
            selectedUIPointOfInterestButton = _entry;
            DijkstraMapMaker.ClearPlannedTravelPath();
            FirebaseCloudFunctionSO.ExplorePointOfInterest(selectedUIPointOfInterestButton.pointOfInterestDefinition);

        }
        else //jinak ti ukazu kolik by te stala cesta tam
        {
            DijkstraMapMaker.ShowPlannedTravelPath(AccountDataSO.CharacterData.position.pointOfInterestId, _entry.Data, AllPointOfInterestIdDefinitionSOSet);

        }

        selectedUIPointOfInterestButton = _entry;

        RefreshButtons();


    }

    private void RefreshButtons()
    {
        foreach (var item in UIPointsOfInterestSpawner.EntryList)
        {
            if (DijkstraMapMaker.PlannedPathNewest != null)
                item.RefreshButtonDisplay(selectedUIPointOfInterestButton, DijkstraMapMaker.PlannedPathNewest.totalWeight);
            else
                item.RefreshButtonDisplay(selectedUIPointOfInterestButton, 0);
        }
    }


    private void TryToFixScrollReckGlitches()
    {
        // ContentFitterRefresh.RefreshContentFitters();
        StartCoroutine(Wait());
    }

    IEnumerator Wait()
    {
        yield return new WaitForEndOfFrame();
        ContentFitterRefresh.RefreshContentFitters();
    }

}
