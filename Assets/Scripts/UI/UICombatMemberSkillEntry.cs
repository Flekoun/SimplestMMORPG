using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using simplestmmorpg.data;
using UnityEngine.UI;

public class UICombatMemberSkillEntry : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    public ImageIdDefinitionSOSet ImageIdDefinitionSOSet;
    public TextMeshProUGUI TitleText;
    public TextMeshProUGUI DescriptionText;
  //  public TextMeshProUGUI RarityText;
    public TextMeshProUGUI ManaCostText;
    public Image SkillPortraitImage;
    public GameObject AlreadyUsedImage;
    public CombatSkill Data;
    public GameObject NotEnoughtMana_GO;
    public GameObject CantCast_GO;
    public GameObject SelectedImage_GO;

    private UICombatMemberSkillsSpawner spawner;

   


    public void SetData(CombatSkill _data , UICombatMemberSkillsSpawner _spawner)
    {
        spawner = _spawner;
        Data = _data;

        TitleText.SetText(Data.GetTitle());
        ManaCostText.SetText(Data.manaCost.ToString());
        DescriptionText.SetText(Data.GetDescription());

        SkillPortraitImage.sprite = ImageIdDefinitionSOSet.GetDefinitionById(Utils.GetMetadataForSkill(Data.skillId).imageId).Image;
        AlreadyUsedImage.SetActive(Data.alreadyUsed);

        CantCast_GO.gameObject.SetActive(Data.characterClass != AccountDataSO.CharacterData.characterClass && Data.characterClass!= Utils.CHARACTER_CLASS.ANY);

        NotEnoughtMana_GO.SetActive(_spawner.Data.stats.mana < Data.manaCost);
        SelectedImage_GO.SetActive(false);
    }

    public void ShowAsSelected(bool _selected)
    {
        SelectedImage_GO.SetActive(_selected);
    }



    // Update is called once per frame
    void Update()
    {

    }

    public void OnClicked()
    {
        spawner.SkillClicked(this);
    }
}
