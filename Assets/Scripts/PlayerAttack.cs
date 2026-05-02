using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static WeaponItem;

public class PlayerAttack : MonoBehaviour
{
    private Animator animator;
    private InputSystem_Actions inputActions;
    private PlayerAttackRadius attackRadius;
    private PlayerAttackGunRange gunRange;

    private bool isAttacking = false;
    private bool canAttack = true;
    private bool attackQueued = false;
    public bool isRanged;
    public WeaponType weaponType;

    private int comboStep = 0;
    private float comboTimer = 0f;

    [Header("Attack Settings")]
    public float comboResetTime = 1.2f;
    public float attackDamage = 20f;

    public float staminaCostPerAttack = 10f;

    [Header("Weapon")]
    public WeaponItem currentWeaponData;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
    }
    public void SetWeapon(WeaponItem weapon)
    {
        currentWeaponData = weapon;

        Debug.Log("SetWeapon called → " + weapon +
                  " from: " + new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().Name);
    }
    private void OnEnable()
    {
        inputActions.Enable();
        inputActions.Player.Attack.performed += OnAttackPerformed;
    }

    private void OnDisable()
    {
        inputActions.Player.Attack.performed -= OnAttackPerformed;
        inputActions.Disable();
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        gunRange = GetComponentInChildren<PlayerAttackGunRange>();
        attackRadius = GetComponentInChildren<PlayerAttackRadius>();
    }

    void Update()
    {
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);

        // Unlock attack when animation almost finishes
        if (!canAttack && IsInComboState(state) && state.normalizedTime >= 0.9f)
        {
            canAttack = true;
            isAttacking = false;
        }

        // Process queued attack
        if (attackQueued && canAttack)
        {
            string animName = "Combo " + (comboStep + 1);
            animator.Play(animName, 0);

            isAttacking = true;

            canAttack = false;
            attackQueued = false;

            comboStep++;
            if (comboStep > 2)
                comboStep = 0;

            comboTimer = 0f;
        }

        // Reset combo if too slow
        if (comboStep > 0)
        {
            comboTimer += Time.deltaTime;
            if (comboTimer >= comboResetTime)
            {
                ResetCombo();
            }
        }
    }

    public bool IsAttacking()
    {
        return isAttacking;
    }

    private bool IsInComboState(AnimatorStateInfo state)
    {
        return state.IsName("Combo 1") ||
               state.IsName("Combo 2") ||
               state.IsName("Combo 3");
    }

    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        // ❌ No stamina → cannot attack
        if (PlayerStats.Instance.energy < staminaCostPerAttack)
        {
            Debug.Log("Not enough stamina!");
            return;
        }

        Debug.Log("Attack pressed");
        attackQueued = true;
    }

    // 🔥 CLOSEST ENEMY DAMAGE
    public void AnimEvent_DealDamage()
    {
        if (currentWeaponData == null)
        {
            Debug.LogError("No weapon equipped!");
            return;
        }

        if (currentWeaponData.weaponType != WeaponItem.WeaponType.Ranged)
        {
            PlayerStats.Instance.ModifyEnergy(-staminaCostPerAttack);
        }

        if (currentWeaponData == null)
        {
            Debug.LogError("No weapon equipped!");
            return;
        }

        if (currentWeaponData == null)
        {
            Debug.LogError("No weapon equipped!");
            return;
        }

        List<BaseEnemy> targets = null;
        Vector3 origin;

        // 🔫 GUN (RANGED)
        if (currentWeaponData.weaponType == WeaponItem.WeaponType.Ranged)
        {
            if (gunRange == null)
            {
                Debug.LogError("Gun range not found!");
                return;
            }

            targets = gunRange.detectedEnemies;
            origin = gunRange.transform.position;
        }
        // 🪓 AXE (MELEE)
        else
        {
            if (attackRadius == null)
            {
                Debug.LogError("Attack radius not found!");
                return;
            }

            targets = attackRadius.detectedEnemies;
            origin = attackRadius.transform.position;
        }

        if (targets != null && targets.Count > 0)
        {
            BaseEnemy closestEnemy = null;
            float closestDistance = Mathf.Infinity;

            foreach (BaseEnemy enemy in targets)
            {
                if (enemy == null) continue;

                Vector3 dir = (enemy.transform.position - transform.position).normalized;
                float dot = Vector3.Dot(transform.forward, dir);
                if (dot < 0.3f) continue;

                float distance = Vector3.Distance(origin, enemy.transform.position);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = enemy;
                }
            }

            if (closestEnemy != null)
            {
                closestEnemy.TakeDamage(currentWeaponData.damage);
                Debug.Log("Hit closest enemy: " + closestEnemy.name);
            }
        }

        // =========================
        // 🧨 ADD THIS PART HERE
        // =========================
        Collider[] hits = Physics.OverlapSphere(origin, 5f);

        foreach (Collider hit in hits)
        {
            BreakableObject breakable = hit.GetComponent<BreakableObject>();

            if (breakable != null)
            {
                Vector3 dir = (hit.transform.position - transform.position).normalized;
                float dot = Vector3.Dot(transform.forward, dir);
                if (dot < 0.3f) continue;

                Debug.Log("Hit breakable: " + hit.name);
                breakable.TakeDamage(currentWeaponData.damage);
            }
        }
    }
    public void ResetCombo()
    {
        comboStep = 0;
        comboTimer = 0f;
        canAttack = true;
        attackQueued = false;
        isAttacking = false;

        animator.Play("Idle", 0);
    }
}