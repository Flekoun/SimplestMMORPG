using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using simplestmmorpg.data;
using static UnityEngine.GraphicsBuffer;

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

    // public UIEnterDungeon UIEnterDungeon;

    public Image BackgroundImage;
    public DijkstraMapMaker DijkstraMapMaker;

    private UIPointOfInterestButton selectedUIPointOfInterestButton = null;
    // private Coroutine ViewMoveCoroutine = null;

    public void Awake()
    {

        AccountDataSO.OnPointOfInterestDataChanged += OnWorldPointOfInterestChanged;
        AccountDataSO.OnLocationDataChanged += OnWorldLocationChanged;
        UIPointsOfInterestSpawner.OnUIEntryClicked += OnPointOfInterestClicked;

        // DijkstraMapMaker.OnVertexReachable += OnVertexReachable;

        AccountDataSO.OnPartyDataChanged += RefreshMap;  // mam to tu kvuli tomu kdyz parta jde do dungeonu, tak se ulozi do ni ze maji prozkoumanou 1. entrance lokaci, tak musim refreshnout mapu abych ji videl.....
        AccountDataSO.OnCharacterDataChanged += RefreshMap;

        //tohle sem pridal kvuli AdminTools....at muzu ru
        //  AccountDataSO.OnLocationDataChanged += RefreshMap;
    }

    //private void OnVertexReachable(DijkstraMapVertex _vertexDef)
    //{
    //    UIPointsOfInterestSpawner.ShowPointOfInterestButton(_vertexDef);
    //}

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

    public void Hide()
    {


      
        UIQuestgiverSpawner.OnRefreshed -= Refresh;
        UIVendorSpawner.OnRefreshed -= Refresh;
        UISpecialsSpawner.OnRefreshed -= Refresh;
        UITrainerSpawner.OnRefreshed -= Refresh;

        Model.gameObject.SetActive(false);
    }

    private void Refresh()
    {
        UIPointsOfInterestSpawner.Refresh();

        DijkstraMapMaker.SetupForCurrentLocation();//, AccountDataSO.LocationData.GetScreenPositionsWithIds());

        RefreshButtons();

        //if (UIPointsOfInterestSpawner.GetPointOfInterestButtonAtCharacterPosition() != null)
        //{
        //    Vector3 pos1 = UIPointsOfInterestSpawner.GetPointOfInterestButtonAtCharacterPosition().transform.localPosition;
        //    Vector3 pos2 = new Vector3((-1) * pos1.x, (-1) * pos1.y, pos1.z);
        //    //   ScrollContent.localPosition = pos2;

        //    if (ViewMoveCoroutine != null)
        //        StopCoroutine(ViewMoveCoroutine);

        //    ViewMoveCoroutine = StartCoroutine(MoveView(pos2));
        //}
        //else
        //    Debug.LogError("Jaktoze nejsem na zadnem PoI buttonu? Kde sem?");

    }

    //    private IEnumerator MoveView(Vector3 targetPosition)
    //    {
    //        float startTime = Time.time;
    //        while (Vector3.Distance(ScrollContent.localPosition, targetPosition) > 10 && (Time.time - startTime) < 1f)
    //        {
    ////            Debug.Log(ScrollContent.localPosition);
    // //           Debug.Log(targetPosition);
    //            ScrollContent.localPosition = Vector3.Lerp(ScrollContent.localPosition, targetPosition, 2f * Time.deltaTime);
    //            yield return null;
    //        }
    //    }



    private void OnQuestGiverClicked(UIQuestgiverEntry _entry)
    {

    }
    private void OnPointOfInterestClicked(UIPointOfInterestButton _entry)
    {

        Debug.Log("clicked");
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
            Debug.Log("clicked showing");
            DijkstraMapMaker.ShowPlannedTravelPath(AccountDataSO.CharacterData.position.pointOfInterestId, _entry.WorldPosition.pointOfInterestId);

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
