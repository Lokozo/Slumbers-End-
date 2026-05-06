using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public CanvasGroup fadeGroup;
    public float fadeDuration = 0.5f;

    private bool isLoading = false;

    void Awake()
    {
        if (fadeGroup == null)
        {
            fadeGroup = FindFirstObjectByType<CanvasGroup>();
        }
    }

    void Start()
    {
        // If scene starts black, fade it out
        if (fadeGroup != null)
        {
            fadeGroup.alpha = 1f; // start fully black
            StartCoroutine(Fade(0)); // fade to visible
        }

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            LoadNextScene();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            LoadNextScene();
        }
    }

    void LoadNextScene()
    {
        if (isLoading) return;

        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int nextIndex = currentIndex + 1;

        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            StartCoroutine(LoadSceneWithFade(nextIndex));
        }
        else
        {
            Debug.Log("No more scenes to load.");
        }
    }

    IEnumerator LoadSceneWithFade(int sceneIndex)
    {
        isLoading = true;

        yield return StartCoroutine(Fade(1)); // fade to black

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // WAIT ONE FRAME to ensure scene is fully ready
        yield return null;

        yield return StartCoroutine(Fade(0)); // <-- fade back in

        isLoading = false;
    }

    IEnumerator Fade(float target)
    {
        float time = 0f;
        float start = fadeGroup.alpha;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            fadeGroup.alpha = Mathf.Lerp(start, target, time / fadeDuration);
            yield return null;
        }

        fadeGroup.alpha = target;
    }
}