using UnityEngine;

public class BreakableObject : MonoBehaviour
{
    public float health = 30f;

    public void TakeDamage(float damage)
    {
        health -= damage;

        Debug.Log(name + " took damage");

        if (health <= 0)
        {
            DestroyObject();
        }
    }

    void DestroyObject()
    {
        Debug.Log(name + " destroyed!");

        Destroy(gameObject);
    }
}