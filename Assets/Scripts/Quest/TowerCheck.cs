using System.Collections.Generic;
using UnityEngine;

public class TowerCheck : MonoBehaviour
{
    public Sprite playerPortrait;

    private bool checkedTower = false;
    private bool playerInside = false;

    [Header("Hold Settings")]
    public float holdTimeRequired = 2f;

    private float holdTimer = 0f;

    private void Update()
    {
        if (!playerInside || checkedTower)
            return;

        // Holding E
        if (Input.GetKey(KeyCode.E))
        {
            holdTimer += Time.deltaTime;

            // Optional debug
            Debug.Log("Inspecting Tower: " + holdTimer);

            if (holdTimer >= holdTimeRequired)
            {
                checkedTower = true;

                //DialogueManager.Instance.StartDialogue(
                //    playerPortrait,
                //    new List<string>
                //    {
                //        "The tower definitely looks run-down. Still no signal.\nMaybe the generator still works."
                //    }
                //);
            }
        }
        else
        {
            // Reset if released early
            holdTimer = 0f;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;

            Debug.Log("Hold E to inspect tower");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            holdTimer = 0f;
        }
    }
}