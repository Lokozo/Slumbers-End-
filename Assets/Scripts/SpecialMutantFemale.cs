using UnityEngine;

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
