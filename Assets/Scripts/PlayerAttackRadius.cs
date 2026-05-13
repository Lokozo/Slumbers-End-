using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackRadius : MonoBehaviour
{
    public List<BaseEnemy> detectedEnemies = new List<BaseEnemy>();
    public List<BreakableObject> detectedBreakables = new List<BreakableObject>();

    [Header("Detection")]
    public float radius = 2f;

    private void Update()
    {
        detectedEnemies.Clear();
        detectedBreakables.Clear();

        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            radius
        );

        foreach (Collider hit in hits)
        {
            // IGNORE SELF
            if (hit.transform.root == transform.root)
                continue;

            string layerName =
                LayerMask.LayerToName(hit.gameObject.layer);

            // =========================
            // ENEMY
            // =========================

            bool isEnemyLayer = layerName == "Enemy";

            bool isEnemyTag =
                hit.CompareTag("Enemy") ||
                hit.transform.root.CompareTag("Enemy");

            if (isEnemyLayer || isEnemyTag)
            {
                BaseEnemy enemy =
                    hit.GetComponentInParent<BaseEnemy>();

                if (enemy != null &&
                    !detectedEnemies.Contains(enemy))
                {
                    detectedEnemies.Add(enemy);

                    Debug.Log("[RADIUS] Enemy detected: "
                              + enemy.name);
                }
            }

            // =========================
            // BREAKABLE
            // =========================

            if (layerName == "Breakables")
            {
                BreakableObject breakable =
                    hit.GetComponentInParent<BreakableObject>();

                if (breakable != null &&
                    !detectedBreakables.Contains(breakable))
                {
                    detectedBreakables.Add(breakable);

                    Debug.Log("[RADIUS] Breakable detected: "
                              + breakable.name);
                }
            }
        }
    }

    // =========================================================
    // ✅ ADDED: CLOSEST ENEMY SELECTION (NO CHANGES ABOVE)
    // =========================================================

    public BaseEnemy GetClosestEnemy()
    {
        BaseEnemy closest = null;
        float closestDist = float.MaxValue;

        Vector3 origin = transform.position;

        for (int i = 0; i < detectedEnemies.Count; i++)
        {
            BaseEnemy enemy = detectedEnemies[i];
            if (enemy == null) continue;

            float dist = (enemy.transform.position - origin).sqrMagnitude;

            if (dist < closestDist)
            {
                closestDist = dist;
                closest = enemy;
            }
        }

        return closest;
    }

    // =========================================================
    // ✅ ADDED: ATTACK FUNCTION (CALL THIS)
    // =========================================================

    public void TryAttackClosest(int damage)
    {
        BaseEnemy target = GetClosestEnemy();

        if (target != null)
        {
            target.TakeDamage(damage);
            Debug.Log("[ATTACK] Hit closest enemy: " + target.name);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}

/*
public class PlayerAttackRadius : MonoBehaviour
{
    public List<BaseEnemy> detectedEnemies = new List<BaseEnemy>();
    public List<BreakableObject> detectedBreakables = new List<BreakableObject>();

    [Header("Detection")]
    public float radius = 2f;

    private void Update()
    {
        detectedEnemies.Clear();
        detectedBreakables.Clear();

        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            radius
        );

        foreach (Collider hit in hits)
        {
            // IGNORE SELF
            if (hit.transform.root == transform.root)
                continue;

            string layerName =
                LayerMask.LayerToName(hit.gameObject.layer);

            // =========================
            // ENEMY
            // =========================

            bool isEnemyLayer = layerName == "Enemy";

            bool isEnemyTag =
                hit.CompareTag("Enemy") ||
                hit.transform.root.CompareTag("Enemy");

            if (isEnemyLayer || isEnemyTag)
            {
                BaseEnemy enemy =
                    hit.GetComponentInParent<BaseEnemy>();

                if (enemy != null &&
                    !detectedEnemies.Contains(enemy))
                {
                    detectedEnemies.Add(enemy);

                    Debug.Log("[RADIUS] Enemy detected: "
                              + enemy.name);
                }
            }

            // =========================
            // BREAKABLE
            // =========================

            if (layerName == "Breakables")
            {
                BreakableObject breakable =
                    hit.GetComponentInParent<BreakableObject>();

                if (breakable != null &&
                    !detectedBreakables.Contains(breakable))
                {
                    detectedBreakables.Add(breakable);

                    Debug.Log("[RADIUS] Breakable detected: "
                              + breakable.name);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
*/