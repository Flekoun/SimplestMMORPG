using System.Collections;
using System.Collections.Generic;
using simplestmmorpg.data;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using static UnityEngine.EventSystems.EventTrigger;

public class UIPointsOfInterestSpawner : MonoBehaviour
{
    //  public PointOfInterestIdDefinitionSOSet AllPointOfInterestIdDefinitionSOSet;
    public AccountDataSO AccountDataSO;
    public PrefabFactory PrefabFactory;
    public Transform Parent;
    public GameObject UIEntryPrefab;
    public GameObject UIEntryPrefab_NoDescriptionData;
    public RectTransform Content; //abychom nastavili spravnou vysku a sirku
    // public UILineMaker AllPathsUILineMaker;
    public DijkstraMapMaker DijkstraMapMaker;
    private float largestPositionX = 0;
    private float largestPositionY = 0;

    public UnityAction<UIPointOfInterestButton> OnUIEntryClicked;

    public List<UIPointOfInterestButton> EntryList = new List<UIPointOfInterestButton>();

    public void Awake()
    {
        //    AccountDataSO.OnLocationDataChanged += Refresh;

    }

    public void OnDestroy()
    {
        //      AccountDataSO.OnLocationDataChanged -= Refresh;

    }



    public void Refresh(bool _forceHardRefresh = false)
    {



        var reachableVertices = DijkstraMapMaker.GetReacheableVertices();

        //        EntryList.Clear();
        //        Utils.DestroyAllChildren(Parent);



        if (EntryList.Count == 0 || _forceHardRefresh)
        {
            EntryList.Clear();
            Utils.DestroyAllChildren(Parent);
            largestPositionX = 0;
            largestPositionY = 0;
            Debug.Log("---------------OK TAK HARD....");
            foreach (var vertex in AccountDataSO.LocationData.dijkstraMap.exportMap)
            {

                UIPointOfInterestButton entry = null;


                entry = PrefabFactory.CreateGameObject<UIPointOfInterestButton>(UIEntryPrefab, Parent);



                var worldPosition = new WorldPosition();
                worldPosition.pointOfInterestId = vertex.id;
                worldPosition.locationId = AccountDataSO.LocationData.id;
                worldPosition.zoneId = "DUNOTAR";


                entry.SetData(worldPosition);

                entry.OnClicked += UIEntryClicked;
                entry.OnQuestgiverClicked += OnQuestgiverClicked;
                entry.OnEncounterClicked += OnEncounterEntryClicked;
                entry.OnEncounterResultClicked += OnEncounterResultEntryClicked;
                entry.OnVendorClicked += OnVendorEntryClicked;
                entry.OnTrainerClicked += OnTrainerEntryClicked;

                entry.SetReachable(reachableVertices.Contains(vertex.id));

                EntryList.Add(entry);

                if (largestPositionX < Mathf.Abs(vertex.screenPosition.x))
                    largestPositionX = Mathf.Abs(vertex.screenPosition.x);

                if (largestPositionY < Mathf.Abs(vertex.screenPosition.y))
                    largestPositionY = Mathf.Abs(vertex.screenPosition.y);
            }
        }
        else
        {
            Debug.Log("---------------OK TAK SOFT....");

            foreach (var poi in EntryList)
            {
                poi.Refresh();
                poi.SetReachable(reachableVertices.Contains(poi.WorldPosition.pointOfInterestId));
            }
        }

        //largestPositionX = (largestPositionX + 500) * 2;
        //largestPositionY = (largestPositionY + 500) * 2;
        Content.sizeDelta = new Vector2((largestPositionX + 500) * 2, (largestPositionY + 500) * 2);

        // Debug.Log("HOTOVO smazany a vytvoreny");
    }

    public UIPointOfInterestButton GetPointOfInterestButtonAtCharacterPosition()
    {
        foreach (var item in EntryList)
        {
            if (AccountDataSO.PointOfInterestData.id == item.WorldPosition.pointOfInterestId)
                return item;
        }

        return null;
    }

    private void OnEncounterResultEntryClicked(UIEncounterResultEntry _entry)
    {
        UIManager.instance.UIEncounterResultDetailPanel.Show(_entry.Data);
    }

    private void OnTrainerEntryClicked(UITrainerEntry _entry)
    {
        UIManager.instance.UITrainerDetailPanel.Show(_entry.Data);
    }

    private void OnQuestgiverClicked(UIQuestgiverEntry _entry)
    {
        UIManager.instance.UIQuestgiverDetailPanel.Show(_entry.Data);
    }

    private void OnEncounterEntryClicked(UIEncounterEntryMap _entry)
    {
        UIManager.instance.UIEncounterDetailSwitcher.Show(_entry.Data);
    }

    private void OnVendorEntryClicked(UIVendorEntry _entry)
    {
        UIManager.instance.UIVendorDetailPanel.Show(_entry.Data);
    }


    private void UIEntryClicked(UIPointOfInterestButton _data)
    {
        if (OnUIEntryClicked != null)
            OnUIEntryClicked.Invoke(_data);

    }

    public UIPointOfInterestButton GetPoIPlayerIsCurrentlyOn()
    {
        foreach (var item in EntryList)
        {
            if (item.WorldPosition.pointOfInterestId == AccountDataSO.CharacterData.position.pointOfInterestId)
                return item;
        }

        return null;
    }
}
