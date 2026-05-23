using UnityEngine;

public class PuzzleAreaTrigger : MonoBehaviour
{
    public Item requiredNote;

    [Header("Sound")]
    public AudioClip activationSound;
    public AudioSource audioSource;

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered)
            return;

        if (!other.CompareTag("Player"))
            return;

        // PLAYER DOESN'T HAVE NOTE
        if (!PlayerInventory.Instance.HasItem(requiredNote))
        {
            Debug.Log("Missing required note!");
            return;
        }

        triggered = true;

        Debug.Log("Correct area entered!");

        PuzzleManager.Instance.enteredCorrectArea = true;

        // 🔥 PLAY ACTIVATION SOUND
        if (audioSource != null && activationSound != null)
        {
            audioSource.PlayOneShot(activationSound);
        }

        PuzzleManager.Instance.CheckPuzzleComplete();
    }
}