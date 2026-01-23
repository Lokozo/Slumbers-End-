using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            //SceneManager.LoadScene("Chapter 2");
            StartCoroutine(LoadSceneAsyncRoutine("Chapter 2"));


        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(LoadSceneAsyncRoutine("Chapter 2"));
            //SceneManager.LoadScene("Chapter 2");
            // Optionally disable this trigger so it doesn't retrigger
            //gameObject.SetActive(false);
        }
    }

    private IEnumerator LoadSceneAsyncRoutine(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        while (!asyncLoad.isDone)
        {
            // Optional: You can show loading progress here via asyncLoad.progress
            yield return null;
        }
    }
}
