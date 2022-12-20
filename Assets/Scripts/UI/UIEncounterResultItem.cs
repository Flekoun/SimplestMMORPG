using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using simplestmmorpg.data;

public class UIEncounterResultItem : MonoBehaviour
{
    public TextMeshProUGUI WhoWillHaveThisItem_Text;
    public TextMeshProUGUI WhoWantedThisItem_Text;
    public UIContentItem UIInventoryItem;
    public GameObject ClaimedGO;
    public GameObject NeedInfoGO;
    public EncounterResultContentLoot Data;
    // Start is called before the first frame update


    //public override string GetUid()
    //{
    //    return Data.content.GetContent().uid;
    //}


    public void SetData(EncounterResultContentLoot _resultLoot, EncounterResult _encounterResult)
    {
        Data = _resultLoot;

        UIInventoryItem.SetData(Data.content.GetContent());
        NeedInfoGO.SetActive(_encounterResult.combatantsList.Count > 1);

        WhoWantedThisItem_Text.text = "";
        foreach (var wanter in Data.charactersWhoWantThis)
        {
            //  WhoWantedThisItem_Text.SetText(WhoWantedThisItem_Text.text+ "<color=#" + ColorUtility.ToHtmlStringRGBA(Utils.GetClassColor(wanter.characterClass)) +">" +wanter.displayName + "</color>, ");
            WhoWantedThisItem_Text.SetText(WhoWantedThisItem_Text.text + Utils.ColorizeGivenTextWithClassColor(wanter.displayName, wanter.characterClass) + ", ");
        }

        if (WhoWantedThisItem_Text.text != "")
            WhoWantedThisItem_Text.SetText(WhoWantedThisItem_Text.text.Remove(WhoWantedThisItem_Text.text.Length - 2));

        if (!_resultLoot.DoesAnyoneWillHaveThisItem())//_resultLoot.characterWhoWillHaveThis == null )
        {
            ClaimedGO.SetActive(false);

        }
        else
        {
            WhoWillHaveThisItem_Text.SetText(Utils.ColorizeGivenTextWithClassColor(Data.characterWhoWillHaveThis.displayName, Data.characterWhoWillHaveThis.characterClass));

            ClaimedGO.SetActive(true);

            foreach (var item in _encounterResult.combatantsWithUnclaimedRewardsList)
            {
                if (_resultLoot.characterWhoWillHaveThis.uid == item)
                {
                    ClaimedGO.SetActive(false);
                    break;

                }
            }
        }

    }

}
