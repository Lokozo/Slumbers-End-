using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using static WeaponItem;

public class PlayerController : MonoBehaviour
{
    private InputSystem_Actions playerInputs;
    private Animator animator;
    private CharacterController controller;
    private PlayerStats stats; // Assumed for energy logic
    private PlayerAttack playerAttack;
    private PlayerHealth playerHealth;

    [Header("Weapon Data Assets")]
    public WeaponItem axeData;
    public WeaponItem pistolData;
    public WeaponItem rifleData;
    public WeaponItem shotgunData;
    public WeaponItem akData;
    public WeaponItem knifeData;


    [Header("Movement Settings")]
    public float speed = 5f;
    public float origSpeed = 5f;
    public float runMulti = 1.25f;
    public float gravity = -9.81f;
    public float jumpHeight = 2f;
    public float rotationFactorPerFrame = 150f;
    float energyCostPerSecond = 10f;

    [Header("State Flags")]
    public bool movementLocked = false;
    public bool isClimbing = false;
    private bool isMovementPressed;
    private bool isRunPressed = false;
    private bool isGrounded;

    public Vector2 currentMovementInput;
    private Vector3 velocity;

    [Header("Tutorial System")]
    [SerializeField] private bool enableTutorial = true;
    private bool hasShownMoveTutorial = false;
    private bool hasShownRunTutorial = false;
    private bool hasCompletedMovementTutorial = false;

    [Header("Weapon Objects & Slots")]
    public GameObject axe;
    public GameObject knife;
    public GameObject pistol;
    public GameObject rifle;
    public GameObject shotgun;
    public GameObject ak;

    public Transform weaponPistolEquip;
    public Transform weaponAxeEquip;
    public Transform weaponKnifeEquip;
    public Transform weaponShotgunEquip;
    public Transform weaponRifleEquip;
    public Transform weaponAKEquip;

    public Transform weaponUnequipBackpack;

    public Transform weaponUnequipHip;
    public Transform weaponUnequipLeft;
    public Transform weaponUnequipRifle;
    public Transform weaponUnequipAK;
    public Transform weaponUnequipShotgun;

    private enum WeaponType
    {
        None,
        Axe,
        Knife,
        Pistol,
        Rifle,
        Shotgun,
        AK
    }
    private WeaponType equippedWeapon = WeaponType.None;
    private float transitionDuration = 0.5f;

    //[Header("Interaction & Camp")]
    //private bool campActive = false;
    //private CampArea nearCampArea;

    private void Awake()
    {
        playerInputs = new InputSystem_Actions();
        controller = GetComponent<CharacterController>();
        playerAttack = GetComponent<PlayerAttack>();
        animator = GetComponent<Animator>();
        stats = GetComponent<PlayerStats>();
        playerHealth = GetComponent<PlayerHealth>();

        InitializeInputActions();
    }

    private void Start()
    {
        if (enableTutorial && TutorialUIManager.Instance != null)
        {
            TutorialUIManager.Instance.ShowStep("moveTutorial", "Press WASD to move");
        }
    }

    private void OnEnable() => playerInputs.Player.Enable();
    private void OnDisable() => playerInputs.Player.Disable();

    private void Update()
    {
        if (playerHealth != null && playerHealth.IsDead)
        {
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsRunning", false);
            return;
        }
        if (isClimbing || movementLocked || (playerAttack != null && playerAttack.IsAttacking()))
        {
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsRunning", false);
            return;
        }

        // Handle Push/Pull Logic
        var pushPull = GetComponent<PlayerPushPull>();
        bool isPushing = pushPull != null && pushPull.IsPushing;

        HandlePushLayerWeight(isPushing);

        if (isPushing)
        {
            pushPull.UpdatePushMovement(currentMovementInput);
        }

        // Tutorial Check
        if (enableTutorial && !hasCompletedMovementTutorial)
        {
            CheckRunTutorial();
        }

        MovePlayer(isPushing);
        ApplyGravity();
        HandleGroundedCheck();
    }
    
    private void InitializeInputActions()
    {
        playerInputs.Player.Move.performed += HandleMovementInput;
        playerInputs.Player.Move.canceled += HandleMovementInput;

        //playerInputs.Player.Jump.performed += HandleJumpInput;
        playerInputs.Player.Sprint.started += ctx => isRunPressed = true;
        playerInputs.Player.Sprint.canceled += ctx => isRunPressed = false;

        playerInputs.Player.EquipAxe.performed += ctx => ToggleWeapon(WeaponType.Axe);
        playerInputs.Player.EquipKnife.performed += ctx => ToggleWeapon(WeaponType.Knife);

        playerInputs.Player.EquipPistol.performed += ctx => ToggleWeapon(WeaponType.Pistol);

        playerInputs.Player.EquipRifle.performed +=
            ctx => ToggleWeapon(WeaponType.Rifle);

        playerInputs.Player.EquipShotgun.performed +=
            ctx => ToggleWeapon(WeaponType.Shotgun);

        playerInputs.Player.EquipAK.performed +=
            ctx => ToggleWeapon(WeaponType.AK);
        playerInputs.Player.Interact.performed += ctx => HandleInteract();
    }

    private void HandleMovementInput(InputAction.CallbackContext context)
    {
        currentMovementInput = context.ReadValue<Vector2>();
        isMovementPressed = currentMovementInput.sqrMagnitude > 0.01f;

        if (enableTutorial && isMovementPressed && !hasShownMoveTutorial)
        {
            hasShownMoveTutorial = true;
            TutorialUIManager.Instance?.ShowStep("movementRunTutorial", "Hold Left Shift while moving to run");
        }
    }

    private void HandleJumpInput(InputAction.CallbackContext context)
    {
        if (isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    private void MovePlayer(bool pushing)
    {
        if (!controller.enabled) return;

        // 1. Determine Speed & Energy Consumption
        if (isRunPressed && isMovementPressed && !pushing)
        {
            speed = origSpeed * runMulti;
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
        Vector3 moveDir = new Vector3(currentMovementInput.x, 0f, currentMovementInput.y);

        // 3. Apply Rotation (only if not pushing AND not climbing)
        if (moveDir.sqrMagnitude > 0.01f && !pushing && !isClimbing)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                rotationFactorPerFrame * Time.deltaTime
            );
        }

        // 4. Final Movement
        controller.Move(moveDir.normalized * speed * Time.deltaTime + velocity * Time.deltaTime);
    }

    private void HandlePushLayerWeight(bool isPushing)
    {
        int pushLayerIndex = animator.GetLayerIndex("Pushable");
        if (pushLayerIndex != -1)
        {
            float targetWeight = isPushing ? 1f : 0f;
            float currentWeight = animator.GetLayerWeight(pushLayerIndex);
            animator.SetLayerWeight(pushLayerIndex, Mathf.MoveTowards(currentWeight, targetWeight, Time.deltaTime * 5f));
        }
    }

    private void CheckRunTutorial()
    {
        if (hasShownMoveTutorial && !hasShownRunTutorial)
        {
            if (isMovementPressed && isRunPressed)
            {
                hasShownRunTutorial = true;
                hasCompletedMovementTutorial = true;
                TutorialUIManager.Instance?.Hide();
            }
        }
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
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    // --- WEAPONS AND INTERACTION ---

    private void HandleInteract()
    {
        Debug.Log("HandleInteract called!"); // Add this to confirm the method runs
        // 1️⃣ CLIMB FIRST
        var climbing = GetComponent<PlayerClimbing>();
        if (climbing != null)
        {
            climbing.ToggleClimb();
            if (climbing.isClimbing) return;
        }

        // 2️⃣ PUSH / PULL
        var pushPull = GetComponent<PlayerPushPull>();
        if (pushPull != null)
        {
            pushPull.TogglePushPull();
            if (pushPull.IsPushing) return;
        }

        //3️⃣ CAMP
        //if (nearCampArea != null && !campActive)
        //{
        //    nearCampArea.ActivateCamp();
        //    campActive = true;
        //}
    }
    private void UpdateInventoryEquipState(WeaponItem newWeapon)
    {
        foreach (var item in PlayerInventory.Instance.GetInventory().Keys)
        {
            if (item is WeaponItem w)
            {
                w.isEquipped = (w.itemName == newWeapon.itemName);
            }
        }

        PlayerInventory.Instance.OnInventoryChanged?.Invoke();
    }

    private void ToggleWeapon(WeaponType weaponType)
    {
        WeaponItem targetWeapon = null;

        if (weaponType == WeaponType.Knife)
            targetWeapon = knifeData;
        else if (weaponType == WeaponType.Axe)
            targetWeapon = axeData;
        else if (weaponType == WeaponType.Pistol)
            targetWeapon = pistolData;
        else if (weaponType == WeaponType.Rifle)
            targetWeapon = rifleData;
        else if (weaponType == WeaponType.Shotgun)
            targetWeapon = shotgunData;
        else if (weaponType == WeaponType.AK)
            targetWeapon = akData;

        //// 🔥 CHECK INVENTORY FIRST
        if (targetWeapon != null && !PlayerInventory.Instance.HasWeapon(targetWeapon))
        {
            Debug.Log("You don't have this weapon yet!");
            return;
        }

        // If same → UNEQUIP
        if (equippedWeapon == weaponType)
        {
            UnequipWeapon(weaponType);
            return;
        }

        WeaponType previousWeapon = equippedWeapon;

        playerAttack.SetWeapon(targetWeapon);

        StartCoroutine(SwitchWeapon(weaponType, previousWeapon));
    }

    private IEnumerator SwitchWeapon(WeaponType newWeapon, WeaponType previousWeapon)
    {
        ResetAllCombatLayers();

        if (previousWeapon != WeaponType.None)
            yield return StartCoroutine(UnequipWeaponRoutine(previousWeapon));


        // 🔥 RESET ALL FIRST
        animator.SetBool("Equip Axe", false);
        animator.SetBool("Equip Pistol", false);
        animator.SetBool("Equip Knife", false);

        equippedWeapon = newWeapon;

        if (newWeapon == WeaponType.Knife)
        {
            animator.SetBool("Equip Knife", true);
            EquipWeaponObject(knife, weaponKnifeEquip);

            SetLayerWeight("Equip Layer", 1f);
            yield return StartCoroutine(SmoothLayerWeightTransition("Equip Layer", 0f, transitionDuration));
            yield return StartCoroutine(SmoothLayerWeightTransition("Combat Knife", 1f, transitionDuration));

            UpdateInventoryEquipState(knifeData);
        }
        else if(newWeapon == WeaponType.Axe)
        {
            animator.SetBool("Equip Axe", true);
            EquipWeaponObject(axe, weaponAxeEquip);

            SetLayerWeight("Equip Layer", 1f);
            yield return StartCoroutine(SmoothLayerWeightTransition("Equip Layer", 0f, transitionDuration));
            yield return StartCoroutine(SmoothLayerWeightTransition("Combat Axe", 1f, transitionDuration));

            UpdateInventoryEquipState(axeData);
        }
        else if (newWeapon == WeaponType.Pistol)
        {
            animator.SetBool("Combat Pistol", true);
            EquipWeaponObject(pistol, weaponPistolEquip);

            SetLayerWeight("Equip Layer", 1f);
            yield return StartCoroutine(SmoothLayerWeightTransition("Equip Layer", 0f, transitionDuration));
            yield return StartCoroutine(SmoothLayerWeightTransition("Combat Pistol", 1f, transitionDuration));

            UpdateInventoryEquipState(pistolData);
        }
        else if (newWeapon == WeaponType.Rifle)
        {
            animator.SetBool("Rifle Equip", true);
            EquipWeaponObject(rifle, weaponRifleEquip);

            SetLayerWeight("Equip Layer", 1f);
            yield return StartCoroutine(SmoothLayerWeightTransition("Equip Layer", 0f, transitionDuration));
            yield return StartCoroutine(SmoothLayerWeightTransition("Rifle Layer", 1f, transitionDuration));

            UpdateInventoryEquipState(rifleData);
        }
        else if (newWeapon == WeaponType.AK)
        {
            animator.SetBool("Rifle Equip", true);

            EquipWeaponObject(ak, weaponAKEquip);

            SetLayerWeight("Equip Layer", 1f);
            yield return StartCoroutine(SmoothLayerWeightTransition("Equip Layer", 0f, transitionDuration));
            yield return StartCoroutine(SmoothLayerWeightTransition("Rifle Layer", 1f, transitionDuration));
            UpdateInventoryEquipState(akData);
        }
        else if (newWeapon == WeaponType.Shotgun)
        {
            animator.SetBool("Rifle Equip", true);
            EquipWeaponObject(shotgun, weaponShotgunEquip);

            SetLayerWeight("Equip Layer", 1f);
            yield return StartCoroutine(SmoothLayerWeightTransition("Equip Layer", 0f, transitionDuration));
            yield return StartCoroutine(SmoothLayerWeightTransition("Shotgun Layer", 1f, transitionDuration));

            UpdateInventoryEquipState(shotgunData);
        }
    }

    private void UnequipWeapon(WeaponType weaponType)
    {
        StartCoroutine(UnequipWeaponRoutine(weaponType));
        equippedWeapon = WeaponType.None;

        // 🔥 CLEAR ALL EQUIPPED FLAGS
        foreach (var item in PlayerInventory.Instance.GetInventory().Keys)
        {
            if (item is WeaponItem w)
            {
                w.isEquipped = false;
            }
        }

        PlayerInventory.Instance.OnInventoryChanged?.Invoke();
    }

    private IEnumerator UnequipWeaponRoutine(WeaponType weaponType)
    {
        if (weaponType == WeaponType.Axe)
        {
            animator.SetBool("Equip Axe", false);
            yield return StartCoroutine(SmoothLayerWeightTransition("Combat Axe", 0f, transitionDuration));
            UnequipWeaponObject(axe, weaponUnequipBackpack);
        }
        else if (weaponType == WeaponType.Pistol)
        {
            animator.SetBool("Equip Pistol", false);
            yield return StartCoroutine(SmoothLayerWeightTransition("Combat Pistol", 0f, transitionDuration));
            UnequipWeaponObject(pistol, weaponUnequipHip);
        }
        else if (weaponType == WeaponType.Rifle)
        {
            animator.SetBool("Rifle Equip", false);
            yield return StartCoroutine(SmoothLayerWeightTransition("Rifle Layer", 0f, transitionDuration));
            UnequipWeaponObject(rifle, weaponUnequipRifle);
        }
        if (weaponType == WeaponType.AK)
        {
            animator.SetBool("Rifle Equip", false);
            yield return StartCoroutine(SmoothLayerWeightTransition("Rifle Layer", 0f, transitionDuration));
            UnequipWeaponObject(ak, weaponUnequipAK);
        }
        else if (weaponType == WeaponType.Shotgun)
        {
            animator.SetBool("Rifle Equip", false);
            yield return StartCoroutine(SmoothLayerWeightTransition("Shotgun Layer", 0f, transitionDuration));
            UnequipWeaponObject(shotgun, weaponUnequipShotgun);
        }
        else if (weaponType == WeaponType.Knife)
        {
            animator.SetBool("Equip Knife", false);
            yield return StartCoroutine(SmoothLayerWeightTransition("Combat Knife", 0f, transitionDuration));
            UnequipWeaponObject(knife, weaponUnequipLeft);
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
    private void ResetAllCombatLayers()
    {
        SetLayerWeight("Combat Axe", 0f);
        SetLayerWeight("Combat Knife", 0f);
        SetLayerWeight("Combat Pistol", 0f);
        SetLayerWeight("Rifle Layer", 0f);
        SetLayerWeight("Shotgun Layer", 0f);
        SetLayerWeight("AK Layer", 0f);
    }

    //public void SetCampZone(CampArea area) => nearCampArea = area;
    //public void ClearCampZone() { nearCampArea = null; campActive = false; }
    public void ResetVelocity() => velocity = Vector3.zero;
}