using UnityEngine;

public class PlayerVisuals : MonoBehaviour
{
    public PlayerMovement movement;
    public Transform visualRoot;
    public Renderer placeholderRenderer;
    [Header("Replaceable Character Model")]
    public GameObject modelPrefab;
    public Vector3 modelPosition;
    public Vector3 modelEulerAngles;
    public Vector3 modelScale = Vector3.one;
    public bool followViewYaw = true;
    GameObject instance;

    void Start()
    {
        if (movement == null) movement = GetComponent<PlayerMovement>();
        if (visualRoot == null)
        {
            visualRoot = new GameObject("Visual Model").transform;
            visualRoot.SetParent(transform, false);
        }
        ApplyModel();
    }

    // Can also be called by a character selection or customization system.
    public void SetModel(GameObject prefab) { modelPrefab = prefab; ApplyModel(); }

    [ContextMenu("Apply Model (Play Mode)")]
    public void ApplyModel()
    {
        if (!Application.isPlaying || visualRoot == null) return;
        if (instance != null) { instance.SetActive(false); Destroy(instance); }
        if (placeholderRenderer != null) placeholderRenderer.enabled = modelPrefab == null;
        if (modelPrefab == null) return;
        instance = Instantiate(modelPrefab, visualRoot);
        instance.transform.localPosition = modelPosition;
        instance.transform.localRotation = Quaternion.Euler(modelEulerAngles);
        instance.transform.localScale = modelScale;
        ModelUtilities.MakeVisualOnly(instance);
    }

    void LateUpdate()
    {
        if (followViewYaw && visualRoot != null && movement != null)
            visualRoot.rotation = movement.ViewYawRotation;
    }
}
