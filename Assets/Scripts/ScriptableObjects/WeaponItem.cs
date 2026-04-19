using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Inventory/Weapon")]
public class WeaponItem : Item
{
    [Header("Type")]
    public WeaponType weaponType;

    public enum WeaponType
    {
        Melee,
        Ranged
    }

    [Header("Combat Stats")]
    public float damage;
    public float attackRange;
    public float attackSpeed;

    [Header("Ranged Only")]
    public int maxAmmo;
    public float reloadTime;
}