using UnityEngine;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public GameObject slotPrefab;
    public Transform slotContainer;

    public GameObject descriptionPanel;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemDescriptionText;

    public int gridWidth = 5;
    public int gridHeight = 2;

    private ItemSlot[,] gridSlots;

    private void OnEnable()
    {
        if (gridSlots == null)
            GenerateGrid();

        RefreshUI();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Item selected = GetSelectedItem();

            if (selected != null)
            {
                var inventory = PlayerInventory.Instance.GetInventory();

                if (inventory.ContainsKey(selected))
                {
                    int amount = inventory[selected]; // 🔥 get ALL quantity

                    PlayerInventory.Instance.RemoveItem(selected, amount);
                    CampsiteInventory.Instance.AddItem(selected, amount);

                    RefreshUI();
                    FindObjectOfType<CampsiteInventoryUI>()?.RefreshInventoryDisplay();
                }
            }
        }
    }
    void GenerateGrid()
    {
        gridSlots = new ItemSlot[gridWidth, gridHeight];

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                GameObject slotObj = Instantiate(slotPrefab, slotContainer);
                ItemSlot slot = slotObj.GetComponent<ItemSlot>();

                slot.parentUI = this;
                gridSlots[x, y] = slot;
            }
        }
    }

    public void RefreshUI()
    {

        if (PlayerInventory.Instance == null) return;
        var inventory = PlayerInventory.Instance.GetInventory();

        if (gridSlots == null)
            GenerateGrid();

        

        // Clear slots safely
        foreach (var slot in gridSlots)
        {
            if (slot != null)
                slot.ClearSlot();
        }

        int index = 0;

        foreach (var pair in inventory)
        {
            int x = index % gridWidth;
            int y = index / gridWidth;

            if (y >= gridHeight) break;

            Item item = pair.Key;
            int quantity = pair.Value;

            gridSlots[x, y].SetSlot(item, quantity, SlotContextType.Inventory);

            index++;
        }

        ClearDescription();
    }

    public void ShowItemDescription(Item item)
    {
        if (descriptionPanel == null) return;

        descriptionPanel.SetActive(true);
        itemNameText.text = item.itemName;
        itemDescriptionText.text = item.description;
    }

    public void ClearDescription()
    {
        if (descriptionPanel == null) return;

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