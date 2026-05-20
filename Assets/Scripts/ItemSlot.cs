using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemSlot : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private Button button;

    private Item currentItem;
    private int currentQuantity;
    private SlotContextType contextType;

    private float lastClickTime = 0f;
    private float doubleClickTime = 0.3f;

    public MonoBehaviour parentUI;

    private void Awake()
    {
        if (button != null)
            button.onClick.AddListener(OnClick);
    }

    public void ClearSlot()
    {
        currentItem = null;
        currentQuantity = 0;

        // FIX: Add null checks before accessing components
        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }

        if (quantityText != null)
        {
            quantityText.text = "";
        }
    }

    public void SetSlot(Item item, int quantity, SlotContextType context)
    {
        // FIX: Add null check for item
        if (item == null)
        {
            Debug.LogWarning("ItemSlot.SetSlot received null item!");
            ClearSlot();
            return;
        }

        currentItem = item;
        currentQuantity = quantity;
        contextType = context;

        // FIX: Add null checks for UI components
        if (iconImage != null)
        {
            iconImage.sprite = item.icon;
            iconImage.enabled = true;
        }

        if (quantityText != null)
        {
            if (item is WeaponItem weapon)
            {
                quantityText.text = weapon.isEquipped ? "Equipped" : "";
            }
            else
            {
                quantityText.text = quantity.ToString();
            }
        }
    }

    private void Update()
    {
        // FIX: Handle right-click in Update instead of OnClick
        // This ensures proper input detection
        if (Input.GetMouseButtonDown(1) && currentItem != null && currentItem.isConsumable)
        {
            if (PlayerInventory.Instance != null)
            {
                PlayerInventory.Instance.UseItem(currentItem);
            }
        }
    }

    private void OnClick()
    {
        // FIX: Add null check for currentItem
        if (currentItem == null)
        {
            Debug.LogWarning("OnClick called but currentItem is null!");
            return;
        }

        Debug.Log("Clicked Item: " + currentItem.itemName);

        float timeSinceLastClick = Time.unscaledTime - lastClickTime;

        if (timeSinceLastClick <= doubleClickTime)
        {
            TransferOne();
        }
        else
        {
            // ONLY SELECT ITEM

            if (parentUI is InventoryUI inventoryUI)
            {
                inventoryUI.SetSelectedItem(currentItem);
            }
            else if (parentUI is CampsiteInventoryUI campsiteUI)
            {
                campsiteUI.SetSelectedItem(currentItem);
            }

            // SAVE LAST CLICKED WEAPON
            if (currentItem is WeaponItem weaponItem)
            {
                PlayerController player = FindFirstObjectByType<PlayerController>();

                if (player != null)
                {
                    player.SetLastClickedWeapon(weaponItem);
                    Debug.Log("Selected weapon: " + weaponItem.itemName);
                }
            }
        }

        lastClickTime = Time.unscaledTime;
    }

    private void RefreshUI()
    {
        // FIX: Add null checks
        if (parentUI is InventoryUI invUI)
        {
            invUI.RefreshUI();
        }
        else if (parentUI is CampsiteInventoryUI campUI)
        {
            campUI.RefreshInventoryDisplay();
        }

        InventoryUI inventoryUIInstance = FindFirstObjectByType<InventoryUI>();
        if (inventoryUIInstance != null)
        {
            inventoryUIInstance.RefreshUI();
        }

        CampsiteInventoryUI campsiteUIInstance = FindFirstObjectByType<CampsiteInventoryUI>();
        if (campsiteUIInstance != null)
        {
            campsiteUIInstance.RefreshInventoryDisplay();
        }
    }

    private void TransferOne()
    {
        if (currentItem == null) return;

        Item transferItem = currentItem;

        // FIX: Check if item is equipped before transferring
        if (transferItem is WeaponItem w && w.isEquipped)
        {
            Debug.Log("Cannot transfer equipped weapon!");
            return;
        }

        // FIX: Add null checks for instances
        if (contextType == SlotContextType.Inventory)
        {
            if (CampsiteInventory.Instance != null)
                CampsiteInventory.Instance.AddItem(transferItem, 1);

            if (PlayerInventory.Instance != null)
                PlayerInventory.Instance.RemoveItem(transferItem, 1);
        }
        else if (contextType == SlotContextType.Campsite)
        {
            if (PlayerInventory.Instance != null)
                PlayerInventory.Instance.AddItem(transferItem, 1);

            if (CampsiteInventory.Instance != null)
                CampsiteInventory.Instance.RemoveItem(transferItem, 1);
        }

        RefreshUI();
    }
}