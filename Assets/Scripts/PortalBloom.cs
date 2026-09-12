using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Bloom only: no fog, color grading, exposure, or sky changes.
public class PortalBloom : MonoBehaviour
{
    public Camera playerCamera;
    [Min(0f)] public float intensity = 0.35f;
    [Min(0f)] public float threshold = 1.2f;
    [Range(0f, 1f)] public float scatter = 0.5f;
    public float priority = 100f;
    VolumeProfile profile;
    void Start()
    {
        if (playerCamera != null) playerCamera.GetUniversalAdditionalCameraData().renderPostProcessing = true;
        var volume = gameObject.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.priority = priority;
        profile = ScriptableObject.CreateInstance<VolumeProfile>();
        var bloom = profile.Add<Bloom>(true);
        bloom.intensity.Override(intensity);
        bloom.threshold.Override(threshold);
        bloom.scatter.Override(scatter);
        volume.sharedProfile = profile;
    }
    void OnDestroy() { if (profile != null) Destroy(profile); }
}
