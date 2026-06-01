using TMPro;
using UnityEngine;

public class AmmoUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject ammoPanel;
    public TMP_Text ammoText;

    [Header("References")]
    public PlayerAttack playerAttack;

    private void Update()
    {
        // No weapon equipped
        if (playerAttack == null ||
            playerAttack.currentWeaponData == null)
        {
            //ammoPanel.SetActive(false);
            return;
        }

        WeaponItem weapon = playerAttack.currentWeaponData;

        // Hide ammo UI for melee weapons
        if (weapon.weaponType != WeaponItem.WeaponType.Ranged)
        {
            //ammoPanel.SetActive(false);
            return;
        }

        // Show for ranged weapons
        ammoPanel.SetActive(true);

        // Find ammo item
        Item ammoItem = FindAmmoItem(weapon.requiredAmmoType);

        int ammoCount = 0;

        if (ammoItem != null)
        {
            var inventory = PlayerInventory.Instance.GetInventory();

            if (inventory.ContainsKey(ammoItem))
            {
                ammoCount = inventory[ammoItem];
            }
        }

        ammoText.text = $"Ammo: {ammoCount}";
    }

    private Item FindAmmoItem(WeaponItem.AmmoType ammoType)
    {
        foreach (var kvp in PlayerInventory.Instance.GetInventory())
        {
            Item item = kvp.Key;

            if (item.ammoType == ammoType)
            {
                return item;
            }
        }

        return null;
    }
}