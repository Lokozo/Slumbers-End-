using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class TutorialUIManager : MonoBehaviour
{
    public static TutorialUIManager Instance;

    [Header("UI")]
    [SerializeField] private string instructionTextObjectName = "InstructionText";
    public TextMeshProUGUI instructionText;

    [Header("Fade Settings")]
    public float fadeDuration = 0.75f;

    private Coroutine currentRoutine;
    private bool isVisible;

    // Track which tutorial steps have been shown
    private HashSet<string> completedSteps = new HashSet<string>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
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
        instructionText = GameObject.Find(instructionTextObjectName)
            ?.GetComponent<TextMeshProUGUI>();

        if (instructionText != null)
            SetAlpha(isVisible ? 1f : 0f);
    }

    // =========================
    // PUBLIC API
    // =========================

    public void ShowStep(string stepID, string text, bool force = true)
    {
        if (completedSteps.Contains(stepID)) return; // Already done

        if (instructionText == null) return;

        if (isVisible && !force) return;

        completedSteps.Add(stepID);

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(FadeInRoutine(text));
    }

    public void Hide()
    {
        if (instructionText == null || !isVisible) return;

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(FadeOutRoutine());
    }

    // =========================
    // ROUTINES
    // =========================

    private IEnumerator FadeInRoutine(string text)
    {
        yield return FadeText(1f, 0f);

        instructionText.text = text;
        isVisible = true;

        yield return FadeText(0f, 1f);
    }

    private IEnumerator FadeOutRoutine()
    {
        yield return FadeText(1f, 0f);

        instructionText.text = "";
        isVisible = false;
    }

    private IEnumerator FadeText(float from, float to)
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
            SetAlpha(alpha);
            yield return null;
        }

        SetAlpha(to);
    }

    private void SetAlpha(float alpha)
    {
        if (instructionText == null) return;

        Color c = instructionText.color;
        instructionText.color = new Color(c.r, c.g, c.b, alpha);
    }
}
