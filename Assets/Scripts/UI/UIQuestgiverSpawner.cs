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
    //    public ListenOnQuestgivers ListenOnQuestgivers;
    public UnityAction<UIQuestgiverEntry> OnEntryClicked;
    public UnityAction OnRefreshed;

    public List<UIQuestgiverEntry> UIEntryList = new List<UIQuestgiverEntry>();

    //public List<QuestgiverMeta> Data;
    //  public GameObject Model;

    public void Awake()
    {
        //  ListenOnQuestgivers.StartListeningOnQuestgiversAtCharacterPosition();
        //  ListenOnQuestgivers.OnQuestgiversAtCharacterWorldPositionChanged += Setup;
        //   AccountDataSO.OnWorldPositionChanged += Refresh;
       // AccountDataSO.OnCharacterDataChanged += Refresh;
      //  AccountDataSO.OnWorldPositionChanged += Refresh;
    }

    public void OnEnable()
    {
        Refresh();

        AccountDataSO.OnWorldPointOfInterestChanged += Refresh;
    }

    public void OnDisable()
    {
       AccountDataSO.OnWorldPointOfInterestChanged -= Refresh;
    }

    public bool HasSpawnedAnyQuests()
    {
        return UIEntryList.Count > 0;
    }
    
    public void Refresh()
    {

            Debug.Log("REFRESHING QUESTS IN SPAWNER");
        Utils.DestroyAllChildren(Parent, 1);

        //TADY SE FILTRUJOU QUEST GIVERI...DOBRE MISTO?!!

        //vyfiltruju questgivery ktere jsem uz splnil pryc.....
        var tempListToBeFiltered = new List<QuestgiverMeta>(AccountDataSO.GetCurrentPointOfInterest().questgivers);

        for (int i = tempListToBeFiltered.Count - 1; i >= 0; i--)
        {
            if (AccountDataSO.CharacterData.questgiversClaimed.Contains(tempListToBeFiltered[i].id))
                tempListToBeFiltered.RemoveAt(i);
        }


        //vyfiltruju questgivery podle prereQestu.....
        for (int i = tempListToBeFiltered.Count - 1; i >= 0; i--)
        {
            int prereqsFound = 0;

            foreach (var item in tempListToBeFiltered[i].prereqQuests)
            {
                if (AccountDataSO.CharacterData.questgiversClaimed.Contains(item))
                    prereqsFound++;
            }

            if (prereqsFound < tempListToBeFiltered[i].prereqQuests.Count)
            {
                //    Debug.Log("Fillteriung out : " + tempListToBeFiltered[i].uid + " prereqsQuestsFound:  " + prereqsFound + " needed : " + tempListToBeFiltered[i].prereqQuests.Count);
                tempListToBeFiltered.RemoveAt(i);
            }
        }


        //vyfiltruju questgivery podle prereqPoI.....
        for (int i = tempListToBeFiltered.Count - 1; i >= 0; i--)
        {
            int prereqsFound = 0;

            foreach (var item in tempListToBeFiltered[i].prereqExploredPointsOfInterest)
            {
                if (AccountDataSO.CharacterData.exploredPositions.pointsOfInterest.Contains(item))
                    prereqsFound++;
            }

            if (prereqsFound < tempListToBeFiltered[i].prereqExploredPointsOfInterest.Count)
            {
                //       Debug.Log("Fillteriung out : " + tempListToBeFiltered[i].uid + " prereqsPoIFound:  " + prereqsFound + " needed : " + tempListToBeFiltered[i].prereqExploredPointsOfInterest.Count);
                tempListToBeFiltered.RemoveAt(i);
            }
        }

        //vyfiltruju questgivery podle levelu.....
        UIEntryList.Clear();
        foreach (var item in tempListToBeFiltered)
        {
            if (item.minLevel <= AccountDataSO.CharacterData.stats.level)
            {
                var entryUI = PrefabFactory.CreateGameObject<UIQuestgiverEntry>(UIEntryPrefab, Parent);
                entryUI.SetData(item);
                UIEntryList.Add(entryUI);
                entryUI.OnClicked += OnQuestgiverEntryClicked;
            }
        }

        if (OnRefreshed != null)
            OnRefreshed.Invoke();
    }

    //public void Setup(List<QuestgiverMeta> _entries)
    //{
    // //   Data = _entries;
    //    Refresh();
    //    if (OnRefreshed != null)
    //        OnRefreshed.Invoke();
    //}

    public void OnQuestgiverEntryClicked(UIQuestgiverEntry _entry)
    {
        if (OnEntryClicked != null)
            OnEntryClicked.Invoke(_entry);
    }






}
