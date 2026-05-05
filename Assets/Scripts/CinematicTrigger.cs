using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class CinematicTrigger : MonoBehaviour
{
    public CinemachineCamera playerCam;
    public CinemachineCamera objectiveCam;

    public float duration = 2.5f;

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;

        if (other.CompareTag("Player"))
        {
            triggered = true;
            StartCoroutine(PlayCinematic());
        }
    }

    IEnumerator PlayCinematic()
    {
        // 🎥 switch to objective camera
        playerCam.Priority = 5;
        objectiveCam.Priority = 20;

        yield return new WaitForSeconds(duration);

        // 🔙 back to player camera
        playerCam.Priority = 20;
        objectiveCam.Priority = 5;
    }
}