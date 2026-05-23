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

    [Header("Temporary Energy Buff")]
    public float bonusRecoveryRate = 0f;

    private float recoveryBuffTimer = 0f;
    // 🔥 REMOVED: static Instance - Bootstrapper handles singleton behavior

    private void Awake()
    {
        InitializeIfNeeded();
    }

    public void Initialize()
    {
        InitializeIfNeeded();
    }

    public void InitializeIfNeeded()
    {
        health = maxHealth;
        hunger = maxHunger;
        energy = maxEnergy;
    }

    void Update()
    {
        HandleHungerDrain();

        if (recoveryBuffTimer > 0)
        {
            recoveryBuffTimer -= Time.deltaTime;
        }
        else
        {
            bonusRecoveryRate = 0f;
        }

        float totalRecovery = energyRecoverRate + bonusRecoveryRate;

        if (energy < maxEnergy)
        {
            ModifyEnergy(totalRecovery * Time.deltaTime);
        }
    }
    public void AddEnergyRecoveryBuff(float bonusAmount, float duration)
    {
        bonusRecoveryRate = bonusAmount;
        recoveryBuffTimer = duration;

        Debug.Log("Energy Recovery Buff Applied!");
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
        health += amount;
        health = Mathf.Clamp(health, 0f, maxHealth);
    }

    public void ModifyHunger(float amount)
    {
        hunger += amount;
        hunger = Mathf.Clamp(hunger, 0f, maxHunger);
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