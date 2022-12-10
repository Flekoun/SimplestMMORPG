using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using simplestmmorpg.data;

public class UIAddOfferToAuctionHouse : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    public FirebaseCloudFunctionSO FirebaseCloudFunctionSO;
    public UIInventory UIInventoryPlayer;
    public UIContentItem UIInventoryItemHighlight;
    public ListenOnAuctionHouse ListenOnAuctionHouse;
    public TMP_InputField BidPriceInput;
    public TMP_InputField BuyoutPriceInput;
    public GameObject Model;
    public UIAuctionOfferSpawner UIAuctionOfferSpawner;

    private List<AuctionOffer> OffersIPutOnAH = new List<AuctionOffer>();
    private List<AuctionOffer> OffersIBidOnAH = new List<AuctionOffer>();
    private List<AuctionOffer> CombinedOffers = new List<AuctionOffer>();


    public void Awake()
    {
        UIInventoryPlayer.OnContentItemClicked += OnContentItemClicked;
    }
    public void OnEnable()
    {
        ListenOnAuctionHouse.OnAllOffersIPutOnAuctionChanged += OnAllOffersIPutOnAuctionChanged;
        ListenOnAuctionHouse.OnnAllOffersIBiddedOnChanged += OnnAllOffersIBiddedOnChanged;
        AccountDataSO.OnCharacterDataChanged += Refresh;
    }

    public void OnDiable()
    {
        ListenOnAuctionHouse.OnAllOffersIPutOnAuctionChanged -= OnAllOffersIPutOnAuctionChanged;
        ListenOnAuctionHouse.OnnAllOffersIBiddedOnChanged -= OnnAllOffersIBiddedOnChanged;
        AccountDataSO.OnCharacterDataChanged -= Refresh;
    }

    private void OnAllOffersIPutOnAuctionChanged(List<AuctionOffer> _offers)
    {
        Debug.Log("_offers put on autcion : " + _offers.Count);
        CombinedOffers.Clear();
        OffersIPutOnAH.Clear();
        OffersIPutOnAH.AddRange(_offers);
        CombinedOffers.AddRange(OffersIPutOnAH);
        CombinedOffers.AddRange(OffersIBidOnAH);
        UIAuctionOfferSpawner.Refresh(CombinedOffers);
    }

    private void OnnAllOffersIBiddedOnChanged(List<AuctionOffer> _offers)
    {
        Debug.Log("_offers bidded on autcion : " + _offers.Count);
        CombinedOffers.Clear();
        OffersIBidOnAH.Clear();
        OffersIBidOnAH.AddRange(_offers);
        CombinedOffers.AddRange(OffersIPutOnAH);
        CombinedOffers.AddRange(OffersIBidOnAH);
        UIAuctionOfferSpawner.Refresh(CombinedOffers);
    }

    public void OnContentItemClicked(UIContentItem _item)
    {
        UIInventoryItemHighlight.SetData(_item.GetData());
    }

    public void OnAuctionOfferEntryClicked(UIAuctionOfferEntry _item)
    {

    }

    // Start is called before the first frame update
    public void Show()
    {
        ListenOnAuctionHouse.StartListeningOnAllOffersIPutOnAuction();
        ListenOnAuctionHouse.StartListeningOnAllOffersIBiddedOn();
        Model.SetActive(true);
        Refresh();
    }

    // Update is called once per frame
    public void Hide()
    {
        ListenOnAuctionHouse.StopListeningOnAllOffersIPutOnAuction();
        ListenOnAuctionHouse.StopListeningOnAllOffersIBiddedOn();
        Model.SetActive(false);
    }

    public void Refresh()
    {
        UIInventoryPlayer.Refresh(AccountDataSO.CharacterData.inventory.content);
    }

    public void AddToAuction()
    {
        if (UIInventoryItemHighlight.GetData() != null)
        {
            int buyoutPrice = 0;
            int bitPrice = 0;

            var choosenItem = (UIInventoryPlayer.GetSelectedEntry() as UIContentItem).GetData();


            if (BuyoutPriceInput.text != "")
                buyoutPrice = int.Parse(BuyoutPriceInput.text);

            if (BidPriceInput.text != "")
                bitPrice = int.Parse(BidPriceInput.text);
            else
            {
                Debug.Log("Enter Bid Price first!");
                return;
            }


            FirebaseCloudFunctionSO.PutContentOnAuctionHouse(choosenItem.GetContentType(), choosenItem.uid, 0, buyoutPrice, bitPrice);
        }
    }




}
