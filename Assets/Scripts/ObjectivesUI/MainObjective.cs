using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Objectives/Main Objective")]
public class MainObjective : ScriptableObject
{
    public string id;
    public string title;
    [TextArea]
    public string description;

    public List<SubObjective> subObjectives; // Can be empty
}
