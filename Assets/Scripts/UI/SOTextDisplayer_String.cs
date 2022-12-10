using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using RoboRyanTron.Unite2017.Events;
using RoboRyanTron.Unite2017.Variables;

public class SOTextDisplayer_String : MonoBehaviour
{
    public TextMeshProUGUI Text;
    public StringVariable VariableToDisplay;

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
        Text.SetText(VariableToDisplay.Value);
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
