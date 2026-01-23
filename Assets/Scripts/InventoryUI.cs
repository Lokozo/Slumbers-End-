using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public GameObject slotPrefab;
    public Transform slotContainer;

    // Description Panel Fields
    public GameObject descriptionPanel;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemDescriptionText;

    private List<ItemSlot> slotInstances = new List<ItemSlot>();

    public void RefreshUI()
    {
        Debug.Log("Refreshing Inventory UI...");
        if (PlayerInventory.Instance == null) return;

        var inventory = PlayerInventory.Instance.GetInventory();

        foreach (Transform child in slotContainer)
            Destroy(child.gameObject);
        slotInstances.Clear();

        foreach (var pair in inventory)
        {
            GameObject slotObj = Instantiate(slotPrefab, slotContainer);
            ItemSlot slot = slotObj.GetComponent<ItemSlot>();

            // Set reference to parent UI
            slot.parentUI = this;

            slot.SetSlot(pair.Key, pair.Value, SlotContextType.Inventory);
            slotInstances.Add(slot);
        }

        //Hide description panel by default
        if (descriptionPanel != null)
            descriptionPanel.SetActive(false);

        ClearDescription();
    }

    // This is called by ItemSlot
    public void ShowItemDescription(Item item)
    {
        if (descriptionPanel == null) return;

        descriptionPanel.SetActive(true);
        itemNameText.text = item.itemName;
        itemDescriptionText.text = item.description;
    }

    public void ClearDescription()
    {
        itemNameText.text = "";
        itemDescriptionText.text = "";
        descriptionPanel.SetActive(false);
    }

    private Item selectedItem;

    public Item GetSelectedItem()
    {
        return selectedItem;
    }

    public void SetSelectedItem(Item item)
    {
        selectedItem = item;
        ShowItemDescription(item);
    }

    public void ClearSelectedItem()
    {
        selectedItem = null;
        ClearDescription();
    }

}
