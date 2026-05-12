using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(TileSlot)),CanEditMultipleObjects]

public class TileSlotEditor : Editor
{

    private GUIStyle centeredStlye;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        base.OnInspectorGUI();


        centeredStlye = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            fontSize = 14
        };


        float oneButtonWith = (EditorGUIUtility.currentViewWidth - 25);
        float twoButtonWidth = (EditorGUIUtility.currentViewWidth - 25) / 2;
        float threeButtonWidth = (EditorGUIUtility.currentViewWidth - 25) / 3;


        GUILayout.Label("Position and Rotation", centeredStlye);

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Rotate Left", GUILayout.Width(twoButtonWidth)))
        {
            foreach (var targetTile in targets)
            {
                ((TileSlot)targetTile).RotateTile(-1);
            }
        }

        if (GUILayout.Button("Rotate Right", GUILayout.Width(twoButtonWidth)))
        {
            foreach (var targetTile in targets)
            {
                ((TileSlot)targetTile).RotateTile(1);
            }
        }

        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("- .1f on the Y", GUILayout.Width(twoButtonWidth)))
        {
            foreach (var targetTile in targets)
            {
                ((TileSlot)targetTile).ADjustY(-1);
            }
        }

        if (GUILayout.Button("+ .1f on the Y", GUILayout.Width(twoButtonWidth)))
        {
            foreach (var targetTile in targets)
            {
                ((TileSlot)targetTile).ADjustY(1);
            }
        }

        GUILayout.EndHorizontal();

        GUILayout.Label("Tile Options", centeredStlye);

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Field", GUILayout.Width(twoButtonWidth)))
        {
            GameObject newTile = FindFirstObjectByType<TileSetHolder>().tileField;

            foreach (var targetTile in targets)
            {
                ((TileSlot)targetTile).SwitchTile(newTile);
            }
        }

        if (GUILayout.Button("Road", GUILayout.Width(twoButtonWidth)))
        {
            GameObject newTile = FindFirstObjectByType<TileSetHolder>().tileRoad;

            foreach (var targetTile in targets)
            {
                ((TileSlot)targetTile).SwitchTile(newTile);
            }
        }


        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Sideway", GUILayout.Width(oneButtonWith)))
        {
            GameObject newTile = FindFirstObjectByType<TileSetHolder>().tileSideway;

            foreach (var targetTile in targets)
            {
                ((TileSlot)targetTile).SwitchTile(newTile);
            }
        }

        GUILayout.EndHorizontal();

        GUILayout.Label("Corner Options", centeredStlye);


        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Inner Corner", GUILayout.Width(twoButtonWidth)))
        {
            GameObject newTile = FindFirstObjectByType<TileSetHolder>().tileInnerCorner;

            foreach (var targetTile in targets)
            {
                ((TileSlot)targetTile).SwitchTile(newTile);
            }
        }

        if (GUILayout.Button("Outer Corner", GUILayout.Width(twoButtonWidth)))
        {
            GameObject newTile = FindFirstObjectByType<TileSetHolder>().tileOuterCorner;

            foreach (var targetTile in targets)
            {
                ((TileSlot)targetTile).SwitchTile(newTile);
            }
        }


        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Small Inner Corner", GUILayout.Width(twoButtonWidth)))
        {
            GameObject newTile = FindFirstObjectByType<TileSetHolder>().tileInnerCornerSmall;

            foreach (var targetTile in targets)
            {
                ((TileSlot)targetTile).SwitchTile(newTile);
            }
        }

        if (GUILayout.Button("Small Outer Corner", GUILayout.Width(twoButtonWidth)))
        {
            GameObject newTile = FindFirstObjectByType<TileSetHolder>().tileOuterCornerSmall;

            foreach (var targetTile in targets)
            {
                ((TileSlot)targetTile).SwitchTile(newTile);
            }
        }


        GUILayout.EndHorizontal();

        GUILayout.Label("Bridges and Hills", centeredStlye);

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Hill 1", GUILayout.Width(threeButtonWidth)))
        {
            GameObject newTile = FindFirstObjectByType<TileSetHolder>().tileHill_1;

            foreach (var targetTile in targets)
            {
                ((TileSlot)targetTile).SwitchTile(newTile);
            }
        }

        if (GUILayout.Button("Hill 2", GUILayout.Width(threeButtonWidth)))
        {
            GameObject newTile = FindFirstObjectByType<TileSetHolder>().tileHill_2;

            foreach (var targetTile in targets)
            {
                ((TileSlot)targetTile).SwitchTile(newTile);
            }
        }

        if (GUILayout.Button("Hill 3", GUILayout.Width(threeButtonWidth)))
        {
            GameObject newTile = FindFirstObjectByType<TileSetHolder>().tileHill_3;

            foreach (var targetTile in targets)
            {
                ((TileSlot)targetTile).SwitchTile(newTile);
            }
        }


        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Bridge with field", GUILayout.Width(threeButtonWidth)))
        {
            GameObject newTile = FindFirstObjectByType<TileSetHolder>().tileBridgeField;

            foreach (var targetTile in targets)
            {
                ((TileSlot)targetTile).SwitchTile(newTile);
            }
        }

        if (GUILayout.Button("Bridge with road", GUILayout.Width(threeButtonWidth)))
        {
            GameObject newTile = FindFirstObjectByType<TileSetHolder>().tileBridgeRoad;

            foreach (var targetTile in targets)
            {
                ((TileSlot)targetTile).SwitchTile(newTile);
            }
        }

        if (GUILayout.Button("Bridge with sideway", GUILayout.Width(threeButtonWidth)))
        {
            GameObject newTile = FindFirstObjectByType<TileSetHolder>().tileBridgeSideway;

            foreach (var targetTile in targets)
            {
                ((TileSlot)targetTile).SwitchTile(newTile);
            }
        }


        GUILayout.EndHorizontal();

        GUILayout.Label("Level Button", centeredStlye);

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Level Button Tile", GUILayout.Width(oneButtonWith)))
        {
            GameObject newTile = FindFirstObjectByType<TileSetHolder>().levelSelectTile;

            foreach (var targetTile in targets)
            {
                ((TileSlot)targetTile).SwitchTile(newTile);
            }
        }

        GUILayout.EndHorizontal();
    }
}
