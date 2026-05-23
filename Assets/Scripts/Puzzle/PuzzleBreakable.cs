using UnityEngine;

public class PuzzleBreakable : MonoBehaviour
{
    public void PuzzleDestroyed()
    {
        PuzzleManager.Instance.destroyedTarget = true;

        PuzzleManager.Instance.CheckPuzzleComplete();
    }
}