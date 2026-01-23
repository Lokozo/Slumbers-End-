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

    //  Link to parent InventoryUI
    [HideInInspector] public InventoryUI parentUI;

    private void Awake()
    {
        if (button != null)
            button.onClick.AddListener(OnClick);
    }

    

    public void SetSlot(Item item, int quantity, SlotContextType context)
    {
        currentItem = item;
        currentQuantity = quantity;
        contextType = context;

        iconImage.sprite = item.icon;
        iconImage.enabled = true;
        quantityText.text = quantity > 1 ? quantity.ToString() : "";
    }

    private void OnClick()
    {
        //Debug.Log($"Clicked on {currentItem?.itemName} x{currentQuantity}");
        if (parentUI != null && currentItem != null)
        {
            parentUI.ShowItemDescription(currentItem);

            parentUI.SetSelectedItem(currentItem);
        }
    }
}
