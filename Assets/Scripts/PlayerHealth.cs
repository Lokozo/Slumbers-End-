using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public PlayerStats stats;
    private Animator animator;
    private PlayerController playerController;
    private PlayerAttack playerAttack;
    private bool isDead = false;

    //void Start()
    //{
    //    animator = GetComponentInChildren<Animator>();
    //    playerController = GetComponent<PlayerController>();
    //    playerAttack = GetComponent<PlayerAttack>();

    //    if (stats == null)
    //        stats = GetComponent<PlayerStats>();
    //}

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        playerController = GetComponent<PlayerController>();
        playerAttack = GetComponent<PlayerAttack>();

        stats = FindFirstObjectByType<PlayerStats>();

        if (stats == null)
        {
            Debug.LogError("PlayerStats not found in scene!");
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        if (stats == null)
        {
            Debug.LogError("TakeDamage failed: stats is NULL");
            return;
        }

        Debug.Log("Taking damage: " + damage);

        stats.ModifyHealth(-damage);

        if (animator != null)
            animator.SetTrigger("isHit");

        if (stats.health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;

        animator.SetTrigger("Die");

        // 🔥 Disable ALL scripts except this one
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();

        foreach (var script in scripts)
        {
            if (script != this)
                script.enabled = false;
        }

        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null)
            cc.enabled = false;

        Debug.Log("Player died.");
    }
}
