using UnityEngine;

public class Ladder : MonoBehaviour
{
    public Transform topExit;
    public Transform bottomExit;

    [Tooltip("The direction the model faces while climbing (usually Vector3.forward)")]
    public Vector3 ladderFaceDirection = Vector3.forward;
}