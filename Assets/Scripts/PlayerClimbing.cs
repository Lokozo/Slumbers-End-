using UnityEngine;

public class PlayerClimbing : MonoBehaviour
{
    private PlayerController playerController;
    private CharacterController characterController;
    private Ladder currentLadder;

    [Header("Settings")]
    public float climbSpeed = 4f;
    public float detectionRange = 1.5f; // How far away you can "grab" the ladder
    public LayerMask ladderLayer;      // Set this to your "Ladder" layer in the Inspector
    public bool isClimbing { get; private set; }

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (isClimbing)
        {
            HandleClimbing();
        }
    }

    public void ToggleClimb()
    {
        if (isClimbing)
        {
            StopClimbing();
        }
        else
        {
            // DETECT LADDER (Similar to your Push/Pull logic)
            // Shooting a Raycast from the center of the player forward
            Vector3 rayOrigin = transform.position + (Vector3.up * 1f);

            if (Physics.Raycast(rayOrigin, transform.forward, out RaycastHit hit, detectionRange, ladderLayer))
            {
                // Look for the Ladder script on the hit object or its parent
                Ladder ladder = hit.collider.GetComponent<Ladder>() ?? hit.collider.GetComponentInParent<Ladder>();

                if (ladder != null)
                {
                    currentLadder = ladder;
                    Debug.Log("Ladder detected via Raycast: " + ladder.name);
                    StartClimbing();
                }
            }
            else
            {
                Debug.LogWarning("No ladder in front of player.");
            }
        }
    }

    private void StartClimbing()
    {
        isClimbing = true;
        playerController.isClimbing = true;
        playerController.ResetVelocity();

        characterController.enabled = false;

        // Snap to ladder center (X and Z)
        Vector3 snapPos = new Vector3(currentLadder.transform.position.x, transform.position.y, currentLadder.transform.position.z);
        transform.position = snapPos;

        // Face the ladder direction defined in the Ladder script
        transform.rotation = Quaternion.LookRotation(currentLadder.ladderFaceDirection);

        characterController.enabled = true;
    }

    private void StopClimbing()
    {
        isClimbing = false;
        playerController.isClimbing = false;
        currentLadder = null;

        // Face exit direction based on horizontal input
        float x = playerController.currentMovementInput.x;
        Vector3 exitFacing = x >= 0 ? Vector3.right : Vector3.left;
        transform.rotation = Quaternion.LookRotation(exitFacing);
    }

    private void HandleClimbing()
    {
        float yInput = playerController.currentMovementInput.y;
        Vector3 verticalMove = new Vector3(0, yInput * climbSpeed, 0);

        characterController.Move(verticalMove * Time.deltaTime);

        // EXIT TOP
        if (yInput > 0.1f && transform.position.y >= currentLadder.topExit.position.y)
        {
            TeleportToPosition(currentLadder.topExit.position);
            StopClimbing();
        }
        // EXIT BOTTOM
        else if (yInput < -0.1f && transform.position.y <= (currentLadder.bottomExit.position.y + 0.1f))
        {
            StopClimbing();
        }
    }

    private void TeleportToPosition(Vector3 pos)
    {
        characterController.enabled = false;
        transform.position = pos;
        characterController.enabled = true;
    }
}