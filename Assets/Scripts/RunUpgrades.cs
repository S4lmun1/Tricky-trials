using System.Collections.Generic;
using UnityEngine;

// Runtime copies only: shared ItemData/Card assets are never changed during play.
public class RunUpgrades : MonoBehaviour
{
    public struct Stats
    {
        public float damage, range, interval, burnDamage, burnInterval, burnDuration;
        public GameObject model;
    }
    readonly Dictionary<ItemData, List<UpgradeCard>> items = new Dictionary<ItemData, List<UpgradeCard>>();
    readonly List<UpgradeCard> playerCards = new List<UpgradeCard>();
    public event System.Action Changed;
    List<UpgradeCard> Cards(ItemData item)
    {
        if (item == null) return playerCards;
        if (!items.TryGetValue(item, out var result)) { result = new List<UpgradeCard>(); items.Add(item, result); }
        return result;
    }
    public bool Eligible(ItemData item, UpgradeCard card)
    {
        return card != null && (card.maxStacks == 0 || Cards(item).FindAll(c => c == card).Count < card.maxStacks);
    }
    public void Apply(ItemData item, UpgradeCard card, PlayerMovement player)
    {
        if (!Eligible(item, card)) return;
        Cards(item).Add(card);
        if (player != null)
        {
            var health = player.GetComponent<DamageableTarget>();
            if (health != null) health.AddHealthBonus(card.maximumHealthAdd, card.healAmount);
            player.movementSpeed += card.movementSpeedAdd;
            player.sprintSpeed += card.sprintSpeedAdd;
            player.jumpForce += card.jumpForceAdd;
        }
        Changed?.Invoke();
    }
    public Stats Evaluate(ItemData item, float damage, float range, float interval)
    {
        var result = new Stats { damage = damage, range = range, interval = interval };
        if (item != null) foreach (var card in Cards(item))
        {
            result.damage = (result.damage + card.damageAdd) * card.damageMultiplier;
            result.range = (result.range + card.rangeAdd) * card.rangeMultiplier;
            result.interval *= card.intervalMultiplier;
            if (card.modelPrefab != null) result.model = card.modelPrefab;
            if (card.burnDamage > 0f)
            {
                result.burnDamage = card.burnDamage;
                result.burnInterval = card.burnInterval;
                result.burnDuration = card.burnDuration;
            }
        }
        result.damage = Mathf.Max(1f, result.damage);
        result.range = Mathf.Max(0.1f, result.range);
        result.interval = Mathf.Max(0.05f, result.interval);
        return result;
    }
    public static void Hit(RaycastHit hit, Stats stats)
    {
        CombatRaycast.Damage(hit, stats.damage);
        var target = hit.collider.GetComponentInParent<DamageableTarget>();
        if (target == null || target.IsDead || stats.burnDamage <= 0f) return;
        var burn = target.GetComponent<BurningStatus>();
        if (burn == null) burn = target.gameObject.AddComponent<BurningStatus>();
        burn.Ignite(stats.burnDamage, stats.burnInterval, stats.burnDuration);
    }
}
