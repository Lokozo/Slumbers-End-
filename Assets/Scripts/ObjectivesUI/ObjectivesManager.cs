using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;

public class ObjectivesManager : MonoBehaviour
{
    public static ObjectivesManager Instance;

    public List<Objective> objectives = new List<Objective>();
    private Dictionary<string, ObjectiveUI> objectiveUIItems = new Dictionary<string, ObjectiveUI>();

    public Transform objectivesParent;      // Assigned dynamically on scene load
    public GameObject objectiveItemPrefab; // Assign your ObjectiveItem prefab here in inspector

    private int currentObjectiveIndex = 0;
    private ObjectiveUI currentObjectiveUI;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void ShowCurrentObjective()
    {
        if (currentObjectiveIndex < 0 || currentObjectiveIndex >= objectives.Count)
        {
            Debug.Log("No more objectives to show.");
            return;
        }

        Objective currentObjective = objectives[currentObjectiveIndex];

        // Instantiate UI prefab for current objective under the current objectivesParent
        GameObject objUIgo = Instantiate(objectiveItemPrefab, objectivesParent);
        currentObjectiveUI = objUIgo.GetComponent<ObjectiveUI>();
        currentObjectiveUI.Setup(currentObjective);

        // Register UI so it can be updated later
        RegisterObjectiveUI(currentObjective.id, currentObjectiveUI);
    }

    public void RegisterObjectiveUI(string id, ObjectiveUI ui)
    {
        if (!objectiveUIItems.ContainsKey(id))
            objectiveUIItems.Add(id, ui);
    }

    public void CompleteObjective(string objectiveID)
    {
        if (Instance == null)
        {
            Debug.LogError("ObjectivesManager instance is null!");
            return;
        }
        StartCoroutine(CompleteObjectiveRoutine(objectiveID));
        Debug.Log("COMPLETE");
    }

    private IEnumerator CompleteObjectiveRoutine(string objectiveID)
    {
        if (currentObjectiveIndex >= objectives.Count) yield break;

        Objective obj = objectives.Find(o => o.id == objectiveID);
        if (obj != null && !obj.isCompleted && objectiveUIItems.ContainsKey(objectiveID))
        {
            obj.isCompleted = true;

            var ui = objectiveUIItems[objectiveID];

            if (ui != null)
            {
                ui.UpdateUIState();

                yield return ui.PlayCompletionAnimation();

                if (ui != null)
                {
                    Destroy(ui.gameObject);
                    objectiveUIItems.Remove(objectiveID);
                }
            }
            else
            {
                Debug.LogWarning($"ObjectiveUI for ID {objectiveID} is null or already destroyed.");
                objectiveUIItems.Remove(objectiveID);
            }

            currentObjectiveIndex++;
            if (currentObjectiveIndex < objectives.Count)
            {
                ShowCurrentObjective();
            }
            else
            {
                Debug.Log("All objectives completed!");
            }
        }
    }

    private void DetermineCurrentObjectiveIndex()
    {
        currentObjectiveIndex = objectives.FindIndex(o => !o.isCompleted);
        if (currentObjectiveIndex == -1)
        {
            // No incomplete objectives: all completed
            currentObjectiveIndex = objectives.Count;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Find the UI container in the new scene
        objectivesParent = GameObject.Find("ObjectivesPanel")?.transform;

        if (objectivesParent == null)
        {
            Debug.LogWarning("ObjectivesPanel not found in scene!");
            return;
        }

        // Destroy all old UI children in the panel
        foreach (Transform child in objectivesParent)
            Destroy(child.gameObject);

        // Clear dictionary since those UI elements are destroyed
        objectiveUIItems.Clear();

        // Determine which objective should be currently shown
        DetermineCurrentObjectiveIndex();

        if (currentObjectiveIndex < objectives.Count)
            ShowCurrentObjective();
        else
            Debug.Log("All objectives completed!");
    }
}
