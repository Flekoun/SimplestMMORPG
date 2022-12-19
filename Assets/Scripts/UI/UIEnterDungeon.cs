using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIEnterDungeon : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    public FirebaseCloudFunctionSO FirebaseCloudFunctionSO;

    public TextMeshProUGUI InfoText;
    public Button EnterDungeonButton;


    public GameObject Model;

    public void Awake()
    {
        AccountDataSO.OnLocationDataChanged += Refresh;
        AccountDataSO.OnPartyDataChanged += Refresh;
     //   AccountDataSO.OnPlayerDataLoadedFirstTime += Refresh;
    }

    public void Refresh()
    {
        Show();

        if (AccountDataSO.LocationData.locationType == Utils.LOCATION_TYPE.DUNGEON)
        {

            if (!AccountDataSO.IsInParty())
            {
                InfoText.SetText("You need to be in party to enter dungeon");
                EnterDungeonButton.interactable = false;
            }
            else
            {

                if (AccountDataSO.PartyData.dungeonProgress == null)
                {
                    if (AccountDataSO.PartyData.partyLeaderUid == AccountDataSO.CharacterData.uid)
                    {
                        if (AccountDataSO.PartyData.AreAllPartyMembersOnSameLocation(AccountDataSO.CharacterData.position.locationId))
                        {
                            InfoText.SetText("You can enter the dungeon!");
                            EnterDungeonButton.interactable = true;
                        }
                        else
                        {
                            InfoText.SetText("All party members must be on dungeon location before you can enter!");
                            EnterDungeonButton.interactable = false;
                        }
                    }
                    else
                    {
                        InfoText.SetText("Only party leader can order to enter the dungeon!");
                        EnterDungeonButton.interactable = false;
                    }
                }
                else
                {
             ///       Debug.Log("HIDUJU!!!!!!!!!!!!!!!");
                    Hide();
                }
            }

        }
        else
            Hide();
    }

    public void Show()
    {
        Model.gameObject.SetActive(true);
    }

    public void Hide()
    {
        Model.gameObject.SetActive(false);
    }

 
    public void EnterDungeon()
    {
        FirebaseCloudFunctionSO.EnterDungeon(AccountDataSO.CharacterData.position.locationId);
    }
}
