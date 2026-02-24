using UnityEngine;

public class Ladder : MonoBehaviour
{
    public Transform LadderModel;
    public Transform climbPoint;
    public Transform topExit;
    public Transform bottomExit;

    // Direction the PLAYER should face
    public Transform faceDirection;

    public Vector3 FaceDirection
    {
        get
        {
            return faceDirection != null
                ? faceDirection.forward
                : transform.forward;
        }
    }

    
}