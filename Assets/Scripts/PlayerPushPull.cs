using UnityEngine;

public class PlayerPushPull : MonoBehaviour
{
    [Header("Detection")]
    public float pushRange = 1f;
    public LayerMask pushableLayer;

    [Header("Movement")]
    public float moveSpeed = 2f;

    [Header("UI")]
    public GameObject pushPromptUI;

    private PushableObject currentObject;
    private Animator animator;

    private bool hasDetectedPushable;

    public bool IsPushing => currentObject != null;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        if (UIManager.Instance != null)
        {
            pushPromptUI = UIManager.Instance.pushPullIcon;
        }
    }

    private void Update()
    {
        DetectPushable();

        if (IsPushing)
        {
            HandlePushPull();
        }
    }

    void DetectPushable()
    {
        // HIDE UI WHILE PUSHING
        if (IsPushing)
        {
            if (pushPromptUI != null)
                pushPromptUI.SetActive(false);

            return;
        }

        // CHECK PLAYER FACING
        Vector3 dir =
            transform.localScale.x > 0
            ? Vector3.right
            : Vector3.left;

        Vector3 origin =
            transform.position + Vector3.up * 0.5f;

        // DETECT PUSHABLE
        if (Physics.Raycast(
            origin,
            dir,
            out RaycastHit hit,
            pushRange,
            pushableLayer))
        {
            PushableObject pushable =
                hit.collider.GetComponent<PushableObject>();

            if (pushable != null)
            {
                // SHOW ICON
                if (pushPromptUI != null)
                    pushPromptUI.SetActive(true);

                // SHOW TUTORIAL ONLY ONCE
                if (!hasDetectedPushable)
                {
                    hasDetectedPushable = true;

                    TutorialUIManager.Instance?.ShowStep(
                        "pushPullTutorial",
                        "Press E to grab moveable objects." +
                        "\nMove to push or pull it."
                    );
                }

                return;
            }
        }

        // HIDE ICON
        if (pushPromptUI != null)
            pushPromptUI.SetActive(false);
    }

    public void TogglePushPull()
    {
        if (IsPushing)
        {
            ReleaseObject();
        }
        else
        {
            GrabObject();
        }
    }

    void GrabObject()
    {
        Vector3 dir =
            transform.localScale.x > 0
            ? Vector3.right
            : Vector3.left;

        Vector3 origin =
            transform.position + Vector3.up * 0.5f;

        if (Physics.Raycast(
            origin,
            dir,
            out RaycastHit hit,
            pushRange,
            pushableLayer))
        {
            currentObject =
                hit.collider.GetComponent<PushableObject>();

            if (currentObject == null)
                return;

            animator.SetBool("IsPushing", true);

            if (pushPromptUI != null)
                pushPromptUI.SetActive(false);
        }
    }

    void ReleaseObject()
    {
        animator.SetBool("IsPushing", false);
        animator.SetFloat("Blend", 0f);

        currentObject = null;
    }

    void HandlePushPull()
    {
        if (currentObject == null)
            return;

        float move =
            Input.GetAxisRaw("Horizontal");

        animator.SetFloat("Blend", move);

        if (Mathf.Abs(move) < 0.1f)
            return;

        Vector3 moveDir =
            Vector3.right * move;

        float moveAmount =
            moveSpeed * Time.deltaTime;

        Collider col =
            currentObject.GetComponent<Collider>();

        if (col == null)
            return;

        Bounds bounds = col.bounds;

        Vector3 boxCenter =
            bounds.center;

        Vector3 halfExtents =
            bounds.extents * 0.95f;

        // CHECK WALL COLLISION
        bool blocked = Physics.BoxCast(
            boxCenter,
            halfExtents,
            moveDir,
            out RaycastHit hit,
            Quaternion.identity,
            moveAmount
        );

        // STOP IF HITTING SOMETHING
        if (blocked)
        {
            if (hit.collider.gameObject != currentObject.gameObject)
                return;
        }

        // MOVE OBJECT
        currentObject.transform.position +=
            moveDir * moveAmount;
    }
}