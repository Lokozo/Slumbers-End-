using UnityEngine;

public class EnemyDetection : MonoBehaviour
{
    private BaseEnemy enemy;

    void Start()
    {
        enemy = GetComponentInParent<BaseEnemy>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (enemy == null) return;

        enemy.SetPlayer(other.transform.root);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (enemy == null) return;

        enemy.ClearPlayer();
    }
}

/*
public class EnemyDetection : MonoBehaviour
{
    private BaseEnemy enemy;

    void Start()
    {
        enemy = GetComponentInParent<BaseEnemy>();

        if (enemy == null)
        {
            Debug.LogError("[EnemyDetection] No BaseEnemy found on parent of " + gameObject.name);
        }
        else
        {
            Debug.Log("[EnemyDetection] Found BaseEnemy: " + enemy.name);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("[EnemyDetection] Trigger Enter with: " + other.name);

        if (!other.CompareTag("Player"))
        {
            Debug.Log("[EnemyDetection] Not Player, ignoring");
            return;
        }

        if (enemy == null)
        {
            Debug.LogError("[EnemyDetection] enemy reference is NULL on trigger enter");
            return;
        }

        Debug.Log("[EnemyDetection] Player detected: " + other.name);

        //enemy.SetPlayer(other.transform.root);

        BaseEnemy targetEnemy = other.GetComponent<BaseEnemy>();

        if (targetEnemy == null || targetEnemy.faction != enemy.faction)
        {
            enemy.SetPlayer(other.transform.root);
            enemy.SetStateChase();
        }
        
        Debug.Log("CHASE TRIGGERED ON: " + enemy.name);

        enemy.SetStateChase();
    }

    void OnTriggerExit(Collider other)
    {
        Debug.Log("[EnemyDetection] Trigger Exit with: " + other.name);

        if (!other.CompareTag("Player"))
        {
            Debug.Log("[EnemyDetection] Not Player on exit, ignoring");
            return;
        }

        if (enemy == null)
        {
            Debug.LogError("[EnemyDetection] enemy reference is NULL on trigger exit");
            return;
        }

        Debug.Log("[EnemyDetection] Player left detection zone");

        enemy.ClearPlayer();
    }
}
*/