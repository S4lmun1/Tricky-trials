using UnityEngine;

public class Gun : MonoBehaviour, IWeaponAction
{
    public ItemData itemData;
    [Header("Fallback stats (used without Item Data)")]
    public float damage = 25f;
    public float range = 100f;
    [Min(0.01f)] public float attackCooldown = 0.2f;
    public LayerMask hitLayers = ~0;
    public Camera playerCamera;
    public bool logHits;
    float nextAttack;

    public void Use(Camera camera, Transform owner)
    {
        if (Time.time < nextAttack) return;
        if (camera == null) camera = playerCamera;
        if (camera == null) return;
        var upgrades = owner != null ? owner.GetComponent<RunUpgrades>() : null;
        var stats = new RunUpgrades.Stats { damage = itemData != null ? itemData.damage : damage, range = itemData != null ? itemData.attackRange : range, interval = itemData != null ? itemData.attackRate : attackCooldown };
        if (upgrades != null) stats = upgrades.Evaluate(itemData, stats.damage, stats.range, stats.interval);
        nextAttack = Time.time + stats.interval;
        if (CombatRaycast.TryHit(camera, owner, stats.range, hitLayers, out var hit))
        {
            RunUpgrades.Hit(hit, stats);
            if (logHits) Debug.Log("Hit: " + hit.collider.name, hit.collider);
        }
    }
}
