using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
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
    private int maxEquippedWeapons = 3;

    [Header("State Flags")]
    public bool movementLocked = false;
    public bool isClimbing = false;
    private bool isMovementPressed;
    private bool isRunPressed = false;
    private bool isGrounded;

    [Header("Weapon State")]
    private bool isWeaponInHand = false;
    private Coroutine currentEquipRoutine;
    private Coroutine currentUnequipRoutine;

    [Header("Audio")]

    public AudioSource audioSource;

    public AudioClip[] footstepClips;
    public AudioClip[] runFootstepClips;
    public AudioClip[] attackGrunts;

    private WeaponItem lastClickedWeapon;

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

        // 🔥 FORCE INITIALIZE WEAPONS AFTER ALL SYSTEMS READY
        StartCoroutine(ForceInitializeStartingWeapons());
    }

    private IEnumerator ForceInitializeStartingWeapons()
    {
        yield return new WaitForSeconds(0.1f); // Wait longer

        Debug.Log("🔥 === FORCE INITIALIZING STARTING WEAPONS ===");

        // AXE
        Debug.Log($"Axe BEFORE: active={axe?.activeSelf}, equipped={axeData?.isEquipped}");
        if (axe != null && axeData != null)
        {
            axe.SetActive(true);
            axeData.isEquipped = true;
        }
        Debug.Log($"Axe AFTER:  active={axe?.activeSelf}, equipped={axeData?.isEquipped}");

        // KNIFE
        Debug.Log($"Knife BEFORE: active={knife?.activeSelf}, equipped={knifeData?.isEquipped}");
        if (knife != null && knifeData != null)
        {
            knife.SetActive(true);
            knifeData.isEquipped = true;
            playerAttack.SetWeapon(knifeData);
        }
        Debug.Log($"Knife AFTER:  active={knife?.activeSelf}, equipped={knifeData?.isEquipped}");

        // FORCE UI UPDATE
        yield return new WaitForEndOfFrame();
        PlayerInventory.Instance.OnInventoryChanged?.Invoke();

        Debug.Log("✅ Force initialization COMPLETE!");
    }
    public void PlayFootstep()
    {
        Debug.Log("FOOTSTEP");
        if (footstepClips.Length == 0) return;

        AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];

        audioSource.PlayOneShot(clip);
    }

    public void PlayRunFootstep()
    {
        Debug.Log("running");

        if (runFootstepClips.Length == 0) return;

        AudioClip clip = runFootstepClips[Random.Range(0, runFootstepClips.Length)];

        audioSource.PlayOneShot(clip);
    }

    public void PlayAttackGrunt()
    {
        if (attackGrunts.Length == 0) return;

        AudioClip clip = attackGrunts[Random.Range(0, attackGrunts.Length)];

        audioSource.PlayOneShot(clip);
    }
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (enableTutorial && TutorialUIManager.Instance != null)
        {
            TutorialUIManager.Instance.ShowStep("moveTutorial", "Press A or D to move");
        }

        InitializeStartingWeapons();
    }
    private void InitializeStartingWeapons()
    {
        // Set both axe and knife as equipped at start
        if (axe != null && axeData != null)
        {
            axe.SetActive(true);
            axeData.isEquipped = true;
            Debug.Log("STARTING WEAPON EQUIPPED: Axe");
        }

        if (knife != null && knifeData != null)
        {
            knife.SetActive(true);
            knifeData.isEquipped = true;
            playerAttack.SetWeapon(knifeData); // Keep knife as active weapon for attacks
            Debug.Log("STARTING WEAPON EQUIPPED: Knife");
        }

        // Don't set equippedWeapon here - let UI handle individual selection
        PlayerInventory.Instance.OnInventoryChanged?.Invoke();
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

        if (Input.GetKeyDown(KeyCode.R))
        {
            ToggleWeaponModels();
        }

        var pushPull = GetComponent<PlayerPushPull>();
        bool isPushing = pushPull != null && pushPull.IsPushing;
        HandlePushLayerWeight(isPushing);

        if (isPushing)
        {
            pushPull.UpdatePushMovement(currentMovementInput);
        }

        if (enableTutorial && !hasCompletedMovementTutorial)
        {
            CheckRunTutorial();
        }

        MovePlayer(isPushing);
        ApplyGravity();
        HandleGroundedCheck();
    }

    private int GetEquippedWeaponCount()
    {
        int count = 0;

        if (axe.activeSelf) count++;
        if (knife.activeSelf) count++;
        if (pistol.activeSelf) count++;
        if (rifle.activeSelf) count++;
        if (shotgun.activeSelf) count++;
        if (ak.activeSelf) count++;

        return count;
    }

    private void ToggleWeaponModels()
    {
       
        if (lastClickedWeapon != null)
        {          
            ToggleSpecificWeaponByItem(lastClickedWeapon);
            return;
        }

       
        if (equippedWeapon != WeaponType.None)
        {          
            ToggleSpecificWeapon(equippedWeapon);
            return;
        }

    }

    public void SetLastClickedWeapon(WeaponItem weapon)
    {
        lastClickedWeapon = weapon;
    }

    public void ToggleSpecificWeaponByItem(WeaponItem weaponItem)
    {
        GameObject weaponObj = null;
        string weaponName = weaponItem.itemName;

        if (weaponName == axeData.itemName) weaponObj = axe;
        else if (weaponName == knifeData.itemName) weaponObj = knife;
        else if (weaponName == pistolData.itemName) weaponObj = pistol;
        else if (weaponName == rifleData.itemName) weaponObj = rifle;
        else if (weaponName == shotgunData.itemName) weaponObj = shotgun;
        else if (weaponName == akData.itemName) weaponObj = ak;

        if (weaponObj != null)
        {
            bool newState = !weaponItem.isEquipped;

            // BLOCK if trying to equip more than max
            if (newState && GetEquippedWeaponCount() >= maxEquippedWeapons)
            {
                Debug.Log("You can only equip 3 weapons!");
                return;
            }

            weaponObj.SetActive(newState);
            weaponItem.isEquipped = newState;

            if (newState)
            {
                playerAttack.SetWeapon(weaponItem);

                equippedWeapon = GetWeaponTypeFromData(weaponItem);

                Debug.Log("🔥 Combat weapon changed to: " + weaponItem.itemName);
            }

            Debug.Log($"✅ {weaponItem.itemName} {(newState ? "EQUIPPED" : "UNEQUIPPED")}");

            PlayerInventory.Instance.OnInventoryChanged?.Invoke();
        }
    }

    private void ToggleSpecificWeapon(WeaponType weaponType)
    {
        GameObject weaponObj = null;
        WeaponItem weaponData = null;

        switch (weaponType)
        {
            case WeaponType.Axe: weaponObj = axe; weaponData = axeData; break;
            case WeaponType.Knife: weaponObj = knife; weaponData = knifeData; break;
            case WeaponType.Pistol: weaponObj = pistol; weaponData = pistolData; break;
            case WeaponType.Rifle: weaponObj = rifle; weaponData = rifleData; break;
            case WeaponType.Shotgun: weaponObj = shotgun; weaponData = shotgunData; break;
            case WeaponType.AK: weaponObj = ak; weaponData = akData; break;
        }

        if (weaponObj != null)
        {
            bool newState = !weaponObj.activeSelf;
            weaponObj.SetActive(newState);
            weaponData.isEquipped = newState;
            equippedWeapon = newState ? weaponType : WeaponType.None;

            Debug.Log($"Toggled {weaponData.itemName} to: {newState}");
            PlayerInventory.Instance.OnInventoryChanged?.Invoke();
        }
    }

    private WeaponType GetWeaponTypeFromData(WeaponItem weaponItem)
    {
        if (weaponItem == axeData) return WeaponType.Axe;
        if (weaponItem == knifeData) return WeaponType.Knife;
        if (weaponItem == pistolData) return WeaponType.Pistol;
        if (weaponItem == rifleData) return WeaponType.Rifle;
        if (weaponItem == shotgunData) return WeaponType.Shotgun;
        if (weaponItem == akData) return WeaponType.AK;
        return WeaponType.None;
    }

    // 🔥 OLD HideWeaponByItem - KEPT for backward compatibility (but not used for clicks)
    public void HideWeaponByItem(WeaponItem weaponItem)
    {
        Debug.Log("HideWeaponByItem called: " + weaponItem.itemName);

        if (weaponItem.itemName == axeData.itemName) axe.SetActive(false);
        else if (weaponItem.itemName == knifeData.itemName) knife.SetActive(false);
        else if (weaponItem.itemName == pistolData.itemName) pistol.SetActive(false);
        else if (weaponItem.itemName == rifleData.itemName) rifle.SetActive(false);
        else if (weaponItem.itemName == shotgunData.itemName) shotgun.SetActive(false);
        else if (weaponItem.itemName == akData.itemName) ak.SetActive(false);

        weaponItem.isEquipped = false;
        equippedWeapon = WeaponType.None;
        PlayerInventory.Instance.OnInventoryChanged?.Invoke();
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

        playerInputs.Player.EquipWeapon.performed += ctx => TriggerEquippedWeaponAnimation();

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
            if (PlayerStats.Get() != null)
                PlayerStats.Get().ModifyEnergy(-energyCostPerSecond * Time.deltaTime);

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
        if (!controller.enabled)
            return;

        // 🚫 NO GRAVITY WHILE CLIMBING
        if (isClimbing)
        {
            velocity.y = 0f;
            return;
        }

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

    private void TriggerEquippedWeaponAnimation()
    {
        if (equippedWeapon == WeaponType.None)
        {
            Debug.Log("❌ No weapon equipped!");
            return;
        }

        Debug.Log($"🎯 Toggle {equippedWeapon} (InHand: {isWeaponInHand})");

        // 🔥 STOP ALL PREVIOUS COROUTINES FIRST
        if (currentEquipRoutine != null)
        {
            StopCoroutine(currentEquipRoutine);
            currentEquipRoutine = null;
        }
        if (currentUnequipRoutine != null)
        {
            StopCoroutine(currentUnequipRoutine);
            currentUnequipRoutine = null;
        }

        // 🔥 TOGGLE BASED ON STATE
        if (isWeaponInHand)
        {
            currentUnequipRoutine = StartCoroutine(UnequipWeaponRoutine(equippedWeapon));
        }
        else
        {
            currentEquipRoutine = StartCoroutine(EquipWeaponRoutine(equippedWeapon));
        }
    }

    private IEnumerator EquipWeaponRoutine(WeaponType weaponType)
    {
        Debug.Log($"🔄 Equip Routine: {weaponType}");

        // Reset layers first
        ResetAllCombatLayers();

        // Block input during animation
        bool wasSwitching = isWeaponInHand;
        isWeaponInHand = true;

        switch (weaponType)
        {
            case WeaponType.Pistol:
                animator.SetBool("Equip Pistol", true);
                EquipWeaponObject(pistol, weaponPistolEquip);
                yield return StartCoroutine(SmoothLayerWeightTransition("Equip Layer", 1f, 0.3f));
                yield return new WaitForSeconds(0.1f); // Small pause
                yield return StartCoroutine(SmoothLayerWeightTransition("Equip Layer", 0f, 0.2f));
                yield return StartCoroutine(SmoothLayerWeightTransition("Combat Pistol", 1f, 0.3f));
                break;

            case WeaponType.Shotgun:
                animator.SetBool("Rifle Equip", true);
                EquipWeaponObject(shotgun, weaponShotgunEquip);
                yield return StartCoroutine(SmoothLayerWeightTransition("Equip Layer", 1f, 0.3f));
                yield return new WaitForSeconds(0.1f);
                yield return StartCoroutine(SmoothLayerWeightTransition("Equip Layer", 0f, 0.2f));
                yield return StartCoroutine(SmoothLayerWeightTransition("Shotgun Layer", 1f, 0.3f));
                break;

            case WeaponType.Axe:
                animator.SetBool("Equip Axe", true);
                EquipWeaponObject(axe, weaponAxeEquip);
                yield return StartCoroutine(SmoothLayerWeightTransition("Equip Layer", 1f, 0.3f));
                yield return new WaitForSeconds(0.1f);
                yield return StartCoroutine(SmoothLayerWeightTransition("Equip Layer", 0f, 0.2f));
                yield return StartCoroutine(SmoothLayerWeightTransition("Combat Axe", 1f, 0.3f));
                break;

            case WeaponType.Knife:
                animator.SetBool("Equip Knife", true);
                EquipWeaponObject(knife, weaponKnifeEquip);
                yield return StartCoroutine(SmoothLayerWeightTransition("Equip Layer", 1f, 0.3f));
                yield return new WaitForSeconds(0.1f);
                yield return StartCoroutine(SmoothLayerWeightTransition("Equip Layer", 0f, 0.2f));
                yield return StartCoroutine(SmoothLayerWeightTransition("Combat Knife", 1f, 0.3f));
                break;

            case WeaponType.Rifle:
                animator.SetBool("Rifle Equip", true);
                EquipWeaponObject(rifle, weaponRifleEquip);
                yield return StartCoroutine(SmoothLayerWeightTransition("Equip Layer", 1f, 0.3f));
                yield return new WaitForSeconds(0.1f);
                yield return StartCoroutine(SmoothLayerWeightTransition("Equip Layer", 0f, 0.2f));
                yield return StartCoroutine(SmoothLayerWeightTransition("Rifle Layer", 1f, 0.3f));
                break;

            case WeaponType.AK:
                animator.SetBool("Rifle Equip", true);
                EquipWeaponObject(ak, weaponAKEquip);
                yield return StartCoroutine(SmoothLayerWeightTransition("Equip Layer", 1f, 0.3f));
                yield return new WaitForSeconds(0.1f);
                yield return StartCoroutine(SmoothLayerWeightTransition("Equip Layer", 0f, 0.2f));
                yield return StartCoroutine(SmoothLayerWeightTransition("Rifle Layer", 1f, 0.3f));
                break;
        }

        // 🔥 FINALIZE STATE
        currentEquipRoutine = null;
        Debug.Log($"✅ Equip COMPLETE: {weaponType}");
    }

    private void ToggleWeapon(WeaponType weaponType)
    {
        WeaponItem targetWeapon = null;

        Debug.Log("Currently Equipped Weapon: " + equippedWeapon);

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
        //if (targetWeapon != null && !PlayerInventory.Instance.HasWeapon(targetWeapon))
        //{
        //    Debug.Log("You don't have this weapon yet!");
        //    return;
        //}

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

            knifeData.isEquipped = true;
        }
        else if(newWeapon == WeaponType.Axe)
        {
            animator.SetBool("Equip Axe", true);
            EquipWeaponObject(axe, weaponAxeEquip);

            SetLayerWeight("Equip Layer", 1f);
            yield return StartCoroutine(SmoothLayerWeightTransition("Equip Layer", 0f, transitionDuration));
            yield return StartCoroutine(SmoothLayerWeightTransition("Combat Axe", 1f, transitionDuration));

            axeData.isEquipped = true;
        }
        else if (newWeapon == WeaponType.Pistol)
        {
            animator.SetBool("Combat Pistol", true);
            EquipWeaponObject(pistol, weaponPistolEquip);

            SetLayerWeight("Equip Layer", 1f);
            yield return StartCoroutine(SmoothLayerWeightTransition("Equip Layer", 0f, transitionDuration));
            yield return StartCoroutine(SmoothLayerWeightTransition("Combat Pistol", 1f, transitionDuration));

            pistolData.isEquipped = true;
        }
        else if (newWeapon == WeaponType.Rifle)
        {
            animator.SetBool("Rifle Equip", true);
            EquipWeaponObject(rifle, weaponRifleEquip);

            SetLayerWeight("Equip Layer", 1f);
            yield return StartCoroutine(SmoothLayerWeightTransition("Equip Layer", 0f, transitionDuration));
            yield return StartCoroutine(SmoothLayerWeightTransition("Rifle Layer", 1f, transitionDuration));

            rifleData.isEquipped = true;
        }
        else if (newWeapon == WeaponType.AK)
        {
            animator.SetBool("Rifle Equip", true);

            EquipWeaponObject(ak, weaponAKEquip);

            SetLayerWeight("Equip Layer", 1f);
            yield return StartCoroutine(SmoothLayerWeightTransition("Equip Layer", 0f, transitionDuration));
            yield return StartCoroutine(SmoothLayerWeightTransition("Rifle Layer", 1f, transitionDuration));

            akData.isEquipped = true;
        }
        else if (newWeapon == WeaponType.Shotgun)
        {
            animator.SetBool("Rifle Equip", true);
            EquipWeaponObject(shotgun, weaponShotgunEquip);

            SetLayerWeight("Equip Layer", 1f);
            yield return StartCoroutine(SmoothLayerWeightTransition("Equip Layer", 0f, transitionDuration));
            yield return StartCoroutine(SmoothLayerWeightTransition("Shotgun Layer", 1f, transitionDuration));

            shotgunData.isEquipped = true;
        }
    }

    private void UnequipWeapon(WeaponType weaponType)
    {
        StartCoroutine(UnequipWeaponRoutine(weaponType));
        equippedWeapon = WeaponType.None;

        Debug.Log("Unequipped Weapon: " + weaponType);

        // 🔥 FIX: Only clear the SPECIFIC weapon, not all
        WeaponItem weaponToUnequip = GetWeaponDataFromType(weaponType);
        if (weaponToUnequip != null)
        {
            weaponToUnequip.isEquipped = false;
        }

        PlayerInventory.Instance.OnInventoryChanged?.Invoke();
    }
    private WeaponItem GetWeaponDataFromType(WeaponType weaponType)
    {
        switch (weaponType)
        {
            case WeaponType.Axe: return axeData;
            case WeaponType.Knife: return knifeData;
            case WeaponType.Pistol: return pistolData;
            case WeaponType.Rifle: return rifleData;
            case WeaponType.Shotgun: return shotgunData;
            case WeaponType.AK: return akData;
            default: return null;
        }
    }
    private IEnumerator UnequipWeaponRoutine(WeaponType weaponType)
    {
        Debug.Log($"🔄 Unequip Routine: {weaponType}");

        isWeaponInHand = false;

        switch (weaponType)
        {
            case WeaponType.Axe:
                animator.SetBool("Equip Axe", false);
                yield return StartCoroutine(SmoothLayerWeightTransition("Combat Axe", 0f, 0.3f));
                UnequipWeaponObject(axe, weaponUnequipBackpack);
                break;

            case WeaponType.Knife:
                animator.SetBool("Equip Knife", false);
                yield return StartCoroutine(SmoothLayerWeightTransition("Combat Knife", 0f, 0.3f));
                UnequipWeaponObject(knife, weaponUnequipLeft);
                break;

            case WeaponType.Pistol:
                animator.SetBool("Equip Pistol", false);
                yield return StartCoroutine(SmoothLayerWeightTransition("Combat Pistol", 0f, 0.3f));
                UnequipWeaponObject(pistol, weaponUnequipHip);
                break;

            case WeaponType.Rifle:
                animator.SetBool("Rifle Equip", false);
                yield return StartCoroutine(SmoothLayerWeightTransition("Rifle Layer", 0f, 0.3f));
                UnequipWeaponObject(rifle, weaponUnequipRifle);
                break;

            case WeaponType.AK:
                animator.SetBool("Rifle Equip", false);
                yield return StartCoroutine(SmoothLayerWeightTransition("Rifle Layer", 0f, 0.3f));
                UnequipWeaponObject(ak, weaponUnequipAK);
                break;

            case WeaponType.Shotgun:
                animator.SetBool("Rifle Equip", false);
                yield return StartCoroutine(SmoothLayerWeightTransition("Shotgun Layer", 0f, 0.3f));
                UnequipWeaponObject(shotgun, weaponUnequipShotgun);
                break;
        }

        // Small delay then reset equip layer
        yield return new WaitForSeconds(0.2f);
        yield return StartCoroutine(SmoothLayerWeightTransition("Equip Layer", 0f, 0.3f));

        currentUnequipRoutine = null;
        Debug.Log($"✅ Unequip COMPLETE: {weaponType}");
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
    public bool IsWeaponEquipped(WeaponItem weapon)
    {
        if (weapon == null) return false;

        return weapon.isEquipped;
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