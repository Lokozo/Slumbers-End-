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
            enemy.SetPlayer(other.transform.root);
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