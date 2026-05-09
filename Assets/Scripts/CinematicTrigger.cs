using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

public class CinematicTrigger : MonoBehaviour
{
    [Header("Cameras")]
    public CinemachineCamera playerCam;
    public CinemachineCamera objectiveCam;

    [Header("Dialogue")]
    public string speakerName;
    public Sprite speakerPortrait;

    [TextArea(2, 5)]
    public List<string> dialogueLines = new List<string>();

    [Header("Settings")]
    public bool playOnlyOnce = true;

    private bool triggered;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (playOnlyOnce && triggered) return;

        triggered = true;
        StartCoroutine(PlayCinematic());
    }

    private IEnumerator PlayCinematic()
    {
        //  switch to cinematic camera
        playerCam.Priority = 5;
        objectiveCam.Priority = 20;

        //  start dialogue (fully handled by DialogueManager)
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(
                speakerName,
                speakerPortrait,
                dialogueLines
            );

            // wait until dialogue finishes
            yield return new WaitUntil(() => DialogueManager.Instance.IsPlaying == false);
        }

        //  return control to player camera
        playerCam.Priority = 20;
        objectiveCam.Priority = 5;
    }
}