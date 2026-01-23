using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    private int currentQuestIndex = 0;

    [SerializeField] private Quest[] quests;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        StartQuest(0); // Start first quest at game start
    }

    public void StartQuest(int index)
    {
        if (index < 0 || index >= quests.Length) return;

        currentQuestIndex = index;
        quests[index].StartQuest();
    }

    public void CompleteQuest()
    {
        quests[currentQuestIndex].CompleteQuest();

        // Start next quest automatically if there is one
        if (currentQuestIndex + 1 < quests.Length)
        {
            StartQuest(currentQuestIndex + 1);
        }
        else
        {
            Debug.Log("All quests completed!");
        }
    }

    public Quest GetCurrentQuest()
    {
        return quests[currentQuestIndex];
    }
}
