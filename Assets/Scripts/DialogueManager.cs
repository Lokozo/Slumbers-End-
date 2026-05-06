using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public Image portraitImage;

    private Coroutine dialogueRoutine;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        dialoguePanel.SetActive(false);
    }

    public void StartDialogue(
        Sprite portrait,
        List<string> lines)
    {
        if (dialogueRoutine != null)
            StopCoroutine(dialogueRoutine);

        dialogueRoutine = StartCoroutine(
            ShowDialogue(portrait, lines));
    }

    IEnumerator ShowDialogue(
        Sprite portrait,
        List<string> lines)
    {
        dialoguePanel.SetActive(true);

        portraitImage.sprite = portrait;

        foreach (string line in lines)
        {
            dialogueText.text = line;

            yield return new WaitUntil(() => Input.GetMouseButtonDown(0));

            yield return null;
        }

        dialoguePanel.SetActive(false);
    }
}