using TMPro;
using UnityEngine;

public class UI_InGame : MonoBehaviour
{
    private UI ui;
    private UI_Pause pauseUI;
    private UI_Animator uiAnimator;


    [SerializeField] private TextMeshProUGUI healthPointsText;
    [SerializeField] private TextMeshProUGUI currencyText;
    [Space]
    [SerializeField] private TextMeshProUGUI waveTimerText;
    [SerializeField] private float waveTimerOffset;
    [SerializeField] UI_TextBlinkEffect waveTimerTextBlinkEffect;

    [SerializeField] private Transform waveTimer;
    private Coroutine waveTimerMoveCo;
    private Vector3 waveTimerDefaultPosition;

    [Header("Victory & Defeat")]
    [SerializeField] private GameObject victoryUI;
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private GameObject levelCompletedUI;

    private void Awake()
    {
        uiAnimator = GetComponentInParent<UI_Animator>();
        ui = GetComponentInParent<UI>();
        pauseUI = ui.GetComponentInChildren<UI_Pause>(true);

        if (waveTimer != null)
            waveTimerDefaultPosition = waveTimer.localPosition;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F10))
            ui.SwitchTo(pauseUI.gameObject);
    }

    public void EnableGameOverUI(bool enable)
    {
        if(gameOverUI != null)
            gameOverUI.SetActive(enable);
    }
    public void EnableVictoryUI(bool enable)
    {
        if(victoryUI != null)
            victoryUI.SetActive(enable);
    }

    public void EnableLevelCompletedUI(bool enable)
    {
        if(levelCompletedUI != null) 
            levelCompletedUI.SetActive(enable);
    }

    

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

    public void UpdateWaveTimerUI(float value) => waveTimerText.text = "seconds : " + value.ToString("00");
    public void EnableWaveTimer(bool enable)
    {
        RectTransform rect = waveTimer.GetComponent<RectTransform>();
        float yOffset = enable ? -waveTimerOffset : waveTimerOffset;

        Vector3 offset = new Vector3(0, yOffset);


        if (uiAnimator == null)
            uiAnimator = GetComponentInParent<UI_Animator>();


        waveTimerMoveCo = StartCoroutine(uiAnimator.ChangePositionCo(rect, offset));
        waveTimerTextBlinkEffect.EnableBlink(enable);
    }

    public void SnapTimerToDefaultPosition()
    {
        if (waveTimer == null)
            return;

        if (waveTimerMoveCo != null)
            StopCoroutine(waveTimerMoveCo);

        waveTimer.localPosition = waveTimerDefaultPosition;
    }

    public void ForceWaveButton()
    {
        WaveManager waveManager = FindFirstObjectByType<WaveManager>();
        waveManager.StartNewWave();
    }
}
