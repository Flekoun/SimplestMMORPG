using System.Collections;
using System.Collections.Generic;
using simplestmmorpg.data;
using UnityEngine;
using UnityEngine.Events;

public class UITrainerSpawner : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    public PrefabFactory PrefabFactory;
    public Transform Parent;
    public GameObject UIPrefab;
    public UITrainerDetailPanel UITrainerDetailPanel;

    public List<UITrainerEntry> UIEntriesList = new List<UITrainerEntry>();
    public UnityAction OnRefreshed;
    public void Awake()
    {
        //AccountDataSO.OnCharacterLoadedFirstTime += Refresh;
        //   AccountDataSO.OnWorldPositionChanged += Refresh;
    }

    public void OnDisable()
    {
        //   AccountDataSO.OnVendorsDataChanged -= Refresh;
        AccountDataSO.OnWorldPointOfInterestChanged -= Refresh;

    }

    public void OnEnable()
    {
        //  AccountDataSO.OnVendorsDataChanged += Refresh;
        //  AccountDataSO.OnWorldPositionChanged += Refresh;
        AccountDataSO.OnWorldPointOfInterestChanged += Refresh;
        Refresh();
    }

    public bool HasSpawnedAnyVendors()
    {
        return UIEntriesList.Count > 0;
    }

    void Refresh()
    {
        Utils.DestroyAllChildren(Parent, 1);
        UIEntriesList.Clear();
        //foreach (var trainer in AccountDataSO.PointOfInterestData.trainers)
        //{
        //    var trainerUI = PrefabFactory.CreateGameObject<UITrainerEntry>(UIPrefab, Parent);
        //    trainerUI.SetData(trainer);
        //    UIEntriesList.Add(trainerUI);
        //    trainerUI.OnClicked += EntryClicked;
        //}

        if (OnRefreshed != null)
            OnRefreshed.Invoke();
    }

    private void EntryClicked(UITrainerEntry _entry)
    {
        UITrainerDetailPanel.Show(_entry.Data);
    }

}
