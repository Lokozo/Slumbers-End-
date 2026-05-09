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

    [Header("Settings")]
    public float textSpeed = 0.02f;

    private Coroutine dialogueRoutine;

    private bool waitingForNext;
    private bool isTyping;

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

    private IEnumerator ShowDialogue(
        Sprite portrait,
        List<string> lines)
    {
        dialoguePanel.SetActive(true);

        portraitImage.sprite = portrait;

        foreach (string line in lines)
        {
            yield return StartCoroutine(TypeLine(line));

            waitingForNext = true;

            yield return new WaitUntil(() => Input.GetMouseButtonDown(0));

            waitingForNext = false;

            yield return null;
        }

        dialoguePanel.SetActive(false);
    }

    private IEnumerator TypeLine(string line)
    {
        isTyping = true;

        dialogueText.text = "";

        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(textSpeed);
        }

        isTyping = false;
    }
}