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

    public UnityAction OnHideClicked;

    public UIContentDetail UIContentDetail;
    public UISkill UISkill;
    public GameObject Model;

    public ContentFitterRefresh ContentFitterRefresh;
    private Equip Data;

    public void Awake()
    {
        UIContentDetail.OnHideClicked += HideClicked;
    }

    public void Show(Equip _data)
    {
        Data = _data;
        Model.gameObject.SetActive(true);
        UISkill.SetData(Data.skill);
        UIContentDetail.Show(_data);
        DisplayNameText.SetText(_data.displayName);

        EquipSlotText.SetText(Data.equipSlotId);
        if (Data.level > AccountDataSO.CharacterData.stats.level)
            LevelText.SetText("<color=red>" + "Requires level " + Data.level.ToString() + "</color>");
        else
            LevelText.SetText("Requires level " + Data.level.ToString());

        RarityText.SetText(Data.rarity);
        RarityText.color = Utils.GetRarityColor(Data.rarity);

        //if (AccountDataSO.CharacterData.characterClass == Data.skill.characterClass)
        //    ClassText.SetText(Utils.ColorizeGivenTextWithClassColor(Data.skill.characterClass, Data.skill.characterClass));
        //else
        //    ClassText.SetText("<color=red>" + Data.skill.characterClass + "</color>");

        Attribute_StrenghtText.gameObject.SetActive(Data.attributes.strength > 0);
        Attribute_StrenghtText.SetText("+" + Data.attributes.strength.ToString() + " Strength");

        Attribute_IntellectText.gameObject.SetActive(Data.attributes.intellect > 0);
        Attribute_IntellectText.SetText("+" + Data.attributes.intellect.ToString() + " Intellect");

        Attribute_AgilityText.gameObject.SetActive(Data.attributes.agility > 0);
        Attribute_AgilityText.SetText("+" + Data.attributes.agility.ToString() + " Agility");

        Attribute_StaminaText.gameObject.SetActive(Data.attributes.stamina > 0);
        Attribute_StaminaText.SetText("+" + Data.attributes.stamina.ToString() + " Stamina");

        Attribute_SpiritText.gameObject.SetActive(Data.attributes.spirit > 0);
        Attribute_SpiritText.SetText("+" + Data.attributes.spirit.ToString() + " Spirit");

        ContentFitterRefresh.RefreshContentFitters();
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

    //private void TryToFixScrollReckGlitches()
    //{
    //    StartCoroutine(Wait());

    //}

    //private IEnumerator Wait()
    //{
    //    yield return new WaitForSecondsRealtime(0.1f);
    //    LayoutRebuilder.ForceRebuildLayoutImmediate(RectTransform);
    //    Canvas.ForceUpdateCanvases();
    //}
}
