using System.Collections;
using System.Collections.Generic;
using RoboRyanTron.Unite2017.Variables;
using UnityEngine;
using UnityEngine.UI;

public class UIValueDisplayer : MonoBehaviour
{

    public Text Value_Text;
    public StringReference Prefix;
    public StringReference Suffix;
    public IntReference ValueToDisplay;
    // public FloatReference ValueToDisplay_Float;

    public void Start() //OnEnable()
    {
        Refresh();
    }
    public void Refresh()
    {
        Value_Text.text = Prefix + ValueToDisplay.Value.ToString() + Suffix;
    }

    public void DisplayValue(int _value)
    {

        ValueToDisplay.Value = _value;
        Refresh();
    }

    public void DisplayValue(float _value)
    {
        ValueToDisplay.Value = Utils.RoundToInt(_value);
        Refresh();
    }
}
