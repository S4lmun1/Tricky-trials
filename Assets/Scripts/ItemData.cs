using UnityEngine;

[CreateAssetMenu(
    fileName = "New Item",
    menuName = "Tricky Trials/Item"
)]
public class ItemData : ScriptableObject
{
    [Header("Item Info")]
    public string itemName;
    public UpgradePool upgradePool;

    [Header("UI")]
    public Sprite icon;

    [Header("Held Object")]
    public GameObject heldPrefab;

    [Header("Weapon Stats")]
    [Min(0)] public int damage = 25;
    [Min(0f)] public float attackRange = 2.0f;
    [Tooltip("Seconds between attacks.")]
    [Min(0.01f)] public float attackRate = 0.5f;
}
