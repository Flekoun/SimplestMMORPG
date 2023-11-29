using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using simplestmmorpg.data;
using UnityEngine.UI;
using UnityEngine.Events;
using System;

public class UICombatMemberSkillEntry : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    //public ImageIdDefinitionSOSet ImageIdDefinitionSOSet;
    //public TextMeshProUGUI TitleText;
    //// public TextMeshProUGUI RankText;
    //public TextMeshProUGUI DescriptionText;
    ////  public TextMeshProUGUI RarityText;
    //public TextMeshProUGUI ManaCostText;
    //public Image SkillPortraitImage;
    //public GameObject AlreadyUsedImage;

    //public GameObject NotEnoughtMana_GO;
    //public GameObject CantCast_GO;
    //public GameObject SelectedImage_GO;
    //public GameObject ManaCost_GO;
    //public GameObject CurseSymbol_GO;
    //public HoldButton HoldButton;
    //public Image RarityImage;
    //public UIQualityProgress UIQualityProgress;

    //public GameObject BeignCastedEffect;

    //[Header("If Buff should be also displayed, fill this")]
    //public GameObject BuffGO;
    //public TextMeshProUGUI BuffTitleText;
    //public TextMeshProUGUI BuffDescriptionText;
    ////  public TextMeshProUGUI BuffRankText;
    //public Image BuffImage;
    public CombatSkill Data;
    public GameObject BeignCastedEffect;
    public DragDrop DragDrop;
    public UICombatSkillVisuals UICombatSkillVisuals;
    //public UnityAction<UICombatMemberSkillEntry> OnClicked;
    //public UnityAction<UICombatMemberSkillEntry> OnHoldFinished;
    public TooltipSpawner TooltipSpawner;

    public UnityAction<UICombatEntity, UICombatMemberSkillEntry> DropedOnCombatEntity;
    // public UnityAction StartDrag;
    //private CombatMember spawner;
    public void OnEnable()
    {

        if (DragDrop != null)
        {
            DragDrop.OnDropedOnZone += OnDropedOnZone;
            //     DragDrop.OnStartDrag += OnStartDrag;
        }
    }

    public void OnDisable()
    {
        if (DragDrop != null)
            DragDrop.OnDropedOnZone -= OnDropedOnZone;
    }

    public void SkillFailedToBeCasted()
    {
        DragDrop.ResetPosition();
        BeignCastedEffect.gameObject.SetActive(false);
    }

    //private void OnStartDrag()
    //{
    //    StartDrag?.Invoke();
    //}


    private void OnDropedOnZone(DropZone _dropZone)
    {
        UICombatEntity combatEntity = _dropZone.GetComponent<UICombatEntity>();

        if (!combatEntity.IsValidTargetForSkill(this.Data))
        {
            combatEntity.FloatingTextSpawner.Spawn("Invalid Target!", Color.red, combatEntity.FloatingTextsParent);
            DragDrop.ResetPosition();
            return;
        }

        if (combatEntity != null)
        {
            DropedOnCombatEntity?.Invoke(combatEntity, this);
            BeignCastedEffect.gameObject.SetActive(true);
            //  Debug.Log("uid entity: " + combatEntity.Data.uid);
        }
        else Debug.LogError("Skript Combat Entita na kterou dropujes Spell neexistuje!");
    }



    public void SetData(CombatSkill _data, int _manaLeft)
    {

        BeignCastedEffect.gameObject.SetActive(false);

        bool isCurse = _data.skillGroupId == "CURSE";
        //spawner = _spawner;
        Data = _data;
        TooltipSpawner.SetCombatSkill(Data, _manaLeft);
        UICombatSkillVisuals.SetData(Data, _manaLeft);
        //if (isCurse)
        //    TitleText.SetText(Data.GetTitle() + "<color=\"purple\">(Curse)</color>");
        //else
        //    TitleText.SetText(Data.GetTitle());

        //ManaCostText.SetText(Data.manaCost.ToString());
        //ManaCost_GO.SetActive(Data.manaCost > 0);
        //DescriptionText.SetText(Data.GetDescription());

        //HoldButton?.SetFunctional(Data.manaCost >= 0);

        //SkillPortraitImage.sprite = ImageIdDefinitionSOSet.GetDefinitionById(Utils.DescriptionsMetadata.GetSkillMetadata(Data.skillId).imageId).Image;
        //AlreadyUsedImage.SetActive(Data.alreadyUsed);

        //CantCast_GO.gameObject.SetActive(Data.characterClass != AccountDataSO.CharacterData.characterClass && Data.characterClass != Utils.CHARACTER_CLASS.ANY);

        //NotEnoughtMana_GO.SetActive(_manaLeft < Data.manaCost);
        //if (SelectedImage_GO != null)
        //    SelectedImage_GO.SetActive(false);

        //CurseSymbol_GO.SetActive(_data.skillGroupId == "CURSE");
        //RarityImage.color = Utils.GetRarityColor(Data.rarity);

        //UIQualityProgress.Setup(Data.quality, 5);

        //if (BuffGO != null)
        //    BuffGO.gameObject.SetActive(false);

        //if (Data.buff != null && BuffGO != null)
        //{
        //    if (!String.IsNullOrEmpty(Data.buff.buffId))
        //    {
        //        BuffGO.gameObject.SetActive(true);
        //        BuffTitleText.SetText(Data.buff.GetTitle());
        //        BuffDescriptionText.SetText(Data.buff.GetDescription());
        //        BuffImage.sprite = ImageIdDefinitionSOSet.GetDefinitionById(Utils.DescriptionsMetadata.GetSkillMetadata(Data.buff.buffId).imageId).Image;
        //        //                BuffRankText.SetText("Rank " + Data.buff.rank.ToString());
        //    }
        //}
    }

    public void ShowAsSelected(bool _selected)
    {
        if (UICombatSkillVisuals.SelectedImage_GO != null)
            UICombatSkillVisuals.SelectedImage_GO.SetActive(_selected);
    }



    // Update is called once per frame
    void Update()
    {

    }

    //public void Clicked()
    //{
    //    OnClicked?.Invoke(this);
    //}
    //public void HoldFinished()
    //{
    //    OnHoldFinished?.Invoke(this);
    //}
}
