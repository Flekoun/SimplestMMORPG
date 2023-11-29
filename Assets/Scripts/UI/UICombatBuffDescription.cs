using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using simplestmmorpg.data;
using UnityEngine.UI;
using UnityEngine.Events;

public class UICombatBuffDescription : MonoBehaviour
{
   // public AccountDataSO AccountDataSO;
    public ImageIdDefinitionSOSet ImageIdDefinitionSOSet;
    public TextMeshProUGUI TitleText;
    public TextMeshProUGUI DescriptionText;
    public Image BuffPortraitImage;
   
    public CombatBuff Data;
    public GameObject Model;



    public void SetData(CombatBuff _data)
    {
       
        Data = _data;
        TitleText.SetText(Data.GetTitle());
        DescriptionText.SetText(Data.GetDescription() + "\n<b><color=\"yellow\">"+Data.turnsLeft.ToString()+" turns left</color></b>");


        BuffPortraitImage.sprite = ImageIdDefinitionSOSet.GetDefinitionById(Utils.DescriptionsMetadata.GetSkillMetadata(Data.buffId).imageId).Image;

        Model.gameObject.SetActive(true);
    }

    public void HideClicked()
    {
        Model.gameObject.SetActive(false);
    }
   
}
