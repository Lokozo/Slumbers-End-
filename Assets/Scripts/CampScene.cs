using UnityEngine;
using UnityEngine.SceneManagement;

public class CampScene : MonoBehaviour
{
    private Animator animator;

    //void Start()
    //{
    //    GameObject player = GameObject.FindGameObjectWithTag("Player");
    //    if (player == null)
    //    {
    //        Debug.LogError("No player found in scene!");
    //        return;
    //    }

    //    animator = player.GetComponent<Animator>();
    //    if (animator == null)
    //    {
    //        Debug.LogError("Player does not have an Animator!");
    //        return;
    //    }

    //    int campLayerIndex = animator.GetLayerIndex("Camp Layer");
    //    if (campLayerIndex >= 0)
    //    {
    //        animator.SetLayerWeight(campLayerIndex, 1f);
    //        Debug.Log("Camp Layer activated.");
    //    }
    //    else
    //    {
    //        Debug.LogWarning("Camp Layer not found in Animator.");
    //    }
    //}

    void Update()
    {
        // Press ESC to exit the campsite scene
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (SceneManager.GetSceneByName("Campsite").isLoaded)
            {
                SceneManager.UnloadSceneAsync("Campsite");
                Debug.Log("Exited campsite scene.");
            }
        }
    }

    void Start()
    {
        Scene campScene = gameObject.scene;

        foreach (GameObject root in campScene.GetRootGameObjects())
        {
            if (root.CompareTag("Player"))
            {
                Animator animator = root.GetComponent<Animator>();
                if (animator != null)
                {
                    int campLayerIndex = animator.GetLayerIndex("Camp Layer");
                    if (campLayerIndex >= 0)
                    {
                        animator.SetLayerWeight(campLayerIndex, 1f);
                        Debug.Log("Camp Layer activated for Campsite scene player.");
                    }
                    else
                    {
                        Debug.LogWarning("Camp Layer not found in Animator.");
                    }
                }
                return;
            }
        }

        Debug.LogError("No Player with tag found in current Campsite scene!");
    }
}
