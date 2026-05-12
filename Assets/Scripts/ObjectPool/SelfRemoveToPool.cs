using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfRemoveToPool : MonoBehaviour
{
    private ObjectPoolManager objectPool;
    private ParticleSystem particle;

    [SerializeField] private float removeDelay = 1;

    private void Awake()
    {
        objectPool = ObjectPoolManager.instance;
        particle = GetComponentInChildren<ParticleSystem>();
    }

    private void OnEnable()
    {
        if (particle != null)
        {
            particle.Clear();
            particle.Play();
        }


        StartCoroutine(RemoveWithDelayCo());
    }

    private IEnumerator RemoveWithDelayCo()
    {
        yield return new WaitForSeconds(removeDelay);
        objectPool.Remove(gameObject);
    }
}
