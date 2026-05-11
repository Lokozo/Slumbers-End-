using UnityEngine;
using System.Collections.Generic;

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
            if (layerName == "Enemy")
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