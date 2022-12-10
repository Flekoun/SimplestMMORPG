using System.Collections;
using System.Collections.Generic;
using simplestmmorpg.data;
using UnityEngine;
using UnityEngine.Events;

public class UIEncountersResultSpawner : UISelectableSpawner
{
    public AccountDataSO AccountDataSO;
    public PrefabFactory PrefabFactory;
    public Transform Parent;
    public GameObject UIEncouterDetailPrefab;
    //public UIEncounterResultEntry SelectedEncounter;

    public UnityAction<UIEncounterResultEntry> OnUIEntryClicked;

    public List<UIEncounterResultEntry> UIEntriesList = new List<UIEncounterResultEntry>();



    public void Awake()
    {
        AccountDataSO.OnEncounterResultsDataChanged += Refresh;
    }

    public void OnDestroy()
    {
        AccountDataSO.OnEncounterResultsDataChanged -= Refresh;
    }

    public void OnEnable()
    {
        Refresh();
    }

    void Refresh()
    {
        // Debug.Log("Encounter data changed");

        ////schova encounterUI kterych je vic nez je encounterDat 
        //for (int i = 0; i < UIEntriesList.Count; i++)
        //{
        //    if (i > AccountDataSO.EncounterResultsData.Count - 1)
        //        UIEntriesList[i].gameObject.SetActive(false);
        //}

        ////vytvori nebo reusne encounter
        //for (int i = 0; i < AccountDataSO.EncounterResultsData.Count; i++)
        //{
        //    if (UIEntriesList.Count > i)
        //    {
        //        //                Debug.Log("REUSE::::");
        //        UIEntriesList[i].SetEncounter(AccountDataSO.EncounterResultsData[i], this);
        //        UIEntriesList[i].gameObject.SetActive(true);
        //    }
        //    else
        //    {
        //        //      Debug.Log("WG:::::");
        //        UIEncounterResultEntry encounterUI = PrefabFactory.CreateGameObject<UIEncounterResultEntry>(UIEncouterDetailPrefab, Parent);
        //        encounterUI.SetEncounter(AccountDataSO.EncounterResultsData[i], this);
        //        encounterUI.OnClicked += OnEntryClicked;
        //        UIEntriesList.Add(encounterUI);


        //    }
        //}

        Utils.DestroyAllChildren(Parent, 1);
        for (int i = 0; i < AccountDataSO.EncounterResultsData.Count; i++)
        {
            UIEncounterResultEntry encounterUI = PrefabFactory.CreateGameObject<UIEncounterResultEntry>(UIEncouterDetailPrefab, Parent);
            encounterUI.SetEncounter(AccountDataSO.EncounterResultsData[i], this);
            encounterUI.OnClicked += OnEntryClicked;
            UIEntriesList.Add(encounterUI);
        }

    }

    private void OnEntryClicked(UIEncounterResultEntry _entry)
    {
       // this.OnUISelectableItemClicked(_entry);
        if (OnUIEntryClicked != null)
            OnUIEntryClicked.Invoke(_entry);

    }

    //public void UIEncounterEntryClicked(EncounterResult _data)
    // {
    //     base.OnUISelectableItemClicked(_data);
    //     OnUIEntryClicked.Invoke(_data);

    // }
}
