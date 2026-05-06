using UnityEngine;
using UnityEditor;

public class SmartStaticSetter : EditorWindow
{
    private bool includeInactive = true;
    private float minSizeThreshold = 3f; // minimum size in at least ONE axis
    private bool skipLODGroups = true;
    private bool skipAnimators = true;
    private bool skipRigidbodies = true;

    [MenuItem("Tools/Lighting/Smart Static Setter")]
    public static void ShowWindow()
    {
        GetWindow<SmartStaticSetter>("Smart Static");
    }

    private void OnGUI()
    {
        GUILayout.Label("Smart Static Settings", EditorStyles.boldLabel);

        includeInactive = EditorGUILayout.Toggle("Include Inactive", includeInactive);
        minSizeThreshold = EditorGUILayout.FloatField("Min Size Threshold", minSizeThreshold);
        skipLODGroups = EditorGUILayout.Toggle("Skip LOD Groups", skipLODGroups);
        skipAnimators = EditorGUILayout.Toggle("Skip Animators", skipAnimators);
        skipRigidbodies = EditorGUILayout.Toggle("Skip Rigidbodies", skipRigidbodies);

        GUILayout.Space(10);

        if (GUILayout.Button("Apply Smart Static"))
        {
            ApplySmartStatic();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Clear ALL Static Flags"))
        {
            ClearAllStatic();
        }
    }

    private void ApplySmartStatic()
    {
        GameObject[] objects = FindObjectsOfType<GameObject>(includeInactive);
        int count = 0;

        foreach (var obj in objects)
        {
            if (obj.hideFlags != HideFlags.None)
                continue;

            Renderer renderer = obj.GetComponent<Renderer>();
            MeshFilter mf = obj.GetComponent<MeshFilter>();

            // Must have valid mesh + renderer
            if (renderer == null || mf == null || mf.sharedMesh == null)
                continue;

            // Skip unwanted components
            if (skipRigidbodies && obj.GetComponent<Rigidbody>() != null)
                continue;

            if (skipAnimators && obj.GetComponent<Animator>() != null)
                continue;

            if (skipLODGroups && obj.GetComponent<LODGroup>() != null)
                continue;

            // STRONG size filter (at least one axis must be big enough)
            Vector3 size = renderer.bounds.size;
            if (size.x < minSizeThreshold &&
                size.y < minSizeThreshold &&
                size.z < minSizeThreshold)
                continue;

            // Apply ONLY lighting-related static flag
            GameObjectUtility.SetStaticEditorFlags(obj, StaticEditorFlags.ContributeGI);

            count++;
        }

        Debug.Log($"[SmartStaticSetter] Applied static to {count} objects.");
    }

    private void ClearAllStatic()
    {
        GameObject[] objects = FindObjectsOfType<GameObject>(true);
        int count = 0;

        foreach (var obj in objects)
        {
            if (obj.hideFlags != HideFlags.None)
                continue;

            GameObjectUtility.SetStaticEditorFlags(obj, 0);
            count++;
        }

        Debug.Log($"[SmartStaticSetter] Cleared static flags on {count} objects.");
    }
}