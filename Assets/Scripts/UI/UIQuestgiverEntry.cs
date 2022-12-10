using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using TMPro;

using simplestmmorpg.data;
using UnityEngine.Events;

public class UIQuestgiverEntry : MonoBehaviour
{
    public FirebaseCloudFunctionSO FirebaseCloudFunctionSO;
    public AccountDataSO AccountDataSO;
    public TextMeshProUGUI DisplayNameText;
    public TextMeshProUGUI EnemiesToKillText;
    public GameObject QuestCompletedGO;

    public UnityAction<UIQuestgiverEntry> OnClicked;
    public QuestgiverMeta Data;

    public void Awake()
    {
        AccountDataSO.OnCharacterDataChanged += Refresh;
    }

    public void Refresh()
    {
        if (this == null)   //tohle je tu potreba protoze kdyz claimnu quest tak se smaze z listu sice, ale tenhle refresh se po smazani z listu zavola bo Charakter data se taky zmeni ale tohle je uz null....tak to chapu ja. A chci poslouchat na tom character data changed protoze kdyz hrac zabiji potrvory tak aby toupdatovalo quest
            return;

        string killsNeeded_Title = "Please hero, help. I need you to kill these monsters for me! \n";
        string killsNeeded = "";
        foreach (var item in Data.killsRequired)
        {
            killsNeeded = killsNeeded + item.id + " : " + item.count + "(you have " + AccountDataSO.CharacterData.GetKillsForEnemyId(item.id) + " )" + "\n";
        }

        EnemiesToKillText.SetText(killsNeeded_Title + killsNeeded);
        DisplayNameText.SetText(Data.displayName);

       QuestCompletedGO.SetActive(AccountDataSO.CharacterData.IsQuestCompleted(Data));
    }

    public void SetData(QuestgiverMeta _data)
    {
        Data = _data;
        Refresh();


    }
    //// Start is called before the first frame update
    //public void ClaimQuestgiverReward()
    //{
    //    FirebaseCloudFunctionSO.ClaimQuestgiverReward(Data.uid);
    //}

    // Update is called once per frame
    public void Clicked()
    {
        if (OnClicked != null)
            OnClicked.Invoke(this);
    }
}
