using System.Collections;
using System.Collections.Generic;
using simplestmmorpg.data;
using UnityEngine;
using TMPro;


public class UIPartyMemberSpawner : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    public FirebaseCloudFunctionSO FirebaseCloudFunctionSO;
    public PrefabFactory PrefabFactory;
    public Transform Parent;
    public GameObject UIPartyMemberPrefab;
    public TextMeshProUGUI MembersCountText;

    public GameObject Model;
    //  public UIPartyFinderEntry SelectedPartyFinder;

    //private List<UIPartyMemberEntry> UIPartyMembersList = new List<UIPartyMemberEntry>();

    public void Awake()
    {
        AccountDataSO.OnPartyDataChanged += Refresh;
    }

    public void OnDestroy()
    {
        AccountDataSO.OnPartyDataChanged -= Refresh;
    }

    //public void OnEnable()
    //{
    //    Refresh();
    //}

    void Refresh()
    {


        Model.SetActive(AccountDataSO.PartyData != null);

        if (AccountDataSO.IsInParty())
        {
            Utils.DestroyAllChildren(Parent);


            foreach (var item in AccountDataSO.PartyData.partyMembers)
            {
                var member = PrefabFactory.CreateGameObject<UIPartyMemberEntry>(UIPartyMemberPrefab, Parent);
                member.SetData(item, AccountDataSO.PartyData.IsPartyLeader(item.uid));

            }



            MembersCountText.text = AccountDataSO.PartyData.partyMembers.Count + "/" + AccountDataSO.PartyData.partySizeMax;
        }

    }

    public void LeaveParty()
    {
        FirebaseCloudFunctionSO.LeaveParty();
    }


}
