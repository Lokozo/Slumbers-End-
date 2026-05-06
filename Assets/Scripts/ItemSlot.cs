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

        if (item is WeaponItem)
        {
            if (item.isEquipped)
                quantityText.text = "Equipped";
            else
                quantityText.text = "Equipped"; // 🔥 ADD THIS
        }
        else
        {
            quantityText.text = quantity > 1 ? quantity.ToString() : "";
        }
    }

    private void OnClick()
    {
        if (currentItem == null) return;

        float timeSinceLastClick = Time.unscaledTime - lastClickTime;

        if (timeSinceLastClick <= doubleClickTime)
        {
            // DOUBLE CLICK → transfer
            TransferOne();
        }
        else
        {
            // SINGLE CLICK → select
            if (parentUI is InventoryUI invUI)
            {
                invUI.SetSelectedItem(currentItem);
            }
            else if (parentUI is CampsiteInventoryUI campUI)
            {
                campUI.SetSelectedItem(currentItem);
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

        // 🔥 BLOCK IF EQUIPPED
        if (currentItem is WeaponItem w && w.isEquipped)
        {
            Debug.Log("Cannot transfer equipped weapon!");
            return;
        }

        if (contextType == SlotContextType.Inventory)
        {
            PlayerInventory.Instance.RemoveItem(currentItem, 1);
            CampsiteInventory.Instance.AddItem(currentItem, 1);
        }
        else if (contextType == SlotContextType.Campsite)
        {
            CampsiteInventory.Instance.RemoveItem(currentItem, 1);
            PlayerInventory.Instance.AddItem(currentItem, 1);
        }

        RefreshUI();
    }
}