using UnityEngine;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public GameObject slotPrefab;
    public Transform slotContainer;

    public GameObject descriptionPanel;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemDescriptionText;

    [HideInInspector] public MonoBehaviour parentUI;

    public int gridWidth = 5;
    public int gridHeight = 2;

    private ItemSlot[,] gridSlots;

   
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            TransferAllSelectedItem();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            gameObject.SetActive(false);
            ClearSelectedItem();
        }
    }
    void TransferAllSelectedItem()
    {
        Item selected = GetSelectedItem();
        if (selected == null) return;

        var inventory = PlayerInventory.Instance.GetInventory(); // ✅ FIXED

        if (!inventory.ContainsKey(selected)) return;

        int amount = inventory[selected];

        PlayerInventory.Instance.RemoveItem(selected, amount);
        FindObjectOfType<InventoryUI>()?.RefreshUI();
        CampsiteInventory.Instance.AddItem(selected, amount);
        FindObjectOfType<CampsiteInventoryUI>()?.RefreshInventoryDisplay();

        RefreshUI();

        var campsiteUI = FindObjectOfType<CampsiteInventoryUI>();
        if (campsiteUI != null)
            campsiteUI.RefreshInventoryDisplay();
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
    private void OnEnable()
    {
        PlayerInventory.Instance.OnInventoryChanged += RefreshUI;
        RefreshUI();
        if (gridSlots == null)
            GenerateGrid();

        RefreshUI();
    }

    private void OnDisable()
    {
        PlayerInventory.Instance.OnInventoryChanged -= RefreshUI;
    }
}