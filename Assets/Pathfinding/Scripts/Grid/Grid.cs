using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public GameObject parentObject;
    public Color gridColor = Color.green; // Color for the grid lines
    private List<Transform> childTransforms = new List<Transform>();
    private Bounds gridBounds;
    public float gridSpacing;
    public GameObject Player;
    public Dictionary<Vector2Int, Vector3> gridIndex = new Dictionary<Vector2Int, Vector3>();
    public MyPathfInding pathfinding;
    [SerializeField] private float MoveSpeed = 5f;

    void Start()
    {
        if (parentObject == null)
        {
            Debug.LogError("Parent Object is not assigned.");
            return;
        }

        // Get all child transforms
        foreach (Transform child in parentObject.transform)
        {
            childTransforms.Add(child);
            // Ensure the child has a ChildTile component
            if (child.GetComponent<Tile>() == null)
            {
                child.gameObject.AddComponent<Tile>();
            }
        }

        if (childTransforms.Count == 0)
        {
            Debug.LogWarning("No child GameObjects found.");
            return;
        }

        // Calculate the bounds and grid spacing
        CalculateBoundsAndSpacing();
        GenerateGridIndex();
        AssignGridCoordinatesToChildren();
    }

    void CalculateBoundsAndSpacing()
    {
        // Initialize bounds with the first child's position
        Vector3 min = childTransforms[0].position;
        Vector3 max = min;

        foreach (Transform child in childTransforms)
        {
            Vector3 position = child.position;
            min = Vector3.Min(min, position);
            max = Vector3.Max(max, position);
        }

        // Calculate grid bounds
        gridBounds = new Bounds((min + max) / 2, max - min);

        // Assuming all children have the same scale, use the scale of the first child for spacing
        Vector3 firstChildScale = childTransforms[0].localScale;
        gridSpacing = Mathf.Max(firstChildScale.x, firstChildScale.y, firstChildScale.z);

        //Debug.Log($"Grid Bounds: Center = {gridBounds.center}, Size = {gridBounds.size}, Grid Spacing = {gridSpacing}");
    }

    void GenerateGridIndex()
    {
        gridIndex.Clear(); // Clear previous grid indices

        Vector3 min = gridBounds.min;
        Vector3 max = gridBounds.max;

        // Determine the number of cells in each dimension
        int xCount = Mathf.CeilToInt((max.x - min.x) / gridSpacing);
        int zCount = Mathf.CeilToInt((max.z - min.z) / gridSpacing);

        // Iterate through possible grid positions
        for (int x = 0; x <= xCount; x++)
        {
            for (int z = 0; z <= zCount; z++)
            {
                Vector3 position = min + new Vector3(x * gridSpacing, 0, z * gridSpacing);

                // Check if a tile is present at this position
                if (IsTileAtPosition(position))
                {
                    gridIndex[new Vector2Int(x, z)] = position;
                }
            }
        }

        //Debug.Log($"Total Grid Nodes: {gridIndex.Count}");
    }

    bool IsTileAtPosition(Vector3 position)
    {
        foreach (Transform child in childTransforms)
        {
            if (Vector3.Distance(child.position, position) < gridSpacing * 0.5f)
            {
                return true;
            }
        }
        return false;
    }

    void AssignGridCoordinatesToChildren()
    {
        foreach (Transform child in childTransforms)
        {
            foreach (var kvp in gridIndex)
            {
                if (Vector3.Distance(child.position, kvp.Value) < gridSpacing * 0.5f)
                {
                    Tile childTile = child.GetComponent<Tile>();
                    childTile.SetGridCoordinates(kvp.Key);
                    //Debug.Log($"Assigned coordinates ({kvp.Key.x}, {kvp.Key.y}) to child {child.name}");
                    break;
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        if (childTransforms.Count == 0) return;

        Gizmos.color = gridColor;

        foreach (var kvp in gridIndex)
        {
            Gizmos.DrawWireCube(kvp.Value, Vector3.one * gridSpacing);
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left click
        {
            HandleMouseClick();
        }
    }

    void HandleMouseClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Vector3 hitPoint = hit.point;

            foreach (var kvp in gridIndex)
            {
                if (Vector3.Distance(hitPoint, kvp.Value) < gridSpacing * 0.5f)
                {
                    //Debug.Log($"Clicked on grid position: Coordinates = ({kvp.Key.x}, {kvp.Key.y}), Position = {kvp.Value}");
                    pathfinding.MovePlayerTo(Player.transform, GetGridCoordinateFromPosition(Player.transform.position), kvp.Key, MoveSpeed);
                }
            }
        }
    }

    public Vector2Int GetGridCoordinateFromPosition(Vector3 position)
    {
        foreach (var kvp in gridIndex)
        {
            if (Vector3.Distance(position, kvp.Value) < gridSpacing * 0.5f)
            {
                return kvp.Key;
            }
        }
        return Vector2Int.zero; // Default value, may need adjustment
    }

    public void MovePlayerToPosition(Vector2Int gridCoord)
    {
        if (gridIndex.TryGetValue(gridCoord, out Vector3 gridPosition))
        {
            if (Player != null)
            {
                Player.transform.position = gridPosition + new Vector3(0f, 3f, 0f);
                Debug.Log($"Player moved to grid position: ({gridCoord.x}, {gridCoord.y})");
            }
            else
            {
                Debug.LogError("Player GameObject is not assigned.");
            }
        }
        else
        {
            Debug.LogWarning($"Grid coordinates ({gridCoord.x}, {gridCoord.y}) not found.");
        }
    }

    // Method to check if a GameObject is present at the given grid coordinates
    public void HighlightChildAtPosition(Vector2Int gridCoord)
    {
        if (gridIndex.TryGetValue(gridCoord, out Vector3 gridPosition))
        {
            foreach (Transform child in parentObject.transform)
            {
                Tile childTile = child.GetComponent<Tile>();
                if (childTile != null && childTile.gridCoordinates == gridCoord)
                {
                    childTile.ChangeColor(Color.white);
                    Debug.Log($"Child at ({gridCoord.x}, {gridCoord.y}) has been highlighted.");
                    return;
                }
            }
            Debug.Log($"No child found at grid position: ({gridCoord.x}, {gridCoord.y})");
        }
        else
        {
            Debug.LogWarning($"Grid coordinates ({gridCoord.x}, {gridCoord.y}) not found.");
        }
    }
}