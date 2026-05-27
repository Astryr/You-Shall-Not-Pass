using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Los enemigos voladores ignoran los waypoints intermedios del suelo y vuelan
// directamente al último punto (el castillo) usando la superficie NavMesh aérea.
public class Enemy_Flying : Enemy
{
    private List<Tower_Harpoon> observingTowers = new List<Tower_Harpoon>();

    protected override bool IsAirborne => true;

    // Vuela directo al destino final en lugar de seguir la ruta terrestre waypoint a waypoint.
    protected override void ChangeWaypoint()
    {
        agent.SetDestination(GetFinalWaypoint());
    }

    // Un único destino: no hay waypoints intermedios a los que cambiar.
    protected override bool ShouldChangeWaypoint() => false;

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
