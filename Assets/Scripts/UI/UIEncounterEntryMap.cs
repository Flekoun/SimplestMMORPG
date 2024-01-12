using System.Collections;
using System.Collections.Generic;
using simplestmmorpg.data;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using DG.Tweening;

public class UIEncounterEntryMap : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    public PrefabFactory Factory;
    public GameObject UICombatEnemyPrefab;
    public GameObject UICombatMemberPrefab;
    public Transform EnemyParent;
    public GameObject EnemyCountGO;
    public TextMeshProUGUI EnemyCountText;
    public UIPortrait EnemyMainPortait;
    public Transform CombatMembersParent;
    public GameObject RareEnemyGO;
    public DOTweenAnimation HighlightAnimation;
    public UnityAction<UIEncounterEntryMap> OnClicked;
    // public List<UICombatEntity> UICombatEnemyList = new List<UICombatEntity>();
    // public List<UICombatEntity> UICombatCombatMemberList = new List<UICombatEntity>();
    public EncounterData Data;


    public void SetEncounter(EncounterData _encounterData, bool _isHighlighted)//, UIEncountersSpawner _parentSpawner)
    {
        EnemyMainPortait.OnClicked += Clicked;
        EnemyMainPortait.EnableAsButton();

        Data = _encounterData;

        EnemyCountGO.SetActive(Data.enemies.Count > 1);

        EnemyCountText.SetText(Data.enemies.Count.ToString());

        RareEnemyGO.SetActive(false);

        if (!_isHighlighted)
        {
            HighlightAnimation.DOKill();
        }

        int maxLeveEnemy = 0;
        int maxEstimatedStrength = 0;
        foreach (var item in Data.enemies)
        {
            if (item.isRare)
                RareEnemyGO.SetActive(true);
            //var combatantEntryUI = Factory.CreateGameObject<UIPortrait>(UICombatEnemyPrefab, EnemyParent);
            //combatantEntryUI.SetPortrait(item.GetPortraitId());

            if (maxLeveEnemy < item.level)
            {
                EnemyMainPortait.SetPortrait(item.GetPortraitId(), item.GetCharacterClassId());
                maxEstimatedStrength = item.stats.healthMax;// + item.damageAmountMax;
                maxLeveEnemy = item.level;
            }
            else if (maxLeveEnemy == item.level)
            {
                int enemyEstimatedStrength = item.stats.healthMax;// + item.damageAmountMax;
                                                                  //  Debug.Log(enemyEstimatedStrength+ " vs " + maxEstimatedStrength);
                if (enemyEstimatedStrength > maxEstimatedStrength)
                {
                    EnemyMainPortait.SetPortrait(item.GetPortraitId(), item.GetCharacterClassId());
                    maxEstimatedStrength = enemyEstimatedStrength;
                }
            }
        }

        foreach (var item in Data.combatants)
        {
            var combatantEntryUI = Factory.CreateGameObject<UIPortrait>(UICombatMemberPrefab, CombatMembersParent);
            combatantEntryUI.SetPortrait(item.GetPortraitId(), item.GetCharacterClassId());
        }


    }

    public void Clicked(UIPortrait _portrait)
    {
        OnClicked?.Invoke(this);
    }


}
