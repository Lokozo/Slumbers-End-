using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class BaseEnemy : MonoBehaviour, IDamageable
{
    protected enum EnemyState { Idle, Patrol, Chase }
    protected EnemyState currentState;

    protected Animator animator;
    protected CharacterController controller;

    [Header("Enemy Data")]
    public EnemyData enemyData;

    [Header("Audio")]
    [SerializeField] protected AudioSource audioSource;

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
    private bool isDead = false;

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
    public EnemyAttackRadius attackRadiusTrigger;
    protected float attackTimer = 0f;

    public void Start()
    {
        controller = GetComponent<CharacterController>();

        animator = GetComponentInChildren<Animator>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (animator == null)
        {
            Debug.LogError($"{gameObject.name} has NO Animator!");
            enabled = false;
            return;
        }

        animator.applyRootMotion = false;

        LoadEnemyData();

        startPosition = transform.position;
        currentState = EnemyState.Idle;
        idleTimer = idleDuration;
    }

    void Update()
    {
        if (controller == null || !controller.enabled)
            return;

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
    // AUDIO
    // =========================

    protected void PlayRandomAttackSound()
    {
        if (enemyData == null) return;

        if (enemyData.attackSounds == null ||
            enemyData.attackSounds.Length == 0)
            return;

        AudioClip clip =
            enemyData.attackSounds[
                Random.Range(0, enemyData.attackSounds.Length)
            ];

        audioSource.PlayOneShot(clip);
    }

    protected void PlayRandomHurtSound()
    {
        if (enemyData == null) return;

        if (enemyData.hurtSounds == null ||
            enemyData.hurtSounds.Length == 0)
            return;

        AudioClip clip =
            enemyData.hurtSounds[
                Random.Range(0, enemyData.hurtSounds.Length)
            ];

        audioSource.PlayOneShot(clip);
    }

    protected void PlaySound(AudioClip clip)
    {
        if (clip == null) return;

        audioSource.PlayOneShot(clip);
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
        if (player == null) return;
        isAttacking = true;
        hasDealtDamage = false;

        velocity = Vector3.zero;

        velocity.x = 0;
        animator.SetBool("IsWalking", false);
        animator.SetTrigger("Attack");

        PlayRandomAttackSound();

        // YOU ASKED TO KEEP THESE EXACTLY
        //Invoke(nameof(HitPlayer), 0.5f);
        //Invoke(nameof(EndAttack), 1.0f);
    }

    //protected void HitPlayer()
    //{
    //    if (hasDealtDamage || player == null) return;

    //    Vector3 origin = attackPoint != null ? attackPoint.position : transform.position;

    //    Vector3 toPlayer = player.position - origin;
    //    float dist = toPlayer.magnitude;

    //    if (dist > attackRadius) return;

    //    Vector3 dirToPlayer = toPlayer.normalized;

    //    float dot = Vector3.Dot(transform.forward, dirToPlayer);
    //    float dotThreshold = Mathf.Cos((attackAngle * 0.5f) * Mathf.Deg2Rad);

    //    if (dot <= dotThreshold) return;

    //    PlayerHealth hp = player.GetComponent<PlayerHealth>();
    //    if (hp != null)
    //    {
    //        hp.TakeDamage((int)attackDamage);
    //        hasDealtDamage = true;
    //    }
    //}

    protected void HitPlayer()
    {
        Debug.Log("Attempting to hit player...");
        if (hasDealtDamage) return; // 🔥 FIRST LINE

        hasDealtDamage = true; // 🔥 MOVE THIS UP

        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist > attackRadius) return;

        PlayerHealth hp = player.GetComponent<PlayerHealth>();
        if (hp != null)
        {
            hp.TakeDamage((int)attackDamage);
        }
    }

    protected void EndAttack()
    {
        isAttacking = false;
    }

    // =========================
    // STATES

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
    //protected void EndHit()
    //{
    //    animator.SetBool("IsHit", false);
    //}

    public virtual void TakeDamage(float damage)
    {
        if (isDead)
            return;

        health -= damage;

        if (health <= 0)
        {
            health = 0;
            Die();
            return;
        }

        PlayRandomHurtSound();
    }

    protected virtual void Die()
    {
        if (isDead)
            return;

        isDead = true;

        CancelInvoke();
        StopAllCoroutines();

        isAttacking = false;
        velocity = Vector3.zero;

        if (controller != null)
            controller.enabled = false;

        animator.SetBool("IsWalking", false);
        //animator.SetBool("IsHit", false);

        // PLAY DEATH ANIMATION DIRECTLY
        animator.Play("BaseEnemyDeath");

        PlaySound(enemyData.deathSound);

        StartCoroutine(DieRoutine());
    }

    private IEnumerator DieRoutine()
    {
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }


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
    //
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
    protected virtual void LoadEnemyData()
    {
        if (enemyData == null)
        {
            Debug.LogWarning("EnemyData missing!");
            return;
        }

        health = enemyData.health;
        speed = enemyData.speed;
        chaseSpeed = enemyData.chaseSpeed;

        attackDamage = enemyData.attackDamage;
        attackInterval = enemyData.attackInterval;
        attackRadius = enemyData.attackRadius;

        chaseRange = enemyData.chaseRange;
        stoppingDistance = enemyData.stoppingDistance;

        patrolDistance = enemyData.patrolDistance;

        if (animator != null && enemyData.animatorController != null)
        {
            animator.runtimeAnimatorController =
                enemyData.animatorController;
        }
    }
}