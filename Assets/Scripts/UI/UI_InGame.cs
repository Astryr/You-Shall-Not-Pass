using TMPro;
using UnityEngine;
using UnityEngine.UI;

// HUD en partida + paneles de victoria / derrota / nivel completado (GameManager los activa).
public class UI_InGame : MonoBehaviour
{
    private UI ui;
    private UI_Pause pauseUI;
    private UI_Animator uiAnimator;

    [SerializeField] private TextMeshProUGUI healthPointsText;
    [SerializeField] private TextMeshProUGUI currencyText;

    [Header("Victory & Defeat")]
    [SerializeField] private GameObject victoryUI;
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private GameObject levelCompletedUI;

    [Header("Tutorial")]
    [SerializeField] private UI_Tutorial tutorialUI;

    [Header("Wave Control")]
    [SerializeField] private Button forceWaveBtn;

    private void Awake()
    {
        uiAnimator = GetComponentInParent<UI_Animator>();
        ui = GetComponentInParent<UI>();
        pauseUI = ui.GetComponentInChildren<UI_Pause>(true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F10))
            ui.SwitchTo(pauseUI.gameObject);
    }

    public void EnableGameOverUI(bool enable)
    {
        if (enable)
            PresentEndGameOverlay(gameOverUI);
        else
            RestoreGameplayAfterEndScreen();
    }

    public void EnableVictoryUI(bool enable)
    {
        if (enable)
            PresentEndGameOverlay(victoryUI);
        else
            RestoreGameplayAfterEndScreen();
    }

    public void EnableLevelCompletedUI(bool enable)
    {
        if (enable)
            PresentEndGameOverlay(levelCompletedUI);
        else
            RestoreGameplayAfterEndScreen();
    }

    /// <summary>Solo el overlay activo; resto del HUD de partida + torres + pausa ocultos.</summary>
    private void PresentEndGameOverlay(GameObject overlay)
    {
        if (overlay == null)
            return;

        // Si el tutorial estaba abierto, cerrarlo sin pausa para que no bloquee el game over.
        if (tutorialUI != null && tutorialUI.gameObject.activeSelf)
        {
            Time.timeScale = 1f;
            tutorialUI.gameObject.SetActive(false);
        }

        ui.ApplyCanvasIsolationForEndGameScreen();

        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;
            bool isOverlay = child == victoryUI || child == gameOverUI || child == levelCompletedUI
                             || (tutorialUI != null && child == tutorialUI.gameObject);
            if (!isOverlay)
                child.SetActive(false);
            else
                child.SetActive(child == overlay);
        }

        ui.SetBuildAndPauseChromeVisible(false);
    }

    private void RestoreGameplayAfterEndScreen()
    {
        victoryUI?.SetActive(false);
        gameOverUI?.SetActive(false);
        levelCompletedUI?.SetActive(false);

        ui.ClearCanvasIsolationForEndGameScreen();

        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;
            bool isOverlay = child == victoryUI || child == gameOverUI || child == levelCompletedUI
                             || (tutorialUI != null && child == tutorialUI.gameObject);
            if (!isOverlay)
                child.SetActive(true);
        }

        ui.SetBuildAndPauseChromeVisible(true);
    }

    /// <summary>Restaura HUD + oculta overlays (al cargar nivel o salir de pantalla de fin).</summary>
    public void ResetToGameplayLayout()
    {
        RestoreGameplayAfterEndScreen();
        // Empieza deshabilitado; WaveManager lo habilita cuando la primera oleada está lista.
        UpdateForceWaveButton(false);
    }

    /// <summary>
    /// Muestra el tutorial la primera vez. LevelSetup lo llama solo en el nivel que corresponde
    /// (Nivel 1); los demás niveles no lo invocan para que no reaparezca innecesariamente.
    /// </summary>
    public void ShowTutorialIfFirstTime() => tutorialUI?.ShowIfFirstTime();

    /// <summary>Abre el tutorial manualmente (botón ? del HUD).</summary>
    public void OpenTutorial() => tutorialUI?.Show();

    

    public void ShakeCurrencyUI() => ui.uiAnim.Shake(currencyText.transform.parent);
    public void ShakeHealthUI() => ui.uiAnim.Shake(healthPointsText.transform.parent);


    public void UpdateHealthPointsUI(int value, int maxValue)
    {
        int newValue = maxValue - value;
        healthPointsText.text = "Threat : " + newValue + "/" + maxValue;
    }

    public void UpdateCurrencyUI(int value)
    {
        currencyText.text = "resources : " + value;
    }

    public void ForceWaveButton()
    {
        WaveManager waveManager = FindFirstObjectByType<WaveManager>();
        if (waveManager == null || !waveManager.CanForceWave()) return;
        waveManager.StartNewWave();
    }

    /// <summary>Muestra/oculta y habilita/deshabilita el botón Force Wave según el estado de la oleada.</summary>
    public void UpdateForceWaveButton(bool visible)
    {
        if (forceWaveBtn == null) return;
        forceWaveBtn.gameObject.SetActive(visible);
        forceWaveBtn.interactable = visible;
    }
}
