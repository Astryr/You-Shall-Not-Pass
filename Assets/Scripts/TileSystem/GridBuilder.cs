using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public class GridBuilder : MonoBehaviour
{
    // Cached — evita llamar GetComponent<NavMeshSurface>() en cada acceso
    private NavMeshSurface _navMesh;
    private NavMeshSurface myNavMesh
    {
        get
        {
            if (_navMesh == null) _navMesh = GetComponent<NavMeshSurface>();
            return _navMesh;
        }
    }

    [SerializeField] private GameObject mainPrefab;

    [SerializeField] private int gridLength = 10;
    [SerializeField] private int gridWidth = 10;

    [SerializeField] private List<GameObject> createdTiles;

    // Caché de TileSlot por tile — evita GetComponent por tile en cada llamada
    private List<TileSlot> cachedTileSlots;

    public List<GameObject> GetTileSetup() => createdTiles;
    public void UpdateNavMesh() => myNavMesh?.BuildNavMesh();

    private bool hadFirstLoad;

    private List<TileSlot> GetTileSlots()
    {
        if (cachedTileSlots == null || cachedTileSlots.Count != createdTiles.Count)
        {
            cachedTileSlots = new List<TileSlot>(createdTiles.Count);
            foreach (var tile in createdTiles)
                cachedTileSlots.Add(tile != null ? tile.GetComponent<TileSlot>() : null);
        }
        return cachedTileSlots;
    }

    public void DisableShadowsIfNeeded()
    {
        foreach (var slot in GetTileSlots())
            slot?.DisableShadowsIfNeeded();
    }

    public bool IsOnFirstLoad()
    {
        if (hadFirstLoad == false)
        {
            hadFirstLoad = true;
            return true;
        }

        return false;
    }

    [ContextMenu("Build grid")]
    private void BuildGrid()
    {
        ClearGrid();
        createdTiles = new List<GameObject>();

        for (int x = 0; x < gridLength; x++)
        {
            for (int z = 0; z < gridWidth; z++)
            {
                CreateTile(x,z);
            }
        }
    }

    [ContextMenu("Clear grid")]
    private void ClearGrid()
    {
        foreach (GameObject tile in createdTiles)
        {
            DestroyImmediate(tile);
        }

        createdTiles.Clear();
    }
    
    private void CreateTile(float xPosition,float zPosition)
    {
        Vector3 newPosition = new Vector3(xPosition, 0, zPosition);
        GameObject newTile = Instantiate(mainPrefab, newPosition, Quaternion.identity, transform);

        createdTiles.Add(newTile);

        newTile.GetComponent<TileSlot>().TurnIntoBuildSlotIfNeeded(mainPrefab);
    }

    public void MakeTilesNonInteractable(bool makeNonInteractable)
    {
        foreach (var slot in GetTileSlots())
            slot?.MakeNonInteractable(makeNonInteractable);
    }
}
