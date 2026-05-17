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

        iconImage.sprite = null;
        iconImage.enabled = false;

        quantityText.text = "";
    }

    public void SetSlot(Item item, int quantity, SlotContextType context)
    {
        currentItem = item;
        currentQuantity = quantity;
        contextType = context;

        iconImage.sprite = item.icon;
        iconImage.enabled = true;

        if (item is WeaponItem weapon)
        {
            // 🔥 FIX: Use the weapon's isEquipped flag directly
            quantityText.text = weapon.isEquipped ? "Equipped" : "";
            Debug.Log($"Setting slot for {weapon.itemName}: isEquipped={weapon.isEquipped}");
        }
        else
        {
            quantityText.text = quantity.ToString();
        }
    }

    private void OnClick()
    {
        if (currentItem == null) return;

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
        if (parentUI is InventoryUI invUI)
            invUI.RefreshUI();
        else if (parentUI is CampsiteInventoryUI campUI)
            campUI.RefreshInventoryDisplay();

        FindFirstObjectByType<InventoryUI>()?.RefreshUI();
        FindFirstObjectByType<CampsiteInventoryUI>()?.RefreshInventoryDisplay();
    }
    private void TransferOne()
    {
        if (currentItem == null) return;

        Item transferItem = currentItem;

        if (transferItem is WeaponItem w && w.isEquipped)
        {
            Debug.Log("Cannot transfer equipped weapon!");
            return;
        }

        if (contextType == SlotContextType.Inventory)
        {
            CampsiteInventory.Instance.AddItem(transferItem, 1);
            PlayerInventory.Instance.RemoveItem(transferItem, 1);
        }
        else if (contextType == SlotContextType.Campsite)
        {
            PlayerInventory.Instance.AddItem(transferItem, 1);
            CampsiteInventory.Instance.RemoveItem(transferItem, 1);
        }

        RefreshUI();
    }
}