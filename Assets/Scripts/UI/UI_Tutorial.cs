using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Overlay de instrucciones que aparece automaticamente la primera vez que el jugador entra a un nivel.
// Puede reabrirse con el boton de ayuda (?) del HUD durante la partida.
public class UI_Tutorial : MonoBehaviour
{
    private const string TutorialShownKey = "tutorial_shown_v1";

    [Header("Textos configurables desde el Inspector")]
    [TextArea(2, 5)]
    [SerializeField] private string objectiveText =
        "Enemigos avanzan por el camino hacia tu castillo.\n¡Construi torres para detenerlos antes de que lleguen!";

    [TextArea(2, 5)]
    [SerializeField] private string controlsText =
        "Tocá una casilla del mapa para abrir el menú de torres.\n" +
        "Un dedo: mover cámara. Dos dedos: zoom.\n" +
        "Tocá FORCE WAVE para iniciar cada oleada.";

    [TextArea(2, 5)]
    [SerializeField] private string tipText =
        "Cada enemigo derrotado da recursos para más torres.\n" +
        "Si muchos enemigos llegan al castillo, perdés.\n" +
        "Podés reabrir esta ayuda con el botón ? del HUD.";

    [Header("Referencias UI (asignar en Inspector)")]
    [SerializeField] private TextMeshProUGUI objectiveTMP;
    [SerializeField] private TextMeshProUGUI controlsTMP;
    [SerializeField] private TextMeshProUGUI tipTMP;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    /// <summary>Muestra el tutorial al entrar al Nivel Tutorial (sin pausar).</summary>
    public void ShowOnLevelStart()
    {
        PopulateTexts();
        gameObject.SetActive(true);
    }

    /// <summary>Primera vez global (reservado para otros flujos).</summary>
    public void ShowIfFirstTime()
    {
        if (!PlayerPrefs.HasKey(TutorialShownKey))
            ShowOnLevelStart();
    }

    /// <summary>Apertura manual (botón ? del HUD): pausa el juego mientras se lee.</summary>
    public void Show()
    {
        PopulateTexts();
        gameObject.SetActive(true);
        Time.timeScale = 0f;
    }

    /// <summary>El botón "Entendido!" llama a esto.</summary>
    public void Hide()
    {
        PlayerPrefs.SetInt(TutorialShownKey, 1);
        Time.timeScale = 1f;
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
        gameObject.SetActive(false);
    }

    private void PopulateTexts()
    {
        if (objectiveTMP != null) objectiveTMP.text = objectiveText;
        if (controlsTMP != null) controlsTMP.text  = controlsText;
        if (tipTMP != null)      tipTMP.text        = tipText;
    }
}
