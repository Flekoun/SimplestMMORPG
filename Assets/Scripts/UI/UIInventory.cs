using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using simplestmmorpg.data;
using System.Linq;

public class UIInventory : UISelectableSpawner
{

    public enum INVENTORY_FILTER
    {
        ALL,
        EQUIP_ONLY,
        ITEMS_ONLY,
        CURRENCY_ONLY,
        FOOD_ONLY,
    }


    public INVENTORY_FILTER Filter;

    public AccountDataSO AccountDataSO;
    public FirebaseCloudFunctionSO FirebaseCloudFunctionSO;
    public PrefabFactory Factory;
    public Transform InventoryItemsLootParent;
    public GameObject UIInventoryItemPrefab;


    private List<ContentContainer> inventoryData = null;

    public UnityAction<UIContentItem> OnContentItemClicked;



    private void SpawnItems(string _contentType)
    {

        foreach (var content in inventoryData)
        {
            if (content.contentType == _contentType)
            {
                UIContentItem uiInventoryItem = Factory.CreateGameObject<UIContentItem>(UIInventoryItemPrefab, InventoryItemsLootParent);
                uiInventoryItem.SetData(content.GetContent());
                uiInventoryItem.OnClicked += OnItemClicked;
            }
        }
    }


    public void Refresh(List<ContentContainer> _data)
    {
        inventoryData = _data;
//        Debug.Log("REFRESHING INVENTORY! : " + _data.Count);
        if (inventoryData == null)
        {
            Debug.LogError("Set inventory data source first!");
            return;
        }


        SpawnInventory();

    }

    private void SpawnInventory()
    {
        Debug.Log("refreshing inventory!!");
        Utils.DestroyAllChildren(InventoryItemsLootParent);

        switch (Filter)
        {
            case INVENTORY_FILTER.ALL:
                {
                    SpawnItems(Utils.CONTENT_TYPE.ITEM);
                    SpawnItems(Utils.CONTENT_TYPE.EQUIP);
                    SpawnItems(Utils.CONTENT_TYPE.CURRENCY);
                    SpawnItems(Utils.CONTENT_TYPE.FOOD);
                }
                break;
            case INVENTORY_FILTER.EQUIP_ONLY:
                {
                    SpawnItems(Utils.CONTENT_TYPE.EQUIP);
                }
                break;
            case INVENTORY_FILTER.ITEMS_ONLY:
                {
                    SpawnItems(Utils.CONTENT_TYPE.ITEM);
                }
                break;
            default:
                break;
        }
    }


    private void OnItemClicked(UIContentItem _item)
    {
        base.OnUISelectableItemClicked(_item);


        if (OnContentItemClicked != null)
            OnContentItemClicked.Invoke(_item);


    }

    public void AddItemOffline(ContentContainer _item)
    {

        inventoryData.Add(_item);

        SpawnInventory();
    }

    public void RemoveItemOffline(string _uid)
    {
        for (int i = inventoryData.Count - 1; i >= 0; i--)
        {
            if (inventoryData[i].GetContent().uid == _uid)
                inventoryData.RemoveAt(i);
        }

        SpawnInventory();

    }
    public void RemoveItemOffline(ContentContainer _item)
    {

        if (inventoryData.Contains(_item))
        {
            Debug.Log("item removed from inventory : " + _item.GetContent().GetDisplayName());
            inventoryData.Remove(_item);
        }


        SpawnInventory();
    }




}
