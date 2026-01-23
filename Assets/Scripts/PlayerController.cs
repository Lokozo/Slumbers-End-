using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private InputSystem_Actions playerInputs;
    private Animator animator;
    private CharacterController controller;

    public float speed = 5f;
    public float origSpeed = 5f;
    public float runMulti = 1.25f;
    public float gravity = -9.81f;
    public float jumpHeight = 2f;
    public float rotationFactorPerFrame = 150f;

    private Vector2 currentMovementInput;
    private Vector3 currentMovement;
    private Vector3 velocity;

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

    public bool isClimbing = false; // shared with PlayerClimb

    private void Awake()
    {
        playerInputs = new InputSystem_Actions();
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        InitializeInputActions();

        if (controller == null)
            Debug.LogError("CharacterController missing from Player!");
        if (animator == null)
            Debug.LogError("Animator missing from Player!");
    }

    private void OnEnable() => playerInputs.Player.Enable();
    private void OnDisable() => playerInputs.Player.Disable();

    private void Update()
    {
        if (isClimbing)
        {
            animator.SetBool("IsWalking", false);
            return;
        }

        MovePlayer();
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
        currentMovement.x = currentMovementInput.x;
        currentMovement.z = currentMovementInput.y;
        isMovementPressed = currentMovementInput.sqrMagnitude > 0.01f;
    }

    private void HandleJumpInput(InputAction.CallbackContext context)
    {
        if (isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    private void MovePlayer()
{
        if (!controller.enabled)
            return;

    bool isWalking = animator.GetBool("IsWalking");
    bool isRunning = animator.GetBool("IsRunning");

    if (isMovementPressed && !isWalking) animator.SetBool("IsWalking", true);
    else if (!isMovementPressed && isWalking) animator.SetBool("IsWalking", false);

    if (isMovementPressed)
    {
        if (isRunPressed)
        {
            speed = origSpeed * runMulti;
            if (!isRunning) animator.SetBool("IsRunning", true);
        }
        else
        {
            speed = origSpeed;
            if (isRunning) animator.SetBool("IsRunning", false);
        }
    }
    else if (isRunning)
    {
        animator.SetBool("IsRunning", false);
    }

    Vector3 moveDirection = new Vector3(currentMovementInput.x, 0f, currentMovementInput.y);
    Vector3 move = moveDirection.normalized * speed * Time.deltaTime;

    if (moveDirection.sqrMagnitude > 0.01f)
    {
        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationFactorPerFrame * Time.deltaTime);
    }

    controller.Move(move + velocity * Time.deltaTime);
}


    private void HandleGroundedCheck()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
    }

    private void ApplyGravity()
    {
        if (!controller.enabled) return;

        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }


    private void ToggleWeapon(WeaponType weaponType)
    {
        if (equippedWeapon == weaponType)
        {
            UnequipWeapon(weaponType);
        }
        else
        {
            StartCoroutine(SwitchWeapon(weaponType));
        }
    }

    private IEnumerator SwitchWeapon(WeaponType newWeapon)
    {
        if (equippedWeapon != WeaponType.None)
        {
            yield return StartCoroutine(UnequipWeaponRoutine(equippedWeapon));
        }

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
        Invoke(nameof(SetLayerWeightZero), 1.5f);
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

    private void SetLayerWeightZero()
    {
        SetLayerWeight("Arms", 0f);
    }

    private void SetLayerWeight(string layerName, float weight)
    {
        int layerIndex = animator.GetLayerIndex(layerName);
        if (layerIndex >= 0)
            animator.SetLayerWeight(layerIndex, weight);
    }

    private IEnumerator SmoothLayerWeightTransition(string layerName, float targetWeight, float duration)
    {
        int layerIndex = animator.GetLayerIndex(layerName);
        float currentWeight = animator.GetLayerWeight(layerIndex);
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float newWeight = Mathf.Lerp(currentWeight, targetWeight, elapsedTime / duration);
            animator.SetLayerWeight(layerIndex, newWeight);
            yield return null;
        }

        animator.SetLayerWeight(layerIndex, targetWeight);
    }

    private void HandleInteract()
    {
        if (nearCampArea != null && !campActive)
        {
            Debug.Log("Player starting camp setup...");
            nearCampArea.ActivateCamp();    ///
            campActive = true;
        }
    }

    public void SetCampZone(CampArea area)
    {
        nearCampArea = area;
    }

    public void ClearCampZone()
    {
        nearCampArea = null;
        campActive = false;
    }
    public void ResetVelocity()
    {
        velocity = Vector3.zero;
    }
}
