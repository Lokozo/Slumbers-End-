using UnityEngine;
using TMPro;

public class DeveloperMode : MonoBehaviour
{
    public static DeveloperMode Instance;

    [Header("Developer Mode")]
    public bool developerModeEnabled = false;

    [Header("UI")]
    public TMP_Text developerModeText;

    [Header("Unlimited Stats")]
    public bool unlimitedHealth = true;
    public bool unlimitedHunger = true;
    public bool unlimitedEnergy = true;

    [Header("Crafting")]
    public bool unlockAllRecipes = true;
    public bool ignoreCraftingRequirements = true;

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

    private void Start()
    {
        UpdateDeveloperUI();
    }

    private void Update()
    {
        // TOGGLE DEV MODE
        if (Input.GetKeyDown(KeyCode.F12))
        {
            developerModeEnabled = !developerModeEnabled;

            Debug.Log("Developer Mode: " + developerModeEnabled);

            UpdateDeveloperUI();
        }
    }

    void UpdateDeveloperUI()
    {
        if (developerModeText == null)
            return;

        developerModeText.gameObject.SetActive(developerModeEnabled);

        developerModeText.text =
            "DEVELOPER MODE ENABLED";
    }
}