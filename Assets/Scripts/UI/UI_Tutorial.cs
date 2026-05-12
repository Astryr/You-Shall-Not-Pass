using TMPro;
using UnityEngine;
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
        "Tocá una CASILLA del mapa → se abre el menú de torres.\nElegí una torre y confirmá para construirla.";

    [TextArea(2, 5)]
    [SerializeField] private string tipText =
        "Cada enemigo derrotado te da recursos para construir más torres.\n¡Sobreviví todas las oleadas para ganar!";

    [Header("Referencias UI (asignar en Inspector)")]
    [SerializeField] private TextMeshProUGUI objectiveTMP;
    [SerializeField] private TextMeshProUGUI controlsTMP;
    [SerializeField] private TextMeshProUGUI tipTMP;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    /// <summary>Llama esto al iniciar el nivel; muestra el tutorial solo si nunca se vio.</summary>
    public void ShowIfFirstTime()
    {
        if (!PlayerPrefs.HasKey(TutorialShownKey))
            Show();
    }

    /// <summary>Fuerza apertura (botón ? del HUD).</summary>
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
        gameObject.SetActive(false);
    }

    private void PopulateTexts()
    {
        if (objectiveTMP != null) objectiveTMP.text = objectiveText;
        if (controlsTMP != null) controlsTMP.text  = controlsText;
        if (tipTMP != null)      tipTMP.text        = tipText;
    }
}
