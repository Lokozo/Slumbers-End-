using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Max Values")]
    public float maxHealth = 100f;
    public float maxHunger = 100f;
    public float maxEnergy = 100f;

    [Header("Current Values")]
    public float health;
    public float hunger;
    public float energy;

    [Header("Energy Recovery Settings")]
    public float energyRecoverRate = 5f;

    [Header("Passive Drain Settings")]
    public float hungerDrainAmount = 5f;
    public float hungerDrainInterval = 20f;
    private float hungerTimer;

    // 🔥 REMOVED: static Instance - Bootstrapper handles singleton behavior

    private void Awake()
    {
        // Don't destroy - parent ManagerBootstrapper handles this
        InitializeIfNeeded();
    }

    public void Initialize()
    {
        InitializeIfNeeded();
    }

    public void InitializeIfNeeded()
    {
        health = Mathf.Max(health, maxHealth);
        hunger = Mathf.Max(hunger, maxHunger);
        energy = Mathf.Max(energy, maxEnergy);
    }

    void Update()
    {
        HandleHungerDrain();

        if (energy < maxEnergy)
        {
            ModifyEnergy(energyRecoverRate * Time.deltaTime);
        }
    }

    private void HandleHungerDrain()
    {
        hungerTimer += Time.deltaTime;
        if (hungerTimer >= hungerDrainInterval)
        {
            ModifyHunger(-hungerDrainAmount);
            hungerTimer = 0f;
        }
    }

    public void ModifyHealth(float amount)
    {
        health = Mathf.Clamp(health + amount, 0f, maxHealth);
    }

    public void ModifyHunger(float amount)
    {
        hunger = Mathf.Clamp(hunger + amount, 0f, maxHunger);
    }

    public void ModifyEnergy(float amount)
    {
        energy = Mathf.Clamp(energy + amount, 0f, maxEnergy);
    }

    // 🔥 STATIC ACCESSOR - Use this everywhere instead of Instance
    public static PlayerStats Get()
    {
        var bootstrapper = Object.FindObjectOfType<ManagerBootstrapper>();
        if (bootstrapper != null)
        {
            var stats = bootstrapper.GetComponentInChildren<PlayerStats>();
            if (stats != null) return stats;
        }
        return null;
    }
}