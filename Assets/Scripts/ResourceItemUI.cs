using UnityEngine;
using UnityEngine.EventSystems;

public class ResourceItemUI : MonoBehaviour, IPointerClickHandler
{
    public Item item;
    public int amount;
    private ResourceInteraction parent;

    public void Setup(Item i, int amt, ResourceInteraction p)
    {
        item = i;
        amount = amt;
        parent = p;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.clickCount == 2)
        {
            TransferSingle();
        }
    }

    void TransferSingle()
    {
        if (PlayerInventory.Instance == null)
        {
            Debug.LogError("❌ PlayerInventory is NULL");
            return;
        }

        if (item == null)
        {
            Debug.LogError("❌ ResourceItemUI has NULL item!");
            return;
        }

        if (parent == null)
        {
            Debug.LogError("❌ ResourceItemUI has NULL parent!");
            return;
        }

        // TRY ADDING ITEM
        bool added = PlayerInventory.Instance.AddItem(item, 1);

        // INVENTORY FULL = STOP
        if (!added)
        {
            Debug.Log("Inventory full - item stays in loot");
            return;
        }

        // ONLY REMOVE IF SUCCESSFULLY ADDED
        amount--;

        parent.RemoveItem(item, 1);

        if (amount <= 0)
        {
            Destroy(gameObject);
        }
        else
        {
            var text = transform.Find("ItemAmount")
                ?.GetComponent<TMPro.TextMeshProUGUI>();

            if (text != null)
                text.text = "x" + amount;
        }
    }
}