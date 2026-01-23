using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class ObjectiveUI : MonoBehaviour
{
    public TextMeshProUGUI objectiveText;      // Assign in inspector
    public TextMeshProUGUI descriptionText;    // Assign in inspector
    //public Image checkmarkIcon;               // Optional, commented out

    private Color normalColor = Color.black;
    private Color completedColor = Color.green;

    private Objective objectiveData;
    private bool isDescriptionVisible = false;
    private Button button;

    private void Awake()
    {

        button = GetComponent<Button>();
        if (button != null)
            button.onClick.AddListener(ToggleDescription);

        if (descriptionText != null)
            descriptionText.gameObject.SetActive(false);
    }

    public void Setup(Objective obj)
    {
        objectiveData = obj;
        objectiveText.text = obj.title;
        descriptionText.text = obj.description;

        // Register this UI with the ObjectivesManager
        ObjectivesManager.Instance.RegisterObjectiveUI(obj.id, this);

        UpdateUIState();
    }

    public void UpdateUIState()
    {
        if (objectiveData.isCompleted)
        {
            objectiveText.color = completedColor;
            // Don't call PlayCompletionAnimation here anymore
            // just update color and hide description

            if (descriptionText != null)
                descriptionText.gameObject.SetActive(false);
            isDescriptionVisible = false;
        }
        else
        {
            objectiveText.color = normalColor;
            // Optionally disable checkmark icon here if used
        }
    }


    public Coroutine PlayCompletionAnimation()
    {
        objectiveText.color = Color.green;
        StopAllCoroutines();
        return StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        float fadeDuration = 2f;
        float elapsed = 0f;

        // Get or add CanvasGroup for fading entire UI
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 1f; // start fully visible

        while (elapsed < fadeDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = 0f;

        // Optionally disable or destroy after fade
        // gameObject.SetActive(false);
        // Destroy(gameObject);
    }

    private void ToggleDescription()
    {

        if (objectiveData == null)
        {
            Debug.LogWarning("Objective data not assigned yet!");
            return;
        }
        if (objectiveData.isCompleted) return;  // Disable toggling once completed

        isDescriptionVisible = !isDescriptionVisible;

        if (descriptionText != null)
            descriptionText.gameObject.SetActive(isDescriptionVisible);
    }
}
