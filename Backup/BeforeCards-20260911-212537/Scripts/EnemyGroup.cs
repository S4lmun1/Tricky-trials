using System.Collections.Generic;
using UnityEngine;

// Put encounter enemies under Enemy Root; additional references can live elsewhere.
public class EnemyGroup : MonoBehaviour
{
    public Transform enemyRoot;
    public DamageableTarget[] additionalEnemies;
    [Min(0.05f)] public float refreshInterval = 0.25f;
    readonly HashSet<DamageableTarget> members = new HashSet<DamageableTarget>();
    float nextRefresh;
    void Awake() { RefreshMembers(); }
    void Update()
    {
        if (Time.time >= nextRefresh) RefreshMembers();
    }
    public void Register(DamageableTarget enemy) { if (enemy != null) members.Add(enemy); }
    public void RefreshMembers()
    {
        nextRefresh = Time.time + Mathf.Max(0.05f, refreshInterval);
        if (enemyRoot != null)
            foreach (var enemy in enemyRoot.GetComponentsInChildren<ChaseEnemy>(true)) Register(enemy.GetComponent<DamageableTarget>());
        if (additionalEnemies != null) foreach (var enemy in additionalEnemies) Register(enemy);
    }
    public int Remaining
    {
        get
        {
            int alive = 0;
            foreach (var enemy in members) if (enemy != null && !enemy.IsDead) alive++;
            return alive;
        }
    }
    public bool IsCleared => Remaining == 0;
}
