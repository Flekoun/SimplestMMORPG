using System.Collections;
using System.Collections.Generic;
using simplestmmorpg.data;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class UICraftingRecipeDetail : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    public PrefabFactory PrefabFactory;
    //   public TextMeshProUGUI TitleText;
    public GameObject UIContentItemPrefab;
    public GameObject UIRandomEquipPrefab;

    public Transform ProductParent;
    public Transform MaterialsParent;
    public UnityAction<UIContentItem> OnMaterialOrProductClicked;

    public CraftingRecipe Data;

    // Start is called before the first frame update
    public void SetData(CraftingRecipe _data)
    {
        Data = _data;

        Utils.DestroyAllChildren(MaterialsParent);
        Utils.DestroyAllChildren(ProductParent);
        foreach (var mat in Data.materials)
        {
            var matUi = PrefabFactory.CreateGameObject<UIContentItem>(UIContentItemPrefab, MaterialsParent);
            matUi.OnClicked += OnClicked;
            matUi.SetData(mat, true);
            matUi.SetAmountOwned(AccountDataSO.CharacterData.inventory.GetAmountOfItemsInInventory(mat.itemId));

        }

        if (Data.product != null)
        {

            var product = PrefabFactory.CreateGameObject<UIContentItem>(UIContentItemPrefab, ProductParent);
            product.OnClicked += OnClicked;
            product.SetData(Data.product, true);
        }
        else if (Data.productContent != null)
        {

            var product = PrefabFactory.CreateGameObject<UIContentItem>(UIContentItemPrefab, ProductParent);
            product.OnClicked += OnClicked;
            product.SetData(Data.productContent.GetContent(), true);
        }
        else if (Data.productRandomEquip != null)
        {
            var product = PrefabFactory.CreateGameObject<UIRandomEquip>(UIRandomEquipPrefab, ProductParent);
            //  product.OnClicked += OnClicked;
            product.SetData(Data.productRandomEquip);
        }



    }

    // Update is called once per frame
    public void OnClicked(UIContentItem _item)
    {
        OnMaterialOrProductClicked?.Invoke(_item);
    }
}
