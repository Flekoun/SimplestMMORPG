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
    public ListenOnQuestgivers ListenOnQuestgivers;
    public UnityAction<UIQuestgiverEntry> OnEntryClicked;
    public UnityAction OnRefreshed;

    public List<QuestgiverMeta> Data;
    //  public GameObject Model;

    public void Awake()
    {
        ListenOnQuestgivers.StartListeningOnQuestgiversAtCharacterPosition();
        ListenOnQuestgivers.OnQuestgiversAtCharacterWorldPositionChanged += Setup;
        AccountDataSO.OnCharacterDataChanged += Refresh;

    }

    public void Refresh()
    {
        Debug.Log("REFRESHING QUESTS IN SPAWNER");
        Utils.DestroyAllChildren(Parent, 1);

        //TADY SE FILTRUJOU QUEST GIVERI...DOBRE MISTO?!!

        //vyfiltruju questgivery ktere jsem uz splnil pryc.....
        var tempListToBeFiltered = new List<QuestgiverMeta>(Data);

        for (int i = tempListToBeFiltered.Count - 1; i >= 0; i--)
        {
            if (AccountDataSO.CharacterData.questgiversClaimed.Contains(Data[i].uid))
                tempListToBeFiltered.RemoveAt(i);
        }


        //vyfiltruju questgivery podle prereQestu.....
        for (int i = tempListToBeFiltered.Count - 1; i >= 0; i--)
        {
            int prereqsFound = 0;

            foreach (var item in tempListToBeFiltered[i].prereqQuests)
            {
                if(AccountDataSO.CharacterData.questgiversClaimed.Contains(item))
                prereqsFound++;
            }

            if (prereqsFound < tempListToBeFiltered[i].prereqQuests.Count)
            {
                Debug.Log("Fillteriung out : " + tempListToBeFiltered[i].uid + " prereqsQuestsFound:  " + prereqsFound + " needed : " + tempListToBeFiltered[i].prereqQuests.Count);
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
                Debug.Log("Fillteriung out : " + tempListToBeFiltered[i].uid + " prereqsPoIFound:  " + prereqsFound + " needed : " + tempListToBeFiltered[i].prereqExploredPointsOfInterest.Count);
                tempListToBeFiltered.RemoveAt(i);
            }
        }

        //vyfiltruju questgivery podle levelu.....
        foreach (var item in tempListToBeFiltered)
        {
            if (item.minLevel <= AccountDataSO.CharacterData.stats.level)
            {
                var entryUI = PrefabFactory.CreateGameObject<UIQuestgiverEntry>(UIEntryPrefab, Parent);
                entryUI.SetData(item);
                entryUI.OnClicked += OnQuestgiverEntryClicked;
            }
        }

        
    }


    public void OnDestroy()
    {
        ListenOnQuestgivers.StopListeningOnQuestgivcersAtCharacterWorldPosition();
    }

    public void Setup(List<QuestgiverMeta> _entries)
    {
        Data = _entries;
        Refresh();
        if (OnRefreshed != null)
            OnRefreshed.Invoke();
    }

    public void OnQuestgiverEntryClicked(UIQuestgiverEntry _entry)
    {
        if (OnEntryClicked != null)
            OnEntryClicked.Invoke(_entry);
    }






}
