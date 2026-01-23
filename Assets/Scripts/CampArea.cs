using UnityEngine;
using UnityEngine.SceneManagement;

public class CampArea : MonoBehaviour
{
    private bool playerWithinRange = false;
    private bool isCampSceneLoaded = false;
    private bool campIsSet = false;

    [SerializeField] private GameObject campSiteModel;

    private bool hasCampPrompt = false;

    void OnEnable()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    void OnDisable()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void OnSceneUnloaded(Scene scene)
    {
        if (scene.name == "Campsite")
        {
            isCampSceneLoaded = false;
            Debug.Log("Campsite scene unloaded. Flag reset.");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && playerWithinRange && campIsSet && !isCampSceneLoaded)
        {
            SceneManager.LoadScene("Campsite", LoadSceneMode.Additive);
            isCampSceneLoaded = true;
        }
    }

    public void ActivateCamp()
    {
        if (campSiteModel != null)
        {
            campSiteModel.SetActive(true);
            campIsSet = true;
            TutorialUIManager tutorialUI = FindAnyObjectByType<TutorialUIManager>();
            if (tutorialUI != null)
            {
                tutorialUI.HideInstruction(); // Fades it out cleanly
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerWithinRange = true;
            other.GetComponent<PlayerController>().SetCampZone(this);
            
            hasCampPrompt = true;
            TutorialUIManager tutorial = FindAnyObjectByType<TutorialUIManager>();
            if (hasCampPrompt && tutorial != null)
            {
                tutorial.ShowInteractionInstruction("Hold E to set up camp");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerWithinRange = false;
            other.GetComponent<PlayerController>().ClearCampZone();
        }
    }
}
