using UnityEngine;

[CreateAssetMenu(menuName = "Items/Consumable")]
public class ConsumableItem : ScriptableObject
{
    public string itemName;
    public Sprite icon;

    [Header("Buff Values")]
    public float healthRestore;
    public float hungerRestore;
    public float energyRestore;

    [Header("Buff Duration (Optional)")]
    public float duration; // If it's a timed buff, else leave 0
}
