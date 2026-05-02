using UnityEngine;

public class EnemyAttackRadius : MonoBehaviour
{
    public PlayerHealth playerInRange;

    private SphereCollider sphereCollider;

    private void Awake()
    {
        sphereCollider = GetComponent<SphereCollider>();

        if (sphereCollider == null)
        {
            Debug.LogWarning("[EnemyAttackRadius] No SphereCollider found.");
        }
        else if (!sphereCollider.isTrigger)
        {
            sphereCollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = other.GetComponent<PlayerHealth>();

        if (playerInRange == null)
            playerInRange = other.GetComponentInChildren<PlayerHealth>();

        if (playerInRange == null)
            playerInRange = other.GetComponentInParent<PlayerHealth>();

        if (playerInRange == null)
        {
            Debug.LogError("[EnemyAttackRadius] PlayerHealth NOT FOUND on player!");
            return;
        }

        Debug.Log("[EnemyAttackRadius] Player entered attack range");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = null;

        Debug.Log("[EnemyAttackRadius] Player left attack range");
    }

    private void OnDrawGizmos()
    {
        if (sphereCollider == null)
            sphereCollider = GetComponent<SphereCollider>();

        if (sphereCollider != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, sphereCollider.radius);
        }
    }
}