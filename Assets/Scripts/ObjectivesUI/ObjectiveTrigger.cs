using UnityEngine;

public class ObjectiveTrigger : MonoBehaviour
{
    [Tooltip("ID of the sub or main objective this trigger completes")]
    public string objectiveID;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;

        if (other.CompareTag("Player"))
        {
            hasTriggered = true;

            if (ObjectivesManager.Instance != null)
            {
                ObjectivesManager.Instance.CompleteObjective(objectiveID);
                Debug.Log($"Triggered objective: {objectiveID}");
            }
            else
            {
                Debug.LogWarning("ObjectivesManager instance not found!");
            }

            // Disable trigger after use
            gameObject.SetActive(false);
        }
    }
}
