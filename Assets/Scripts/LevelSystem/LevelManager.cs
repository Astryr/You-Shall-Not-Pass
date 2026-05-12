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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
            LoadLevelFromMenu("Level_1");

        if (Input.GetKeyDown(KeyCode.K))
            LoadMainMenu();

        if(Input.GetKeyDown(KeyCode.R))
            RestartCurrentLevel();
    }

    public void RestartCurrentLevel() => StartCoroutine(LoadLevelCo(currentLevelName));
    public void LoadLevel(string levelName) => StartCoroutine(LoadLevelCo(levelName));
    public void LoadNextLevel() => LoadLevel(GetNextLevelName());
    public void LoadLevelFromMenu(string levelName) => StartCoroutine(LoadLevelFromMenuCo(levelName));
    public void LoadMainMenu() => StartCoroutine(LoadMainMenuCo());

    private IEnumerator LoadLevelCo(string levelName)
    {
        CleanUpScene();
        ui.EnableInGameUI(false);

        cameraEffects.SwitchToGameView();
        yield return tileAnimator.GetCurrentActiveCo();

        UnloadCurrentScene();
        LoadScene(levelName);
    }

    private IEnumerator LoadLevelFromMenuCo(string levelName)
    {
        tileAnimator.ShowMainGrid(false);
        ui.EnableMainMenuUI(false);

        cameraEffects.SwitchToGameView();
        

        yield return tileAnimator.GetCurrentActiveCo();

        tileAnimator.EnableMainSceneObjects(false);

        LoadScene(levelName);
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

        ui.EnableMainMenuUI(true);     
    }

    private void LoadScene(string sceneNameToLoad)
    {
        currentLevelName = sceneNameToLoad;
        SceneManager.LoadSceneAsync(sceneNameToLoad,LoadSceneMode.Additive);
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
