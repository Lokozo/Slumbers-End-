using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryManager : MonoBehaviour
{
    public GameObject InventoryMenu;
    public InventoryUI inventoryUI;

    private bool isInventoryOpen = false;

    public static InventoryManager Instance;

    private void Awake()
    {

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    void Update()
    {
        if (Keyboard.current.iKey.wasPressedThisFrame)
        {
            ToggleInventory();
            inventoryUI.ClearDescription();
        }

    }

    private void ToggleInventory()
    {
        if (InventoryMenu == null)
        {
            Debug.LogWarning("InventoryMenu is NULL! Trying to recover...");
            InventoryMenu = GameObject.Find("InventoryMenu");

            if (InventoryMenu == null)
            {
                Debug.LogError("InventoryMenu is STILL null. Cannot toggle inventory.");
                return; // Prevent the crash
            }
        }
        isInventoryOpen = !isInventoryOpen;
        InventoryMenu.SetActive(isInventoryOpen);

        // Pause or resume the game
        Time.timeScale = isInventoryOpen ? 0f : 1f;

        // Cursor state
        //Cursor.lockState = isInventoryOpen ? CursorLockMode.None : CursorLockMode.Locked;
        //Cursor.visible = isInventoryOpen;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Find canvas first
        var canvas = GameObject.Find("GameUICanvas");
        if (canvas == null)
        {
            Debug.LogError("[InventoryManager] GameUICanvas NOT found!");
            return;
        }

        // Find InventoryMenu *inside* canvas (even if inactive)
        InventoryMenu = canvas.transform.Find("InventoryMenu")?.gameObject;

        if (InventoryMenu == null)
            Debug.LogError("[InventoryManager] InventoryMenu NOT found under GameUICanvas!");
        else
            Debug.Log("[InventoryManager] InventoryMenu successfully assigned.");

        // Find InventoryUI component anywhere in scene
        inventoryUI = GameObject.FindFirstObjectByType<InventoryUI>(FindObjectsInactive.Include); // searches inactive too

        if (inventoryUI == null)
            Debug.LogError("[InventoryManager] InventoryUI not found!");
    }

}
