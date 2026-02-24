using UnityEngine;

public class PlayerClimbing : MonoBehaviour
{
    private PlayerController playerController;
    private CharacterController characterController;
    private Ladder currentLadder;

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
        isClimbing = true;
        playerController.isClimbing = true;
        playerController.movementLocked = true;
        playerController.ResetVelocity();

        characterController.enabled = false;

        Vector3 pos = transform.position;
        pos.x = currentLadder.climbPoint.position.x;
        pos.z = currentLadder.climbPoint.position.z;
        transform.position = pos;

        characterController.enabled = true;

        FaceLadder();
    }

    private void StopClimbing()
    {
        isClimbing = false;
        playerController.isClimbing = false;
        playerController.movementLocked = false;
    }


    private void FaceLadder()
    {
        if (currentLadder == null) return;

        Vector3 forward = currentLadder.FaceDirection;
        forward.y = 0f; // keep player upright

        transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
    }

    private void HandleClimbing()
    {
        float yInput = Input.GetAxisRaw("Vertical");

        float topY = currentLadder.topExit.position.y;
        float bottomY = currentLadder.bottomExit.position.y;

        float verticalMove = yInput * climbSpeed * Time.deltaTime;
        characterController.Move(Vector3.up * verticalMove);

        Vector3 pos = transform.position;
        pos.x = currentLadder.climbPoint.position.x;
        pos.z = currentLadder.climbPoint.position.z;
        pos.y = Mathf.Clamp(pos.y, bottomY, topY);
        transform.position = pos;

        if (Mathf.Abs(pos.y - topY) < 0.02f && yInput > 0.1f)
        {
            StartLadderExit(currentLadder.topExit.position);
        }

        if (yInput < -0.1f && pos.y <= bottomY + 0.02f)
        {
            StartLadderExit(currentLadder.bottomExit.position);
        }
    }
    private void StartLadderExit(Vector3 target)
    {
        StopClimbing();

        isExitingLadder = true;
        ladderExitTarget = target;

        // Face the exit direction
        Vector3 dir = ladderExitTarget - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
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

        // Ease out (animation friendly)
        float speed = Mathf.Lerp(0.5f, ladderExitSpeed, distance);
        Vector3 move = toTarget.normalized * speed;

        characterController.Move(move * Time.deltaTime);
    }
    //private void TeleportToPosition(Vector3 pos)
    //{
    //    characterController.enabled = false;
    //    transform.position = pos;
    //    characterController.enabled = true;
    //}

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
        if (ladder != null && ladder == currentLadder && !isClimbing)
            currentLadder = null;
    }
}