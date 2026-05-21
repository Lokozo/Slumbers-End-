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
            }
            else
            {
                quantityText.text = quantity.ToString();
            }
        }
    }

    private void Update()
    {
        // PRESS E TO USE SELECTED CONSUMABLE
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (currentItem != null &&
                currentItem.isConsumable &&
                contextType == SlotContextType.Inventory)
            {
                PlayerInventory.Instance.UseItem(currentItem);

                Debug.Log("Consumed: " + currentItem.itemName);
            }
        }
    }

    private void OnClick()
    {
        if (currentItem == null)
        {
            return;
        }

        float timeSinceLastClick = Time.unscaledTime - lastClickTime;

        // DOUBLE CLICK
        if (timeSinceLastClick <= doubleClickTime)
        {
            // 🔥 USE CONSUMABLES
            if (currentItem.isConsumable)
            {
                // ONLY USE FROM PLAYER INVENTORY
                if (contextType == SlotContextType.Inventory)
                {
                    Item usedItem = currentItem;

                    PlayerInventory.Instance.UseItem(usedItem);

                    Debug.Log("Consumed: " + usedItem.itemName);

                    return;
                }
            }
            else
            {
                // TRANSFER NON-CONSUMABLES
                TransferOne();
            }
        }
        else
        {
            // SINGLE CLICK = SELECT ITEM

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

        // CHECK EQUIPPED WEAPON
        if (transferItem is WeaponItem w && w.isEquipped)
        {
            Debug.Log("Cannot transfer equipped weapon!");
            return;
        }

        // 🔥 CHECK CAMP FIRST
        CampArea camp = FindFirstObjectByType<CampArea>();

        if (camp == null || !camp.IsInCamp())
        {
            Debug.Log("⚠️ Cannot transfer items outside campsite!");
            return;
        }

        if (contextType == SlotContextType.Inventory)
        {
            // REMOVE FIRST
            bool removed =
                PlayerInventory.Instance.RemoveItem(transferItem, 1);

            // ONLY ADD IF REMOVE SUCCEEDED
            if (removed)
            {
                CampsiteInventory.Instance.AddItem(transferItem, 1);
            }
        }
        else if (contextType == SlotContextType.Campsite)
        {
            bool removed =
                CampsiteInventory.Instance.RemoveItem(transferItem, 1);

            if (removed)
            {
                PlayerInventory.Instance.AddItem(transferItem, 1);
            }
        }

        RefreshUI();
    }
}