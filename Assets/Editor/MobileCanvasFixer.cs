using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class MobileCanvasFixer
{
    [MenuItem("Tools/Mobile/Ajustar Escalado de Canvas (TCL 408)")]
    public static void FixCanvasScalers()
    {
        // Encuentra todos los CanvasScaler en la escena actual (incluso inactivos)
        CanvasScaler[] scalers = Resources.FindObjectsOfTypeAll<CanvasScaler>();
        
        int count = 0;
        foreach (var scaler in scalers)
        {
            // Omitir prefabs en el proyecto para no modificar los originales sin querer, 
            // aunque puedes cambiar Resources.FindObjectsOfTypeAll si prefieres afectar toda la carpeta
            if (EditorUtility.IsPersistent(scaler.gameObject)) continue;

            Undo.RecordObject(scaler, "Ajustar Escalado de Canvas");
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(720, 1600); // Resolución del TCL 408
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f; // Balance perfecto entre ancho y alto
            EditorUtility.SetDirty(scaler);
            count++;
        }
        
        Debug.Log($"Se han ajustado {count} Canvas Scalers para el dispositivo móvil (Resolución: 720x1600).");
    }
}