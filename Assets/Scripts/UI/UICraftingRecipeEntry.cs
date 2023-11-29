using System.Collections;
using System.Collections.Generic;
using simplestmmorpg.data;
using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class UICraftingRecipeEntry : UISelectableEntry
{
    public AccountDataSO AccountDataSO;
    public TextMeshProUGUI TitleText;
    public UnityAction<UICraftingRecipeEntry> OnClicked;

    public CraftingRecipe Data;

    // Start is called before the first frame update
    public void SetData(CraftingRecipe _data)
    {
        Data = _data;
        if (Utils.DescriptionsMetadata.DoesDescriptionMetadataForIdExist(_data.id))
            TitleText.SetText(Utils.ReplacePlaceholdersInTextWithDescriptionFromMetadata(Utils.DescriptionsMetadata.GetCratingRecipesMetadata(_data.id).title.GetText()));
        else
            Debug.LogError("Cant find Metadata for recipe : " + _data.id);

        if (_data.CanBeCrafted(AccountDataSO.CharacterData))
            TitleText.color = Color.green;
        else
            TitleText.color = Color.white;
    }

    // Update is called once per frame
    public void Clicked()
    {
        OnClicked?.Invoke(this);
    }

    public override string GetUid()
    {
        return Data.id;
    }
}
