using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using simplestmmorpg.adminToolsData;
using UnityEngine.Events;



public class UIDropTableItem : MonoBehaviour
{
    public ImageIdDefinitionSOSet AllImageIdDefinitionSOSet;

    public TMP_InputField AmountInput;
    public TMP_InputField ChanceInput;
    //  public TextMeshProUGUI ChanceText;
    public TextMeshProUGUI ItemIdText;
    public TMP_Dropdown RarityDropdown;
    public Image RarityImage;
    public Image PortraitImage;

    public DropTableItem Data;

    public UnityAction<UIDropTableItem> OnRemoveClicked;

    private string chanceInPercentage;

    // Start is called before the first frame update
    public void Setup(DropTableItem _item)
    {
        Data = _item;

        AmountInput.text = _item.amount.ToString();

        ChanceInput.text = ((_item.chanceToSpawn * 100) + "%").ToString();
        ItemIdText.SetText(_item.itemId.ToString());

        bool hasRarity = !string.IsNullOrEmpty(_item.rarity);
        RarityDropdown.gameObject.SetActive(hasRarity);

        if (hasRarity)
        {
            RarityImage.color = Utils.GetRarityColor(_item.rarity.ToString());
            RarityDropdown.value = Utils.GetRarityIndex(_item.rarity.ToString());
        }

        PortraitImage.sprite = AllImageIdDefinitionSOSet.GetDefinitionById(_item.GetImageId()).Image;

    }

    public void OnRarityDropDownValueChanged(int _value)
    {
        Data.rarity = Utils.GetRarityByIndex(_value);
    }

    public void OnValueChanged(string _value)
    {
        double parsedChance = 0;
        double.TryParse(_value, out parsedChance);

        Data.chanceToSpawn = parsedChance;

        ChanceInput.text = ((parsedChance * 100) + "%").ToString();
    }

    public void OnSelect(string _value)
    {
        ChanceInput.text = Data.chanceToSpawn.ToString();
    }

    public void Save()
    {
        Data.amount = int.Parse(AmountInput.text);
        Debug.Log(Data.itemId + " : " + int.Parse(AmountInput.text));
    }

    public void RemoveClicked()
    {
        OnRemoveClicked.Invoke(this);
    }
}
