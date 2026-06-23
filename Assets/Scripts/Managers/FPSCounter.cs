using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Contador de FPS persistente en toda la aplicación.
/// Se auto-inicializa al arrancar el juego; no requiere ser colocado en ninguna escena.
/// Aparece en el centro-derecha de la pantalla con fondo semitransparente.
/// Verde ≥60 fps | Amarillo 45-59 fps | Rojo <45 fps
/// </summary>
public class FPSCounter : MonoBehaviour
{
    private TextMeshProUGUI label;
    private float elapsed;
    private int frameCount;

    private const float UpdateInterval = 0.5f;

    // -------------------------------------------------------------------------
    // Auto-inicialización: se ejecuta una vez al cargar la primera escena.
    // -------------------------------------------------------------------------
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Init()
    {
        var go = new GameObject("[FPSCounter]");
        DontDestroyOnLoad(go);
        go.AddComponent<FPSCounter>();
    }

    // -------------------------------------------------------------------------
    // Construcción de la UI procedural
    // -------------------------------------------------------------------------
    private void Awake()
    {
        BuildUI();
    }

    private void BuildUI()
    {
        // Canvas dedicado que siempre queda sobre todo lo demás
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;

        // Escala con la pantalla usando la resolución landscape de referencia del proyecto
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600f, 720f);
        scaler.matchWidthOrHeight = 0.5f;

        // Panel de fondo oscuro semitransparente (mejora legibilidad sobre cualquier color)
        var panel = new GameObject("BG");
        panel.transform.SetParent(transform, false);

        var bg = panel.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.50f);
        bg.raycastTarget = false;

        var panelRect = panel.GetComponent<RectTransform>();
        // Centro-derecha de la pantalla
        panelRect.anchorMin = new Vector2(1f, 0.5f);
        panelRect.anchorMax = new Vector2(1f, 0.5f);
        panelRect.pivot     = new Vector2(1f, 0.5f);
        panelRect.anchoredPosition = new Vector2(-8f, 0f);
        panelRect.sizeDelta = new Vector2(100f, 34f);

        // Texto TMP dentro del panel
        var textGo = new GameObject("Label");
        textGo.transform.SetParent(panel.transform, false);

        label = textGo.AddComponent<TextMeshProUGUI>();
        label.text      = "FPS --";
        label.fontSize  = 19f;
        label.fontStyle = FontStyles.Bold;
        label.color     = Color.green;
        label.alignment = TextAlignmentOptions.Center;
        label.raycastTarget = false;

        // Contorno negro para que sea legible sobre cualquier fondo
        label.outlineWidth = 0.18f;
        label.outlineColor = new Color32(0, 0, 0, 200);

        var textRect = textGo.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(4f, 2f);
        textRect.offsetMax = new Vector2(-4f, -2f);
    }

    // -------------------------------------------------------------------------
    // Cálculo y actualización de FPS (unscaledDeltaTime: funciona en pausa)
    // -------------------------------------------------------------------------
    private void Update()
    {
        frameCount++;
        elapsed += Time.unscaledDeltaTime;

        if (elapsed < UpdateInterval)
            return;

        int fps = Mathf.RoundToInt(frameCount / elapsed);

        label.text  = $"FPS {fps}";
        label.color = fps >= 60 ? Color.green
                    : fps >= 45 ? Color.yellow
                    :             Color.red;

        frameCount = 0;
        elapsed    = 0f;
    }
}
