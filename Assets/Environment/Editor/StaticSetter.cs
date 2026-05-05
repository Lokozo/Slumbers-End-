using UnityEngine;
using UnityEditor;

public class AutoStaticSetter : EditorWindow
{
    private bool includeChildren = true;
    private bool skipIfHasAnimator = true;
    private bool skipIfHasRigidbody = true;

    [MenuItem("Tools/Lighting/Auto Static Setter")]
    public static void ShowWindow()
    {
        GetWindow<AutoStaticSetter>("Auto Static Setter");
    }

    private void OnGUI()
    {
        GUILayout.Label("Auto Static Settings", EditorStyles.boldLabel);

        includeChildren = EditorGUILayout.Toggle("Include Children", includeChildren);
        skipIfHasRigidbody = EditorGUILayout.Toggle("Skip if Has Rigidbody", skipIfHasRigidbody);
        skipIfHasAnimator = EditorGUILayout.Toggle("Skip if Has Animator", skipIfHasAnimator);

        GUILayout.Space(10);

        if (GUILayout.Button("SET STATIC (Scene)"))
        {
            SetStaticObjects();
        }

        if (GUILayout.Button("UNSET STATIC (Scene)"))
        {
            UnsetStaticObjects();
        }
    }

    private void SetStaticObjects()
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        int count = 0;

        foreach (var obj in allObjects)
        {
            if (!includeChildren && obj.transform.parent != null)
                continue;

            if (skipIfHasRigidbody && obj.GetComponent<Rigidbody>() != null)
                continue;

            if (skipIfHasAnimator && obj.GetComponent<Animator>() != null)
                continue;

            // Skip editor-only objects
            if (obj.hideFlags != HideFlags.None)
                continue;

            // Basic rule: must have renderer or collider to matter for lighting
            if (obj.GetComponent<Renderer>() == null && obj.GetComponent<Collider>() == null)
                continue;

            GameObjectUtility.SetStaticEditorFlags(obj,
                StaticEditorFlags.ContributeGI |
                StaticEditorFlags.OccludeeStatic |
                StaticEditorFlags.BatchingStatic);

            count++;
        }

        Debug.Log($"Marked {count} objects as static.");
    }

    private void UnsetStaticObjects()
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        int count = 0;

        foreach (var obj in allObjects)
        {
            if (!includeChildren && obj.transform.parent != null)
                continue;

            GameObjectUtility.SetStaticEditorFlags(obj, 0);
            count++;
        }

        Debug.Log($"Cleared static flags on {count} objects.");
    }
}