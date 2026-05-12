using UnityEngine;

public class UI_Hyperlink : MonoBehaviour
{
    [SerializeField] private string url;

    public void OpenURL() => Application.OpenURL(url);
}
