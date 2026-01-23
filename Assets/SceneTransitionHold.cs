using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class SceneTransitionHold : MonoBehaviour
{
    [SerializeField] private string sceneToLoad;  // Target scene name
    [SerializeField] private float holdDuration = 1f; // Seconds to hold

    private InputSystem_Actions inputActions;
    private float holdTimer = 0f;
    private bool isHolding = false;
    private bool playerInside = false; // NEW — track if player is in trigger

    private void OnEnable()
    {
        inputActions = new InputSystem_Actions();
        inputActions.Enable();
        inputActions.Player.Interact.performed += OnInteractStarted;
        inputActions.Player.Interact.canceled += OnInteractCanceled;
    }

    private void OnDisable()
    {
        inputActions.Player.Interact.performed -= OnInteractStarted;
        inputActions.Player.Interact.canceled -= OnInteractCanceled;
        inputActions.Disable();
    }

    private void Update()
    {
        if (isHolding && playerInside) // ✅ only count hold inside trigger
        {
            holdTimer += Time.deltaTime;
            Debug.Log($"Holding Interact... {holdTimer:F2} / {holdDuration} seconds");

            if (holdTimer >= holdDuration)
            {
                Debug.Log("Hold duration reached — loading scene.");
                LoadScene();
                isHolding = false;
            }
        }
    }

    private void OnInteractStarted(InputAction.CallbackContext context)
    {
        if (!playerInside) // ✅ prevent holding outside trigger
        {
            Debug.Log("Tried to interact, but player is not in trigger.");
            return;
        }

        isHolding = true;
        holdTimer = 0f;
        Debug.Log("Interact key pressed — starting hold timer.");
    }

    private void OnInteractCanceled(InputAction.CallbackContext context)
    {
        if (isHolding)
        {
            Debug.Log("Interact key released — hold timer reset.");
        }

        isHolding = false;
        holdTimer = 0f;
    }

    private void LoadScene()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.Log($"Loading scene: {sceneToLoad}");
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogWarning("Scene name not set in Inspector!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = true; // ✅ mark as inside
        Debug.Log("Player entered scene transition trigger.");
        inputActions.Player.Interact.Enable();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = false; // ✅ mark as outside
        Debug.Log("Player left scene transition trigger — disabling input.");
        inputActions.Player.Interact.Disable();
        isHolding = false;
        holdTimer = 0f;
    }
}
