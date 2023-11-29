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



    public void UIEncounterEntryClicked(UIEncounterEntry _data)
    {
        OnUIEnecounterEntryClicked.Invoke(_data.Data);
    }
}
