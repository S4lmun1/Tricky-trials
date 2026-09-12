using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

// Read-only asset wiring check; also available from the Tools menu.
public static class RewardAssetCheck
{
    [MenuItem("Tools/Tricky Trials/Check Reward Assets")]
    public static void Check()
    {
        var report = new StringBuilder();
        foreach (var guid in AssetDatabase.FindAssets("t:ItemData"))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var item = AssetDatabase.LoadAssetAtPath<ItemData>(path);
            report.AppendLine(path + " => " + (item == null ? "NULL ITEM" : item.itemName + ", pool=" + (item.upgradePool == null ? "NULL" : AssetDatabase.GetAssetPath(item.upgradePool))));
        }
        foreach (var guid in AssetDatabase.FindAssets("t:UpgradePool"))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var pool = AssetDatabase.LoadAssetAtPath<UpgradePool>(path);
            report.AppendLine(path + ": " + (pool == null ? "NULL POOL" : JsonUtility.ToJson(pool)));
            if (pool != null && pool.cards != null) foreach (var card in pool.cards)
                report.AppendLine(card == null ? "  NULL CARD" : "  " + AssetDatabase.GetAssetPath(card) + ": " + JsonUtility.ToJson(card));
        }
        var knife = AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Items/Knife.asset");
        var knifePool = AssetDatabase.LoadAssetAtPath<UpgradePool>("Assets/Upgrades/Knife Pool.asset");
        bool valid = knife != null && knifePool != null && knife.upgradePool == knifePool && knifePool.cards != null && knifePool.cards.Length == 5;
        if (valid)
        {
            var savedRandom = Random.state;
            try
            {
                for (int i = 0; i < 100; i++)
                {
                    var card = knife.upgradePool.Draw(candidate => true);
                    if (card == null || System.Array.IndexOf(knifePool.cards, card) < 0 || card.maximumHealthAdd != 0 || card.healAmount != 0) { valid = false; break; }
                }
            }
            finally { Random.state = savedRandom; }
        }
        report.AppendLine(valid ? "PASS: Unity-loaded knife links to five weapon cards; 100 actual pool draws succeeded without player-health cards." : "FAIL: Knife reward wiring/draw check.");
        Directory.CreateDirectory("Temp");
        File.WriteAllText("Temp/RewardAssetCheck.txt", report.ToString());
        Debug.Log("Reward asset check written to Temp/RewardAssetCheck.txt");
    }
}
