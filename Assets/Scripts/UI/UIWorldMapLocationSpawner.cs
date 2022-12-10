using System.Collections;
using System.Collections.Generic;
using simplestmmorpg.data;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class UIWorldMapLocationSpawner : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    public PrefabFactory PrefabFactory;
    public Transform Parent;
    public GameObject UIEntryPrefab;
    // public UILineMaker AllPathsUILineMaker;

    public UnityAction<UIWorldMapLocationButton> OnUIEntryClicked;

    public List<UIWorldMapLocationButton> EntryList = new List<UIWorldMapLocationButton>();

    public void Awake()
    {
        //nechci to ridit od tud protoze ten UIWorldMap taky poslouch na onMapsChanged a pak tu sou konflikty ,vubec tenhle spawner je k hovnu kdyz je tak fixne stejne navazany na UIWorldMap

        //AccountDataSO.OnMapsChanged += SpawnWorldMap;

    }

    public void OnDestroy()
    {
        //AccountDataSO.OnMapsChanged -= SpawnWorldMap;

    }

    public void ShowMapLocationButton(BaseIdDefinition _locationDef)
    {
        foreach (var item in EntryList)
        {
            if (item.Data == _locationDef.Id)
                item.Show(true);
        }
    }

    public void SpawnWorldMap()
    {
        EntryList.Clear();
        Utils.DestroyAllChildren(Parent);

        foreach (var vertex in AccountDataSO.MapsData.worldMap)
        {
            var entry = PrefabFactory.CreateGameObject<UIWorldMapLocationButton>(UIEntryPrefab, Parent);
            entry.SetData(vertex.id);
            entry.OnClicked += UIEncounterEntryClicked;
            EntryList.Add(entry);
            entry.Show(false);

        }

    }

    public void UIEncounterEntryClicked(UIWorldMapLocationButton _data)
    {


        if (OnUIEntryClicked != null)
            OnUIEntryClicked.Invoke(_data);

    }
}
