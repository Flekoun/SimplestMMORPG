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
    private List<InboxItem> InboxToShow;

    public UnityAction<UIInboxItemEntry> OnUIEntryClicked;

    //   public List<UIEncounterResultEntry> UIEntriesList = new List<UIEncounterResultEntry>();

    public void Awake()
    {
        AccountDataSO.OnInboxDataCharacterChanged += Refresh;
        AccountDataSO.OnInboxDataPlayerChanged += Refresh;
    }

    public void OnDestroy()
    {
        AccountDataSO.OnInboxDataCharacterChanged -= Refresh;
        AccountDataSO.OnInboxDataPlayerChanged -= Refresh;
    }

    public void Setup(bool _showPlayerInbox)
    {
        if (_showPlayerInbox)
            InboxToShow = AccountDataSO.InboxDataPlayer;
        else
            InboxToShow = AccountDataSO.InboxDataCharacter;

        Refresh();
    }

    public void Refresh()
    {
        if (InboxToShow == null)
            return;

        Utils.DestroyAllChildren(Parent);

        foreach (var inboxItem in InboxToShow)
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
