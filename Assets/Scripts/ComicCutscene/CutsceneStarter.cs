using System.Collections;
using UnityEngine;

public class CutsceneStarter : MonoBehaviour
{
    public GameObject cutsceneParent;

    //private IEnumerator Start()
    //{
    //    yield return null; // wait 1 frame

    //    if (CutsceneManager.Instance != null && cutsceneParent != null)
    //    {
    //        CutsceneManager.Instance.PlayCutscene(cutsceneParent);
    //    }
    //    else
    //    {
    //        Debug.LogWarning("CutsceneStarter: Missing reference.");
    //    }
    //}
    private IEnumerator Start()
    {
        yield return new WaitUntil(() => CutsceneManager.Instance != null);

        yield return null;

        if (cutsceneParent != null)
            CutsceneManager.Instance.PlayCutscene(cutsceneParent);
    }
}