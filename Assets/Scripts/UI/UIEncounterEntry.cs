using System.Collections;
using System.Collections.Generic;
using simplestmmorpg.data;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class UIEncounterEntry : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    public FirebaseCloudFunctionSO FirebaseCloudFunctionSO;
    public PrefabFactory Factory;
    public TextMeshProUGUI FoundByText;
    public TextMeshProUGUI CapacityText;
    public TextMeshProUGUI ExpireDateText;
    public GameObject UICombatEnemyPrefab;
    public GameObject UICombatMemberPrefab;
    public Transform EnemyParent;
    public Transform CombatMembersParent;
    public GameObject DividerGO;
    public ContentFitterRefresh ContentFitterRefresh;

    public List<UICombatEntity> UICombatEnemyList = new List<UICombatEntity>();
    public List<UICombatEntity> UICombatCombatMemberList = new List<UICombatEntity>();
    public bool IgnoreClicksOnEntries = false;
    public EncounterData Data;

    public UnityAction<UICombatEntity> OnCombatEntityClicked;
    public UnityAction<UIEncounterEntry> OnClicked;



    public void SelectButtonClicked()
    {
        if (OnClicked != null)
            OnClicked.Invoke(this);
    }

    public UICombatEntity GetRandomEnemy()
    {
        if (UICombatEnemyList.Count > 0)
            return UICombatEnemyList[Random.Range(0, UICombatEnemyList.Count - 1)];
        else
            return null;
    }

    public void SelectRandomEnemy()
    {
        UICombatEntity entity = GetRandomEnemy();
        if (entity != null)
            OnEntryClicked(entity);
    }

    private void OnEntryClicked(UICombatEntity _entry)
    {
        if (!IgnoreClicksOnEntries)
        {
            foreach (var item in UICombatEnemyList)
                item.SetAsSelected(false);

            foreach (var item in UICombatCombatMemberList)
                item.SetAsSelected(false);

            _entry.SetAsSelected(true);

            if (OnCombatEntityClicked != null)
                OnCombatEntityClicked.Invoke(_entry);
        }
        else
        {
            //pokud ignoruju kliky na entry tak delam jako bych klikl na samotny entry
            SelectButtonClicked();
        }
    }

    private void OnForceRestClicked(UICombatMember _entry)
    {

        FirebaseCloudFunctionSO.ForceRestEncounter(Data.uid, _entry.Data.uid);
    }



    public void SetEncounter(EncounterData _encounterData)//, UIEncountersSpawner _parentSpawner)
    {

        Data = _encounterData;
        // Spawner = _parentSpawner;
        FoundByText.SetText("Found by " + Data.foundByName);
        //CapacityText.SetText("Party size " + Data.combatants.Length + "/" + Data.maxCombatants);

        if (Data.combatants.Length == 0)
            CapacityText.SetText("Unopposed");
        else
            CapacityText.SetText("In combat");

        //ExpireDateText.SetText(Data.GetTimeLeft() + " left");




        //vytvori nebo reusne encounter

        foreach (var item in UICombatEnemyList)
            item.gameObject.SetActive(false);

        for (int i = 0; i < Data.enemies.Length; i++)
        {
            if (UICombatEnemyList.Count >= i + 1)
            {
                UICombatEnemyList[i].SetData(Data.enemies[i], Data);
                UICombatEnemyList[i].OnClicked += OnEntryClicked;
                UICombatEnemyList[i].gameObject.SetActive(true);
            }
            else
            {

                UICombatEnemy combatantEntryUI = Factory.CreateGameObject<UICombatEnemy>(UICombatEnemyPrefab, EnemyParent);
                combatantEntryUI.SetData(Data.enemies[i], Data);
                combatantEntryUI.OnClicked += OnEntryClicked;

                UICombatEnemyList.Add(combatantEntryUI);
            }
        }




        //vytvori nebo reusne encounter

        CombatMembersParent.gameObject.SetActive(Data.combatantList.Length > 0);
        DividerGO.gameObject.SetActive(Data.combatantList.Length > 0);


        foreach (var item in UICombatCombatMemberList)
            item.gameObject.SetActive(false);


        for (int i = 0; i < Data.combatants.Length; i++)
        {
            if (UICombatCombatMemberList.Count >= i + 1)
            {
                UICombatCombatMemberList[i].SetData(Data.combatants[i], Data);
                UICombatCombatMemberList[i].OnClicked += OnEntryClicked;
                UICombatCombatMemberList[i].gameObject.SetActive(true);
            }
            else
            {
                Debug.Log("vytvarim noveho comabt membera");
                UICombatMember combatantEntryUI = Factory.CreateGameObject<UICombatMember>(UICombatMemberPrefab, CombatMembersParent);
                combatantEntryUI.SetData(Data.combatants[i], Data);
                combatantEntryUI.OnClicked += OnEntryClicked;
                combatantEntryUI.OnForceRestClicked += OnForceRestClicked;
                UICombatCombatMemberList.Add(combatantEntryUI);
            }
        }

        //TryToFixScrollReckGlitches();
        ContentFitterRefresh.RefreshContentFitters();
        InvokeRepeating("CheckForTurnTimerOver", 0, 1f);
    }

    private void CheckForTurnTimerOver()
    {
        foreach (UICombatEntity item in UICombatCombatMemberList)
        {
            //show end turn button on guys when - turn timer expired and only for players who did not rest,yet and it is not me and are not dead
            ((UICombatMember)item).ForceRestButton.SetActive(Data.HasTurnTimerExpired() && !((CombatMember)((UICombatMember)item).Data).hasRested && ((UICombatMember)item).Data.uid != AccountDataSO.CharacterData.uid && ((UICombatMember)item).Data.stats.health > 0);
        }

    }
    //private void TryToFixScrollReckGlitches()
    //{
    //    if (this.isActiveAndEnabled)
    //        StartCoroutine(Wait());

    //}

    //private IEnumerator Wait()
    //{
    //    yield return new WaitForSecondsRealtime(0.1f);
    //    LayoutRebuilder.ForceRebuildLayoutImmediate(TransforToFix);
    //    Canvas.ForceUpdateCanvases();
    //}
}
