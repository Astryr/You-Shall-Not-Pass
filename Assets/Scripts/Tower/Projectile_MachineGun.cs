using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile_MachineGun : MonoBehaviour
{
    private ObjectPoolManager objectPool;
    private TrailRenderer trail;
    
    private Vector3 target;
    private IDamagable damagable;
    private float damage;
    private float speed;
    private float threshold = .01f;
    private bool isActive = true;

    [SerializeField] private GameObject onHitFx;

    private void Awake()
    {
        trail = GetComponent<TrailRenderer>();
    }

    public void SetupProjectile(Vector3 targetPosition,IDamagable newDamagable, float newDamage,float newSpeed,ObjectPoolManager newObjectPool)
    {
        trail.Clear();
        objectPool = newObjectPool;
        isActive = true;

        target = targetPosition;
        damagable = newDamagable;

        damage = newDamage;
        speed = newSpeed;
    }

    private void Update()
    {
        if (isActive == false)
            return;

        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        if ((transform.position - target).sqrMagnitude <= threshold * threshold)
        {
            isActive = false;
            damagable.TakeDamage(damage);

            objectPool.Get(onHitFx,transform.position);
            objectPool.Remove(gameObject);   
        }
    }
}
