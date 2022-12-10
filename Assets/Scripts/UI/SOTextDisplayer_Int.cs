using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using RoboRyanTron.Unite2017.Events;
using RoboRyanTron.Unite2017.Variables;

public class SOTextDisplayer_Int : MonoBehaviour
{
    public string Prefix;
    public string Suffix;

    public TextMeshProUGUI Text;
    public IntVariable VariableToDisplay;

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
        Text.SetText(Prefix+VariableToDisplay.Value.ToString()+ Suffix);
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
