using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class LevelSetup : MonoBehaviour
{
    private UI ui;
    private TileAnimator tileAnimator;
    private LevelManager levelManager;
    private GameManager gameManager;
    private BuildManager buildManager;

    [Header("Level Details")]
    [SerializeField] private int levelCurrency = 1000;
    [Tooltip("Cuántos enemigos puede absorber el castillo antes de que el jugador pierda.")]
    [SerializeField] private int levelMaxHp = 100;
    [Tooltip("Activar solo en el Nivel 1. Muestra el tutorial la primera vez que el jugador juega.")]
    [SerializeField] private bool showTutorialIfFirstTime = false;
    [SerializeField] private List<TowerUnlockData> towerUnlocks;

    [Header("Level Setup")]
    [SerializeField] private GridBuilder myMainGrid;
    [SerializeField] private WaveManager myWaveManager;
    [SerializeField] private List<GameObject> extraObjectsToDelete = new List<GameObject>();


    private IEnumerator Start()
    {
        // Las escenas de nivel se cargan de forma aditiva junto a MainScene, que ya tiene su propio
        // EventSystem. Desactivar el duplicado evita el warning "There can be only one active EventSystem".
        EventSystem localEventSystem = GetComponentInChildren<EventSystem>(true)
            ?? FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include);
        if (localEventSystem != null && FindObjectsByType<EventSystem>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Length > 1)
            localEventSystem.gameObject.SetActive(false);

        if (LevelWasLoadedToMainScene())
        {
            DeleteExtraObjects();

            buildManager = FindFirstObjectByType<BuildManager>();
            buildManager.UpdateBuildManager(myWaveManager);

            levelManager.UpdateCurrentGrid(myMainGrid);

            tileAnimator = FindFirstObjectByType<TileAnimator>();
            tileAnimator.ShowGrid(myMainGrid, true);

            yield return tileAnimator.GetCurrentActiveCo();

            ui = FindFirstObjectByType<UI>();
            ui.EnableInGameUI(true);

            gameManager = FindFirstObjectByType<GameManager>();
            gameManager.PrepareLevel(levelCurrency, myWaveManager, levelMaxHp);

            // Solo el Nivel 1 muestra el tutorial automático la primera vez que el jugador juega.
            if (showTutorialIfFirstTime)
                gameManager.inGameUI.ShowTutorialIfFirstTime();

            // La transición desde menú puede dejar controles apagados hasta que termina el tween; al estar el nivel listo, activar zoom/pan.
            FindFirstObjectByType<CameraController>()?.EnableCameraConrolls(true);
        }

        UnlockAvalibleTowers();
    }

    private bool LevelWasLoadedToMainScene()
    {
        levelManager = FindFirstObjectByType<LevelManager>();

        return levelManager != null;
    }

    private void DeleteExtraObjects()
    {
        foreach (var obj in extraObjectsToDelete)
        {
             Destroy(obj);
        }
    }

    private void UnlockAvalibleTowers()
    {
        UI ui = FindFirstObjectByType<UI>();

        foreach (var unlockData in towerUnlocks)
        {
            foreach(var buildButton in ui.buildButtonsUI.GetBuildButtons())
            {
                buildButton.UnlockTowerIfNeeded(unlockData.towerName, unlockData.unlocked);
            }
        }

        ui.buildButtonsUI.UpdateUnlockedButtons();
    }

    public WaveManager GetWaveManager() => myWaveManager;

    [ContextMenu("Initialize Tower Data")]
    private void InitialiezTowerData()
    {
        towerUnlocks.Clear();

        towerUnlocks.Add(new TowerUnlockData("Crossbow", false));
        towerUnlocks.Add(new TowerUnlockData("Cannon", false));
        towerUnlocks.Add(new TowerUnlockData("Rapid Fire Gun", false));
        towerUnlocks.Add(new TowerUnlockData("Hammer", false));
        towerUnlocks.Add(new TowerUnlockData("Spider Nest", false));
        towerUnlocks.Add(new TowerUnlockData("AA Harpon", false));
        towerUnlocks.Add(new TowerUnlockData("Just Fan", false));
    }
}


[System.Serializable]
public class TowerUnlockData
{
    public string towerName;
    public bool unlocked;

    public TowerUnlockData(string newTowerName, bool newUnlockedStatus)
    {
        towerName = newTowerName;
        unlocked = newUnlockedStatus;
    }
}