using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

/// <summary>
/// Herramientas de optimización para Android.
/// Menú: Tools → Android Optimizer
/// Correr ANTES de hacer la build de Android.
/// </summary>
public static class AndroidOptimizer
{
    // ─── Texturas ────────────────────────────────────────────────────────────

    [MenuItem("Tools/Android Optimizer/1. Optimize UI Textures for Android")]
    public static void OptimizeUITextures()
    {
        // Rutas de texturas propias del proyecto (no assets de terceros)
        string[] searchFolders =
        {
            "Assets/Graphics/UI",
        };

        string[] guids = AssetDatabase.FindAssets("t:Texture2D", searchFolders);
        int count = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) continue;

            bool changed = false;

            // DefaultTexturePlatform: forzar compresión si estaba en None
            TextureImporterPlatformSettings def = importer.GetDefaultPlatformTextureSettings();
            if (def.textureCompression == TextureImporterCompression.Uncompressed)
            {
                def.textureCompression = TextureImporterCompression.Compressed;
                importer.SetPlatformTextureSettings(def);
                changed = true;
            }

            // Android override
            TextureImporterPlatformSettings android = importer.GetPlatformTextureSettings("Android");
            bool needsOverride = !android.overridden
                || android.maxTextureSize > 512
                || android.format != TextureImporterFormat.ETC2_RGBA8;

            if (needsOverride)
            {
                android.name               = "Android";
                android.overridden         = true;
                // Sprites UI: máx 512 (más que suficiente en 720p)
                android.maxTextureSize     = 512;
                // ETC2 RGBA8 soportado en todos los Android con OpenGL ES 3.0+ (Android 4.3+)
                android.format             = TextureImporterFormat.ETC2_RGBA8;
                android.textureCompression = TextureImporterCompression.Compressed;
                android.compressionQuality = 50;
                importer.SetPlatformTextureSettings(android);
                changed = true;
            }

            if (changed)
            {
                EditorUtility.SetDirty(importer);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                count++;
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[AndroidOptimizer] Texturas UI optimizadas: {count} archivos actualizados.");
        EditorUtility.DisplayDialog("Texturas UI", $"{count} texturas actualizadas para Android (ETC2, max 512px).", "OK");
    }

    // ─── Audio ───────────────────────────────────────────────────────────────

    [MenuItem("Tools/Android Optimizer/2. Optimize Audio for Android")]
    public static void OptimizeAudio()
    {
        OptimizeAudioFolder("Assets/Audio/Bgm", loadType: AudioClipLoadType.Streaming, forceToMono: false);
        OptimizeAudioFolder("Assets/Audio/Sfx", loadType: AudioClipLoadType.DecompressOnLoad, forceToMono: true);

        AssetDatabase.SaveAssets();
        Debug.Log("[AndroidOptimizer] Audio optimizado: BGM → Streaming, SFX → DecompressOnLoad + Mono.");
        EditorUtility.DisplayDialog("Audio", "BGM: Streaming Vorbis.\nSFX: DecompressOnLoad Vorbis Mono.", "OK");
    }

    private static void OptimizeAudioFolder(string folder, AudioClipLoadType loadType, bool forceToMono)
    {
        string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { folder });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AudioImporter importer = AssetImporter.GetAtPath(path) as AudioImporter;
            if (importer == null) continue;

            bool changed = false;

            // Configuración por defecto
            AudioImporterSampleSettings def = importer.defaultSampleSettings;
            if (def.loadType != loadType || def.compressionFormat != AudioCompressionFormat.Vorbis)
            {
                def.loadType         = loadType;
                def.compressionFormat = AudioCompressionFormat.Vorbis;
                def.quality          = 0.5f;
                importer.defaultSampleSettings = def;
                changed = true;
            }

            // Forzar mono en SFX (ahorra ~50% de memoria en clips estéreo)
            if (importer.forceToMono != forceToMono)
            {
                importer.forceToMono = forceToMono;
                changed = true;
            }

            // Override específico para Android
            AudioImporterSampleSettings android = importer.GetOverrideSampleSettings("Android");
            bool hasAndroidOverride = importer.ContainsSampleSettingsOverride("Android");

            if (!hasAndroidOverride
                || android.loadType != loadType
                || android.compressionFormat != AudioCompressionFormat.Vorbis)
            {
                android.loadType          = loadType;
                android.compressionFormat = AudioCompressionFormat.Vorbis;
                android.quality           = 0.5f;
                importer.SetOverrideSampleSettings("Android", android);
                changed = true;
            }

            if (changed)
            {
                EditorUtility.SetDirty(importer);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }
        }
    }

    // ─── Objetos estáticos ────────────────────────────────────────────────────

    [MenuItem("Tools/Android Optimizer/3. Mark Scene Terrain+Tiles as Static")]
    public static void MarkSceneObjectsStatic()
    {
        int count = 0;
        // Marca como Contributor + Receiver de Occlusion/GI todo lo que tenga "Tile" o "Ground" en el nombre
        // y no sea un prefab de enemigo / torre.
        GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);

        foreach (GameObject go in allObjects)
        {
            if (go == null) continue;

            string name = go.name.ToLower();
            bool isTerrain = name.Contains("tile") || name.Contains("ground")
                             || name.Contains("wall") || name.Contains("castle")
                             || name.Contains("path")  || name.Contains("floor");

            bool isDynamic = name.Contains("enemy") || name.Contains("tower")
                             || name.Contains("projectile") || name.Contains("portal");

            if (isTerrain && !isDynamic && go.isStatic == false)
            {
                GameObjectUtility.SetStaticEditorFlags(go,
                    StaticEditorFlags.ContributeGI
                    | StaticEditorFlags.OccluderStatic
                    | StaticEditorFlags.OccludeeStatic
                    | StaticEditorFlags.BatchingStatic);
                count++;
            }
        }

        Debug.Log($"[AndroidOptimizer] {count} objetos marcados como Static.");
        EditorUtility.DisplayDialog("Static Flags",
            $"{count} objetos marcados. Ahora:\n1. Window → Rendering → Occlusion Culling → Bake\n2. Window → Rendering → Lighting → Generate Lighting",
            "OK");
    }

    // ─── Sprite Atlas ─────────────────────────────────────────────────────────

    [MenuItem("Tools/Android Optimizer/4. Create Tower Icons Sprite Atlas")]
    public static void CreateTowerIconsAtlas()
    {
        const string atlasPath = "Assets/Graphics/UI/Tower_Icon/TowerIcons_Atlas.spriteatlas";

        SpriteAtlas atlas = new SpriteAtlas();

        // Configuración del atlas
        SpriteAtlasPackingSettings packSettings = new SpriteAtlasPackingSettings
        {
            blockOffset  = 1,
            enableRotation = false,
            enableTightPacking = false,
            padding      = 4
        };
        atlas.SetPackingSettings(packSettings);

        SpriteAtlasTextureSettings texSettings = new SpriteAtlasTextureSettings
        {
            readable     = false,
            generateMipMaps = false,
            sRGB         = true,
            filterMode   = FilterMode.Bilinear
        };
        atlas.SetTextureSettings(texSettings);

        // Override Android
        TextureImporterPlatformSettings androidSettings = new TextureImporterPlatformSettings
        {
            name               = "Android",
            overridden         = true,
            maxTextureSize     = 1024,
            format             = TextureImporterFormat.ETC2_RGBA8,
            textureCompression = TextureImporterCompression.Compressed,
            compressionQuality = 50
        };
        atlas.SetPlatformSettings(androidSettings);

        // Agregar carpeta de iconos como fuente
        Object folder = AssetDatabase.LoadAssetAtPath<Object>("Assets/Graphics/UI/Tower_Icon");
        if (folder != null)
            atlas.Add(new Object[] { folder });

        AssetDatabase.CreateAsset(atlas, atlasPath);
        AssetDatabase.SaveAssets();

        Debug.Log($"[AndroidOptimizer] Sprite Atlas creado en: {atlasPath}");
        EditorUtility.DisplayDialog("Sprite Atlas",
            $"Atlas creado en:\n{atlasPath}\n\nAbrilo en el Inspector para verificar que los sprites están incluidos.\nLuego: Edit → Project Settings → Editor → Sprite Packer Mode = Always Enabled.",
            "OK");
    }

    // ─── Correr todo junto ────────────────────────────────────────────────────

    [MenuItem("Tools/Android Optimizer/0. Run ALL Optimizations")]
    public static void RunAll()
    {
        OptimizeUITextures();
        OptimizeAudio();
        CreateTowerIconsAtlas();

        Debug.Log("[AndroidOptimizer] Todas las optimizaciones aplicadas. " +
                  "Recordá hacer el bake de Occlusion Culling y Lighting manualmente desde el Editor.");
    }
}
