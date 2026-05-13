using UnityEngine;

public class PlayerClimbing : MonoBehaviour
{
    private PlayerController playerController;
    private CharacterController characterController;
    private Ladder currentLadder;
    private Animator animator;

    [Header("Settings")]
    public float climbSpeed = 4f;
    public float detectionRange = 1.5f;
    public LayerMask ladderLayer;

    private bool isExitingLadder;
    private Vector3 ladderExitTarget;

    [Header("Exit Settings")]
    public float ladderExitSpeed = 2.5f;
    public float ladderExitStopDistance = 0.05f;

    public bool isClimbing { get; private set; }

    private static readonly int IsClimbing = Animator.StringToHash("IsClimbing");
    private static readonly int ClimbSpeed = Animator.StringToHash("ClimbSpeed");

    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (isExitingLadder)
        {
            MoveOutOfLadder();
            return;
        }

        if (isClimbing)
            HandleClimbing();
    }

    public void ToggleClimb()
    {
        if (isClimbing) return;

        if (currentLadder != null)
        {
            StartClimbing();
            return;
        }

        Collider[] hits = Physics.OverlapSphere(
            transform.position + Vector3.up,
            detectionRange,
            ladderLayer
        );

        foreach (Collider hit in hits)
        {
            Ladder ladder = hit.GetComponentInParent<Ladder>();

            if (ladder != null)
            {
                currentLadder = ladder;
                StartClimbing();
                break;
            }
        }
    }

    private void StartClimbing()
    {
        if (isClimbing || currentLadder == null)
            return;

        isClimbing = true;

        playerController.isClimbing = true;
        playerController.movementLocked = true;
        playerController.ResetVelocity();

        animator.SetBool(IsClimbing, true);
        animator.SetFloat(ClimbSpeed, 0.5f); // idle start

        characterController.enabled = false;

        Vector3 pos = transform.position;

        pos.x = currentLadder.climbPoint.position.x;
        pos.z = currentLadder.climbPoint.position.z;

        float top = currentLadder.topExit.position.y;
        float bottom = currentLadder.bottomExit.position.y;

        pos.y = (Mathf.Abs(transform.position.y - top) <
                 Mathf.Abs(transform.position.y - bottom))
                 ? top
                 : bottom - 0.5f;

        transform.position = pos;

        FaceLadder();
    }

    private void StopClimbing()
    {
        animator.SetBool(IsClimbing, false);
        animator.SetFloat(ClimbSpeed, 0.5f);

        isClimbing = false;

        playerController.isClimbing = false;
        playerController.movementLocked = false;

        characterController.enabled = true;
    }

    private void HandleClimbing()
    {
        if (currentLadder == null) return;

        float input = Input.GetAxisRaw("Vertical");

        // IMPORTANT: remap -1..1 → 0..1
        float blendValue = Mathf.Clamp01((input + 1f) * 0.5f);

        animator.SetFloat(ClimbSpeed, blendValue, 0.1f, Time.deltaTime);

        float top = currentLadder.topExit.position.y;
        float bottom = currentLadder.bottomExit.position.y;

        Vector3 pos = transform.position;

        pos.y += input * climbSpeed * Time.deltaTime;

        pos.x = currentLadder.climbPoint.position.x;
        pos.z = currentLadder.climbPoint.position.z;

        pos.y = Mathf.Clamp(pos.y, bottom, top);

        transform.position = pos;

        if (Mathf.Abs(pos.y - top) < 0.02f && input > 0.1f)
            StartLadderExit(currentLadder.topExit.position);

        if (input < -0.1f && pos.y <= bottom + 0.02f)
            StartLadderExit(currentLadder.bottomExit.position);
    }

    private void StartLadderExit(Vector3 target)
    {
        StopClimbing();

        characterController.enabled = true;

        isExitingLadder = true;
        ladderExitTarget = target;

        Vector3 dir = ladderExitTarget - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(dir);
    }

    private void MoveOutOfLadder()
    {
        Vector3 toTarget = ladderExitTarget - transform.position;
        toTarget.y = 0f;

        if (toTarget.magnitude <= ladderExitStopDistance)
        {
            isExitingLadder = false;
            currentLadder = null;
            return;
        }

        float speed = Mathf.Lerp(0.5f, ladderExitSpeed, toTarget.magnitude);

        characterController.Move(toTarget.normalized * speed * Time.deltaTime);
    }

    private void FaceLadder()
    {
        if (currentLadder == null) return;

        Vector3 dir = currentLadder.FaceDirection;
        dir.y = 0f;

        transform.rotation = Quaternion.LookRotation(dir);
    }
}