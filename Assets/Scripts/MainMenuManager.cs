using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Main Menu")]
    public CanvasGroup fadeGroup;
    public string firstSceneName = "Chapter 1";

    [Header("Pause Menu")]
    public GameObject pauseMenu;

    private bool isPaused = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }
    }

    public void StartGame()
    {
        StartCoroutine(StartGameRoutine());
    }

    IEnumerator StartGameRoutine()
    {
        yield return StartCoroutine(Fade(1));

        SceneManager.LoadScene(firstSceneName);
    }

    IEnumerator Fade(float target)
    {
        float duration = 0.5f;
        float time = 0;
        float start = fadeGroup.alpha;

        while (time < duration)
        {
            time += Time.deltaTime;
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
        Time.timeScale = 1f;

        SceneManager.LoadScene("MainMenu");
    }

    public void ExitGame()
    {
        Debug.Log("EXIT GAME");

        Application.Quit();
    }
}