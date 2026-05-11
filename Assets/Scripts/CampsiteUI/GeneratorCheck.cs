using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class GeneratorCheck : MonoBehaviour
{
    public bool generatorStarted = false;

    [Header("Audio")]
    public AudioSource generatorAudio;

    [Header("Required Item")]
    public Item gasItem;

    [Header("Cinematic")]
    public CinematicTrigger phoneDialogueTrigger;

    private void Start()
    {
        // PRELOAD AUDIO
        if (generatorAudio != null && generatorAudio.clip != null)
        {
            generatorAudio.clip.LoadAudioData();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (generatorStarted)
            return;

        // Check for gas
        if (PlayerInventory.Instance.HasResource(gasItem))
        {
            generatorStarted = true;

            // Remove gas
            PlayerInventory.Instance.RemoveResource(gasItem, 1);

            // Play sound
            if (generatorAudio != null)
                generatorAudio.Play();

            Debug.Log("Generator Started");

            // PLAY PHONE CINEMATIC
            if (phoneDialogueTrigger != null)
            {
                StartCoroutine(StartPhoneCall());
            }
        }
        else
        {
            Debug.Log("Need Gas");
        }
    }

    private IEnumerator StartPhoneCall()
    {
        yield return new WaitForSeconds(2f);

        phoneDialogueTrigger.PlayExternally();
    }
}