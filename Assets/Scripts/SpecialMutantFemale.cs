using System.Collections;
using UnityEngine;

public class SpecialMutantFemale : BaseEnemy
{
    public GameObject spitPrefab;
    public Transform spitPoint;
    public float spitForce = 12f;

    public GameObject zombiePrefab;
    public Transform summonPoint;
    public GameObject summonEffectPrefab;

    public float spitRange = 7f;
    public float summonRange = 10f;

    public GameObject[] summonPrefabs;
    public int summonCount = 2;
    public float summonSpread = 2f;
    /*
    protected override void Attack()
    {
        if (player == null) return;

        isAttacking = true;
        velocity = Vector3.zero;

        animator.SetBool("IsWalking", false);

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= spitRange)
        {
            animator.SetTrigger("Spit");
        }
        else if (distance <= summonRange && (!isAttacking || SummonCooldown < 0))
        {
            animator.SetTrigger("Summon");
        }
        else
        {
            isAttacking = false;
            return;
        }

        Invoke(nameof(EndAttack), 1.5f);
    }*/


    public float summonCooldown = 0;

    protected void LateUpdate()
    {
        //base.Update();
        if (summonCooldown > 0)
            summonCooldown -= Time.deltaTime;
    }

    private void StartAttack()
    {
        isAttacking = true;
        velocity = Vector3.zero;
        animator.SetBool("IsWalking", false);
    }

    protected override void Attack()
    {
        if (player == null) return;
        if (isAttacking) return;

        float distance = Vector3.Distance(transform.position, player.position);

        // decide attack type ONLY
        if (distance <= spitRange)
        {
            StartAttack();
            animator.SetTrigger("Spit");
        }
        else if (distance <= summonRange)
        {
            if (summonCooldown <= 0f)
            {
                StartAttack();
                animator.SetTrigger("Summon");

                summonCooldown = 15f;
            }
        }
    }

    private void Spit()
    {
        if (spitPrefab == null || spitPoint == null)
        {
            Debug.LogError("Spit failed: missing prefab or spitPoint");
            return;
        }

        GameObject spit = Instantiate(spitPrefab, spitPoint.position, spitPoint.rotation);

        Projectile proj = spit.GetComponent<Projectile>();
        if (proj != null)
        {
            proj.owner = transform.root;
        }

        Rigidbody rb = spit.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = spitPoint.forward * spitForce;
        }
        else
        {
            Debug.LogError("Spit prefab missing Rigidbody");
        }
    }

    private Vector3 GetGroundPosition(Vector3 origin)
    {
        if (Physics.Raycast(origin + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 10f))
        {
            return hit.point;
        }

        return origin;
    }

    private void Summon()
    {
        if (summonPrefabs == null || summonPrefabs.Length == 0 || summonPoint == null)
        {
            Debug.LogError("Summon failed: missing prefabs or summonPoint");
            return;
        }

        for (int i = 0; i < summonCount; i++)
        {
            int index = Random.Range(0, summonPrefabs.Length);

            Vector3 offset = new Vector3(
                Random.Range(-summonSpread, summonSpread),
                0,
                0 // keep 2.5D
            );


            //Vector3 fxPos = transform.position + Vector3.down * 1f;
            //    StartCoroutine(PlaySlamFX(pos));


            // base spawn position
            Vector3 spawnPos = summonPoint.position + offset;
            spawnPos.z = summonPoint.position.z;

            // enforce spacing so they don't stack
            float spacing = 0.6f;
            spawnPos.x += (i - (summonCount - 1) * 0.5f) * spacing;

            GameObject obj = Instantiate(summonPrefabs[index], spawnPos, Quaternion.identity);


            // FORCE TAG
            obj.tag = "Enemy";

            BaseEnemy enemy = obj.GetComponentInChildren<BaseEnemy>();
            if (enemy != null)
            {
                enemy.PlaySpawnAnimation();
                enemy.faction = BaseEnemy.Faction.Enemy;
            }


            Vector3 fxPos = spawnPos - Vector3.up * 0.5f;
            StartCoroutine(PlaySummonFX(fxPos));
        }
    }
    private IEnumerator PlaySummonFX(Vector3 pos)

    {
        GameObject fx = Instantiate(summonEffectPrefab, pos, Quaternion.Euler(-90f, 0f, 0f)); //Instantiate(slamEffectPrefab, pos, Quaternion.identity);

        ParticleSystem ps = fx.GetComponent<ParticleSystem>();

        Destroy(fx, ps != null ? ps.main.duration + ps.main.startLifetime.constantMax : 2f);

        yield return null; // Wait 1 frame

    }

    /*
    private void Summon()
    {
        if (zombiePrefab != null && summonPoint != null)
        {
            Instantiate(zombiePrefab, summonPoint.position, Quaternion.identity);
        }
    }*/

    private void OnDrawGizmosSelected()
    {
        // SPIT RANGE (red)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, spitRange);

        // SUMMON RANGE (green)
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, summonRange);

        if (summonPoint == null) return;

        // Draw summon point
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(summonPoint.position, 0.2f);

        // Draw label (optional)
#if UNITY_EDITOR
        UnityEditor.Handles.Label(summonPoint.position + Vector3.up * 0.5f, "Summon Point");
#endif

        // Draw spawn spread range (if you're using summonSpread)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(summonPoint.position, summonSpread);
    }



    public override void AnimEvent_Spit()
    {
        Spit();
    } 

    public override void AnimEvent_Summon()
    {
        Summon();
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