using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public int gridSize = 3;
    public float cellSize = 1.5f;
    public GameObject cellPrefab;
    
    [Header("Grid Visual")]
    public Color lightColor = Color.white;
    public Color darkColor = Color.gray;
    
    private GameObject[,] gridCells;
    private Vector2 gridOffset;
    
    void Start()
    {
        CreateGrid();
    }
    
    void CreateGrid()
    {
        gridCells = new GameObject[gridSize, gridSize];
        
        // Grid'i ortala
        gridOffset = new Vector2(
            -(gridSize - 1) * cellSize * 0.5f,
            -(gridSize - 1) * cellSize * 0.5f
        );
        
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                CreateCell(x, y);
            }
        }
    }
    
    void CreateCell(int x, int y)
    {
        Vector3 position = new Vector3(
            gridOffset.x + x * cellSize,
            gridOffset.y + y * cellSize,
            0
        );
        
        GameObject cell = Instantiate(cellPrefab, position, Quaternion.identity, transform);
        cell.name = $"Cell_{x}_{y}";
        
        // Satranç tahtası renklendirmesi
        SpriteRenderer renderer = cell.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.color = (x + y) % 2 == 0 ? lightColor : darkColor;
        }
        
        gridCells[x, y] = cell;
    }
    
    public Vector3 GetWorldPosition(int x, int y)
    {
        if (x < 0 || x >= gridSize || y < 0 || y >= gridSize)
            return Vector3.zero;
            
        return new Vector3(
            gridOffset.x + x * cellSize,
            gridOffset.y + y * cellSize,
            0
        );
    }
    
    public Vector2Int GetGridPosition(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt((worldPos.x - gridOffset.x) / cellSize);
        int y = Mathf.RoundToInt((worldPos.y - gridOffset.y) / cellSize);
        return new Vector2Int(x, y);
    }
    
    public bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < gridSize && y >= 0 && y < gridSize;
    }
}