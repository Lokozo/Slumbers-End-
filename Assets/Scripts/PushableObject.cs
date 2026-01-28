using UnityEngine;

public class PushableObject : MonoBehaviour
{
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void SetPhysics(bool active)
    {
        if (rb == null) return;

        // When NOT active (grabbed), kinematic is TRUE so it follows the player perfectly
        rb.isKinematic = !active;

        if (active)
        {
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }
        else
        {
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }
    }
}