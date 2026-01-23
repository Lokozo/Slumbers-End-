using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class TutorialUIManager : MonoBehaviour
{
    public static TutorialUIManager Instance;

    public TextMeshProUGUI instructionText;
    public float fadeDuration = 1f;
    public float delayBeforeNextStep = 2f;

    private int step = 0;
    private bool isFading = false;
    private bool canCheckInput = true;

    private void Awake()
    {
        // Singleton pattern for persistence
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        instructionText.text = "Press A or D to move";
        SetAlpha(1f);
    }

    void Update()
    {
        if (!canCheckInput || isFading) return;

        switch (step)
        {
            case 0:
                if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
                    StartCoroutine(FadeToNext("Hold Left Shift while moving to run"));
                break;

                // You can uncomment and expand steps as needed
        }
    }

    IEnumerator FadeToNext(string nextText)
    {
        isFading = true;
        canCheckInput = false;

        yield return StartCoroutine(FadeText(1f, 0f));   // Fade out
        instructionText.text = nextText;
        yield return StartCoroutine(FadeText(0f, 1f));   // Fade in

        yield return new WaitForSeconds(delayBeforeNextStep); // Let player read

        isFading = false;
        canCheckInput = true;
        step++;
    }

    IEnumerator FadeOutAndDisable()
    {
        isFading = true;
        canCheckInput = false;

        yield return StartCoroutine(FadeText(1f, 0f));
        instructionText.text = "";
        //enabled = false;
    }

    IEnumerator FadeText(float from, float to)
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
            SetAlpha(alpha);
            yield return null;
        }
        SetAlpha(to);
    }

    void SetAlpha(float alpha)
    {
        Color c = instructionText.color;
        instructionText.color = new Color(c.r, c.g, c.b, alpha);
    }

    public void ShowInteractionInstruction(string instruction)
    {
        if (!isFading)
            StartCoroutine(FadeToNext(instruction));
    }

    public void HideInstruction()
    {
        if (!isFading)
            StartCoroutine(FadeOutAndDisable());
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
        instructionText = GameObject.Find("InstructionText")?.GetComponent<TextMeshProUGUI>();
    }

}
