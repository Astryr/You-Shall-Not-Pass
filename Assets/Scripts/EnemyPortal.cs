using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Punto de spawn: hijos con Waypoint definen el camino (spawn → meta); instancia enemigos desde pool según la oleada.
public class EnemyPortal : MonoBehaviour
{
    private ObjectPoolManager objectPool;

    [SerializeField] private WaveManager myWaveManager;
    [SerializeField] private float spawnCooldown;
    private float spawnTimer;

    [Space]

    [SerializeField] private ParticleSystem flyPortalFx;
    private Coroutine flyPortalFxCo;
    
    [Space]

    [SerializeField] private List<Waypoint> waypointList;

    /// <summary>Posiciones ordenadas de la ruta (hijos Waypoint, de spawn a objetivo).</summary>
    public Vector3[] CurrentWaypoints { get; private set; }

    private List<GameObject> enemiesToCreate = new List<GameObject>();
    private List<GameObject> activeEnemies = new List<GameObject>();

    private void Awake()
    {
        CollectWaypoints();

        if (myWaveManager == null)
        {
            var levelSetup = FindFirstObjectByType<LevelSetup>();
            if (levelSetup != null)
                myWaveManager = levelSetup.GetWaveManager();
        }
    }

    private void Start()
    {
        objectPool = ObjectPoolManager.instance;
    }

    private void Update()
    {
        if (CanMakeNewEnemy())
            CreateEnemy();
    }

    public void AssignWaveManager(WaveManager newWaveManager) => myWaveManager = newWaveManager;

    private bool CanMakeNewEnemy()
    {
        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0 && enemiesToCreate.Count > 0)
        {
            spawnTimer = spawnCooldown;
            return true;
        }

        return false;
    }


    private void CreateEnemy()
    {
        if (objectPool == null)
            objectPool = ObjectPoolManager.instance;

        GameObject randomEnemy = GetRandomEnemy();
        GameObject newEnemy = objectPool.Get(randomEnemy, transform.position, Quaternion.identity);

        Enemy enemyScript = newEnemy.GetComponent<Enemy>();
        if (enemyScript == null)
        {
            Debug.LogError("Prefab sin componente Enemy.", newEnemy);
            return;
        }

        enemyScript.SetupEnemy(this);

        PlaceEnemyAtFlyPortalIfNeeded(newEnemy, enemyScript.GetEnemyType());
        activeEnemies.Add(newEnemy);
    }

    private void PlaceEnemyAtFlyPortalIfNeeded(GameObject newEnemy, EnemyType enemyType)
    {
        if (enemyType != EnemyType.Flying || flyPortalFx == null)
            return;

        if (flyPortalFxCo != null)
            StopCoroutine(flyPortalFxCo);

        flyPortalFxCo = StartCoroutine(EnableFlyPortalFxCo());
        newEnemy.transform.position = flyPortalFx.transform.position;
    }

    private IEnumerator EnableFlyPortalFxCo()
    {
        if (flyPortalFx == null)
            yield break;

        flyPortalFx.Play();

        yield return new WaitForSeconds(2);

        flyPortalFx.Stop();
    }

    private GameObject GetRandomEnemy()
    {
        int randomIndex = Random.Range(0, enemiesToCreate.Count);
        GameObject chosenEnemy = enemiesToCreate[randomIndex];

        enemiesToCreate.Remove(chosenEnemy);

        return chosenEnemy;
    }

    public void AddEnemy(GameObject enemyToAdd) => enemiesToCreate.Add(enemyToAdd);
    public void RemoveActiveEnemy(GameObject enemyToRemove)
    {
        if (activeEnemies.Contains(enemyToRemove))
            activeEnemies.Remove(enemyToRemove);

        myWaveManager?.CheckIfWaveCompleted();
    }

    public List<GameObject> GetActiveEnemies() => activeEnemies;


    private void CollectWaypoints()
    {
        waypointList = new List<Waypoint>(); 

        foreach (Transform child in transform)
        {
            Waypoint waypoint = child.GetComponent<Waypoint>();

            if(waypoint != null)
                waypointList.Add(waypoint);
        }

        CurrentWaypoints = new Vector3[waypointList.Count];

        for (int i = 0; i < CurrentWaypoints.Length; i++)
            CurrentWaypoints[i] = waypointList[i].transform.position;
    }
}
