using UnityEngine;

public class StairTrigger : MonoBehaviour
{
    public enum TriggerType { Bottom, Top }
    public TriggerType triggerType;

    private Stairs stairs;

    void Awake()
    {
        stairs = GetComponentInParent<Stairs>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        GoToStairs goToStairs = other.GetComponent<GoToStairs>();
        if (goToStairs == null) return;

        if (goToStairs.IsMovingByStairs) return;

        float v = Input.GetAxisRaw("Vertical");

        if (triggerType == TriggerType.Bottom && v > 0.5f)
            goToStairs.GoToPointFromBottom(stairs.bottomClimbPoint);

        if (triggerType == TriggerType.Top && v < -0.5f)
            goToStairs.GoToPointFromTop(stairs.topClimbPoint);
    }
}