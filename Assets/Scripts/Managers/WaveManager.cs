using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

[System.Serializable]
public class WaveDetails
{
    public GridBuilder nextGrid;
    public EnemyPortal[] newPortals;
    public int basicEnemy;
    public int fastEnemy;
    public int heavyEnemy;
    public int swarmEnemy;
    public int stealthEnemy;
    public int flyingEnemy;
    public int flyingBossEnemy;
    public int spiderBossEnemy;
}

public class WaveManager : MonoBehaviour
{
    private GameManager gameManager;
    private TileAnimator tileAnimator;
    private UI_InGame inGameUI;
    [SerializeField] private GridBuilder currentGrid;
    [SerializeField] private NavMeshSurface droneNavSurface;
    [SerializeField] private NavMeshSurface flyingNavSurface;
    [SerializeField] private MeshCollider[] flyingNavMeshColliders;

    [Header("Wave Details")]
    [SerializeField] private WaveDetails[] levelWaves;
    [SerializeField] private int waveIndex;

    [Header("Level Update Details")]
    [SerializeField] private float yOffset = 5;
    [SerializeField] private float tileDelay = .1f;

    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject basicEnemy;
    [SerializeField] private GameObject fastEnemy;
    [SerializeField] private GameObject heavyEnemy;
    [SerializeField] private GameObject swarmEnemy;
    [SerializeField] private GameObject stealthEnemy;
    [SerializeField] private GameObject flyingEnemy;
    [SerializeField] private GameObject flyingBossEnemy;
    [SerializeField] private GameObject spiderBossEnemy;
    private List<EnemyPortal> enemyPortals;
    private bool makingNextWave;
    private bool waveInProgress;   // true mientras la oleada actual tiene enemigos vivos o en cola
    public bool gameBegun;

    /// <summary>El jugador puede forzar la siguiente oleada solo cuando la actual terminó y quedan oleadas pendientes.</summary>
    public bool CanForceWave() => gameBegun && !waveInProgress && !HasNoMoreWaves();

    private void Awake()
    {
        enemyPortals = new List<EnemyPortal>(Object.FindObjectsByType<EnemyPortal>(FindObjectsInactive.Exclude, FindObjectsSortMode.None));
        
        gameManager = FindFirstObjectByType<GameManager>();
        tileAnimator = FindFirstObjectByType<TileAnimator>();
        inGameUI = FindFirstObjectByType<UI_InGame>(FindObjectsInactive.Include);

        if (flyingNavMeshColliders == null || flyingNavMeshColliders.Length == 0)
            flyingNavMeshColliders = GetComponentsInChildren<MeshCollider>();
    }

    

    private void Update()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (Input.GetKeyDown(KeyCode.T))
            ActivateWaveManager();
#endif
    }

    public void ActivateWaveManager()
    {
        gameBegun = true;
        waveInProgress = false;
        inGameUI = gameManager.inGameUI;
        StartPreparationForNextWave();
    }
    public void DeactivateWaveManager() => gameBegun = false;
    public void CheckIfWaveCompleted()
    {
        if (gameBegun == false)
            return;

        if (AllEnemiesDefeated() == false || makingNextWave)
            return;

        makingNextWave = true;
        waveIndex++;

        if (HasNoMoreWaves())
        {
            gameManager.LevelCompleted();
            return;
        }

        if (HasNewLayout())
            AttemptToUpdateLayout();
        else
            StartPreparationForNextWave();
    }
    public void StartNewWave()
    {
        if (!CanForceWave()) return;

        waveInProgress = true;
        UpdateNavMeshes();
        GiveEnemiesToPortals();
        makingNextWave = false;
        inGameUI.UpdateForceWaveButton(false);
    }

    private void GiveEnemiesToPortals()
    {
        List<GameObject> newEnemies = GetNewEnemies();
        int portalIndex = 0;

        if (newEnemies == null)
        {
            Debug.LogWarning("I had no wave to setup");
            return;
        }

        for (int i = 0; i < newEnemies.Count; i++)
        {
            GameObject enemyToAdd = newEnemies[i];
            EnemyPortal portalToReciveEnemy = enemyPortals[portalIndex];

            portalToReciveEnemy.AddEnemy(enemyToAdd);

            portalIndex++;

            if (portalIndex >= enemyPortals.Count)
                portalIndex = 0;
        }
    }
    private void AttemptToUpdateLayout() => UpdateLevelLayout(levelWaves[waveIndex]);
    private void UpdateLevelLayout(WaveDetails nextWave)
    {
        GridBuilder nextGrid = nextWave.nextGrid;
        List<GameObject> grid = currentGrid.GetTileSetup();
        List<GameObject> newGrid = nextGrid.GetTileSetup();

        if (grid.Count != newGrid.Count)
        {
            Debug.LogWarning("Current grid and new grid have different size.");
            return;
        }

        List<TileSlot> tilesToRemove = new List<TileSlot>();
        List<TileSlot> tilesToAdd = new List<TileSlot>();

        for (int i = 0; i < grid.Count; i++)
        {
            TileSlot currentTile = grid[i].GetComponent<TileSlot>();
            TileSlot newTile = newGrid[i].GetComponent<TileSlot>();


            bool shouldBeUpdated = currentTile.GetMesh() != newTile.GetMesh() ||
                                   currentTile.GetMaterial() != newTile.GetMaterial() ||
                                   currentTile.GetAllChildren().Count != newTile.GetAllChildren().Count ||
                                   currentTile.transform.rotation != newTile.transform.rotation;

            if (shouldBeUpdated)
            {
                tilesToRemove.Add(currentTile);
                tilesToAdd.Add(newTile);

                grid[i] = newTile.gameObject;
            }
        }

        StartCoroutine(RebuildLevelCo(tilesToRemove, tilesToAdd,nextWave,tileDelay));
    }
    private IEnumerator RebuildLevelCo(List<TileSlot> tilesToRemove,List<TileSlot> tilesToAdd, WaveDetails waveDetails,float delay)
    {
        for (int i = 0; i < tilesToRemove.Count; i++)
        {
            yield return new WaitForSeconds(delay);
            RemoveTile(tilesToRemove[i]);
        }

        for (int i = 0; i < tilesToAdd.Count; i++)
        {
            yield return new WaitForSeconds(delay);
            AddTile(tilesToAdd[i]);
        }

        EnableNewPortals(waveDetails.newPortals);
        StartPreparationForNextWave();
    }
    private void AddTile(TileSlot newTile)
    {
        newTile.gameObject.SetActive(true);
        newTile.transform.position += new Vector3(0, -yOffset, 0);
        newTile.transform.parent = currentGrid.transform;

        Vector3 targetPosition = newTile.transform.position + new Vector3(0, yOffset, 0);
        tileAnimator.MoveTile(newTile.transform, targetPosition);
    }
    private void RemoveTile(TileSlot tileToRemove)
    {
        Vector3 targetPosition = tileToRemove.transform.position + new Vector3(0,-yOffset, 0);
        tileAnimator.MoveTile(tileToRemove.transform, targetPosition);

        Destroy(tileToRemove.gameObject, 1);
    }
    private void StartPreparationForNextWave()
    {
        waveInProgress = false;
        // Habilita el botón solo si quedan oleadas por jugar.
        inGameUI?.UpdateForceWaveButton(!HasNoMoreWaves());
    }
    private void EnableNewPortals(EnemyPortal[] newPortals)
    {
        foreach (EnemyPortal portal in newPortals)
        {
            portal.AssignWaveManager(this);
            portal.gameObject.SetActive(true);
            enemyPortals.Add(portal);
        }
    }
    private void UpdateNavMeshes()
    {
        foreach (var collider in flyingNavMeshColliders)
        {
            collider.enabled = true;
        }

        flyingNavSurface.BuildNavMesh();

        foreach (var collider in flyingNavMeshColliders)
        {
            collider.enabled = false;
        }

        currentGrid.UpdateNavMesh();
        droneNavSurface.BuildNavMesh();
    }

    public void UpdateDroneNavMesh() => droneNavSurface.BuildNavMesh();

    private List<GameObject> GetNewEnemies()
    {
        if (waveIndex >= levelWaves.Length)
        {
            return null;
        }

        List<GameObject> newEnemyList = new List<GameObject>();

        for (int i = 0; i < levelWaves[waveIndex].basicEnemy; i++)
        {
            newEnemyList.Add(basicEnemy);
        }

        for (int i = 0; i < levelWaves[waveIndex].fastEnemy; i++)
        {
            newEnemyList.Add(fastEnemy);
        }

        for (int i = 0; i < levelWaves[waveIndex].heavyEnemy; i++)
        {
            newEnemyList.Add(heavyEnemy);
        }

        for (int i = 0; i < levelWaves[waveIndex].swarmEnemy; i++)
        {
            newEnemyList.Add(swarmEnemy);
        }

        for (int i = 0; i < levelWaves[waveIndex].stealthEnemy; i++)
        {
            newEnemyList.Add(stealthEnemy);
        }

        for (int i = 0; i < levelWaves[waveIndex].flyingEnemy; i++)
        {
            newEnemyList.Add(flyingEnemy);
        }

        for (int i = 0; i < levelWaves[waveIndex].flyingBossEnemy; i++)
        {
            newEnemyList.Add(flyingBossEnemy);
        }

        for (int i = 0; i < levelWaves[waveIndex].spiderBossEnemy; i++)
        {
            newEnemyList.Add(spiderBossEnemy);
        }


        return newEnemyList;
    }
    public WaveDetails[] GetLevelWaves() => levelWaves;

    // Click derecho en el componente WaveManager → "Setup Level X Waves" para aplicar una configuración por defecto.

    [ContextMenu("Setup Level 1 Waves")]
    private void SetupLevel1DefaultWaves()
    {
        levelWaves = new WaveDetails[]
        {
            new WaveDetails { basicEnemy = 8 },                          // Oleada 1: básicos lentos
            new WaveDetails { basicEnemy = 12 },                         // Oleada 2: más básicos
            new WaveDetails { basicEnemy = 15, fastEnemy = 3 }           // Oleada 3: básicos + rápidos
        };
        Debug.Log("[WaveManager] Oleadas del Nivel 1 configuradas.");
    }

    [ContextMenu("Setup Level 2 Waves")]
    private void SetupLevel2DefaultWaves()
    {
        levelWaves = new WaveDetails[]
        {
            new WaveDetails { basicEnemy = 10, fastEnemy = 2 },                              // Oleada 1
            new WaveDetails { basicEnemy = 8,  fastEnemy = 4,  heavyEnemy = 2 },             // Oleada 2
            new WaveDetails { basicEnemy = 5,  fastEnemy = 5,  heavyEnemy = 3, swarmEnemy = 2 },   // Oleada 3
            new WaveDetails { fastEnemy = 6,   heavyEnemy = 4, swarmEnemy = 3, stealthEnemy = 1 }  // Oleada 4
        };
        Debug.Log("[WaveManager] Oleadas del Nivel 2 configuradas.");
    }

    [ContextMenu("Setup Level 3 Waves")]
    private void SetupLevel3DefaultWaves()
    {
        levelWaves = new WaveDetails[]
        {
            new WaveDetails { fastEnemy = 8,  heavyEnemy = 4,  swarmEnemy = 2 },                                    // Oleada 1
            new WaveDetails { fastEnemy = 6,  heavyEnemy = 5,  swarmEnemy = 4,  stealthEnemy = 2 },                 // Oleada 2
            new WaveDetails { fastEnemy = 4,  heavyEnemy = 6,  swarmEnemy = 5,  stealthEnemy = 3, flyingEnemy = 1 }, // Oleada 3
            new WaveDetails { heavyEnemy = 5, swarmEnemy = 6,  stealthEnemy = 4, flyingEnemy = 2 },                  // Oleada 4
            new WaveDetails { heavyEnemy = 4, swarmEnemy = 5,  stealthEnemy = 4, flyingEnemy = 3, flyingBossEnemy = 1 } // Oleada 5 (boss)
        };
        Debug.Log("[WaveManager] Oleadas del Nivel 3 configuradas.");
    }


    private bool AllEnemiesDefeated()
    {
        foreach (EnemyPortal portal in enemyPortals)
        {
            // Considera tanto los enemigos activos como los que aún esperan en la cola de spawn.
            if (portal.GetActiveEnemies().Count > 0 || portal.HasPendingEnemies())
                return false;
        }
        return true;
    }
    private bool HasNewLayout() => waveIndex < levelWaves.Length && levelWaves[waveIndex].nextGrid != null;
    private bool HasNoMoreWaves() => waveIndex >= levelWaves.Length;

}
