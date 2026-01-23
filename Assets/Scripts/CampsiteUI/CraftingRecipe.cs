using UnityEngine;

[CreateAssetMenu(menuName = "Crafting/Recipe")]
public class CraftingRecipe : ScriptableObject
{
    public string recipeName;
    public Item resultItem;
    public int resultAmount = 1;

    [System.Serializable]
    public class Ingredient
    {
        public Item item;
        public int amount;
    }

    public Ingredient[] ingredients;
}