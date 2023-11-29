using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using simplestmmorpg.adminToolsData;
using UnityEngine.Events;

public class UIItemIdChooser : UISelectableSpawner
{
    public PrefabFactory PrefabFactory;
    public GameObject UIBaseDescriptionMetadataPrefab;
    public Transform Parent;
    public GameObject Model;

    public UnityAction<List<UISelectableEntry>> OnItemsToAddSelected;

    public bool Items = true;
    public bool CraftingRecipes = true;
    public bool Enemies = false;
    public bool PointsOfInterest = false;

    // private List<UIBaseDescriptionMetadata> List = new List<UIBaseDescriptionMetadata>();

    // Method to clear all listeners
    public void ClearAllListeners()
    {
        OnItemsToAddSelected = null;
    }


    // Start is called before the first frame update
    public void Show()
    {
        Utils.DestroyAllChildren(Parent);
        //  List.Clear();

        //   DropCountText.SetText(_item.dropCountMin.ToString() + " - " + _item.dropCountMax.ToString());
        if (Items)
        {
            foreach (var item in Utils.DescriptionsMetadata.items)
            {
                var UIItem = PrefabFactory.CreateGameObject<UIBaseDescriptionMetadata>(UIBaseDescriptionMetadataPrefab, Parent);
                UIItem.Setup(item);

                UIItem.OnClicked += OnItemClicked;
                // List.Add(UIItem);
            }
        }
        if (CraftingRecipes)
        {
            foreach (var item in Utils.DescriptionsMetadata.cratingRecipes)
            {
                var UIItem = PrefabFactory.CreateGameObject<UIBaseDescriptionMetadata>(UIBaseDescriptionMetadataPrefab, Parent);
                UIItem.Setup(item);
                UIItem.OnClicked += OnItemClicked;
                // List.Add(UIItem);
            }
        }
        if (Enemies)
        {
            foreach (var item in Utils.DescriptionsMetadata.enemies)
            {
                var UIItem = PrefabFactory.CreateGameObject<UIBaseDescriptionMetadata>(UIBaseDescriptionMetadataPrefab, Parent);
                UIItem.Setup(item);
                UIItem.OnClicked += OnItemClicked;
                // List.Add(UIItem);
            }
        }
        if (PointsOfInterest)
        {
            foreach (var item in Utils.DescriptionsMetadata.pointsOfInterest)
            {
                var UIItem = PrefabFactory.CreateGameObject<UIBaseDescriptionMetadata>(UIBaseDescriptionMetadataPrefab, Parent);
                UIItem.Setup(item);
                UIItem.OnClicked += OnItemClicked;
                // List.Add(UIItem);
            }
        }
        Model.gameObject.SetActive(true);

    }


    public void Hide()
    {
        Model.gameObject.SetActive(false);
    }

    private void OnItemClicked(UIBaseDescriptionMetadata _item)
    {
        OnUISelectableItemClicked(_item);
    }

    public void AddItemsClicked()
    {
        OnItemsToAddSelected.Invoke(SelectedItems);
        ClearItemsSelected();
        Hide();
    }

}
