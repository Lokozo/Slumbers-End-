using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using System;
using System.Collections.Generic;

public class SceneDataFixer : EditorWindow
{
    private float minSize = 3f;
    private bool includeInactive = true;

    private List<GameObject> tempDisabledObjects = new List<GameObject>();

    [MenuItem("Tools/Scene/Fix Scene (SAFE + TRACKED)")]
    public static void ShowWindow()
    {
        GetWindow<SceneDataFixer>("Scene Fixer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Scene Fixer (NO DELETE MODE)", EditorStyles.boldLabel);

        minSize = EditorGUILayout.FloatField("Min Size for GI Static", minSize);
        includeInactive = EditorGUILayout.Toggle("Include Inactive", includeInactive);

        GUILayout.Space(10);

        if (GUILayout.Button("BACKUP + FIX SCENE"))
        {
            BackupScene();
            FixScene();
        }

        if (GUILayout.Button("RESTORE DISABLED OBJECTS"))
        {
            RestoreObjects();
        }
    }

    // ================= BACKUP =================
    private void BackupScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();

        if (string.IsNullOrEmpty(currentScene.path))
        {
            Debug.LogError("Scene must be saved first.");
            return;
        }

        string folder = "Assets/SceneBackups";

        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        string sceneName = Path.GetFileNameWithoutExtension(currentScene.path);
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        string backupPath = $"{folder}/{sceneName}_BACKUP_{timestamp}.unity";

        if (EditorSceneManager.SaveScene(currentScene, backupPath, true))
            Debug.Log($"[SceneFixer] Backup created: {backupPath}");
        else
            Debug.LogError("[SceneFixer] Backup FAILED");

        AssetDatabase.Refresh();
    }

    // ================= MAIN FIX =================
    private void FixScene()
    {
        GameObject[] objs = FindObjectsOfType<GameObject>(includeInactive);

        int checkedCount = 0;
        int disabledCount = 0;
        int giAssigned = 0;
        int decorationCount = 0;

        tempDisabledObjects.Clear();

        foreach (var obj in objs)
        {
            if (!obj) continue; // prevents MissingReferenceException

            checkedCount++;

            if (obj.hideFlags != HideFlags.None)
                continue;

            string path = GetFullPath(obj);

            // 🔥 SKIP PARTICLE SYSTEMS (your fire fix)
            if (obj.GetComponent<ParticleSystem>() != null)
            {
                Debug.Log($"[SKIP PARTICLE] {path}");
                continue;
            }

            MeshFilter mf = obj.GetComponent<MeshFilter>();
            Renderer r = obj.GetComponent<Renderer>();

            bool hasRenderer = r != null;
            bool hasMesh = mf != null && mf.sharedMesh != null;

            // ⚠️ "BROKEN" → DISABLE instead of delete
            if (mf != null && mf.sharedMesh == null)
            {
                obj.SetActive(false);

                if (!tempDisabledObjects.Contains(obj))
                    tempDisabledObjects.Add(obj);

                disabledCount++;

                Debug.Log($"[DISABLED BROKEN MESH] {path}");
                continue;
            }

            if (!hasRenderer || !hasMesh)
                continue;

            Vector3 size = r.bounds.size;

            bool isLarge =
                size.x >= minSize ||
                size.y >= minSize ||
                size.z >= minSize;

            if (isLarge)
            {
                GameObjectUtility.SetStaticEditorFlags(obj,
                    StaticEditorFlags.ContributeGI);

                giAssigned++;

                Debug.Log($"[GI STATIC] {path}");
            }
            else
            {
                GameObjectUtility.SetStaticEditorFlags(obj, 0);

                decorationCount++;

                Debug.Log($"[DECORATION - NO GI] {path}");
            }
        }

        Debug.Log(
            $"[SceneFixer SUMMARY]\n" +
            $"Checked: {checkedCount}\n" +
            $"Disabled (safe): {disabledCount}\n" +
            $"GI Assigned: {giAssigned}\n" +
            $"Decorations: {decorationCount}"
        );
    }

    // ================= RESTORE =================
    private void RestoreObjects()
    {
        int restored = 0;

        foreach (var obj in tempDisabledObjects)
        {
            if (!obj) continue;

            obj.SetActive(true);
            restored++;
        }

        Debug.Log($"[SceneFixer] Restored objects: {restored}");

        tempDisabledObjects.Clear();
    }

    // ================= HELPER =================
    private string GetFullPath(GameObject obj)
    {
        string path = obj.name;
        Transform current = obj.transform;

        while (current.parent != null)
        {
            current = current.parent;
            path = current.name + "/" + path;
        }

        return path;
    }
}