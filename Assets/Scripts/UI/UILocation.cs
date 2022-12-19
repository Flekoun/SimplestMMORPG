using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using simplestmmorpg.data;

public class UILocation : MonoBehaviour
{
    // public PointOfInterestIdDefinitionSOSet AllPointOfInterestIdDefinitionSOSet;
    public FirebaseCloudFunctionSO FirebaseCloudFunctionSO;
    public AccountDataSO AccountDataSO;

    public UITrainerSpawner UITrainerSpawner;
    public UIVendorSpawner UIVendorSpawner;
    public UIQuestgiverSpawner UIQuestgiverSpawner;
    public UIPointsOfInterestSpawner UIPointsOfInterestSpawner;
    public UISpecialsSpawner UISpecialsSpawner;

    public GameObject Model;
    public ContentFitterRefresh ContentFitterRefresh;
    public ListenOnEncounterData ListenOnEncounterData;

    public GameObject QuestsPanel;
    public GameObject EncountersPanel;
    public GameObject EncounterRewardsPanel;
    public GameObject VendorsPanel;
    public GameObject SpecialsPanel;
    public GameObject TrainersPanel;

    // public UIEnterDungeon UIEnterDungeon;

    public Image BackgroundImage;
    public DijkstraMapMaker DijkstraMapMaker;

    private UIPointOfInterestButton selectedUIPointOfInterestButton = null;


    public void Awake()
    {
        AccountDataSO.OnWorldPointOfInterestChanged += OnWorldPositionChanged;
        AccountDataSO.OnLocationDataChanged += OnWorldLocationChanged;
        UIPointsOfInterestSpawner.OnUIEntryClicked += OnPointOfInterestClicked;
        DijkstraMapMaker.OnVertexReachable += OnVertexReachable;
        AccountDataSO.OnPartyDataChanged += RefreshMap;  // mam to tu kvuli tomu kdyz parta jde do dungeonu, tak se ulozi do ni ze maji prozkoumanou 1. entrance lokaci, tak musim refreshnout mapu abych ji videl.....
    }

    private void OnVertexReachable(ScreenPoisitionWihtId _vertexDef)
    {
        UIPointsOfInterestSpawner.ShowPointOfInterestButton(_vertexDef);
    }

    private void RefreshMap()
    {
        if (AccountDataSO.LocationData.GetScreenPositionsWithIds().Count > 0) //kdyz partdyData changed toto zavola nazacatku tak nejsou jeste ani nactene data lokace a pak je tu error....uf
            DijkstraMapMaker.Setup(AccountDataSO.LocationData.dijkstraMap, AccountDataSO.LocationData.GetScreenPositionsWithIds());

    }

    private void OnWorldLocationChanged()
    {

        ListenOnEncounterData.StartListening(AccountDataSO.CharacterData.position.locationId, AccountDataSO.CharacterData.position.zoneId);

    }

    private void OnWorldPositionChanged()
    {


        selectedUIPointOfInterestButton = null;

        UIPointsOfInterestSpawner.Refresh();
        DijkstraMapMaker.Setup(AccountDataSO.LocationData.dijkstraMap, AccountDataSO.LocationData.GetScreenPositionsWithIds());

        DijkstraMapMaker.ClearPlannedTravelPath();
        RefreshButtons();



    }
    // Start is called before the first frame update
    public void Show()
    {


        ListenOnEncounterData.StartListening(AccountDataSO.CharacterData.position.locationId, AccountDataSO.CharacterData.position.zoneId);

        AccountDataSO.OnEncounterDataChanged += Refresh;
        AccountDataSO.OnEncounterResultsDataChanged += Refresh;
        UIQuestgiverSpawner.OnRefreshed += Refresh;
        UIVendorSpawner.OnRefreshed += Refresh;
        UISpecialsSpawner.OnRefreshed += Refresh;
        UITrainerSpawner.OnRefreshed += Refresh;

        UIPointsOfInterestSpawner.Refresh();
        Model.gameObject.SetActive(true);

        DijkstraMapMaker.Setup(AccountDataSO.LocationData.dijkstraMap, AccountDataSO.LocationData.GetScreenPositionsWithIds());

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

        VendorsPanel.SetActive(UIVendorSpawner.HasSpawnedAnyVendors());
        QuestsPanel.SetActive(UIQuestgiverSpawner.HasSpawnedAnyQuests());
        SpecialsPanel.SetActive(UISpecialsSpawner.HasSpawnedAnySpecials());
        EncountersPanel.SetActive(AccountDataSO.EncountersData.Count > 0);
        EncounterRewardsPanel.SetActive(AccountDataSO.EncounterResultsData.Count > 0);
        TrainersPanel.SetActive(UITrainerSpawner.HasSpawnedAnyVendors());

        TryToFixScrollReckGlitches();
    }

    private void OnPointOfInterestClicked(UIPointOfInterestButton _entry)
    {


        //pokud PoI na kterou klikam uz je vybrana (tedy klikam po 2.) a neni to moje PoI, jdeme cestovat
        if (_entry == selectedUIPointOfInterestButton && _entry.Data.id != AccountDataSO.CharacterData.position.pointOfInterestId)
        {
            _entry.TravelToThisPoI();
        }

        // pokud je to PoI na ktere jsem, tak do ni jednoduce vlezem
        if (_entry.Data.id == AccountDataSO.CharacterData.position.pointOfInterestId)
        {

            selectedUIPointOfInterestButton = _entry;
            DijkstraMapMaker.ClearPlannedTravelPath();

            FirebaseCloudFunctionSO.ExplorePointOfInterest(selectedUIPointOfInterestButton.Data.id);

        }
        else //jinak ti ukazu kolik by te stala cesta tam
        {
            DijkstraMapMaker.ShowPlannedTravelPath(AccountDataSO.CharacterData.position.pointOfInterestId, _entry.Data.id, AccountDataSO.LocationData.GetScreenPositionsWithIds());

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
