using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter))]
public class WorldTileUVs : MonoBehaviour
{
    public float tilesPerUnit = 1f;

    Mesh instanceMesh;

    void OnEnable()
    {
        EnsureMeshInstance();
        RecalculateUVs();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (!enabled) return;
        EnsureMeshInstance();
        RecalculateUVs();
    }

    void OnDestroy()
    {
        if (!Application.isPlaying && instanceMesh != null)
        {
            DestroyImmediate(instanceMesh);
        }
    }
#endif

    void EnsureMeshInstance()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        if (!mf || mf.sharedMesh == null) return;

        if (instanceMesh != null) return;

        instanceMesh = Instantiate(mf.sharedMesh);
        instanceMesh.name = mf.sharedMesh.name + "_WorldUV_Instance";
        mf.sharedMesh = instanceMesh;
    }

    void RecalculateUVs()
    {
        if (instanceMesh == null) return;

        Vector3[] verts = instanceMesh.vertices;
        Vector3[] normals = instanceMesh.normals;
        Vector2[] uvs = new Vector2[verts.Length];

        for (int i = 0; i < verts.Length; i++)
        {
            Vector3 worldPos = transform.TransformPoint(verts[i]);

            Vector3 n = normals[i];
            n = new Vector3(Mathf.Abs(n.x), Mathf.Abs(n.y), Mathf.Abs(n.z));

            if (n.y > n.x && n.y > n.z)
            {
                // Top and bottom faces use XZ
                uvs[i] = new Vector2(worldPos.x, worldPos.z) * tilesPerUnit;
            }
            else if (n.x > n.z)
            {
                // Left and right faces use ZY
                uvs[i] = new Vector2(worldPos.z, worldPos.y) * tilesPerUnit;
            }
            else
            {
                // Front and back faces use XY
                uvs[i] = new Vector2(worldPos.x, worldPos.y) * tilesPerUnit;
            }
        }

        instanceMesh.uv = uvs;
    }
}
