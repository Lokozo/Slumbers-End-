using UnityEngine;

public class BackgroundNPC : MonoBehaviour
{
    [Header("Animations")]
    [SerializeField] private string[] idleAnimations;

    [SerializeField] private Animator animator;

    [Header("Behavior")]
    [SerializeField] private bool randomIdle = true;

    [SerializeField] private float minIdleTime = 5f;
    [SerializeField] private float maxIdleTime = 12f;

    private void Start()
    {
        if (randomIdle)
        {
            Invoke(nameof(PlayRandomIdle), Random.Range(1f, 3f));
        }
    }

    private void PlayRandomIdle()
    {
        if (idleAnimations.Length == 0 || animator == null)
            return;

        string anim = idleAnimations[Random.Range(0, idleAnimations.Length)];

        animator.Play(anim);

        Invoke(nameof(PlayRandomIdle),
            Random.Range(minIdleTime, maxIdleTime));
    }
}