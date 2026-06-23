using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        if (LevelWasLoadedToMainScene())
        {
            ui = FindMainSceneUI();
            ui?.ShowLoadingScreen("Construyendo nivel...");

            LevelEnvironmentOptimizer.Apply();
            DeleteExtraObjects();

            // Tras borrar duplicados del nivel, re-vincular la UI de MainScene (nunca la del nivel).
            ui = FindMainSceneUI();

            buildManager = FindFirstObjectByType<BuildManager>();
            buildManager?.ClearBuildSelection();
            buildManager?.UpdateBuildManager(myWaveManager);

            levelManager.UpdateCurrentGrid(myMainGrid);

            tileAnimator = FindFirstObjectByType<TileAnimator>();
            if (tileAnimator == null)
            {
                Debug.LogError("[LevelSetup] No se encontró TileAnimator en la escena.");
                yield break;
            }

            tileAnimator.ShowGrid(myMainGrid, true);

            ui?.SetLoadingProgress(0.35f);
            yield return tileAnimator.GetCurrentActiveCo();
            ui?.SetLoadingProgress(0.85f);

            ui?.EnableInGameUI(true);

            gameManager = FindFirstObjectByType<GameManager>();
            if (gameManager == null)
            {
                Debug.LogError("[LevelSetup] No se encontró GameManager en la escena.");
                yield break;
            }

            gameManager.PrepareLevel(levelCurrency, myWaveManager, levelMaxHp);

            AudioManager.instance?.PlayLevelMusic();

            if (showTutorialIfFirstTime)
                gameManager.inGameUI?.ShowTutorialOnLevelStart();

            FindFirstObjectByType<CameraController>()?.EnableCameraConrolls(true);

            ui?.SetLoadingProgress(1f);
            ui?.HideLoadingScreen();
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
            if (obj == null) continue;
            Destroy(obj);
        }

        DestroyDuplicateUICanvases();
    }

    private static void DestroyDuplicateUICanvases()
    {
        Scene mainScene = SceneManager.GetSceneByName("MainScene");
        UI[] allUi = Object.FindObjectsByType<UI>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (UI levelUi in allUi)
        {
            if (levelUi == null)
                continue;

            if (mainScene.IsValid() && levelUi.gameObject.scene == mainScene)
                continue;

            Destroy(levelUi.gameObject);
        }
    }

    private static UI FindMainSceneUI()
    {
        Scene mainScene = SceneManager.GetSceneByName("MainScene");
        if (!mainScene.IsValid())
            return FindFirstObjectByType<UI>(FindObjectsInactive.Include);

        foreach (GameObject root in mainScene.GetRootGameObjects())
        {
            UI found = root.GetComponentInChildren<UI>(true);
            if (found != null)
                return found;
        }

        return null;
    }

    private void UnlockAvalibleTowers()
    {
        UI ui = FindMainSceneUI();
        if (ui == null || ui.buildButtonsUI == null)
            return;

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