using UnityEngine;
using System.Collections.Generic;

public class PlayerAttackGunRange : MonoBehaviour
{
    public List<BaseEnemy> detectedEnemies = new List<BaseEnemy>();

    private SphereCollider sphereCollider;

    private void Awake()
    {
        sphereCollider = GetComponent<SphereCollider>();

        if (sphereCollider == null)
        {
            Debug.LogWarning("[GunRange] No SphereCollider found.");
        }
        else if (!sphereCollider.isTrigger)
        {
            sphereCollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        BaseEnemy enemy = other.GetComponentInParent<BaseEnemy>();

        if (enemy != null && !detectedEnemies.Contains(enemy))
        {
            detectedEnemies.Add(enemy);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        BaseEnemy enemy = other.GetComponentInParent<BaseEnemy>();

        if (enemy != null && detectedEnemies.Contains(enemy))
        {
            detectedEnemies.Remove(enemy);
        }
    }

    private void Update()
    {
        detectedEnemies.RemoveAll(e => e == null);
    }
}