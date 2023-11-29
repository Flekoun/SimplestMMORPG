using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using simplestmmorpg.data;
using UnityEngine.UI;
using UnityEngine.Events;
using System;

public class UICombatSkillVisuals : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    public ImageIdDefinitionSOSet ImageIdDefinitionSOSet;
    public TextMeshProUGUI TitleText;
    public TextMeshProUGUI DescriptionText;
    public TextMeshProUGUI ManaCostText;
    public Image SkillPortraitImage;
    // public GameObject AlreadyUsedImage;
    public CombatSkill Data;
    public GameObject NotEnoughtMana_GO;
    public GameObject CantCast_GO;
    public GameObject SelectedImage_GO;
    public GameObject ManaCost_GO;
    public GameObject CurseSymbol_GO;
    public Image RarityImage;
    public UIQualityProgress UIQualityProgress;
    public GameObject SkillGO;

    [Header("If Buff should be also displayed, fill this")]
    public GameObject BuffGO;
    public TextMeshProUGUI BuffTitleText;
    public TextMeshProUGUI BuffDescriptionText;
    //  public TextMeshProUGUI BuffRankText;
    public Image BuffImage;


    public void SetData(CombatSkill _data, int _manaLeft)
    {
        if (SkillGO != null)
            SkillGO.gameObject.SetActive(false);

        if (BuffGO != null)
            BuffGO.gameObject.SetActive(false);

        if (SkillGO != null)
        {
            SkillGO.gameObject.SetActive(true);

            bool isCurse = _data.skillGroupId == "CURSE";
            //spawner = _spawner;
            Data = _data;
            if (isCurse)
                TitleText.SetText(Data.GetTitle() + "<color=\"purple\">(Curse)</color>");
            else
                TitleText.SetText(Data.GetTitle());

            ManaCostText.SetText(Data.manaCost.ToString());
            ManaCost_GO.SetActive(Data.manaCost > 0);
            DescriptionText.SetText(Data.GetDescription());


            SkillPortraitImage.sprite = ImageIdDefinitionSOSet.GetDefinitionById(Utils.DescriptionsMetadata.GetSkillMetadata(Data.skillId).imageId).Image;
            //  AlreadyUsedImage.SetActive(Data.alreadyUsed);

            CantCast_GO.gameObject.SetActive(Data.characterClass != AccountDataSO.CharacterData.characterClass && Data.characterClass != Utils.CHARACTER_CLASS.ANY);

            NotEnoughtMana_GO.SetActive(_manaLeft < Data.manaCost);
            if (SelectedImage_GO != null)
                SelectedImage_GO.SetActive(false);

            CurseSymbol_GO.SetActive(_data.skillGroupId == "CURSE");
            RarityImage.color = Utils.GetRarityColor(Data.rarity);

            UIQualityProgress.Setup(Data.quality, 5);
        }




        if (Data.buff != null && BuffGO != null)
        {
            if (!String.IsNullOrEmpty(Data.buff.buffId))
            {
                BuffGO.gameObject.SetActive(true);
                BuffTitleText.SetText(Data.buff.GetTitle());
                BuffDescriptionText.SetText(Data.buff.GetDescription());
                BuffImage.sprite = ImageIdDefinitionSOSet.GetDefinitionById(Utils.DescriptionsMetadata.GetSkillMetadata(Data.buff.buffId).imageId).Image;
                //                BuffRankText.SetText("Rank " + Data.buff.rank.ToString());
            }
        }
    }

    public void ShowAsSelected(bool _selected)
    {
        if (SelectedImage_GO != null)
            SelectedImage_GO.SetActive(_selected);
    }



}
