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

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeIfNeeded();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeIfNeeded()
    {
        if (health <= 0f) health = maxHealth;
        if (hunger <= 0f) hunger = maxHunger;
        if (energy <= 0f) energy = maxEnergy;
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
