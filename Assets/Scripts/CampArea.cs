using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CampArea : MonoBehaviour
{
    [Header("Respawn Point")]
    [SerializeField] private Transform playerRespawnPoint;

    private bool playerWithinRange = false;
    private bool isCampSceneLoaded = false; // 🔥 Reset this properly
    private bool isInitialized = false;
    private bool wasInCamp = false;

    [SerializeField] private GameObject blackOverlay;
    [SerializeField] private float fadeDelay = 1f;

    [Header("Camp Settings")]
    [SerializeField] private bool blockPlayerInventory = true; // Toggle in inspector

    void Start()
    {
        Debug.Log("Camp Layer activated for Campsite scene player.");

        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    // 🔥 RESET FLAG WHEN CAMPSITE UNLOADS
    void OnSceneUnloaded(Scene scene)
    {
        if (scene.name == "Campsite")
        {
            isCampSceneLoaded = false;
            Debug.Log("🏕️ Campsite unloaded - Can re-enter!");
        }
    }

    void Update()
    {
        // 🔥 BLOCK INVENTORY IN CAMP
        if (isCampSceneLoaded && blockPlayerInventory)
        {
            BlockPlayerInventory(true);
            wasInCamp = true;
        }
        else if (wasInCamp)
        {
            BlockPlayerInventory(false); // Restore when exit
            wasInCamp = false;
        }

        if (Input.GetKeyDown(KeyCode.E) && playerWithinRange && !isCampSceneLoaded)
        {
            CreateCheckpoint();
            StartCoroutine(LoadCampSceneSmooth());
        }
    }
    public bool IsInCamp()
    {
        return isCampSceneLoaded;
    }
    void BlockPlayerInventory(bool block)
    {

        // 🔥 ALSO BLOCK INPUT
        if (Input.GetKeyDown(KeyCode.I) && block) // I = inventory key
        {
            Debug.Log("⚠️ Inventory blocked in camp!");
        }
    }
    void CreateCheckpoint()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        TeleportPlayerSafely(player, playerRespawnPoint.position);
        SaveCheckpoint();
        Debug.Log($"✅ CHECKPOINT SAVED at {playerRespawnPoint.name}");
    }

    void SaveCheckpoint()
    {
        PlayerStats stats = PlayerStats.Get();

        PlayerPrefs.SetFloat("CheckpointHealth", stats.health);
        PlayerPrefs.SetFloat("CheckpointHunger", stats.hunger);
        PlayerPrefs.SetFloat("CheckpointEnergy", stats.energy);

        PlayerPrefs.SetFloat("CheckpointPosX", playerRespawnPoint.position.x);
        PlayerPrefs.SetFloat("CheckpointPosY", playerRespawnPoint.position.y);
        PlayerPrefs.SetFloat("CheckpointPosZ", playerRespawnPoint.position.z);

        PlayerPrefs.SetString("LastCheckpoint", "Camp");
        PlayerPrefs.Save();
    }


    // 🔥 STATIC RESPAWN (ONLY CALLED ON DEATH)
    public static void RespawnAtCheckpoint()
    {
        if (!PlayerPrefs.HasKey("CheckpointHealth"))
        {
            Debug.LogWarning("No camp checkpoint!");
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        PlayerStats stats = PlayerStats.Get();
        PlayerInventory inventory = PlayerInventory.Instance;

        // 20% HEALTH REMAINING
        stats.hunger = PlayerPrefs.GetFloat("CheckpointHunger");
        stats.energy = PlayerPrefs.GetFloat("CheckpointEnergy");
        stats.health = stats.maxHealth * 0.20f;

        Debug.Log($"💀 Health set to 20%: {stats.health:F1}/{stats.maxHealth}");

        // 30% INVENTORY LOSS
        ApplyStaticInventoryPenalty(inventory, 0.30f);

        // RESPAWN POSITION
        Vector3 respawnPos = new Vector3(
            PlayerPrefs.GetFloat("CheckpointPosX"),
            PlayerPrefs.GetFloat("CheckpointPosY"),
            PlayerPrefs.GetFloat("CheckpointPosZ")
        );

        TeleportPlayerStatic(player, respawnPos);
        Debug.Log("🔄 CAMP RESPAWN COMPLETE!");
    }

    private static void ApplyStaticInventoryPenalty(PlayerInventory inventory, float percent)
    {
        if (inventory == null) return;

        var inventoryCopy = inventory.GetInventory().ToList();
        foreach (var kvp in inventoryCopy)
        {
            Item item = kvp.Key;
            if (item is WeaponItem) continue;

            int loss = Mathf.RoundToInt(kvp.Value * percent);
            if (loss > 0)
            {
                inventory.RemoveItem(item, loss);
                Debug.Log($"🎒 {item.itemName}: -{loss}");
            }
        }
    }

    private static void TeleportPlayerStatic(GameObject player, Vector3 position)
    {
        CharacterController cc = player.GetComponent<CharacterController>();
        Rigidbody rb = player.GetComponent<Rigidbody>();

        if (cc != null) cc.enabled = false;
        if (rb != null) rb.isKinematic = true;

        player.transform.position = position;
        player.transform.rotation = Quaternion.identity;

        if (cc != null) cc.enabled = true;
        if (rb != null) rb.isKinematic = false;
    }

    void TeleportPlayerSafely(GameObject player, Vector3 position)
    {
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        player.transform.position = position;
        player.transform.rotation = Quaternion.identity;

        if (cc != null) cc.enabled = true;
    }

    private IEnumerator LoadCampSceneSmooth()
    {
        // FADE IN
        if (blackOverlay != null)
            blackOverlay.SetActive(true);

        yield return new WaitForSeconds(fadeDelay);

        SceneManager.LoadScene("Campsite", LoadSceneMode.Additive);

        isCampSceneLoaded = true;

        Debug.Log("🏕️ Entering Campsite...");

        yield return new WaitForSeconds(0.2f);

        // FADE OUT
        if (blackOverlay != null)
            blackOverlay.SetActive(false);
    }
    public IEnumerator ExitCampRoutine()
    {
        // FADE IN
        if (blackOverlay != null)
            blackOverlay.SetActive(true);

        yield return new WaitForSeconds(fadeDelay);

        yield return SceneManager.UnloadSceneAsync("Campsite");

        yield return new WaitForSeconds(0.2f);

        // FADE OUT
        if (blackOverlay != null)
            blackOverlay.SetActive(false);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerWithinRange = true;
            TutorialUIManager.Instance?.ShowStep(
                "campIntro",
                $"🏕️ Press E to SAVE & ENTER TENT\n📍 {playerRespawnPoint?.name ?? "None"}"
            );
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerWithinRange = false;
            TutorialUIManager.Instance?.Hide();
        }
    }
}