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

    private void Awake()
    {
        Instance = this;
    }
}