using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.EventSystems.EventTrigger;

public class UIWorldMap : MonoBehaviour
{
    public BaseDefinitionSOSet LocationsSOSet;
    public AccountDataSO AccountDataSO;
    public UIWorldMapLocationSpawner UIWorldMapLocationSpawner;
    public GameObject Model;
    public DijkstraMapMaker DijkstraMapMaker;

    //  jestli tyhle dijskara veci a vsechno s tema butonama z hodoble nedat do toho spawneru, ten udelal nejak vic obecne i s buttonama, neco jako Djiskra location spawner a Djiskra node button
    //protoze spawner i ty buttony jsou defakto stejne v mape i lokaci ..... jen se lisi djiskra mapou kterou pouzivaji a pak co konkretne se deje pri kliku na ne

    private UIWorldMapLocationButton selectedWorldLocationButton = null;
    public void Awake()
    {
        UIWorldMapLocationSpawner.OnUIEntryClicked += OnWorldPositionButtonClicked;
        AccountDataSO.OnWorldPositionChanged += OnWorldPositionChanged;
        AccountDataSO.OnMapsChanged += OnMapsChanged;
        DijkstraMapMaker.OnVertexReachable += OnVertexReachable;
    }

    private void OnVertexReachable(BaseIdDefinition _vertexDef)
    {
        UIWorldMapLocationSpawner.ShowMapLocationButton(_vertexDef);
    }

    private void OnMapsChanged()
    {
        UIWorldMapLocationSpawner.SpawnWorldMap();
        DijkstraMapMaker.Setup(AccountDataSO.MapsData.worldMap, LocationsSOSet);
        RefreshButtons();
    }

    private void OnWorldPositionButtonClicked(UIWorldMapLocationButton _entry)
    {

        //pokud lokace na kterou klikam uz je vybrana (tedy klikam po 2.) a neni to moje lokace, jdeme cestovat
        if (_entry == selectedWorldLocationButton && _entry.LocationDef.Id != AccountDataSO.CharacterData.position.locationId)
        {
            _entry.TravelToThisLocation();
        }

        // pokud je to lokace na ktere jsem, tak do ni jednoduce vlezem
        if (_entry.LocationDef.Id == AccountDataSO.CharacterData.position.locationId)
        {
            DijkstraMapMaker.ClearPlannedTravelPath();

            if (_entry.LocationDef.IsTownLocation)
                OnOpenTownLocation.Invoke();
            else if (_entry.LocationDef.IsEncounterLocation)
                OnOpenEncounterLocation.Invoke();
        }
        else //jinak ti ukazu kolik by te stala cesta tam
        {
            DijkstraMapMaker.ShowPlannedTravelPath(AccountDataSO.CharacterData.position.locationId, _entry.Data, LocationsSOSet);

        }

        selectedWorldLocationButton = _entry;

        RefreshButtons();

    }

    public void OnWorldPositionChanged()
    {
        selectedWorldLocationButton = null;

        UIWorldMapLocationSpawner.SpawnWorldMap();
        DijkstraMapMaker.Setup(AccountDataSO.MapsData.worldMap, LocationsSOSet);

        DijkstraMapMaker.ClearPlannedTravelPath();
        RefreshButtons();
    }

    private void RefreshButtons()
    {
        foreach (var item in UIWorldMapLocationSpawner.EntryList)
        {
            if (DijkstraMapMaker.PlannedPathNewest != null)
                item.RefreshButtonDisplay(selectedWorldLocationButton, DijkstraMapMaker.PlannedPathNewest.totalWeight);
            else
                item.RefreshButtonDisplay(selectedWorldLocationButton, 0);
        }
    }

    // Start is called before the first frame update
    public void Show()
    {
        Model.gameObject.SetActive(true);
        OnWorldPositionChanged();

        Debug.Log("------TTTESSSSZTZZZ----------");
    }

    public void Hide()
    {
        Model.gameObject.SetActive(false);
    }

    public UnityEvent OnOpenEncounterLocation;
    public UnityEvent OnOpenTownLocation;
}
