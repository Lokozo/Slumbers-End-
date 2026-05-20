using System.Collections;
using TMPro;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;


public class SceneLoader : MonoBehaviour
{
    public CanvasGroup fadeGroup;
    public float fadeDuration = 0.5f;
    public CanvasGroup chapterTitleGroup;
    public float titleFadeDuration = 0.5f;
    public float titleDisplayTime = 2f;

    public TextMeshProUGUI chapterTitleText;

    [TextArea]
    public string chapterTitle;

    private bool waitingForCutscene = false;
    private bool isLoading = false;

    private bool hasShownTitle = false;

    [Header("Scene Transition")]
    public SceneLoader sceneLoader;
    public bool loadNextSceneAfterDialogue = false;

    void Start()
    {
        fadeGroup.gameObject.SetActive(true);
        fadeGroup.alpha = 1f;
        fadeGroup.blocksRaycasts = true;

        chapterTitleText.text = string.IsNullOrEmpty(chapterTitle)
        ? "CHAPTER"
        : chapterTitle;

        //  Ensure title starts hidden
        if (chapterTitleGroup != null)
            chapterTitleGroup.alpha = 0f;

        //  Start fade system
        if (fadeGroup != null)
        {
            fadeGroup.alpha = 1f;
            StartCoroutine(SceneStartRoutine());
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

    public void LoadNextSceneExternally()
    {
        LoadNextScene();
    }

    IEnumerator LoadSceneWithFade(int sceneIndex)
    {
        isLoading = true;

        yield return StartCoroutine(Fade(1));

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        yield return null;

        yield return StartCoroutine(Fade(0));

        isLoading = false;
    }

    IEnumerator SceneStartRoutine()
    {
        yield return StartCoroutine(Fade(0));

        yield return null;

        if (!waitingForCutscene)
        {
            StartCoroutine(ShowChapterTitle());
        }
    }

    IEnumerator Fade(float target)
    {
        float time = 0f;
        float start = fadeGroup.alpha;

        fadeGroup.blocksRaycasts = true;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;

            fadeGroup.alpha =
                Mathf.Lerp(start, target, time / fadeDuration);

            yield return null;
        }

        fadeGroup.alpha = target;

        // 🔥 ONLY disable raycasts
        if (target == 0)
        {
            fadeGroup.blocksRaycasts = false;
        }
    }

    public IEnumerator ShowChapterTitle()
    {
        if (chapterTitleGroup == null || hasShownTitle)
            yield break;

        hasShownTitle = true;

        float t = 0;

        // Fade IN
        while (t < titleFadeDuration)
        {
            t += Time.deltaTime;
            chapterTitleGroup.alpha = Mathf.Lerp(0, 1, t / titleFadeDuration);
            yield return null;
        }

        chapterTitleGroup.alpha = 1;

        yield return new WaitForSeconds(titleDisplayTime);

        // Fade OUT
        t = 0;
        while (t < titleFadeDuration)
        {
            t += Time.deltaTime;
            chapterTitleGroup.alpha = Mathf.Lerp(1, 0, t / titleFadeDuration);
            yield return null;
        }

        chapterTitleGroup.alpha = 0;
    }

    public void SetWaitingForCutscene(bool value)
    {
        waitingForCutscene = value;
    }

    public IEnumerator FadeToBlack()
    {
        yield return StartCoroutine(Fade(1));
    }

    public IEnumerator FadeFromBlack()
    {
        yield return StartCoroutine(Fade(0));
    }
}