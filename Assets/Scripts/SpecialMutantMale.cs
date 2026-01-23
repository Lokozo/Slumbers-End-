using UnityEngine;

public class SpecialMutantMale : MonoBehaviour
{
    private enum EnemyState { Idle, Patrol, Chase }
    private EnemyState currentState;

    private Animator animator;
    private CharacterController controller;

    public float speed = 1.2f;
    public float chaseSpeed = 2f;
    public float gravity = -9.81f;
    public float health = 300f; // High HP pool
    private Vector3 velocity;
    private bool isGrounded;

    [Header("Patrol Settings")]
    public float patrolDistance = 6f;
    public float rotationSpeed = 5f;
    private Vector3 startPosition;
    private bool movingRight = true;

    [Header("Idle Settings")]
    public float idleDuration = 2f;
    private float idleTimer = 0f;

    [Header("Chase Settings")]
    public float chaseRange = 8f;
    public float stoppingDistance = 1.5f;
    private Transform player;

    [Header("Attack Settings")]
    public float attackInterval = 4f;
    public float attackDamage = 30f;
    public float attackRadius = 2f;
    private float attackTimer = 0f;
    private bool isAttacking = false;
    private bool hasDealtDamage = false;

    [Header("Special Attacks")]
    public GameObject rockPrefab;
    public Transform throwPoint;
    public float throwForce = 15f;

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
            if (Random.value > 0.5f)
                GroundSlam();
            else
                ThrowRock();

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

    private void GroundSlam()
    {
        isAttacking = true;
        animator.SetTrigger("GroundSlam");
        Invoke(nameof(DealSlamDamage), 0.7f);
        Invoke(nameof(EndAttack), 1.5f);
    }

    private void DealSlamDamage()
    {
        if (player == null) return;
        if (Vector3.Distance(transform.position, player.position) <= attackRadius)
        {
            PlayerHealth hp = player.GetComponent<PlayerHealth>();
            if (hp != null) hp.TakeDamage((int)attackDamage);
        }
    }

    private void ThrowRock()
    {
        if (rockPrefab == null || throwPoint == null) { EndAttack(); return; }
        isAttacking = true;
        animator.SetTrigger("Throw");

        GameObject rock = Instantiate(rockPrefab, throwPoint.position, Quaternion.identity);
        Rigidbody rb = rock.GetComponent<Rigidbody>();
        if (rb != null && player != null)
        {
            Vector3 dir = (player.position - throwPoint.position).normalized;
            rb.AddForce(dir * throwForce, ForceMode.VelocityChange);
        }

        Invoke(nameof(EndAttack), 1.0f);
    }

    private void EndAttack() { isAttacking = false; }

    // Reuse patrol/idle/damage logic from BaseEnemy
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
    