using UnityEngine;

public class GeneratorQuest : MonoBehaviour
{
    [Header("Quest Settings")]
    public Item generatorItem;  // Assign your "Generator" item ScriptableObject here
    public int requiredAmount = 1;

    [Header("Quest State")]
    public bool questStarted = false;
    public bool questCompleted = false;

    [Header("Debug")]
    [TextArea]
    public string currentObjective = "Find Gus to start the generator quest.";

    private void Start()
    {
        Debug.Log("[Quest] Waiting for player to talk to Gus...");
    }

    // Call when player interacts with Gus to start quest
    public void StartQuest()
    {
        if (questStarted) return;

        questStarted = true;
        currentObjective = $"Bring {requiredAmount}x {generatorItem.itemName} to Gus.";
        Debug.Log($"[Quest] Started: {currentObjective}");
    }

    // Call when player talks to Gus again to try completing
    public void TryCompleteQuest()
    {
        if (!questStarted || questCompleted)
            return;

        if (PlayerInventory.Instance.HasItem(generatorItem, requiredAmount))
        {
            PlayerInventory.Instance.RemoveItem(generatorItem, requiredAmount);

            questCompleted = true;
            currentObjective = "Quest Complete! Generator has been delivered.";
            Debug.Log("[Quest] Completed! Generator delivered to Gus.");
        }
        else
        {
            int playerHas = PlayerInventory.Instance.GetItemQuantity(generatorItem);
            Debug.Log($"[Quest] You still need {requiredAmount - playerHas} more {generatorItem.itemName}.");
        }
    }
}
