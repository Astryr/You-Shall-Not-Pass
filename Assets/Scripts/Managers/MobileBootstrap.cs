using UnityEngine;

/// <summary>
/// Ajustes de rendimiento aplicados al iniciar en Android (TCL 408 y similares).
/// Se ejecuta antes de cargar escenas; no requiere estar en la escena.
/// </summary>
public static class MobileBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ApplyAndroidSettings()
    {
        // Cap en 90 para que el dispositivo pueda superar 60fps si tiene capacidad,
        // y el contador siempre muestre valores reales (sin cap artificial en 60).
        Application.targetFrameRate = 90;
        QualitySettings.vSyncCount  = 0;

        // Reducir iteraciones del solver de física (default 6 → 4):
        // conserva comportamiento de juego pero baja la carga de CPU por FixedUpdate.
        Physics.defaultSolverIterations         = 4;
        Physics.defaultSolverVelocityIterations = 1;

#if UNITY_ANDROID && !UNITY_EDITOR
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        UnityEngine.Android.AndroidDevice.SetSustainedPerformanceMode(true);

        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;
        Screen.orientation = ScreenOrientation.LandscapeRight;

        // Performant = índice 0 en QualitySettings del proyecto.
        if (QualitySettings.GetQualityLevel() != 0)
            QualitySettings.SetQualityLevel(0, applyExpensiveChanges: true);

        // Reducir distancia de sombras en Android para aliviar GPU.
        QualitySettings.shadowDistance = 15f;

        // LOD más agresivo en móvil: reduce polígonos a mayor distancia.
        QualitySettings.lodBias = 0.7f;
#endif
    }

}
