using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryInputHandler : MonoBehaviour
{
    public InventoryManager inventoryManager;
    public PlayerInventory playerInventory;
    public PlayerStats playerStats;

    void Update()
    {
        //if (!inventoryManager.IsInventoryOpen()) return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            Item selected = playerInventory.inventoryUI.GetSelectedItem();
            if (selected != null && selected.isConsumable)
            {
                bool removed = playerInventory.RemoveItem(selected, 1);
                if (removed)
                {
                    ConsumeItem(selected);
                    playerInventory.inventoryUI.RefreshUI();
                    playerInventory.inventoryUI.ClearSelectedItem();
                }
            }
        }
    }

    void ConsumeItem(Item item)
    {
        playerStats.ModifyHealth(item.healthRestoreAmount);
        playerStats.ModifyHunger(item.hungerRestoreAmount);
        playerStats.ModifyEnergy(item.energyRestoreAmount);
        Debug.Log($"Consumed {item.itemName}");
    }
}
