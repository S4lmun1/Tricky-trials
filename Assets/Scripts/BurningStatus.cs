using UnityEngine;

// One refreshable burn per target. Four 0.5-second ticks with the default card.
[RequireComponent(typeof(DamageableTarget))]
public class BurningStatus : MonoBehaviour
{
    DamageableTarget target;
    float damage, interval, expires, nextTick;
    public void Ignite(float amount, float tickInterval, float duration)
    {
        target = GetComponent<DamageableTarget>();
        bool active = Time.time <= expires && damage > 0f;
        damage = amount;
        interval = Mathf.Max(0.01f, tickInterval);
        expires = Time.time + duration;
        if (!active) nextTick = Time.time + interval;
    }
    void Update()
    {
        if (target == null || target.IsDead || damage <= 0f) return;
        while (nextTick <= expires + 0.0001f && Time.time >= nextTick)
        {
            nextTick += interval;
            target.TakeDamage(damage);
            if (target.IsDead) return;
        }
        if (Time.time > expires) damage = 0f;
    }
}
