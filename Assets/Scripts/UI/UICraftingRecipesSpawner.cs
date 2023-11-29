using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;
using simplestmmorpg.data;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.EventSystems.EventTrigger;

public class UICraftingRecipesSpawner : UISelectableSpawner
{
    public AccountDataSO AccountDataSO;
    public PrefabFactory PrefabFactory;
    public Transform Parent;
    public GameObject UICraftingRecipeEntryPrefab;
    public UnityAction<UICraftingRecipeEntry> OnRecipeClicked;
    private List<UICraftingRecipeEntry> UIEntriesList = new List<UICraftingRecipeEntry>();

    private string lastChoosenRecipeUid = "";
    public UICraftingRecipeEntry LastChoosenRecipe = null;
    public void Awake()
    {
    }

    public void OnDisable()
    {

    }

    public void OnEnable()
    {

    }


    public void Refresh()
    {
        Utils.DestroyAllChildren(Parent);//, 1);
        UIEntriesList.Clear();

        foreach (var recipeId in AccountDataSO.CharacterData.craftingRecipesUnlocked)
        {
            var recipe = AccountDataSO.CraftingRecipesMetadata.GetRecipeById(recipeId);
            var entry = PrefabFactory.CreateGameObject<UICraftingRecipeEntry>(UICraftingRecipeEntryPrefab, Parent);
            UIEntriesList.Add(entry);
            entry.OnClicked += OnEntryClicked;
            entry.SetData(recipe);

            //snazim se tu vybrat posledni vybrany recipe...kdyz refreshuju data at to nevybira furt znova ten prvni ale vrati se to na posledni vybrany
            if (lastChoosenRecipeUid != "")
                if (entry.GetUid() == lastChoosenRecipeUid)
                {
                    Debug.Log("ok nasel sem ho");
                    LastChoosenRecipe = entry;
                    OnEntryClicked(LastChoosenRecipe);
                }
        }





    }

    public void ClickOnFirstEntry()
    {
        if (UIEntriesList.Count > 0)
            OnEntryClicked(UIEntriesList[0]);
    }

    private void OnEntryClicked(UICraftingRecipeEntry _entry)
    {
        LastChoosenRecipe = _entry;
        lastChoosenRecipeUid = _entry.GetUid();

        base.OnUISelectableItemClicked(_entry);
        
        OnRecipeClicked.Invoke(_entry);

      

    }






}
