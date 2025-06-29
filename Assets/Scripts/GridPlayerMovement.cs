using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GridPlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float swipeThreshold = 50f;
    
    [Header("Grid References")]
    [SerializeField] private Transform gridParent; // Grid GameObject'ini buraya sürükleyin
    [SerializeField] private RectTransform[] gridCells; // 16 placeholder'ı manuel sırayla atayın (0-15)
    
    private RectTransform playerRect;
    private Vector2Int currentGridPosition;
    private Vector2 startTouchPosition;
    private bool isMoving = false;
    private bool isTouching = false;
    
    // Grid sistem
    private const int GRID_WIDTH = 4;
    private const int GRID_HEIGHT = 4;
    
    void Awake()
    {
        playerRect = GetComponent<RectTransform>();
        
        // Grid cells array'i boşsa otomatik doldur
        if (gridCells == null || gridCells.Length == 0)
        {
            AutoFillGridCells();
        }
        
        // Player'ı grid merkezine yerleştir (1,1)
        currentGridPosition = new Vector2Int(1, 1);
        SetPlayerToGridPosition(currentGridPosition);
    }
    
    void Update()
    {
        HandleInput();
    }
    
    void HandleInput()
    {
        if (isMoving) return;
        
        // Touch/Mouse Input
        if (Input.GetMouseButtonDown(0))
        {
            startTouchPosition = Input.mousePosition;
            isTouching = true;
        }
        
        if (Input.GetMouseButtonUp(0) && isTouching)
        {
            Vector2 endTouchPosition = Input.mousePosition;
            Vector2 swipeVector = endTouchPosition - startTouchPosition;
            
            if (swipeVector.magnitude >= swipeThreshold)
            {
                Vector2Int moveDirection = GetSwipeDirection(swipeVector);
                if (moveDirection != Vector2Int.zero)
                {
                    TryMovePlayer(moveDirection);
                }
            }
            
            isTouching = false;
        }
        
        // Keyboard Input (Test için)
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            TryMovePlayer(new Vector2Int(0, -1)); // Yukarı = Y azalt
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            TryMovePlayer(new Vector2Int(0, 1)); // Aşağı = Y arttır
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            TryMovePlayer(Vector2Int.left); // Sol = X azalt
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            TryMovePlayer(Vector2Int.right); // Sağ = X arttır
    }
    
    void AutoFillGridCells()
    {
        if (gridParent == null)
        {
            gridParent = GameObject.Find("Grid").transform;
        }
        
        if (gridParent != null)
        {
            gridCells = new RectTransform[16];
            for (int i = 0; i < gridParent.childCount && i < 16; i++)
            {
                gridCells[i] = gridParent.GetChild(i).GetComponent<RectTransform>();
            }
            Debug.Log($"Auto-filled {gridCells.Length} grid cells");
        }
    }
    
    Vector2Int GetSwipeDirection(Vector2 swipeVector)
    {
        Vector2 normalizedSwipe = swipeVector.normalized;
        
        // Hangi yön daha baskın
        if (Mathf.Abs(normalizedSwipe.x) > Mathf.Abs(normalizedSwipe.y))
        {
            // Yatay hareket
            return normalizedSwipe.x > 0 ? Vector2Int.right : Vector2Int.left;
        }
        else
        {
            // Dikey hareket - Grid sisteminde Y aşağı doğru artar
            // Yukarı swipe = Y azalt (grid yukarı)
            // Aşağı swipe = Y arttır (grid aşağı)
            return normalizedSwipe.y > 0 ? new Vector2Int(0, -1) : new Vector2Int(0, 1);
        }
    }
    
    void TryMovePlayer(Vector2Int direction)
    {
        Vector2Int newPosition = currentGridPosition + direction;
        
        // Sınır kontrolü
        if (IsValidGridPosition(newPosition))
        {
            MovePlayerToPosition(newPosition);
        }
        else
        {
            Debug.Log($"Invalid move to position: {newPosition}");
        }
    }
    
    bool IsValidGridPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < GRID_WIDTH && pos.y >= 0 && pos.y < GRID_HEIGHT;
    }
    
    void MovePlayerToPosition(Vector2Int newGridPos)
    {
        currentGridPosition = newGridPos;
        
        // Grid pozisyonunu array index'ine çevir
        int arrayIndex = (newGridPos.y * GRID_WIDTH) + newGridPos.x;
        
        if (arrayIndex >= 0 && arrayIndex < gridCells.Length && gridCells[arrayIndex] != null)
        {
            Vector2 targetWorldPos = gridCells[arrayIndex].anchoredPosition;
            StartCoroutine(AnimateMovement(targetWorldPos));
            Debug.Log($"Moving to grid position: {newGridPos}, array index: {arrayIndex}, world position: {targetWorldPos}");
        }
        else
        {
            Debug.LogError($"Invalid grid cell at index: {arrayIndex}");
        }
    }
    
    IEnumerator AnimateMovement(Vector2 targetPosition)
    {
        isMoving = true;
        Vector2 startPosition = playerRect.anchoredPosition;
        float elapsed = 0f;
        float duration = 1f / moveSpeed;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            
            playerRect.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);
            yield return null;
        }
        
        playerRect.anchoredPosition = targetPosition;
        isMoving = false;
    }
    
    void SetPlayerToGridPosition(Vector2Int gridPos)
    {
        if (IsValidGridPosition(gridPos))
        {
            currentGridPosition = gridPos;
            
            // Grid pozisyonunu array index'ine çevir
            int arrayIndex = (gridPos.y * GRID_WIDTH) + gridPos.x;
            
            if (arrayIndex >= 0 && arrayIndex < gridCells.Length && gridCells[arrayIndex] != null)
            {
                playerRect.anchoredPosition = gridCells[arrayIndex].anchoredPosition;
                Debug.Log($"Player set to grid position: {gridPos}, array index: {arrayIndex}");
            }
        }
    }
    
    // Public methods
    public Vector2Int GetCurrentGridPosition() => currentGridPosition;
    public bool IsMoving() => isMoving;
    
    public void SetGridPosition(Vector2Int newPosition)
    {
        if (IsValidGridPosition(newPosition))
        {
            SetPlayerToGridPosition(newPosition);
        }
    }
    
    // Debug için - Scene view'da grid pozisyonlarını göster
    void OnDrawGizmos()
    {
        if (gridCells == null || !Application.isPlaying) return;
        
        // Grid pozisyonlarını çiz
        Gizmos.color = Color.yellow;
        for (int i = 0; i < gridCells.Length; i++)
        {
            if (gridCells[i] != null)
            {
                int x = i % GRID_WIDTH;
                int y = i / GRID_WIDTH;
                
                Vector3 worldPos = transform.parent.TransformPoint(gridCells[i].anchoredPosition);
                Gizmos.DrawWireCube(worldPos, Vector3.one * 50f);
                
                // Grid koordinatlarını yazı olarak göster
                UnityEditor.Handles.Label(worldPos, $"{x},{y}[{i}]");
            }
        }
        
        // Mevcut pozisyonu özel renkte göster
        if (currentGridPosition.x >= 0 && currentGridPosition.y >= 0)
        {
            int currentIndex = (currentGridPosition.y * GRID_WIDTH) + currentGridPosition.x;
            if (currentIndex < gridCells.Length && gridCells[currentIndex] != null)
            {
                Gizmos.color = Color.red;
                Vector3 currentWorldPos = transform.parent.TransformPoint(gridCells[currentIndex].anchoredPosition);
                Gizmos.DrawCube(currentWorldPos, Vector3.one * 60f);
            }
        }
    }
    
    // Inspector'da test butonu için
    [ContextMenu("Test Move Up")]
    void TestMoveUp() => TryMovePlayer(new Vector2Int(0, -1));
    
    [ContextMenu("Test Move Down")]
    void TestMoveDown() => TryMovePlayer(new Vector2Int(0, 1));
    
    [ContextMenu("Test Move Left")]
    void TestMoveLeft() => TryMovePlayer(Vector2Int.left);
    
    [ContextMenu("Test Move Right")]
    void TestMoveRight() => TryMovePlayer(Vector2Int.right);
    
    [ContextMenu("Recalculate Grid")]
    void RecalculateGrid() => AutoFillGridCells();
}