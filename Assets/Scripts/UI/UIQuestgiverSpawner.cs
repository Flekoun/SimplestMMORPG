using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using simplestmmorpg.data;
using System.Linq;

public class UIQuestgiverSpawner : UISelectableSpawner
{
    public AccountDataSO AccountDataSO;
    public WorldPosition ThisWorldPostion;
    public PrefabFactory PrefabFactory;
    public Transform Parent;
    public GameObject UIEntryPrefab;
    public UnityAction<UIQuestgiverEntry> OnEntryClicked;
    public UnityAction OnRefreshed;

    public List<UIQuestgiverEntry> UIEntryList = new List<UIQuestgiverEntry>();



    public void OnEnable()
    {
        Refresh();

        AccountDataSO.OnWorldPointOfInterestChanged += Refresh;
        AccountDataSO.OnCharacterDataChanged += Refresh;
    }

    public void OnDisable()
    {
        AccountDataSO.OnWorldPointOfInterestChanged -= Refresh;
        AccountDataSO.OnCharacterDataChanged -= Refresh;
    }

    public bool HasSpawnedAnyQuests()
    {
        return UIEntryList.Count > 0;
    }

    public void Refresh()
    {

        Utils.DestroyAllChildren(Parent, 1);


        UIEntryList.Clear();
        foreach (var item in AccountDataSO.PointOfInterestData.GetValidQuestGivers(AccountDataSO.CharacterData))
        {

            var entryUI = PrefabFactory.CreateGameObject<UIQuestgiverEntry>(UIEntryPrefab, Parent);
            entryUI.SetData(item);
            UIEntryList.Add(entryUI);
            entryUI.OnClicked += OnQuestgiverEntryClicked;
        }

        if (OnRefreshed != null)
            OnRefreshed.Invoke();
    }



    public void OnQuestgiverEntryClicked(UIQuestgiverEntry _entry)
    {
        OnEntryClicked?.Invoke(_entry);
    }






}
