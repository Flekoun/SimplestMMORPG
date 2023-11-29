using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class UIPartyInvite : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    public FirebaseCloudFunctionSO FirebaseCloudFunctionSO;

    public GameObject Model;
    public TextMeshProUGUI DescriptionText;
    // Start is called before the first frame update
    void Awake()
    {
        AccountDataSO.OnPartyInviteDataChanged += Refresh;
    }

    // Update is called once per frame
    void Refresh()
    {
        if (AccountDataSO.PartyInviteData != null)
        {
            Model.SetActive(true);

            DescriptionText.SetText(AccountDataSO.PartyInviteData.partyLeaderDisplayName + " has invited you to his party!");
        }
        else
            Model.SetActive(false);
    }

    public async void Accept()
    {
        var result = await FirebaseCloudFunctionSO.AcceptPartyInvite(AccountDataSO.PartyInviteData.partyLeaderUid);
        if (result.Result)
        {
            UIManager.instance.ImportantMessage.ShowMesssage("Welcome to Party!");
        }
        Model.SetActive(false);
    }

    public async void Decline()
    {
        var result = await FirebaseCloudFunctionSO.DeclinePartyInvite(AccountDataSO.PartyInviteData.partyLeaderUid);
        if (result.Result)
        {
            UIManager.instance.ImportantMessage.ShowMesssage("Invitation declined!");
        }
        Model.SetActive(false);
    }

    //public void SendTestPartyInvite()
    //{
    //    FirebaseCloudFunctionSO.SendPartyInvite(AccountDataSO.CharacterData.uid);
    //}
}
