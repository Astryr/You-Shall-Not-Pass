using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    [SerializeField] private Image fadeImageUI;
    [SerializeField] private GameObject[] uiElements;

    private UI_Settings settingsUI;
    private UI_MainMenu mainMenuUI;


    public UI_InGame inGameUI { get; private set; }
    public UI_Animator uiAnim { get; private set; }
    public UI_BuildButtonsHolder buildButtonsUI { get; private set; }

    [Header("UI SFX")]
    public AudioSource onHoverSfx;
    public AudioSource onClickSfx;

    private List<(GameObject go, bool wasActive)> _canvasRootsHiddenForEndGame;

    private void Awake()
    {
        FixCanvasScalersForMobile();

        buildButtonsUI = GetComponentInChildren<UI_BuildButtonsHolder>(true);
        settingsUI = GetComponentInChildren<UI_Settings>(true);
        mainMenuUI = GetComponentInChildren<UI_MainMenu>(true);
        inGameUI = GetComponentInChildren<UI_InGame>(true);
        uiAnim = GetComponent<UI_Animator>();

        ActivateFadeEffect(true);

        SwitchTo(settingsUI.gameObject);
        SwitchTo(mainMenuUI.gameObject);

        if (GameManager.instance != null && GameManager.instance.IsTestingLevel())
            SwitchTo(inGameUI.gameObject);
    }

    private void FixCanvasScalersForMobile()
    {
        CanvasScaler[] scalers = GetComponentsInChildren<CanvasScaler>(true);
        foreach (var scaler in scalers)
        {
            if (scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(720, 1600); // TCL 408 default assumption, adjusts automatically
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
            }
        }
    }


    public void SwitchTo(GameObject uiToEnable)
    {
        foreach (GameObject ui in uiElements)
        {
            ui.SetActive(false);
        }

        if(uiToEnable != null) 
            uiToEnable.SetActive(true);
    }

    public void EnableMainMenuUI(bool enable)
    {
        if (enable)
            SwitchTo(mainMenuUI.gameObject);
        else
            SwitchTo(null);
    }

    public void EnableInGameUI(bool enable)
    {
        if(enable)
        {
            ClearCanvasIsolationForEndGameScreen();
            SwitchTo(inGameUI.gameObject);
            inGameUI.ResetToGameplayLayout();
        }
        else
        {
            inGameUI.SnapTimerToDefaultPosition();
            SwitchTo(null);
        }
    }

    /// <summary>Torres + panel de pausa (Canvas). Se oculta en game over / victoria para dejar solo el overlay.</summary>
    public void SetBuildAndPauseChromeVisible(bool visible)
    {
        if (buildButtonsUI != null)
            buildButtonsUI.gameObject.SetActive(visible);

        UI_Pause pause = GetComponentInChildren<UI_Pause>(true);
        if (pause != null)
        {
            if (!visible)
                pause.gameObject.SetActive(false);
        }
    }

    /// <summary>Oculta otros hijos del Canvas (fade, menús residuales, SFX se mantiene para sonidos del overlay).</summary>
    public void ApplyCanvasIsolationForEndGameScreen()
    {
        if (_canvasRootsHiddenForEndGame != null)
            return;

        _canvasRootsHiddenForEndGame = new List<(GameObject, bool)>();
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject ch = transform.GetChild(i).gameObject;
            if (ch == inGameUI.gameObject)
                continue;
            if (ch.name == "UI_SFX")
                continue;

            _canvasRootsHiddenForEndGame.Add((ch, ch.activeSelf));
            ch.SetActive(false);
        }
    }

    public void ClearCanvasIsolationForEndGameScreen()
    {
        if (_canvasRootsHiddenForEndGame == null)
            return;

        foreach (var entry in _canvasRootsHiddenForEndGame)
        {
            if (entry.go != null)
                entry.go.SetActive(entry.wasActive);
        }

        _canvasRootsHiddenForEndGame = null;
    }

    public void QuitButton()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void ActivateFadeEffect(bool fadeIn)
    {
        if (fadeImageUI.gameObject.activeSelf == false)
            return;

        if (fadeIn)
            uiAnim.ChangeColor(fadeImageUI, 0, 1.5f);
        else
            uiAnim.ChangeColor(fadeImageUI, 1, 1.5f);
    }
}
