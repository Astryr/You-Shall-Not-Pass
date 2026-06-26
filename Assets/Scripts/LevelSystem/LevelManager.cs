using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    private UI ui;
    private TileAnimator tileAnimator;
    private CameraEffects cameraEffects;

    private GridBuilder currentActiveGrid;
    public string currentLevelName { get; private set; }


    private void Awake()
    {
        cameraEffects = FindFirstObjectByType<CameraEffects>();
        tileAnimator = FindFirstObjectByType<TileAnimator>();
        ui = FindFirstObjectByType<UI>();
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
            LoadLevelFromMenu("Level_1");

        if (Input.GetKeyDown(KeyCode.K))
            LoadMainMenu();

        if (Input.GetKeyDown(KeyCode.R))
            RestartCurrentLevel();
    }
#endif

    public void RestartCurrentLevel() => StartCoroutine(LoadLevelCo(currentLevelName));
    public void LoadLevel(string levelName) => StartCoroutine(LoadLevelCo(levelName));
    public void LoadNextLevel() => LoadLevel(GetNextLevelName());
    public void LoadLevelFromMenu(string levelName) => StartCoroutine(LoadLevelFromMenuCo(levelName));
    public void LoadMainMenu() => StartCoroutine(LoadMainMenuCo());

    private IEnumerator LoadLevelCo(string levelName)
    {
        ui.ShowLoadingScreen("Recargando nivel...");
        CleanUpScene();
        ui.EnableInGameUI(false);
        cameraEffects.SwitchToGameView();

        // Esperar la animación de limpieza
        yield return tileAnimator.GetCurrentActiveCo();

        // Descargar nivel anterior
        string sceneToUnload = currentLevelName;
        if (!string.IsNullOrEmpty(sceneToUnload))
        {
            AsyncOperation unload = SceneManager.UnloadSceneAsync(sceneToUnload);
            if (unload != null) yield return unload;
        }

        // Precargar nueva escena en background, pausada antes de activar GameObjects
        currentLevelName = levelName;
        Application.backgroundLoadingPriority = ThreadPriority.Low;
        AsyncOperation load = SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Additive);
        load.allowSceneActivation = false;

        while (load.progress < 0.9f)
            yield return null;

        load.allowSceneActivation = true;
        yield return new WaitUntil(() => load.isDone);
        Application.backgroundLoadingPriority = ThreadPriority.BelowNormal;
    }

    private IEnumerator LoadLevelFromMenuCo(string levelName)
    {
        ui.ShowLoadingScreen("Cargando nivel...");

        // Iniciar la carga async INMEDIATAMENTE, pausada hasta que la animación termine.
        // Así el I/O y la descompresión de assets ocurren mientras el menú se anima,
        // evitando el spike de FPS que se producía al cargar después de la animación.
        Application.backgroundLoadingPriority = ThreadPriority.Low;
        AsyncOperation sceneLoad = SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Additive);
        sceneLoad.allowSceneActivation = false;
        currentLevelName = levelName;

        tileAnimator.ShowMainGrid(false);
        ui.EnableMainMenuUI(false);

        BuildManager buildManager = FindFirstObjectByType<BuildManager>();
        buildManager?.ClearBuildSelection();

        cameraEffects.SwitchToGameView();

        // Esperar animación de salida del menú Y que la escena esté prelistta (>= 90 %)
        yield return tileAnimator.GetCurrentActiveCo();
        while (sceneLoad.progress < 0.9f)
            yield return null;

        tileAnimator.EnableMainSceneObjects(false);

        // Activar la escena: Unity instancia todos los GameObjects en este frame
        sceneLoad.allowSceneActivation = true;
        yield return new WaitUntil(() => sceneLoad.isDone);
        Application.backgroundLoadingPriority = ThreadPriority.BelowNormal;
    }

    private IEnumerator LoadMainMenuCo()
    {
        CleanUpScene();
        ui.EnableInGameUI(false);

        cameraEffects.SwitchToMenuView();

        yield return tileAnimator.GetCurrentActiveCo();
        UnloadCurrentScene();

        tileAnimator.EnableMainSceneObjects(true);
        tileAnimator.ShowMainGrid(true);

        yield return tileAnimator.GetCurrentActiveCo();

        ui.HideLoadingScreen();
        ui.EnableMainMenuUI(true);
        AudioManager.instance?.PlayMenuMusic();
    }

    private void UnloadCurrentScene() => SceneManager.UnloadSceneAsync(currentLevelName);

    private void CleanUpScene()
    {
        EleminateAllEnemies();
        EleminateAllTowers();

        if(currentActiveGrid != null)
            tileAnimator.ShowGrid(currentActiveGrid, false);
    }

    private void EleminateAllEnemies()
    {
        Enemy[] enemies = Object.FindObjectsByType<Enemy>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (Enemy enemy in enemies)
        {
            enemy.RemoveEnemy();
        }
    }

    private void EleminateAllTowers()
    {
        Tower[] towers = Object.FindObjectsByType<Tower>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (Tower tower in towers)
        {
            Destroy(tower.gameObject);
        }
    }

    public void UpdateCurrentGrid(GridBuilder newGrid) => currentActiveGrid = newGrid;

    private int GetCurrentLevelBuildIndex()
    {
        Scene scene = SceneManager.GetSceneByName(currentLevelName);
        return scene.IsValid() ? scene.buildIndex : -1;
    }

    /// <summary>Build index of the scene that follows the current level in Build Settings.</summary>
    public int GetNextLevelIndex() => GetCurrentLevelBuildIndex() + 1;

    public string GetNextLevelName()
    {
        int nextBuildIndex = GetNextLevelIndex();
        if (nextBuildIndex < 0 || nextBuildIndex >= SceneManager.sceneCountInBuildSettings)
            return string.Empty;

        string scenePath = SceneUtility.GetScenePathByBuildIndex(nextBuildIndex);
        return Path.GetFileNameWithoutExtension(scenePath);
    }

    public bool HasNoMoreLevels() => GetNextLevelIndex() >= SceneManager.sceneCountInBuildSettings;
}
