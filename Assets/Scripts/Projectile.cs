using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float damage = 10f;
    public float lifetime = 5f;

    public Transform owner;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // ignore owner completely
        if (owner != null && other.transform.root == owner)
            return;

        // ignore ALL enemies
        if (other.CompareTag("Enemy"))
            return;


        if (other.CompareTag("Player"))
        {
            PlayerHealth hp = other.GetComponent<PlayerHealth>();
            if (hp != null)
            {
                hp.TakeDamage((int)damage);
                Destroy(gameObject);
            }
        }
    }
}
