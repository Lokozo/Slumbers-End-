using UnityEngine;

public class ObjectiveTrigger : MonoBehaviour
{
    public string objectiveID;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ObjectivesManager.Instance.CompleteObjective(objectiveID);
            // Optionally disable this trigger so it doesn't retrigger
            gameObject.SetActive(false);
        }
    }
}
