using UnityEngine;
using System.Collections;

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

    [Header("Ranged Only - AMMO SYSTEM")]
    public AmmoType requiredAmmoType;     // ✅ What ammo this weapon uses
    public int ammoPerShot = 1;           // ✅ How much ammo per shot (ONLY HERE)
    public float reloadTime = 2f;


    [Header("Effects")]
    public GameObject fireEffects;

    [Header("Audio")]
    //public AudioSource fireAudio;
    public AudioClip[] fireClips;
    [Range(0, 1)] public float volume = 1f;

    public enum AmmoType  // ✅ Keep enum here
    {
        None,      // Melee weapons
        Light,     // Pistol ammo
        Medium,    // Rifle/AK ammo  
        Shotgun    // Shotgun shells
    }
}