using UnityEngine;

public class ManagerBootstrapper : MonoBehaviour
{
    private static bool initialized = false;

    private void Awake()
    {
        if (initialized)
        {
            Destroy(gameObject);
            return;
        }

        initialized = true;
        DontDestroyOnLoad(gameObject);

        // 🔥 Initialize ALL child managers in correct order
        InitializeChildren();
    }

    private void InitializeChildren()
    {
        // Initialize PlayerStats FIRST (needs to be singleton)
        PlayerStats[] stats = GetComponentsInChildren<PlayerStats>();
        foreach (var stat in stats)
        {
            stat.Initialize();
        }

        // 🎵 Initialize AudioManager
        AudioManager audio = GetComponentInChildren<AudioManager>();
        if (audio != null)
        {
            audio.Initialize();
        }
    }
}