using UnityEngine;
using System.Collections.Generic;

public class PlayerAttackRadius : MonoBehaviour
{
    public List<BaseEnemy> detectedEnemies = new List<BaseEnemy>();

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
            Debug.Log($"[AttackRadius] Enemy detected: {enemy.name}");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        BaseEnemy enemy = other.GetComponent<BaseEnemy>();
        if (enemy != null && detectedEnemies.Contains(enemy))
        {
            detectedEnemies.Remove(enemy);
            Debug.Log($"[AttackRadius] Enemy left detection: {enemy.name}");
        }
    }

    private void Update()
    {
        // ✅ Remove null/destroyed enemies from the list
        detectedEnemies.RemoveAll(enemy => enemy == null);

        if (detectedEnemies.Count > 0)
        {
            string enemyNames = string.Join(", ", detectedEnemies.ConvertAll(e => e != null ? e.name : "Destroyed"));
            Debug.Log($"[AttackRadius] Enemies currently detected: {enemyNames}");
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
