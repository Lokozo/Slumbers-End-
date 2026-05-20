using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;


public class ResourceInteraction : MonoBehaviour
{
    [Header("UI References")]
    public GameObject magnifyingGlassIcon;
    public GameObject lockIcon;
    public GameObject InventoryMenu;
    public GameObject ResourcePanel;
    public Transform ResourceContentPanel; // Parent panel for dynamic resource UI
    public GameObject ResourceItemUIPrefab; // UI prefab to display item + amount
    public GameObject RecipeItemUIPrefab;


    [Header("Interaction Settings")]
    public float holdTimeToOpen = 1.0f;
    private bool playerInRange = false;
    private float holdTimer = 0f;
    private bool panelOpened = false;
    private bool hasGeneratedLoot = false;

    [Header("Item Drops")]
    public List<ItemDropData> possibleDrops;

    [Header("Recipe Drops")]
    public List<RecipeDropData> possibleRecipeDrops;



    private Dictionary<Item, int> currentDropList = new Dictionary<Item, int>();
    private List<CraftingRecipe> currentRecipeDrops = new List<CraftingRecipe>();


    public GameObject checkIcon;
    private bool hasBeenCollected = false;

    private LockableObject lockable;

    [Header("Lootable Metadata")]
    public string lootableDisplayName;  // set in Inspector
    public TextMeshProUGUI lootableNameText;



    private void Awake()
    {
        if (string.IsNullOrEmpty(lootableDisplayName))
            lootableDisplayName = gameObject.name;
    }

    private void Start()
    {
        var ui = UIManager.Instance;


        lockable = GetComponent<LockableObject>();
        InventoryMenu = ui.inventoryMenu;
        ResourcePanel = ui.resourcePanel;
        ResourceContentPanel = ui.resourceContentPanel;
        magnifyingGlassIcon = ui.magnifyingGlassIcon;
        checkIcon = ui.checkIcon;
        lootableNameText = ui.lootableNameText;
        lockIcon = ui.lockIcon;
    }

    private void Update()
    {
        if (!playerInRange || hasBeenCollected) return;

        // EXIT (ESC) ✅ FIX
        if (panelOpened && Keyboard.current.backquoteKey.wasPressedThisFrame)
        {
            ClosePanels();
            return;
        }

        // TAKE ALL (Q)
        if (panelOpened && Keyboard.current.qKey.wasPressedThisFrame)
        {
            CollectResource();
            RecipeItemUIPrefab.GetComponent<RecipeLootUI>()?.CollectRecipe();
            ClosePanels();
            return;
        }

        // OPEN (Hold E)
        if (!panelOpened && Keyboard.current.eKey.isPressed)
        {
            holdTimer += Time.unscaledDeltaTime;

            if (holdTimer >= holdTimeToOpen)
            {
                // 🔒 CHECK LOCK FIRST
                if (lockable != null && lockable.isLocked)
                {
                    bool unlocked = lockable.TryUnlock();

                    if (!unlocked)
                    {
                        holdTimer = 0f;
                        return; // ❌ STOP opening
                    }
                }

                // ✅ OPEN LOOT (your existing logic)
                if (!hasGeneratedLoot)
                {
                    GenerateRandomResources();
                    hasGeneratedLoot = true;
                }
                else
                {
                    RebuildUI();
                }

                OpenPanels();
                panelOpened = true;
            }
        }
        else if (!Keyboard.current.eKey.isPressed)
        {
            holdTimer = 0f;
        }
    }
    private void RebuildUI()
    {
        if (ResourceContentPanel == null)
            return;

        ClearUI();

        if (currentDropList.Count == 0)
        {
            Debug.Log("Loot is empty");
        }

        foreach (var pair in currentDropList)
        {
            if (pair.Key == null)
            {
                Debug.LogError("❌ NULL item in RebuildUI");
                continue;
            }

            GameObject uiElement = Instantiate(ResourceItemUIPrefab, ResourceContentPanel);

            uiElement.transform.Find("ItemName")
                .GetComponent<TextMeshProUGUI>().text = pair.Key.itemName;

            uiElement.transform.Find("ItemAmount")
                .GetComponent<TextMeshProUGUI>().text = "x" + pair.Value;

            uiElement.transform.Find("ItemIcon")
                .GetComponent<Image>().sprite = pair.Key.icon;

            var itemUI = uiElement.GetComponent<ResourceItemUI>();
            itemUI.Setup(pair.Key, pair.Value, this);
        }

        foreach (var recipe in currentRecipeDrops)
        {
            GameObject uiElement =
                Instantiate(RecipeItemUIPrefab, ResourceContentPanel);

            uiElement.transform.Find("ItemName")
                .GetComponent<TextMeshProUGUI>().text =
                recipe.recipeName + " Notes";

            //uiElement.transform.Find("ItemAmount")
            //    .GetComponent<TextMeshProUGUI>().text = "";

            uiElement.transform.Find("ItemIcon")
                .GetComponent<Image>().sprite =
                recipe.recipeIcon;

            var recipeUI = uiElement.GetComponent<RecipeLootUI>();

            if (recipeUI != null)
            {
                recipeUI.Setup(recipe, this);
            }
        }
    }

    private void GenerateRandomResources()
    {
        currentDropList.Clear();
        currentRecipeDrops.Clear();

        foreach (var drop in possibleDrops)
        {
            // ✅ NOW it's valid
            Debug.Log("Drop item: " + drop.item);

            if (drop == null || drop.item == null)
            {
                Debug.LogError("❌ NULL item in possibleDrops!");
                continue;
            }

            if (Random.value <= drop.dropChance)
            {
                int amount = Random.Range(drop.minAmount, drop.maxAmount + 1);

                if (currentDropList.ContainsKey(drop.item))
                    currentDropList[drop.item] += amount;
                else
                    currentDropList.Add(drop.item, amount);
            }
            
        

        }

        ClearUI();

        foreach (var pair in currentDropList)
        {
            if (pair.Key == null)
            {
                Debug.LogError("❌ NULL item reached UI!");
                continue;
            }

            GameObject uiElement = Instantiate(ResourceItemUIPrefab, ResourceContentPanel);

            uiElement.transform.Find("ItemName")
                .GetComponent<TextMeshProUGUI>().text = pair.Key.itemName;

            uiElement.transform.Find("ItemAmount")
                .GetComponent<TextMeshProUGUI>().text = "x" + pair.Value;

            uiElement.transform.Find("ItemIcon")
                .GetComponent<Image>().sprite = pair.Key.icon;

            var itemUI = uiElement.GetComponent<ResourceItemUI>();

            if (itemUI == null)
            {
                Debug.LogError("❌ ResourceItemUI missing on prefab!");
                return;
            }

            itemUI.Setup(pair.Key, pair.Value, this);
        }

foreach (var recipeDrop in possibleRecipeDrops)
{
    if (recipeDrop.recipe == null)
        continue;

    if (Random.value <= recipeDrop.dropChance)
    {
        currentRecipeDrops.Add(recipeDrop.recipe);

        Debug.Log("Generated recipe: " + recipeDrop.recipe.recipeName);
    }
}

foreach (var recipe in currentRecipeDrops)
{
            GameObject uiElement =
            Instantiate(RecipeItemUIPrefab, ResourceContentPanel);

            uiElement.transform.Find("ItemName")
        .GetComponent<TextMeshProUGUI>().text = 
        recipe.recipeName + " Notes";

    //uiElement.transform.Find("ItemAmount")
    //    .GetComponent<TextMeshProUGUI>().text = "";

    uiElement.transform.Find("ItemIcon")
        .GetComponent<Image>().sprite =
        recipe.recipeIcon;

            var recipeUI = uiElement.GetComponent<RecipeLootUI>();

            if (recipeUI != null)
            {
                recipeUI.Setup(recipe, this);
            }
        }
    }

    private void OpenPanels()
    {
        InventoryMenu?.SetActive(true);
        ResourcePanel?.SetActive(true);
        lockIcon?.SetActive(false);
        TutorialUIManager.Instance?.Hide();

        if (lootableNameText != null)
            lootableNameText.text = lootableDisplayName;

        Time.timeScale = 0f;
        //Cursor.lockState = CursorLockMode.None;
        //Cursor.visible = true;
    }

    private void ClosePanels()
    {
        InventoryMenu?.SetActive(false);
        ResourcePanel?.SetActive(false);
        magnifyingGlassIcon?.SetActive(false);
        checkIcon?.SetActive(false);

        panelOpened = false;

        Time.timeScale = 1f;
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
    }

    private void CollectResource()
    {
        if (PlayerInventory.Instance == null)
        {
            Debug.LogError("ResourcesManager instance is missing in the scene!");
            return;
        }

        // =========================
        // COLLECT NORMAL ITEMS
        // =========================

        foreach (var pair in currentDropList)
        {
            PlayerInventory.Instance.AddItem(pair.Key, pair.Value);
        }

        // =========================
        // COLLECT RECIPES
        // =========================

        foreach (var recipe in currentRecipeDrops.ToList())
        {
            RecipeManager.Instance.UnlockRecipe(recipe);

            Debug.Log("Learned recipe: " + recipe.recipeName);
        }

        // Clear collected things
        currentDropList.Clear();
        currentRecipeDrops.Clear();

        ClearUI();

        if (currentDropList.Count == 0 && currentRecipeDrops.Count == 0)
        {
            hasBeenCollected = true;
            ClosePanels();
        }

        TutorialUIManager.Instance?.Hide();

        TutorialUIManager.Instance.ShowStep(
            "inventoryTutorial",
            "Press I to open your inventory"
        );
    }
    public void RemoveItem(Item item, int amountToRemove = 1)
    {
        if (currentDropList.ContainsKey(item))
        {
            currentDropList[item] -= amountToRemove;

            if (currentDropList[item] <= 0)
            {
                currentDropList.Remove(item);
            }
        }

        if (currentDropList.Count == 0)
        {
            ClearUI();

            hasBeenCollected = true;
            ClosePanels();
        }
    }

    public void RemoveRecipe(CraftingRecipe recipe)
    {
        if (currentRecipeDrops.Contains(recipe))
        {
            currentRecipeDrops.Remove(recipe);
        }
        if (currentDropList.Count == 0 && currentRecipeDrops.Count == 0)
        {
            hasBeenCollected = true;
            ClosePanels();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered interaction range.");
            playerInRange = true;

            bool isCurrentlyLocked =
                lockable != null &&
                lockable.isLocked;
            // ✅ LOCKED OBJECT
            if (isCurrentlyLocked)
            {
                lockIcon?.SetActive(true);
                magnifyingGlassIcon?.SetActive(false);
                checkIcon?.SetActive(false);

                TutorialUIManager.Instance?.ShowStep(
                    "lockedLootTutorial",
                    "It's locked. A lockpick might open it.\nMaybe there's a way to craft one at the tent."
                );

                return;
            }
            if (hasBeenCollected)
            {
                checkIcon?.SetActive(true);
                magnifyingGlassIcon?.SetActive(false);
            }
            else
            {
                magnifyingGlassIcon?.SetActive(true);
                checkIcon?.SetActive(false);

                TutorialUIManager.Instance?.ShowStep("examineTutorial", "Hold E to examine");

            }


        }
    }
    private void ClearUI()
    {
        for (int i = ResourceContentPanel.childCount - 1; i >= 0; i--)
        {
            Destroy(ResourceContentPanel.GetChild(i).gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player exited interaction range.");
            playerInRange = false;
            magnifyingGlassIcon?.SetActive(false);
            checkIcon?.SetActive(false);
            lockIcon?.SetActive(false);
            holdTimer = 0f;


            TutorialUIManager.Instance?.Hide();

        }
    }

}