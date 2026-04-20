using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    private Dictionary<Item, int> resourceInventory = new Dictionary<Item, int>();

    public InventoryUI inventoryUI;

    public WeaponItem startingWeapon;


    void Awake()
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

        //if (Instance != null && Instance != this)
        //{
        //    Destroy(gameObject);
        //    return;
        //}

        //Instance = this;
        //DontDestroyOnLoad(gameObject); // <-- THIS KEEPS IT ALIVE
    }
    void Start()
    {
        if (startingWeapon != null)
        {
            Item weaponInstance = Instantiate(startingWeapon);
            weaponInstance.isEquipped = true;

            resourceInventory.Add(weaponInstance, 1);

            Debug.Log("Starting axe added to inventory");
        }
    }

    public void AddItem(Item item, int quantity = 1)
    {
        if (item is WeaponItem)
        {
            Item uniqueItem = Instantiate(item);
            resourceInventory[uniqueItem] = 1;
            Debug.Log($"Added weapon: {item.itemName}");
            return;
        }

        if (resourceInventory.ContainsKey(item))
            resourceInventory[item] += quantity;
        else
            resourceInventory[item] = quantity;

        Debug.Log($"{quantity} {item.itemName}(s) added.");
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
