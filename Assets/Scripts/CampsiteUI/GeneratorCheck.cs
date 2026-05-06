using System.Collections.Generic;
using UnityEngine;

public class GeneratorCheck : MonoBehaviour
{
    public bool checkedGenerator = false;
    public bool generatorStarted = false;

    public Sprite playerPortrait;

    [Header("Audio")]
    public AudioSource generatorAudio;

    [Header("Required Item")]
    public Item gasItem;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        // =========================
        // FIRST CHECK
        // =========================
        if (!checkedGenerator)
        {
            checkedGenerator = true;

            DialogueManager.Instance.StartDialogue(
                playerPortrait,
                new List<string>
                {
                    "The generator still looks connected...",
                    "It's just covered in vines.",
                    "And there's no gas left."
                }
            );

            return;
        }

        // =========================
        // GENERATOR ALREADY STARTED
        // =========================
        if (generatorStarted)
            return;

        // =========================
        // CHECK FOR GAS
        // =========================
        if (PlayerInventory.Instance.HasResource(gasItem))
        {
            generatorStarted = true;

            // Remove gas
            PlayerInventory.Instance.RemoveResource(gasItem, 1);

            // Play sound
            generatorAudio.Play();

            DialogueManager.Instance.StartDialogue(
                playerPortrait,
                new List<string>
                {
                    "Come on...",
                    "Please work...",
                    "There we go.",
                    "The tower should have power now."
                }
            );

            // Objective update here
            // ObjectivesManager.Instance.StartNextObjective();
        }
    }
}