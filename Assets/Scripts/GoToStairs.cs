using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class GoToStairs : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float rotateSpeed = 8f;

    [Header("Camera")]
    public Transform cameraTransform;

    [Header("Gravity")]
    public float gravity = -25f;
    public float groundedStick = -2f;

    private CharacterController controller;

    private Vector3 pathTarget;
    private bool hasPath;
    private bool isMovingPath;

    private float verticalVelocity;
    private float verticalInput;

    public bool IsMovingByStairs { get; private set; }
    public bool CanExit { get; private set; }

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        verticalInput = Input.GetAxisRaw("Vertical");

        ApplyGravity();

        if (isMovingPath)
            FollowPath();
    }

    // =========================
    // PUBLIC ENTRY
    // =========================

    public void GoToPointFromBottom(Transform target)
    {
        SetPath(target);
    }

    public void GoToPointFromTop(Transform target)
    {
        SetPath(target);
    }

    public void GoToExitPoint(Transform target)
    {
        SetPath(target);
    }

    // =========================
    // PATH SYSTEM
    // =========================

    private void SetPath(Transform target)
    {
        pathTarget = target.position;
        hasPath = true;
        isMovingPath = true;

        IsMovingByStairs = true;
        CanExit = false;
    }

    private void FollowPath()
    {
        Vector3 current = transform.position;

        Vector3 toTarget = pathTarget - current;
        toTarget.y = 0f;

        float distance = toTarget.magnitude;

        // =========================
        // ARRIVAL = HARD STOP
        // =========================
        if (distance < 0.03f)
        {
            isMovingPath = false;
            hasPath = false;

            IsMovingByStairs = false;
            verticalVelocity = 0f;

            CanExit = true;

            return;
        }

        // =========================
        // MOVE ALONG PATH
        // =========================
        Vector3 move = toTarget.normalized * moveSpeed * Time.deltaTime;
        controller.Move(move);

        // rotate toward target
        if (toTarget.sqrMagnitude > 0.001f)
        {
            Quaternion rot = Quaternion.LookRotation(toTarget);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                rot,
                Time.deltaTime * rotateSpeed
            );
        }
    }

    // =========================
    // GRAVITY
    // =========================

    private void ApplyGravity()
    {
        if (IsMovingByStairs)
            return;

        if (controller.isGrounded)
        {
            if (verticalVelocity < 0f)
                verticalVelocity = groundedStick;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);
    }

    // =========================
    // CAMERA DIRECTION (optional)
    // =========================

    public Vector3 GetCameraForward()
    {
        if (cameraTransform == null)
            return Vector3.forward;

        Vector3 f = cameraTransform.forward;
        f.y = 0f;
        return f.normalized;
    }
}
/*
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class GoToStairs : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float rotateSpeed = 8f;

    [Header("Gravity")]
    public float gravity = -25f;
    public float groundedStick = -2f;

    private CharacterController controller;
    private Coroutine moveRoutine;

    private float verticalVelocity;
    private float verticalInput;

    public bool IsMovingByStairs { get; private set; }
    public bool JustFinishedClimb { get; private set; }
    public bool CanExit { get; private set; }

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        verticalInput = Input.GetAxisRaw("Vertical");

        // 🔑 THIS IS WHY YOU WERE FLOATING
            if (!IsMovingByStairs && controller.enabled)
            {
                ApplyGravity();
            }
    }

    // =======================
    // STAIR ENTRY
    // =======================

    public void GoToPointFromBottom(Transform target)
    {
        StartMove(target);
    }

    public void GoToPointFromTop(Transform target)
    {
        StartMove(target);
    }

    public void GoToExitPoint(Transform target)
    {
        StartMove(target);
    }

    private void StartMove(Transform target)
    {
        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        moveRoutine = StartCoroutine(MoveRoutine(target));
    }

    // =======================
    // STAIR MOVEMENT
    // =======================

    IEnumerator MoveRoutine(Transform target)
    {
        IsMovingByStairs = true;
        CanExit = false;
        JustFinishedClimb = false;

        controller.enabled = false;

        while (Vector3.Distance(transform.position, target.position) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                target.position,
                moveSpeed * Time.deltaTime
            );

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                target.rotation,
                Time.deltaTime * rotateSpeed
            );

            yield return null;
        }

        transform.position = target.position;
        transform.rotation = target.rotation;

        controller.enabled = true;

        // 🔑 RESET gravity so it SLIDES
        verticalVelocity = 0f;

        IsMovingByStairs = false;
        JustFinishedClimb = true;

        yield return null;
        yield return null;

        JustFinishedClimb = false;

        yield return new WaitUntil(() => Mathf.Abs(verticalInput) > 0.1f);
        CanExit = true;

        moveRoutine = null;
    }

    // =======================
    // GRAVITY (THIS IS THE KEY)
    // =======================

    private void ApplyGravity()
{
    // 🚨 FIX: don't run if controller is disabled
    if (controller == null || !controller.enabled)
        return;

    if (controller.isGrounded)
    {
        if (verticalVelocity < 0f)
            verticalVelocity = groundedStick;
    }
    else
    {
        verticalVelocity += gravity * Time.deltaTime;
    }

    controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);
}

    // =======================
    // EXIT INTENT
    // =======================

    public bool PlayerIsTryingToMove()
    {
        return Mathf.Abs(verticalInput) > 0.5f;
    }
}
*/