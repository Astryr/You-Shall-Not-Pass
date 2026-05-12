using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower_Harpoon : Tower
{
    private Harpoon_Visuals harpoonVisuals;

    [Header("Harpoon Details")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileDefaultPosition;
    [SerializeField] private float projectileSpeed = 15;
    private Projectile_Harpoon currentProjectile;

    [Header("Damage Details")]
    [SerializeField] private float initialDamage = 5;
    [SerializeField] private float damageOverTime = 10;
    [SerializeField] private float overTimeEffectDuration = 4;
    [Range(0f, 1f)]
    [SerializeField] private float slowEffect = .7f;


    private bool reachedTarget;
    private bool busyWithAttack;
    private Coroutine damageOverTimeCo;

    protected override void Awake()
    {
        base.Awake();
        currentProjectile = GetComponentInChildren<Projectile_Harpoon>();
        harpoonVisuals = GetComponent<Harpoon_Visuals>();
    }

    protected override void Attack()
    {
        base.Attack();

        if (Physics.Raycast(gunPoint.position, gunPoint.forward, out RaycastHit hitInfo, Mathf.Infinity, whatIsTargetable))
        {
            busyWithAttack = true;
            currentEnemy = hitInfo.collider.GetComponent<Enemy>();
            currentProjectile.SetupProjectile(currentEnemy, projectileSpeed,this);
            harpoonVisuals.EnableChainVisuals(true, currentProjectile.GetConnectionPoint());

            Invoke(nameof(ResetAttackIfMissed), 1);
        }
    }

    public void ActivateAttack()
    {
        reachedTarget = true;
        currentEnemy.GetComponent<Enemy_Flying>().AddObservingTower(this);
        currentEnemy.SlowEnemy(slowEffect, overTimeEffectDuration);
        harpoonVisuals.CreateElectrifyVFX(currentEnemy.transform);

        IDamagable damagable = currentEnemy.GetComponent<IDamagable>();
        damagable?.TakeDamage(initialDamage);

        damageOverTimeCo = StartCoroutine(DamageOverTimeCo(damagable));
    }

    private IEnumerator DamageOverTimeCo(IDamagable damagable)
    {
        float time = 0;
        float damageFrequency = overTimeEffectDuration / damageOverTime;
        float damagePerTick = damageOverTime / (overTimeEffectDuration / damageFrequency);

        while (time < overTimeEffectDuration)
        {
            damagable?.TakeDamage(damagePerTick);
            yield return new WaitForSeconds(damageFrequency);
            time += damageFrequency;
        }

        ResetAttack();
    }


    public void ResetAttack()
    {
        // Si la torre está siendo destruida, evitar llamar rutinas de MonoBehaviour
        if (this == null || !gameObject.activeInHierarchy)
            return;

        if(damageOverTimeCo != null)
            StopCoroutine(damageOverTimeCo);

        busyWithAttack = false;
        reachedTarget = false;

        currentEnemy = null;
        lastTimeAttacked = Time.time;
        harpoonVisuals.EnableChainVisuals(false);
        CreateNewProjectile();
    }

    private void CreateNewProjectile()
    {
        GameObject newProjectile = 
            objectPool.Get(projectilePrefab,projectileDefaultPosition.position,projectileDefaultPosition.rotation,towerHead);
        
        currentProjectile = newProjectile.GetComponent<Projectile_Harpoon>();
    }

    private void ResetAttackIfMissed()
    {
        if (reachedTarget)
            return;

        objectPool.Remove(currentProjectile.gameObject);
        ResetAttack();
    }
    

    protected override bool CanAttack()
    {
        return base.CanAttack() && busyWithAttack == false;
    }

    protected override void LooseTargetIfNeeded()
    {
        if(busyWithAttack == false)
            base.LooseTargetIfNeeded();
    }
}
