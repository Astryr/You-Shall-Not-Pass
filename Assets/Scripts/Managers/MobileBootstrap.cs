using System.Runtime;
using UnityEngine;

/// <summary>
/// Ajustes de rendimiento aplicados al iniciar en Android (TCL 408 y similares).
/// Se ejecuta antes de cargar escenas; no requiere estar en la escena.
/// </summary>
public static class MobileBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ApplySettings()
    {
        // Cap en 90 para que el dispositivo pueda superar 60fps si tiene capacidad,
        // y el contador siempre muestre valores reales (sin cap artificial en 60).
        Application.targetFrameRate = 90;
        QualitySettings.vSyncCount  = 0;

        // La carga en segundo plano con prioridad Low cede más tiempo al hilo principal,
        // reduciendo los spikes de FPS durante la carga de escenas. Se eleva a BelowNormal
        // una vez que la escena está activa (lo hace LevelManager).
        Application.backgroundLoadingPriority = ThreadPriority.BelowNormal;

        // Reducir iteraciones del solver de física (default 6 → 4):
        // conserva comportamiento de juego pero baja la carga de CPU por FixedUpdate.
        Physics.defaultSolverIterations         = 4;
        Physics.defaultSolverVelocityIterations = 1;

        // Limita el deltaTime reportado a scripts. Si un frame tarda 200ms (spike de carga),
        // Unity solo reporta 50ms a la física y animaciones, evitando explosiones de simulación.
        Time.maximumDeltaTime = 0.05f;

        // GC de baja latencia: el recolector de basura distribuye su trabajo entre frames
        // en lugar de pausar el juego con una colección completa ("stop-the-world").
        GCSettings.LatencyMode = GCLatencyMode.LowLatency;

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

        // MSAA consume mucha memoria de framebuffer en gama baja; lo desactivamos globalmente.
        QualitySettings.antiAliasing = 0;
#endif
    }

    // Se ejecuta después de la primera escena para configurar ajustes de cámara.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void ApplyCameraSettings()
    {
        // PhysicsRaycaster en la cámara principal: sin este componente el EventSystem
        // nunca llama OnPointerDown en objetos 3D (como los BuildSlot tiles del nivel).
        // Se añade aquí en runtime porque no está en la escena de forma estática.
        EnsurePhysicsRaycasterOnMainCamera();

#if UNITY_ANDROID && !UNITY_EDITOR
        Camera cam = Camera.main;
        if (cam != null)
        {
            // HDR requiere un render target de alta precisión (fp16/fp32), muy costoso en móvil.
            cam.allowHDR  = false;
            // MSAA en la cámara es redundante si ya está desactivado en QualitySettings.
            cam.allowMSAA = false;
        }
#endif
    }

    /// <summary>
    /// Agrega PhysicsRaycaster a la cámara principal si aún no lo tiene.
    /// Se puede llamar varias veces de forma segura (el check previene duplicados).
    /// </summary>
    public static void EnsurePhysicsRaycasterOnMainCamera()
    {
        Camera cam = Camera.main;
        if (cam != null && cam.GetComponent<UnityEngine.EventSystems.PhysicsRaycaster>() == null)
            cam.gameObject.AddComponent<UnityEngine.EventSystems.PhysicsRaycaster>();
    }
}
