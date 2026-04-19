using UnityEngine;

public class CutsceneTrigger : MonoBehaviour
{
    public GameObject cutsceneParent;

    private bool hasPlayed = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasPlayed) return;

        if (other.CompareTag("Player"))
        {
            hasPlayed = true;
            CutsceneManager.Instance.PlayCutscene(cutsceneParent);
        }
    }
}