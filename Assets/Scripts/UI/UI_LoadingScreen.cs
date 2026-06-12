using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Pantalla de carga mostrada mientras se construye el nivel (animación de tiles + setup).
public class UI_LoadingScreen : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Image fillImage;

    private static readonly string[] StatusMessages =
    {
        "Preparando nivel...",
        "Construyendo mapa...",
        "Colocando defensas...",
        "Casi listo..."
    };

    private float _messageTimer;
    private int _messageIndex;

    private void Awake()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    private void Update()
    {
        if (panel == null || !panel.activeSelf || statusText == null)
            return;

        _messageTimer += Time.unscaledDeltaTime;
        if (_messageTimer < 1.2f)
            return;

        _messageTimer = 0f;
        _messageIndex = (_messageIndex + 1) % StatusMessages.Length;
        statusText.text = StatusMessages[_messageIndex];
    }

    public void Show(string message = null)
    {
        EnsureBuilt();

        _messageIndex = 0;
        _messageTimer = 0f;

        if (statusText != null)
            statusText.text = message ?? StatusMessages[0];

        if (fillImage != null)
            fillImage.fillAmount = 0.15f;

        panel.SetActive(true);
        transform.SetAsLastSibling();
    }

    public void SetProgress(float normalized)
    {
        if (fillImage != null)
            fillImage.fillAmount = Mathf.Clamp01(normalized);
    }

    public void Hide()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    private void EnsureBuilt()
    {
        if (panel != null)
            return;

        panel = new GameObject("LoadingPanel");
        panel.transform.SetParent(transform, false);

        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0.05f, 0.06f, 0.1f, 0.92f);
        bg.raycastTarget = true;

        GameObject textGo = new GameObject("LoadingText");
        textGo.transform.SetParent(panel.transform, false);
        RectTransform textRect = textGo.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.55f);
        textRect.anchorMax = new Vector2(0.5f, 0.55f);
        textRect.sizeDelta = new Vector2(600f, 80f);

        statusText = textGo.AddComponent<TextMeshProUGUI>();
        statusText.alignment = TextAlignmentOptions.Center;
        statusText.fontSize = 28;
        statusText.color = Color.white;
        statusText.text = StatusMessages[0];

        GameObject barBgGo = new GameObject("LoadingBarBg");
        barBgGo.transform.SetParent(panel.transform, false);
        RectTransform barBgRect = barBgGo.AddComponent<RectTransform>();
        barBgRect.anchorMin = new Vector2(0.5f, 0.42f);
        barBgRect.anchorMax = new Vector2(0.5f, 0.42f);
        barBgRect.sizeDelta = new Vector2(420f, 18f);
        Image barBg = barBgGo.AddComponent<Image>();
        barBg.color = new Color(1f, 1f, 1f, 0.15f);

        GameObject barFillGo = new GameObject("LoadingBarFill");
        barFillGo.transform.SetParent(barBgGo.transform, false);
        RectTransform barFillRect = barFillGo.AddComponent<RectTransform>();
        barFillRect.anchorMin = Vector2.zero;
        barFillRect.anchorMax = Vector2.one;
        barFillRect.offsetMin = Vector2.zero;
        barFillRect.offsetMax = Vector2.zero;

        fillImage = barFillGo.AddComponent<Image>();
        fillImage.color = new Color(0.85f, 0.75f, 0.2f, 1f);
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillAmount = 0.1f;
    }
}
