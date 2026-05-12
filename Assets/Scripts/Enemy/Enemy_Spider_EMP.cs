using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Spider_EMP : MonoBehaviour
{
    private ObjectPoolManager objectPool;

    [SerializeField] private GameObject empFx;
    [SerializeField] private float moveSpeed = 1;
    [SerializeField] private float empRadius = 2;
    [SerializeField] private float empEffectDuration = 5;

    private Vector3 destionation;
    private float shrinkSpeed = 3;
    private bool shouldShrink;

    private void Awake()
    {
        objectPool = ObjectPoolManager.instance;
    }

    private void Update()
    {
        MoveTowardsTarget();

        if (shouldShrink)
            Shrink();
    }


    private void Shrink()
    {
        transform.localScale -= Vector3.one * shrinkSpeed * Time.deltaTime;

        if (transform.localScale.x <= .01f)
            objectPool.Remove(gameObject);
    }

    private void MoveTowardsTarget()
    {
        transform.position = Vector3.MoveTowards(transform.position, destionation, moveSpeed * Time.deltaTime);

        //comment this IF , if you want EMP not to desapear,when it reached destionation
        if (Vector3.Distance(transform.position, destionation) < .1f)
            DeactivateEMP();
    }


    public void SetupEMP(float duration, Vector3 newTarget,float empDuration)
    {
        empEffectDuration = duration;
        destionation = newTarget;


        //Use this if you want EMP to desapear after some time and not when it reached destination
        //Invoke(nameof(DeactivateEMP), empDuration);
    }

    private void DeactivateEMP() => shouldShrink = true;


    private void OnTriggerEnter(Collider other)
    {
        Tower tower = other.GetComponent<Tower>();

        if (tower != null)
            tower.DeactivateTower(empEffectDuration,empFx);
    }


    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, empRadius);
    }
}
