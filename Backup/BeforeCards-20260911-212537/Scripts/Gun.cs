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
        nextAttack = Time.time + Mathf.Max(0.01f, itemData != null ? itemData.attackRate : attackCooldown);
        if (CombatRaycast.TryHit(camera, owner, itemData != null ? itemData.attackRange : range, hitLayers, out var hit))
        {
            CombatRaycast.Damage(hit, itemData != null ? itemData.damage : damage);
            if (logHits) Debug.Log("Hit: " + hit.collider.name, hit.collider);
        }
    }
}
