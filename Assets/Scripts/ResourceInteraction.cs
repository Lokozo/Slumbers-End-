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
    public GameObject InventoryMenu;
    public GameObject ResourcePanel;
    public Transform ResourceContentPanel; // Parent panel for dynamic resource UI
    public GameObject ResourceItemUIPrefab; // UI prefab to display item + amount

    [Header("Interaction Settings")]
    public float holdTimeToOpen = 1.0f;
    private bool playerInRange = false;
    private float holdTimer = 0f;
    private bool panelOpened = false;

    [Header("Item Drops")]
    public List<ItemDropData> possibleDrops;


    private Dictionary<Item, int> currentDropList = new Dictionary<Item, int>();

    
    public GameObject checkIcon;
    private bool hasBeenCollected = false;

    

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

        InventoryMenu = ui.inventoryMenu;
        ResourcePanel = ui.resourcePanel;
        ResourceContentPanel = ui.resourceContentPanel;
        magnifyingGlassIcon = ui.magnifyingGlassIcon;
        checkIcon = ui.checkIcon;
        lootableNameText = ui.lootableNameText;
    }

    private void Update()
    {
        if (!playerInRange || hasBeenCollected) return;

        // TAKE ALL (Q)
        if (panelOpened && Keyboard.current.qKey.wasPressedThisFrame)
        {
            CollectResource();
            ClosePanels(); // ← only if you want it to close
            return;
        }

        // EXIT (ESC)
        if (panelOpened && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ClosePanels();
            return;
        }

        if (!panelOpened && Keyboard.current.eKey.isPressed)
        {
            holdTimer += Time.unscaledDeltaTime;

            if (holdTimer >= holdTimeToOpen)
            {
                GenerateRandomResources();
                OpenPanels();
                panelOpened = true;
            }
        }
        else if (!Keyboard.current.eKey.isPressed)
        {
            holdTimer = 0f;
        }
    }

    private void GenerateRandomResources()
    {
        currentDropList.Clear();

        foreach (var drop in possibleDrops)
        {
            if (Random.value <= drop.dropChance)
            {
                int amount = Random.Range(drop.minAmount, drop.maxAmount + 1);
                if (currentDropList.ContainsKey(drop.item))
                    currentDropList[drop.item] += amount;
                else
                    currentDropList.Add(drop.item, amount);
            }
        }

        // Clear previous UI
        foreach (Transform child in ResourceContentPanel)
        {
            Destroy(child.gameObject);
        }

        // Populate new UI
        foreach (var pair in currentDropList)
        {
            GameObject uiElement = Instantiate(ResourceItemUIPrefab, ResourceContentPanel);
            uiElement.transform.Find("ItemName").GetComponent<TextMeshProUGUI>().text = pair.Key.itemName;
            uiElement.transform.Find("ItemAmount").GetComponent<TextMeshProUGUI>().text = "x" + pair.Value;
            uiElement.transform.Find("ItemIcon").GetComponent<Image>().sprite = pair.Key.icon;

            var itemUI = uiElement.AddComponent<ResourceItemUI>();
            itemUI.Setup(pair.Key, pair.Value, this);
        }
    }

    private void OpenPanels()
    {
        InventoryMenu?.SetActive(true);
        ResourcePanel?.SetActive(true);

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

        foreach (var pair in currentDropList)
        {
            PlayerInventory.Instance.AddItem(pair.Key, pair.Value);
        }

        currentDropList.Clear();

        // Clear UI
        foreach (Transform child in ResourceContentPanel)
        {
            Destroy(child.gameObject);
        }

        hasBeenCollected = true;

        Debug.Log("Inventory contains: " + PlayerInventory.Instance.GetInventory().Count + " items.");

        TutorialUIManager.Instance?.Hide();
        TutorialUIManager.Instance.ShowStep("inventoryTutorial", "Press I to open your inventory");
        // EXIT (ESC)
       
        // ❌ DO NOT CLOSE PANELS
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

            if (hasBeenCollected)
            {
                checkIcon?.SetActive(true);
                magnifyingGlassIcon?.SetActive(false);
            }
            else
            {
                magnifyingGlassIcon?.SetActive(true);
                checkIcon?.SetActive(false);

                TutorialUIManager.Instance?.ShowStep("examineTutorial","Hold E to examine");

            }


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
            holdTimer = 0f;

            
            TutorialUIManager.Instance?.Hide();
            
        }
    }

}
