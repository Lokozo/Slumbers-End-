using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryManager : MonoBehaviour
{
    public GameObject InventoryMenu;
    public InventoryUI inventoryUI;

    private static bool hasOpenedInventoryOnce = false;

    public static InventoryManager Instance;

    private void Awake()
    {

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        //DontDestroyOnLoad(gameObject);
    }
    void Update()
    {
        if (Keyboard.current.iKey.wasPressedThisFrame)
        {
            ToggleInventory();
            inventoryUI.ClearDescription();
        }

    }

    public void ToggleInventory()
    {
        CampArea camp = FindFirstObjectByType<CampArea>();

        if (camp != null && camp.IsInCamp())
            return;

        if (InventoryMenu == null)
        {
            InventoryMenu = GameObject.Find("InventoryMenu");

            if (InventoryMenu == null)
                return;
        }

        // BLOCK IN CAMPSITE
        if (SceneManager.GetSceneByName("Campsite").isLoaded)
            return;

        // USE REAL UI STATE INSTEAD OF BOOL
        bool newState = !InventoryMenu.activeSelf;

        InventoryMenu.SetActive(newState);

        Time.timeScale = newState ? 0f : 1f;

        // CURSOR
        Cursor.lockState = newState
            ? CursorLockMode.None
            : CursorLockMode.Locked;

        Cursor.visible = newState;

        Debug.Log("Inventory toggled: " + newState);

        if (newState && !hasOpenedInventoryOnce)
        {
            hasOpenedInventoryOnce = true;
            TutorialUIManager.Instance?.Hide();
        }
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
