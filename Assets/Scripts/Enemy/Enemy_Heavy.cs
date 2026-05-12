using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Heavy : Enemy
{
    [Header("Enemy details")]
    [SerializeField] private float maxShield = 50;
    [SerializeField] private float currentShield = 50;
    [SerializeField] private Enemy_Shield shieldObject;

    protected override void OnEnable()
    {
        base.OnEnable();

        currentShield = maxShield;
        EnableShieldIfNeeded();
    }
    private void EnableShieldIfNeeded()
    {
        if (shieldObject != null & currentShield > 0)
            shieldObject.gameObject.SetActive(true);
    }

    public override void TakeDamage(float damage)
    {
        if (currentShield > 0)
        {
            currentShield -= damage;
            shieldObject.ActivateShieldImpact();

            if (currentShield <= 0)
                shieldObject.gameObject.SetActive(false);
        }
        else
            base.TakeDamage(damage);
    }
}
