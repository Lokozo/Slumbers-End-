using UnityEngine;

public class LockableObject : MonoBehaviour
{
    public enum LockType
    {
        None,
        Key,
        Lockpick,
        Puzzle
    }

    [Header("Lock Settings")]
    public LockType lockType = LockType.None;

    public Item requiredKey;
    public Item lockpickItem;

    [Header("Puzzle")]
    public GameObject puzzleUI;

    public bool isLocked = true;

    public bool TryUnlock()
    {
        if (!isLocked) return true;

        switch (lockType)
        {
            case LockType.Key:
                return TryKeyUnlock();

            case LockType.Lockpick:
                return TryLockpickUnlock();

            case LockType.Puzzle:
                OpenPuzzle();
                return false;

            default:
                return true;
        }
    }

    private bool TryKeyUnlock()
    {
        if (PlayerInventory.Instance.HasItem(requiredKey, 1))
        {
            Debug.Log("Unlocked with key");
            PlayerInventory.Instance.RemoveItem(requiredKey, 1);
            isLocked = false;
            return true;
        }

        Debug.Log("Need a key!");
        return false;
    }

    private bool TryLockpickUnlock()
    {
        if (PlayerInventory.Instance.HasItem(lockpickItem, 1))
        {
            Debug.Log("Used lockpick");
            PlayerInventory.Instance.RemoveItem(lockpickItem, 1);

            // Optional: success chance
            if (Random.value > 0.3f)
            {
                isLocked = false;
                Debug.Log("Lockpick success!");
                return true;
            }
            else
            {
                Debug.Log("Lockpick failed!");
                return false;
            }
        }

        Debug.Log("No lockpick!");
        return false;
    }

    private void OpenPuzzle()
    {
        if (puzzleUI != null)
        {
            puzzleUI.SetActive(true);
            Debug.Log("Opening puzzle UI");
        }
    }

    public void UnlockFromPuzzle()
    {
        isLocked = false;
        Debug.Log("Unlocked via puzzle!");
    }
}