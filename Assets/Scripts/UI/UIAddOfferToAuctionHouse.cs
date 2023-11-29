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
    public UIGoldAmountInput BidPriceInput;
    public UIGoldAmountInput BuyoutPriceInput;
    public TMP_InputField AmountInput;
    public GameObject ChooseItemForAuctionPanelGO;
    public GameObject ChooseItemToolsGO;
    public GameObject AddtItemToAuctionToolsGO;

    public GameObject Model;
    public UIAuctionOfferSpawner UIAuctionOfferSpawner;

    private List<AuctionOffer> OffersIPutOnAH = new List<AuctionOffer>();
    private List<AuctionOffer> OffersIBidOnAH = new List<AuctionOffer>();
    private List<AuctionOffer> CombinedOffers = new List<AuctionOffer>();


    public void Awake()
    {
        UIInventoryPlayer.OnContentItemClicked += OnContentItemClicked;
        UIInventoryItemHighlight.OnClicked += OnUIInventoryItemHighlightClicked;
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

    private void OnUIInventoryItemHighlightClicked(UIContentItem _item)
    {
        ShowAddToAuctionChooser();
    }



    public void ShowAddToAuctionChooser()
    {
        UIInventoryPlayer.ClearItemsSelected();
        ChooseItemForAuctionPanelGO.gameObject.SetActive(true);

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
        ChooseItemForAuctionPanelGO.SetActive(false);

        AmountInput.gameObject.SetActive(_item.GetData().stackSize > 1);


        AmountInput.SetTextWithoutNotify(_item.GetData().amount.ToString());

        ChooseItemToolsGO.SetActive(false);
        AddtItemToAuctionToolsGO.SetActive(true);
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

        ChooseItemToolsGO.SetActive(true);
        AddtItemToAuctionToolsGO.SetActive(false);


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
            //int buyoutPrice = 0;
            //int bitPrice = 0;
            int amount = 1;

            var choosenItem = (UIInventoryPlayer.GetSelectedEntry() as UIContentItem).GetData();

            if (AmountInput.text != "")
            {
                amount = int.Parse(AmountInput.text);

                if (amount < 1)
                {
                    UIManager.instance.ImportantMessage.ShowMesssage("Enter amount!");
                    return;
                }
            }
            else
            {
                UIManager.instance.ImportantMessage.ShowMesssage("Enter amount!");
                return;
            }

            //if (BuyoutPriceInput.GetAmount()>0)
            //    buyoutPrice = int.Parse(BuyoutPriceInput.text);


            if (BidPriceInput.GetAmount() <= 0)
            {
                Debug.Log("Enter Bid Price first!");
                UIManager.instance.ImportantMessage.ShowMesssage("Enter Bid price!");
                return;
            }

            if (BuyoutPriceInput.GetAmount() <= BidPriceInput.GetAmount() && BuyoutPriceInput.GetAmount() > 0)
            {
                UIManager.instance.ImportantMessage.ShowMesssage("Buyout price must be higher than Bid price");
                return;
            }


            FirebaseCloudFunctionSO.PutContentOnAuctionHouse(choosenItem.uid, BuyoutPriceInput.GetAmount(), BidPriceInput.GetAmount(), amount);

            ChooseItemToolsGO.SetActive(true);
            AddtItemToAuctionToolsGO.SetActive(false);


        }
    }

    //public void IncreaseAmountClicked()
    //{

    //}


    //public void DecreaseAmountClicked()
    //{

    //}

}
