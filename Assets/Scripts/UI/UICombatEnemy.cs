using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;
using simplestmmorpg.data;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class UICombatEnemy : UICombatEntity
{

    public TextMeshProUGUI DamageText;
    public TextMeshProUGUI TargetText;
    public GameObject TargetGO;
    public GameObject RareEnemyGO;
    public UIEnemyDefinitionsStatsMoveSetSkill NextMove;


    public override void SetData(CombatEntity _data, EncounterData _encounter, bool _initSetup)
    {
        base.SetData(_data, _encounter, _initSetup);

        // DamageText.SetText((Data as CombatEnemy).damageAmountMin.ToString()+"-"+ (Data as CombatEnemy).damageAmountMax.ToString());

        if (_encounter != null)
        {
            CombatMember target = _encounter.GetCombatMemeberByUid((Data as CombatEnemy).targetUid);

            // Debug.Log("target: " + target.displayName);
            if (target != null)
            {
                TargetText.SetText(target.displayName);
            }
            else
                TargetText.SetText("no target");
        }

        TargetGO.SetActive(_encounter.GetLivingCombatantsCount() > 1);

        RareEnemyGO.SetActive((Data as CombatEnemy).isRare);
        NextMove.SetData((_data as CombatEnemy).nextSkill, _data);
    }
}

