using UnityEngine;

public class Tower_Cannon : Tower
{

    [Header("Cannon details")]
    [SerializeField] private float damage;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float timeToTarget = 1.5f;
    [SerializeField] private ParticleSystem attackVFX;

    protected override void Attack()
    {
        base.Attack();

        Vector3 velocity = CalculateLaunchVelocity();
        attackVFX.Play();

        GameObject newProjectile = objectPool.Get(projectilePrefab, gunPoint.position, Quaternion.identity);
        newProjectile.GetComponent<Projectile_Cannon>().SetupProjectile(velocity, damage, objectPool);
    }

    protected override Enemy FindEnemyWithinRange()
    {
        int collidersFound = Physics.OverlapSphereNonAlloc(transform.position, attackRange, allocatedColliders, whatIsEnemy);
        Enemy bestTarget = null;
        int maxNearbyEnemies = 0;

        for (int i = 0; i < collidersFound; i++)
        {
            Transform enemyTransform = allocatedColliders[i].transform;

            int amountOfEnemiesAround = EnemiesAroundEnemy(enemyTransform);

            if (amountOfEnemiesAround > maxNearbyEnemies)
            {
                maxNearbyEnemies = amountOfEnemiesAround;
                bestTarget = enemyTransform.GetComponent<Enemy>();
            }
        }

        return bestTarget;
    }

    private int EnemiesAroundEnemy(Transform enemyToCheck)
    {
        return Physics.OverlapSphereNonAlloc(enemyToCheck.position, 1, allocatedColliders, whatIsEnemy);
    }

    protected override void HandleRotation()
    {
        if (currentEnemy == null)
            return;

        RotateBodyTowardsEnemy();
        FaceLaunchDirection();
    }


    private void FaceLaunchDirection()
    {
        Vector3 attackDirection = CalculateLaunchVelocity();
        Quaternion lookRotation = Quaternion.LookRotation(attackDirection);

        Vector3 rotation = Quaternion.Lerp(towerHead.rotation, lookRotation, rotationSpeed * Time.deltaTime).eulerAngles;

        towerHead.rotation = Quaternion.Euler(rotation.x, towerHead.eulerAngles.y, 0);
    }

    private Vector3 CalculateLaunchVelocity()
    {
        Vector3 direction = currentEnemy.CenterPoint() - gunPoint.position;
        Vector3 directionXZ = new Vector3(direction.x, 0, direction.z);
        Vector3 velocityXZ = directionXZ / timeToTarget;

        float yVelocity = (direction.y - (Physics.gravity.y * Mathf.Pow(timeToTarget, 2)) / 2) / timeToTarget;
        Vector3 launchVelocity = velocityXZ + (Vector3.up * yVelocity);

        return launchVelocity;
    }
}
