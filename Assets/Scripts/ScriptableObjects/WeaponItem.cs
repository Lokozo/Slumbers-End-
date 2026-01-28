using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Inventory/Weapon")]
public class WeaponItem : Item // Inherits from your Item script
{
    [Header("Combat Stats")]
    public float damage;
    public float attackRange;
    public float attackSpeed;

    [Header("Ranged Only")]
    public bool isRanged;
    public int maxAmmo;
    public float reloadTime;
}