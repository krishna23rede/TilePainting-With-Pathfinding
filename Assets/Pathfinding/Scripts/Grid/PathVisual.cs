using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PathVisual : MonoBehaviour
{
    public enum HighlightMode
    {
        Click,
        Hover
    }

    public HighlightMode currentMode = HighlightMode.Hover;
    public Color highlightColor = Color.yellow;
    public Color defaultColor = Color.white;

    private List<Renderer> highlightedTiles = new List<Renderer>();
    private Grid grid;

    private void Start()
    {
        grid = FindObjectOfType<Grid>(); // Automatically find the Grid script in the scene
    }

    public void HighlightPathTo(Vector2Int gridPosition, Vector2Int start = default)
    {
        ClearHighlight();

        if (grid == null)
        {
            Debug.LogWarning("Grid is null.");
            return;
        }

        if(start == Vector2Int.zero)
        {
            Vector2Int playerPosition = grid.GetGridCoordinateFromPosition(grid.Player.transform.position);
            start = playerPosition;
        }
        List<Vector3> path = grid.pathfinding.FindPath(start, gridPosition);

        if (path != null)
        {
            foreach (Vector3 position in path)
            {
                Vector2Int gridPos = grid.GetGridCoordinateFromPosition(position);
                Tile tile = GetTileAt(gridPos);
                if (tile != null)
                {
                    Renderer tileRenderer = tile.GetComponent<Renderer>();
                    if (tileRenderer != null)
                    {
                        tileRenderer.material.color = highlightColor;
                        highlightedTiles.Add(tileRenderer);
                    }
                }
            }
        }
    }

    public void ClearHighlight()
    {
        foreach (Renderer renderer in highlightedTiles)
        {
            if (renderer != null)
            {
                renderer.material.color = defaultColor;
            }
        }
        highlightedTiles.Clear();
    }

    private Tile GetTileAt(Vector2Int position)
    {
        foreach (Transform child in grid.parentObject.transform)
        {
            Tile childTile = child.GetComponent<Tile>();
            if (childTile != null && childTile.gridCoordinates == position)
            {
                return childTile;
            }
        }
        return null;
    }
}