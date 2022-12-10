using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class UIPromptWindow : MonoBehaviour
{
    public TextMeshProUGUI DescriptionText;
    public TextMeshProUGUI TitleText;
    public GameObject DeclineButtonGO;
    public TextMeshProUGUI DeclineButtonText;
    public TextMeshProUGUI AcceptButtonText;
    public GameObject TitleGO;


    private UnityAction OnAcceptClicked;
    private UnityAction OnDeclineClicked;
    // Start is called before the first frame update
    public void Setup(string _description, UnityAction _onAcceptClicked, UnityAction _onDeclineClicked)
    {
        DescriptionText.SetText(_description);
        OnAcceptClicked = _onAcceptClicked;
        OnDeclineClicked = _onDeclineClicked;
    }

    public void Setup(string _description, string _title, UnityAction _onAcceptClicked, UnityAction _onDeclineClicked)
    {
        TitleGO.SetActive(true);
        TitleText.SetText(_title);
        Setup(_description, _onAcceptClicked, _onDeclineClicked);
    }

    public void HideDeclineButton()
    {
        DeclineButtonGO.gameObject.SetActive(false);
    }

    public void SetDeclineButtonText(string _text)
    {
        DeclineButtonText.SetText(_text);
    }

    public void SetAcceptButtonText(string _text)
    {
        AcceptButtonText.SetText(_text);
    }


    // Update is called once per frame
    public void Decline()
    {
        if (OnDeclineClicked != null)
            OnDeclineClicked.Invoke();

        Destroy(this.gameObject);
    }

    // Update is called once per frame
    public void Accept()
    {
        if (OnAcceptClicked != null)
            OnAcceptClicked.Invoke();


        Destroy(this.gameObject);
    }

}
