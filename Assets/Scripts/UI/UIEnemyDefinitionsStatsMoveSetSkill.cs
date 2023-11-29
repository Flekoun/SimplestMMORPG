using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using simplestmmorpg.data;
using UnityEngine.UI;

public class UIEnemyDefinitionsStatsMoveSetSkill : MonoBehaviour
{
    public ImageIdDefinitionSOSet AllImageIdDefinitionSOSet;
    public TextMeshProUGUI NameText;
    public TextMeshProUGUI AmountText;
    public Image SkillTypeImage;
    public TooltipSpawner TooltipSpawner;
    private EnemyDefinitionsStatsMoveSetSkill Data;

    // Start is called before the first frame update
    public void SetData(EnemyDefinitionsStatsMoveSetSkill _data, CombatEntity _entity)
    {
        Data = _data;

        NameText.SetText(Data.id);
        TooltipSpawner.stringId = _data.typeId;
        SkillTypeImage.sprite = AllImageIdDefinitionSOSet.GetDefinitionById(_data.typeId).Image;

        if (Data.typeId == Utils.MONSTER_SKILL_TYPES.SKILL_TYPE_ATTACK_NORMAL || Data.typeId == Utils.MONSTER_SKILL_TYPES.SKILL_TYPE_ATTACK_AND_DEBUFF)
        {
            AmountText.SetText((Data.amounts[0] + _entity.stats.damagePowerTotal).ToString());
            AmountText.gameObject.SetActive(true);
        }
        else
            AmountText.gameObject.SetActive(false);
    }

}
