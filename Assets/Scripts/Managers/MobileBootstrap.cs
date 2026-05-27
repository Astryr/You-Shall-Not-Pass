using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

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

        // Performant = índice 0 en QualitySettings del proyecto.
        if (QualitySettings.GetQualityLevel() != 0)
            QualitySettings.SetQualityLevel(0, applyExpensiveChanges: true);
#endif
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void ApplyAndroidPostProcessing()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        Volume[] volumes = Object.FindObjectsByType<Volume>(FindObjectsSortMode.None);
        foreach (Volume volume in volumes)
        {
            if (volume.profile != null && volume.profile.TryGet(out Bloom bloom))
                bloom.active = false;
        }
#endif
    }
}
