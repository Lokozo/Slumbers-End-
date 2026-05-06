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

        if (cutsceneParent != null)
        {
            CutsceneManager.Instance.PlayCutscene(cutsceneParent);
        }
        else
        {
            Debug.LogWarning("Missing cutsceneParent");
        }
    }

    // Called by CutsceneManager when first panel is ready
    public void ReleaseBlackScreen()
    {
        if (blackOverlay != null)
            blackOverlay.SetActive(false);
    }
}