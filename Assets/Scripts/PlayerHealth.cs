using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public PlayerStats stats;
    private Animator animator;
    private PlayerController playerController;
    private PlayerAttack playerAttack;
    private bool isDead = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
        playerAttack = GetComponent<PlayerAttack>();

        if (stats == null)
            stats = GetComponent<PlayerStats>();
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        stats.ModifyHealth(-damage); // use ModifyHealth in PlayerStats

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

        if (playerController != null)
            playerController.enabled = false;

        if (playerAttack != null)
            playerAttack.enabled = false;

        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        Debug.Log("Player died.");
    }
}
