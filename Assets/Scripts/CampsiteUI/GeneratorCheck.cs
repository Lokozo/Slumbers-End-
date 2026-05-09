using UnityEngine;

public class GeneratorCheck : MonoBehaviour
{
    public bool generatorStarted = false;

    [Header("Audio")]
    public AudioSource generatorAudio;

    [Header("Required Item")]
    public Item gasItem;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        // Already activated
        if (generatorStarted)
            return;

        // Check for gas
        if (PlayerInventory.Instance.HasResource(gasItem))
        {
            generatorStarted = true;

            // Remove gas
            PlayerInventory.Instance.RemoveResource(gasItem, 1);

            // Play generator sound
            if (generatorAudio != null)
                generatorAudio.Play();

            Debug.Log("Generator Started");

            // OPTIONAL:
            // ObjectivesManager.Instance.StartNextObjective();
        }
        else
        {
            Debug.Log("Need Gas");
        }
    }
}