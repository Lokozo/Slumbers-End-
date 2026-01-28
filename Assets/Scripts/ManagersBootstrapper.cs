using UnityEngine;

public class ManagerBootstrapper : MonoBehaviour
{
    private static bool initialized = false;

    private void Awake()
    {
        if (initialized)
        {
            Destroy(gameObject); // destroy duplicate if one already exists
            return;
        }

        initialized = true;
        DontDestroyOnLoad(gameObject); // persist this prefab across scenes
    }
}
