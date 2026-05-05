using UnityEngine;
using System.Collections;

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
        if (isDead) return;

        isDead = true;

        animator.SetTrigger("Die");

        StartCoroutine(HandleDeath());
    }
    IEnumerator HandleDeath()
    {
        // wait for death animation
        yield return new WaitForSeconds(2f);

        Respawn();
    }
    void EnablePlayer()
    {
        isDead = false;

        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();

        foreach (var script in scripts)
        {
            script.enabled = true;
        }

        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = true;

        Debug.Log("Player Respawned");
    }
    void Respawn()
    {
        Debug.Log("Respawning...");

        // 🔄 Load saved position
        float x = PlayerPrefs.GetFloat("PosX");
        float y = PlayerPrefs.GetFloat("PosY");
        float z = PlayerPrefs.GetFloat("PosZ");

        transform.position = new Vector3(x, y, z);

        // ❤️ Restore some health (optional)
        stats.health = stats.maxHealth * 0.5f;

        // 🎒 Apply inventory penalty
        PlayerInventory.Instance.ApplyDeathPenalty();

        // 🔓 Re-enable player
        EnablePlayer();
    }
}
