using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using simplestmmorpg.realtimeDatabaseData;
using UnityEngine.Events;

public class UIChatEntry : MonoBehaviour
{
    public TextMeshProUGUI BodyText;
    public RealtimeDatabaseChatMessageData Data;
    public UnityAction<UIChatEntry> OnClicked;
    public void Setup(RealtimeDatabaseChatMessageData _data)
    {
        Data = _data;
        //BodyText.SetText("["+Data.characterName +" "+Data.characterLevel+"] " + _data.body);
        BodyText.SetText("[" + Data.characterName +"] " + _data.body);
        if (Data.channelType == CHANNEL_TYPE.PARTY)
            BodyText.color = new Color(0.6f,0.85f,1f);
    }

    public void Clicked()
    {
        if (OnClicked != null)
            OnClicked.Invoke(this);
    }
}
