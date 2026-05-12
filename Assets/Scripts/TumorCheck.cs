using UnityEngine;
using System.Collections;

public class TumorCheck : MonoBehaviour
{
    public bool tumorActivated = false;

    [Header("Effects")]
    public GameObject fireEffects;

    [Header("Audio")]
    public AudioSource tumorAudio;

    [Header("Required Item")]
    public Item lighterItem;

    [Header("Optional Cinematic")]
    public CinematicTrigger tumorTrigger;

    [Header("Ladder")]
    public Ladder basementLadder;

    private void Start()
    {
        // Fire OFF at start
        if (fireEffects != null)
            fireEffects.SetActive(false);

        // Sound OFF at start
        if (tumorAudio != null)
        {
            tumorAudio.Stop();
            tumorAudio.playOnAwake = false;
        }

        // Disable ladder first
        if (basementLadder != null)
            basementLadder.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (tumorActivated)
            return;

        // Check if player has lighter
        if (PlayerInventory.Instance.HasResource(lighterItem))
        {
            tumorActivated = true;

            // Turn ON fire
            if (fireEffects != null)
                fireEffects.SetActive(true);

            // Play fire sound
            if (tumorAudio != null)
                tumorAudio.Play();

            Debug.Log("Tumor Burned");

            // Optional cinematic
            if (tumorTrigger != null)
            {
                StartCoroutine(StartTumorEvent());
            }
        }
        else
        {
            Debug.Log("Need Lighter");
        }
    }

    private IEnumerator StartTumorEvent()
    {
        yield return new WaitForSeconds(2f);

        tumorTrigger.PlayExternally();
    }
}