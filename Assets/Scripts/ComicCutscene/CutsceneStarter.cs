using System.Collections;
using UnityEngine;

public class CutsceneStarter : MonoBehaviour
{
    public GameObject cutsceneParent;
    public GameObject blackOverlay;

    private IEnumerator Start()
    {
        if (blackOverlay != null)
            blackOverlay.SetActive(true);

        yield return new WaitUntil(() => CutsceneManager.Instance != null);

        yield return null;

        SceneLoader loader = FindFirstObjectByType<SceneLoader>();

        if (loader != null)
            loader.SetWaitingForCutscene(true);

        // BLOCK ATTACK
        PlayerAttack attack = FindFirstObjectByType<PlayerAttack>();

        if (attack != null)
        {
            attack.canUseAttack = false;
            attack.ForceStopAttack();
        }

        // START CUTSCENE
        if (cutsceneParent != null)
        {
            CutsceneManager.Instance.PlayCutscene(cutsceneParent);
        }
        else
        {
            Debug.LogWarning("Missing cutsceneParent");
        }

        // WAIT A LITTLE THEN RE-ENABLE
        yield return new WaitForSeconds(5f);

        if (attack != null)
        {
            attack.canUseAttack = true;

            Debug.Log("✅ Attack Re-enabled");
        }
    }

    // Called by CutsceneManager when first panel is ready
    public void ReleaseBlackScreen()
    {
        if (blackOverlay != null)
            blackOverlay.SetActive(false);
    }
}