using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public PlayerStats stats;

    private Animator animator;
    private PlayerController playerController;
    private PlayerAttack playerAttack;
    private CharacterController cc;

    private bool isDead = false;

    public bool IsDead => isDead;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        playerController = GetComponent<PlayerController>();
        playerAttack = GetComponent<PlayerAttack>();
        cc = GetComponent<CharacterController>();

        

        if (stats == null)
        {
            Debug.LogError("PlayerStats instance not found!");
        }
    }

    void Start()
    {
        if (stats == null)
        {
            stats = PlayerStats.Instance;
        }
    }

    public void TakeDamage(int damage)
    {
        // Prevent damage if already dead
        if (isDead || stats == null || stats.health <= 0)
            return;

        Debug.Log("Taking damage: " + damage);

        // Apply damage
        stats.ModifyHealth(-damage);

        // DEAD
        if (stats.health <= 0)
        {
            Die();
            return;
        }

        // HIT animation only if alive
        if (animator != null)
        {
            animator.SetTrigger("isHit");
        }
    }

    private void Die()
    {
        if (isDead)
            return;

        Debug.Log("PLAYER DIED");

        isDead = true;

        // Lock movement
        if (playerController != null)
        {
            playerController.movementLocked = true;
        }

        // Disable attack
        if (playerAttack != null)
        {
            playerAttack.enabled = false;
        }

        // Disable CharacterController
        if (cc != null)
        {
            cc.enabled = false;
        }

        // Stop movement animations
        animator.SetBool("IsWalking", false);
        animator.SetBool("IsRunning", false);

        // FORCE PLAY death animation directly
        animator.CrossFade("Dead", 0.1f);

        StartCoroutine(HandleDeath());
    }

    IEnumerator HandleDeath()
    {
        // Wait for death animation
        yield return new WaitForSeconds(4f);

        Respawn();
    }

    void Respawn()
    {
        Debug.Log("Respawning...");

        // Load respawn position
        float x = PlayerPrefs.GetFloat("PosX");
        float y = PlayerPrefs.GetFloat("PosY");
        float z = PlayerPrefs.GetFloat("PosZ");

        // Move player
        transform.position = new Vector3(x, y, z);

        // Restore health
        stats.health = stats.maxHealth * 0.5f;

        // Inventory penalty
        PlayerInventory.Instance.ApplyDeathPenalty();

        // Exit death animation
        animator.SetBool("IsDead", false);

        // Re-enable player
        EnablePlayer();
    }

    void EnablePlayer()
    {
        // Enable controller
        if (cc != null)
        {
            cc.enabled = true;
        }

        // Unlock movement
        if (playerController != null)
        {
            playerController.movementLocked = false;
        }

        // Enable attack
        if (playerAttack != null)
        {
            playerAttack.enabled = true;
        }

        isDead = false;

        Debug.Log("Player Respawned");
    }
}