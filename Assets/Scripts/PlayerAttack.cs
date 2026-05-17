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
    public bool canUseAttack = true;


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

    [Header("Ammo")]
    public AmmoType ammoType;
    public int ammoPerShot = 1;

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
    public void EnableAttackInput(bool enable)
    {
        if (enable)
        {
            inputActions.Player.Attack.Enable();
        }
        else
        {
            inputActions.Player.Attack.Disable();
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
        Debug.Log("Current Weapon: " + currentWeaponData.itemName);

        if (currentWeaponData.weaponType == WeaponItem.WeaponType.Ranged)
        {
            if (!HasAmmo())
            {
                Debug.Log("🔫 Out of ammo! (Attack blocked)");
                return;
            }
        }

        // BLOCK ATTACK DURING DIALOGUE
        if (DialogueManager.Instance != null &&
            DialogueManager.Instance.IsPlaying)
        {
            return;
        }

        if (!canUseAttack)
        {
            Debug.Log("Attack blocked by cinematic");
            return;
        }
        // ❌ No stamina → cannot attack
        if (PlayerStats.Get().energy < staminaCostPerAttack)
        {
            Debug.Log("Not enough stamina!");
            return;
        }
        if (currentWeaponData.weaponType == WeaponItem.WeaponType.Ranged)
        {
            if (!HasAmmo())
            {
                Debug.Log("🔫 Out of ammo! (Attack blocked)");
                return;
            }
        }
        if (!canUseAttack)
            return;

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

        // ✅ Allow melee attacks even without enemies
        if (currentWeaponData.weaponType == WeaponItem.WeaponType.Ranged && !hasValidTarget)
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
        Debug.Log($"💥 [ATTACK] {currentWeaponData?.itemName} - Damage: {currentWeaponData?.damage}");

        // 🪓 MELEE WEAPONS (No ammo needed)
        if (currentWeaponData.weaponType == WeaponItem.WeaponType.Melee)
        {
            PlayerStats.Get().ModifyEnergy(-staminaCostPerAttack);
            DealMeleeDamage();
            return;
        }

        // 🔫 RANGED WEAPONS (Ammo required!)
        if (!ConsumeAmmo())
        {
            Debug.Log("❌ OUT OF AMMO!");
            return;
        }

        DealRangedDamage();
    }

    // 🔥 NEW: Separate ammo consumption
    private bool ConsumeAmmo()
    {
        if (currentWeaponData.requiredAmmoType == WeaponItem.AmmoType.None)
            return true; // Melee - no ammo needed

        Item ammoItem = FindAmmoItem(currentWeaponData.requiredAmmoType);
        if (ammoItem == null)
        {
            Debug.Log($"❌ No {currentWeaponData.requiredAmmoType} ammo in inventory!");
            return false;
        }

        if (!PlayerInventory.Instance.HasItem(ammoItem, currentWeaponData.ammoPerShot))
        {
            Debug.Log($"❌ Not enough {ammoItem.itemName}! Need: {currentWeaponData.ammoPerShot}");
            return false;
        }

        // ✅ CONSUME AMMO
        bool consumed = PlayerInventory.Instance.RemoveItem(ammoItem, currentWeaponData.ammoPerShot);
        Debug.Log($"✅ Consumed {currentWeaponData.ammoPerShot}x {ammoItem.itemName}");

        return consumed;
    }

    // 🔥 NEW: Melee damage only
    private void DealMeleeDamage()
    {
        List<BaseEnemy> targets = attackRadius.detectedEnemies;

        // Hit enemies
        if (targets != null && targets.Count > 0)
        {
            foreach (BaseEnemy enemy in targets)
            {
                if (enemy == null) continue;
                enemy.TakeDamage(currentWeaponData.damage);
                Debug.Log($"🗡️ MELEE HIT: {enemy.name}");
                break; // Only first enemy
            }
        }

        // Hit breakables
        foreach (BreakableObject breakable in attackRadius.detectedBreakables)
        {
            if (breakable == null) continue;
            breakable.TakeDamage((int)currentWeaponData.damage);
        }

        // Knife small obstacles
        if (currentWeaponData.itemName.Contains("Knife"))
        {
            Collider[] hits = Physics.OverlapSphere(
                attackRadius.transform.position,
                attackRadius.GetComponent<SphereCollider>().radius
            );

            foreach (Collider hit in hits)
            {
                SmallObstacle obstacle = hit.GetComponent<SmallObstacle>();
                if (obstacle != null)
                    obstacle.HitObstacle();
            }
        }
    }

    // 🔥 NEW: Ranged damage only  
    private void DealRangedDamage()
    {
        List<BaseEnemy> targets = gunRange.detectedEnemies;

        if (targets != null && targets.Count > 0)
        {
            foreach (BaseEnemy enemy in targets)
            {
                if (enemy == null) continue;
                enemy.TakeDamage(currentWeaponData.damage);
                Debug.Log($"🔫 RANGED HIT: {enemy.name}");
                break; // Only first enemy
            }
        }
    }

    // 🔥 IMPROVED: Find exact ammo type
    private Item FindAmmoItem(WeaponItem.AmmoType ammoType)
    {
        foreach (var kvp in PlayerInventory.Instance.GetInventory())
        {
            Item item = kvp.Key;
            if (item.ammoType == ammoType)  // ✅ Match WeaponItem.AmmoType
            {
                return item;
            }
        }
        return null;
    }
    private bool HasAmmo()
    {
        Item ammoItem = FindAmmoItem(currentWeaponData.requiredAmmoType);
        return ammoItem != null &&
               PlayerInventory.Instance.HasItem(ammoItem, currentWeaponData.ammoPerShot);
    }
    public void ForceStopAttack()
    {
        attackQueued = false;
        isAttacking = false;
        canAttack = true;

        animator.Play("Idle", 0);
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