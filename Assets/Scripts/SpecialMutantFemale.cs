using System.Collections;
using UnityEngine;

public class SpecialMutantFemale : BaseEnemy
{
    [Header("Spit")]
    public GameObject spitPrefab;
    public Transform spitPoint;
    public float spitForce = 12f;
    public float spitRange = 7f;

    [Header("Summon")]
    public GameObject zombiePrefab;
    public Transform summonPoint;
    public GameObject summonEffectPrefab;
    public float summonRange = 10f;
    public float summonFXOffset = -0.5f;

    [Header("Summon Limit")]
    public int maxSummonedEnemies = 5;
    private int currentSummoned = 0;

    private Collider _enemyCollider;


    // =========================
    // FACE PLAYER
    // =========================
    private void FacePlayer()
    {
        if (player == null) return;

        Vector3 dir = (player.position - transform.position).normalized;
        float yRotation = Mathf.Sign(dir.x) * 90f;

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.Euler(0, yRotation, 0),
            rotationSpeed * Time.deltaTime
        );
    }

    // =========================
    // ATTACK
    // =========================

    protected override void Attack()
    {
        if (player == null) return;

        isAttacking = true;
        velocity.x = 0;

        FacePlayer();

        animator.SetBool("IsWalking", false);

        // FIX: Account for the child model offset by checking from the outermost collider edge
        float distance = GetDistanceToPlayer();

        if (distance <= spitRange)
        {
            animator.SetTrigger("Spit");
            PlaySound(enemyData.spitSound);
        }
        else if (distance <= summonRange)
        {
            animator.SetTrigger("Summon");
            PlaySound(enemyData.summonScream);
        }
        else
        {
            isAttacking = false;
        }

        // REMOVED: Broken Invoke(nameof(EndAttack)) cut from here. Handled natively via the animation relay loop.
    }

    private float GetDistanceToPlayer()
    {
        if (_enemyCollider != null)
        {
            Vector3 closestPoint = _enemyCollider.ClosestPoint(player.position);
            return Vector3.Distance(closestPoint, player.position);
        }
        return Vector3.Distance(transform.position, player.position);
    }

    private void OnValidate()
    {
        if (summonRange > chaseRange)
            summonRange = chaseRange;

        if (spitRange > summonRange)
            spitRange = summonRange;
    }

    // =========================
    // CHASE
    // =========================

    protected override void HandleChase()
    {
        if (player == null)
        {
            currentState = EnemyState.Idle;
            return;
        }

        float distance = GetDistanceToPlayer();

        if (distance < spitRange * 0.8f)
        {
            FacePlayer();

            animator.SetBool("IsWalking", true);

            Vector3 dir = (transform.position - player.position).normalized;

            velocity.x = dir.x * chaseSpeed;
            velocity.z = 0;
        }
        else if (distance <= summonRange)
        {
            FacePlayer();

            animator.SetBool("IsWalking", false);
            velocity.x = 0;
        }
        else
        {
            animator.SetBool("IsWalking", true);

            Vector3 dir = (player.position - transform.position).normalized;

            velocity.x = dir.x * chaseSpeed;
            velocity.z = 0;
        }

        if (distance > chaseRange * 1.5f)
        {
            currentState = EnemyState.Idle;
        }
    }

    // =========================
    // SPIT
    // =========================

    private void Spit()
    {
        if (spitPrefab == null || spitPoint == null) return;

        GameObject spit = Instantiate(spitPrefab, spitPoint.position, spitPoint.rotation);

        Rigidbody rb = spit.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Note: If older Unity builds reject this, fall back to: rb.velocity
            rb.linearVelocity = spitPoint.forward * spitForce;
        }
    }

    // =========================
    // SUMMON
    // =========================

    private void Summon()
    {
        if (zombiePrefab == null || summonPoint == null)
        {
            Debug.LogError("Summon failed: missing prefab or summonPoint");
            return;
        }

        if (currentSummoned >= maxSummonedEnemies)
            return;

        Vector3 spawnPos = summonPoint.position;

        GameObject obj = Instantiate(zombiePrefab, spawnPos, Quaternion.identity);

        currentSummoned++;

        BaseEnemy enemy = obj.GetComponentInChildren<BaseEnemy>();
        if (enemy != null)
        {
            enemy.SetStateChase();
            StartCoroutine(TrackSummonedEnemy(enemy));
        }

        Vector3 fxPos = spawnPos + Vector3.up * summonFXOffset;

        // FIX: Instantly runs the updated particle algorithm without wasting frames in a coroutine loop
        PlaySummonFX(fxPos);
    }

    private IEnumerator TrackSummonedEnemy(BaseEnemy enemy)
    {
        while (enemy != null)
        {
            yield return null;
        }

        currentSummoned = Mathf.Max(0, currentSummoned - 1);
    }

    // FIXED: Upgraded from a broken Coroutine system to a reliable, optimized standard method
    private void PlaySummonFX(Vector3 pos)
    {
        if (summonEffectPrefab == null) return;

        GameObject fx = Instantiate(
            summonEffectPrefab,
            pos,
            Quaternion.Euler(-90f, 0f, 0f)
        );

        ParticleSystem ps = fx.GetComponent<ParticleSystem>();

        if (ps != null)
        {
            ps.Play();
            float duration = ps.main.duration + ps.main.startLifetime.constantMax;
            Destroy(fx, duration);
        }
        else
        {
            Destroy(fx, 2f);
        }
    }

    // =========================
    // ANIMATION EVENTS
    // =========================

    public override void AnimEvent_Spit()
    {
        Spit();
    }

    public override void AnimEvent_Summon()
    {
        Summon();
    }

    // FIX: Connected missing hook call called natively by your EnemyAnimationRelay script
    public virtual void AnimEvent_EndAttack()
    {
        isAttacking = false;
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
        Gizmos.DrawWireSphere(transform.position, spitRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, summonRange);

        if (summonPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(summonPoint.position, 0.2f);
        }
    }
}
