using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using simplestmmorpg.data;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.Events;
using static UnityEngine.ParticleSystem;

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

    //   public Transform ScrollContent;

    public GameObject QuestsPanel;
    public GameObject EncountersPanel;
    public GameObject EncounterRewardsPanel;
    public GameObject VendorsPanel;
    public GameObject SpecialsPanel;
    public GameObject TrainersPanel;

    public TextMeshProUGUI PoIChooserModeText;
    // public UIEnterDungeon UIEnterDungeon;

    public Image BackgroundImage;
    public DijkstraMapMaker DijkstraMapMaker;

    private UIPointOfInterestButton selectedUIPointOfInterestButton = null;
    private UnityAction<string> OnPoIChoosen;
    private bool IsPoIChooserModeOn = false; //POI chooser mod vyuzivam na vybirani POI pro nejakou akci, treba teleport nebo carriage
                                             // private Coroutine ViewMoveCoroutine = null;

    public void DisablePoIChooser()
    {
        IsPoIChooserModeOn = false;
        PoIChooserModeText.gameObject.SetActive(false);

        //if (IsPoIChooserModeOn)
        //{

        //}
    }

    public void SetPoIChooser(UnityAction<string> _onPoIChoosen, string _textToShow)
    {
        OnPoIChoosen = _onPoIChoosen;
        IsPoIChooserModeOn = true;
        PoIChooserModeText.gameObject.SetActive(true);
        PoIChooserModeText.SetText(_textToShow);

        //if (IsPoIChooserModeOn)
        //{

        //}
    }

    public void Awake()
    {

        AccountDataSO.OnPointOfInterestDataChanged += OnWorldPointOfInterestChanged;
        AccountDataSO.OnLocationDataChanged += OnWorldLocationChanged;
        UIPointsOfInterestSpawner.OnUIEntryClicked += OnPointOfInterestClicked;

    }



    private void RefreshMap()
    {

        //    if (AccountDataSO.LocationData.GetScreenPositionsWithIds().Count > 0) //kdyz partdyData changed toto zavola nazacatku tak nejsou jeste ani nactene data lokace a pak je tu error....uf
        DijkstraMapMaker.SetupForCurrentLocation();//, AccountDataSO.LocationData.GetScreenPositionsWithIds());
        Refresh();
    }

    private void OnWorldLocationChanged()
    {
        Refresh();
        DijkstraMapMaker.ClearPlannedTravelPath();
    }

    private void OnWorldPointOfInterestChanged()
    {
        Refresh();
        DijkstraMapMaker.ClearPlannedTravelPath();
    }
    // Start is called before the first frame update
    public void Show()
    {
        //Pridal sem to tu kvuli Admin tools, ale proc ne? stejne kdyz ukazuju mapu tak chci aby byla takto nastavena na?
        DijkstraMapMaker.SetupForCurrentLocation();

        Model.gameObject.SetActive(true);

        UIQuestgiverSpawner.OnRefreshed += Refresh;
        UIVendorSpawner.OnRefreshed += Refresh;
        UISpecialsSpawner.OnRefreshed += Refresh;
        UITrainerSpawner.OnRefreshed += Refresh;

        Refresh();
    }

    public void ShowWithHardRefresh()
    {
        //Show();
        //RefreshHard();
        DijkstraMapMaker.SetupForCurrentLocation(true);
        UIPointsOfInterestSpawner.Refresh(true);

        Model.gameObject.SetActive(true);

        UIQuestgiverSpawner.OnRefreshed += Refresh;
        UIVendorSpawner.OnRefreshed += Refresh;
        UISpecialsSpawner.OnRefreshed += Refresh;
        UITrainerSpawner.OnRefreshed += Refresh;

        Refresh();
    }

    public void Hide()
    {

        UIQuestgiverSpawner.OnRefreshed -= Refresh;
        UIVendorSpawner.OnRefreshed -= Refresh;
        UISpecialsSpawner.OnRefreshed -= Refresh;
        UITrainerSpawner.OnRefreshed -= Refresh;

        Model.gameObject.SetActive(false);
    }

    private void RefreshHard()
    {
        DijkstraMapMaker.SetupForCurrentLocation(true);
        UIPointsOfInterestSpawner.Refresh(true);

        RefreshButtons();
    }
    private void Refresh()
    {
        DijkstraMapMaker.SetupForCurrentLocation();
        UIPointsOfInterestSpawner.Refresh();

        RefreshButtons();

    }


    private void OnPointOfInterestClicked(UIPointOfInterestButton _entry)
    {
        if (IsPoIChooserModeOn)
        {
            OnPoIChoosen?.Invoke(_entry.WorldPosition.pointOfInterestId);
            DisablePoIChooser();
            return;
        }

        //    Debug.Log("clicked");
        //pokud PoI na kterou klikam uz je vybrana (tedy klikam po 2.) a neni to moje PoI, jdeme cestovat
        if (_entry == selectedUIPointOfInterestButton && _entry.WorldPosition.pointOfInterestId != AccountDataSO.CharacterData.position.pointOfInterestId)
        {
            _entry.TravelToThisPoI();
        }

        // pokud je to PoI na ktere jsem, tak do ni jednoduce vlezem
        if (_entry.WorldPosition.pointOfInterestId == AccountDataSO.CharacterData.position.pointOfInterestId)
        {

            //      selectedUIPointOfInterestButton = _entry;
            DijkstraMapMaker.ClearPlannedTravelPath();

            _entry.ExploreThisPoI();
            //  await FirebaseCloudFunctionSO.ExplorePointOfInterest(selectedUIPointOfInterestButton.WorldPosition.pointOfInterestId);

            //UIManager.instance.ImportantMessage.ShowMesssage("New location discovered!");

        }
        else //jinak ti ukazu kolik by te stala cesta tam
        {
            //       Debug.Log("clicked showing");
            DijkstraMapMaker.ShowPlannedTravelPath(AccountDataSO.CharacterData.position.pointOfInterestId, _entry.WorldPosition.pointOfInterestId);

        }

        selectedUIPointOfInterestButton = _entry;

        RefreshButtons();


    }

    private void RefreshButtons()
    {
        foreach (var item in UIPointsOfInterestSpawner.EntryList)
        {
            //if (DijkstraMapMaker.PlannedPathNewest != null)
            //    item.RefreshButtonDisplay(selectedUIPointOfInterestButton, DijkstraMapMaker.PlannedPathNewest.totalWeight);
            //else
            //    item.RefreshButtonDisplay(selectedUIPointOfInterestButton, 0);
            item.RefreshButtonDisplay(selectedUIPointOfInterestButton, DijkstraMapMaker.GetPlannedPathCost());
        }
    }


    //private void TryToFixScrollReckGlitches()
    //{
    //    // ContentFitterRefresh.RefreshContentFitters();
    //    StartCoroutine(Wait());
    //}

    //IEnumerator Wait()
    //{
    //    yield return new WaitForEndOfFrame();
    //    ContentFitterRefresh.RefreshContentFitters();
    //}

}
