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
            SwitchTo(inGameUI.gameObject);
        else
        {
            inGameUI.SnapTimerToDefaultPosition();
            SwitchTo(null);
        }
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
