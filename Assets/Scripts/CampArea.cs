using UnityEngine;
using UnityEngine.SceneManagement;

public class CampArea : MonoBehaviour
{
    private bool playerWithinRange = false;
    private bool isCampSceneLoaded = false;
    //private bool campIsSet = false;

    //[SerializeField] private GameObject campSiteModel;

    private bool hasCampPrompt = false;

    private float lastSaveTime = 0f;
    private float saveCooldown = 2f;



    void OnEnable()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    void OnDisable()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }
    void Start()
    {
        if (PlayerPrefs.HasKey("Health"))
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player == null)
            {
                Debug.LogError("Player not found!");
                return;
            }

            PlayerStats stats = PlayerStats.Instance;

            stats.health = PlayerPrefs.GetFloat("Health");
            stats.hunger = PlayerPrefs.GetFloat("Hunger");
            stats.energy = PlayerPrefs.GetFloat("Energy");

            float x = PlayerPrefs.GetFloat("PosX");
            float y = PlayerPrefs.GetFloat("PosY");
            float z = PlayerPrefs.GetFloat("PosZ");


            CharacterController cc = player.GetComponent<CharacterController>();

            if (cc != null)
                cc.enabled = false;


            player.transform.position = new Vector3(x, y, z);

            if (cc != null)
                cc.enabled = true;

            Debug.Log("Game Loaded!");

        }
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
        if (Input.GetKeyDown(KeyCode.E) && playerWithinRange)
        {
            TutorialUIManager.Instance?.Hide();

            SaveGame(); // ✅ SAVE FIRST

            if (!isCampSceneLoaded)
            {
                SceneManager.LoadScene("Campsite", LoadSceneMode.Additive);
                isCampSceneLoaded = true;
            }
        }
    }

    //public void ActivateCamp()
    //{
    //    if (campSiteModel != null)
    //    {
    //        campSiteModel.SetActive(true);
    //        campIsSet = true;
    //        //TutorialUIManager tutorialUI = FindAnyObjectByType<TutorialUIManager>();
    //        //if (tutorialUI != null)
    //        //{
    //        //    tutorialUI.HideInstruction();
    //        //}
    //    }
    //}

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerWithinRange = true;

            PlayerHealth health = other.GetComponent<PlayerHealth>();

            if (health != null && !health.IsDead)
            {
                SaveGame();
            }

            //other.GetComponent<PlayerController>().SetCampZone(this);
            hasCampPrompt = true;
            TutorialUIManager.Instance?.ShowStep(
            "campIntro",
            "Your tent has been set up here.\nThis will serve as your resting point and crafting area.\nPress E to enter the campsite."
        );
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerWithinRange = false;
            //other.GetComponent<PlayerController>().ClearCampZone();
        }
    }
    void SaveGame()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogError("Player not found!");
            return;
        }

        PlayerStats stats = PlayerStats.Instance;

        Vector3 pos = player.transform.position;

        PlayerPrefs.SetFloat("Health", stats.health);
        PlayerPrefs.SetFloat("Hunger", stats.hunger);
        PlayerPrefs.SetFloat("Energy", stats.energy);

        PlayerPrefs.SetFloat("PosX", pos.x);
        PlayerPrefs.SetFloat("PosY", pos.y);
        PlayerPrefs.SetFloat("PosZ", pos.z);

        PlayerPrefs.Save();

        Debug.Log("Game Saved at Campsite!");
    }
}
