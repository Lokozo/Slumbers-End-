using UnityEngine;

[CreateAssetMenu(menuName = "Crafting/Recipe")]
public class CraftingRecipe : ScriptableObject
{
    [Header("Recipe Visuals")]
    public Sprite recipeIcon;

    public string recipeName;

    public Item resultItem;
    public int resultAmount = 1;

    [Header("Crafting Station Requirement")]
    public CraftingStationType requiredStation;

    [System.Serializable]
    public class Ingredient
    {
        public Item item;
        public int amount;
    }

    public Ingredient[] ingredients;
}

public enum CraftingStationType
{
    None,
    Workbench,
    Furnace
}