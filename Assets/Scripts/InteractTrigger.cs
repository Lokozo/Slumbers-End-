using UnityEngine;

public class InteractTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        UIManager.Instance.ladderHoldEIcon.SetActive(true);

        TutorialUIManager.Instance.ShowStep(
            "LadderTutorial",
            "Hold E to grab the ladder\nPress W or S to climb"
            );
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        UIManager.Instance.ladderHoldEIcon.SetActive(false);
    }
}