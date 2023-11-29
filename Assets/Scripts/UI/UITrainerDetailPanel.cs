using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using simplestmmorpg.data;
using System;

public class UITrainerDetailPanel : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    public FirebaseCloudFunctionSO FirebaseCloudFunctionSO;
    public TextMeshProUGUI DescriptionText;
    public TextMeshProUGUI ProfessionDescription;
    public TextMeshProUGUI TitleText;
    public UIPriceLabel UIPriceLabel;
    public GameObject Model;
    public Button TrainButton;
    public Trainer Data;

    //  private List<string> equiToSellUids = new List<string>();
    // private List<string> equiToBuyUids = new List<string>();

    public void Awake()
    {

    }


    public void Show(Trainer _data)
    {
        AccountDataSO.OnCharacterDataChanged += Refresh;
        Data = _data;
        Model.SetActive(true);
        Refresh();
    }

    public void Hide()
    {
        Model.SetActive(false);

        AccountDataSO.OnCharacterDataChanged -= Refresh;

    }


    public void Refresh()
    {
        TitleText.SetText(Utils.DescriptionsMetadata.GetTrainersMetadata(Data.id).title.GetText());
        DescriptionText.SetText(String.Format(Utils.ReplacePlaceholdersInTextWithDescriptionFromMetadata(Utils.DescriptionsMetadata.GetTrainersMetadata(Data.id).description.GetText()), "<b>" + AccountDataSO.CharacterData.characterName + "</b>"));

        UIPriceLabel.SetPrice(Data.trainPrice);

        if (!AccountDataSO.CharacterData.HasAlreadyThisOrMoreOfProfessionSkillToBeTrained(Data.professionMaxTrainAmount, Data.professionHeTrains))
        {
            TrainButton.gameObject.SetActive(true);
            //   ProfessionDescription.gameObject.SetActive(true);
            TrainButton.interactable = true;
            ProfessionDescription.SetText(Utils.ReplacePlaceholdersInTextWithDescriptionFromMetadata("I can expand your limits of knowledge on <color=\"yellow\">" + Data.professionHeTrains + "</color> to <color=\"yellow\">" + Data.professionMaxTrainAmount + "</color>.\n<color=\"red\">You can learn only single profession! Choose wisely!</color>"));

            if (AccountDataSO.CharacterData.currency.gold < Data.trainPrice)
                TrainButton.interactable = false;
            if (!AccountDataSO.CharacterData.HasEnoughProfessionSkillToBeTrained(Data.professionMinAmountNeededToTrain, Data.professionHeTrains))
                TrainButton.interactable = false;
        }
        else
        {
            //   ProfessionDescription.gameObject.SetActive(false);
            TrainButton.gameObject.SetActive(false);
            ProfessionDescription.SetText("<color=\"yellow\">I cant teach you nothing new, you already know everything I do</color>");
        }
    }




    public void OnTrainButtonClicked()
    {
        FirebaseCloudFunctionSO.TrainAtTrainer(Data.id);
    }




}
