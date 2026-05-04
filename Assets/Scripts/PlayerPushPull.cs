using UnityEngine;

public class PlayerPushPull : MonoBehaviour
{
    public float pushRange = 1.5f;
    public LayerMask pushableLayer;

    [Header("Setup")]
    public Transform pushPoint; // The "Push" empty object from your screenshot

    [Header("UI")]
    public GameObject pushPromptUI;

    private PushableObject currentObject;
    private Animator animator;

    public bool IsPushing => currentObject != null;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    private bool hasDetectedPushable;

    private void Start()
    {
        pushPromptUI = UIManager.Instance.pushPullIcon;
    }

    private void Update()
    {
        if (IsPushing)
        {
            if (pushPromptUI != null)
                pushPromptUI.SetActive(false);
            return;
        }

        Vector3 rayOrigin = transform.position + (Vector3.up * 0.5f) + (transform.forward * 0.2f);

        if (Physics.Raycast(rayOrigin, transform.forward, out RaycastHit hit, pushRange, pushableLayer))
        {
            if (hit.collider.GetComponent<PushableObject>() != null)
            {
                // SHOW ICON
                if (pushPromptUI != null)
                    pushPromptUI.SetActive(true);

                //  SHOW TUTORIAL (only once)
                TutorialUIManager.Instance?.ShowStep(
                    "pushPullTutorial",
                    "Press E to grab objects. Move to push or pull."
                );

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
            // RELEASE
            currentObject.SetPhysics(true); // Re-enable physics
            currentObject.transform.SetParent(null);
            currentObject = null;
            animator.SetBool("IsPushing", false);
        }
        else
        {
            // ATTACH
            Vector3 rayOrigin = transform.position + (Vector3.up * 0.5f) + (transform.forward * 0.2f);
            
            if (Physics.Raycast(rayOrigin, transform.forward, out RaycastHit hit, pushRange, pushableLayer))
            {
                currentObject = hit.collider.GetComponent<PushableObject>();
                
                if (currentObject != null)
                {
                    currentObject.SetPhysics(false); // Disable physics so it follows player perfectly

                    currentObject.transform.SetParent(pushPoint);
                    currentObject.transform.localPosition = Vector3.zero;
                    currentObject.transform.localRotation = Quaternion.identity;

                    animator.SetBool("IsPushing", true);
                
                }
            }
        }
    }

    public void UpdatePushMovement(Vector2 moveInput)
    {
        if (!IsPushing) return;

        // Dot product checks if you are moving in the direction you face
        float moveValue = Vector3.Dot(new Vector3(moveInput.x, 0, moveInput.y), transform.forward);

        // Use "Blend" to match your Animator screenshot
        animator.SetFloat("Blend", moveValue);
    }
}