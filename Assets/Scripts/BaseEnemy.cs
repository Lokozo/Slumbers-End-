using UnityEngine;

public class BaseEnemy : MonoBehaviour
{
    protected enum EnemyState { Idle, Patrol, Chase }
    protected EnemyState currentState;

    protected Animator animator;
    protected CharacterController controller;

    [Header("Core Stats")]
    public float speed = 1.5f;
    public float chaseSpeed = 3f;
    public float gravity = -9.81f;
    public float health = 100f;

    protected Vector3 velocity;
    protected bool isGrounded;

    [Header("Attack State")]
    protected bool isAttacking = false;
    protected bool hasDealtDamage = false;

    [Header("Patrol")]
    public float patrolDistance = 5f;
    public float rotationSpeed = 5f;
    protected Vector3 startPosition;
    protected bool movingRight = true;

    [Header("Idle")]
    public float idleDuration = 2f;
    protected float idleTimer = 0f;

    [Header("Chase")]
    public float chaseRange = 7f;
    public float stoppingDistance = 1.2f;
    protected Transform player;

    [Header("Attack")]
    public float attackInterval = 3f;
    public float attackDamage = 15f;
    public float attackRadius = 1.5f;
    public Transform attackPoint;
    protected float attackTimer = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // FIX: always grab correct animator from model child
        animator = GetComponentInChildren<Animator>();

        if (animator == null)
        {
            Debug.LogError("Animator not found in children!");
        }

        animator.applyRootMotion = false;

        startPosition = transform.position;
        currentState = EnemyState.Idle;
        idleTimer = idleDuration;
    }

    void Update()
    {
        HandleGroundedCheck();
        ApplyGravity();

        attackTimer += Time.deltaTime;

        if (!isAttacking &&
            attackTimer >= attackInterval &&
            currentState == EnemyState.Chase)
        {
            Attack();
            attackTimer = 0f;
        }

        if (!isAttacking)
        {
            switch (currentState)
            {
                case EnemyState.Idle: HandleIdle(); break;
                case EnemyState.Patrol: HandlePatrol(); break;
                case EnemyState.Chase: HandleChase(); break;
            }
        }

        controller.Move(velocity * Time.deltaTime);
    }

    // =========================
    // CHASE
    // =========================

    protected virtual void HandleChase()
    {
        if (player == null)
        {
            currentState = EnemyState.Idle;
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > stoppingDistance)
        {
            animator.SetBool("IsWalking", true);

            Vector3 dir = (player.position - transform.position).normalized;

            velocity.x = dir.x * chaseSpeed;
            velocity.z = 0;

            float yRotation = Mathf.Sign(dir.x) * 90f;
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.Euler(0, yRotation, 0),
                rotationSpeed * Time.deltaTime
            );
        }
        else
        {
            animator.SetBool("IsWalking", false);
            velocity.x = 0;
        }

        if (distance > chaseRange * 1.5f)
        {
            currentState = EnemyState.Idle;
        }
    }


    protected virtual void Attack()
    {
        isAttacking = true;
        hasDealtDamage = false;

        velocity = Vector3.zero;

        animator.SetBool("IsWalking", false);
        animator.SetTrigger("Attack");

        // YOU ASKED TO KEEP THESE EXACTLY
        //Invoke(nameof(HitPlayer), 0.5f);
        //Invoke(nameof(EndAttack), 1.0f);
    }

    protected void HitPlayer()
    {
        if (hasDealtDamage || player == null) return;

        Vector3 origin = attackPoint != null ? attackPoint.position : transform.position;

        Vector3 toPlayer = player.position - origin;
        float dist = toPlayer.magnitude;

        if (dist > attackRadius) return;

        Vector3 dirToPlayer = toPlayer.normalized;

        float dot = Vector3.Dot(transform.forward, dirToPlayer);
        float dotThreshold = Mathf.Cos((attackAngle * 0.5f) * Mathf.Deg2Rad);

        if (dot <= dotThreshold) return;

        PlayerHealth hp = player.GetComponent<PlayerHealth>();
        if (hp != null)
        {
            hp.TakeDamage((int)attackDamage);
            hasDealtDamage = true;
        }
    }

    protected void EndAttack()
    {
        isAttacking = false;
    }

    // =========================
    // STATES
    // =========================

    protected void HandleIdle()
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

    protected void HandlePatrol()
    {
        animator.SetBool("IsWalking", true);

        float dir = movingRight ? 1f : -1f;
        velocity.x = dir * speed;

        float yRotation = dir > 0 ? 90f : -90f;

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.Euler(0, yRotation, 0),
            rotationSpeed * Time.deltaTime
        );

        float distFromStart = transform.position.x - startPosition.x;

        if (movingRight && distFromStart >= patrolDistance)
        {
            movingRight = false;
            currentState = EnemyState.Idle;
        }
        else if (!movingRight && distFromStart <= -patrolDistance)
        {
            movingRight = true;
            currentState = EnemyState.Idle;
        }

        if (IsPlayerInRange())
        {
            currentState = EnemyState.Chase;
        }
    }

    protected bool IsPlayerInRange()
    {
        return player != null &&
               Vector3.Distance(transform.position, player.position) <= chaseRange;
    }

    // =========================
    // DAMAGE
    // =========================

    public virtual void TakeDamage(float amount)
    {
        if (health <= 0) return;

        health -= amount;

        animator.SetBool("IsHit", true);
        Invoke(nameof(EndHit), 0.5f);

        if (health <= 0)
        {
            Die();
        }
    }

    protected void EndHit()
    {
        animator.SetBool("IsHit", false);
    }

    protected virtual void Die()
    {
        Destroy(gameObject);
    }

    // =========================
    // PHYSICS
    // =========================

    protected void HandleGroundedCheck()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
    }

    protected void ApplyGravity()
    {
        if (!isGrounded)
        {
            velocity.y += gravity * Time.deltaTime;
        }
    }

    // =========================
    // DETECTION HOOKS
    // =========================

    public void SetPlayer(Transform newPlayer)
    {
        player = newPlayer;
    }

    public void ClearPlayer()
    {
        player = null;
        currentState = EnemyState.Idle;
    }

    public void SetStateChase()
    {
        currentState = EnemyState.Chase;
    }

    public virtual void AnimEvent_ThrowRock() { }
    public virtual void AnimEvent_Spit() { }
    public virtual void AnimEvent_Summon() { }
    public virtual void AnimEvent_SlamDamage() { }

    public virtual void AnimEvent_EndAttack()
    {
        EndAttack();
    }

    public virtual void AnimEvent_HitPlayer()
    {
        HitPlayer();
    }

    public float attackAngle = 60f;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        float halfAngle = attackAngle * 0.5f;

        Vector3 forward = transform.forward * attackRadius;
        Vector3 leftBoundary = Quaternion.Euler(0, -halfAngle, 0) * forward;
        Vector3 rightBoundary = Quaternion.Euler(0, halfAngle, 0) * forward;

        // Main lines
        Gizmos.DrawLine(transform.position, transform.position + forward);
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);

        // Arc
        int segments = 10;
        float angleStep = attackAngle / segments;

        Vector3 lastPoint = transform.position + leftBoundary;

        for (int i = 1; i <= segments; i++)
        {
            float angle = -halfAngle + angleStep * i;
            Vector3 nextPoint = transform.position + Quaternion.Euler(0, angle, 0) * forward;

            Gizmos.DrawLine(lastPoint, nextPoint);
            lastPoint = nextPoint;
        }
    }
}

/*using UnityEngine;

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
            //tutorial.ShowInteractionInstruction("Click Left Mouse Button to attack");
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
               // tutorialUI.HideInstruction(); // Fades it out cleanly
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
*/