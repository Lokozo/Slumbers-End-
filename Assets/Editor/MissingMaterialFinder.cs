using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class MissingMaterialFinder : EditorWindow
{
    private Vector2 scroll;

    private readonly List<GameObject> missingMaterialObjects = new List<GameObject>();
    private readonly List<GameObject> magentaObjects = new List<GameObject>();

    private Material replacementMaterial;

    [MenuItem("Tools/Material/Missing & Magenta Fixer")]
    public static void ShowWindow()
    {
        GetWindow<MissingMaterialFinder>("Material Fixer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Replacement Material", EditorStyles.boldLabel);
        replacementMaterial = (Material)EditorGUILayout.ObjectField(replacementMaterial, typeof(Material), false);

        GUILayout.Space(10);

        if (GUILayout.Button("Scan Scene"))
        {
            ScanScene();
        }

        if (GUILayout.Button("FIX ALL (Replace Broken Materials)"))
        {
            FixAll();
        }

        GUILayout.Space(10);

        scroll = EditorGUILayout.BeginScrollView(scroll);

        GUILayout.Label("Missing Materials", EditorStyles.boldLabel);
        DrawList(missingMaterialObjects);

        GUILayout.Space(10);

        GUILayout.Label("Magenta / Broken Shaders", EditorStyles.boldLabel);
        DrawList(magentaObjects);

        EditorGUILayout.EndScrollView();
    }

    private void DrawList(List<GameObject> list)
    {
        foreach (var obj in list)
        {
            if (obj == null) continue;

            EditorGUILayout.BeginHorizontal();

            GUILayout.Label(obj.name);

            if (GUILayout.Button("Select"))
            {
                Selection.activeGameObject = obj;
                EditorGUIUtility.PingObject(obj);
            }

            EditorGUILayout.EndHorizontal();
        }
    }

    private void ScanScene()
    {
        missingMaterialObjects.Clear();
        magentaObjects.Clear();

        Renderer[] renderers = FindObjectsOfType<Renderer>();

        foreach (var rend in renderers)
        {
            if (rend == null) continue;

            foreach (var mat in rend.sharedMaterials)
            {
                if (mat == null)
                {
                    missingMaterialObjects.Add(rend.gameObject);
                    break;
                }

                if (IsBroken(mat))
                {
                    magentaObjects.Add(rend.gameObject);
                    break;
                }
            }
        }

        Debug.Log($"Scan Complete: {missingMaterialObjects.Count} missing, {magentaObjects.Count} broken");
    }

    private void FixAll()
    {
        if (replacementMaterial == null)
        {
            Debug.LogWarning("No replacement material assigned!");
            return;
        }

        Renderer[] renderers = FindObjectsOfType<Renderer>();

        int fixedCount = 0;

        foreach (var rend in renderers)
        {
            if (rend == null) continue;

            Material[] mats = rend.sharedMaterials;
            bool changed = false;

            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] == null || IsBroken(mats[i]))
                {
                    mats[i] = replacementMaterial;
                    changed = true;
                    fixedCount++;
                }
            }

            if (changed)
            {
                rend.sharedMaterials = mats;
                EditorUtility.SetDirty(rend);
            }
        }

        Debug.Log($"Fixed {fixedCount} broken materials.");
    }

    private bool IsBroken(Material mat)
    {
        if (mat == null) return true;

        if (mat.shader == null || mat.shader.name == "Hidden/InternalErrorShader")
            return true;

        return false;
    }
}