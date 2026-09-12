using System.Collections;
using UnityEngine;

[RequireComponent(typeof(DamageableTarget), typeof(PlayerMovement))]
public class PlayerRespawn : MonoBehaviour
{
    public Transform respawnPoint;
    [Min(0f)] public float respawnDelay = 1.5f;
    public bool respawnOnDeath = true;
    DamageableTarget health;
    PlayerMovement movement;
    Rigidbody body;
    Vector3 initialPosition;
    void Awake()
    {
        health = GetComponent<DamageableTarget>();
        movement = GetComponent<PlayerMovement>();
        body = GetComponent<Rigidbody>();
        initialPosition = transform.position;
        health.onDeath.AddListener(OnDeath);
    }
    void OnDestroy() { if (health != null) health.onDeath.RemoveListener(OnDeath); }
    void OnDeath()
    {
        movement.SetControlsEnabled(false);
        if (respawnOnDeath) StartCoroutine(Respawn());
    }
    IEnumerator Respawn()
    {
        yield return new WaitForSeconds(respawnDelay);
        Vector3 position = respawnPoint != null ? respawnPoint.position : initialPosition;
        if (respawnPoint != null) movement.TeleportTo(respawnPoint);
        else body.position = position;
        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        health.RestoreFullHealth();
        movement.SetControlsEnabled(true);
    }
}
