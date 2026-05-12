using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile_Cannon : MonoBehaviour
{
    private TrailRenderer trail;
    private ObjectPoolManager objectPool;
    private Rigidbody rb;
    private float damage;

    [SerializeField] private float damageRadius;
    [SerializeField] private LayerMask whatIsEnemy;
    [SerializeField] private GameObject explosionFx;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        trail = GetComponent<TrailRenderer>();
    }

    public void SetupProjectile(Vector3 newVelocity,float newDamage,ObjectPoolManager newPool)
    {
        trail.Clear();
        objectPool = newPool;
        rb.linearVelocity = newVelocity;
        damage = newDamage;
    }

    private void DamageEnemiesAround()
    {
        Collider[] enemiesAround = Physics.OverlapSphere(transform.position, damageRadius, whatIsEnemy);

        foreach (Collider enemy in enemiesAround)
        {
            IDamagable damagable = enemy.GetComponent<IDamagable>();

            if (damagable != null)
            {
                damagable.TakeDamage(damage);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        DamageEnemiesAround();

        objectPool.Get(explosionFx, transform.position + new Vector3(0, .5f, 0));
        objectPool.Remove(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, damageRadius);  
    }
}
