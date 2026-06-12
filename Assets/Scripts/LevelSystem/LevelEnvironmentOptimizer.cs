using UnityEngine;

// Apaga luces puntuales/spot (costosas). Mantiene la direccional para que el nivel se vea bien iluminado.
public static class LevelEnvironmentOptimizer
{
    public static void Apply()
    {
        Light mainLight = ConfigureMainDirectionalLight();
        DisableExpensiveLights();

        if (mainLight != null)
            RenderSettings.sun = mainLight;
    }

    private static Light ConfigureMainDirectionalLight()
    {
        Light[] lights = Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Light brightestDirectional = null;

        foreach (Light light in lights)
        {
            if (light == null || light.type != LightType.Directional)
                continue;

            if (!light.gameObject.activeInHierarchy)
                light.gameObject.SetActive(true);

            light.enabled = true;

#if UNITY_ANDROID && !UNITY_EDITOR
            light.shadows = LightShadows.None;
#endif

            if (brightestDirectional == null || light.intensity > brightestDirectional.intensity)
                brightestDirectional = light;
        }

        return brightestDirectional;
    }

    private static void DisableExpensiveLights()
    {
        Light[] lights = Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Light light in lights)
        {
            if (light == null || light.type == LightType.Directional)
                continue;

            light.enabled = false;
        }
    }
}
