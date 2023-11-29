using System;
using System.Collections.Generic;
using Firebase.Firestore;
using UnityEngine;
using UnityEngine.TextCore.Text;


namespace simplestmmorpg.data
{

    [Serializable]
    [FirestoreData]
    public class CraftingRecipesMetadata
    {
        [field: SerializeField]
        [FirestoreProperty]
        public List<CraftingRecipe> craftingRecipes { get; set; }



        public CraftingRecipe GetRecipeById(string _id)
        {
            foreach (var item in craftingRecipes)
            {
                if (item.id == _id)
                    return item;
            }

            Debug.LogError("Cant find crafting recipe with Id : " + _id);
            return null;
        }
    }

    [Serializable]
    [FirestoreData]
    public class CraftingRecipe 
    {
        [field: SerializeField]
        [FirestoreProperty]
        public string id { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public string professionId { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int professionAmountNeeded { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public List<ItemIdWithAmount> materials { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public ItemIdWithAmount product { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public RandomEquip productRandomEquip { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public ContentContainer productContent { get; set; }

        [field: SerializeField]
        [FirestoreProperty]
        public int timePrice { get; set; }

        


        public string GetDisplayName()
        {
            if (product != null)
                return Utils.DescriptionsMetadata.GetItemsMetadata(product.itemId).title.GetText();

            else if (productRandomEquip != null)
                return Utils.DescriptionsMetadata.GetCratingRecipesMetadata(id).title.GetText();

            else if (productContent != null)
                return productContent.GetContent().GetDisplayName();

            return "unknown";

        }

        public bool CanBeCrafted(CharacterData _character)
        {

            //if (_character.GetProfessionById(professionId) == null)
            //    return false;

            //if (_character.GetProfessionById(professionId).count < professionAmountNeeded)
            //    return false;



            foreach (var mat in materials)
            {
                if (_character.inventory.GetAmountOfItemsInInventory(mat.itemId) < mat.amount)
                    return false;
            }

            return true;
        }

    }





}

