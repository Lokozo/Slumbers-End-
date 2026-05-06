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
        // wait for manager
        yield return new WaitUntil(() => CutsceneManager.Instance != null);

       
        //blackOverlay = UIManager.Instance.blackOverlay;

         // FORCE BLACK ON LOAD

        yield return null; // wait 1 frame



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