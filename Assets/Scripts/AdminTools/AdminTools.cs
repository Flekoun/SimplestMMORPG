using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdminTools : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    public FirebaseCloudFunctionSO_Admin FirebaseCloudFunctionSO_Admin;

    //  public GameObject EnemyDropTablesPanel;

    public GameObject Model;

    public void Start()
    {
        AccountDataSO.OnPlayerDataChanged += OnPlayerDataChanged;
    }
    // Start is called before the first frame update
    //public void ShowEnemyDropTables()
    //{
    //    EnemyDropTablesPanel.gameObject.SetActive(true);
    //}

    private void OnPlayerDataChanged()
    {
        if (Model != null)
            Model.gameObject.SetActive(true);
    }

    public void StorePointOfInterestScreenPositions(UIPointsOfInterestSpawner _spawner)
    {
        FirebaseCloudFunctionSO_Admin.SavePointOfInterestScreenPositionsForLocation(_spawner.EntryList);
    }

    //public void Show()
    //{
    //    Model.gameObject.SetActive(true);
    //}

}
