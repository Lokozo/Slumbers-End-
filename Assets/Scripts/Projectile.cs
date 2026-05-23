using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float damage = 10f;
    public float lifetime = 5f;

    [Header("Effects")]
    [SerializeField] private GameObject hitEffectPrefab;

    [HideInInspector] public Transform owner;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Ignore owner
        if (owner != null && other.transform.root == owner)
            return;

        // Ignore enemies
        if (other.CompareTag("Enemy"))
            return;

        if (other.CompareTag("Player"))
        {
            PlayerHealth hp =
                other.GetComponentInParent<PlayerHealth>();

            if (hp != null)
            {
                hp.TakeDamage((int)damage);

                PlayHitFX(transform.position);

                Destroy(gameObject);
            }
        }
    }

    private void PlayHitFX(Vector3 position)
    {
        if (hitEffectPrefab == null)
            return;

        GameObject fx = Instantiate(
            hitEffectPrefab,
            position,
            transform.rotation
        );

        ParticleSystem ps =
            fx.GetComponentInChildren<ParticleSystem>();

        if (ps != null)
        {
            ps.Clear();
            ps.Play();

            float duration =
                ps.main.duration +
                ps.main.startLifetime.constantMax;

            Destroy(fx, duration);
        }
        else
        {
            Destroy(fx, 2f);
        }
    }
}