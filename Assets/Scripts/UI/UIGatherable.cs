using System.Collections;
using System.Collections.Generic;
using simplestmmorpg.data;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIGatherable : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    public ImageIdDefinitionSOSet AllImageIdDefinitionSOSet;
    public TextMeshProUGUI DisplayNameText;
    public Image PortratImage;
    public Image RarityImage;

    public Gatherable Data = null;

    public UnityAction<UIGatherable> OnClicked;


    public void SetData(Gatherable _data)
    {
        Data = _data;
        DisplayNameText.SetText(Utils.DescriptionsMetadata.GetGatherablesMetadata(Data.gatherableType).title.GetText());

        RarityImage.color = Utils.GetRarityColor(Data.rarity);
        PortratImage.sprite = AllImageIdDefinitionSOSet.GetDefinitionById((Utils.DescriptionsMetadata.GetGatherablesMetadata(Data.gatherableType).imageId)).Image;

        if (Data.HasEnoughtSkillToGatherThis(AccountDataSO.CharacterData.professions))
            PortratImage.color = Color.white;
        else
            PortratImage.color = Color.gray;
    }

    public void ButtonClicked()
    {
        if (OnClicked != null)
            OnClicked.Invoke(this);
    }

}
