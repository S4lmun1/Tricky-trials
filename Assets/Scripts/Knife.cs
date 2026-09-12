using UnityEngine;

public class Knife : PickupItem
{
    [Header("Melee (Item Data overrides these fallback stats)")]
    public float damage = 25f, range = 3f, attackCooldown = 0.45f;
    public LayerMask hitLayers = ~0;
    [Header("Swing")]
    [Min(0.01f)] public float swingDuration = 0.2f;
    public Vector3 swingAngles = new Vector3(-65f, 0f, -25f);
    public bool logHits;
    [System.Serializable]
    public class PlaceholderPart
    {
        public string name;
        public Vector3 position, scale;
        public Color color;
    }
    [Header("Temporary Model (used when Model Prefab is empty)")]
    public PlaceholderPart[] placeholderParts = {
        new PlaceholderPart { name = "Handle", position = new Vector3(0f, -0.08f, 0f), scale = new Vector3(0.07f, 0.2f, 0.06f), color = new Color(0.18f, 0.18f, 0.2f) },
        new PlaceholderPart { name = "Guard", scale = new Vector3(0.14f, 0.025f, 0.08f), color = Color.gray },
        new PlaceholderPart { name = "Blade", position = new Vector3(0f, 0.17f, 0f), scale = new Vector3(0.065f, 0.32f, 0.018f), color = new Color(0.8f, 0.85f, 0.9f) }
    };
    float nextAttack, swingUntil;

    protected override void BuildPlaceholder()
    {
        foreach (var part in placeholderParts)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = part.name;
            cube.transform.SetParent(Visual, false);
            cube.transform.localPosition = part.position;
            cube.transform.localScale = part.scale;
            cube.GetComponent<Collider>().enabled = false;
            var properties = new MaterialPropertyBlock();
            properties.SetColor("_BaseColor", part.color);
            properties.SetColor("_Color", part.color);
            cube.GetComponent<Renderer>().SetPropertyBlock(properties);
        }
    }

    RunUpgrades.Stats CurrentStats()
    {
        float d = itemData != null ? itemData.damage : damage;
        float r = itemData != null ? itemData.attackRange : range;
        float i = itemData != null ? itemData.attackRate : attackCooldown;
        return Upgrades != null ? Upgrades.Evaluate(itemData, d, r, i) : new RunUpgrades.Stats { damage = d, range = r, interval = i };
    }
    public override void Use()
    {
        if (!IsHeld || Time.time < nextAttack) return;
        var stats = CurrentStats();
        nextAttack = Time.time + stats.interval;
        swingUntil = Time.time + Mathf.Max(0.01f, swingDuration);
        if (CombatRaycast.TryHit(OwnerCamera, Owner, stats.range, hitLayers, out var hit))
        {
            RunUpgrades.Hit(hit, stats);
            if (logHits) Debug.Log(DisplayName + " hit: " + hit.collider.name, hit.collider);
        }
    }

    void Update()
    {
        SetUpgradeModel(CurrentStats().model);
        if (!IsHeld) return;
        float swing = Mathf.Sin(Mathf.Clamp01((swingUntil - Time.time) / Mathf.Max(0.01f, swingDuration)) * Mathf.PI);
        Visual.localRotation = Quaternion.Euler(swingAngles * swing);
    }
}
