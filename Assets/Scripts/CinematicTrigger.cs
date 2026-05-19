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

    [Header("Scene Transition")]
    public SceneLoader sceneLoader;
    public bool loadNextSceneAfterDialogue = false;

    [Header("Player Scripts")]
    public PlayerAttack playerAttack; // ADD THIS

    [Header("Tutorial")]
    public bool showTutorialAfterDialogue;

    public string tutorialStepID;

    [TextArea(2, 5)]
    public string tutorialText;

    [Header("World Icon")]
    public GameObject cinematicIcon;

    private bool triggered;



    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (playOnlyOnce && triggered) return;

        triggered = true;
        StartCoroutine(PlayCinematic());
    }

    private void OnTriggerExit(Collider other)
    {
        TutorialUIManager.Instance.Hide();
    }

    private IEnumerator PlayCinematic()
    {

        // HIDE ICON
        if (cinematicIcon != null)
        {
            cinematicIcon.SetActive(false);
        }

        // DISABLE ATTACK
        if (playerAttack != null)
        {
            playerAttack.canUseAttack = false;
            playerAttack.ForceStopAttack();
        }

        // switch to cinematic camera
        playerCam.Priority = 5;
        objectiveCam.Priority = 20;

        // start dialogue
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(
                speakerName,
                speakerPortrait,
                dialogueLines
            );

            // wait until dialogue finishes
            yield return new WaitUntil(
                () => DialogueManager.Instance.IsPlaying == false);
        }

        // return control to player camera
        playerCam.Priority = 20;
        objectiveCam.Priority = 5;

        // ENABLE ATTACK AGAIN
        if (playerAttack != null)
        {
            playerAttack.canUseAttack = true;
        }

        if (showTutorialAfterDialogue &&
    TutorialUIManager.Instance != null)
        {
            TutorialUIManager.Instance.ShowStep(
                tutorialStepID,
                tutorialText
            );
            //showTutorialAfterDialogue = false; // prevent showing again if triggered multiple times
        }

        // LOAD NEXT SCENE
        if (loadNextSceneAfterDialogue && sceneLoader != null)
        {
            sceneLoader.LoadNextSceneExternally();
        }
    }

    public void PlayExternally()
    {
        if (playOnlyOnce && triggered)
            return;

        triggered = true;

        StartCoroutine(PlayCinematic());
    }
}