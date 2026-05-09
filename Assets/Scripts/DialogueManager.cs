using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI (single panel)")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public Image portraitImage;
    public TextMeshProUGUI speakerNameText;

    [Header("Settings")]
    public float textSpeed = 0.02f;

    public bool IsPlaying { get; private set; }

    private Coroutine routine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        dialoguePanel.SetActive(false);
    }

    public void StartDialogue(string speakerName, Sprite portrait, List<string> lines)
    {
        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(RunDialogue(speakerName, portrait, lines));
    }

    private IEnumerator RunDialogue(string speakerName, Sprite portrait, List<string> lines)
    {
        IsPlaying = true;
        dialoguePanel.SetActive(true);

        speakerNameText.text = speakerName;
        portraitImage.sprite = portrait;

        foreach (string line in lines)
        {
            yield return TypeLine(line);

            yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
        }

        dialoguePanel.SetActive(false);
        IsPlaying = false;
    }

    private IEnumerator TypeLine(string line)
    {
        dialogueText.text = "";

        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
    }
}