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
        animator.SetBool("IsClimbing", true);

        // RESET LADDER ANIMATION BOOLS
        animator.SetBool("ClimbUp", false);
        animator.SetBool("ClimbDown", false);

        int ladderLayer = animator.GetLayerIndex("Ladder layer");

        if (ladderLayer != -1)
        {
            animator.SetLayerWeight(ladderLayer, 1f);
        }

        isClimbing = true;

        playerController.isClimbing = true;
        playerController.movementLocked = true;
        playerController.ResetVelocity();

        Physics.IgnoreLayerCollision(
            gameObject.layer,
            LayerMask.NameToLayer("Ladder"),
            true
        );

        characterController.enabled = false;

        Vector3 pos = transform.position;

        // LOCK PLAYER TO LADDER CENTER
        pos.x = currentLadder.climbPoint.position.x;
        pos.z = currentLadder.climbPoint.position.z;

        float distanceToTop =
            Mathf.Abs(transform.position.y - currentLadder.topExit.position.y);

        float distanceToBottom =
            Mathf.Abs(transform.position.y - currentLadder.bottomExit.position.y);

        // PLAYER IS CLOSER TO TOP
        if (distanceToTop < distanceToBottom)
        {
            pos.y = currentLadder.topExit.position.y;
            transform.position = pos;
        }
        else
        {
            // PLAYER IS CLOSER TO BOTTOM
            pos.y = currentLadder.bottomExit.position.y - 0.5f;
            transform.position = pos;
        }

        FaceLadder();
    }

    private void StopClimbing()
    {
        animator.SetBool("IsClimbing", false);

        animator.SetBool("ClimbUp", false);
        animator.SetBool("ClimbDown", false);

        int ladderLayer = animator.GetLayerIndex("Ladder layer");

        if (ladderLayer != -1)
        {
            animator.SetLayerWeight(ladderLayer, 0f);
        }

        isClimbing = false;

        playerController.isClimbing = false;
        playerController.movementLocked = false;

        Physics.IgnoreLayerCollision(
            gameObject.layer,
            LayerMask.NameToLayer("Ladder"),
            false
        );

        characterController.enabled = true;
    }

    private void FaceLadder()
    {
        if (currentLadder == null) return;

        Vector3 forward = currentLadder.FaceDirection;

        forward.y = 0f;

        transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
    }

    private void HandleClimbing()
    {
        float yInput = Input.GetAxisRaw("Vertical");

        // ANIMATION
        if (yInput > 0.1f)
        {
            animator.SetBool("ClimbUp", true);
            animator.SetBool("ClimbDown", false);
        }
        else if (yInput < -0.1f)
        {
            animator.SetBool("ClimbUp", false);
            animator.SetBool("ClimbDown", true);
        }
        else
        {
            animator.SetBool("ClimbUp", false);
            animator.SetBool("ClimbDown", false);
        }

        float topY = currentLadder.topExit.position.y;
        float bottomY = currentLadder.bottomExit.position.y;

        float verticalMove = yInput * climbSpeed * Time.deltaTime;

        // MOVE PLAYER
        transform.position += Vector3.up * verticalMove;

        // LOCK PLAYER TO LADDER
        Vector3 pos = transform.position;

        pos.x = currentLadder.climbPoint.position.x;
        pos.z = currentLadder.climbPoint.position.z;

        pos.y = Mathf.Clamp(pos.y, bottomY, topY);

        transform.position = pos;

        // TOP EXIT
        if (Mathf.Abs(pos.y - topY) < 0.02f && yInput > 0.1f)
        {
            StartLadderExit(currentLadder.topExit.position);
        }

        // BOTTOM EXIT
        if (yInput < -0.1f && pos.y <= bottomY + 0.02f)
        {
            StartLadderExit(currentLadder.bottomExit.position);
        }
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
        {
            transform.rotation =
                Quaternion.LookRotation(dir, Vector3.up);
        }
    }

    private void MoveOutOfLadder()
    {
        Vector3 toTarget = ladderExitTarget - transform.position;

        toTarget.y = 0f;

        float distance = toTarget.magnitude;

        if (distance <= ladderExitStopDistance)
        {
            isExitingLadder = false;
            currentLadder = null;
            return;
        }

        float speed = Mathf.Lerp(0.5f, ladderExitSpeed, distance);

        Vector3 move = toTarget.normalized * speed;

        characterController.Move(move * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isClimbing || isExitingLadder) return;

        Ladder ladder = other.GetComponentInParent<Ladder>();

        if (ladder != null)
            currentLadder = ladder;
    }

    private void OnTriggerExit(Collider other)
    {
        if (isExitingLadder) return;

        Ladder ladder = other.GetComponentInParent<Ladder>();

        if (ladder != null &&
            ladder == currentLadder &&
            !isClimbing)
        {
            currentLadder = null;
        }
    }
}