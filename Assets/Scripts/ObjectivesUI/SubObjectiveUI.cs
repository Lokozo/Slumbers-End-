using UnityEngine;
using TMPro;

public class SubObjectiveUI : MonoBehaviour
{
    public TextMeshProUGUI descriptionText;

    public string ID { get; private set; }

    public void Setup(SubObjective sub)
    {
        ID = sub.id;
        descriptionText.text = sub.description;
    }

    public void MarkComplete()
    {
        descriptionText.text = "<s>" + descriptionText.text + "</s>";
    }
}
