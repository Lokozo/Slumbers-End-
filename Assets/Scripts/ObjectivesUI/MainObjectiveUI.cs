using UnityEngine;

public class MainObjectiveUI : MonoBehaviour
{
    public TMPro.TextMeshProUGUI titleText;
    public TMPro.TextMeshProUGUI descriptionText;
    public Transform subContainer;    // Assign empty GameObject under main UI
    public GameObject subPrefab;      // Assign SubObjectiveUI prefab

    public void Setup(MainObjective main)
    {
        titleText.text = main.title;

        if (descriptionText != null)
            descriptionText.text = main.description;
    }

    // Called by manager to spawn next sub
    public void AddSubObjective(SubObjective sub)
    {
        if (subPrefab == null || subContainer == null) return;

        GameObject subObj = Instantiate(subPrefab, subContainer);
        subObj.GetComponent<SubObjectiveUI>().Setup(sub);
    }

    // Called by manager to mark a sub complete
    public void CompleteSubObjective(string subID)
    {
        foreach (SubObjectiveUI ui in subContainer.GetComponentsInChildren<SubObjectiveUI>())
        {
            if (ui.ID == subID)
                ui.MarkComplete();
        }
    }

    // Called when all subs completed
    public void PlayMainCompletion()
    {
        //Play some animation or effect to indicate main objective completion
    }
}
