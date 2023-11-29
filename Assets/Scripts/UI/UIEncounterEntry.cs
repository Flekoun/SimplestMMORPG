using System.Collections;
using System.Collections.Generic;
using simplestmmorpg.data;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class UIEncounterEntry : UISelectableSpawner
{
    public AccountDataSO AccountDataSO;
    public FirebaseCloudFunctionSO FirebaseCloudFunctionSO;
    public PrefabFactory Factory;
    public TextMeshProUGUI FoundByText;
    public TextMeshProUGUI CapacityText;
    public TextMeshProUGUI TierText;
    public GameObject UICombatEnemyPrefab;
    public GameObject UICombatMemberPrefab;
    public GameObject BasicInfoGO;
    public Transform EnemyParent;
    public Transform CombatMembersParent;
    public GameObject DividerGO;
    public ContentFitterRefresh ContentFitterRefresh;

    public CombatFlowEffectSpawner CombatFlowEffectSpawner;

    public List<UICombatEntity> UICombatEnemyList = new List<UICombatEntity>();
    public List<UICombatEntity> UICombatCombatMemberList = new List<UICombatEntity>();

    public UILineMaker UILineMaker;


    public bool IgnoreClicksOnEntries = false;
    public EncounterData Data;

    public UnityAction<UICombatEntity> OnCombatEntityClicked;
    public UnityAction<UIEncounterEntry> OnClicked;
    public UnityAction<UIBuff, UICombatEntity> OnCombatEntityBuffClicked;

    //abych vedel kdy se zmenil kompletne novy ecnounter a kdy jsou to jen refreshe stavajiciho
    private string lastEncounterUid = "";

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


    //public void ClickOnRandomEnemy()
    //{

    //    UICombatEntity entity = GetRandomEnemy();
    //    if (entity != null)
    //        OnEntryClicked(entity);
    //}

    public void ShowBasicInfoPanel(bool _show)
    {
        BasicInfoGO.SetActive(_show);
    }
    //private void OnEntryClicked(UICombatEntity _entry)
    //{
    //    if (!IgnoreClicksOnEntries)
    //    {

    //        base.OnUISelectableItemClicked(_entry);

    //        //if (_entry.Data is CombatEnemy)
    //        //{
    //        //    var targetUid = Data.GetCombatMemeberByUid((_entry.Data as CombatEnemy).targetUid);
    //        //    if (targetUid != null)
    //        //    {
    //        //        var target = GetUICombatEntityByUid(targetUid.uid);
    //        //        if (target != null)
    //        //        {
    //        //            UILineMaker.MakeLine(_entry.gameObject.transform.position.x, _entry.gameObject.transform.position.y, target.gameObject.transform.position.x, target.gameObject.transform.position.y, Color.red);
    //        //        }
    //        //    }
    //        //}

    //        if (OnCombatEntityClicked != null)
    //            OnCombatEntityClicked.Invoke(_entry);
    //    }
    //    else
    //    {
    //        //pokud ignoruju kliky na entry tak delam jako bych klikl na samotny entry
    //        SelectButtonClicked();
    //    }
    //}

    private void OnForceRestClicked(UICombatMember _entry)
    {

        FirebaseCloudFunctionSO.ForceRestEncounter(Data.uid, _entry.Data.uid);
    }




    public void SetEncounter(EncounterData _encounterData, bool _initSetup)//, UIEncountersSpawner _parentSpawner)
    {
        Data = _encounterData;

        if (lastEncounterUid != Data.uid) //novy encounter to je ne jen refreshe stavajiciho co probiha
        {
            lastEncounterUid = Data.uid;
            CombatFlowEffectSpawner.ResetCombatFlow(Data);
        }

        FoundByText.SetText("Found by " + Data.foundByName);

        if (Data.combatants.Count < Data.maxCombatants)
            CapacityText.color = Color.red;
        else
            CapacityText.color = Color.green;

        CapacityText.SetText(Data.combatants.Count + "/" + Data.maxCombatants);

        TierText.SetText((Data.tier + 1).ToString() + ". floor");


        //ExpireDateText.SetText(Data.GetTimeLeft() + " left");
        //vytvori nebo reusne encounter

        foreach (var item in UICombatEnemyList)
            item.gameObject.SetActive(false);

        for (int i = 0; i < Data.enemies.Count; i++)
        {
            if (UICombatEnemyList.Count >= i + 1)
            {
                UICombatEnemyList[i].gameObject.SetActive(true);
                UICombatEnemyList[i].SetData(Data.enemies[i], Data, _initSetup);
                //    UICombatEnemyList[i].OnClicked += OnEntryClicked; //nema to tu co delat?uz mas listenery natavene?



            }
            else
            {

                UICombatEnemy combatantEntryUI = Factory.CreateGameObject<UICombatEnemy>(UICombatEnemyPrefab, EnemyParent);
                combatantEntryUI.SetData(Data.enemies[i], Data, _initSetup);
                //  combatantEntryUI.OnClicked += OnEntryClicked;
                //  combatantEntryUI.OnBuffClicked += BuffClicked;
                UICombatEnemyList.Add(combatantEntryUI);
            }
        }

        //vytvori nebo reusne encounter

        CombatMembersParent.gameObject.SetActive(Data.combatantList.Count > 0);
        //  DividerGO.gameObject.SetActive(Data.combatantList.Length > 0);

        foreach (var item in UICombatCombatMemberList)
            item.gameObject.SetActive(false);


        for (int i = 0; i < Data.combatants.Count; i++)
        {
            if (UICombatCombatMemberList.Count >= i + 1)
            {
                UICombatCombatMemberList[i].gameObject.SetActive(true);
                UICombatCombatMemberList[i].SetData(Data.combatants[i], Data, _initSetup);
                //   UICombatCombatMemberList[i].OnClicked += OnEntryClicked;//nema to tu co delat?uz mas listenery natavene?

            }
            else
            {
                Debug.Log("vytvarim noveho comabt membera");
                UICombatMember combatantEntryUI = Factory.CreateGameObject<UICombatMember>(UICombatMemberPrefab, CombatMembersParent);
                combatantEntryUI.SetData(Data.combatants[i], Data, _initSetup);
                //    combatantEntryUI.OnClicked += OnEntryClicked;
                combatantEntryUI.OnForceRestClicked += OnForceRestClicked;
                // combatantEntryUI.OnBuffClicked += BuffClicked;

                UICombatCombatMemberList.Add(combatantEntryUI);

            }
        }

        //TryToFixScrollReckGlitches();
        ContentFitterRefresh.RefreshContentFitters();
        InvokeRepeating("CheckForTurnTimerOver", 0, 1f);

        CombatFlowEffectSpawner.SpawnEffect(this);
    }

    private void CheckForTurnTimerOver()
    {
        foreach (UICombatEntity item in UICombatCombatMemberList)
        {
            //show end turn button on guys when - turn timer expired and only for players who did not rest,yet and it is not me and are not dead
            ((UICombatMember)item).ForceRestButton.SetActive(Data.HasTurnTimerExpired() && !((CombatMember)((UICombatMember)item).Data).hasRested && ((UICombatMember)item).Data.uid != AccountDataSO.CharacterData.uid && ((UICombatMember)item).Data.stats.health > 0);
        }


    }

    //private void BuffClicked(UIBuff _buff, UICombatEntity _entity)
    //{
    //    OnCombatEntityBuffClicked.Invoke(_buff, _entity);
    //}

    public void MakeLine(float ax, float ay, float bx, float by, Color col)
    {
        UILineMaker.MakeLine(ax, ay, bx, by, col);
    }

    public UICombatEntity GetUICombatEntityByUid(string _uid)
    {
        foreach (var item in UICombatEnemyList)
        {
            if (item.Data.uid == _uid)
                return item;
        }

        foreach (var item in UICombatCombatMemberList)
        {
            if (item.Data.uid == _uid)
                return item;
        }

        return null;
    }


}
