using System.Collections;
using System.Collections.Generic;
using simplestmmorpg.data;
using UnityEngine;
using UnityEngine.Events;

public class UIInboxItemsSpawner : MonoBehaviour
{
  
    public AccountDataSO AccountDataSO;
    public PrefabFactory PrefabFactory;
    public Transform Parent;
    public GameObject UIEntryPrefab;
    //public UIEncounterResultEntry SelectedEncounter;

    public UnityAction<UIInboxItemEntry> OnUIEntryClicked;

 //   public List<UIEncounterResultEntry> UIEntriesList = new List<UIEncounterResultEntry>();

    public void Awake()
    {
        AccountDataSO.OnInboxDataChanged += Refresh;
    }

    public void OnDestroy()
    {
        AccountDataSO.OnInboxDataChanged -= Refresh;
    }


    public void Refresh()
    {
        Utils.DestroyAllChildren(Parent);

        foreach (var inboxItem in AccountDataSO.InboxData)
        {
            var entry = PrefabFactory.CreateGameObject<UIInboxItemEntry>(UIEntryPrefab, Parent);
            entry.SetData(inboxItem);
            entry.OnClicked += UIEncounterEntryClicked;
        }
    }


    public void UIEncounterEntryClicked(UIInboxItemEntry _data)
    {
        OnUIEntryClicked.Invoke(_data);

    }
}
