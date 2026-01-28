using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;

    [Header("Max Values")]
    public float maxHealth = 100f;
    public float maxHunger = 100f;
    public float maxEnergy = 100f;

    [Header("Current Values")]
    public float health;
    public float hunger;
    public float energy;

    [Header("Energy Recovery Settings")]
    public float energyRecoverRate = 5f; // How much energy to recover per second

    [Header("Passive Drain Settings")]
    public float hungerDrainAmount = 5f;
    public float hungerDrainInterval = 20f;
    private float hungerTimer;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

<<<<<<< Updated upstream
=======
           

>>>>>>> Stashed changes
            DontDestroyOnLoad(gameObject);
            InitializeIfNeeded();
        }
        else
        {
            Destroy(gameObject);
        }
<<<<<<< Updated upstream
        health = maxHealth;
        hunger = maxHunger;
        energy = maxEnergy;

    }

    private void InitializeIfNeeded()
    {
        if (health <= 0f) health = maxHealth;
        if (hunger <= 0f) hunger = maxHunger;
        if (energy <= 0f) energy = maxEnergy;
=======
>>>>>>> Stashed changes

        health = maxHealth;
        hunger = maxHunger;
        energy = maxEnergy;
    }

    void InitializeIfNeeded()
    {
        if (health <= 0f) health = maxHealth;
        if (hunger <= 0f) hunger = maxHunger;
        if (energy <= 0f) energy = maxEnergy;
    }

    void Update()
    {
        // 1. Passive Hunger Drain
        HandleHungerDrain();

        // 2. Energy Recovery
        // This will only run if energy is less than max.
        // It will recover energy "non-permanently" because it clamps at maxEnergy.
        if (energy < maxEnergy)
        {
            // We multiply by Time.deltaTime to recover per second, not per frame
            ModifyEnergy(energyRecoverRate * Time.deltaTime);
        }
    }

    private void HandleHungerDrain()
    {
        hungerTimer += Time.deltaTime;
        if (hungerTimer >= hungerDrainInterval)
        {
            ModifyHunger(-hungerDrainAmount);
            Debug.Log($"<color=orange>Passive Hunger Drain:</color> Current Hunger: {hunger}");
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
}