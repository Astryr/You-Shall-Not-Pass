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
#if UNITY_ANDROID && !UNITY_EDITOR
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
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
#endif
    }

}
