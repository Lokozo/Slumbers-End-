using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectivesUIController : MonoBehaviour
{
    public GameObject objectivesPanel; // drag your panel here

    private bool isOpen = false;

    void Update()
    {
        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            ToggleObjectives();
        }
    }

    public void ToggleObjectives()
    {
        isOpen = !isOpen;
        objectivesPanel.SetActive(isOpen);

        // Optional: pause player movement later
        Time.timeScale = isOpen ? 0f : 1f;
    }
}