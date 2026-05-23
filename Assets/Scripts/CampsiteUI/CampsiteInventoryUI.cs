using System.Collections;
using UnityEngine;
using TMPro;

public class CampsiteInventoryUI : MonoBehaviour
{
    public Transform InventorySlotContainer;
    public GameObject ItemSlotPrefab;

    private ItemSlot[,] gridSlots;
    public int gridWidth = 5;
    public int gridHeight = 4;

    public GameObject descriptionPanel;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemDescriptionText;

    private Item selectedItem;
    private IEnumerator WaitForInventory()
    {
        while (CampsiteInventory.Instance == null)
            yield return null;

        CampsiteInventory.Instance.OnInventoryChanged += RefreshInventoryDisplay;

        RefreshInventoryDisplay();
    }
    void Update()
    {
        // ONLY RUN IF CAMPSITE UI IS OPEN
        if (!gameObject.activeInHierarchy)
            return;

        // CLOSE CAMP
        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            CampArea camp = FindFirstObjectByType<CampArea>();

            if (camp != null)
            {
                StartCoroutine(camp.ExitCampRoutine());
            }

            ClearSelectedItem();
        }
    }

    void CloseCampsite()
    {
        StartCoroutine(ExitCamp());
    }

    IEnumerator ExitCamp()
    {
        ClearSelectedItem();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Time.timeScale = 1f;

        PlayerController player = FindFirstObjectByType<PlayerController>();

        if (player != null)
            player.enabled = true;

        CampArea campArea = FindFirstObjectByType<CampArea>();

        if (campArea != null)
        {
            yield return StartCoroutine(campArea.ExitCampRoutine());
        }

        gameObject.SetActive(false);
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
                slot.parentUI = this; // ✅ VERY IMPORTANT
                gridSlots[x, y] = slot;
            }
        }
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

    private void OnEnable()
    {
        StartCoroutine(WaitForInventory());
    }
    private void OnDisable()
    {
        if (CampsiteInventory.Instance != null)
            CampsiteInventory.Instance.OnInventoryChanged -= RefreshInventoryDisplay;
    }
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
