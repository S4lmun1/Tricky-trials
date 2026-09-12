using UnityEngine;

public static class CombatRaycast
{
    public static bool TryHit(Camera camera, Transform owner, float range, LayerMask layers, out RaycastHit nearest)
    {
        nearest = default;
        if (camera == null) return false;
        bool found = false;
        foreach (var hit in Physics.RaycastAll(camera.transform.position, camera.transform.forward, Mathf.Max(0f, range), layers, QueryTriggerInteraction.Ignore))
        {
            if (owner != null && hit.transform.IsChildOf(owner)) continue;
            if (!found || hit.distance < nearest.distance) { nearest = hit; found = true; }
        }
        return found;
    }

    public static void Damage(RaycastHit hit, float amount)
    {
        foreach (var component in hit.collider.GetComponentsInParent<MonoBehaviour>())
            if (component is IDamageable target) { target.TakeDamage(amount); return; }
    }
}
