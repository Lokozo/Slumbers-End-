using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;

public class MainMenuManager : MonoBehaviour
{
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

        // AUTO ASSIGN PAUSE PANEL (WORKS EVEN IF INACTIVE)
        if (pauseMenu == null)
        {
            pauseMenu = Resources.FindObjectsOfTypeAll<GameObject>()
                .FirstOrDefault(obj => obj.name == "Pause Panel");
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !isTransitioning)
        {
            TogglePauseMenu();
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
            fadeGroup.alpha = Mathf.Lerp(start, target, time / duration);
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

        Cursor.lockState = isPaused
            ? CursorLockMode.None
            : CursorLockMode.Locked;

        Cursor.visible = isPaused;
    }

    public void ResumeGame()
    {
        isPaused = false;

        if (pauseMenu != null)
            pauseMenu.SetActive(false);

        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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