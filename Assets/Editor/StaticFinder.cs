using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

public class StaticObjectCounter : EditorWindow
{
    private int totalStatic;
    private int giStatic;
    private int navigationStatic;
    private int occluderStatic;
    private int batchingStatic;

    [MenuItem("Tools/Scene/Count Static Objects")]
    public static void ShowWindow()
    {
        GetWindow<StaticObjectCounter>("Static Counter");
    }

    private void OnGUI()
    {
        GUILayout.Label("Static Object Counter", EditorStyles.boldLabel);

        if (GUILayout.Button("COUNT STATIC OBJECTS"))
        {
            CountStatics();
        }

        GUILayout.Space(10);

        GUILayout.Label($"Total Static: {totalStatic}");
        GUILayout.Label($"Contribute GI: {giStatic}");
        GUILayout.Label($"Navigation Static: {navigationStatic}");
        GUILayout.Label($"Occluder Static: {occluderStatic}");
        GUILayout.Label($"Batching Static: {batchingStatic}");
    }

    private void CountStatics()
    {
        totalStatic = 0;
        giStatic = 0;
        navigationStatic = 0;
        occluderStatic = 0;
        batchingStatic = 0;

        GameObject[] objects = SceneManager.GetActiveScene().GetRootGameObjects();

        foreach (GameObject root in objects)
        {
            Traverse(root);
        }

        Debug.Log(
            $"[StaticCounter] Total: {totalStatic}, GI: {giStatic}, Nav: {navigationStatic}, Occluder: {occluderStatic}, Batching: {batchingStatic}"
        );
    }

    private void Traverse(GameObject obj)
    {
        if (!obj) return;

        StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(obj);

        if (flags != 0)
        {
            totalStatic++;

            if ((flags & StaticEditorFlags.ContributeGI) != 0)
                giStatic++;

            if ((flags & StaticEditorFlags.NavigationStatic) != 0)
                navigationStatic++;

            if ((flags & StaticEditorFlags.OccludeeStatic) != 0)
                occluderStatic++;

            if ((flags & StaticEditorFlags.BatchingStatic) != 0)
                batchingStatic++;
        }

        foreach (Transform child in obj.transform)
        {
            Traverse(child.gameObject);
        }
    }
}