using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using simplestmmorpg.data;
using System.Linq;
using static UnityEditor.Progress;

public class UIInventory : UISelectableSpawner
{

    public enum INVENTORY_FILTER
    {
        ALL,
        EQUIP_ONLY,
        ITEMS_ONLY,
        CURRENCY_ONLY,
        FOOD_ONLY,
        FOOD_SUPPLY_ONLY,

    }


    public INVENTORY_FILTER Filter;
    public bool ShowTooltip = false;
    public AccountDataSO AccountDataSO;
    public FirebaseCloudFunctionSO FirebaseCloudFunctionSO;
    public PrefabFactory Factory;
    public Transform InventoryItemsLootParent;
    public GameObject UIInventoryItemPrefab;

    private List<IContentDisplayable> inventoryData = null;

    public UnityAction<UIContentItem> OnContentItemClicked;

    public bool HasAnyItems()
    {
        if (inventoryData.Count == 0)
            return false;

        bool anyItemFound = false;

        foreach (var item in inventoryData)
        {
            if (item.amount > 0)
            {
                anyItemFound = true;
                break;
            }
        }
        return anyItemFound;
    }

    public int GetValueOfAllItems()
    {
        int result = 0;

        foreach (var item in inventoryData)
        {
            result += item.sellPrice * item.amount;
        }
        return result;
    }


    public int GetFoodSupplyValueOfAllItems()
    {
        int result = 0;

        foreach (var item in inventoryData)
        {
            if (item.amount > 0)
                if (item.contentType == Utils.CONTENT_TYPE.FOOD_SUPPLY)
                    result += Mathf.FloorToInt(item.customData.integers[0]) * item.amount;
        }
        return result;
    }

    public List<string> GetDescriptionsOfAllItems()
    {
        List<string> result = new List<string>();

        foreach (var item in inventoryData)
        {
            for (int i = 0; i < item.amount; i++)
            {
                result.Add(item.GetDescription());
                Debug.Log("adding! :" + item.GetDescription());
            }


        }
        Debug.Log("result count! :" + result.Count);
        return result;
    }

    public List<string> GetAllItemUids()
    {
        List<string> result = new List<string>();

        foreach (var item in inventoryData)
        {
            if (item.amount > 0)
                result.Add(item.uid);
        }
        return result;
    }

    public List<int> GetAllItemAmounts()
    {
        List<int> result = new List<int>();

        foreach (var item in inventoryData)
        {
            if (item.amount > 0)
                result.Add(item.amount);
        }
        return result;
    }

    private void SpawnItems(string _contentType)
    {

        foreach (var content in inventoryData)
        {
            if (content.contentType == _contentType && content.amount > 0)
            {
                UIContentItem uiInventoryItem = Factory.CreateGameObject<UIContentItem>(UIInventoryItemPrefab, InventoryItemsLootParent);
                uiInventoryItem.SetData(content, ShowTooltip);
                uiInventoryItem.OnClicked += OnItemClicked;
            }
        }
    }


    public void Refresh(List<IContentDisplayable> _data)
    {
        inventoryData = _data;
        if (inventoryData == null)
        {
            Debug.LogError("Set inventory data source first!");
            return;
        }

        SpawnInventory();

    }

    public void Refresh(List<ContentContainer> _data)
    {
        inventoryData = new List<IContentDisplayable>();
        foreach (var item in _data)
        {
            inventoryData.Add(item.GetContent());

        }
        if (inventoryData == null)
        {
            Debug.LogError("Set inventory data source first!");
            return;
        }

        SpawnInventory();

    }

    private void SpawnInventory()
    {
        // Debug.Log("refreshing inventory!!");
        Utils.DestroyAllChildren(InventoryItemsLootParent);

        switch (Filter)
        {
            case INVENTORY_FILTER.ALL:
                {
                    SpawnItems(Utils.CONTENT_TYPE.ITEM);
                    SpawnItems(Utils.CONTENT_TYPE.EQUIP);
                    SpawnItems(Utils.CONTENT_TYPE.CURRENCY);
                    SpawnItems(Utils.CONTENT_TYPE.FOOD);
                    SpawnItems(Utils.CONTENT_TYPE.RECIPE);
                    SpawnItems(Utils.CONTENT_TYPE.GENERATED);
                    SpawnItems(Utils.CONTENT_TYPE.FOOD_SUPPLY);
                    SpawnItems(Utils.CONTENT_TYPE.RANDOM_EQUIP);
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
            case INVENTORY_FILTER.FOOD_SUPPLY_ONLY:
                {
                    SpawnItems(Utils.CONTENT_TYPE.FOOD_SUPPLY);
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



    public void AddItemOffline(IContentDisplayable _item, bool _addOnlyOneFromAmount = false)
    {
        //  if (_item.contentType != Utils.CONTENT_TYPE.EQUIP)
        //{




        bool contentAlreadyPresent = false;
        foreach (var itemInInventory in inventoryData)
        {
            //if (_item.contentType != Utils.CONTENT_TYPE.EQUIP)
            //{
            //    if (itemInInventory.itemId == _item.itemId)
            //    {
            //        contentAlreadyPresent = true;
            //        if (_addOnlyOneFromAmount)
            //            itemInInventory.amount += 1;
            //        else
            //            itemInInventory.amount += _item.amount;
            //        break;
            //    }
            //}
            //else
            //{
            if (itemInInventory.uid == _item.uid)
            {
                contentAlreadyPresent = true;
                if (_addOnlyOneFromAmount)
                    itemInInventory.amount += 1;
                else
                    itemInInventory.amount += _item.amount;
                break;
            }
            //  }
        }
        if (!contentAlreadyPresent)
        {
            Debug.Log("v inventory jeste :" + _item.GetDisplayName() + " neni, pridam ho");
            var copy = _item.CopySelf();
            if (_addOnlyOneFromAmount)
                copy.amount = 1;
            else
            {
                copy.amount = _item.amount;
                Debug.Log("copy.amount: " + copy.amount);
            }
            //ContentContainer dummyContent = new ContentContainer();
            //dummyContent.SetContent(copy);
            inventoryData.Add(copy);
        }
        //}
        //else
        //{
        //    Debug.Log("Ok pridavam equip");
        //    inventoryData.Add(_item); //equip pridamm tak jak je
        //}

        SpawnInventory();
    }

    public void RemoveItemOffline(string _uid, bool _remveOnlyOneFromAmount = false)
    {

        for (int i = inventoryData.Count - 1; i >= 0; i--)
        {

            if (inventoryData[i].uid == _uid)
            {

                if (inventoryData[i].amount == 0)
                    return;

                //  if (inventoryData[i].contentType != Utils.CONTENT_TYPE.EQUIP)
                // {
                if (_remveOnlyOneFromAmount)
                    inventoryData[i].amount--;
                else
                    inventoryData[i].amount = 0;

                //if (inventoryData[i].amount == 0)
                //{
                //    inventoryData.RemoveAt(i);
                //}
                //}
                //else
                //    inventoryData[i].amount--;// inventoryData.RemoveAt(i); //equip hned pryc cely
            }
        }

        SpawnInventory();

    }
    public void RemoveItemOffline(IContentDisplayable _item, bool _remveOnlyOneFromAmount = false)
    {

        RemoveItemOffline(_item.uid, _remveOnlyOneFromAmount);
    }

    public void RemoveAllItemsOffline()
    {
        inventoryData = new List<IContentDisplayable>();
        SpawnInventory();
    }




}

