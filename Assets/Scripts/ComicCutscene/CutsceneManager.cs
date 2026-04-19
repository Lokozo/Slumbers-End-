using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CutsceneManager : MonoBehaviour
{
    public static CutsceneManager Instance;

    [Header("UI")]
    public Image fadeImage;
    public GameObject continuePrompt;

    [Header("Timing")]
    public float panelFadeDuration = 0.15f;
    public float endFadeDuration = 0.8f;

    private List<GameObject> panels = new List<GameObject>();
    private int currentPanelIndex = 0;

    private bool isPlaying = false;
    private bool isTransitioning = false;   // SINGLE LOCK ONLY

    public static bool IsCutscenePlaying { get; private set; }

    private GameObject currentCutscene;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        SafeInitUI();
    }

    private void Update()
    {
        if (!isPlaying || isTransitioning) return;

        if (Input.GetMouseButtonDown(0) ||
            Input.GetKeyDown(KeyCode.E) ||
            Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(NextPanelRoutine());
        }
    }

    #region PUBLIC ENTRY

    public void PlayCutscene(GameObject cutsceneParent)
    {
        StartCoroutine(PlayRoutine(cutsceneParent));
    }

    private IEnumerator PlayRoutine(GameObject cutsceneParent)
    {
        StopAllCoroutines();

        isPlaying = false;
        isTransitioning = false;
        currentPanelIndex = 0;

        while (fadeImage == null)
            yield return null;

        if (cutsceneParent == null)
        {
            Debug.LogWarning("Cutscene is NULL.");
            yield break;
        }

        IsCutscenePlaying = true;

        currentCutscene = cutsceneParent;
        currentCutscene.SetActive(true);

        fadeImage.gameObject.SetActive(true);

        panels.Clear();

        foreach (Transform child in currentCutscene.transform)
        {
            panels.Add(child.gameObject);
            child.gameObject.SetActive(false);
        }

        currentPanelIndex = 0;
        isPlaying = true;
        isTransitioning = false;

        if (continuePrompt != null)
            continuePrompt.SetActive(false);

        StartCoroutine(FadeAndStart());
    }

    #endregion

    private IEnumerator FadeAndStart()
    {
        panels[currentPanelIndex].SetActive(true);

        yield return StartCoroutine(Fade(0, panelFadeDuration));

        if (continuePrompt != null)
            continuePrompt.SetActive(true);
    }

    private IEnumerator NextPanelRoutine()
    {
        if (!isPlaying || isTransitioning)
            yield break;

        isTransitioning = true;
        isPlaying = false;

        if (continuePrompt != null)
            continuePrompt.SetActive(false);

        yield return new WaitForSeconds(0.05f);

        yield return StartCoroutine(Fade(1, panelFadeDuration));

        panels[currentPanelIndex].SetActive(false);
        currentPanelIndex++;

        if (currentPanelIndex >= panels.Count)
        {
            yield return StartCoroutine(EndCutsceneRoutine());
            yield break;
        }

        panels[currentPanelIndex].SetActive(true);

        yield return StartCoroutine(Fade(0, panelFadeDuration));

        if (continuePrompt != null)
            continuePrompt.SetActive(true);

        isPlaying = true;
        isTransitioning = false;   // IMPORTANT RESET
    }

    private IEnumerator EndCutsceneRoutine()
    {
        if (continuePrompt != null)
            continuePrompt.SetActive(false);

        yield return new WaitForSeconds(0.1f);

        yield return StartCoroutine(Fade(1, endFadeDuration));

        if (currentCutscene != null)
            currentCutscene.SetActive(false);

        isPlaying = false;
        isTransitioning = false;

        IsCutscenePlaying = false;

        yield return StartCoroutine(Fade(0, endFadeDuration));
    }

    private IEnumerator Fade(float targetAlpha, float duration)
    {
        if (fadeImage == null) yield break;

        float startAlpha = fadeImage.color.a;
        float time = 0;

        while (time < duration)
        {
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            time += Time.deltaTime;
            yield return null;
        }

        fadeImage.color = new Color(0, 0, 0, targetAlpha);
    }

    private void SafeInitUI()
    {
        
        GameObject uiRoot = GameObject.Find("GameUICanvas");

        if (uiRoot != null)
        {
            fadeImage = uiRoot.transform.Find("FadeImage")?.GetComponent<Image>();
            //continuePrompt = uiRoot.transform.Find("ContinuePrompt")?.gameObject;
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
        SafeInitUI();
    }
}