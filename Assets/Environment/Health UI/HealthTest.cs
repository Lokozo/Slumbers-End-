using UnityEngine;

public class HealthTest : MonoBehaviour
{
    public PlayerHealth playerHealth;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            Debug.Log("Pressed H: Dealing 10 damage");
            playerHealth.TakeDamage(10);
        }
    }
}
