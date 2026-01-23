using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerClimb : MonoBehaviour
{
    private Animator animator;
    private CharacterController controller;
    private InputSystem_Actions inputActions;

    [Header("Detection Settings")]
    public Transform climbCheckPoint;
    public float climbCheckDistance = 0.5f;
    public LayerMask climbableLayer;

    private bool isClimbing = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        inputActions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        inputActions.Enable();
        inputActions.Player.Jump.performed += OnJumpPressed;
    }

    private void OnDisable()
    {
        inputActions.Player.Jump.performed -= OnJumpPressed;
        inputActions.Disable();
    }

    private void OnJumpPressed(InputAction.CallbackContext context)
    {
        if (isClimbing) return;

        // Check for edge
        if (Physics.Raycast(climbCheckPoint.position, transform.forward, out RaycastHit hit, climbCheckDistance, climbableLayer))
        {
            Debug.Log("Climbable detected! Press Space to climb.");
            StartClimb();
        }
        else
        {
            Debug.Log("No climbable ledge detected.");
        }
    }

    private void StartClimb()
    {
        isClimbing = true;
        animator.SetTrigger("ClimbEdge");

        // Disable character controller during climb
        controller.enabled = false;
    }

    // Called via Animation Event at the END of climb animation
    public void OnClimbComplete()
    {
        controller.enabled = true;
        isClimbing = false;

        // Prevent sudden jump after climb
        var player = GetComponent<PlayerController>();
        if (player != null)
        {
            player.ResetVelocity();
        }
    }



    private void OnDrawGizmosSelected()
    {
        if (climbCheckPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(climbCheckPoint.position, climbCheckPoint.position + transform.forward * climbCheckDistance);
        }
    }
}
