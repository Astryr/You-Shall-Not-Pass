using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
    protected ObjectPoolManager objectPool;
    public Enemy currentEnemy;

    protected bool towerActive = true;
    protected Coroutine deactiveatedTowerCo;
    protected GameObject currentEmpFx;

    [Tooltip("Enabling this allows tower to change target beetwen attacks")]
    [SerializeField] private bool dynamicTargetChange;
    [SerializeField] protected float attackCooldown = 1;
    protected float lastTimeAttacked;

    [Header("Tower Setup")]
    [SerializeField] protected EnemyType enemyPriorityType = EnemyType.None;
    [SerializeField] protected Transform towerHead;
    [SerializeField] protected Transform towerBody;
    [SerializeField] protected Transform gunPoint;
    [SerializeField] protected float rotationSpeed = 10;

    [SerializeField] protected float attackRange = 2.5f;
    [SerializeField] protected LayerMask whatIsEnemy;
    [SerializeField] protected LayerMask whatIsTargetable;
    public bool towerAttacksForward;

    private float targetCheckInterval = .1f;
    private float lastTimeCheckedTarget;
    protected Collider[] allocatedColliders = new Collider[100];

    [Header("SFX Details")]
    [SerializeField] protected AudioSource attackSfx;


    protected virtual void Awake()
    {
    }

    protected virtual void Start()
    {
        objectPool = ObjectPoolManager.instance;
    }

    protected virtual void FixedUpdate()
    {
        if (towerActive == false)
            return;

        LooseTargetIfNeeded();
        UpdateTargetIfNeeded();
        HandleRotation();

        if (CanAttack())
            AttemptToAttack();
    }

    public void DeactivateTower(float duration, GameObject empFxPrefab)
    {
        if(deactiveatedTowerCo != null)
            StopCoroutine(deactiveatedTowerCo);

        if(currentEmpFx != null)
            objectPool.Remove(currentEmpFx);

        currentEmpFx = objectPool.Get(empFxPrefab, transform.position + new Vector3(0,.5f,0),Quaternion.identity);
        deactiveatedTowerCo = StartCoroutine(DeactivateTowerCo(duration));
    }
    private IEnumerator DeactivateTowerCo(float duration)
    {
        towerActive = false;

        yield return new WaitForSeconds(duration);

        towerActive = true;
        lastTimeAttacked = Time.time;
        objectPool.Remove(currentEmpFx);
    }
    protected virtual void LooseTargetIfNeeded()
    {
        if (currentEnemy == null)
            return;

        if (Vector3.Distance(currentEnemy.CenterPoint(), transform.position) > attackRange)
            currentEnemy = null;
    }
    private void UpdateTargetIfNeeded()
    {
        if (currentEnemy == null)
        {
            currentEnemy = FindEnemyWithinRange();
            return;
        }

        if (dynamicTargetChange == false)
            return;

        if (Time.time > lastTimeCheckedTarget + targetCheckInterval)
        {
            lastTimeCheckedTarget = Time.time;
            currentEnemy = FindEnemyWithinRange();
        }
    }
    
    protected void AttemptToAttack()
    {
        if (currentEnemy.gameObject.activeSelf == false)
        {
            currentEnemy = null;
            return;
        }

        Attack();
    }


    protected virtual void Attack()
    {
        lastTimeAttacked = Time.time;
    }

    protected virtual bool CanAttack()
    {
        return Time.time > lastTimeAttacked + attackCooldown && currentEnemy != null;
    }

    protected virtual Enemy FindEnemyWithinRange()
    {
        List<Enemy> priorityTargets = new List<Enemy>();
        List<Enemy> possibleTargets = new List<Enemy>();

        int enemiesAround = Physics.OverlapSphereNonAlloc(transform.position, attackRange, allocatedColliders, whatIsEnemy);

        for (int i = 0; i < enemiesAround; i++)
        {
            Enemy newEnemy = allocatedColliders[i].GetComponent<Enemy>();
            
            if (newEnemy == null)
                continue;

            float distanceToEnemy = Vector3.Distance(transform.position, newEnemy.transform.position);

            if (distanceToEnemy > attackRange)
                continue;

            EnemyType newEnemyType = newEnemy.GetEnemyType();

            if (newEnemyType == enemyPriorityType)
                priorityTargets.Add(newEnemy);
            else
                possibleTargets.Add(newEnemy);
        }

        if (priorityTargets.Count > 0)
            return GetMostAdvancedEnemy(priorityTargets);

        if (possibleTargets.Count > 0)
            return GetMostAdvancedEnemy(possibleTargets);

        return null;
    }
    private Enemy GetMostAdvancedEnemy(List<Enemy> targets)
    {
        Enemy mostAdvancedEnemy = null;
        float minRemainingDistance = float.MaxValue;

        foreach (Enemy enemy in targets)
        {
            float remainingDistance = enemy.DistanceToFinishLine();

            if (remainingDistance < minRemainingDistance)
            {
                minRemainingDistance = remainingDistance;
                mostAdvancedEnemy = enemy;
            }
        }

        return mostAdvancedEnemy;
    }


    protected bool AtLeastOneEnemyAround()
    {
        int enemyColliders = Physics.OverlapSphereNonAlloc(transform.position, attackRange,allocatedColliders, whatIsEnemy);
        return enemyColliders > 0;
    }

    protected virtual void HandleRotation()
    {
        RotateTowardsEnemy();
        RotateBodyTowardsEnemy();
    }

    protected virtual void RotateTowardsEnemy()
    {
        if (currentEnemy == null || towerHead == null)
            return;

        // Calculate the vector direction from the tower's head to the current enemy.
        Vector3 directionToEnemy = DirectionToEnemyFrom(towerHead);

        // Create a Quaternion for the rotation towards the enemy, based on the direction vector.
        Quaternion lookRotation = Quaternion.LookRotation(directionToEnemy);

        // Interpolate smoothly between the current rotation of the tower's head and the desired look rotation.
        // 'rotationSpeed * Time.deltaTime' adjusts the speed of rotation to be frame-rate independent.
        Vector3 rotation = Quaternion.Lerp(towerHead.rotation, lookRotation, rotationSpeed * Time.deltaTime).eulerAngles;

        // Apply the interpolated rotation back to the tower's head. This step converts the Quaternion back to Euler angles for straightforward application.
        towerHead.rotation = Quaternion.Euler(rotation);
    }

    protected void RotateBodyTowardsEnemy()
    {
        if (towerBody == null || currentEnemy == null)
            return;

        Vector3 directionToEnemy = DirectionToEnemyFrom(towerBody);
        directionToEnemy.y = 0;

        Quaternion lookRotation = Quaternion.LookRotation(directionToEnemy);
        towerBody.rotation = Quaternion.Slerp(towerBody.rotation, lookRotation, rotationSpeed * Time.deltaTime);
    }


    protected Vector3 DirectionToEnemyFrom(Transform startPoint)
    {
        return (currentEnemy.CenterPoint() - startPoint.position).normalized;
    }

    public float GetAttackRange() => attackRange;
    public float GetAttackCooldown() => attackCooldown;


    protected virtual void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
