using UnityEngine;

[System.Serializable]
public class ItemDropData
{
    public Item item;
    [Range(0f, 1f)] public float dropChance = 1f; // Drop chance from 0.0 to 1.0
    public int minAmount = 1;
    public int maxAmount = 3;
}
    