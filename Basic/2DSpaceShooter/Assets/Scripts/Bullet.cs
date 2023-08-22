using CodeMonkey.HealthSystem.Scripts;
using System;
using Unity.Netcode;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    public const int BULLET_DAMAGE = 5;
    bool m_Bounce;
    int m_Damage = BULLET_DAMAGE;
    ShipControl m_Owner;

    public GameObject explosionParticle;

    public void Config(ShipControl owner, int damage, bool bounce, float lifetime)
    {
        m_Owner = owner;
        m_Damage = damage;
        m_Bounce = bounce;
        if (IsServer)
        {
            // This is bad code don't use invoke.
            Invoke(nameof(DestroyBullet), lifetime);
        }
    }

    public override void OnNetworkDespawn()
    {
        // This is inefficient, the explosion object could be pooled.
        GameObject explodeParticles = Instantiate(explosionParticle, transform.position + new Vector3(0, 0, -2), Quaternion.identity);
    }

    private void DestroyBullet()
    {
        if (!NetworkObject.IsSpawned)
        {
            return;
        }

        NetworkObject.Despawn(true);
    }

    public void SetVelocity(Vector2 velocity)
    {
        if (IsServer)
        {
            var bulletRb = GetComponent<Rigidbody2D>();
            bulletRb.velocity = velocity;
            SetVelocityClientRpc(velocity);
        }
    }

    [ClientRpc]
    void SetVelocityClientRpc(Vector2 velocity)
    {
        if (!IsHost)
        {
            var bulletRb = GetComponent<Rigidbody2D>();
            bulletRb.velocity = velocity;
        }
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        GameObject otherObject = other.gameObject;

        if (!NetworkManager.Singleton.IsServer || !NetworkObject.IsSpawned)
        {
            return;
        }

        if (otherObject.TryGetComponent<IDamageable>(out var damageableObj)) {
            if (otherObject.TryGetComponent(out ShipControl shipControl)) {
                if (shipControl == m_Owner) return;
            }
            damageableObj.Damage(m_Damage);
            DestroyBullet();
            return;
        }

        if (m_Bounce == false && otherObject.TryGetComponent(out Bullet _) == false) {
            DestroyBullet();
        }
    }
}
