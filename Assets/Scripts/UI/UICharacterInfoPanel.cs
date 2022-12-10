using System.Collections;
using System.Collections.Generic;
using simplestmmorpg.data;
using UnityEngine;
using TMPro;
public class UICharacterInfoPanel : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    public TextMeshProUGUI CharacterNameText;
    public TextMeshProUGUI ClassText;
    // public TextMeshProUGUI GoldText;
    //  public TextMeshProUGUI SilverText;
    //  public TextMeshProUGUI FoodText;

    public UIProgressBar UIXPProgressBar;
    public TextMeshProUGUI Level;

    public UIProgressBar UIHealthProgress;
    public UIProgressBar UIManaProgress;

    public TextMeshProUGUI Attribute_StrengthText;
    public TextMeshProUGUI Attribute_StaminaText;
    public TextMeshProUGUI Attribute_IntellectText;
    public TextMeshProUGUI Attribute_AgilityText;
    public TextMeshProUGUI Attribute_SpiritText;

    public TextMeshProUGUI HealthText;
    public TextMeshProUGUI ManaText;

    public TextMeshProUGUI FatigueText;
    public TextMeshProUGUI TravelTimeText;



    public GameObject Model;
    public CharacterData Data;
    // Start is called before the first frame update

    public void OnEnable()
    {

    }

    public void OnDisable()
    {

    }

    public void ShowPlayerCurrentCharacterInfo()
    {
        Show(AccountDataSO.CharacterData);
    }

    public void Show(CharacterData _data)  //TODO: NEFACHA POUZIVAM STEJNE ACCOUNT DATA
    {
        Data = _data;
        Model.gameObject.SetActive(true);
        Refresh();
        AccountDataSO.OnCharacterDataChanged += Refresh;
    }

    private void Refresh()
    {
        Data = AccountDataSO.CharacterData;

        Debug.Log("REFRESHING CHARACTER INFO");
        CharacterNameText.SetText(Data.characterName);
        ClassText.SetText("Level " + Data.stats.level + " " + Utils.ColorizeGivenTextWithClassColor(Data.characterClass, Data.characterClass));
        //   GoldText.SetText(Data.currency.gold.ToString() + " gold");
        UIXPProgressBar.SetValues(Data.stats.expNeededToReachNextLevel - Data.stats.expNeededToReachLastLevel, Data.stats.exp - Data.stats.expNeededToReachLastLevel);
        Level.SetText("Level " + Data.stats.level.ToString());
        //    SilverText.SetText( Data.currency.silver.ToString()+ " silver");
        //     FoodText.SetText(Data.currency.food.ToString() + " food");
        int totalHealth = Mathf.RoundToInt(Data.stats.healthMultiplier * (Data.GetTotalGearAttribute(Utils.EQUIP_ATTRIBUTES.STAMINA) + Data.stats.stamina));
        int totalMana = Mathf.RoundToInt(Data.stats.manaMultiplier * (Data.GetTotalGearAttribute(Utils.EQUIP_ATTRIBUTES.INTELLECT) + Data.stats.intellect));
        UIHealthProgress.SetValues(totalHealth, totalHealth);
        UIManaProgress.SetValues(totalMana, totalMana);

        int totalStamina = (Data.stats.stamina + Data.GetTotalGearAttribute(Utils.EQUIP_ATTRIBUTES.STAMINA));
        int totalIntelect = (Data.stats.stamina + Data.GetTotalGearAttribute(Utils.EQUIP_ATTRIBUTES.INTELLECT));

        Attribute_StrengthText.SetText("Strength: " + (Data.stats.strength + Data.GetTotalGearAttribute(Utils.EQUIP_ATTRIBUTES.STRENGTH)).ToString());
        Attribute_StaminaText.SetText("Stamina: " + totalStamina.ToString());
        Attribute_IntellectText.SetText("Intellect: " + (Data.stats.intellect + Data.GetTotalGearAttribute(Utils.EQUIP_ATTRIBUTES.INTELLECT)).ToString());
        Attribute_AgilityText.SetText("Agility: " + totalIntelect.ToString());
        Attribute_SpiritText.SetText("Spirit: " + (Data.stats.spirit + Data.GetTotalGearAttribute(Utils.EQUIP_ATTRIBUTES.SPIRIT)).ToString());

        int totalHealthMax = Mathf.RoundToInt(totalStamina * Data.stats.healthMultiplier);
        int totalHealthTakenByFatigue = Mathf.RoundToInt(((totalStamina * Data.stats.healthMultiplier) /100f)* Data.currency.fatigue) ;

        int totalManaMax = Mathf.RoundToInt(totalIntelect * Data.stats.manaMultiplier);
        int totalManaTakenByFatigue = Mathf.RoundToInt(((totalIntelect * Data.stats.manaMultiplier) / 100f) * Data.currency.fatigue);


        HealthText.SetText("Health: "+totalHealthMax.ToString() + "( <color=\"red\">-" + totalHealthTakenByFatigue + "</color>)");
        ManaText.SetText("Mana: "+totalManaMax.ToString() + "( <color=\"red\">-" + totalManaTakenByFatigue + "</color>)");

        FatigueText.SetText("Fatigue: " + " <color=\"red\">" + Data.currency.fatigue.ToString() + "%</color>");
        TravelTimeText.SetText("Travel Time:" +Data.currency.time.ToString() +"/48 hours" );
    }


    // Update is called once per frame
    public void Close()
    {
        Model.gameObject.SetActive(false);
        AccountDataSO.OnCharacterDataChanged -= Refresh;
    }


}
