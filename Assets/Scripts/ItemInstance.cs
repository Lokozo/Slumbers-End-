[System.Serializable]
public class ItemInstance
{
    public Item itemData;
    public int quantity;

    public int currentUses;

    public ItemInstance(Item item, int qty)
    {
        itemData = item;
        quantity = qty;
        currentUses = item.maxUses;
    }
}