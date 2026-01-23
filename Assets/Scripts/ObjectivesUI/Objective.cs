using UnityEngine;

[System.Serializable]
public class Objective
{
    public string id;
    public string title;
    [TextArea]
    public string description;
    public bool isCompleted;
}
