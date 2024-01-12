using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using simplestmmorpg.adminToolsData;
using UnityEngine.Events;
using simplestmmorpg.data;

public class UIItemIdWithAmountAdmin : MonoBehaviour
{
    public ImageIdDefinitionSOSet AllImageIdDefinitionSOSet;

    public TMP_InputField AmountInput;
    //  public TextMeshProUGUI ChanceText;
    public TextMeshProUGUI ItemIdText;
    public Image PortraitImage;

    public ItemIdWithAmountAdmin Data;
    public ItemIdWithAmount Data2;

    public UnityAction<UIItemIdWithAmountAdmin> OnRemoveClicked;

    private string chanceInPercentage;

    // Start is called before the first frame update
    public void SetData(ItemIdWithAmountAdmin _item)
    {
        Data = _item;
        Data2 = null;
        AmountInput.text = _item.amount.ToString();
        ItemIdText.SetText(_item.itemId.ToString());
        PortraitImage.sprite = AllImageIdDefinitionSOSet.GetDefinitionById(_item.GetImageId()).Image;

    }

    // Start is called before the first frame update
    public void SetData(ItemIdWithAmount _item)
    {
        Data2 = _item;
        Data = null;
        AmountInput.text = _item.amount.ToString();
        ItemIdText.SetText(_item.itemId.ToString());
        PortraitImage.sprite = AllImageIdDefinitionSOSet.GetDefinitionById(_item.GetImageId()).Image;

    }



    public void AmountInputValueChanged(string _value)
    {
        if (Data != null)
            Data.amount = int.Parse(_value);
        else if (Data2 != null)
            Data2.amount = int.Parse(_value);
    }

    public void RemoveClicked()
    {
        OnRemoveClicked.Invoke(this);
    }
}
