using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyPathfInding : MonoBehaviour
{
    public Grid grid; // Reference to the Grid script

    private Coroutine moveCoroutine;
    private Vector2Int currentTarget;

    private void Start()
    {
        if (grid == null)
        {
            Debug.LogError("Grid script reference is missing.");
        }
    }

    public void MovePlayerTo(Transform player, Vector2Int start, Vector2Int target , float speed)
    {
        // Stop any ongoing movement coroutine
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }

        // Start new pathfinding and movement
        moveCoroutine = StartCoroutine(FindAndMove(player, start, target , speed));
    }

    private IEnumerator FindAndMove(Transform player, Vector2Int start, Vector2Int target , float speed)
    {
        List<Vector3> path = FindPath(start, target);

        if (path != null)
        {
            // Start moving along the new path
            foreach (Vector3 position in path)
            {
                while (Vector3.Distance(player.position, position) > 0.1f)
                {
                    player.position = Vector3.MoveTowards(player.position, position, Time.deltaTime * speed);
                    yield return null;
                }
            }
        }
        else
        {
            Debug.Log($"No valid path found to target. {start} and {target} and {grid.gridIndex[start]}");
        }

        // Clear the coroutine reference when done
        moveCoroutine = null;
    }

    public List<Vector3> FindPath(Vector2Int start, Vector2Int target)
    {
        // Get the grid positions from the grid script
        Dictionary<Vector2Int, Vector3> gridPositions = grid.gridIndex;

        if (!gridPositions.ContainsKey(start) || !gridPositions.ContainsKey(target))
        {
            Debug.LogWarning("Start or target position is not in the grid.");
            return null;
        }

        // Initialize the open and closed lists
        List<Vector2Int> openList = new List<Vector2Int> { start };
        HashSet<Vector2Int> closedList = new HashSet<Vector2Int>();

        // Initialize dictionaries for cost and parent tracking
        Dictionary<Vector2Int, float> gCost = new Dictionary<Vector2Int, float> { [start] = 0 };
        Dictionary<Vector2Int, float> hCost = new Dictionary<Vector2Int, float> { [start] = GetDistance(start, target) };
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();

        while (openList.Count > 0)
        {
            // Get the node with the lowest fCost
            Vector2Int current = GetLowestFCostNode(openList, gCost, hCost, target);

            if (current == target)
            {
                // Reconstruct the path
                return ReconstructPath(cameFrom, current);
            }

            openList.Remove(current);
            closedList.Add(current);

            foreach (Vector2Int neighbor in GetNeighbors(current))
            {
                if (closedList.Contains(neighbor) || !grid.gridIndex.ContainsKey(neighbor) || !TileExistsAt(neighbor))
                {
                    continue;
                }

                float tentativeGCost = gCost[current] + GetDistance(current, neighbor);

                if (!gCost.ContainsKey(neighbor) || tentativeGCost < gCost[neighbor])
                {
                    gCost[neighbor] = tentativeGCost;
                    hCost[neighbor] = GetDistance(neighbor, target);
                    cameFrom[neighbor] = current;

                    if (!openList.Contains(neighbor))
                    {
                        openList.Add(neighbor);
                    }
                }
            }
        }

        // No path found
        return null;
    }

    private Vector2Int GetLowestFCostNode(List<Vector2Int> openList, Dictionary<Vector2Int, float> gCost, Dictionary<Vector2Int, float> hCost, Vector2Int target)
    {
        Vector2Int lowest = openList[0];
        float lowestFCost = gCost[lowest] + hCost[lowest];

        foreach (Vector2Int node in openList)
        {
            float fCost = gCost[node] + hCost[node];
            if (fCost < lowestFCost)
            {
                lowest = node;
                lowestFCost = fCost;
            }
        }

        return lowest;
    }

    private List<Vector2Int> GetNeighbors(Vector2Int node)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();

        neighbors.Add(new Vector2Int(node.x + 1, node.y));
        neighbors.Add(new Vector2Int(node.x - 1, node.y));
        neighbors.Add(new Vector2Int(node.x, node.y + 1));
        neighbors.Add(new Vector2Int(node.x, node.y - 1));

        return neighbors;
    }

    private float GetDistance(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private List<Vector3> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
    {
        List<Vector3> path = new List<Vector3>();
        Dictionary<Vector2Int, Vector3> gridPositions = grid.gridIndex;

        while (cameFrom.ContainsKey(current))
        {
            path.Add(gridPositions[current]);
            current = cameFrom[current];
        }
        path.Reverse();
        return path;
    }

    private bool TileExistsAt(Vector2Int position)
    {
        // Check if there's a tile at the given grid coordinates
        foreach (Transform child in grid.parentObject.transform)
        {
            Tile childTile = child.GetComponent<Tile>();
            if (childTile != null && childTile.gridCoordinates == position)
            {
                return true;
            }
        }
        return false;
    }
}