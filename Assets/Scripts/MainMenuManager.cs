using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public CanvasGroup fadeGroup;
    public string firstSceneName = "Chapter 1";

    public void StartGame()
    {
        StartCoroutine(StartGameRoutine());
    }

    IEnumerator StartGameRoutine()
    {
        yield return StartCoroutine(Fade(1)); // fade to black

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
}