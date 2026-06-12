using UnityEditor;
using UnityEngine;

public static class ConvertTileMaterialsToUnlit
{
    [MenuItem("Tools/Android Optimizer/7. Convert Tile Materials to Unlit")]
    public static void Convert()
    {
        Shader unlit = Shader.Find("Universal Render Pipeline/Unlit");
        if (unlit == null)
        {
            EditorUtility.DisplayDialog("Error", "No se encontró el shader URP Unlit.", "OK");
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:Material", new[] { "Assets/Graphics/Materials" });
        int count = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null) continue;

            string lower = mat.name.ToLower();
            if (!lower.Contains("tiles_mat") && !lower.Contains("main_mat"))
                continue;

            Color baseColor = mat.HasProperty("_BaseColor") ? mat.GetColor("_BaseColor") : Color.white;
            Texture baseMap = mat.HasProperty("_BaseMap") ? mat.GetTexture("_BaseMap") : null;

            mat.shader = unlit;
            mat.SetColor("_BaseColor", baseColor);
            if (baseMap != null)
                mat.SetTexture("_BaseMap", baseMap);

            EditorUtility.SetDirty(mat);
            count++;
        }

        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Materiales", $"{count} materiales de tiles convertidos a Unlit.", "OK");
    }
}
