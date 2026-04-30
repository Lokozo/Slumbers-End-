using System.Collections.Generic;
using UnityEngine;

public class CampsiteInventory : MonoBehaviour
{
    public static CampsiteInventory Instance;

    public System.Action OnInventoryChanged; // ✅ ADD THIS

    private Dictionary<Item, int> inventory = new Dictionary<Item, int>();

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
    }

    public void AddItem(Item item, int amount)
    {
        if (inventory.ContainsKey(item))
            inventory[item] += amount;
        else
            inventory[item] = amount;

        Debug.Log($"[CampsiteInventory] Added {amount}x {item.itemName}");

        OnInventoryChanged?.Invoke(); // ✅ CALL EVENT
    }

    public void RemoveItem(Item item, int amount)
    {
        if (!inventory.ContainsKey(item)) return;

        inventory[item] -= amount;

        if (inventory[item] <= 0)
            inventory.Remove(item);

        OnInventoryChanged?.Invoke(); // ✅ CALL EVENT
    }

    public Dictionary<Item, int> GetInventory()
    {
        return inventory;
    }
}