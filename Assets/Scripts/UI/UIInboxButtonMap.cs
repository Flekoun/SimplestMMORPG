using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class UIInboxButtonMap : MonoBehaviour
{

    public AccountDataSO AccountDataSO;
    public GameObject NewMailGO;
    public TextMeshProUGUI MailCountText;
    public bool IsPlayerInbox = false;
    public void Awake()
    {
        AccountDataSO.OnInboxDataCharacterChanged += Refresh;
        AccountDataSO.OnInboxDataPlayerChanged += Refresh;
    }

    public void OnEnable()
    {
        Refresh();
    }
    // Start is called before the first frame update
    void Refresh()
    {
        if (this == null)
            return;

        if (IsPlayerInbox)
        {
            NewMailGO.SetActive(AccountDataSO.InboxDataPlayer.Count > 0);
            MailCountText.SetText(AccountDataSO.InboxDataPlayer.Count.ToString());
        }
        else
        {
            NewMailGO.SetActive(AccountDataSO.InboxDataCharacter.Count > 0);
            MailCountText.SetText(AccountDataSO.InboxDataCharacter.Count.ToString());
        }
    }

}
