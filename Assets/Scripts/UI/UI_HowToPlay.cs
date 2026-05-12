using TMPro;
using UnityEngine;

// Panel "Cómo Jugar" que vive dentro de MainMenu_UI.
// El botón HowToPlay_BTN llama a Show(); el botón Volver llama a Hide().
public class UI_HowToPlay : MonoBehaviour
{
    [Header("Textos configurables desde el Inspector")]
    [TextArea(2, 5)]
    [SerializeField] private string objectiveText =
        "Enemigos avanzan por el camino hacia tu castillo.\n¡Construí torres para detenerlos antes de que lleguen!";

    [TextArea(2, 5)]
    [SerializeField] private string controlsText =
        "PC: Click izquierdo en una CASILLA → elegí una torre → construí.\n" +
        "Mobile: Tocá una casilla → elegí una torre → confirmá.\n" +
        "Zoom: Rueda del mouse / Pellizco con dos dedos.";

    [TextArea(2, 5)]
    [SerializeField] private string tipText =
        "Cada enemigo derrotado te da recursos para construir más torres.\n" +
        "¡Sobreviví todas las oleadas para ganar!";

    [Header("Referencias UI (asignar en Inspector)")]
    [SerializeField] private TextMeshProUGUI objectiveTMP;
    [SerializeField] private TextMeshProUGUI controlsTMP;
    [SerializeField] private TextMeshProUGUI tipTMP;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    /// <summary>Llamado por HowToPlay_BTN. Muestra el panel sobre el menú principal.</summary>
    public void Show()
    {
        PopulateTexts();
        gameObject.SetActive(true);
    }

    /// <summary>Llamado por el botón "Volver" dentro del panel.</summary>
    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void PopulateTexts()
    {
        if (objectiveTMP != null) objectiveTMP.text = objectiveText;
        if (controlsTMP  != null) controlsTMP.text  = controlsText;
        if (tipTMP       != null) tipTMP.text        = tipText;
    }
}
