using System.Collections;
using System.Collections.Generic;
using simplestmmorpg.data;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//using simplestmmorpg.data;
using UnityEngine.Events;
using simplestmmorpg.adminToolsData;

public class UIVendorGoodEntryAdmin : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    public PrefabFactory PrefabFactory;
    public Transform Parent;
    //   public FirebaseCloudFunctionSO FirebaseCloudFunctionSO;
    // public TextMeshProUGUI SellPriceText;
    public TextMeshProUGUI DisplayNameText;
    //  public UIContentItem UIInventoryItem;

    public TMP_InputField PriceInput;
    public TMP_Dropdown CurrencyTypeDropbox;

    public TMP_InputField StockTotalInput;
    public TMP_InputField StockPerCharacterInput;


    public GameObject UIRandomEquipAdminPrefab;
    public GameObject UIItemWithIdAdminPrefab;

    //  public TextMeshProUGUI StockLeftText;
    public GameObject SoldOutGO;
    public Button Button;
    public VendorGood Data;
    public UnityAction<UIVendorGoodEntryAdmin> OnClicked;
    //public UnityAction<UIContentItem> OnPortraitClicked;

    //public void Awake()
    //{
    //    UIInventoryItem.OnClicked += OnItemPortraitClicked;
    //}

    //public void OnDestroy()
    //{
    //    UIInventoryItem.OnClicked -= OnItemPortraitClicked;
    //}

    public void ChooseItemClicked()
    {
        AdminToolsManager.instance.ShowItemChooserItems((result) =>
        {
            //  var itemChoosen = PrefabFactory.CreateGameObject<UIContentItem>(UIInventoryItemPrefab, GoodParent);

            if (result[0].GetUid() == "EQUIP")
            {
                RandomEquip newItem = new RandomEquip();
                newItem.rarity = Utils.RARITY.COMMON;
                newItem.mLevel = 1;
                newItem.equipSlotId = Utils.EQUIP_SLOT_ID.ANY;

                Data.contentRandomEquip = newItem;
            }
            else
            {
                ItemIdWithAmount newItem = new ItemIdWithAmount(); // nema tu by ItemIdWithAmountAdmin????
                newItem.amount = 1;
                newItem.itemId = result[0].GetUid();

                Data.contentGenerated = newItem;

            }

            SetData(this.Data);

        });


    }

    public void OnPriceChanged(string _price)
    {
        PriceInput.text = _price;
        Data.sellPrice = int.Parse(_price);
    }

    public void OnStockTotalChanged(string _value)
    {
        StockTotalInput.text = _value;
        Data.stockTotal = int.Parse(_value);
        Data.stockTotalLeft = Data.stockTotal;
    }

    public void OnStockPerCharacterChanged(string _value)
    {
        StockPerCharacterInput.text = _value;
        Data.stockPerCharacter = int.Parse(_value);
    }


    public void OnCurrencyDropDownValueChanged(int _value)
    {
        Data.currencyType = Utils.GetCurrencyIdByIndex(_value);
    }

    public void SetData(VendorGood _data)
    {
        //   tady musim zohlednit ze vendor good muze byt jak content ktery je jasne dany anebo generated content ( ItemDropDefinition)
        Data = _data;




        PriceInput.text = Data.sellPrice.ToString();
        StockPerCharacterInput.text = Data.stockPerCharacter.ToString();
        StockTotalInput.text = Data.stockTotal.ToString();

        //if (Data.content != null)
        //{
        //    DisplayNameText.SetText(Utils.ReplacePlaceholdersInTextWithDescriptionFromMetadata(Data.content.GetContent().GetDisplayName()));
        //    UIInventoryItem.SetData(Data.content.GetContent(), true);
        //}
        Utils.DestroyAllChildren(Parent);
        if (Data.contentGenerated != null)
        {
            DisplayNameText.SetText(Utils.ReplacePlaceholdersInTextWithDescriptionFromMetadata(Data.contentGenerated.GetDisplayName()));

            var item = PrefabFactory.CreateGameObject<UIItemIdWithAmountAdmin>(UIItemWithIdAdminPrefab, Parent);
            item.SetData(Data.contentGenerated);
        }
        else if (Data.contentRandomEquip != null)
        {
            DisplayNameText.SetText(Utils.ReplacePlaceholdersInTextWithDescriptionFromMetadata(Data.contentRandomEquip.GetDisplayName()));

            var item = PrefabFactory.CreateGameObject<UIRandomEquipAdmin>(UIRandomEquipAdminPrefab, Parent);
            item.SetData(Data.contentRandomEquip);
        }

        //  int myStockLeft = (_data.stockPerCharacter - AccountDataSO.CharacterData.GetVendorGoodsPurchased(_vendorId, _data.uid));




        //  StockLeftText.SetText(Data.stockTotalLeft.ToString());




        CurrencyTypeDropbox.value = Utils.GetCurrencyIdIndex(Data.currencyType.ToString());
    }

    public void Clicked()
    {
        OnClicked.Invoke(this);
    }

    //public override string GetUid()
    //{
    //    return Data.uid;
    //}


}
