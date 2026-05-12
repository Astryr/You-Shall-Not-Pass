using System.Collections;
using UnityEngine;

// Estado global del nivel: vidas (amenaza), oro, oleada activa y disparo de pantallas victoria / derrota vía UI_InGame.
public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public WaveManager currentActiveWaveManager;
    public UI_InGame inGameUI { get; private set; }
    private LevelManager levelManager;
    private CameraEffects cameraEffects;


    [SerializeField] private int currency;

    [SerializeField] private int maxHp;
    [SerializeField] private int currentHp;

    public int enemiesKilled { get; private set; }

    private bool gameLost;

    private void Awake()
    {
        instance = this;

        inGameUI = FindFirstObjectByType<UI_InGame>(FindObjectsInactive.Include);
        levelManager = FindFirstObjectByType<LevelManager>();
        cameraEffects = FindFirstObjectByType<CameraEffects>();
    }

    private void Start()
    {
        currentHp = maxHp;

        if (IsTestingLevel())
        {
            currency += 9999;
            currentHp += 9999;
        }


        inGameUI.UpdateHealthPointsUI(currentHp, maxHp);
        inGameUI.UpdateCurrencyUI(currency);
    }

    public bool IsTestingLevel() => levelManager == null;

    /// <summary>Derrota: detiene oleadas, enfoque al castillo y muestra game over.</summary>
    public IEnumerator LevelFailedCo()
    {
        gameLost = true;
        currentActiveWaveManager?.DeactivateWaveManager();
        if (cameraEffects != null)
            cameraEffects.FocusOnCastle();

        if (cameraEffects != null)
            yield return cameraEffects.GetActiveCamCo();

        inGameUI.EnableGameOverUI(true);
    }

    public void LevelCompleted() => StartCoroutine(LevelCompletedCo());

    /// <summary>Victoria de oleada/nivel: desbloquea siguiente nivel o pantalla final según LevelManager.</summary>
    private IEnumerator LevelCompletedCo()
    {
        if (cameraEffects != null)
            cameraEffects.FocusOnCastle();

        if (cameraEffects != null)
            yield return cameraEffects.GetActiveCamCo();

        if (levelManager != null && levelManager.HasNoMoreLevels())
        {
            inGameUI.EnableVictoryUI(true);
        }
        else if (levelManager != null)
        {
            string nextLevel = levelManager.GetNextLevelName();
            if (!string.IsNullOrEmpty(nextLevel))
                PlayerPrefs.SetInt(nextLevel + "unlocked", 1);
            inGameUI.EnableLevelCompletedUI(true);
        }
        else
        {
            inGameUI.EnableLevelCompletedUI(true);
        }
    }
    
    public void PrepareLevel(int levelCurrency, WaveManager newWaveManager)
    {
        gameLost = false;
        enemiesKilled = 0;

        currentActiveWaveManager = newWaveManager;
        currency = levelCurrency;
        currentHp = maxHp;

        inGameUI.UpdateHealthPointsUI(currentHp, maxHp);
        inGameUI.UpdateCurrencyUI(currency);

        newWaveManager.ActivateWaveManager();
    }

    public void UpdateHp(int value)
    {
        currentHp += value;
        inGameUI.UpdateHealthPointsUI(currentHp, maxHp);
        inGameUI.ShakeHealthUI();

        if (currentHp <= 0 && gameLost == false)
            StartCoroutine(LevelFailedCo());
    }

    public void UpdateCurrency(int value)
    {
        enemiesKilled++;
        currency += value;
        inGameUI.UpdateCurrencyUI(currency);
    }

    public void SpendCurrency(int value)
    {
        currency -= value;
        inGameUI.UpdateCurrencyUI(currency);
    }

    public bool HasEnoughCurrency(int price)
    {
        return currency >= price;
    }
}
