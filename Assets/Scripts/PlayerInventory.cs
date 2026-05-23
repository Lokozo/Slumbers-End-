using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    private Dictionary<Item, int> resourceInventory = new Dictionary<Item, int>();

    public InventoryUI inventoryUI;

    public System.Action OnInventoryChanged;
    
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
    public void UseItem(Item item)
    {
        if (item == null)
            return;

        if (!item.isConsumable)
        {
            Debug.Log(item.itemName + " is not consumable.");
            return;
        }

        PlayerStats stats = PlayerStats.Get();

        if (stats == null)
            return;

        // RESTORE STATS
        stats.ModifyHealth(item.healthRestoreAmount);

        stats.ModifyHunger(item.hungerRestoreAmount);

        stats.ModifyEnergy(item.energyRestoreAmount);

        if (item.energyRecoveryBonus > 0)
        {
            stats.AddEnergyRecoveryBuff(
                item.energyRecoveryBonus,
                item.energyRecoveryDuration
            );
        }

        // REMOVE ONE ITEM
        RemoveItem(item, 1);

        OnInventoryChanged?.Invoke();
    }
    public bool HasResource(Item item)
    {
        return resourceInventory.ContainsKey(item)
            && resourceInventory[item] > 0;
    }
    public void RemoveResource(Item item, int amount)
    {
        if (!resourceInventory.ContainsKey(item))
            return;

        resourceInventory[item] -= amount;

        if (resourceInventory[item] <= 0)
            resourceInventory.Remove(item);

        inventoryUI.RefreshUI();
    }
    public bool HasWeapon(WeaponItem weapon)
    {
        foreach (var item in resourceInventory.Keys)
        {
            if (item is WeaponItem w && w.itemName == weapon.itemName)
                return true;
        }
        return false;
    }
    void Start()
    {
        // 🔥 FIXED: Use PlayerController's EXISTING weapon references
        var playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
        {
            // Add EXISTING weapons from PlayerController (KEEP their isEquipped state!)
            if (playerController.axeData != null)
            {
                AddItem(playerController.axeData, 1);
                Debug.Log("✅ Added EXISTING axeData to inventory");
            }

            if (playerController.knifeData != null)
            {
                AddItem(playerController.knifeData, 1);
                Debug.Log("✅ Added EXISTING knifeData to inventory");
            }
        }

        // Set attack weapon to knife (existing reference)
        PlayerAttack attack = FindFirstObjectByType<PlayerAttack>();
        if (attack != null && playerController?.knifeData != null)
        {
            attack.SetWeapon(playerController.knifeData);
        }

        OnInventoryChanged?.Invoke();
    }

    public bool AddItem(Item item, int quantity = 1)
    {
        if (item == null)
            return false;

        // WEAPONS
        if (item is WeaponItem)
        {
            // already owned
            if (resourceInventory.ContainsKey(item))
            {
                Debug.Log("Weapon already exists!");
                return false;
            }

            // inventory full
            if (resourceInventory.Count >= 10)
            {
                Debug.Log("Inventory Full!");
                return false;
            }

            resourceInventory[item] = 1;

            OnInventoryChanged?.Invoke();
            return true;
        }

        int currentAmount = 0;
        resourceInventory.TryGetValue(item, out currentAmount);

        // EXISTING STACK
        if (resourceInventory.ContainsKey(item))
        {
            // STACK FULL
            if (currentAmount + quantity > item.maxStack)
            {
                Debug.Log(item.itemName + " stack is full!");
                return false;
            }

            // SAFE TO ADD
            resourceInventory[item] += quantity;

            Debug.Log($"{item.itemName} now: {resourceInventory[item]}/{item.maxStack}");

            OnInventoryChanged?.Invoke();
            return true;
        }

        // NEW ITEM NEEDS EMPTY SLOT
        if (resourceInventory.Count >= 10)
        {
            Debug.Log("Inventory Full!");
            return false;
        }

        // CREATE NEW STACK
        resourceInventory[item] = quantity;

        Debug.Log($"{item.itemName} now: {resourceInventory[item]}/{item.maxStack}");

        OnInventoryChanged?.Invoke();
        return true;
    }
    public bool HasItem(Item item)
    {
        return resourceInventory.ContainsKey(item);
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

            OnInventoryChanged?.Invoke(); // 🔥 ADD THIS
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
    public bool IsInventoryFull(Item itemToAdd)
    {
        // Weapons already exist = don't add duplicates
        if (itemToAdd is WeaponItem)
        {
            return resourceInventory.ContainsKey(itemToAdd);
        }

        // Existing stack has room
        if (resourceInventory.ContainsKey(itemToAdd))
        {
            return resourceInventory[itemToAdd] >= itemToAdd.maxStack
                && resourceInventory.Count >= 10;
        }

        // New item needs empty slot
        return resourceInventory.Count >= 10;
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

    public void ApplyDeathPenalty()
    {
        Debug.Log("Applying death penalty...");

        List<Item> keys = new List<Item>(resourceInventory.Keys);

        foreach (Item item in keys)
        {
            // ❌ Skip weapons
            if (item is WeaponItem) continue;

            int currentAmount = resourceInventory[item];

            int loss = Mathf.CeilToInt(currentAmount * 0.2f);

            resourceInventory[item] -= loss;

            if (resourceInventory[item] <= 0)
            {
                resourceInventory.Remove(item);
            }

            Debug.Log($"{item.itemName} lost: {loss}");
        }

        OnInventoryChanged?.Invoke();
    }
}
