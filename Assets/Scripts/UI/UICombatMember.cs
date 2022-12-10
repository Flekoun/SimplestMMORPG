using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.PerformanceData;
using simplestmmorpg.data;

using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class UICombatMember : UICombatEntity
{

    public TextMeshProUGUI RestsCountText;
    public UIProgressBar ManaProgress;
    public GameObject ForceRestButton;
    public UnityAction<UICombatMember> OnForceRestClicked;

    public override void SetData(CombatEntity _data, EncounterData _encounter)
    {
        base.SetData(_data, _encounter);

        NameText.color = Utils.GetClassColor((Data as CombatMember).characterClass);

        ManaProgress.SetValues(Data.stats.manaMax, Data.stats.mana);

        if ((Data as CombatMember).hasRested)
        {
            RestsCountText.SetText("<color=\"green\">rest taken</color>");
        }
        else
            RestsCountText.SetText("<color=\"black\">fighting..</color>");


        if (OldData is CombatMember && Data is CombatMember)
        {
            if (!( (OldData as CombatMember).hasRested  ) && (Data as CombatMember).hasRested)
            {
                FloatingTextSpawner.Spawn("Resting...", Color.yellow, FloatingTextsParent);
            }
        }

    }


    public void ForceRestClicked()
    {
        if (OnForceRestClicked != null)
            OnForceRestClicked.Invoke(this);
    }

}
