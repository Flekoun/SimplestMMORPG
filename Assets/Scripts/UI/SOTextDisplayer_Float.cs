using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using RoboRyanTron.Unite2017.Events;
using RoboRyanTron.Unite2017.Variables;

public class SOTextDisplayer_Float : MonoBehaviour
{
    public string Prefix;
    public string Suffix;

    public bool RoundToInt = false;
    public TextMeshProUGUI Text;
    public FloatVariable VariableToDisplay;

    // Start is called before the first frame update
    void OnEnable()
    {
        RefreshText();
        VariableToDisplay.ListenOnChangeEvent(RefreshText);

    }

    private void OnDisable()
    {
        VariableToDisplay.UnlistenOnChangeEvent(RefreshText);
    }

    private void RefreshText()
    {
        if (RoundToInt)
            Text.SetText(Prefix + Utils.RoundToInt(VariableToDisplay.Value).ToString() + Suffix);
        else
            Text.SetText(Prefix + VariableToDisplay.Value.ToString() + Suffix);
    }
    // Update is called once per frame
    void Update()
    {

    }

    public void Reset()
    {
        Text = this.GetComponent<TextMeshProUGUI>();
    }
}
