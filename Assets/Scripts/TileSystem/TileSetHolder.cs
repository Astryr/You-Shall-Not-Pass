using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileSetHolder : MonoBehaviour
{
    public GameObject levelSelectTile;

    [Header("Common Tiles")]
    public GameObject tileRoad;
    public GameObject tileField;
    public GameObject tileSideway;

    [Header("Corners")]
    public GameObject tileInnerCorner;
    public GameObject tileInnerCornerSmall;
    public GameObject tileOuterCorner;
    public GameObject tileOuterCornerSmall;

    [Header("Hills")]
    public GameObject tileHill_1;
    public GameObject tileHill_2;
    public GameObject tileHill_3;

    [Header("Bridges")]
    public GameObject tileBridgeField;
    public GameObject tileBridgeRoad;
    public GameObject tileBridgeSideway;
}
