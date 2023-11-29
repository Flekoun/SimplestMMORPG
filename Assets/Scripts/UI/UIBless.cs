using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBless : MonoBehaviour
{
    public ImageIdDefinitionSOSet AllImageIdDefinitionSOSet;
    public Image Portrait;
    public TooltipSpawner TooltipSpawner;

    public void Setup(string _blessId)
    {
        Portrait.sprite = AllImageIdDefinitionSOSet.GetDefinitionById(_blessId).Image;
        TooltipSpawner.SetString(_blessId);
    }


}
