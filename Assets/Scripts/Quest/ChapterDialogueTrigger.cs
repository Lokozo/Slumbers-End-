using System.Collections.Generic;
using UnityEngine;

public class ChapterDialogueTrigger : MonoBehaviour
{
    [Header("Dialogue")]
    public Sprite speakerPortrait;

    [TextArea(2, 5)]
    public List<string> dialogueLines = new List<string>();

    [Header("Settings")]
    public bool playOnlyOnce = true;

    private bool hasPlayed = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (playOnlyOnce && hasPlayed)
            return;

        hasPlayed = true;

        DialogueManager.Instance.StartDialogue(
            speakerPortrait,
            dialogueLines
        );
    }
}