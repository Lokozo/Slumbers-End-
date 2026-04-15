using UnityEngine;

public class StairExitTrigger : MonoBehaviour
{
    public Transform exitPoint;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        GoToStairs goToStairs = other.GetComponent<GoToStairs>();
        if (goToStairs == null) return;

        if (goToStairs.IsMovingByStairs) return;

        goToStairs.GoToExitPoint(exitPoint);
    }
}