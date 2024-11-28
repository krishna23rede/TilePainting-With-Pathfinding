using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileMap : MonoBehaviour
{
    public Camera mainCamera; // Assign your camera here
    public GameObject cursorPrefab; // Assign your cursor prefab here
    private GameObject cursorInstance;
    public Grid grid; // Reference to your Grid script
    private PathVisual tileHighlighter; // Reference to TileHighlighter script

    private Vector2Int currentGridPosition; // Track the current grid position

    void Start()
    {
        // Instantiate and hide the cursor initially
        cursorInstance = Instantiate(cursorPrefab);
        cursorInstance.SetActive(false); // Initially hidden

        tileHighlighter = FindObjectOfType<PathVisual>(); // Automatically find the TileHighlighter script in the scene
    }

    void LateUpdate()
    {
        // Move cursor based on mouse position
        MoveCursor();

        if (Input.GetMouseButtonDown(0)) // Detect left mouse button click
        {
            ChangeTileColor();
        }
    }

    void MoveCursor()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Vector3 hitPoint = hit.point;

            foreach (var kvp in grid.gridIndex)
            {
                // Check if the hit point is close to the grid position
                if (Vector3.Distance(hitPoint, kvp.Value) < grid.gridSpacing * 0.5f)
                {
                    cursorInstance.transform.position = kvp.Value + new Vector3(0f, 0.5f, 0f);
                    cursorInstance.SetActive(true);
                    currentGridPosition = kvp.Key; // Update current grid position

                    // Highlight the path to the cursor's current position
                    if (tileHighlighter != null && tileHighlighter.currentMode == PathVisual.HighlightMode.Hover)
                    {
                        tileHighlighter.HighlightPathTo(currentGridPosition);
                    }

                    return; // Exit loop when the nearest grid position is found
                }
            }
        }
        cursorInstance.SetActive(false);

        // Clear the highlight when the cursor is not over the grid
        if (tileHighlighter != null && tileHighlighter.currentMode == PathVisual.HighlightMode.Hover)
        {
            tileHighlighter.ClearHighlight();
        }
    }

    void ChangeTileColor()
    {
        if (grid.gridIndex.ContainsKey(currentGridPosition))
        {
            // Implement the logic to change the color or perform any action
            // Debug.Log($"Color change or action at grid position: ({currentGridPosition.x}, {currentGridPosition.y})");

            // Highlight the path on click if in Click mode
            if (tileHighlighter != null && tileHighlighter.currentMode == PathVisual.HighlightMode.Click)
            {
                tileHighlighter.HighlightPathTo(currentGridPosition);
            }
        }
    }
}