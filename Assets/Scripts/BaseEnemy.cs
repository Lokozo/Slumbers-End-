using UnityEngine;

public class BaseEnemy : MonoBehaviour
{
    private enum EnemyState { Idle, Patrol, Chase }
    private EnemyState currentState;

    private Animator animator;
    private CharacterController controller;

    public float speed = 1.5f;
    public float chaseSpeed = 3f;
    public float gravity = -9.81f;
    public float health = 100f;
    private Vector3 velocity;
    private bool isGrounded;

    [Header("Patrol Distance Settings")]
    public float patrolDistance = 5f;
    public float rotationSpeed = 5f;
    private Vector3 startPosition;
    private bool movingRight = true;

    [Header("Idle Settings")]
    public float idleDuration = 2f;
    private float idleTimer = 0f;

    [Header("Chase Settings")]
    public float chaseRange = 7f;
    public float stoppingDistance = 1.2f; // ✅ New stopping distance
    private Transform player;

    [Header("Attack Settings")]
    public float attackInterval = 3f;
    public float attackDamage = 15f;
    public float attackRadius = 1.5f;
    private float attackTimer = 0f;
    private bool isAttacking = false;
    private bool hasDealtDamage = false;

    private bool hasCombatPrompt = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        animator.applyRootMotion = false;
        startPosition = transform.position;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        currentState = EnemyState.Idle;
        idleTimer = idleDuration;
    }

    void Update()
    {
        HandleGroundedCheck();
        ApplyGravity();

        // Attack timer
        attackTimer += Time.deltaTime;
        if (attackTimer >= attackInterval && currentState == EnemyState.Chase && !isAttacking)
        {
            Attack();
            attackTimer = 0f;
        }

        switch (currentState)
        {
            case EnemyState.Idle:
                HandleIdle();
                break;
            case EnemyState.Patrol:
                HandlePatrol();
                break;
            case EnemyState.Chase:
                HandleChase();
                break;
        }

        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleChase()
    {
        hasCombatPrompt = true;
        TutorialUIManager tutorial = FindAnyObjectByType<TutorialUIManager>();
        if (hasCombatPrompt && tutorial != null)
        {
            tutorial.ShowInteractionInstruction("Click Left Mouse Button to attack");
        }

        if (player == null)
        {
            currentState = EnemyState.Idle;
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > stoppingDistance) // ✅ Move only if outside stopping distance
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
            velocity.x = 0; // ✅ Stop moving
        }

        if (distanceToPlayer > chaseRange * 1.5f) // Exit chase if too far
        {
            currentState = EnemyState.Idle;
        }
    }

    private void Attack()
    {
        isAttacking = true;
        hasDealtDamage = false;
        velocity.x = 0;
        animator.SetBool("IsWalking", false);
        animator.SetTrigger("IsAttacking");

        Invoke(nameof(HitPlayer), 0.5f);
        Invoke(nameof(EndAttack), 1.0f);
    }

    private void HitPlayer()
    {
        if (hasDealtDamage || player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= attackRadius)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage((int)attackDamage);
                hasDealtDamage = true;
            }
        }
    }

    private void EndAttack()
    {
        isAttacking = false;
    }

    private void HandleIdle()
    {
        animator.SetBool("IsWalking", false);
        velocity.x = 0;

        if (IsPlayerInRange())
        {
            currentState = EnemyState.Chase;
            return;
        }

        idleTimer -= Time.deltaTime;
        if (idleTimer <= 0f)
        {
            idleTimer = idleDuration;
            currentState = EnemyState.Patrol;
        }
    }

    private void HandlePatrol()
    {
        animator.SetBool("IsWalking", true);

        float moveDir = movingRight ? 1f : -1f;
        velocity.x = moveDir * speed;
        velocity.z = 0;

        Vector3 lookDirection = new Vector3(moveDir, 0, 0);
        if (lookDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        float distanceFromStart = transform.position.x - startPosition.x;
        if (movingRight && distanceFromStart >= patrolDistance)
        {
            movingRight = false;
            currentState = EnemyState.Idle;
        }
        else if (!movingRight && distanceFromStart <= -patrolDistance)
        {
            movingRight = true;
            currentState = EnemyState.Idle;
        }

        if (IsPlayerInRange())
        {
            currentState = EnemyState.Chase;
        }
    }

    private bool IsPlayerInRange()
    {
        if (player == null) return false;
        return Vector3.Distance(transform.position, player.position) <= chaseRange;
    }

    public void TakeDamage(float amount)
    {
        if (health <= 0) return;

        health -= amount;

        if (animator != null)
        {
            animator.SetBool("IsHit", true);
            Invoke(nameof(EndHit), 0.5f);
        }

        if (health <= 0)
        {
            Die();
            TutorialUIManager tutorialUI = FindAnyObjectByType<TutorialUIManager>();
            if (tutorialUI != null)
            {
                tutorialUI.HideInstruction(); // Fades it out cleanly
            }
        }
    }

    private void EndHit()
    {
        animator.SetBool("IsHit", false);
    }

    private void Die()
    {
        Debug.Log("Enemy died.");
        Destroy(gameObject);
    }

    private void HandleGroundedCheck()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
    }

    private void ApplyGravity()
    {
        if (!isGrounded)
        {
            velocity.y += gravity * Time.deltaTime;
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        // ✅ Draw forward-facing cone
        Vector3 forward = transform.forward * attackRadius;
        Vector3 leftBoundary = Quaternion.Euler(0, -30, 0) * forward;
        Vector3 rightBoundary = Quaternion.Euler(0, 30, 0) * forward;

        Gizmos.DrawLine(transform.position, transform.position + forward);
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);

        // Optional: Draw arc lines for visualization
        int segments = 10;
        float angleStep = 60f / segments;
        Vector3 lastPoint = transform.position + leftBoundary;
        for (int i = 1; i <= segments; i++)
        {
            Vector3 nextPoint = transform.position + Quaternion.Euler(0, -30 + angleStep * i, 0) * forward;
            Gizmos.DrawLine(lastPoint, nextPoint);
            lastPoint = nextPoint;
        }
    }

}
