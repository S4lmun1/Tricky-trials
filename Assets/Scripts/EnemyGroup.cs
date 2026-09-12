using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Children and additional references form one level encounter.
public class EnemyGroup : MonoBehaviour
{
    public Transform enemyRoot;
    public DamageableTarget[] additionalEnemies;
    [Min(0.05f)] public float refreshInterval = 0.25f;
    public UnityEvent onCleared = new UnityEvent();
    public bool RewardPending { get; set; }
    public bool Completed { get; private set; }
    readonly HashSet<DamageableTarget> members = new HashSet<DamageableTarget>();
    float nextRefresh;
    bool hadEnemies;
    void Awake() { RefreshMembers(); }
    void Update() { if (Time.time >= nextRefresh) { RefreshMembers(); CheckCleared(); } }
    public void Register(DamageableTarget enemy)
    {
        if (enemy == null || !members.Add(enemy)) return;
        hadEnemies = true;
        enemy.onDeath.AddListener(CheckCleared);
    }
    public void RefreshMembers()
    {
        nextRefresh = Time.time + Mathf.Max(0.05f, refreshInterval);
        var root = enemyRoot != null ? enemyRoot : transform;
        foreach (var enemy in root.GetComponentsInChildren<ChaseEnemy>(true)) Register(enemy.GetComponent<DamageableTarget>());
        if (additionalEnemies != null) foreach (var enemy in additionalEnemies) Register(enemy);
    }
    void CheckCleared()
    {
        if (Completed || !hadEnemies) return;
        RefreshMembers();
        if (!IsCleared) return;
        Completed = true;
        onCleared.Invoke();
    }
    void OnDestroy()
    {
        foreach (var enemy in members) if (enemy != null) enemy.onDeath.RemoveListener(CheckCleared);
    }
    public int Remaining
    {
        get { int alive = 0; foreach (var enemy in members) if (enemy != null && !enemy.IsDead) alive++; return alive; }
    }
    public bool IsCleared => Remaining == 0;
}
