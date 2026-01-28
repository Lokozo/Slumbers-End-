using UnityEngine;

public class GunController : MonoBehaviour
{
    public WeaponItem gunData; // Drag your Pistol ScriptableObject here

    public void Fire()
    {
        Debug.Log($"Firing {gunData.itemName} for {gunData.damage} damage!");
        // Play sound, spawn bullet, etc.
    }
}