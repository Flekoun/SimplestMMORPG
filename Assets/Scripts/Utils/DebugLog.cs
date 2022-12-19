using UnityEngine;
using TMPro;

public class DebugLog : MonoBehaviour
{
    public string output = "";
    public string stack = "";
    public TMP_InputField DebugTextInput;
    public TextMeshProUGUI DebugText;
    public GameObject Model;
    public ContentFitterRefresh ContentFitterRefresh;

    private bool logStackTrace = false;

    public void Start()
    {
        logStackTrace = false;
    }

    public void LogStackTrace(bool _log)
    {
        logStackTrace = _log;
    }
    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        // Debug.Log("ee:");

        if (logStackTrace)
        {
            if (type == LogType.Exception)
            {
                output += logString + "\n" + "<color=\"red\">" + stackTrace + "</color>" + "\n";
            }
            else

                output += logString + "\n" + "<color=\"blue\">" + stackTrace + "</color>" + "\n";

        }
        else
        {
            if (type == LogType.Exception)
            {
                output += "<color=\"red\">" + logString + "</color>" + "\n";
            }
            else

                output += "<color=\"blue\">" + logString + "</color>" + "\n";
        }
        //  output += logString+"\n";
        //  stack +=  stackTrace + "\n";

    }

    public void PrintOutput()
    {
        if (DebugTextInput != null)
            DebugTextInput.text = output;

        if (DebugText != null)
            DebugText.text = output;

    }

    public void Show()
    {
        Model.SetActive(true);
        PrintOutput();
        ContentFitterRefresh.RefreshContentFitters();
    }

    public void Hide()
    {
        Model.SetActive(false);
    }
}

