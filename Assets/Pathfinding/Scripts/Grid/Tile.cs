using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public Vector2Int gridCoordinates;

    // Method to set the grid coordinates
    public void SetGridCoordinates(Vector2Int coordinates)
    {
        gridCoordinates = coordinates;
    }

    // Method to change the color of the child object
    public void ChangeColor(Color newColor)
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = newColor;
        }
        else
        {
            Debug.LogWarning($"GameObject {gameObject.name} does not have a Renderer component.");
        }
    }
}