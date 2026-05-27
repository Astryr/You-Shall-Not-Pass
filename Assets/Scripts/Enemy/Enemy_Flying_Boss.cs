using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Flying_Boss : Enemy_Flying
{
    [Header("Boss Details")]
    [SerializeField] private GameObject bossUnitPrefab;
    [Tooltip("Cantidad de unidades que suelta el boss. Cada una puede hacer 1 daño al castillo.")]
    [SerializeField] private int amountToCreate = 15;
    private int unitsCreated;
    [Tooltip("Segundos entre cada unidad spawneada. Valor más bajo = llegada en avalancha.")]
    [SerializeField] private float cooldown = 0.5f;
    private float creationTimer;

    private List<Enemy> createdEnemies = new List<Enemy>();


    protected override void OnEnable()
    {
        base.OnEnable();
        unitsCreated = 0;
    }

    protected override void Update()
    {
        base.Update();

        creationTimer -= Time.deltaTime;

        if (creationTimer < 0 && unitsCreated < amountToCreate)
        {
            creationTimer = cooldown;
            CreateNewBossUnit();
        }
    }

    private void CreateNewBossUnit()
    {
        unitsCreated++;
        GameObject newUnit = objectPool.Get(bossUnitPrefab, transform.position, Quaternion.identity);

        Enemy_BossUnit bossUnit = newUnit.GetComponent<Enemy_BossUnit>();

        bossUnit.SetupEnemy(GetFinalWaypoint(),this,myPortal);

        createdEnemies.Add(bossUnit);
    }

    private void EliminateAllUnits()
    {
        foreach (Enemy enemy in createdEnemies)
        {
            enemy.Die();
        }
    }

    public override void Die()
    {
        EliminateAllUnits();
        base.Die();
    }
}
