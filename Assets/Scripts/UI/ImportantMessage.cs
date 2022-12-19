using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class ImportantMessage : MonoBehaviour
{
    public TweenEffects TweenEffects;
    public TextMeshProUGUI BodyText;
    public GameObject Model;

    private bool IsMessageAnimationInProgress = false;
    private List<string> MessageQueue = new List<string>();


    public void ShowMesssage(string _message)
    {
        MessageQueue.Add(_message);
        ShowNextMessage();
    }

    private void ShowNextMessage(float delay = 1f)
    {
        if (IsMessageAnimationInProgress)
            return;

        if (MessageQueue.Count > 0)
        {
            BodyText.SetText(MessageQueue[0]);
            IsMessageAnimationInProgress = true;
            TweenEffects.ImportantMessageShowTween(Model.transform, () => { ShowAnimOver(); }, delay);
            Model.gameObject.SetActive(true);

            MessageQueue.RemoveAt(0);
        }
    }


    private void ShowAnimOver()
    {
        IsMessageAnimationInProgress = false;
        Model.gameObject.SetActive(false);
        ShowNextMessage();
    }

    public void Clicked()
    {
        MessageQueue.Clear();
    }


}
