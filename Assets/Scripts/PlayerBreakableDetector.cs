using UnityEngine;

public class PlayerBreakableDetector : MonoBehaviour
{
    public float detectRange = 2f;
    public LayerMask breakableLayer;

    private GameObject breakableIcon;

    public BreakableObject currentBreakable;

    private void Start()
    {
        breakableIcon = UIManager.Instance.breakableIcon;
    }

    void Update()
    {
        if (breakableIcon == null) return;

        Vector3 origin = transform.position + Vector3.up * 0.5f;

        if (Physics.Raycast(origin, transform.forward, out RaycastHit hit, detectRange, breakableLayer))
        {
            if (hit.collider.GetComponent<BreakableObject>() != null)
            {
                breakableIcon.SetActive(true);

                TutorialUIManager.Instance?.ShowStep(
                    "breakableTutorial",
                    "This is a breakable object." +
                    "\nEquip your axe (Press 2)" +
                    "\nand attack it (Left Click) to break it."
                );

                return;
            }
        }
        breakableIcon.SetActive(false);
        //TutorialUIManager.Instance?.Hide();
    }
}