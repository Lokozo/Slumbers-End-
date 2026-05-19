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

    // =========================
    // FACE PLAYER (FIX)
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

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= spitRange)
        {
            animator.SetTrigger("Spit");
            //Invoke(nameof(Spit), 0.4f);
            PlaySound(enemyData.spitSound);
        }
        else if (distance <= summonRange)
        {
            animator.SetTrigger("Summon");
            //Invoke(nameof(Summon), 0.6f);
            PlaySound(enemyData.summonScream);
        }
        else
        {
            isAttacking = false;
            return;
        }

        //Invoke(nameof(EndAttack), 1.5f);
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

        float distance = Vector3.Distance(transform.position, player.position);

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
        StartCoroutine(PlaySummonFX(fxPos));
    }

    private IEnumerator TrackSummonedEnemy(BaseEnemy enemy)
    {
        while (enemy != null)
        {
            yield return null;
        }

        currentSummoned = Mathf.Max(0, currentSummoned - 1);
    }

    private IEnumerator PlaySummonFX(Vector3 pos)
    {
        GameObject fx = Instantiate(
            summonEffectPrefab,
            pos,
            Quaternion.Euler(-90f, 0f, 0f)
        );

        ParticleSystem ps = fx.GetComponent<ParticleSystem>();

        Destroy(fx, ps != null
            ? ps.main.duration + ps.main.startLifetime.constantMax
            : 2f);

        yield return null;
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

/*
public class SpecialMutantFemale : MonoBehaviour
{
    private enum EnemyState { Idle, Patrol, Chase }
    private EnemyState currentState;

    private Animator animator;
    private CharacterController controller;

    public float speed = 1.5f;
    public float chaseSpeed = 2.5f;
    public float gravity = -9.81f;
    public float health = 120f; // Weaker HP
    private Vector3 velocity;
    private bool isGrounded;

    [Header("Patrol Settings")]
    public float patrolDistance = 4f;
    public float rotationSpeed = 5f;
    private Vector3 startPosition;
    private bool movingRight = true;

    [Header("Idle Settings")]
    public float idleDuration = 2f;
    private float idleTimer = 0f;

    [Header("Chase Settings")]
    public float chaseRange = 9f;
    public float stoppingDistance = 2f;
    private Transform player;

    [Header("Attack Settings")]
    public float attackInterval = 5f;
    public float spitDamage = 20f;
    private float attackTimer = 0f;
    private bool isAttacking = false;

    [Header("Special Attacks")]
    public GameObject spitPrefab;
    public Transform spitPoint;
    public float spitForce = 12f;

    public GameObject zombiePrefab;
    public Transform summonPoint;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        animator.applyRootMotion = false;
        startPosition = transform.position;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        currentState = EnemyState.Idle;
        idleTimer = idleDuration;
    }

    private void Update()
    {
        HandleGroundedCheck();
        ApplyGravity();

        attackTimer += Time.deltaTime;
        if (attackTimer >= attackInterval && currentState == EnemyState.Chase && !isAttacking)
        {
            if (Random.value > 0.5f) SpitProjectile();
            else SummonZombie();

            attackTimer = 0f;
        }

        switch (currentState)
        {
            case EnemyState.Idle: HandleIdle(); break;
            case EnemyState.Patrol: HandlePatrol(); break;
            case EnemyState.Chase: HandleChase(); break;
        }

        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleChase()
    {
        if (player == null) { currentState = EnemyState.Idle; return; }
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > stoppingDistance)
        {
            animator.SetBool("IsWalking", true);
            Vector3 direction = (player.position - transform.position).normalized;
            velocity.x = direction.x * chaseSpeed;
            velocity.z = 0;

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, 0));
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
        else
        {
            animator.SetBool("IsWalking", false);
            velocity.x = 0;
        }

        if (distanceToPlayer > chaseRange * 1.5f) currentState = EnemyState.Idle;
    }

    private void SpitProjectile()
    {
        if (spitPrefab == null || spitPoint == null) return;
        isAttacking = true;
        animator.SetTrigger("Spit");

        GameObject spit = Instantiate(spitPrefab, spitPoint.position, Quaternion.identity);
        Rigidbody rb = spit.GetComponent<Rigidbody>();
        if (rb != null && player != null)
        {
            Vector3 dir = (player.position - spitPoint.position).normalized;
            rb.AddForce(dir * spitForce, ForceMode.VelocityChange);
        }

        Invoke(nameof(EndAttack), 1.0f);
    }

    private void SummonZombie()
    {
        if (zombiePrefab == null || summonPoint == null) return;
        isAttacking = true;
        animator.SetTrigger("Summon");

        Instantiate(zombiePrefab, summonPoint.position, Quaternion.identity);
        Invoke(nameof(EndAttack), 1.0f);
    }

    private void EndAttack() { isAttacking = false; }

    private void HandleIdle()
    {
        animator.SetBool("IsWalking", false);
        velocity.x = 0;
        if (IsPlayerInRange()) { currentState = EnemyState.Chase; return; }
        idleTimer -= Time.deltaTime;
        if (idleTimer <= 0f) { idleTimer = idleDuration; currentState = EnemyState.Patrol; }
    }

    private void HandlePatrol()
    {
        animator.SetBool("IsWalking", true);
        float moveDir = movingRight ? 1f : -1f;
        velocity.x = moveDir * speed;
        velocity.z = 0;

        Quaternion targetRotation = Quaternion.LookRotation(new Vector3(moveDir, 0, 0));
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        float dist = transform.position.x - startPosition.x;
        if (movingRight && dist >= patrolDistance) { movingRight = false; currentState = EnemyState.Idle; }
        else if (!movingRight && dist <= -patrolDistance) { movingRight = true; currentState = EnemyState.Idle; }

        if (IsPlayerInRange()) currentState = EnemyState.Chase;
    }

    private bool IsPlayerInRange() => player != null && Vector3.Distance(transform.position, player.position) <= chaseRange;

    public void TakeDamage(float dmg)
    {
        if (health <= 0) return;
        health -= dmg;
        animator.SetTrigger("IsHit");
        if (health <= 0) Die();
    }

    private void Die() { animator.SetTrigger("Die"); Destroy(gameObject, 2f); }

    private void HandleGroundedCheck()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0) velocity.y = -2f;
    }

    private void ApplyGravity() { if (!isGrounded) velocity.y += gravity * Time.deltaTime; }
}
*/