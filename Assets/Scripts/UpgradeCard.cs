using UnityEngine;

[CreateAssetMenu(menuName = "Tricky Trials/Upgrades/Card")]
public class UpgradeCard : ScriptableObject
{
    public string title;
    [TextArea] public string description;
    [Min(0f)] public float weight = 8f;
    [Tooltip("Zero means unlimited selections.")] [Min(0)] public int maxStacks;
    public Color accent = new Color(0.65f, 0.85f, 0.8f);
    [Header("Weapon modifiers (additions, then multipliers)")]
    public float damageAdd, rangeAdd;
    [Min(0.01f)] public float damageMultiplier = 1f;
    [Min(0.01f)] public float intervalMultiplier = 1f;
    [Min(0.01f)] public float rangeMultiplier = 1f;
    [Header("Optional replacement visual; item identity and other upgrades remain")]
    public GameObject modelPrefab;
    [Header("Burn (refreshes on hit; does not stack separate timers)")]
    [Min(0f)] public float burnDamage;
    [Min(0.01f)] public float burnInterval = 0.5f;
    [Min(0f)] public float burnDuration = 2f;
    [Header("Player bonuses")]
    public float maximumHealthAdd, healAmount, movementSpeedAdd, sprintSpeedAdd, jumpForceAdd;
}
