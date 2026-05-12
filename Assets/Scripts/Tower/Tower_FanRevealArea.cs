using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower_FanRevealArea : MonoBehaviour
{
    private Tower_Fan tower;

    private void Awake()
    {
        tower = GetComponentInParent<Tower_Fan>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Enemy enemy = other.GetComponent<Enemy>();

        if(enemy != null)
            tower.AddEnemyToReveal(enemy);
    }

    private void OnTriggerExit(Collider other)
    {
        Enemy enemy = other.GetComponent<Enemy>();

        if (enemy != null)
            tower.RemoveEnemyToReveal(enemy);
    }
}
