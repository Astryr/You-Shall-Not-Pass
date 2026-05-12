using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower_Fan : Tower
{
    [Header("Fan Details")]
    [SerializeField] private float revealFrequency = .1f;
    [SerializeField] private float revealDuration = 1f;

    private List<Enemy> enemiesToReveal = new List<Enemy>();

    protected override void Awake()
    {
        base.Awake();
        InvokeRepeating(nameof(RevealEnemies), .1f, revealFrequency);
    }

    private void RevealEnemies()
    {
        foreach (var enemy in enemiesToReveal)
            enemy.DisableHide(revealDuration);
    }

    public void AddEnemyToReveal(Enemy enemy) => enemiesToReveal.Add(enemy);
    public void RemoveEnemyToReveal(Enemy enemy) => enemiesToReveal.Remove(enemy);

    private void OnValidate()
    {
        ForwardAttackDisplay display = GetComponent<ForwardAttackDisplay>();

        if (display != null)
            display.CreateLines(true, attackRange);
    }
}
