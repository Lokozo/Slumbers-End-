using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    private Animator animator;
    private InputSystem_Actions inputActions;
    private PlayerAttackRadius attackRadius;

    private int comboStep = 0;
    private float comboTimer = 0f;
    public float comboResetTime = 1.2f;
    public float attackDamage = 20f;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
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
        attackRadius = GetComponentInChildren<PlayerAttackRadius>(); // ✅ Detect radius script
    }

    void Update()
    {
        if (comboStep > 0)
        {
            comboTimer += Time.deltaTime;
            if (comboTimer >= comboResetTime)
            {
                ResetCombo();
            }
        }
    }

    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        if (comboStep == 0)
        {
            animator.SetTrigger("Combo1");
        }
        else if (comboStep == 1)
        {
            animator.SetTrigger("Combo2");
        }
        else if (comboStep == 2)
        {
            animator.SetTrigger("Combo3");
        }

        comboStep++;
        if (comboStep > 2) comboStep = 2;

        comboTimer = 0f;

        // ✅ Damage enemies during attack
        DealDamageToEnemies();
    }

    private void DealDamageToEnemies()
    {
        if (attackRadius == null) return;

        foreach (BaseEnemy enemy in attackRadius.detectedEnemies)
        {
            if (enemy != null)
            {
                enemy.TakeDamage(attackDamage);
            }
        }
    }

    public void ResetCombo()
    {
        comboStep = 0;
        comboTimer = 0f;

        animator.ResetTrigger("Combo1");
        animator.ResetTrigger("Combo2");
        animator.ResetTrigger("Combo3");

        animator.Play("Idle", 0);
    }
}
