using System;
using UnityEngine;

public class BreakableObject : MonoBehaviour
{
    public static event Action OnAnyBreakableDestroyed;

    public int health = 30;

    public void TakeDamage(int damage)
    {
        health -= damage;

        if (health <= 0)
        {
            DestroyObject();
        }
    }

    private void DestroyObject()
    {
        OnAnyBreakableDestroyed?.Invoke();

        Destroy(gameObject);
    }
}