using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ObjectivesManager : MonoBehaviour
{
    public static ObjectivesManager Instance;

    [Header("Objective Data")]
    public List<MainObjective> mainObjectives = new List<MainObjective>();

    [Header("UI References")]
    public Transform objectivesParent;
    public GameObject objectivesPanel;  //

    public GameObject mainObjectivePrefab;
    public GameObject subObjectivePrefab;

    private int currentMainIndex = 0;
    private int currentSubIndex = 0;

    private MainObjectiveUI currentMainUI;

    

    // NEW: runtime copy of subobjectives
    private List<SubObjective> runtimeSubObjectives = new List<SubObjective>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (objectivesParent == null)
        {
           //objectivesParent = GameObject.Find("ObjectivesPanel")?.transform;


            if (objectivesParent == null)
                Debug.LogWarning("ObjectivesPanel not found in Start!");
        }

        StartCurrentObjective();
    }

    // =============================
    // START CURRENT MAIN OBJECTIVE
    // =============================
    private void StartCurrentObjective()
    {
        if (currentMainIndex >= mainObjectives.Count)
        {
            Debug.Log("All main objectives completed!");
            return;
        }

        if (objectivesParent == null)
        {
            Debug.LogWarning("ObjectivesParent not assigned!");
            return;
        }

        // Destroy old UI
        foreach (Transform child in objectivesParent)
        {
            Destroy(child.gameObject);
        }

        currentMainUI = null;

        MainObjective main = mainObjectives[currentMainIndex];

        // -----------------------------
        // BUILD RUNTIME SUBOBJECTIVE LIST
        // -----------------------------
        runtimeSubObjectives = new List<SubObjective>();

        if (main.subObjectives != null && main.subObjectives.Count > 0)
        {
            runtimeSubObjectives.AddRange(main.subObjectives);
        }
        else
        {
            runtimeSubObjectives.Add(new SubObjective
            {
                id = main.id + "_dummy",
                description = main.description
            });
        }

        // Spawn UI
        GameObject mainUIObj = Instantiate(mainObjectivePrefab, objectivesParent);
        currentMainUI = mainUIObj.GetComponent<MainObjectiveUI>();

        currentMainUI.subPrefab = subObjectivePrefab;
        currentMainUI.Setup(main);

        currentSubIndex = 0;

        ActivateCurrentSubObjective();

        Debug.Log("Starting Main Objective: " + main.title);
    }

    // =============================
    // ACTIVATE CURRENT SUB OBJECTIVE
    // =============================
    private void ActivateCurrentSubObjective()
    {
        if (currentSubIndex >= runtimeSubObjectives.Count)
            return;

        SubObjective sub = runtimeSubObjectives[currentSubIndex];

        currentMainUI.AddSubObjective(sub);

        sub.onStartEvent?.Invoke();
    }

    // =============================
    // COMPLETE SUB OBJECTIVE
    // =============================
    public void CompleteSubObjective(string subID)
    {
        if (currentMainIndex >= mainObjectives.Count) return;

        if (currentSubIndex >= runtimeSubObjectives.Count) return;

        SubObjective sub = runtimeSubObjectives[currentSubIndex];

        if (sub.id != subID || sub.isCompleted) return;

        StartCoroutine(CompleteSubRoutine(sub));
    }

    private IEnumerator CompleteSubRoutine(SubObjective sub)
    {
        sub.isCompleted = true;

        currentMainUI.CompleteSubObjective(sub.id);

        sub.onCompleteEvent?.Invoke();

        yield return new WaitForSeconds(0.5f);

        currentSubIndex++;

        if (currentSubIndex >= runtimeSubObjectives.Count)
        {
            CompleteMainObjective();
        }
        else
        {
            ActivateCurrentSubObjective();
        }
    }

    // =============================
    // COMPLETE MAIN OBJECTIVE
    // =============================
    public void CompleteMainObjective()
    {
        if (currentMainUI != null)
            currentMainUI.PlayMainCompletion();

        currentMainUI = null;

        currentMainIndex++;

        StartCurrentObjective();
    }

    // =============================
    // COMPLETE BY TRIGGER ID
    // =============================
    public void CompleteObjective(string id)
    {
        if (currentMainIndex >= mainObjectives.Count) return;

        SubObjective sub = runtimeSubObjectives.Find(s => s.id == id);

        if (sub != null)
        {
            CompleteSubObjective(sub.id);
        }
        else
        {
            CompleteMainObjective();
        }
    }

    // =============================
    // SCENE LOADING
    // =============================
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
        var canvas = GameObject.Find("GameUICanvas");

        if (canvas == null)
        {
            Debug.LogError("[ObjectivesManager] GameUICanvas NOT found!");
            return;
        }

        objectivesPanel = canvas.transform.Find("ObjectivesPanel")?.gameObject;

        if (objectivesPanel == null)
        {
            Debug.LogError("[ObjectivesManager] ObjectivesPanel NOT found!");
            return;
        }

        objectivesParent = objectivesPanel.transform;

        Debug.Log("[ObjectivesManager] ObjectivesPanel successfully assigned.");

        StartCurrentObjective();
    }
    public void CompleteCurrentObjective()
    {
        Debug.Log("Objective Completed");
    }
}