using System;
using System.Collections;
using System.Collections.Generic;
using simplestmmorpg.adminToolsData;
using simplestmmorpg.data;
using UnityEngine;
using UnityEngine.Events;

public class UIVendorPanelAdmin : MonoBehaviour
{
    public FirebaseCloudFunctionSO_Admin FirebaseCloudFunctionSO_Admin;
    public AccountDataSO AccountDataSO;
    public ListenOnInternalDefinitions ListenOnInternalDefinitions;



    public PrefabFactory PrefabFactory;
    public GameObject Model;
    public Transform Parent;



    public GameObject UIVendorGoodEntryAdminPrefab;

    public string vendorId = "NOT_SET";

    public List<VendorGood> EditedVendorGoods = new List<VendorGood>();

    public void Awake()
    {
        AdminToolsManager.instance.OnInternalDefinitionChanged += Refresh;
    }

    //private void OnPoIChoosen(List<UISelectableEntry> _item)
    //{
    //    TiersPanelGO.gameObject.SetActive(true);
    //    ListenOnTiers.StopListening();
    //    PointOfInterest = _item[0].GetUid();
    //    ListenOnTiers.StartListening(ZoneId, LocationId, PointOfInterest);
    //    UIItemIdChooserPoI.Hide();
    //}

    public void SetVendorId(string _vendorId)
    {
        vendorId = _vendorId;
    }
    public void Show()
    {
        if (vendorId == "NOT_SET")
        {
            Debug.LogError("Vendor ID not set!");
            return;
        }
        Model.gameObject.SetActive(true);
        ListenOnInternalDefinitions.StartListening();

        //  Refresh();
    }

    //public void ChangePointOfInterest(string _pointOfInterest)
    //{
    //    foreach (var item in AccountDataSO.LocationData.dijkstraMap.exportMap)
    //    {
    //        Debug.Log("noda" + item.id);
    //    } 
    //}

    private void Refresh()
    {
        if (!Model.gameObject.activeSelf)
            return;

        Debug.Log("vendorId:" + vendorId);
        Debug.Log("X:" + AdminToolsManager.instance.InternalDefinition.MERCHANT.Count);
        EditedVendorGoods = AdminToolsManager.instance.InternalDefinition.MERCHANT.Find(merchant => merchant.id == vendorId).vendors[0].goods;


        Utils.DestroyAllChildren(Parent);

        foreach (var vendorGood in EditedVendorGoods)
        {
            var good = PrefabFactory.CreateGameObject<UIVendorGoodEntryAdmin>(UIVendorGoodEntryAdminPrefab, Parent);
            good.SetData(vendorGood);
            //UIItem.Setup(item);
            //UIItem.OnRemoveTierClicked += OnRemoveTier;
        }


    }



    public void Hide()
    {
        Model.gameObject.SetActive(false);
        ListenOnInternalDefinitions.StopListening();
    }

    public void SaveClicked()
    {
        FirebaseCloudFunctionSO_Admin.SaveInternalDefinitionsMapGenerator(AdminToolsManager.instance.InternalDefinition);
        //     FirebaseCloudFunctionSO_Admin.SaveTiers(AdminToolsManager.instance.ServerData, ZoneId, LocationId, PointOfInterest);
    }
    public void AdddGoodClicked()
    {
        var newVendorGood = new VendorGood();
        newVendorGood.content = null;
        newVendorGood.contentGenerated = null;
        newVendorGood.contentRandomEquip = null;
        newVendorGood.currencyType = Utils.CURRENCY_ID.GOLD;
        newVendorGood.sellPrice = 1;
        newVendorGood.stockPerCharacter = -1;
        newVendorGood.stockTotal = -1;
        newVendorGood.stockTotalLeft = -1;
        newVendorGood.uid = System.Guid.NewGuid().ToString();
        EditedVendorGoods.Add(newVendorGood);
        //AdminToolsManager.instance.ServerData.tiers.Add(newVendorGood);
        Refresh();
    }


    private void OnRemoveTier(UITier _item)
    {
        AdminToolsManager.instance.ServerData.tiers.Remove(_item.Data);
        Refresh();
    }

    private void OnRemoveRarePerk(UIPerkOfferAdmin _item)
    {
        // AdminToolsManager.instance.ServerData.perkOffersRare.Remove(_item.Data);
        Refresh();
    }

}
