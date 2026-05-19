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

            // BLOCK ATTACK
            PlayerAttack attack = other.GetComponent<PlayerAttack>();

            if (attack != null)
            {
                attack.canUseAttack = false;
                attack.ForceStopAttack();
            }

            CutsceneManager.Instance.PlayCutscene(cutsceneParent);
        }
    }
}