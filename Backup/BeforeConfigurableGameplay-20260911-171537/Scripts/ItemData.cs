using UnityEngine;

[CreateAssetMenu(
    fileName = "New Item",
    menuName = "Tricky Trials/Item"
)]
public class ItemData : ScriptableObject
{
    [Header("Item Info")]
    public string itemName;

    [Header("UI")]
    public Sprite icon;

    [Header("Held Object")]
    public GameObject heldPrefab;

    [Header("Weapon Stats")]
    public int damage = 25;
    public float attackRange = 2.0f;
    public float attackRate = 0.5f;
}
