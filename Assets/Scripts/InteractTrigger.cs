using UnityEngine;

public class InteractTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        UIManager.Instance.ladderHoldEIcon.SetActive(true);
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        UIManager.Instance.ladderHoldEIcon.SetActive(false);
    }
}