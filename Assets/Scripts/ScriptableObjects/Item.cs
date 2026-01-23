using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public string itemName;
    [TextArea(2, 5)]
    public string description;
    public Sprite icon;
    public ItemType itemType;

    public bool isCrafted;        
    public bool isUsedInCrafting;

    [Header("Consumable Effects")]
    public bool isConsumable;
    public float healthRestoreAmount;
    public float hungerRestoreAmount;
    public float energyRestoreAmount;
    public enum ItemType
    {
        Material,      // Basic crafting ingredients (wood, stone, cloth)
        Consumable,    // Items that can be used (berries, potions, bandages)
        QuestItem,    // Items related to quests (keys, artifacts)
        //Tool,          // Tools like hammer, knife, etc.
        //Equipment,     // Optional: Wearables like clothes or armor
        //QuestItem,     // Optional: If you have story-related items
        //UpgradePart    // Items used to enhance or upgrade tools
    }

    public override bool Equals(object obj)
    {
        if (obj is Item other)
        {
            return itemName == other.itemName;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return itemName.GetHashCode();
    }
}
