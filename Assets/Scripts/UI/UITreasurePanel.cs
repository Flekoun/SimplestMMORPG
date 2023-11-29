using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using static UnityEngine.EventSystems.EventTrigger;
using UnityEngine.UI;
using System;

public class UITreasurePanel : MonoBehaviour
{
    public FirebaseCloudFunctionSO FirebaseCloudFunctionSO;
    public AccountDataSO AccountDataSO;
    public PrefabFactory PrefabFactory;
    public Button FreeOpenButton;
    public Button CurseOpenButton;
    public Button MagicKeyOpenButton;

    public TextMeshProUGUI FreeOpenButtonDescriptionText;
    public TextMeshProUGUI CurseOpenButtonDescriptionText;
    public TextMeshProUGUI MagicKeyOpenDescriptionText;
    public GameObject Model;

    private UIPortrait lastlyClickedEntry;

    public void Show()
    {
        AccountDataSO.OnCharacterDataChanged += Refresh;

        Refresh();

        Model.gameObject.SetActive(true);
    }


    public void Hide()
    {
        AccountDataSO.OnCharacterDataChanged -= Refresh;
        Model.gameObject.SetActive(false);
    }

    private void Refresh()
    {
        bool isTreasureClaimed = AccountDataSO.CharacterData.IsTreasureClaimedOnMyPosition();


        FreeOpenButton.interactable = !isTreasureClaimed;
        CurseOpenButton.interactable = !isTreasureClaimed;
        MagicKeyOpenButton.interactable = !isTreasureClaimed;


        if (isTreasureClaimed)
        {
            FreeOpenButtonDescriptionText.SetText("Treasure already claimed");
            CurseOpenButtonDescriptionText.SetText("Treasure already claimed");
            MagicKeyOpenDescriptionText.SetText("Treasure already claimed");
        }
        else
        {
            FreeOpenButtonDescriptionText.SetText("Good reward");
            CurseOpenButtonDescriptionText.SetText("Great reward");
            MagicKeyOpenDescriptionText.SetText("Best reward");
        }



    }

    public void FreeOpenClicked()
    {
        FirebaseCloudFunctionSO.TreasureOpenFree();

    }

    public void CurseOpenClicked()
    {
        FirebaseCloudFunctionSO.TreasureOpenForCurse();
    }

    public void MagicKeyClicked()
    {
        FirebaseCloudFunctionSO.TreasureOpenWithKey();
    }


}
