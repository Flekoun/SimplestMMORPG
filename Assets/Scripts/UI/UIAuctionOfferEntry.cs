using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using simplestmmorpg.data;
using UnityEngine.Events;

public class UIAuctionOfferEntry : UISelectableEntry
{
    public AccountDataSO AccountDataSO;
    public FirebaseCloudFunctionSO FirebaseCloudFunctionSO;
    public TextMeshProUGUI SellerNameText;
    public TextMeshProUGUI HighestBidderNameText;
    public TextMeshProUGUI ExpireDateText;
    public TextMeshProUGUI BidPriceText;
    public TextMeshProUGUI BuyoutPriceText;
    public UIContentItem UIInventoryItem;

    public GameObject BuyoutPriceGO;

    public GameObject ItemSold_GO;
    public GameObject ItemNotSold_GO;
    public GameObject ItemBidWon_GO;

    public AuctionOffer Data;
    public UnityAction<UIAuctionOfferEntry> OnClicked;
    public UnityAction<UIContentItem> OnGetInfo;

    //public void Awake()
    //{
    //    UIInventoryItem.OnClicked += OnGetInfoClicked;
    //}

    //public void OnDestroy()
    //{
    //    UIInventoryItem.OnClicked -= OnGetInfoClicked;
    //}

    //private void OnGetInfoClicked(UIInventoryItem _item)
    //{
    //    if (OnGetInfo != null)
    //        OnGetInfo.Invoke(_item);
    //}

    public void SetData(AuctionOffer _data)
    {
        Data = _data;
        SellerNameText.SetText(Data.sellerDisplayName);
        if (Data.highestBidderDiplayName != "")
        {
            HighestBidderNameText.SetText(Data.highestBidderDiplayName);
            HighestBidderNameText.color = Color.yellow;
        }
        else
        {
            HighestBidderNameText.SetText("No Bid");
            HighestBidderNameText.color = Color.red;

        }

        ExpireDateText.SetText(Data.GetTimeLeft());
        BidPriceText.SetText(Data.nextBidPrice.ToString() );

        BuyoutPriceGO.SetActive(Data.hasBuyoutPrice);
        if (Data.hasBuyoutPrice)
            BuyoutPriceText.SetText(Data.buyoutPrice.ToString());
        else
            BuyoutPriceText.gameObject.SetActive(false);


        UIInventoryItem.SetData(Data.content.GetContent());

        if (Data.IsExpired())
        {
            if (Data.sellerUid == AccountDataSO.CharacterData.uid)
            {
                if (Data.highestBidderDiplayName == "")
                {
                    ItemNotSold_GO.gameObject.SetActive(true);
                }
                else
                    ItemSold_GO.gameObject.SetActive(true);
            }

            if (Data.highestBidderUid == AccountDataSO.CharacterData.uid)
            {
                ItemBidWon_GO.gameObject.SetActive(true);
            }


        }


    }

    public void Clicked()
    {
        OnClicked.Invoke(this);
    }

    public override string GetUid()
    {
        return Data.uid;
    }

    public void ItemSold_CollectGold()
    {
        FirebaseCloudFunctionSO.CollectGoldForMySoldContentOnAuctionHouse(Data.uid);
    }

    public void ItemNotSold_CollectContent()
    {
        FirebaseCloudFunctionSO.CollectMyUnsoldContentOnAuctionHouse(Data.uid);
    }

    public void ItemBidWon_CollectContent()
    {
        FirebaseCloudFunctionSO.CollectContentForMyWonAuctionOnAuctionHouse(Data.uid);
    }
}
