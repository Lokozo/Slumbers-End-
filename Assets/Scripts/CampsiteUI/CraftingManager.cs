
using System.Collections.Generic;
using UnityEngine;

public class CraftingManager : MonoBehaviour
{
    public static CraftingManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public bool CanCraft(CraftingRecipe recipe)
    {
        foreach (var ingredient in recipe.ingredients)
        {
            if (!PlayerInventory.Instance.HasItem(ingredient.item, ingredient.amount))
                return false;
        }
        return true;
    }

    public void Craft(CraftingRecipe recipe)
    {
        if (!CanCraft(recipe)) return;

        foreach (var ingredient in recipe.ingredients)
        {
            PlayerInventory.Instance.RemoveItem(ingredient.item, ingredient.amount);
        }

        PlayerInventory.Instance.AddItem(recipe.resultItem, recipe.resultAmount);
    }
}
