using System.Collections;
using System.Collections.Generic;
using simplestmmorpg.data;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

public class UIEquipDetail : MonoBehaviour
{
    public AccountDataSO AccountDataSO;

    public TextMeshProUGUI DisplayNameText;
    public TextMeshProUGUI RarityText;
    public TextMeshProUGUI EquipSlotText;
    public TextMeshProUGUI LevelText;
    //    public TextMeshProUGUI ClassText;

    public TextMeshProUGUI Attribute_StrenghtText;
    public TextMeshProUGUI Attribute_StaminaText;
    public TextMeshProUGUI Attribute_IntellectText;
    public TextMeshProUGUI Attribute_AgilityText;
    public TextMeshProUGUI Attribute_SpiritText;

    public TextMeshProUGUI BidTypeText;

    public TextMeshProUGUI RareEffect_Text;
  //  public TextMeshProUGUI Bonu_Text;

    public UnityAction OnHideClicked;

    public UIContentDetail UIContentDetail;
    public UISkill UISkill;
    public GameObject Model;

    public UIQualityProgress UIQualityProgress;

    public ContentFitterRefresh ContentFitterRefresh;
    private Equip Data;

    public void Awake()
    {
        UIContentDetail.OnHideClicked += HideClicked;
    }

    public void Show(Equip _data, int _forcedQuality)
    {
        Data = _data;
        Model.gameObject.SetActive(true);
        UISkill.SetData(Data.skill, _forcedQuality);
        UIContentDetail.Show(_data);
        DisplayNameText.SetText(_data.displayName);

        UIQualityProgress.Setup(_forcedQuality, Data.qualityMax);

        EquipSlotText.SetText(Data.equipSlotId);
        if (Data.level > AccountDataSO.CharacterData.stats.level)
            LevelText.SetText("<color=red>" + "Requires level " + Data.level.ToString() + "</color>");
        else
            LevelText.SetText("Requires level " + Data.level.ToString());

        RarityText.SetText(Data.rarity);
        RarityText.color = Utils.GetRarityColor(Data.rarity);

        RareEffect_Text.gameObject.SetActive(Data.rareEffects.Count > 0 || Data.skillBonusEffects.Count>0 || Data.buffBonusEffects.Count > 0);
        RareEffect_Text.text = "";
        foreach (var effect in Data.rareEffects)
        {
            RareEffect_Text.SetText(RareEffect_Text.text + effect.GetDescription() + "\n");
        }
        foreach (var skillEffect in Data.skillBonusEffects)
        {
            RareEffect_Text.SetText(RareEffect_Text.text + Utils.ReplacePlaceholdersInTextWithDescriptionFromMetadata(skillEffect.GetDescription()) + "\n");
        }

        foreach (var buffEffect in Data.buffBonusEffects)
        {
            RareEffect_Text.SetText(RareEffect_Text.text + Utils.ReplacePlaceholdersInTextWithDescriptionFromMetadata(buffEffect.GetDescription()) + "\n");
        }


        BidTypeText.gameObject.SetActive(true);

        if (Data.neverEquiped)
            BidTypeText.SetText("Binds when equipped");
        else
            BidTypeText.SetText("Soulbound");


        ContentFitterRefresh.RefreshContentFitters();
    }

    public void Show(Equip _data)
    {
        Show(_data, _data.quality);

    }

    public void Hide()
    {
        Model.gameObject.SetActive(false);
    }

    private void HideClicked()
    {
        if (OnHideClicked != null)
            OnHideClicked.Invoke();
    }

  
}
