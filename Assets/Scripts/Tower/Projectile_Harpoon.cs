using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile_Harpoon : MonoBehaviour
{
    private Tower_Harpoon tower;
    private Enemy enemy;
    private float speed;
    private bool isAttached;


    [SerializeField] private Transform connectionPoint;

    private void Update()
    {
        if (enemy == null || isAttached)
            return;

        MoveTowardsEnemy();

        if (Vector3.Distance(transform.position, enemy.transform.position) < .35f)
            AttachToEnemy();
    }

    private void MoveTowardsEnemy()
    {
        transform.position = Vector3.MoveTowards(transform.position, enemy.transform.position, speed * Time.deltaTime);

        Vector3 direction = enemy.transform.position - transform.position;
        if (direction != Vector3.zero && direction.sqrMagnitude > 0.001f)
            transform.forward = direction;
    }

    private void AttachToEnemy()
    {
        isAttached = true;
        transform.parent = enemy.transform;
        tower.ActivateAttack();
    }

    public void SetupProjectile(Enemy newEnemy, float newSpeed, Tower_Harpoon newTower)
    {
        speed = newSpeed;
        enemy = newEnemy;
        tower = newTower;
    }

    public Transform GetConnectionPoint()
    {
        if (connectionPoint == null)
            return transform;

        return connectionPoint;
    }
}
