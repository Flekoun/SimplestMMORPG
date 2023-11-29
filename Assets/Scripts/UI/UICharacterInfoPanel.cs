using System.Collections;
using System.Collections.Generic;
using simplestmmorpg.data;
using UnityEngine;
using TMPro;
using System;

public class UICharacterInfoPanel : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    public PrefabFactory PrefabFactory;

    public TextMeshProUGUI CharacterNameText;
    public TextMeshProUGUI ClassText;
    // public TextMeshProUGUI GoldText;
    //  public TextMeshProUGUI SilverText;
    //  public TextMeshProUGUI FoodText;

    public UIProgressBar UIXPProgressBar;
    public TextMeshProUGUI Level;

    public UIProgressBar UIHealthProgress;
    public UIProgressBar UIManaProgress;

    public TextMeshProUGUI Attribute_CritChanceText;
    public TextMeshProUGUI Attribute_DamagePowerText;
    public TextMeshProUGUI Attribute_ManaRegenText;
    public TextMeshProUGUI Attribute_HealthRegenText;
    public TextMeshProUGUI Attribute_DefenseText;
    public TextMeshProUGUI Attribute_ResistenceText;

    public TextMeshProUGUI HealthText;
    public TextMeshProUGUI ManaText;

    public TextMeshProUGUI FatigueText;
    public TextMeshProUGUI TravelTimeText;

    public TextMeshProUGUI ProfessionsText;

    public Transform CursesParent;
    public GameObject CursePrefab;


    public Transform BlessParent;
    public GameObject BlessPrefab;

    public UIFoodEffectSpawner UIFoodEffectSpawner;

    public GameObject Model;
    public CharacterData Data;
    // Start is called before the first frame update

    public void OnEnable()
    {

    }

    public void OnDisable()
    {

    }

    //public void ShowPlayerCharacter()
    //{
    //    Show(AccountDataSO.CharacterData);
    //}

    public void Show(CharacterData _data)  //TODO: NEFACHA POUZIVAM STEJNE ACCOUNT DATA
    {
        Data = _data;
        Model.gameObject.SetActive(true);
        Refresh();
        AccountDataSO.OnCharacterDataChanged += Refresh;
    }

    private void Refresh()
    {
        // Data = AccountDataSO.CharacterData;

        Debug.Log("REFRESHING CHARACTER INFO");
        CharacterNameText.SetText(Data.characterName);
        ClassText.SetText("Level " + Data.stats.level + " " + Utils.ColorizeGivenTextWithClassColor(Data.characterClass, Data.characterClass));
        UIXPProgressBar.SetValues(Data.stats.expNeededToReachNextLevel - Data.stats.expNeededToReachLastLevel, Data.stats.exp - Data.stats.expNeededToReachLastLevel);
        Level.SetText("Level " + Data.stats.level.ToString());

        Attribute_CritChanceText.SetText("<color=\"yellow\">Crit chance: </color>" + Data.stats.totalCritChance + "%");
        Attribute_DamagePowerText.SetText("<color=\"yellow\">Damage power: </color> " + Data.stats.totalDamagePower.ToString());
        Attribute_DefenseText.SetText("<color=\"yellow\">Defense: </color> " + Data.stats.totalDefense.ToString());
        Attribute_HealthRegenText.SetText("<color=\"yellow\">Health regen:  </color>" + Data.stats.totalHealthRegen.ToString());
        Attribute_ManaRegenText.SetText("<color=\"yellow\">Mana regen: </color> " + Data.stats.totalManaRegen.ToString());
        Attribute_ResistenceText.SetText("<color=\"yellow\">Resistence: </color> " + Data.stats.totalResistence.ToString());
        //int totalHealth = Mathf.RoundToInt(Data.stats.healthMultiplier * (Data.GetTotalGearAttribute(Utils.EQUIP_ATTRIBUTES.STAMINA) + Data.stats.stamina));
        //int totalMana = Mathf.RoundToInt(Data.stats.manaMultiplier * (Data.GetTotalGearAttribute(Utils.EQUIP_ATTRIBUTES.INTELLECT) + Data.stats.intellect));
        //UIHealthProgress.SetValues(totalHealth, totalHealth);
        //UIManaProgress.SetValues(totalMana, totalMana);

        //int totalStamina = (Data.stats.stamina + Data.GetTotalGearAttribute(Utils.EQUIP_ATTRIBUTES.STAMINA));
        //int totalIntelect = (Data.stats.intellect + Data.GetTotalGearAttribute(Utils.EQUIP_ATTRIBUTES.INTELLECT));

        //Attribute_StrengthText.SetText("Strength: " + (Data.stats.strength + Data.GetTotalGearAttribute(Utils.EQUIP_ATTRIBUTES.STRENGTH)).ToString());
        //Attribute_StaminaText.SetText("Stamina: " + totalStamina.ToString());
        //Attribute_IntellectText.SetText("Intellect: " + totalIntelect.ToString());
        //Attribute_AgilityText.SetText("Agility: " + (Data.stats.agility + Data.GetTotalGearAttribute(Utils.EQUIP_ATTRIBUTES.AGILITY)).ToString());
        ////  Attribute_SpiritText.SetText("Spirit: " + (Data.stats.spirit + Data.GetTotalGearAttribute(Utils.EQUIP_ATTRIBUTES.SPIRIT)).ToString());

        int totalHealthMax = Data.GetTotalHealth(true);
        int totalHealthTakenByFatigue = totalHealthMax - Data.GetTotalHealth(false);

        int totalManaMax = Data.stats.totalMaxMana;//Utils.RoundToInt(totalIntelect * Data.stats.manaMultiplier);

        int currentHealth = Data.stats.currentHealth;
        if (currentHealth > totalHealthMax - totalHealthTakenByFatigue)
            currentHealth = totalHealthMax - totalHealthTakenByFatigue;
        //HealthText.SetText("Health: " + totalHealthMax.ToString() + "( <color=\"red\">-" + totalHealthTakenByFatigue + "</color>)");
        //ManaText.SetText("Mana: " + totalManaMax.ToString());//+ "( <color=\"red\">-" + totalManaTakenByFatigue + "</color>)");

        UIManaProgress.SetValues(totalManaMax, totalManaMax);
        UIHealthProgress.SetValues(Data.GetTotalHealth(true) - Data.GetHealthTakenByFatiguePenalty(), Data.stats.currentHealth, Data.GetHealthTakenByFatiguePenalty());
        // UIHealthProgress.SetValues(totalHealthMax - totalHealthTakenByFatigue, currentHealth, totalHealthTakenByFatigue);

        FatigueText.SetText("Fatigue: " + " <color=\"red\">" + Data.currency.fatigue.ToString() + "%</color>");
        TravelTimeText.SetText("Travel Time:" + Data.currency.time.ToString() + "/" + Data.currency.timeMax.ToString());

        ProfessionsText.transform.parent.gameObject.SetActive(Data.professions.Count > 0);
        ProfessionsText.SetText("");
        foreach (var profession in Data.professions)
        {
            ProfessionsText.SetText(Utils.ReplacePlaceholdersInTextWithDescriptionFromMetadata(ProfessionsText.text + "\n" + profession.id + " : " + profession.count + "/" + profession.countMax));
        }

        CursesParent.transform.parent.gameObject.SetActive(Data.curses.Count > 0);
        Utils.DestroyAllChildren(CursesParent);
        foreach (var item in Data.curses)
        {
            var curse = PrefabFactory.CreateGameObject<UICombatMemberSkillEntry>(CursePrefab, CursesParent);
            curse.SetData(item, 100);
            //curse.OnClicked += OnCurseClicked;

        }

        BlessParent.transform.parent.gameObject.SetActive(Data.blesses.Count > 0);
        Utils.DestroyAllChildren(BlessParent);
        foreach (var item in Data.blesses)
        {
            var bless = PrefabFactory.CreateGameObject<UIBless>(BlessPrefab, BlessParent);
            bless.Setup(item);
        }

        UIFoodEffectSpawner.gameObject.SetActive(Data.foodEffects.Count > 0);
        UIFoodEffectSpawner.Refresh();
    }

    //private void OnCurseClicked(UICombatMemberSkillEntry arg0)
    //{
    //    UIManager.instance.ContextInfoPanel.ShowContextCombatSkill(arg0.Data, 100);

    //}


    // Update is called once per frame
    public void Close()
    {
        Model.gameObject.SetActive(false);
        AccountDataSO.OnCharacterDataChanged -= Refresh;
    }


}
