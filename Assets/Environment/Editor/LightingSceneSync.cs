using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections.Generic;

public class LightingSceneSync : EditorWindow
{
    SceneAsset sourceScene;
    SceneAsset targetScene;

    [MenuItem("Tools/Lighting/LightingSceneSync")]
    static void Open()
    {
        GetWindow<LightingSceneSync>("Lighting Scene Sync");
    }

    void OnGUI()
    {
        sourceScene = (SceneAsset)EditorGUILayout.ObjectField("Source Scene", sourceScene, typeof(SceneAsset), false);
        targetScene = (SceneAsset)EditorGUILayout.ObjectField("Target Scene", targetScene, typeof(SceneAsset), false);

        if (GUILayout.Button("SYNC LIGHTING"))
            Sync();
    }

    void Sync()
    {
        if (sourceScene == null || targetScene == null)
        {
            Debug.LogError("Assign both scenes.");
            return;
        }

        string srcPath = AssetDatabase.GetAssetPath(sourceScene);
        string dstPath = AssetDatabase.GetAssetPath(targetScene);

        // =========================
        // LOAD SOURCE (SNAPSHOT)
        // =========================
        Scene src = EditorSceneManager.OpenScene(srcPath, OpenSceneMode.Single);

        if (!src.IsValid())
        {
            Debug.LogError("Source scene invalid.");
            return;
        }

        GameObject[] srcRoots = src.GetRootGameObjects();

        // =========================
        // SNAPSHOT RENDER SETTINGS (LIGHTING WINDOW CORE)
        // =========================
        var skybox = RenderSettings.skybox;
        var fog = RenderSettings.fog;
        var fogDensity = RenderSettings.fogDensity;
        var fogColor = RenderSettings.fogColor;
        var fogMode = RenderSettings.fogMode;

        var ambientLight = RenderSettings.ambientLight;
        var ambientMode = RenderSettings.ambientMode;
        var ambientIntensity = RenderSettings.ambientIntensity;
        var sun = RenderSettings.sun;

        // SAFE LIGHTING SETTINGS (NO CRASH)
        var lightingSettings = Lightmapping.lightingSettings;

        // =========================
        // SNAPSHOT LIGHTS
        // =========================
        var lightData = Object.FindObjectsOfType<Light>()
            .Select(l => new
            {
                l.type,
                l.color,
                l.intensity,
                l.range,
                l.spotAngle,
                l.bounceIntensity,
                l.shadows,
                l.shadowStrength,
                pos = l.transform.position,
                rot = l.transform.rotation
            })
            .ToList();

        // =========================
        // SNAPSHOT REFLECTION PROBES
        // =========================
        var probeData = Object.FindObjectsOfType<ReflectionProbe>()
            .Select(p => new
            {
                p.size,
                p.boxProjection,
                p.mode,
                p.resolution,
                p.refreshMode,
                p.timeSlicingMode,
                p.intensity,
                p.bakedTexture,
                p.customBakedTexture,
                pos = p.transform.position,
                rot = p.transform.rotation
            })
            .ToList();

        // =========================
        // SNAPSHOT POST PROCESS VOLUMES (FIXED PROPERLY)
        // =========================
        var volumeData = Object.FindObjectsOfType<UnityEngine.Rendering.Volume>()
            .Select(v => new
            {
                v.isGlobal,
                v.priority,
                v.blendDistance,
                v.weight,
                v.sharedProfile,
                pos = v.transform.position,
                rot = v.transform.rotation,
                layer = v.gameObject.layer
            })
            .ToList();

        // =========================
        // LOAD TARGET
        // =========================
        EditorSceneManager.CloseScene(src, true);
        Scene dst = EditorSceneManager.OpenScene(dstPath, OpenSceneMode.Single);

        if (!dst.IsValid())
        {
            Debug.LogError("Target scene invalid.");
            return;
        }

        // =========================
        // APPLY LIGHTING WINDOW SETTINGS
        // =========================
        RenderSettings.skybox = skybox;
        RenderSettings.fog = fog;
        RenderSettings.fogDensity = fogDensity;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogMode = fogMode;

        RenderSettings.ambientLight = ambientLight;
        RenderSettings.ambientMode = ambientMode;
        RenderSettings.ambientIntensity = ambientIntensity;
        RenderSettings.sun = sun;

        if (lightingSettings != null)
            Lightmapping.lightingSettings = lightingSettings;

        // =========================
        // CLEAN TARGET (ONLY LIGHTING SYSTEMS)
        // =========================
        foreach (var l in Object.FindObjectsOfType<Light>())
            DestroyImmediate(l.gameObject);

        foreach (var p in Object.FindObjectsOfType<ReflectionProbe>())
            DestroyImmediate(p.gameObject);

        foreach (var v in Object.FindObjectsOfType<UnityEngine.Rendering.Volume>())
            DestroyImmediate(v.gameObject);

        // =========================
        // REBUILD LIGHTS
        // =========================
        foreach (var l in lightData)
        {
            var go = new GameObject($"Light_{l.type}");
            SceneManager.MoveGameObjectToScene(go, dst);

            var nl = go.AddComponent<Light>();

            nl.type = l.type;
            nl.color = l.color;
            nl.intensity = l.intensity;
            nl.range = l.range;
            nl.spotAngle = l.spotAngle;
            nl.bounceIntensity = l.bounceIntensity;
            nl.shadows = l.shadows;
            nl.shadowStrength = l.shadowStrength;

            go.transform.position = l.pos;
            go.transform.rotation = l.rot;
        }

        // =========================
        // REBUILD REFLECTION PROBES
        // =========================
        foreach (var p in probeData)
        {
            var go = new GameObject("ReflectionProbe");
            SceneManager.MoveGameObjectToScene(go, dst);

            var np = go.AddComponent<ReflectionProbe>();

            np.size = p.size;
            np.boxProjection = p.boxProjection;
            np.mode = p.mode;
            np.resolution = p.resolution;
            np.refreshMode = p.refreshMode;
            np.timeSlicingMode = p.timeSlicingMode;
            np.intensity = p.intensity;
            np.bakedTexture = p.bakedTexture;
            np.customBakedTexture = p.customBakedTexture;

            go.transform.position = p.pos;
            go.transform.rotation = p.rot;
        }

        // =========================
        // REBUILD POST PROCESS VOLUMES (FIXED)
        // =========================
        foreach (var v in volumeData)
        {
            var go = new GameObject("PostProcess Volume");
            SceneManager.MoveGameObjectToScene(go, dst);

            var nv = go.AddComponent<UnityEngine.Rendering.Volume>();

            nv.isGlobal = v.isGlobal;
            nv.priority = v.priority;
            nv.blendDistance = v.blendDistance;
            nv.weight = v.weight;
            nv.sharedProfile = v.sharedProfile;

            go.transform.position = v.pos;
            go.transform.rotation = v.rot;
            go.layer = v.layer;
        }

        // =========================
        // SAVE
        // =========================
        EditorSceneManager.MarkSceneDirty(dst);
        EditorSceneManager.SaveScene(dst);

        Debug.Log("LIGHTING SYNC COMPLETE (FIXED + FULL POST PROCESS RESTORE)");
    }
}