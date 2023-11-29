using System.Collections;
using System.Collections.Generic;
using simplestmmorpg.data;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class UIBuff : MonoBehaviour
{
    public ImageIdDefinitionSOSet ImageIdDefinitionSOSet;
    public Image Portrait;
    public TextMeshProUGUI TurnLeftText;
    public CombatBuff Data;
    public TooltipSpawner TooltipSpawner;
    public UnityAction<UIBuff> OnClicked;

    public void Setup(CombatBuff _data)
    {
        Data = _data;
        Portrait.sprite = ImageIdDefinitionSOSet.GetDefinitionById(Utils.DescriptionsMetadata.GetSkillMetadata(Data.buffId).imageId).Image;
        TurnLeftText.SetText(Data.turnsLeft.ToString());
        TooltipSpawner.SetCombatBuff(Data);
    }

    public void Clicked()
    {
        OnClicked?.Invoke(this);
    }
}
