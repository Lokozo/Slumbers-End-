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

    private GameObject blackOverlay;


    [Header("Timing")]
    public float panelFadeDuration = 0.3f;   // was 0.15
    public float sceneFadeDuration = 0.7f;   // was 0.4
    public float endFadeDuration = 1.0f;     // was 0.8

    private List<GameObject> panels = new List<GameObject>();
    private int currentPanelIndex = 0;

    private bool isPlaying = false;
    private bool isTransitioning = false;

    public static bool IsCutscenePlaying { get; private set; }

    private GameObject currentCutscene;

    private Coroutine fadeCoroutine;

    //private CutsceneStarter CutsceneStarterRef;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        //CutsceneStarterRef = FindFirstObjectByType<CutsceneStarter>();

        SafeInitUI();
    }

    private void Start()
    {

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

    //public void PlayCutscene(GameObject cutsceneParent)
    //{
    //    StartCoroutine(PlayWithFadeRoutine(cutsceneParent));
    //}

    public void PlayCutscene(GameObject cutsceneParent) { StartCoroutine(PlayWithFadeRoutine(cutsceneParent)); }

    private IEnumerator PlayRoutine(GameObject cutsceneParent)
    {
        //StopAllCoroutines();

        isPlaying = false;
        isTransitioning = false;
        currentPanelIndex = 0;

        // Wait until fadeImage is found
        while (fadeImage == null)
        {
            SafeInitUI();
            yield return null;
        }

        if (cutsceneParent == null)
        {
            Debug.LogWarning("Cutscene is NULL.");
            yield break;
        }

        IsCutscenePlaying = true;

        currentCutscene = cutsceneParent;
        currentCutscene.SetActive(true);

        blackOverlay = GameObject.Find("BlackOverlay");

        if (blackOverlay == null)
        {
            Debug.LogWarning("BlackOverlay not found!");
        }

        panels.Clear();  //

        foreach (Transform child in currentCutscene.transform)
        {
            GameObject panel = child.gameObject;

            CanvasGroup cg = panel.GetComponent<CanvasGroup>();
            if (cg == null)
                cg = panel.AddComponent<CanvasGroup>();

            cg.alpha = 0f;

            panels.Add(panel);
            panel.SetActive(false);
        }  //

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
        GameObject firstPanel = panels[currentPanelIndex];
        firstPanel.SetActive(true);

        CanvasGroup cg = firstPanel.GetComponent<CanvasGroup>();
        cg.alpha = 0f;

        // 1. Ensure we are internally blacked out
        fadeImage.color = new Color(0, 0, 0, 1f);
        fadeImage.gameObject.SetActive(true);

        // RELEASE BLACK OVERLAY (SAFE MOMENT)
        if (blackOverlay != null)
        {
            blackOverlay.SetActive(false);
            Debug.Log("CutsceneManager turned off BlackOverlay");
        }

        // 3. Fade IN panel
        float time = 0f;
        while (time < panelFadeDuration)
        {
            cg.alpha = Mathf.Lerp(0f, 1f, time / panelFadeDuration);
            time += Time.deltaTime;
            yield return null;
        }
        cg.alpha = 1f;

        // 4. Reveal everything
        yield return StartCoroutine(Fade(0f, sceneFadeDuration));

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

        int nextIndex = currentPanelIndex + 1;

        // END CASE
        if (nextIndex >= panels.Count)
        {
            yield return StartCoroutine(EndCutsceneRoutine());
            yield break;
        }

        GameObject currentPanel = panels[currentPanelIndex];
        GameObject nextPanel = panels[nextIndex];

        CanvasGroup nextGroup = nextPanel.GetComponent<CanvasGroup>();

        // Activate next panel ON TOP
        nextPanel.SetActive(true);
        nextGroup.alpha = 0f;

        float time = 0f;

        // Fade IN the next panel
        while (time < panelFadeDuration)
        {
            float t = time / panelFadeDuration;
            nextGroup.alpha = Mathf.Lerp(0f, 1f, t);

            time += Time.deltaTime;
            yield return null;
        }

        nextGroup.alpha = 1f;

        // NOW remove the old panel (after fade)
        currentPanel.SetActive(false);

        currentPanelIndex = nextIndex;

        if (continuePrompt != null)
            continuePrompt.SetActive(true);

        isPlaying = true;
        isTransitioning = false;
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

     
        SceneLoader loader = FindFirstObjectByType<SceneLoader>();

        if (loader != null)
        {
            loader.SetWaitingForCutscene(false);
            loader.StartCoroutine(loader.ShowChapterTitle());
        }
    }

    private IEnumerator Fade(float targetAlpha, float duration)
    {
        if (fadeImage == null) yield break;

        float startAlpha = fadeImage.color.a;
        fadeImage.color = new Color(0, 0, 0, startAlpha);

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
            Transform fade = uiRoot.transform.Find("FadeImage");

            if (fade != null)
                fadeImage = fade.GetComponent<Image>();
        }

        if (continuePrompt != null)
            continuePrompt.SetActive(false);
    }

    private IEnumerator PlayWithFadeRoutine(GameObject cutsceneParent)
    {
        while (fadeImage == null)
        {
            SafeInitUI();
            yield return null;
        }

        // Just ensure fadeImage is active
        fadeImage.gameObject.SetActive(true);

        fadeImage.color = new Color(0, 0, 0, 0f);
        // Fade TO black ONLY
        yield return StartCoroutine(Fade(1f, sceneFadeDuration));

        // IMPORTANT: wait for fade to fully settle
        yield return new WaitForSeconds(0.2f);

        // Start cutscene AFTER fade is complete
        yield return StartCoroutine(PlayRoutine(cutsceneParent));
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

        // Re-enable the static blackout so the NEXT scene starts dark too
        
    }
}