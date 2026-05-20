using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject inventoryPanel;

    [Header("Main Menu")]
    [SerializeField] private CanvasGroup fadeGroup;
    public string firstSceneName = "Chapter 1";

    [Header("Pause Menu")]
    [SerializeField] private GameObject pauseMenu;

    private bool isPaused = false;
    private bool isTransitioning = false;

    private void Start()
    {
        // AUTO ASSIGN FADE PANEL
        if (fadeGroup == null)
        {
            GameObject fadeObj = GameObject.Find("FadePanel");

            if (fadeObj != null)
                fadeGroup = fadeObj.GetComponent<CanvasGroup>();
        }

        // AUTO ASSIGN PAUSE PANEL
        if (pauseMenu == null)
        {
            pauseMenu = Resources.FindObjectsOfTypeAll<GameObject>()
                .FirstOrDefault(obj => obj.name == "Pause Panel");
        }

        // AUTO ASSIGN INVENTORY PANEL
        if (inventoryPanel == null)
        {
            inventoryPanel = Resources.FindObjectsOfTypeAll<GameObject>()
                .FirstOrDefault(obj => obj.name == "InventoryMenu");
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !isTransitioning)
        {
            // CHECK LOOT UI
            ResourceInteraction interaction =
                FindFirstObjectByType<ResourceInteraction>();

            if (interaction != null && interaction.IsPanelOpen())
            {
                Debug.Log("Loot UI open - ignore pause");
                return;
            }

            // CHECK INVENTORY UI
            if (inventoryPanel != null &&
                inventoryPanel.activeSelf)
            {
                Debug.Log("Inventory UI open - ignore pause");
                return;
            }

            if (Time.timeScale == 1f)
            {
                TogglePauseMenu();
            }
        }
    }

    public void StartGame()
    {
        if (!isTransitioning)
            StartCoroutine(StartGameRoutine());
    }

    IEnumerator StartGameRoutine()
    {
        isTransitioning = true;

        yield return StartCoroutine(Fade(1));

        SceneManager.LoadScene(firstSceneName);
    }

    IEnumerator Fade(float target)
    {
        if (fadeGroup == null)
            yield break;

        float duration = 0.5f;
        float time = 0;
        float start = fadeGroup.alpha;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;

            fadeGroup.alpha =
                Mathf.Lerp(start, target, time / duration);

            yield return null;
        }

        fadeGroup.alpha = target;
    }

    public void TogglePauseMenu()
    {
        isPaused = !isPaused;

        if (pauseMenu != null)
            pauseMenu.SetActive(isPaused);

        Time.timeScale = isPaused ? 0f : 1f;

        if (isPaused)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            Debug.Log("Pause OPEN - Cursor visible");
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            Debug.Log("Pause CLOSED - Cursor hidden");
        }
    }

    public void ResumeGame()
    {
        isPaused = false;

        if (pauseMenu != null)
            pauseMenu.SetActive(false);

        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("ResumeGame - Cursor hidden");
    }

    public void BackToMainMenu()
    {
        if (!isTransitioning)
            StartCoroutine(BackToMainMenuRoutine());
    }

    IEnumerator BackToMainMenuRoutine()
    {
        isTransitioning = true;

        Time.timeScale = 1f;

        yield return StartCoroutine(Fade(1));

        SceneManager.LoadScene("MainMenu");
    }

    public void ExitGame()
    {
        if (!isTransitioning)
            StartCoroutine(ExitGameRoutine());
    }

    IEnumerator ExitGameRoutine()
    {
        isTransitioning = true;

        Time.timeScale = 1f;

        yield return StartCoroutine(Fade(1));

        Debug.Log("EXIT GAME");

        Application.Quit();
    }
}