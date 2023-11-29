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
    public UIPriceLabel GoldRewardPriceLabel;
    public Image PortratImage;
    public Image RarityImage;
    public GameObject ExpiredGO;
    public Image SkillImage;
    public Image SkillClassImage;
    public TooltipSpawner TooltipSpawner;
    public UIQualityProgress UIQualityProgress;

    public IContentDisplayable Data = null;
    //  public ItemIdWithAmount DataITemWithId = null;
    // private Color amountTextcolor;
    public UnityAction<UIContentItem> OnClicked;

    public IContentDisplayable GetData()
    {
        return Data;
    }

    public void Start()
    {

    }
    public void SetData(IContentDisplayable _data, bool _enableTooltip = false)
    {
        Data = _data;

        DisplayNameText.SetText(Utils.ReplacePlaceholdersInTextWithDescriptionFromMetadata(Data.GetDisplayName()));

        AmountText.SetText(Data.amount.ToString());//+"/"+ Data.stackSize.ToString());
        PriceText.SetText((Data.amount * Data.sellPrice).ToString());

        RarityImage.color = Utils.GetRarityColor(Data.rarity);
        PortratImage.sprite = AllImageIdDefinitionSOSet.GetDefinitionById(Data.GetImageId()).Image;

        if (ExpiredGO != null)
        {
            if (!string.IsNullOrEmpty(Data.expireDate))
                ExpiredGO.SetActive(Utils.GetTimeLeftToDateInSeconds(Data.expireDate) <= 0);
            else
                ExpiredGO.SetActive(false);
        }

        //   Debug.Log("Eh2?" + Data.amount);
        AmountText.gameObject.SetActive(Data.amount != 1);


        if (Data is Equip)
        {
            UIQualityProgress.gameObject.SetActive(true);
            UIQualityProgress.Setup(Data.quality, Data.qualityMax);

            SkillImage.gameObject.SetActive(true);
            SkillImage.sprite = AllImageIdDefinitionSOSet.GetDefinitionById((Data as Equip).skill.skillGroupId).Image;
            SkillClassImage.gameObject.SetActive(true);
            SkillClassImage.color = Utils.GetClassColor((Data as Equip).skill.characterClass);
        }
        else
        {
            UIQualityProgress.gameObject.SetActive(false);
            SkillImage.gameObject.SetActive(false);
            SkillClassImage.gameObject.SetActive(false);
        }



        if (GoldRewardPriceLabel != null)
        {
            if (Data.itemId == "GOLD")
            {
                AmountText.gameObject.SetActive(false);
                PortratImage.gameObject.SetActive(false);
                GoldRewardPriceLabel.gameObject.SetActive(true);
                GoldRewardPriceLabel.SetPrice(Data.amount);
            }
        }

        TooltipSpawner.IsFunctional = _enableTooltip;
        TooltipSpawner.SetContentCointainer(Data);

    }



    public void SetAmountOwned(int _amount)
    {
        AmountText.gameObject.SetActive(true);
        //  Debug.Log("Eh?" + Data.amount);
        AmountText.SetText(_amount.ToString() + "/" + Data.amount.ToString());
        if (_amount < Data.amount)
            AmountText.color = Color.red;
        else
            AmountText.color = Color.white;
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
