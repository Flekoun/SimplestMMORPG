using System.Collections;
using System.Collections.Generic;
using simplestmmorpg.data;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIBuff : MonoBehaviour
{
    public ImageIdDefinitionSOSet ImageIdDefinitionSOSet;
    public Image Portrait;
    public TextMeshProUGUI TurnLeftText;
    public CombatBuff Data;


    public void Setup(CombatBuff _data)
    {
        Data = _data;
        Portrait.sprite = ImageIdDefinitionSOSet.GetDefinitionById(Utils.GetMetadataForSkill(Data.buffId).imageId).Image;
        TurnLeftText.SetText(Data.durationTurns.ToString());
    }
}
