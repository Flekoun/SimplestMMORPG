using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using simplestmmorpg.adminToolsData;

public class UIDropTable : MonoBehaviour
{
    public PrefabFactory PrefabFactory;
    public GameObject UIDropTableItemPrefab;
    public UIItemIdChooser UIItemIdChooser;
    public TextMeshProUGUI DropCountText;
    public TMP_InputField DropCountMaxInput;
    public TMP_InputField DropCountMinInput;
    public Transform Parent;

    private List<UIDropTableItem> List = new List<UIDropTableItem>();
    private DropTable Data;


    public void Awake()
    {
        UIItemIdChooser.OnItemsToAddSelected += OnItemsToAddSelected;
    }

    private void OnItemsToAddSelected(List<UISelectableEntry> _itemsToAdd)
    {
        foreach (var itemToAdd in _itemsToAdd)
        {

            DropTableItem newItem = new DropTableItem();
            newItem.amount = 1;
            newItem.chanceToSpawn = 0;
            newItem.itemId = itemToAdd.GetUid();
            if (newItem.itemId == "EQUIP")
                newItem.rarity = "COMMON";
            else
                newItem.rarity = null;

            Data.dropTableItems.Add(newItem);
       
        }
        Refresh();
    }


    private void Refresh()
    {
        Utils.DestroyAllChildren(Parent);
        List.Clear();

        DropCountMaxInput.text = Data.dropCountMax.ToString();
        DropCountMinInput.text = Data.dropCountMin.ToString();

        //   DropCountText.SetText(_item.dropCountMin.ToString() + " - " + _item.dropCountMax.ToString());
        foreach (var dropTableItem in Data.dropTableItems)
        {
            var UIItem = PrefabFactory.CreateGameObject<UIDropTableItem>(UIDropTableItemPrefab, Parent);
            UIItem.Setup(dropTableItem);
            UIItem.OnRemoveClicked += OnRemoveItemClicked;
            List.Add(UIItem);
        }
    }


    // Start is called before the first frame update
    public void Setup(DropTable _item)
    {
        Data = _item;
        Refresh();

    }

    public void ShowChooser()
    {
        UIItemIdChooser.Show();
    }

    public void Save()
    {

        Data.dropCountMax = int.Parse(DropCountMaxInput.text);
        Data.dropCountMin = int.Parse(DropCountMinInput.text);
        foreach (var item in List)
        {
            item.Save();
        }
    }

    private void OnRemoveItemClicked(UIDropTableItem _item)
    {
        Data.dropTableItems.Remove(_item.Data);
        Refresh();
    }
}
