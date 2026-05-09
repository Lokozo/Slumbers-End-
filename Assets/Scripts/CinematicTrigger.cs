using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

public class CinematicTrigger : MonoBehaviour
{
    [Header("Cameras")]
    public CinemachineCamera playerCam;
    public CinemachineCamera objectiveCam;

    [Header("Cinematic")]
    public float duration = 2.5f;

    [Header("Dialogue")]
    public Sprite speakerPortrait;

    [TextArea(2, 5)]
    public List<string> dialogueLines = new List<string>();

    [Header("Settings")]
    public bool playOnlyOnce = true;

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (playOnlyOnce && triggered)
            return;

        triggered = true;

        StartCoroutine(PlayCinematic());
    }

    IEnumerator PlayCinematic()
    {
        // 🎥 switch camera
        playerCam.Priority = 5;
        objectiveCam.Priority = 20;

        // 💬 play dialogue
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(
                speakerPortrait,
                dialogueLines
            );
        }

        yield return new WaitForSeconds(duration);

        // 🔙 return to player camera
        playerCam.Priority = 20;
        objectiveCam.Priority = 5;

        // OPTIONAL
        // Destroy(gameObject);
    }
}