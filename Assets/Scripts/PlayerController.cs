using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private InputSystem_Actions playerInputs;
    private Animator animator;
    private CharacterController controller;

    [Header("Weapon Data Assets")]
    public WeaponItem axeData;
    public WeaponItem gunData;

    public float speed = 5f;
    public float origSpeed = 5f;
    public float runMulti = 1.25f;
    public float gravity = -9.81f;
    public float jumpHeight = 2f;
    public float rotationFactorPerFrame = 150f;
    float energyCostPerSecond = 10f;

    public bool movementLocked = false;
    public Vector2 currentMovementInput;
    private Vector3 velocity;

    public bool isClimbing = false;
    public Ladder currentLadder;

    private bool isMovementPressed;
    private bool isRunPressed = false;
    private bool isGrounded;

    private bool campActive = false;
    private CampArea nearCampArea;

    public GameObject axe;
    public GameObject gun;
    public Transform weaponGunEquip;
    public Transform weaponAxeEquip;
    public Transform weaponUnequipBackpack;
    public Transform weaponUnequipHip;

    private enum WeaponType { None, Axe, Gun }
    private WeaponType equippedWeapon = WeaponType.None;
    private float transitionDuration = 0.5f;

    private void Awake()
    {
        playerInputs = new InputSystem_Actions();
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        InitializeInputActions();
    }

    private void OnEnable() => playerInputs.Player.Enable();
    private void OnDisable() => playerInputs.Player.Disable();

    private void Update()
    {
        if (isClimbing || movementLocked)
        {
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsRunning", false);
            return;
        }

        var pushPull = GetComponent<PlayerPushPull>();
        bool isPushing = pushPull != null && pushPull.IsPushing;

        // HANDLE PUSHABLE LAYER WEIGHT - COMMENTED OUT

        int pushLayerIndex = animator.GetLayerIndex("Pushable");
        if (pushLayerIndex != -1)
        {
            float targetWeight = isPushing ? 1f : 0f;
            float currentWeight = animator.GetLayerWeight(pushLayerIndex);
            animator.SetLayerWeight(pushLayerIndex, Mathf.MoveTowards(currentWeight, targetWeight, Time.deltaTime * 5f));
        }


        if (isPushing)
        {
            pushPull.UpdatePushMovement(currentMovementInput);
        }

        MovePlayer(isPushing);
        ApplyGravity();
        HandleGroundedCheck();
    }

    private void InitializeInputActions()
    {
        playerInputs.Player.Move.performed += HandleMovementInput;
        playerInputs.Player.Move.canceled += HandleMovementInput;
        playerInputs.Player.Jump.performed += HandleJumpInput;
        playerInputs.Player.Sprint.started += ctx => isRunPressed = true;
        playerInputs.Player.Sprint.canceled += ctx => isRunPressed = false;
        playerInputs.Player.EquipAxe.performed += ctx => ToggleWeapon(WeaponType.Axe);
        playerInputs.Player.EquipGun.performed += ctx => ToggleWeapon(WeaponType.Gun);
        playerInputs.Player.Interact.performed += ctx => HandleInteract();
    }

    private void HandleMovementInput(InputAction.CallbackContext context)
    {
        currentMovementInput = context.ReadValue<Vector2>();
        isMovementPressed = currentMovementInput.sqrMagnitude > 0.01f;
    }

    private void HandleJumpInput(InputAction.CallbackContext context)
    {
        if (isGrounded) velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }

    private void MovePlayer(bool pushing)
    {
        if (!controller.enabled) return;

        // 1. Calculate Speed
        if (isRunPressed && isMovementPressed && !pushing)
        {
            speed = origSpeed * runMulti;
            // Null check to prevent the crash you were seeing
            if (PlayerStats.Instance != null)
                PlayerStats.Instance.ModifyEnergy(-energyCostPerSecond * Time.deltaTime);

            animator.SetBool("IsRunning", true);
        }
        else
        {
            speed = pushing ? origSpeed * 0.5f : origSpeed;
            animator.SetBool("IsRunning", false);
        }

        animator.SetBool("IsWalking", isMovementPressed);

        // 2. Direction logic
        Vector3 moveDir = new Vector3(currentMovementInput.x, 0f, currentMovementInput.y);  // Use both X and Y for all cases (pushing or not)

        // 3. Apply Rotation (only if not pushing)
        if (moveDir.sqrMagnitude > 0.01f && !pushing)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationFactorPerFrame * Time.deltaTime);
        }

        // 4. Final Move
        controller.Move(moveDir.normalized * speed * Time.deltaTime + velocity * Time.deltaTime);
    }

    private void HandleGroundedCheck()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0) velocity.y = -2f;
    }

    private void ApplyGravity()
    {
        if (!controller.enabled) return;
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    // --- WEAPONS AND INTERACTION ---

    private void ToggleWeapon(WeaponType weaponType)
    {
        if (equippedWeapon == weaponType) UnequipWeapon(weaponType);
        else StartCoroutine(SwitchWeapon(weaponType));
    }

    private IEnumerator SwitchWeapon(WeaponType newWeapon)
    {
        if (equippedWeapon != WeaponType.None) yield return StartCoroutine(UnequipWeaponRoutine(equippedWeapon));
        equippedWeapon = newWeapon;

        if (newWeapon == WeaponType.Axe)
        {
            animator.SetBool("Equip Axe", true);
            EquipWeaponObject(axe, weaponAxeEquip);
            SetLayerWeight("Equip Layer", 1f);
            yield return StartCoroutine(SmoothLayerWeightTransition("Equip Layer", 0f, transitionDuration));
            yield return StartCoroutine(SmoothLayerWeightTransition("Combat Axe", 1f, transitionDuration));
        }
        else if (newWeapon == WeaponType.Gun)
        {
            animator.SetBool("Equip Pistol", true);
            EquipWeaponObject(gun, weaponGunEquip);
            SetLayerWeight("Equip Layer", 1f);
            yield return StartCoroutine(SmoothLayerWeightTransition("Equip Layer", 0f, transitionDuration));
            yield return StartCoroutine(SmoothLayerWeightTransition("Combat Pistol", 1f, transitionDuration));
        }
    }

    private void UnequipWeapon(WeaponType weaponType)
    {
        StartCoroutine(UnequipWeaponRoutine(weaponType));
        equippedWeapon = WeaponType.None;
    }

    private IEnumerator UnequipWeaponRoutine(WeaponType weaponType)
    {
        if (weaponType == WeaponType.Axe)
        {
            animator.SetBool("Equip Axe", false);
            yield return StartCoroutine(SmoothLayerWeightTransition("Combat Axe", 0f, transitionDuration));
            UnequipWeaponObject(axe, weaponUnequipBackpack);
        }
        else if (weaponType == WeaponType.Gun)
        {
            animator.SetBool("Equip Pistol", false);
            yield return StartCoroutine(SmoothLayerWeightTransition("Combat Pistol", 0f, transitionDuration));
            UnequipWeaponObject(gun, weaponUnequipHip);
        }
        yield return StartCoroutine(SmoothLayerWeightTransition("Equip Layer", 1f, transitionDuration));
    }

    private void EquipWeaponObject(GameObject weapon, Transform equipSlot)
    {
        weapon.transform.SetParent(equipSlot);
        weapon.transform.localPosition = Vector3.zero;
        weapon.transform.localRotation = Quaternion.identity;
    }

    private void UnequipWeaponObject(GameObject weapon, Transform unequipSlot)
    {
        weapon.transform.SetParent(unequipSlot);
        weapon.transform.localPosition = Vector3.zero;
        weapon.transform.localRotation = Quaternion.identity;
    }

    private void SetLayerWeight(string layerName, float weight)
    {
        int layerIndex = animator.GetLayerIndex(layerName);
        if (layerIndex >= 0) animator.SetLayerWeight(layerIndex, weight);
    }

    private IEnumerator SmoothLayerWeightTransition(string layerName, float targetWeight, float duration)
    {
        int layerIndex = animator.GetLayerIndex(layerName);
        if (layerIndex < 0) yield break;
        float currentWeight = animator.GetLayerWeight(layerIndex);
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            animator.SetLayerWeight(layerIndex, Mathf.Lerp(currentWeight, targetWeight, elapsedTime / duration));
            yield return null;
        }
        animator.SetLayerWeight(layerIndex, targetWeight);
    }

    private void HandleInteract()
    {
        var pushPull = GetComponent<PlayerPushPull>();
        var climb = GetComponent<PlayerClimbing>();

        if (climb != null)
        {
            // If we are on a ladder OR already climbing, E handles the ladder.
            // We'll need a way to know we are near a ladder, or just let ToggleClimb handle it.
            climb.ToggleClimb();
            if (climb.isClimbing) return; // Don't do other interactions if we just started climbing
        }

        if (pushPull != null)
        {
            pushPull.TogglePushPull();
            if (pushPull.IsPushing) return;
        }

        if (nearCampArea != null && !campActive)
        {
            nearCampArea.ActivateCamp();
            campActive = true;
        }
    }

    public void SetCampZone(CampArea area) => nearCampArea = area;
    public void ClearCampZone() { nearCampArea = null; campActive = false; }
    public void ResetVelocity() => velocity = Vector3.zero;
}