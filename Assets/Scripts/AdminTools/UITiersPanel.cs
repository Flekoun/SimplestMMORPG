using System;
using System.Collections;
using System.Collections.Generic;
using simplestmmorpg.adminToolsData;
using simplestmmorpg.data;
using UnityEngine;
using UnityEngine.Events;

public class UITiersPanel : MonoBehaviour
{
    public FirebaseCloudFunctionSO_Admin FirebaseCloudFunctionSO_Admin;
    public AccountDataSO AccountDataSO;

    public PrefabFactory PrefabFactory;
    public GameObject Model;
    public Transform Parent;

    public GameObject TiersPanelGO;
    public UIItemIdChooser UIItemIdChooserPoI;

    public GameObject UIPerkOfferPrefab;
    public GameObject UITierPrefab;
    public ListenOnLivePointOfInterestTiers ListenOnTiers;

    public string ZoneId = "DUNOTAR";
    public string LocationId = "VALLEY_OF_TRIALS";
    public string PointOfInterest = "POI_A1";

    //private List<UITier> List = new List<UITier>();


    public void Awake()
    {
        AdminToolsManager.instance.OnTiersChanged += Refresh;
        UIItemIdChooserPoI.OnItemsToAddSelected += OnPoIChoosen;
    }

    private void OnPoIChoosen(List<UISelectableEntry> _item)
    {
        TiersPanelGO.gameObject.SetActive(true);
        ListenOnTiers.StopListening();
        PointOfInterest = _item[0].GetUid();
        ListenOnTiers.StartListening(ZoneId, LocationId, PointOfInterest);
        UIItemIdChooserPoI.Hide();
    }

    public void Show()
    {
        ListenOnTiers.StopListening();
        Model.gameObject.SetActive(true);

        TiersPanelGO.gameObject.SetActive(false);
        UIItemIdChooserPoI.Show();
        //ListenOnTiers.StartListening(ZoneId, LocationId, PointOfInterest);
        //ChangePointOfInterest("X");
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
        Utils.DestroyAllChildren(Parent);
        // List.Clear();

        foreach (var item in AdminToolsManager.instance.ServerData.tiers)
        {
            var UIItem = PrefabFactory.CreateGameObject<UITier>(UITierPrefab, Parent);

            UIItem.Setup(item);
            UIItem.OnRemoveTierClicked += OnRemoveTier;
            // List.Add(UIItem);
        }

        //foreach (var item in AdminToolsManager.instance.ServerData.perkOffersRare)
        //{
        //    var UIItem = PrefabFactory.CreateGameObject<UIPerkOfferAdmin>(UIPerkOfferPrefab, Parent);

        //    UIItem.Setup(item, true);
        //    UIItem.OnRemoveThisPerk += OnRemoveRarePerk;
        //    // List.Add(UIItem);
        //}
    }



    public void Hide()
    {
        Model.gameObject.SetActive(false);
        ListenOnTiers.StopListening();
    }

    public void SaveClicked()
    {
        //foreach (var item in List)
        //    item.Save();

        FirebaseCloudFunctionSO_Admin.SaveTiers(AdminToolsManager.instance.ServerData, ZoneId, LocationId, PointOfInterest);

    }
    public void AdddTierClicked()
    {
        var newTier = new TierMonstersDefinition();
        newTier.enemies = new List<string>();
        newTier.entryTimePrice = 0;
        newTier.perkOffers = new List<PerkOfferDefinitionAdmin>();
        AdminToolsManager.instance.ServerData.tiers.Add(newTier);
        Refresh();
    }

    public void AdddRarePerkClicked()
    {
        var perk = PerkOfferDefinitionAdmin.FactoryNewPerk();
        perk.stockLeft = 10;
        perk.rarity = Utils.RARITY.UNCOMMON;
        //   AdminToolsManager.instance.ServerData.perkOffersRare.Add(perk);
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
