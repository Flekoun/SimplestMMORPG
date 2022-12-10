using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;
using simplestmmorpg.data;

public class UIContentItem : UISelectableEntry
{
    public ImageIdDefinitionSOSet AllImageIdDefinitionSOSet;
    public TextMeshProUGUI DisplayNameText;
    public TextMeshProUGUI EquipSlotText;
    public TextMeshProUGUI AmountText;
    public TextMeshProUGUI PriceText;
    public Image PortratImage;
    public Image RarityImage;


    private Content Data = null;

    public UnityAction<UIContentItem> OnClicked;

    public Content GetData()
    {
        return Data;
    }
    // Start is called before the first frame update
    public void SetData(Content _data)
    {
        Data = _data;
        DisplayNameText.SetText(Data.GetDisplayName());

        AmountText.SetText(Data.amount.ToString());//+"/"+ Data.stackSize.ToString());
        PriceText.SetText((Data.amount * Data.sellPrice).ToString());
        RarityImage.color = Utils.GetRarityColor(Data.rarity);
        PortratImage.sprite = AllImageIdDefinitionSOSet.GetDefinitionById(Data.GetImageId()).Image;

        AmountText.gameObject.SetActive(Data.amount != 1);
        //if(Data is Equip)
        //{
        //    EquipSlotText.gameObject.SetActive(true);
        //    EquipSlotText.SetText(((Equip)Data).equipSlotId);
        //}
    }



    public void ButtonClicked()
    {
        if (OnClicked != null)
            OnClicked.Invoke(this);

    }

    public override string GetUid()
    {
        return Data.uid;
    }


}
