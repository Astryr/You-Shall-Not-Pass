using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

// Gestiona pools de objetos reutilizables (enemigos, proyectiles, VFX) usando UnityEngine.Pool.
// Evita el costo de Instantiate/Destroy en runtime: los objetos se desactivan y reusan en lugar de destruirse.
public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager instance;

    [Header("Object Pool Details")]
    [SerializeField] private GameObject[] enemyPools;
    [SerializeField] private GameObject[] projectilePools;
    [SerializeField] private GameObject[] vfxPools;
    [SerializeField] private int defaultPoolSize = 50;
    [SerializeField] private int maxPoolSize = 500;

    // Un pool por prefab; la clave es el prefab original para identificar el tipo.
    private Dictionary<GameObject, ObjectPool<GameObject>> poolDictionary;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        InitializePools();
    }

    /// <summary>Obtiene un objeto del pool (o lo crea si no existe pool para ese prefab).</summary>
    public GameObject Get(GameObject prefab, Vector3 position, Quaternion? rotation = null, Transform parent = null)
    {
        if (poolDictionary.ContainsKey(prefab) == false)
            CreateNewPool(prefab);

        GameObject objectToGet = poolDictionary[prefab].Get();
        objectToGet.transform.position = position;
        objectToGet.transform.rotation = rotation ?? Quaternion.identity;
        objectToGet.transform.parent   = parent;
        objectToGet.SetActive(true);

        return objectToGet;
    }

    /// <summary>Devuelve el objeto al pool (lo desactiva; no lo destruye).</summary>
    public void Remove(GameObject objectToRemove)
    {
        GameObject originalPrefab = objectToRemove.GetComponent<PooledObject>()?.originalPrefab;

        if (originalPrefab == null)
        {
            Debug.LogWarning("You do not have object pool for this game object. Game object will be destroyed!");
            Destroy(objectToRemove);
            return;
        }

        poolDictionary[originalPrefab].Release(objectToRemove);
    }

    private void InitializePools()
    {
        poolDictionary = new Dictionary<GameObject, ObjectPool<GameObject>>();

        foreach (GameObject prefab in enemyPools)      CreateNewPool(prefab);
        foreach (GameObject prefab in projectilePools) CreateNewPool(prefab);
        foreach (GameObject prefab in vfxPools)        CreateNewPool(prefab);
    }

    private void CreateNewPool(GameObject prefab)
    {
        var pool = new ObjectPool<GameObject>(
            createFunc:      () => NewPoolObject(prefab),
            actionOnRelease: obj => { obj.SetActive(false); obj.transform.parent = transform; },
            actionOnDestroy: obj => Destroy(obj),
            collectionCheck: false,
            defaultCapacity: defaultPoolSize,
            maxSize:         maxPoolSize
        );

        poolDictionary.Add(prefab, pool);
        // Pre-instancia los objetos al inicio para evitar picos de CPU durante el juego.
        StartCoroutine(PreloadPoolCo(pool, defaultPoolSize));
    }

    // Crea los objetos del pool de a uno por frame para no bloquear el hilo principal al cargar.
    private IEnumerator PreloadPoolCo(ObjectPool<GameObject> poolToPreload, int count)
    {
        List<GameObject> preloadedObjects = new List<GameObject>();

        for (int i = 0; i < count; i++)
        {
            GameObject obj = poolToPreload.Get();
            preloadedObjects.Add(obj);
            obj.SetActive(false);
            yield return null;
        }

        foreach (GameObject obj in preloadedObjects)
            poolToPreload.Release(obj);
    }

    private GameObject NewPoolObject(GameObject prefab)
    {
        bool wasActive = prefab.activeSelf;
        prefab.SetActive(false);
        GameObject newObject = Instantiate(prefab);
        prefab.SetActive(wasActive);

        newObject.AddComponent<PooledObject>().originalPrefab = prefab;
        return newObject;
    }
}
