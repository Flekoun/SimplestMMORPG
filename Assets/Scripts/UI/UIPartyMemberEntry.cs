using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using simplestmmorpg.data;


public class UIPartyMemberEntry : MonoBehaviour
{

    public GameObject PartyLeaderGO;
    public TextMeshProUGUI MemberNameText;
    public TextMeshProUGUI ClassText;
    public TextMeshProUGUI LevelText;
    public UIPortrait Portrait;
    public GameObject OfflineTagGO;
    public PartyMember Data;


    public void Awake()
    {

    }

    public void OnDestroy()
    {
    }

    public void SetData(PartyMember _data, bool _isLeader)
    {
        Data = _data;

        MemberNameText.SetText(Data.displayName);
        MemberNameText.color = Utils.GetClassColor(Data.characterClass);
        ClassText.SetText(Data.characterClass);
        LevelText.SetText("Level " + Data.level.ToString());

        Portrait.SetPortrait(Data.characterPortrait, Data.characterClass);

        OfflineTagGO.SetActive(!Data.isOnline);
        PartyLeaderGO.SetActive(_isLeader);
    }

}
