using System;
using UnityEngine;

public class BreakableObject : MonoBehaviour
{
    public static event Action OnAnyBreakableDestroyed;

    public int health = 30;

    private PuzzleBreakable pb;

    private void Awake()
    {
        pb = GetComponent<PuzzleBreakable>();
    }

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
        // 🔥 PUZZLE CHECK
        if (pb != null)
        {
            pb.PuzzleDestroyed();
        }

        OnAnyBreakableDestroyed?.Invoke();

        Destroy(gameObject);
    }
}