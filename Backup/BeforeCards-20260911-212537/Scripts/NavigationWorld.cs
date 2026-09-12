using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Assign only static world geometry. Rebuild after changing the corridor at runtime.
[DefaultExecutionOrder(-100)]
public class NavigationWorld : MonoBehaviour
{
    public Collider[] geometry;
    [Min(0.05f)] public float agentRadius = 0.45f;
    [Min(0.1f)] public float agentHeight = 2.2f;
    [Min(0f)] public float stepHeight = 0.3f;
    [Range(0f, 60f)] public float maximumSlope = 45f;
    [Min(0.1f)] public float boundsPadding = 3f;
    NavMeshData data;
    NavMeshDataInstance instance;
    void Awake() { Rebuild(); }
    [ContextMenu("Rebuild Navigation (Play Mode)")]
    public void Rebuild()
    {
        if (!Application.isPlaying) return;
        if (instance.valid) instance.Remove();
        if (data != null) Destroy(data);
        var sources = new List<NavMeshBuildSource>();
        var bounds = new Bounds(transform.position, Vector3.zero);
        foreach (var collider in geometry)
        {
            if (collider == null || !collider.enabled || collider.isTrigger) continue;
            if (collider is BoxCollider box)
            {
                sources.Add(new NavMeshBuildSource {
                    shape = NavMeshBuildSourceShape.Box,
                    transform = box.transform.localToWorldMatrix * Matrix4x4.Translate(box.center),
                    size = box.size, area = 0
                });
                bounds.Encapsulate(box.bounds);
            }
            else if (collider is MeshCollider mesh && mesh.sharedMesh != null)
            {
                sources.Add(new NavMeshBuildSource {
                    shape = NavMeshBuildSourceShape.Mesh, sourceObject = mesh.sharedMesh,
                    transform = mesh.transform.localToWorldMatrix, area = 0
                });
                bounds.Encapsulate(mesh.bounds);
            }
        }
        if (sources.Count == 0 || NavMesh.GetSettingsCount() == 0) return;
        var settings = NavMesh.GetSettingsByIndex(0);
        settings.agentRadius = agentRadius;
        settings.agentHeight = agentHeight;
        settings.agentClimb = stepHeight;
        settings.agentSlope = maximumSlope;
        bounds.Expand(boundsPadding * 2f);
        data = NavMeshBuilder.BuildNavMeshData(settings, sources, bounds, Vector3.zero, Quaternion.identity);
        if (data != null) instance = NavMesh.AddNavMeshData(data);
    }
    void OnDestroy()
    {
        if (instance.valid) instance.Remove();
        if (data != null) Destroy(data);
    }
}
