using UnityEngine;
using UnityEngine.Events;
using System;

[Serializable]
public class SubObjective
{
    public string id;

    [TextArea]
    public string description;

    [HideInInspector]
    public bool isCompleted = false;

    // Events when sub objective begins
    public UnityEvent onStartEvent;

    // Events when sub objective completes
    public UnityEvent onCompleteEvent;
}
