using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using simplestmmorpg.data;
using System.Linq;

public class UIVendorGoodsSpawner : MonoBehaviour
{



    // public AccountDataSO AccountDataSO;
    //   public FirebaseCloudFunctionSO FirebaseCloudFunctionSO;
    public PrefabFactory Factory;
    public Transform GoodListParent;
    public GameObject UIVendorGoodEntryPrefab;


    private Vendor Data = null;

    public UnityAction<UIVendorGoodEntry> OnEntryClicked;
    public UnityAction<UIContentItem> OnEntryPortraitClicked;

    public void Refresh(Vendor _data)
    {
        Data = _data;

        Utils.DestroyAllChildren(GoodListParent);

        foreach (var vendorGood in Data.goods)
        {
            UIVendorGoodEntry entry = Factory.CreateGameObject<UIVendorGoodEntry>(UIVendorGoodEntryPrefab, GoodListParent);
            entry.SetData(vendorGood, _data.id);
            entry.OnClicked += OnItemClicked;
            entry.OnPortraitClicked += OnItemPortraitClicked;
        }


    }


    private void OnItemClicked(UIVendorGoodEntry _item)
    {
        //  base.OnUISelectableItemClicked(_item);

        if (OnEntryClicked != null)
            OnEntryClicked.Invoke(_item);

    }

    private void OnItemPortraitClicked(UIContentItem _item)
    {
        if (OnEntryPortraitClicked != null)
            OnEntryPortraitClicked.Invoke(_item);
    }

    //public void AddItemOffline(ItemSimple _item)
    //{

    //    if (_item is Equip)
    //    {
    //        inventoryData.GetItemsEquip().Add((Equip)_item);
    //    }
    //    else if (_item is InventoryItemSimple)
    //        inventoryData.GetInventoryItemsSimple().Add((InventoryItemSimple)_item);

    //    SpawnInventory();
    //}

    //public void RemoveItemOffline(ItemSimple _item)
    //{

    //    if (_item is Equip)
    //    {
    //        Debug.Log("sem tuB");
    //        if (inventoryData.GetItemsEquip().Contains(_item))
    //        {
    //            Debug.Log("item removed from inventory : " + _item.displayName);
    //            inventoryData.GetItemsEquip().Remove((Equip)_item);

    //        }
    //    }
    //    else if (_item is InventoryItemSimple)
    //    {
    //        if (inventoryData.GetInventoryItemsSimple().Contains(_item))
    //        {
    //            inventoryData.GetInventoryItemsSimple().Remove((InventoryItemSimple)_item);
    //        }
    //    }

    //    SpawnInventory();
    //}




}
