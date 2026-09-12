using UnityEngine;

// Keep gameplay on the root; replace modelPrefab with your final visual model.
public class Knife : MonoBehaviour
{
    public GameObject modelPrefab;
    public Vector3 modelOffset, modelEulerAngles;
    public Vector3 modelScale = Vector3.one;
    public float damage = 25f, range = 2f, attackCooldown = 0.45f;
    public Vector3 heldPosition = new Vector3(0.3f, -0.25f, 0.55f);
    public Vector3 heldEulerAngles = new Vector3(65f, 0f, -15f);
    Rigidbody body;
    Collider pickupCollider;
    Transform visual, owner;
    Camera ownerCamera;
    float nextAttack, swingUntil, pickupAfter;
    public bool IsHeld { get; private set; }
    public bool CanPickup => !IsHeld && Time.time >= pickupAfter;

    void Awake()
    {
        body = gameObject.AddComponent<Rigidbody>();
        body.mass = 0.3f;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        var box = gameObject.AddComponent<BoxCollider>();
        box.center = new Vector3(0f, 0.12f, 0f);
        box.size = new Vector3(0.14f, 0.55f, 0.08f);
        pickupCollider = box;
        visual = new GameObject("Model").transform;
        visual.SetParent(transform, false);
        if (modelPrefab != null)
        {
            var model = Instantiate(modelPrefab, visual);
            model.transform.localPosition = modelOffset;
            model.transform.localRotation = Quaternion.Euler(modelEulerAngles);
            model.transform.localScale = modelScale;
            foreach (var collider in model.GetComponentsInChildren<Collider>()) collider.enabled = false;
            foreach (var rb in model.GetComponentsInChildren<Rigidbody>())
            {
                rb.isKinematic = true;
                rb.detectCollisions = false;
            }
        }
        else
        {
            AddPart("Handle", new Vector3(0f, -0.08f, 0f), new Vector3(0.07f, 0.2f, 0.06f), new Color(0.18f, 0.18f, 0.2f));
            AddPart("Guard", Vector3.zero, new Vector3(0.14f, 0.025f, 0.08f), Color.gray);
            AddPart("Blade", new Vector3(0f, 0.17f, 0f), new Vector3(0.065f, 0.32f, 0.018f), new Color(0.8f, 0.85f, 0.9f));
        }
    }

    void AddPart(string partName, Vector3 position, Vector3 scale, Color color)
    {
        var part = GameObject.CreatePrimitive(PrimitiveType.Cube);
        part.name = partName;
        part.transform.SetParent(visual, false);
        part.transform.localPosition = position;
        part.transform.localScale = scale;
        part.GetComponent<Collider>().enabled = false;
        var properties = new MaterialPropertyBlock();
        properties.SetColor("_BaseColor", color);
        properties.SetColor("_Color", color);
        part.GetComponent<Renderer>().SetPropertyBlock(properties);
    }

    public void Equip(Camera camera, Transform player)
    {
        IsHeld = true;
        ownerCamera = camera;
        owner = player;
        body.isKinematic = true;
        pickupCollider.enabled = false;
        transform.SetParent(camera.transform, false);
        transform.localPosition = heldPosition;
        transform.localRotation = Quaternion.Euler(heldEulerAngles);
        gameObject.SetActive(true);
    }

    public void Drop(Vector3 position, Vector3 velocity)
    {
        transform.SetParent(null, true);
        transform.position = position;
        IsHeld = false;
        ownerCamera = null;
        owner = null;
        gameObject.SetActive(true);
        pickupCollider.enabled = true;
        body.isKinematic = false;
        body.linearVelocity = velocity;
        body.angularVelocity = Vector3.right * 3f;
        pickupAfter = Time.time + 0.4f;
    }

    void Update()
    {
        if (!IsHeld || ownerCamera == null) return;
        if (Cursor.lockState == CursorLockMode.Locked && Input.GetMouseButtonDown(0) && Time.time >= nextAttack)
        {
            nextAttack = Time.time + attackCooldown;
            swingUntil = Time.time + 0.2f;
            RaycastHit? closest = null;
            foreach (var hit in Physics.RaycastAll(ownerCamera.transform.position, ownerCamera.transform.forward, range, ~0, QueryTriggerInteraction.Ignore))
            {
                if (hit.transform.IsChildOf(owner)) continue;
                if (!closest.HasValue || hit.distance < closest.Value.distance) closest = hit;
            }
            if (closest.HasValue)
            {
                var hit = closest.Value;
                foreach (var behaviour in hit.collider.GetComponentsInParent<MonoBehaviour>())
                    if (behaviour is IDamageable target) { target.TakeDamage(damage); break; }
                Debug.Log("Knife hit: " + hit.collider.name, hit.collider);
            }
        }
        float swing = Mathf.Sin(Mathf.Clamp01((swingUntil - Time.time) / 0.2f) * Mathf.PI);
        visual.localRotation = Quaternion.Euler(-65f * swing, 0f, -25f * swing);
    }
}
