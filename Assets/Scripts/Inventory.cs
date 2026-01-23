using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public Dictionary<Item, int> items = new Dictionary<Item, int>();

  


    public void AddItem(Item item, int quantity = 1)
    {
        if (items.ContainsKey(item))
            items[item] += quantity;
        else
            items[item] = quantity;

        Debug.Log($"{quantity} {item.itemName}(s) added.");
    }

    public void RemoveItem(Item item, int quantity = 1)
    {
        if (items.ContainsKey(item))
        {
            items[item] -= quantity;
            if (items[item] <= 0)
                items.Remove(item);

            Debug.Log($"{quantity} {item.itemName}(s) removed.");
        }
        else
        {
            Debug.Log($"{item.itemName} not in inventory.");
        }
    }

    public bool HasItem(Item item, int amount = 1)
    {
        return items.ContainsKey(item) && items[item] >= amount;
    }

    public void PrintInventory()
    {
        Debug.Log("Inventory Contents:");
        foreach (var kvp in items)
            Debug.Log($"{kvp.Key.itemName} x{kvp.Value}");
    }


}
