using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance;

    [Header("Required Item")]
    public Item requiredNote;

    [Header("Puzzle States")]
    public bool hasReadNote;
    public bool enteredCorrectArea;
    public bool destroyedTarget;

    [Header("Reward")]
    public GameObject rewardRoom;
    public GameObject rewardChest;

    private void Awake()
    {
        Instance = this;
    }

    public void CheckPuzzleComplete()
    {
        if (hasReadNote &&
            enteredCorrectArea &&
            destroyedTarget)
        {
            Debug.Log("PUZZLE COMPLETE!");

            if (rewardRoom != null)
                rewardRoom.SetActive(true);

            if (rewardChest != null)
                rewardChest.SetActive(true);
        }
    }
}