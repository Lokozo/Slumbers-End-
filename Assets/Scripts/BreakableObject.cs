using UnityEngine;

public class BreakableObject : MonoBehaviour, IDamageable
{
    public float health = 30f;
    public static event System.Action OnAnyBreakableDestroyed;

    public void TakeDamage(float damage)
    {
        health -= damage;

        if (health <= 0)
        {
            DestroyObject();
        }
    }

    void DestroyObject()
    {
        Debug.Log("Fence destroyed!");

        OnAnyBreakableDestroyed?.Invoke();
        // Optional: particles / sound
        Destroy(gameObject);
    }

}