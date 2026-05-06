using UnityEngine;
using System.Collections.Generic;

public class PlayerAttackRadius : MonoBehaviour
{
    public List<BaseEnemy> detectedEnemies = new List<BaseEnemy>();

    public List<BreakableObject> detectedBreakables = new List<BreakableObject>();

    private SphereCollider sphereCollider;

    private void Awake()
    {
        sphereCollider = GetComponent<SphereCollider>();

        if (sphereCollider == null)
        {
            Debug.LogWarning("[AttackRadius] No SphereCollider found on this object.");
        }
        else if (!sphereCollider.isTrigger)
        {
            sphereCollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        BaseEnemy enemy = other.GetComponent<BaseEnemy>();

        if (enemy != null && !detectedEnemies.Contains(enemy))
        {
            detectedEnemies.Add(enemy);
        }

        BreakableObject breakable = other.GetComponent<BreakableObject>();

        if (breakable != null && !detectedBreakables.Contains(breakable))
        {
            detectedBreakables.Add(breakable);

            Debug.Log("[ATTACK RADIUS] Breakable detected: " + breakable.name);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        BaseEnemy enemy = other.GetComponent<BaseEnemy>();

        if (enemy != null && detectedEnemies.Contains(enemy))
        {
            detectedEnemies.Remove(enemy);
        }

        BreakableObject breakable = other.GetComponent<BreakableObject>();

        if (breakable != null && detectedBreakables.Contains(breakable))
        {
            detectedBreakables.Remove(breakable);

            Debug.Log("[ATTACK RADIUS] Breakable removed: " + breakable.name);
        }
    }

    private void Update()
    {
        // ✅ Remove null/destroyed enemies from the list
        detectedEnemies.RemoveAll(enemy => enemy == null);

        if (detectedEnemies.Count > 0)
        {
            string enemyNames = string.Join(", ", detectedEnemies.ConvertAll(e => e != null ? e.name : "Destroyed"));
        }
    }

    private void OnDrawGizmos()
    {
        if (sphereCollider == null)
            sphereCollider = GetComponent<SphereCollider>();

        if (sphereCollider != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, sphereCollider.radius);
        }
    }
}
