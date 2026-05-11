using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Enemies/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Info")]
    public string enemyName;

    [Header("Stats")]
    public float health = 100f;
    public float speed = 1.5f;
    public float chaseSpeed = 3f;

    [Header("Attack")]
    public float attackDamage = 15f;
    public float attackInterval = 2f;
    public float attackRadius = 1.5f;

    [Header("Detection")]
    public float chaseRange = 7f;
    public float stoppingDistance = 1.2f;

    [Header("Patrol")]
    public float patrolDistance = 5f;

    [Header("Animation")]
    public RuntimeAnimatorController animatorController;

    [Header("Audio")]
    public AudioClip attackSound;
    public AudioClip hurtSound;
    public AudioClip deathSound;
}