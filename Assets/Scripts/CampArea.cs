using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class CampArea : MonoBehaviour
{
    [SerializeField] private Vector3 respawnOffset = new Vector3(0, 1f, 2f); // Offset from camp center
    private bool playerWithinRange = false;
    private bool isCampSceneLoaded = false;
    private Vector3 campRespawnPoint; // ✅ RESPAWN POINT
    private bool hasCampPrompt = false;

    void Start()
    {
        // ✅ SETUP RESPAWN POINT from saved camp position
        campRespawnPoint = new Vector3(
            PlayerPrefs.GetFloat("CampPosX", transform.position.x),
            PlayerPrefs.GetFloat("CampPosY", transform.position.y),
            PlayerPrefs.GetFloat("CampPosZ", transform.position.z)
        ) + respawnOffset;

        if (PlayerPrefs.HasKey("Health"))
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;

            PlayerStats stats = PlayerStats.Get();
            stats.health = PlayerPrefs.GetFloat("Health");
            stats.hunger = PlayerPrefs.GetFloat("Hunger");
            stats.energy = PlayerPrefs.GetFloat("Energy");

            // ✅ ONLY teleport if NOT near CAMP RESPAWN POINT
            float distanceToCamp = Vector3.Distance(player.transform.position, campRespawnPoint);
            if (distanceToCamp > 10f) // Was far from camp
            {
                Vector3 savedPos = new Vector3(
                    PlayerPrefs.GetFloat("PosX"),
                    PlayerPrefs.GetFloat("PosY"),
                    PlayerPrefs.GetFloat("PosZ")
                );
                TeleportPlayer(player, savedPos);
                Debug.Log($"Loaded from world position (dist to camp: {distanceToCamp:F1}m)");
            }
            else // Was at camp
            {
                TeleportPlayer(player, campRespawnPoint);
                Debug.Log("Loaded from camp - spawned at respawn point!");
            }
        }
        else
        {
            // First time - spawn at camp
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player) TeleportPlayer(player, campRespawnPoint);
        }
    }

    void TeleportPlayer(GameObject player, Vector3 position)
    {
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        player.transform.position = position;
        if (cc != null) cc.enabled = true;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && playerWithinRange && !isCampSceneLoaded)
        {
            TutorialUIManager.Instance?.Hide();

            // ✅ LOCK PLAYER AT RESPAWN POINT
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                campRespawnPoint = transform.position + respawnOffset;
                TeleportPlayer(player, campRespawnPoint);
                SaveGame(); // Saves respawn point too
            }

            StartCoroutine(LoadCampSceneSmooth());
        }
    }

    private IEnumerator LoadCampSceneSmooth()
    {
        yield return null; // 1 frame delay
        SceneManager.LoadScene("Campsite", LoadSceneMode.Additive);
        isCampSceneLoaded = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerWithinRange = true;

            // ✅ UPDATE RESPAWN POINT every time player enters
            campRespawnPoint = transform.position + respawnOffset;
            PlayerPrefs.SetFloat("CampPosX", campRespawnPoint.x);
            PlayerPrefs.SetFloat("CampPosY", campRespawnPoint.y);
            PlayerPrefs.SetFloat("CampPosZ", campRespawnPoint.z);

            PlayerHealth health = other.GetComponent<PlayerHealth>();
            if (health != null && !health.IsDead) SaveGame();

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

    void SaveGame()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        PlayerStats stats = PlayerStats.Get();
        Vector3 pos = player.transform.position;

        PlayerPrefs.SetFloat("Health", stats.health);
        PlayerPrefs.SetFloat("Hunger", stats.hunger);
        PlayerPrefs.SetFloat("Energy", stats.energy);
        PlayerPrefs.SetFloat("PosX", pos.x);
        PlayerPrefs.SetFloat("PosY", pos.y);
        PlayerPrefs.SetFloat("PosZ", pos.z);

        // ✅ ALSO SAVE CAMP RESPAWN POINT
        PlayerPrefs.SetFloat("CampPosX", campRespawnPoint.x);
        PlayerPrefs.SetFloat("CampPosY", campRespawnPoint.y);
        PlayerPrefs.SetFloat("CampPosZ", campRespawnPoint.z);

        PlayerPrefs.Save();
        Debug.Log($"Game Saved! Respawn: {campRespawnPoint}");
    }
}