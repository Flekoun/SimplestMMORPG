using System.Collections;
using System.Collections.Generic;
using simplestmmorpg.data;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIEncountersSpawner : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    public PrefabFactory PrefabFactory;
    public Transform PersonalEncountersParent;
    public GameObject UIEncouterDetailPrefab;

    public UnityAction<EncounterData> OnUIEnecounterEntryClicked;

    public List<UIEncounterEntry> UIEncountersList = new List<UIEncounterEntry>();


    public void Awake()
    {
        AccountDataSO.OnEncounterDataChanged += Refresh;
    }

    public void OnDestroy()
    {
        AccountDataSO.OnEncounterDataChanged -= Refresh;
    }

    public void OnEnable()
    {
        Refresh();
    }



    void Refresh()
    {

        Utils.DestroyAllChildren(PersonalEncountersParent, 1);
        //vytvori nebo reusne encounter
        for (int i = 0; i < AccountDataSO.EncountersData.Count; i++)
        {

            UIEncounterEntry encounterUI = PrefabFactory.CreateGameObject<UIEncounterEntry>(UIEncouterDetailPrefab, PersonalEncountersParent);
            encounterUI.SetEncounter(AccountDataSO.EncountersData[i]);//, this);
            encounterUI.IgnoreClicksOnEntries = true;
            encounterUI.OnClicked += UIEncounterEntryClicked;
            UIEncountersList.Add(encounterUI);
        }


        //vytvori nebo reusne encounter
        //for (int i = 0; i < AccountDataSO.EncountersData.Count; i++)
        //{
        //    if (UIEncountersList.Count > i)
        //    {
        //        UIEncountersList[i].SetEncounter(AccountDataSO.EncountersData[i]);//, this);
        //        UIEncountersList[i].gameObject.SetActive(true);
        //    }
        //    else
        //    {
        //        UIEncounterEntry encounterUI = PrefabFactory.CreateGameObject<UIEncounterEntry>(UIEncouterDetailPrefab, PersonalEncountersParent);
        //        Debug.Log("AccountDataSO.EncountersData[i] : " + AccountDataSO.EncountersData[i].expireDate);
        //        encounterUI.SetEncounter(AccountDataSO.EncountersData[i]);//, this);
        //        encounterUI.IgnoreClicksOnEntries = true;
        //        encounterUI.OnClicked += UIEncounterEntryClicked;
        //        UIEncountersList.Add(encounterUI);
        //    }
        //}
    }

    public void UIEncounterEntryClicked(UIEncounterEntry _data)
    {
        OnUIEnecounterEntryClicked.Invoke(_data.Data);
    }
}
