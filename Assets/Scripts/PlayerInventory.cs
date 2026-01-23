using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    private Dictionary<Item, int> resourceInventory = new Dictionary<Item, int>();

    public InventoryUI inventoryUI;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);

        }

        //if (Instance != null && Instance != this)
        //{
        //    Destroy(gameObject);
        //    return;
        //}

        //Instance = this;
        //DontDestroyOnLoad(gameObject); // <-- THIS KEEPS IT ALIVE
    }

    public void AddItem(Item item, int amount)
    {
        if (resourceInventory.ContainsKey(item))
        {
            resourceInventory[item] += amount;
        }
        else
        {
            resourceInventory[item] = amount;
        }

        Debug.Log($"Collected {amount}x {item.itemName}");
        inventoryUI?.RefreshUI();
    }

    public bool HasItem(Item item, int amount)
    {
        return resourceInventory.ContainsKey(item) && resourceInventory[item] >= amount;
    }

    public bool RemoveItem(Item item, int amount)
    {
        if (HasItem(item, amount))
        {
            resourceInventory[item] -= amount;
            if (resourceInventory[item] <= 0)
            {
                resourceInventory.Remove(item);
            }
            return true;
        }
        return false;
    }

    public Dictionary<Item, int> GetInventory()
    {
        return resourceInventory;
    }

    public int GetItemQuantity(Item item)
    {
        if (resourceInventory.TryGetValue(item, out int quantity))
        {
            return quantity;
        }
        return 0;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        inventoryUI = GameObject.FindFirstObjectByType<InventoryUI>(FindObjectsInactive.Include);
        inventoryUI.RefreshUI();
        if (inventoryUI == null)
            Debug.LogError("[PlayerInventory] InventoryUI not found in new scene!");
    }

}
