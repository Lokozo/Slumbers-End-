using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecipeButton : MonoBehaviour
{
    public CraftingRecipe recipe;

    public void Initialize(CraftingRecipe recipe)
    {
        this.recipe = recipe;
        GetComponentInChildren<TMP_Text>().text = recipe.recipeName;

        GetComponent<Button>().onClick.AddListener(() =>
        {
            Debug.Log("Clicked " + recipe.recipeName); //
            CraftingMenuController.Instance.ShowRecipeDetails(recipe);
        });
    }
}
