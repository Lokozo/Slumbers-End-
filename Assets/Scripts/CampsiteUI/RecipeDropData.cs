using UnityEngine;

[System.Serializable]
public class RecipeDropData
{
    public CraftingRecipe recipe;

    [Range(0f, 1f)]
    public float dropChance = 1f;
}