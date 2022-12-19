using System.Collections;
using System.Collections.Generic;
using simplestmmorpg.data;
using UnityEngine;
using UnityEngine.Events;

public class UIVendorSpawner : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    public PrefabFactory PrefabFactory;
    public Transform Parent;
    public GameObject UIVendorPrefab;
    public UIVendorDetailPanel UIVendorDetailPanel;

    public List<UIVendorEntry> UIEntriesList = new List<UIVendorEntry>();
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
        Debug.Log("kdo to zavolal??");
        Utils.DestroyAllChildren(Parent,1);
        UIEntriesList.Clear();
        foreach (var vendor in AccountDataSO.GetCurrentPointOfInterest().vendors)
        {
            var vendorUI = PrefabFactory.CreateGameObject<UIVendorEntry>(UIVendorPrefab, Parent);
            vendorUI.SetData(vendor);
            UIEntriesList.Add(vendorUI);
            vendorUI.OnClicked += VendorEntryClicked;
           // UIEntriesList.Add(vendorUI);
        }

        if (OnRefreshed != null)
            OnRefreshed.Invoke();

        ////schova encounterUI kterych je vic nez je encounterDat 
        //for (int i = 0; i < UIEntriesList.Count; i++)
        //{
        //    if (i > AccountDataSO.EncounterResultsData.Count-1)
        //        UIEntriesList[i].gameObject.SetActive(false);
        //}

        ////vytvori nebo reusne encounter
        //for (int i = 0; i < AccountDataSO.VendorsData.Count; i++)
        //{
        //    if (UIEntriesList.Count > i)
        //    {
        //        //                Debug.Log("REUSE::::");
        //        UIEntriesList[i].SetData(AccountDataSO.VendorsData[i]);
        //        UIEntriesList[i].gameObject.SetActive(true);
        //    }
        //    else
        //    {
        //        //      Debug.Log("WG:::::");
        //        var vendorUI = PrefabFactory.CreateGameObject<UIVendorEntry>(UIVendorPrefab, Parent);
        //        vendorUI.SetData(AccountDataSO.VendorsData[i]);
        //        UIEntriesList.Add(vendorUI);


        //    }
        //}
    }

    private void VendorEntryClicked(UIVendorEntry _entry)
    {
        UIVendorDetailPanel.Show(_entry.Data);
    }

}
