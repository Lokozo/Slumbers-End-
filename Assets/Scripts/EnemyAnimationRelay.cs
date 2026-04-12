using UnityEngine;

public class EnemyAnimationRelay : MonoBehaviour
{
    private BaseEnemy enemy;

    void Awake()
    {
        enemy = GetComponentInParent<BaseEnemy>();
    }

    public void Hit()
    {
        enemy.AnimEvent_HitPlayer();
    }


    public void ThrowRock()
    {
        enemy.AnimEvent_ThrowRock();
    }

    public void Spit()
    {
        enemy.AnimEvent_Spit();
    }

    public void Summon()
    {
        enemy.AnimEvent_Summon();
    }

    public void SlamDamage()
    {
        enemy.AnimEvent_SlamDamage();
    }

    public void EndAttack()
    {
        enemy.AnimEvent_EndAttack();
    }

    // this is for summoning enemies, so they can call this at the end of their spawn animation to enable their behavior
    public void EndSpawn()
    {
        if (enemy != null)
        {
            enemy.EndSpawn();
        }
    }
}