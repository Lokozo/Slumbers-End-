using System.Collections;
using UnityEngine;

public class SpecialMutantMale : BaseEnemy
{
    [Header("Attacks")]
    public GameObject rockPrefab;
    public GameObject slamEffectPrefab;
    public Transform throwPoint;
    public float throwForce = 15f;

    [Header("Ranges")]
    public float meleeRange = 2f;
    public float slamRange = 6f;
    public float throwRange = 12f;

    private Collider _enemyCollider;


    // =========================
    // ATTACK LOGIC (NO RANDOM)
    // =========================

    protected override void Attack()
    {
        if (player == null) return;

        isAttacking = true;
        velocity = Vector3.zero;

        animator.SetBool("IsWalking", false);

        // FIX: Use the helper function to get an accurate distance accounting for model offsets
        float distance = GetDistanceToPlayer();

        if (distance <= meleeRange)
        {
            animator.SetTrigger("Attack");
        }
        else if (distance <= slamRange)
        {
            animator.SetTrigger("GroundSlam");
            PlaySound(enemyData.slamSound);
        }
        else if (distance <= throwRange)
        {
            animator.SetTrigger("Throw");
            PlaySound(enemyData.throwSound);
        }
        else
        {
            isAttacking = false;
        }

        // Let the relay's EndAttack animation event handle resetting state naturally!
    }

    private float GetDistanceToPlayer()
    {
        if (_enemyCollider != null)
        {
            // Calculates distance from the outermost edge of the enemy to the player
            Vector3 closestPoint = _enemyCollider.ClosestPoint(player.position);
            return Vector3.Distance(closestPoint, player.position);
        }
        return Vector3.Distance(transform.position, player.position);
    }

    private void OnValidate()
    {
        if (throwRange > chaseRange) throwRange = chaseRange;
        if (slamRange > throwRange) slamRange = throwRange;
        if (meleeRange > slamRange) meleeRange = slamRange;
    }

    // =========================
    // SLAM DAMAGE
    // =========================

    private void SlamDamage()
    {
        if (player == null) return;

        float dist = GetDistanceToPlayer();

        // FIX: If the slam animation played, they are close enough. 
        // We look at slamRange instead of an unassigned or tight attackRadius variable.
        if (dist <= slamRange + 0.5f)
        {
            PlayerHealth hp = player.GetComponent<PlayerHealth>();
            if (hp != null)
                hp.TakeDamage((int)(attackDamage * 2));
        }

        PlaySlamFX(transform.position);
    }

    private void PlaySlamFX(Vector3 pos)
    {
        if (slamEffectPrefab == null) return;

        GameObject fx = Instantiate(slamEffectPrefab, pos, Quaternion.Euler(-90f, 0f, 0f));
        ParticleSystem ps = fx.GetComponent<ParticleSystem>();

        if (ps != null)
        {
            ps.Play();
            Destroy(fx, ps.main.duration + ps.main.startLifetime.constantMax);
        }
        else
        {
            Destroy(fx, 2f);
        }
    }

    // =========================
    // THROW ROCK
    // =========================

    private void ThrowRock()
    {
        if (rockPrefab == null || throwPoint == null) return;

        GameObject rock = Instantiate(rockPrefab, throwPoint.position, throwPoint.rotation);
        Rigidbody rb = rock.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = throwPoint.forward * throwForce;
        }
    }

    // =========================
    // ANIMATION RELAY INTERFACES
    // =========================

    public override void AnimEvent_ThrowRock()
    {
        ThrowRock();
    }

    public override void AnimEvent_SlamDamage()
    {
        SlamDamage();
    }

    public override void AnimEvent_HitPlayer()
    {
        // Double check distance during the actual swing frame to see if player dodged
        if (GetDistanceToPlayer() <= meleeRange + 0.5f)
        {
            HitPlayer(); // Calls your standard base damage function
        }
    }

    // FIX: Added the missing hook that your EnemyAnimationRelay script calls!
    public virtual void AnimEvent_EndAttack()
    {
        isAttacking = false;
        // Allows your base enemy state machine to resume chasing/moving
        if (animator != null)
        {
            animator.SetBool("IsWalking", true);
        }
    }

    // =========================
    // GIZMOS
    // =========================

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, slamRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, throwRange);
    }
}
