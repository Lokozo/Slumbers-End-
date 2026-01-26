using UnityEngine;

[System.Serializable]
public class Quest
{
    public string questName;
    public string questDescription;
    public bool isCompleted;

    public void StartQuest()
    {
        Debug.Log($"Quest Started: {questName} - {questDescription}");
    }

    public void CompleteQuest()
    {
        isCompleted = true;
        Debug.Log($"Quest Completed: {questName}");
    }
}
