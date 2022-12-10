using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using simplestmmorpg.data;
using UnityEngine.Events;

public class UIInboxItemEntry : MonoBehaviour
{

    public UIContentItem UIContentItem;
    public TextMeshProUGUI TitleText;

    public UnityAction<UIInboxItemEntry> OnClicked;
    public InboxItem Data;
    // Start is called before the first frame update

    public void SetData(InboxItem _data)
    {
        Data = _data;
        UIContentItem.SetData(Data.content.GetContent());
        TitleText.SetText(Data.messageTitle);

    }

    public void Clicked()
    {
        if(OnClicked!=null)
        {
            OnClicked.Invoke(this);
        }
    }
}
