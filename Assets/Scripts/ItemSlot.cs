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

    [HideInInspector] public InventoryUI parentUI;

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
        if (item.isEquipped)
        {
            quantityText.text = "Equiped"; // or "Equipped"
        }
        else
        {
            quantityText.text = quantity > 1 ? quantity.ToString() : "";
        }
    }

    private void OnClick()
    {
        if (parentUI != null && currentItem != null)
        {
            parentUI.SetSelectedItem(currentItem);
        }
    }
}