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


    public override void SetData(CombatEntity _data, EncounterData _encounter)
    {
        base.SetData(_data, _encounter);

        DamageText.SetText((Data as CombatEnemy).damageAmountMin.ToString()+"-"+ (Data as CombatEnemy).damageAmountMax.ToString());

        CombatMember target = _encounter.GetCombatMemeberByUid((Data as CombatEnemy).targetUid);
        // Debug.Log("target: " + target.displayName);
        if (target != null)
            TargetText.SetText(target.displayName);
        else
            TargetText.SetText("no target");
    }
}

