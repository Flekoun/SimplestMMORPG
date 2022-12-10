using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using simplestmmorpg.data;

public class UIAuctionHouse : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    public FirebaseCloudFunctionSO FirebaseCloudFunctionSO;
    public UIInventory UIInventoryPlayer;
    public ListenOnAuctionHouse ListenOnAuctionHouse;
    public TMP_InputField BidPriceInput;
    public UIContentContainerDetail UIItemDetail;
    public GameObject Model;

    public Button BuyoutButton;

    public UIAuctionOfferSpawner UIAuctionOfferSpawner;

    public void Awake()
    {
        UIInventoryPlayer.OnContentItemClicked += OnPlayerInventoryItemClicked;
        UIAuctionOfferSpawner.OnEntryClicked += OnAuctionOfferEntryClicked;
      //  UIAuctionOfferSpawner.OnGetInfo += OnGetInfoAboutOfferItemClicked;
    }
    public void OnEnable()
    {
        ListenOnAuctionHouse.OnAllAuctionsChanged += OnAllAuctionsChanged;
        AccountDataSO.OnCharacterDataChanged += Refresh;
    }

    public void OnDiable()
    {
        ListenOnAuctionHouse.OnAllAuctionsChanged -= OnAllAuctionsChanged;
        AccountDataSO.OnCharacterDataChanged -= Refresh;
    }

    private void OnAllAuctionsChanged(List<AuctionOffer> _offers)
    {
        UIAuctionOfferSpawner.Refresh(_offers);
    }

    public void OnPlayerInventoryItemClicked(UIContentItem _item)
    {

    }

    //public void OnGetInfoAboutOfferItemClicked(UIInventoryItem _item)
    //{
    //    Debug.Log("clciked");
    //    UIItemDetail.Show(_item.GetData());
    //}

    public void OnAuctionOfferEntryClicked(UIAuctionOfferEntry _item)
    {
        UIItemDetail.Show(_item.Data.content.GetContent());

        BidPriceInput.SetTextWithoutNotify(_item.Data.nextBidPrice.ToString());

        BuyoutButton.gameObject.SetActive(_item.Data.buyoutPrice > 0);
    }

    // Start is called before the first frame update
    public void Show()
    {
        ListenOnAuctionHouse.StartListeningOnAllAuctions();
        Model.SetActive(true);
        Refresh();
    }

    // Update is called once per frame
    public void Hide()
    {
        ListenOnAuctionHouse.StopListeningOnAllAuctions();
        Model.SetActive(false);
    }

    public void Refresh()
    {
        UIInventoryPlayer.Refresh(AccountDataSO.CharacterData.inventory.content);
    }


    public void BidOnAuctionOffer()
    {
        if (UIAuctionOfferSpawner.IsAnyItemSelected())
        {
            var choosenOffer = UIAuctionOfferSpawner.GetSelectedEntry();
            int bitPrice = int.Parse(BidPriceInput.text);

            FirebaseCloudFunctionSO.BidContentOnAuctionHouse(choosenOffer.GetUid(), bitPrice);
        }
    }

    public void BuyoutAuctionOffer()
    {
        if (UIAuctionOfferSpawner.IsAnyItemSelected())
        {
            var choosenOffer = UIAuctionOfferSpawner.GetSelectedEntry();
            FirebaseCloudFunctionSO.BuyoutContentOnAuctionHouse(choosenOffer.GetUid());
        }
    }



}
