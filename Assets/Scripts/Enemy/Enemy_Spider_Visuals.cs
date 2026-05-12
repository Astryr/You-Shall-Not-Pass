using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Spider_Visuals : Enemy_Visuals
{
    [Header("Leg Details")]
    public float legSpeed = 3;
    public float increasedLegSpeed = 10;

    private Spider_Leg[] legs;

    [Header("Body animation")]
    [SerializeField] private Transform bodyTransform;
    [SerializeField] private float bodyAnimSpeed = 1;
    [SerializeField] private float maxHeight = .1f;

    private Vector3 startPosition;
    private float elapsedTime;

    [Header("Smoke animation")]
    [SerializeField] private ParticleSystem[] smokeFx;
    [SerializeField] private float smokeCooldown;
    private float smokeTimer;

    protected override void Awake()
    {
        base.Awake();
        legs = GetComponentsInChildren<Spider_Leg>();
    }

    protected override void Start()
    {
        base.Start();

        startPosition = bodyTransform.localPosition;
    }

    protected override void Update()
    {
        base.Update();

        AnimateBody();
        ActivateSmokeFxIfCan();
        UpdateSpiderLegs();
    }

    private void ActivateSmokeFxIfCan()
    {
        smokeTimer -= Time.deltaTime;

        if (smokeTimer < 0)
        {
            smokeTimer = smokeCooldown;

            foreach (var smoke in smokeFx)
            {
                smoke.Play();
            }
        }
    }

    private void AnimateBody()
    {
        elapsedTime += Time.deltaTime * bodyAnimSpeed; 

        float sinValue = (Mathf.Sin(elapsedTime) + 1) / 2; // 0 and 1
        float newY = Mathf.Lerp(0,maxHeight, sinValue);

        bodyTransform.localPosition = startPosition + new Vector3(0,newY,0);
    }

    private void UpdateSpiderLegs()
    {
        foreach (var leg in legs)
        {
            leg.UpdateLeg();
        }
    }

    public void BrieflySpeedUpLegs()
    {
        foreach (var leg in legs)
        {
            leg.SpeedUpLeg();
        }
    }
}
