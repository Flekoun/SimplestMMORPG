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
    private List<MessageData> MessageQueue = new List<MessageData>();


    public void ShowMesssage(string _message, float _displayDuration = 1f)
    {

        MessageQueue.Add(new MessageData(_message, _displayDuration));
        ShowNextMessage();
    }

    private void ShowNextMessage()
    {
        if (IsMessageAnimationInProgress)
            return;

        if (MessageQueue.Count > 0)
        {
            BodyText.SetText(MessageQueue[0].Message);
            IsMessageAnimationInProgress = true;
            TweenEffects.ImportantMessageShowTween(Model.transform, () => { ShowAnimOver(); }, MessageQueue[0].Duration);
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


    private struct MessageData
    {
        public string Message;
        public float Duration;

        public MessageData(string _message, float _duration)
        {
            Message = _message;
            Duration = _duration;
        }

    }
}
