using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    private Dictionary<Item, int> resourceInventory = new Dictionary<Item, int>();

    public InventoryUI inventoryUI;

    public List<WeaponItem> startingWeapons;

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
        PlayerAttack attack = FindFirstObjectByType<PlayerAttack>();

        bool firstWeaponEquipped = false;

        foreach (WeaponItem weapon in startingWeapons)
        {
            if (weapon == null) continue;

            WeaponItem instance = Instantiate(weapon);

            // ✅ ONLY first weapon is equipped
            instance.isEquipped = false;

            resourceInventory.Add(instance, 1);
            Debug.Log("Added starting weapon: " + instance.itemName);

            if (!firstWeaponEquipped && attack != null)
            {
                instance.isEquipped = true;
                attack.SetWeapon(instance);
                firstWeaponEquipped = true;
            }
        }

        OnInventoryChanged?.Invoke();
    }

    public void AddItem(Item item, int quantity = 1)
    {
        if (item == null)
        {
            Debug.LogError("❌ Tried to add NULL item to inventory!");
            return;
        }

        // Weapons (no stacking)
        if (item is WeaponItem)
        {
            resourceInventory[item] = 1;

            Debug.Log($"Added weapon: {item.itemName}");
            OnInventoryChanged?.Invoke();
            return;
        }

        int currentAmount = 0;
        resourceInventory.TryGetValue(item, out currentAmount);

        int newAmount = currentAmount + quantity;

        // 🔥 APPLY MAX STACK
        newAmount = Mathf.Min(newAmount, item.maxStack);

        resourceInventory[item] = newAmount;

        Debug.Log($"{item.itemName} now: {newAmount}/{item.maxStack}");

        OnInventoryChanged?.Invoke();
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
