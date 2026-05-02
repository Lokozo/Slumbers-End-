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
        if (other.CompareTag("Player"))
        {
            PlayerHealth ph = other.GetComponentInParent<PlayerHealth>();

            if (ph != null)
            {
                enemy.SetPlayer(ph.transform);
                enemy.SetStateChase();
            }
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