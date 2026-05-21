using UnityEngine;

public class EnemyDetection : MonoBehaviour
{
    private BaseEnemy enemy;

    [Header("Vision")]
    public LayerMask obstacleMask;
    public float eyeHeight = 1.5f;

    void Start()
    {
        enemy = GetComponentInParent<BaseEnemy>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerHealth ph =
            other.GetComponentInParent<PlayerHealth>();

        if (ph == null)
            return;

        // 🔥 Eye positions
        Vector3 enemyEye =
            transform.position + Vector3.up * eyeHeight;

        Vector3 playerEye =
            ph.transform.position + Vector3.up * 1.5f;

        Vector3 dir =
            (playerEye - enemyEye).normalized;

        float distance =
            Vector3.Distance(enemyEye, playerEye);

        // 🔥 Check if wall blocks vision
        if (!Physics.Raycast(
            enemyEye,
            dir,
            distance,
            obstacleMask))
        {
            enemy.SetPlayer(ph.transform);
            enemy.SetStateChase();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            enemy.ClearPlayer();
        }
    }
}