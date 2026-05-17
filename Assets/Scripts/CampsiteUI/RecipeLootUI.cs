using UnityEngine;
using UnityEngine.EventSystems;

public class RecipeLootUI : MonoBehaviour, IPointerClickHandler
{
    private CraftingRecipe recipe;
    private ResourceInteraction parent;

    public void Setup(CraftingRecipe r, ResourceInteraction p)
    {
        recipe = r;
        parent = p;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.clickCount == 2)
        {
            CollectRecipe();
        }
    }

    public void CollectRecipe()
    {
        if (recipe == null) return;

        RecipeManager.Instance.UnlockRecipe(recipe);

        parent.RemoveRecipe(recipe);

        Destroy(gameObject);
    }
}