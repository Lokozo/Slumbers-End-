using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public GameObject inventoryMenu;
    public GameObject resourcePanel;
    public Transform resourceContentPanel;
    public GameObject magnifyingGlassIcon;
    public GameObject checkIcon;
    public TMPro.TextMeshProUGUI lootableNameText;
    public GameObject pushPullIcon;
    public GameObject breakableIcon;
    public GameObject dialoguePanel;
    public TMPro.TextMeshProUGUI speakerNameText;
    public UnityEngine.UI.Image portraitImage;
    public TMPro.TextMeshProUGUI dialogueText;
    public GameObject lockIcon;
    public GameObject ladderHoldEIcon;
    //public GameObject blackOverlay;


    private void Awake()
    {
        Instance = this;
    }
}