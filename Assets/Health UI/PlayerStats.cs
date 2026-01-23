using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public float maxHealth = 100f;
    public float maxHunger = 100f;
    public float maxEnergy = 100f;

    public float health;
    public float hunger;
    public float energy;

    void Start()
    {
        health = maxHealth;
        hunger = maxHunger;
        energy = maxEnergy;
    }

    public void ModifyHealth(float amount)
    {
        health = Mathf.Clamp(health + amount, 0f, maxHealth);
        Debug.Log($"Health: {health}");
    }

    public void ModifyHunger(float amount)
    {
        hunger = Mathf.Clamp(hunger + amount, 0f, maxHunger);
        Debug.Log($"Hunger: {hunger}");
    }

    public void ModifyEnergy(float amount)
    {
        energy = Mathf.Clamp(energy + amount, 0f, maxEnergy);
        Debug.Log($"Energy: {energy}");
    }
}
