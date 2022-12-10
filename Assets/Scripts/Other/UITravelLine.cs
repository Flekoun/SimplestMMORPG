using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class UITravelLine : MonoBehaviour
{
    public TextMeshProUGUI TimeWeightText;

    public void Setup(int _timeWeight)
    {

        TimeWeightText.SetText(_timeWeight.ToString() );
        //if (_timeWeight > 1)
        //    TimeWeightText.SetText(_timeWeight.ToString() + " hours");
        //else
        //    TimeWeightText.SetText(_timeWeight.ToString() + " hour");
    }
}
