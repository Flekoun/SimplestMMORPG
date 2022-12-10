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
    public TextMeshProUGUI RankText;
    // public TextMeshProUGUI ClassText;
    public Image SkillImage;

    public GameObject BuffGO;
    public TextMeshProUGUI BuffTitleText;
    public TextMeshProUGUI BuffDescriptionText;
    public TextMeshProUGUI BuffRankText;
    public Image BuffImage;


    public Skill Data;

    public void SetData(Skill _data)
    {
        Data = _data;
    
        string classText = "";
      //  if (AccountDataSO.CharacterData.characterClass == Data.characterClass)
            classText = (Utils.ColorizeGivenTextWithClassColor(Data.characterClass, Data.characterClass));
      //  else
       //     classText = "<color=red>" + Data.characterClass + "</color>";

        TitleText.SetText(Data.GetTitle() + " (" + classText + ")");
        DescriptionText.SetText(Data.GetDescription());
        SkillImage.sprite = AllImageIdDefinitionSOSet.GetDefinitionById(Utils.GetMetadataForSkill(Data.skillId).imageId).Image;
        ManaCostText.SetText(Data.manaCost.ToString() + " Mana");
        RankText.SetText("Rank " + Data.rank.ToString());




        BuffGO.gameObject.SetActive(false);

        if (Data.buff != null)
        {
            Debug.Log("BUFF: " + Data.buff.buffId);
            if (Data.buff.buffId != "")
            {
                BuffGO.gameObject.SetActive(true);
                BuffTitleText.SetText(Data.buff.GetTitle());
                BuffDescriptionText.SetText(Data.buff.GetDescription());
                BuffImage.sprite = AllImageIdDefinitionSOSet.GetDefinitionById(Data.buff.buffId).Image;
                BuffRankText.SetText("Rank " + Data.buff.rank.ToString());
            }
        }



    }

}
