using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class Tower_MachineGun : Tower
{
    private MachineGun_Visuals machineGunVisuals;

    [Header("Machine Gun Details")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float damage;
    [SerializeField] private float projectileSpeed;
    [Space]
    [SerializeField] private Vector3 rotationOffset;
    [SerializeField] private Transform[] gunPointSet;
    private int gunPointIndex;

    protected override void Awake()
    {
        base.Awake();
        machineGunVisuals = GetComponent<MachineGun_Visuals>();
    }

    protected override void Attack()
    {
        gunPoint = gunPointSet[gunPointIndex];
        Vector3 directionToEnemy = DirectionToEnemyFrom(gunPoint);

        if (Physics.Raycast(gunPoint.position, directionToEnemy, out RaycastHit hitInfo, Mathf.Infinity, whatIsTargetable))
        {
            IDamagable damagable = hitInfo.transform.GetComponent<IDamagable>();

            if (damagable == null)
                return;

            GameObject newProjectile = objectPool.Get(projectilePrefab,gunPoint.position,gunPoint.rotation);
            newProjectile.GetComponent<Projectile_MachineGun>().SetupProjectile(hitInfo.point, damagable, damage,projectileSpeed,objectPool);

            machineGunVisuals.RecoilFx(gunPoint);


            base.Attack();
            gunPointIndex = (gunPointIndex + 1) % gunPointSet.Length;
        }
    }

    protected override void RotateTowardsEnemy()
    {
        if (currentEnemy == null)
            return;

        Vector3 directionToEnemy = (currentEnemy.CenterPoint() - rotationOffset) - towerHead.position;
        Quaternion lookRotation = Quaternion.LookRotation(directionToEnemy);

        Vector3 rotation = Quaternion.Lerp(towerHead.rotation, lookRotation, rotationSpeed * Time.deltaTime).eulerAngles;
        towerHead.rotation = Quaternion.Euler(rotation);
    }
}
