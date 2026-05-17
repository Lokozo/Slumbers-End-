using UnityEngine;
using static PlayerAttack;

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
    public bool isEquipped;

    [Header("Ammo - Set this for AMMO ITEMS")]
    public WeaponItem.AmmoType ammoType;

    [Header("Stack & Durability")]
    public int maxStack = 1;
    public int maxUses = 1;

    public int width = 1;
    public int height = 1;

    [Header("Consumable Effects")]
    public bool isConsumable;
    public float healthRestoreAmount;
    public float hungerRestoreAmount;
    public float energyRestoreAmount;

    public enum ItemType
    {
        Material,
        Consumable,
        QuestItem,
        Equipment
    }
}