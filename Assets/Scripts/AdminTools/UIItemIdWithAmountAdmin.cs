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

    public UnityAction<UIItemIdWithAmountAdmin> OnRemoveClicked;

    private string chanceInPercentage;

    // Start is called before the first frame update
    public void SetData(ItemIdWithAmountAdmin _item)
    {
        Data = _item;
        AmountInput.text = _item.amount.ToString();
        ItemIdText.SetText(_item.itemId.ToString());
        PortraitImage.sprite = AllImageIdDefinitionSOSet.GetDefinitionById(_item.GetImageId()).Image;

    }




    public void AmountInputValueChanged(string _value)
    {
        Data.amount = int.Parse(_value);
    }

    public void RemoveClicked()
    {
        OnRemoveClicked.Invoke(this);
    }
}
