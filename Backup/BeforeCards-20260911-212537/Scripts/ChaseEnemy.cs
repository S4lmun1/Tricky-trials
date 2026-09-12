using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(DamageableTarget))]
public class ChaseEnemy : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    public DamageableTarget targetHealth;
    [Min(0f)] public float detectionRange = 10f;
    public bool keepChasingAfterDetection = true;
    [Header("Movement")]
    [Min(0f)] public float moveSpeed = 3f;
    [Min(0f)] public float acceleration = 12f;
    [Min(0f)] public float turnSpeed = 360f;
    [Min(0.05f)] public float radius = 0.45f;
    [Min(0.1f)] public float height = 2.2f;
    [Min(0f)] public float stoppingDistance = 0f;
    [Min(0.05f)] public float pathRefreshInterval = 0.2f;
    [Min(0.1f)] public float navigationSnapDistance = 2f;
    [Header("Contact Damage")]
    [Min(0f)] public float attackDamage = 20f;
    [Min(0.01f)] public float attackInterval = 1f;
    [Header("Replaceable Model")]
    public Transform visualRoot;
    public Renderer placeholderRenderer;
    public GameObject modelPrefab;
    public Vector3 modelOffset, modelEulerAngles;
    public Vector3 modelScale = Vector3.one;
    NavMeshAgent agent;
    DamageableTarget health;
    bool alerted;
    float nextPath, nextAttack;
    readonly HashSet<Collider> touchingTarget = new HashSet<Collider>();

    void Start()
    {
        health = GetComponent<DamageableTarget>();
        if (modelPrefab != null && visualRoot != null)
        {
            if (placeholderRenderer != null) placeholderRenderer.enabled = false;
            var model = Instantiate(modelPrefab, visualRoot);
            model.transform.localPosition = modelOffset;
            model.transform.localRotation = Quaternion.Euler(modelEulerAngles);
            model.transform.localScale = modelScale;
            ModelUtilities.MakeVisualOnly(model);
        }
        if (targetHealth == null && target != null) targetHealth = target.GetComponent<DamageableTarget>();
        var body = GetComponent<Rigidbody>();
        if (body == null) body = gameObject.AddComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;
        agent = GetComponent<NavMeshAgent>();
        if (agent == null) agent = gameObject.AddComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
        agent.acceleration = acceleration;
        agent.angularSpeed = turnSpeed;
        agent.radius = radius;
        agent.height = height;
        agent.stoppingDistance = Mathf.Max(0f, stoppingDistance);
        if (NavMesh.SamplePosition(transform.position, out var hit, navigationSnapDistance, NavMesh.AllAreas)) agent.Warp(hit.position);
        if (!agent.isOnNavMesh) Debug.LogWarning("Enemy is outside the navigation area. Check Navigation World geometry and spawn position.", this);
    }

    void Update()
    {
        if (agent == null || !agent.isOnNavMesh) return;
        if (health.IsDead || target == null || targetHealth == null || targetHealth.IsDead)
        {
            agent.isStopped = true;
            return;
        }
        float distance = Vector3.Distance(transform.position, target.position);
        if (distance <= detectionRange) alerted = true;
        else if (!keepChasingAfterDetection) alerted = false;
        touchingTarget.RemoveWhere(collider => collider == null || !collider.enabled || !collider.gameObject.activeInHierarchy);
        agent.isStopped = !alerted || touchingTarget.Count > 0;
        if (!alerted) return;
        if (Time.time >= nextPath)
        {
            nextPath = Time.time + Mathf.Max(0.05f, pathRefreshInterval);
            agent.SetDestination(target.position);
        }
        // Only a physics contact can cause damage; distance alone never attacks.
        if (touchingTarget.Count == 0 || Time.time < nextAttack) return;
        nextAttack = Time.time + Mathf.Max(0.01f, attackInterval);
        targetHealth.TakeDamage(attackDamage);
    }

    void OnCollisionEnter(Collision collision) { TrackContact(collision.collider); }
    void OnCollisionStay(Collision collision) { TrackContact(collision.collider); }
    void OnCollisionExit(Collision collision) { touchingTarget.Remove(collision.collider); }
    void OnDisable() { touchingTarget.Clear(); }
    void TrackContact(Collider other)
    {
        if (targetHealth != null && other.GetComponentInParent<DamageableTarget>() == targetHealth)
        {
            touchingTarget.Add(other);
            alerted = true;
        }
    }
}