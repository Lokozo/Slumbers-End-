using UnityEngine;

public class SceneManagerBootstrapper : MonoBehaviour
{
    [Header("Drag your Managers prefab here")]
    public GameObject managersPrefab;

    private void Awake()
    {
        if (managersPrefab == null)
        {
            Debug.LogError("Managers prefab not assigned in SceneManagersBootstrapper!");
            return;
        }

        // Check if any manager already exists
        if (Object.FindFirstObjectByType<InventoryManager>() == null ||
            Object.FindFirstObjectByType<PlayerInventory>() == null ||
            Object.FindFirstObjectByType<TutorialUIManager>() == null)
        {
            Instantiate(managersPrefab); // spawn the prefab if missing
        }
    }
}
