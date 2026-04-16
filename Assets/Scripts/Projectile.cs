using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float damage = 10f;
    public float lifetime = 5f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerHealth hp = other.GetComponent<PlayerHealth>();

        if (hp != null)
        {
            hp.TakeDamage((int)damage);
        }

        Destroy(gameObject);
    }
}