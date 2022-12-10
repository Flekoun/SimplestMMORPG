using System.Collections;
using System.Collections.Generic;
using simplestmmorpg.data;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

public class UIContentDetail : MonoBehaviour
{
    public ImageIdDefinitionSOSet AllImageIdDefinitionSOSet;
    public TextMeshProUGUI DisplayNameText;
    public TextMeshProUGUI RarityText;
    public TextMeshProUGUI LevelText;
    public TextMeshProUGUI SellPriceText;
    public Image PortraitImage;
    public GameObject Model;
    public UnityAction OnHideClicked;
    private Content Data;

    public void Show(Content _data)
    {
        Data = _data;
        DisplayNameText.SetText(_data.GetDisplayName());
        SellPriceText.SetText((_data.amount * _data.sellPrice).ToString());
        //   LevelText.SetText("Requires level " + Data.level.ToString());
        RarityText.SetText(Data.rarity);
        RarityText.color = Utils.GetRarityColor(Data.rarity);
        PortraitImage.sprite = AllImageIdDefinitionSOSet.GetDefinitionById(Data.GetImageId()).Image;
        Model.gameObject.SetActive(true);
    }

    public void Hide()
    {
        Model.gameObject.SetActive(false);
    }

    public void HideButtonClicked()
    {
        if (OnHideClicked != null)
            OnHideClicked.Invoke();

        Hide();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
