using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class PortalGate : MonoBehaviour
{
    [Header("Travel")]
    public PlayerMovement player;
    public Transform destination;
    public PortalGate arrivalPortal;
    public bool arrivalOnly;
    public bool sealAfterDeparture = true;
    public bool sealAfterArrival = true;
    public bool updateCheckpoint = true;
    [Min(0f)] public float reentryDelay = 1f;
    [Header("Unlock Requirements")]
    public bool requireEncounterClear = true;
    public EnemyGroup encounter;
    [Header("Completion Reward (after arrival)")]
    public bool grantCompletionReward = true;
    public LevelRewards rewards;
    public Collider blocker;
    public GameObject[] lockedIndicators;
    [Header("White Glow")]
    public Renderer[] glowingSurfaces;
    [ColorUsage(false, true)] public Color glowColor = Color.white;
    [Min(0f)] public float lockedEmission = 2f;
    [Min(0f)] public float unlockedEmission = 10f;
    [Min(0f)] public float pulseAmount = 0.5f;
    [Min(0f)] public float pulseSpeed = 2f;
    public Vector3 lightOffset = new Vector3(0f, 2f, 0.8f);
    [Min(0f)] public float lightRange = 7f;
    [Min(0f)] public float lightIntensity = 3f;
    bool sealedGate;
    float nextTravel;
    Light glow;
    MaterialPropertyBlock properties;
    public bool IsUnlocked => !sealedGate && !arrivalOnly && (!requireEncounterClear || encounter != null && encounter.IsCleared);

    void Awake()
    {
        GetComponent<BoxCollider>().isTrigger = true;
        properties = new MaterialPropertyBlock();
        if (rewards == null) rewards = FindFirstObjectByType<LevelRewards>();
        var lightObject = new GameObject("Portal Glow");
        lightObject.transform.SetParent(transform, false);
        lightObject.transform.localPosition = lightOffset;
        glow = lightObject.AddComponent<Light>();
        glow.type = LightType.Point;
        glow.shadows = LightShadows.None;
    }
    void Update()
    {
        bool open = IsUnlocked;
        if (blocker != null) blocker.enabled = !open;
        if (lockedIndicators != null)
            foreach (var indicator in lockedIndicators) if (indicator != null) indicator.SetActive(sealedGate || requireEncounterClear && !open);
        float emission = (open || arrivalOnly && !sealedGate ? unlockedEmission : lockedEmission) + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        if (glowingSurfaces != null)
            foreach (var surface in glowingSurfaces)
                if (surface != null)
                {
                    surface.GetPropertyBlock(properties);
                    properties.SetColor("_EmissionColor", glowColor * Mathf.Max(0f, emission));
                    surface.SetPropertyBlock(properties);
                }
        glow.color = glowColor;
        glow.range = lightRange;
        glow.intensity = lightIntensity;
    }
    void OnTriggerEnter(Collider other) { TryTravel(other); }
    void OnTriggerStay(Collider other) { TryTravel(other); }
    void TryTravel(Collider other)
    {
        if (player == null || destination == null || other.GetComponentInParent<PlayerMovement>() != player || !player.ControlsEnabled || Time.time < nextTravel) return;
        // Include any enemies added immediately before this trigger update.
        if (encounter != null) encounter.RefreshMembers();
        if (!IsUnlocked) return;
        nextTravel = Time.time + reentryDelay;
        if (sealAfterDeparture) sealedGate = true;
        if (arrivalPortal != null) arrivalPortal.ReceiveArrival();
        player.TeleportTo(destination);
        if (updateCheckpoint && player.TryGetComponent<PlayerRespawn>(out var respawn)) respawn.respawnPoint = destination;
        // Travel and checkpoint update happen before the reward UI is queued.
        if (grantCompletionReward && rewards != null) rewards.OnPortalArrived(encounter, player);
    }
    public void ReceiveArrival()
    {
        nextTravel = Time.time + reentryDelay;
        if (sealAfterArrival) sealedGate = true;
    }
}
