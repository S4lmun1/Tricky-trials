using UnityEngine;

// Shared pickup/equip/drop behavior. Subclass and override Use for a new item action.
[DisallowMultipleComponent]
public class PickupItem : MonoBehaviour, IWeaponAction
{
    [Header("Item")]
    public ItemData itemData;
    public string fallbackName = "Item";
    [Header("Visual Model")]
    public GameObject modelPrefab;
    public Vector3 modelOffset, modelEulerAngles;
    public Vector3 modelScale = Vector3.one;
    [Header("Held Pose")]
    public Vector3 heldPosition = new Vector3(0.3f, -0.25f, 0.55f);
    public Vector3 heldEulerAngles;
    [Header("World Physics")]
    [Min(0.01f)] public float mass = 0.3f;
    public Vector3 colliderCenter = new Vector3(0f, 0.12f, 0f);
    public Vector3 colliderSize = new Vector3(0.14f, 0.55f, 0.08f);
    [Min(0f)] public float pickupDelay = 0.4f;
    public Vector3 dropSpin = new Vector3(3f, 0f, 0f);

    protected Transform Visual { get; private set; }
    protected Transform Owner { get; private set; }
    protected Camera OwnerCamera { get; private set; }
    Rigidbody body;
    Collider[] colliders;
    bool[] colliderStates;
    float pickupAfter;
    public bool IsHeld { get; private set; }
    public bool CanPickup => !IsHeld && Time.time >= pickupAfter;
    public string DisplayName => itemData != null && !string.IsNullOrWhiteSpace(itemData.itemName) ? itemData.itemName : fallbackName;

    protected virtual void Awake()
    {
        body = GetComponent<Rigidbody>();
        if (body == null) body = gameObject.AddComponent<Rigidbody>();
        body.mass = Mathf.Max(0.01f, mass);
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        if (GetComponent<Collider>() == null)
        {
            var box = gameObject.AddComponent<BoxCollider>();
            box.center = colliderCenter;
            box.size = colliderSize;
        }
        colliders = GetComponents<Collider>();
        colliderStates = new bool[colliders.Length];
        for (int i = 0; i < colliders.Length; i++) colliderStates[i] = colliders[i].enabled;
        Visual = new GameObject("Model").transform;
        Visual.SetParent(transform, false);
        if (modelPrefab != null)
        {
            var model = Instantiate(modelPrefab, Visual);
            model.transform.localPosition = modelOffset;
            model.transform.localRotation = Quaternion.Euler(modelEulerAngles);
            model.transform.localScale = modelScale;
            ModelUtilities.MakeVisualOnly(model);
        }
        else BuildPlaceholder();
    }

    protected virtual void BuildPlaceholder() { }
    public virtual void Use()
    {
        // A generic pickup can share its root with a Gun or another action component.
        foreach (var component in GetComponents<MonoBehaviour>())
            if (component != this && component is IWeaponAction action) { action.Use(OwnerCamera, Owner); return; }
    }
    public void Use(Camera camera, Transform owner) { Use(); }

    public void Equip(Camera camera, Transform player, Transform socket = null)
    {
        OwnerCamera = camera;
        Owner = player;
        IsHeld = true;
        body.isKinematic = true;
        body.detectCollisions = false;
        foreach (var collider in colliders) collider.enabled = false;
        transform.SetParent(socket != null ? socket : camera.transform, false);
        transform.localPosition = heldPosition;
        transform.localRotation = Quaternion.Euler(heldEulerAngles);
        gameObject.SetActive(true);
    }

    public void Drop(Vector3 position, Vector3 velocity)
    {
        transform.SetParent(null, true);
        transform.position = position;
        IsHeld = false;
        OwnerCamera = null;
        Owner = null;
        Visual.localRotation = Quaternion.identity;
        gameObject.SetActive(true);
        for (int i = 0; i < colliders.Length; i++) colliders[i].enabled = colliderStates[i];
        body.detectCollisions = true;
        body.isKinematic = false;
        body.linearVelocity = velocity;
        body.angularVelocity = dropSpin;
        pickupAfter = Time.time + Mathf.Max(0f, pickupDelay);
    }
}
