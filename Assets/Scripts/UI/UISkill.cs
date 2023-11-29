using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using simplestmmorpg.data;
using UnityEngine.UI;
using System;

public class UISkill : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    public ImageIdDefinitionSOSet AllImageIdDefinitionSOSet;
    public TextMeshProUGUI TitleText;
    public TextMeshProUGUI DescriptionText;
    public TextMeshProUGUI ManaCostText;
    //public UISpawnGOCount UISpawnGOCount;
    //public TextMeshProUGUI RankText;
    // public TextMeshProUGUI ClassText;
    public Image SkillImage;

    public GameObject BuffGO;
    public TextMeshProUGUI BuffTitleText;
    public TextMeshProUGUI BuffDescriptionText;
    //  public TextMeshProUGUI BuffRankText;
    public Image BuffImage;


    public Skill Data;

    public void SetData(Skill _data, int _quality)
    {
        Data = _data;

        string classText = "";
        //  if (AccountDataSO.CharacterData.characterClass == Data.characterClass)
        classText = (Utils.ColorizeGivenTextWithClassColor(Data.characterClass, Data.characterClass));
        //  else
        //     classText = "<color=red>" + Data.characterClass + "</color>";

        TitleText.SetText(Data.GetTitle() + " (" + classText + ")");
        DescriptionText.SetText(Data.GetDescription(_quality));
        SkillImage.sprite = AllImageIdDefinitionSOSet.GetDefinitionById(Utils.DescriptionsMetadata.GetSkillMetadata(Data.skillId).imageId).Image;
        ManaCostText.SetText(Data.manaCost.ToString() + " Mana");
        //  RankText.SetText("Rank " + Data.rank.ToString());

        //if (UISpawnGOCount != null)
        //    UISpawnGOCount.Spawn(Data.successSlots);



        BuffGO.gameObject.SetActive(false);

        if (Data.buff != null)
        {
            if (!String.IsNullOrEmpty(Data.buff.buffId))
            {
                BuffGO.gameObject.SetActive(true);
                BuffTitleText.SetText(Data.buff.GetTitle());
                BuffDescriptionText.SetText(Data.buff.GetDescription(_quality));
                BuffImage.sprite = AllImageIdDefinitionSOSet.GetDefinitionById(Utils.DescriptionsMetadata.GetSkillMetadata(Data.buff.buffId).imageId).Image;
                //                BuffRankText.SetText("Rank " + Data.buff.rank.ToString());
            }
        }



    }




}
