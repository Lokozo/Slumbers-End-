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
        CampArea camp = FindFirstObjectByType<CampArea>();

        // 🔥 BLOCK IF NOT INSIDE CAMP
        if (camp == null || !camp.IsInCamp())
        {
            Debug.Log("⚠️ Cannot transfer items outside campsite!");
            return;
        }

        if (item == null)
        {
            Debug.LogError("Tried to add NULL item to campsite!");
            return;
        }

        if (inventory.ContainsKey(item))
            inventory[item] += amount;
        else
            inventory[item] = amount;

        Debug.Log($"[CampsiteInventory] Added {amount}x {item.itemName}");

        OnInventoryChanged?.Invoke();
    }

    public bool RemoveItem(Item item, int amount)
    {
        if (!inventory.ContainsKey(item))
            return false;

        inventory[item] -= amount;

        if (inventory[item] <= 0)
            inventory.Remove(item);

        OnInventoryChanged?.Invoke();

        return true;
    }

    public Dictionary<Item, int> GetInventory()
    {
        return inventory;
    }
}