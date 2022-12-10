using System.Collections;
using System.Collections.Generic;
using simplestmmorpg.data;
using UnityEngine;
using static UIInventory;

public class UISelectableSpawner : MonoBehaviour
{

    [Header("Selectable")]
    //pokud to chapu spravne vzajemne se vylucujou? jedno je vyber vice itemu druhy je "toggle" select jednoho
    public bool UseItemSelectFeature = false;
    public bool MultiSelect = true;

    public List<UISelectableEntry> SelectedItems = new List<UISelectableEntry>();

    public List<string> GetSelectedItemsUids()
    {
        List<string> uids = new List<string>();
        foreach (var item in SelectedItems)
        {
            uids.Add(item.GetUid());
        }

        return uids;
    }

    public void ClearItemsSelected()
    {
        foreach (var item in SelectedItems)
        {
            if (item != null)
                item.SetSelected(false);
        }

        SelectedItems.Clear();
    }

    public bool IsAnyItemSelected()
    {
        for (int i = SelectedItems.Count - 1; i >= 0; i--)
            if (SelectedItems[i] == null)
                SelectedItems.RemoveAt(i);


        return (SelectedItems.Count > 0);
    }

    public UISelectableEntry GetSelectedEntry()
    {
        for (int i = SelectedItems.Count - 1; i >= 0; i--)
            if (SelectedItems[i] == null)
                SelectedItems.RemoveAt(i);

        if (IsAnyItemSelected())
            return SelectedItems[0];
        else
            return null;
    }

    //TODO: Tohle se nesmi zapomenout volat kdyz volam kliknuti na itemy.....jinak to cele nefunguje
    protected void OnUISelectableItemClicked(UISelectableEntry _item)
    {
        if (UseItemSelectFeature)
        {
            bool itemSelected = _item.IsSelected;

            if (!MultiSelect)
                ClearItemsSelected();

            _item.ToggleSelected();

            if (_item.IsSelected)
            {
                if (!SelectedItems.Contains(_item))
                    SelectedItems.Add(_item);
            }
            else
            {
                SelectedItems.Remove(_item);
            }

            //hack - to unselect if we clicked on already selected item
            if (!MultiSelect && itemSelected)
            {
                _item.ToggleSelected();
                SelectedItems.Remove(_item);
            }
            //hack - to unselect if we clicked on already selected item
        }



    }

}
