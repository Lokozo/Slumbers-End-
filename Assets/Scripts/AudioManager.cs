using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio")]
    public AudioSource musicSource;

    [System.Serializable]
    public class SceneMusicEntry
    {
#if UNITY_EDITOR
        public SceneAsset scene; // 👈 drag scene here in inspector
#endif

        [HideInInspector]
        public string sceneName; // runtime use

        public AudioClip clip;
        public bool loop = true;
        public float volume = 1f;
    }

    [Header("Scene Music List")]
    public List<SceneMusicEntry> sceneMusic = new List<SceneMusicEntry>();

    private void Awake()
    {
        Instance = this;
        CacheSceneNames();
    }

    public void Initialize()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        Debug.Log("🎵 AudioManager initialized (SceneAsset support enabled)");
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        CacheSceneNames();
    }
#endif

    private void CacheSceneNames()
    {
        foreach (var entry in sceneMusic)
        {
#if UNITY_EDITOR
            if (entry.scene != null)
                entry.sceneName = entry.scene.name;
#endif
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayForScene(scene.name);
    }

    private void PlayForScene(string sceneName)
    {
        SceneMusicEntry entry = sceneMusic.Find(x => x.sceneName == sceneName);

        if (entry == null || entry.clip == null)
            return;

        if (musicSource.clip == entry.clip)
            return;

        musicSource.clip = entry.clip;
        musicSource.loop = entry.loop;
        musicSource.volume = entry.volume;
        musicSource.Play();
    }
}