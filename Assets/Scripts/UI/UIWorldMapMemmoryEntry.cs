using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class UIWorldMapMemmoryEntry : MonoBehaviour
{

    public Image Portrait;
    public ImageIdDefinitionSOSet AllImageIdDefinitionSOSet;


    public void Setup(string _POISpecialId)
    {
        Portrait.sprite = AllImageIdDefinitionSOSet.GetDefinitionById(_POISpecialId, "POI_SPECIAL").Image;
    }

}
