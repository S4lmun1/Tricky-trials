using UnityEngine;
using UnityEngine.Events;

public class DamageableTarget : MonoBehaviour, IDamageable
{
    [Min(0f)] public float health = 100f;
    public bool destroyOnDeath = true;
    public UnityEvent<float> onDamaged = new UnityEvent<float>();
    public UnityEvent onDeath = new UnityEvent();
    bool dead;
    float startingHealth;
    public bool IsDead => dead;
    public float MaxHealth => startingHealth;
    public float HealthFraction => startingHealth > 0f ? Mathf.Clamp01(health / startingHealth) : 0f;
    void Awake() { startingHealth = health; }
    public void RestoreFullHealth() { health = startingHealth; dead = false; }
    public void AddHealthBonus(float maximum, float heal)
    {
        startingHealth = Mathf.Max(1f, startingHealth + maximum);
        if (!dead) health = Mathf.Clamp(health + maximum + heal, 0f, startingHealth);
    }
    public void TakeDamage(float damage)
    {
        if (dead || damage <= 0f) return;
        health = Mathf.Max(0f, health - damage);
        onDamaged.Invoke(damage);
        if (health > 0f) return;
        dead = true;
        onDeath.Invoke();
        if (destroyOnDeath) Destroy(gameObject);
    }
}
