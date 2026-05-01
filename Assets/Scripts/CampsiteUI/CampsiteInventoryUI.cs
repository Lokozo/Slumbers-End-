using System.Collections;
using UnityEngine;

public class CampsiteInventoryUI : MonoBehaviour
{
    public Transform InventorySlotContainer;
    public GameObject ItemSlotPrefab;

    private ItemSlot[,] gridSlots;
    public int gridWidth = 5;
    public int gridHeight = 4;

    private IEnumerator WaitForInventory()
    {
        while (CampsiteInventory.Instance == null)
            yield return null;

        CampsiteInventory.Instance.OnInventoryChanged += RefreshInventoryDisplay;

        RefreshInventoryDisplay();
    }

    public void Update()
    {
        foreach (var pair in PlayerInventory.Instance.GetInventory())
        {
            Debug.Log($"Item: {pair.Key.itemName}, Qty: {pair.Value}");
        }
    }

    public void RefreshInventoryDisplay()
    {
        if (gridSlots == null)
            GenerateGrid();

        var inventory = CampsiteInventory.Instance.GetInventory();

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

            gridSlots[x, y].SetSlot(pair.Key, pair.Value, SlotContextType.Campsite);

            index++;
        }
    }
    void GenerateGrid()
    {
        gridSlots = new ItemSlot[gridWidth, gridHeight];

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                GameObject slotObj = Instantiate(ItemSlotPrefab, InventorySlotContainer);
                ItemSlot slot = slotObj.GetComponent<ItemSlot>();

                gridSlots[x, y] = slot;
            }
        }
    }

    private void OnEnable()
    {
        StartCoroutine(WaitForInventory());
    }
    private void OnDisable()
    {
        if (CampsiteInventory.Instance != null)
            CampsiteInventory.Instance.OnInventoryChanged -= RefreshInventoryDisplay;
    }
}
