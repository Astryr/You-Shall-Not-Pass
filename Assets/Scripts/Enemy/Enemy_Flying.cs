using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Flying : Enemy
{
    private List<Tower_Harpoon> observingTowers = new List<Tower_Harpoon>();

    protected override void Start()
    {
        base.Start();
        agent.SetDestination(GetFinalWaypoint());
    }

    public override float DistanceToFinishLine()
    {
        return Vector3.Distance(transform.position, GetFinalWaypoint());
    }

    public void AddObservingTower(Tower_Harpoon newTower) => observingTowers.Add(newTower);

    public override void RemoveEnemy()
    {
        if (observingTowers != null)
        {
            foreach (var tower in observingTowers)
            {
                if (tower != null)
                    tower.ResetAttack();
            }
            observingTowers.Clear();
        }

        foreach (var harpon in GetComponentsInChildren<Projectile_Harpoon>())
        {
            if (harpon != null && harpon.GetComponent<PooledObject>() != null)
                objectPool.Remove(harpon.gameObject);
            else if (harpon != null)
                Destroy(harpon.gameObject);
        }

        base.RemoveEnemy();
    }
}
