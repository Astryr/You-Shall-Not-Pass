using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Stealth_Hidearea : MonoBehaviour
{
    private Enemy_Stealth enemy;
    private void Awake() => enemy = GetComponentInParent<Enemy_Stealth>();

    private void OnTriggerEnter(Collider other)
    {
        AddEnemyToHideList(other, true);
    }

    private void OnTriggerExit(Collider other)
    {
        AddEnemyToHideList(other, false);
    }

    private void AddEnemyToHideList(Collider enemyCollider, bool addEnemy)
    {
        Enemy newEnemy = enemyCollider.GetComponent<Enemy>();

        if (newEnemy == null)
            return;

        if (newEnemy.GetEnemyType() == EnemyType.Stealth)
            return;

        if (addEnemy)
            enemy.GetEnemiesToHide().Add(newEnemy);
        else
            enemy.GetEnemiesToHide().Remove(newEnemy);
    }

}
