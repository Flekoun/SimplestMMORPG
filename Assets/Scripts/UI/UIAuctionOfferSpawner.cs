using System.Collections;
using System.Collections.Generic;
using simplestmmorpg.data;
using UnityEngine;
using UnityEngine.Events;

public class UIAuctionOfferSpawner : UISelectableSpawner
{
    public PrefabFactory PrefabFactory;
    public Transform Parent;
    public GameObject UIEntryPrefab;
    public UnityAction<UIAuctionOfferEntry> OnEntryClicked;
    public UnityAction<UIContentItem> OnGetInfo;
    //   public ListenOnAuctionHouse ListenOnAuctionHouse;
    //  public List<UIAuctionOfferEntry> UIEntriesList = new List<UIAuctionOfferEntry>();



    //public void OnDisable()
    //{
    //    ListenOnAuctionHouse.OnNewAuctionHouseDataChanged -= Refresh;
    //}

    //public void OnEnable()
    //{
    //    ListenOnAuctionHouse.OnNewAuctionHouseDataChanged += Refresh;
    //}

    public void Refresh(List<AuctionOffer> _entries)
    {
        Utils.DestroyAllChildren(Parent);

        foreach (var item in _entries)
        {
            var entryUI = PrefabFactory.CreateGameObject<UIAuctionOfferEntry>(UIEntryPrefab, Parent);
            entryUI.SetData(item);
            entryUI.OnClicked += AuctionOfferClicked;
        //    entryUI.OnGetInfo += OnGetInfoClicked;
           // UIEntriesList.Add(vendorUI);
        }

     

    }

    //private void OnGetInfoClicked(UIInventoryItem _entry)
    //{
    //    if (OnGetInfo != null)
    //        OnGetInfo.Invoke(_entry);
    //}

    private void AuctionOfferClicked(UIAuctionOfferEntry _entry)
    {
        base.OnUISelectableItemClicked(_entry);

        if (OnEntryClicked != null)
            OnEntryClicked.Invoke(_entry);
    }

}
