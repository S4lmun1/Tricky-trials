using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Tricky Trials/Upgrades/Pool")]
public class UpgradePool : ScriptableObject
{
    public UpgradeCard[] cards;
    public UpgradeCard Draw(System.Func<UpgradeCard, bool> eligible)
    {
        var unique = new HashSet<UpgradeCard>();
        float total = 0f;
        if (cards != null) foreach (var card in cards)
            if (card != null && card.weight > 0f && eligible(card) && unique.Add(card)) total += card.weight;
        float roll = Random.value * total;
        UpgradeCard last = null;
        foreach (var card in unique) { last = card; roll -= card.weight; if (roll < 0f) return card; }
        return last;
    }
}
