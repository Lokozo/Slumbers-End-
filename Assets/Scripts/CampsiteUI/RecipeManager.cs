using System.Collections.Generic;
using UnityEngine;

public class RecipeManager : MonoBehaviour
{
    public static RecipeManager Instance;

    private HashSet<CraftingRecipe> unlockedRecipes = new HashSet<CraftingRecipe>();

    public System.Action OnRecipesChanged;

    [Header("Starting Recipes")]
    public List<CraftingRecipe> startingRecipes;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        foreach (var recipe in startingRecipes)
        {
            UnlockRecipe(recipe);
        }
    }   

    public void UnlockRecipe(CraftingRecipe recipe)
    {
        if (recipe == null) return;

        if (unlockedRecipes.Add(recipe))
        {
            Debug.Log("Unlocked recipe: " + recipe.recipeName);

            OnRecipesChanged?.Invoke();
        }
    }


    public bool IsRecipeUnlocked(CraftingRecipe recipe)
    {
        // DEVELOPER MODE = UNLOCK EVERYTHING
        if (DeveloperMode.Instance != null &&
            DeveloperMode.Instance.developerModeEnabled &&
            DeveloperMode.Instance.unlockAllRecipes)
        {
            return true;
        }

        return unlockedRecipes.Contains(recipe);
    }
    public List<CraftingRecipe> GetUnlockedRecipes()
    {
        return new List<CraftingRecipe>(unlockedRecipes);
    }
}