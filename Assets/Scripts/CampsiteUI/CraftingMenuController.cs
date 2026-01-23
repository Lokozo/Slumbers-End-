using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using static CraftingRecipe;

public class CraftingMenuController : MonoBehaviour
{
    [Header("Tabs")]

    public Button toolTab;
    public Button utilityTab;
    public Button cookingTab;
    public Button storageTab;

    [Header("Content Panels")]
    public GameObject storageContentPanel;
    public GameObject craftingContentPanel; // Used for Inventory/Tool/Utility/Cooking tabs

    [Header("Crafting Panels")]
    public GameObject recipeListPanel;
    public GameObject recipeDetailPanel;
    public Transform recipeListContent;
    public GameObject recipeButtonPrefab;

    [Header("Recipe Lists")]
    
    public List<CraftingRecipe> toolRecipes;
    public List<CraftingRecipe> utilityRecipes;
    public List<CraftingRecipe> cookingRecipes;

    private Button currentTab;

    [Header("Panels and Containers")]
    public GameObject DetailPanel;
    public Transform ingredientListContainer;

    [Header("Prefabs")]
    public GameObject ingredientEntryPrefab;

    [Header("Result Display")]
    public Image resultIcon;
    public TMP_Text resultNameText;

    [Header("Craft Button")]
    public Button craftButton;

    public CraftingRecipe selectedRecipe;

    public static CraftingMenuController Instance { get; private set; }

    void Start()
    {
        // Assign tab actions
        storageTab.onClick.AddListener(() => { SwitchToStorageTab(); });

        toolTab.onClick.AddListener(() => { SwitchToCraftingTab(toolRecipes, toolTab); });
        utilityTab.onClick.AddListener(() => { SwitchToCraftingTab(utilityRecipes, utilityTab); });
        cookingTab.onClick.AddListener(() => { SwitchToCraftingTab(cookingRecipes, cookingTab); });

        // Default view
        SwitchToStorageTab();
    }

    void Awake()
    {
        Instance = this;
    }

    void SwitchToCraftingTab(List<CraftingRecipe> recipes, Button selectedTab)
    {
        // Toggle content
        craftingContentPanel.SetActive(true);
        storageContentPanel.SetActive(false);

        // Tab visual state
        SelectTab(selectedTab);

        // Clear and populate recipe list
        foreach (Transform child in recipeListContent)
            Destroy(child.gameObject);

        foreach (var recipe in recipes)
        {
            GameObject btn = Instantiate(recipeButtonPrefab, recipeListContent);
            btn.GetComponent<RecipeButton>().Initialize(recipe);
        }

        recipeDetailPanel.SetActive(false); // Hide details until a recipe is clicked
    }

    void SwitchToStorageTab()
    {
        craftingContentPanel.SetActive(false);
        storageContentPanel.SetActive(true);

        SelectTab(storageTab);
    }

    public void ShowRecipeDetails(CraftingRecipe recipe)
    {
        Debug.Log("Showing recipe: " + recipe.recipeName);
        selectedRecipe = recipe;

        recipeDetailPanel.SetActive(true);

        // Clear previous ingredients
        foreach (Transform child in ingredientListContainer)
        {
            Destroy(child.gameObject);
        }

        // Populate new ingredient entries
        foreach (var ingredient in recipe.ingredients)
        {
            GameObject entry = Instantiate(ingredientEntryPrefab, ingredientListContainer);
            entry.transform.Find("Icon").GetComponent<Image>().sprite = ingredient.item.icon;
            entry.transform.Find("Name").GetComponent<TMP_Text>().text = ingredient.item.itemName;

            int have = PlayerInventory.Instance.GetItemQuantity(ingredient.item);
            int need = ingredient.amount;
            var qtyText = entry.transform.Find("Qty").GetComponent<TMP_Text>();
            qtyText.text = $"{have}/{need}";
            qtyText.color = (have >= need) ? Color.green : Color.red;
        }

        // Set result icon and name
        
        resultIcon.sprite = recipe.resultItem.icon;
        resultNameText.text = recipe.resultItem.itemName;


        // Set craft button
        craftButton.onClick.RemoveAllListeners();
        craftButton.onClick.AddListener(() => TryCraft());
    }

    void TryCraft()
    {
        if (selectedRecipe == null)
            return;

        // Check if the player has all required ingredients
        foreach (var ingredient in selectedRecipe.ingredients)
        {
            if (!PlayerInventory.Instance.HasItem(ingredient.item, ingredient.amount))
            {
                Debug.Log("Not enough resources to craft.");
                return;
            }
        }

        // Remove ingredients
        foreach (var ingredient in selectedRecipe.ingredients)
        {
            PlayerInventory.Instance.RemoveItem(ingredient.item, ingredient.amount);
        }

        // Add result item
        PlayerInventory.Instance.AddItem(selectedRecipe.resultItem, selectedRecipe.resultAmount);

        Debug.Log($"Crafted {selectedRecipe.resultAmount}x {selectedRecipe.resultItem.itemName}");

        // Refresh UI
        ShowRecipeDetails(selectedRecipe);
    }


    void SelectTab(Button newTab)
    {
        if (currentTab != null)
            currentTab.interactable = true;

        currentTab = newTab;
        currentTab.interactable = false;
    }
}
