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
        Debug.DrawLine(transform.position, attackRadius.transform.position, Color.red);
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

        // =========================
        // ✅ CHECK IF ENEMY IN RANGE
        // =========================
        bool hasValidTarget = false;

        List<BaseEnemy> targets = (currentWeaponData.weaponType == WeaponItem.WeaponType.Ranged)
            ? gunRange.detectedEnemies
            : attackRadius.detectedEnemies;

        float radius = (currentWeaponData.weaponType == WeaponItem.WeaponType.Ranged)
            ? gunRange.GetComponent<SphereCollider>().radius
            : attackRadius.GetComponent<SphereCollider>().radius;

        Vector3 origin = (currentWeaponData.weaponType == WeaponItem.WeaponType.Ranged)
            ? gunRange.transform.position
            : attackRadius.transform.position;

        foreach (var enemy in targets)
        {
            if (enemy == null) continue;

            float dist = Vector3.Distance(origin, enemy.transform.position);

            if (dist <= radius)
            {
                hasValidTarget = true;
                break;
            }
        }

        // ❌ No enemy → DO NOT ATTACK
        if (!hasValidTarget)
        {
            Debug.Log("[ATTACK BLOCKED] Enemy not in actual range");
            return;
        }

        // ✅ Attack allowed
        Debug.Log("Attack pressed");
        attackQueued = true;
    }

    // 🔥 CLOSEST ENEMY DAMAGE
    public void AnimEvent_DealDamage()
    {
        if (currentWeaponData.weaponType != WeaponItem.WeaponType.Ranged && (attackRadius.detectedEnemies == null || attackRadius.detectedEnemies.Count == 0))
        {
            return;
        }

        Debug.Log($"[ATTACK] Weapon Type: {currentWeaponData.weaponType}");

        List<BaseEnemy> targets;
        Vector3 origin;

        // 🔫 GUN RANGE (uses GunRange object ONLY)
        if (currentWeaponData.weaponType == WeaponItem.WeaponType.Ranged)
        {
            if (gunRange == null)
            {
                Debug.LogError("[ATTACK] GunRange missing!");
                return;
            }

            targets = gunRange.detectedEnemies;
            origin = gunRange.transform.position;

            Debug.Log("[ATTACK] Using GUN RANGE trigger");
        }
        // 🪓 MELEE (uses AttackRadius object ONLY)
        else
        {
            if (attackRadius == null)
            {
                Debug.LogError("[ATTACK] AttackRadius missing!");
                return;
            }

            targets = attackRadius.detectedEnemies;
            origin = attackRadius.transform.position;

            Debug.Log("[ATTACK] Using MELEE RADIUS trigger");

            PlayerStats.Instance.ModifyEnergy(-staminaCostPerAttack);
        }

        // ❌ NOTHING DETECTED
        if (targets == null || targets.Count == 0)
        {
            Debug.Log("[DAMAGE] No enemy inside trigger");
            return;
        }

        // ✅ DAMAGE FIRST VALID ENEMY
        foreach (BaseEnemy enemy in targets)
        {
            if (enemy == null) continue;

            Debug.Log($"[DAMAGE] {currentWeaponData.weaponType} hit {enemy.name}");
            enemy.TakeDamage(currentWeaponData.damage);

            break; // hit only one (remove if you want multi-hit)
        }

        // =========================
        // 🪓 BREAKABLES (MELEE ONLY)
        // =========================
        if (currentWeaponData.weaponType != WeaponItem.WeaponType.Ranged)
        {
            foreach (BreakableObject breakable in attackRadius.detectedBreakables)
            {
                if (breakable == null) continue;

                Debug.Log($"[BREAKABLE HIT] {breakable.name}");
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