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
        if (PlayerInventory.Instance == null) return;

        PlayerInventory.Instance.AddItem(item, 1);
        amount--;

        parent.RemoveItem(item, 1); // ✅ FIX

        if (amount <= 0)
        {
            Destroy(gameObject);
        }
        else
        {
            transform.Find("ItemAmount")
                .GetComponent<TMPro.TextMeshProUGUI>().text = "x" + amount;
        }
    }
}