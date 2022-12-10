using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SearchForNewEncounters : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    public FirebaseCloudFunctionSO FirebaseCloudFunctionSO;
    public bool IsEnabled = true;
    private float SearchDelayInit = 2f;
    private float SearchInterval = 30f;
    private int MaxEncounters = 3;

    private bool isSearching = false;


    private IEnumerator Search(float _delay)
    {
        Debug.Log("Started searching....");
        isSearching = true;
        yield return new WaitForSecondsRealtime(_delay);

        SearchForEncounters();
        SearchOver();
    }

    private void SearchForEncounters()
    {


        if (AccountDataSO.EncountersData.Count >= MaxEncounters)
        {

            Debug.Log("Uz jsi dosahl max. poctu encounteru, nehledm dalsi: " + MaxEncounters);
            return;
        }

        if (!AccountDataSO.EncountersContainsEncounterCreatedByMe())
        {
          //  FirebaseCloudFunctionSO.CreateEncounter();
        }
        else
        {
//            FirebaseCloudFunctionSO.SearchForRandomEncounterFromOthers();
        }


    }

    public void OnEnable()
    {
        AccountDataSO.OnEncounterLoadedFirstTime += OnEncounterDataLoaded;
    }
    public void OnDestroy()
    {
        AccountDataSO.OnEncounterLoadedFirstTime -= OnEncounterDataLoaded;
    }

    private void OnEncounterDataLoaded()
    {
        if (IsEnabled)
            StartSearching();
    }
 
    private void SearchOver()
    {
        Debug.Log("Seach is over");
        isSearching = false;

        RunSearchCoroutine(SearchInterval);
    }

    private void StartSearching()
    {
        RunSearchCoroutine(SearchDelayInit);
    }


    private void RunSearchCoroutine(float _delay)
    {
        if (!isSearching)
            StartCoroutine(Search(_delay));
        else
            Debug.LogWarning("already searching! why you called this?");
    }

}
