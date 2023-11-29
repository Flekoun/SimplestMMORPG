using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using static UnityEngine.EventSystems.EventTrigger;
using UnityEngine.UI;
using System;

public class UIChapelPanel : MonoBehaviour
{
    public FirebaseCloudFunctionSO FirebaseCloudFunctionSO;
    public AccountDataSO AccountDataSO;
    public PrefabFactory PrefabFactory;

    public UIBless UIBlessChapel;
    public Button BlessButton;
    public TextMeshProUGUI BlessDescriptionText;

    public Button RemoveCursesButton;
    public UIPriceLabel RemoveCursesPrice;
    public TextMeshProUGUI RemoveCursesButtonDescriptionText;

    public Button GiftButton;
    public TextMeshProUGUI GiftDescriptionText;

    public GameObject Model;

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
        bool hasCurses = AccountDataSO.CharacterData.curses.Count > 0;
        bool hasAlreadyPrayedHere = AccountDataSO.CharacterData.IsChapelAtMyPositionAlreadyUsed();


        double result;
        if ((AccountDataSO.CharacterData.stats.level + 1) > 20)
            result = Math.Pow(Math.E, (30 * Math.Log(100000) / 20));
        else
            result = Math.Pow(Math.E, ((AccountDataSO.CharacterData.innHealhRestsCount + 1) * Math.Log(100000) / 20));

        int removeCursesPrice = (int)Math.Round(result);
        RemoveCursesPrice.SetPrice(removeCursesPrice);

        RemoveCursesButton.interactable = hasCurses && AccountDataSO.CharacterData.currency.gold >= removeCursesPrice && !hasAlreadyPrayedHere;

        if (hasAlreadyPrayedHere)
            RemoveCursesButtonDescriptionText.SetText("You have already made prayer here");
        else if (!hasCurses)
            RemoveCursesButtonDescriptionText.SetText("You dont have any curses");
        else
            RemoveCursesButtonDescriptionText.SetText("Removes up to 3 random curses");


        UIBlessChapel.Setup(AccountDataSO.CharacterData.GetChapelInfoOnMyPosition().blessId);
        BlessButton.interactable = !hasAlreadyPrayedHere;
        if (hasAlreadyPrayedHere)
        {
            BlessDescriptionText.SetText("You have already made prayer here");
            GiftDescriptionText.SetText("You have already made prayer here");
        }
        else
        {
            BlessDescriptionText.SetText("Get permanent blessing");
            GiftDescriptionText.SetText("Get rare or better loot ");
        }

        GiftButton.interactable = !hasAlreadyPrayedHere;


    }

    public void RemoveCursesClicked()
    {
        FirebaseCloudFunctionSO.ChapelRemoveCurses();

    }

    public void RecieveBlessClicked()
    {
        FirebaseCloudFunctionSO.ChapelRecieveBless();

    }


    public void GiftClicked()
    {
        FirebaseCloudFunctionSO.ChapelGift();

    }



}
