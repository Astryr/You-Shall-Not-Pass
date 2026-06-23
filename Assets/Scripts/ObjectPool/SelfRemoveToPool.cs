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
        particle = GetComponentInChildren<ParticleSystem>();
    }

    private void OnEnable()
    {
        if (objectPool == null)
            objectPool = ObjectPoolManager.instance;

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
        if (objectPool != null)
            objectPool.Remove(gameObject);
        else
            gameObject.SetActive(false);
    }
}
