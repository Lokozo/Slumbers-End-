using System.Collections;
using UnityEngine;

public class CampsiteInventoryUI : MonoBehaviour
{
    public Transform InventorySlotContainer;
    public GameObject ItemSlotPrefab;

    private void OnEnable()
    {
        RefreshInventoryDisplay();
    }

    public void Update()
    {
        Debug.Log("Inventory count: " + PlayerInventory.Instance.GetInventory().Count);
        foreach (var pair in PlayerInventory.Instance.GetInventory())
        {
            Debug.Log($"Item: {pair.Key.itemName}, Qty: {pair.Value}");
        }
    }

    public void RefreshInventoryDisplay()
    {
        var inventory = PlayerInventory.Instance.GetInventory();//

        foreach (Transform child in InventorySlotContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (var pair in PlayerInventory.Instance.GetInventory())
        {
            Item item = pair.Key;
            int quantity = pair.Value;

            if (item != null && quantity > 0)
            {
                GameObject slotGO = Instantiate(ItemSlotPrefab, InventorySlotContainer);
                slotGO.GetComponent<ItemSlot>().SetSlot(item, quantity, SlotContextType.Inventory);
            }
        }

    }


}
