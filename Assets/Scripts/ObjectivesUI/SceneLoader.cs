using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            LoadNextScene();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            LoadNextScene();
        }
    }

    void LoadNextScene()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int nextIndex = currentIndex + 1;

        // Prevent going out of bounds
        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            StartCoroutine(LoadSceneAsyncRoutine(nextIndex));
        }
        else
        {
            Debug.Log("No more scenes to load.");
        }
    }

    private IEnumerator LoadSceneAsyncRoutine(int sceneIndex)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}