using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;
using UnityEngine.AI;

public class Projectile_SpiderNest : MonoBehaviour
{
    private TrailRenderer trail;
    private ObjectPoolManager objectPool;
    private NavMeshAgent agent;
    private Transform currentTarget;


    [SerializeField] private float damage;
    [SerializeField] private float damageRadius = .8f;
    [SerializeField] private float detonateDistance = .5f;
    [SerializeField] private GameObject explosionFx;
    [Space]
    [SerializeField] private LayerMask whatIsEnemy;
    [SerializeField] private float enemyCheckRadius = 10;
    [SerializeField] private float targetUpdateInterval = .5f;

    private void Awake()
    {
        trail = GetComponent<TrailRenderer>();
        agent = GetComponent<NavMeshAgent>();
        objectPool = ObjectPoolManager.instance;

        InvokeRepeating(nameof(UpdateClosestTarget), .1f, targetUpdateInterval);
    }

    private void Update()
    {
        if (currentTarget == null || agent.enabled == false || agent.isOnNavMesh == false)
            return;

        agent.SetDestination(currentTarget.position);

        if (Vector3.Distance(transform.position, currentTarget.position) < detonateDistance)
            Explode();
    }

    private void Explode()
    {
        DamageEnemiesAround();

        objectPool.Get(explosionFx, transform.position + new Vector3(0,.4f,0));
        objectPool.Remove(gameObject);
    }

    public void SetupSpider(float newDamage)
    {
        trail.Clear();
        damage = newDamage;
        agent.enabled = true;
        transform.parent = null;
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

    private void UpdateClosestTarget()
    {
        currentTarget = FindClosestEnemy();
    }

    private Transform FindClosestEnemy()
    {
        Collider[] enemiesAround = Physics.OverlapSphere(transform.position, enemyCheckRadius, whatIsEnemy);
        Transform nearestEnemy = null; float shortestDistance = float.MaxValue;

        foreach (Collider enemyCollider in enemiesAround)
        {
            float distance = Vector3.Distance(transform.position, enemyCollider.transform.position);

            if (distance < shortestDistance)
            {
                nearestEnemy = enemyCollider.transform;
                shortestDistance = distance;
            }
        }

        return nearestEnemy;
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }
}
