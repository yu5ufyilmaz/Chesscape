using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


public enum PieceType
{
    Pawn,    // Piyon - 0
    Rook,    // Kale - 1
    Bishop,  // Fil - 2  
    Knight,  // At - 3
    Queen,   // Vezir - 4
}

public class ChessGameManager : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject[] chessPiecePrefabs; // 0:Pawn, 1:Rook, 2:Bishop, 3:Knight, 4:Queen
    [SerializeField] private RectTransform[] spawnCells;
    [SerializeField] private float spawnInterval = 3f;
    [SerializeField] private float fallSpeed = 1f;
    [SerializeField] private float difficultyIncreaseRate = 0.05f;
    
    [Header("Chess Piece Settings")]
    [SerializeField] private float[] pieceSpawnRates = {0.4f, 0.2f, 0.2f, 0.1f, 0.1f}; // Pawn, Rook, Bishop, Knight, Queen
    
    [Header("Grid References")]
    [SerializeField] private Transform gridParent;
    [SerializeField] private RectTransform[] gridCells;
    [SerializeField] private GridPlayerMovement playerMovement;
    
    [Header("Game Settings")]
    [SerializeField] private bool gameActive = true;
    [SerializeField] private int score = 0;
    [SerializeField] private int piecesSurvived = 0;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text pieceCountText;
    [SerializeField] private Text gameOverText;
    
    // Grid sistem
    private const int GRID_WIDTH = 4;
    private const int GRID_HEIGHT = 4;
    
    // Aktif satranç taşları - generic interface
    private List<MonoBehaviour> activeChessPieces = new List<MonoBehaviour>();
    
    // Grid durumu - hangi hücrelerde taş var
    private bool[,] gridOccupied = new bool[GRID_WIDTH, GRID_HEIGHT];
    
    void Start()
    {
        // Grid cells'i otomatik doldur
        if (gridCells == null || gridCells.Length == 0)
        {
            AutoFillGridCells();
        }
        
        // Spawn cells'i otomatik doldur
        if (spawnCells == null || spawnCells.Length == 0)
        {
            AutoFillSpawnCells();
        }
        
        // Player referansını bul
        if (playerMovement == null)
        {
            playerMovement = FindObjectOfType<GridPlayerMovement>();
        }
        
        // UI'yi güncelle
        UpdateUI();
        
        // Spawn'ı başlat
        StartCoroutine(SpawnRoutine());
        StartCoroutine(FallRoutine());
        StartCoroutine(ChessMoveRoutine());
    }
    
    void Update()
    {
        if (!gameActive) return;
        
        // Oyuncu çarpışma kontrolü
        CheckPlayerCollision();
        
        // Zorluk artırma
        IncreaseDifficulty();
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
    
    void AutoFillSpawnCells()
    {
        GameObject spawnParent = GameObject.Find("SpawnGrid");
        if (spawnParent == null)
        {
            spawnParent = GameObject.Find("Spawn");
        }
        
        if (spawnParent != null)
        {
            spawnCells = new RectTransform[4];
            for (int i = 0; i < spawnParent.transform.childCount && i < 4; i++)
            {
                spawnCells[i] = spawnParent.transform.GetChild(i).GetComponent<RectTransform>();
            }
            Debug.Log($"Auto-filled {spawnCells.Length} spawn cells");
        }
    }
    
    IEnumerator SpawnRoutine()
    {
        while (gameActive)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnChessPiece();
        }
    }
    
    void SpawnChessPiece()
    {
        if (spawnCells == null || spawnCells.Length == 0 || chessPiecePrefabs == null || chessPiecePrefabs.Length == 0) 
        {
            Debug.LogError("SpawnChessPiece: Missing spawn cells or prefabs!");
            return;
        }
        
        // Rastgele spawn pozisyonu seç
        int randomSpawnIndex = Random.Range(0, spawnCells.Length);
        RectTransform spawnCell = spawnCells[randomSpawnIndex];
        
        if (spawnCell == null) 
        {
            Debug.LogError($"SpawnCell {randomSpawnIndex} is null!");
            return;
        }
        
        // Rastgele piece type seç (ağırlıklı)
        PieceType selectedType = GetRandomPieceType();
        int prefabIndex = (int)selectedType;
        
        if (prefabIndex >= chessPiecePrefabs.Length || chessPiecePrefabs[prefabIndex] == null) 
        {
            Debug.LogError($"Prefab {prefabIndex} for {selectedType} is missing!");
            return;
        }
        
        // Piece'i spawn cell'inde oluştur
        GameObject obj = Instantiate(chessPiecePrefabs[prefabIndex], spawnCell.transform);
        
        // RectTransform'unu ayarla
        RectTransform objRect = obj.GetComponent<RectTransform>();
        objRect.anchoredPosition = Vector2.zero;
        objRect.localScale = Vector3.one;
        
        // Piece'i initialize et
        Vector2Int startGridPos = new Vector2Int(randomSpawnIndex, -1);
        
        // Her piece tipine göre initialize
        MonoBehaviour addedPiece = null;
        
        switch (selectedType)
        {
            case PieceType.Pawn:
                var pawn = obj.GetComponent<PawnPiece>();
                if (pawn != null)
                {
                    pawn.Initialize(startGridPos, this);
                    addedPiece = pawn;
                    Debug.Log("PawnPiece component found and initialized");
                }
                else
                {
                    Debug.LogError("PawnPiece component not found on prefab!");
                }
                break;
            case PieceType.Rook:
                var rook = obj.GetComponent<RookPiece>();
                if (rook != null)
                {
                    rook.Initialize(startGridPos, this);
                    addedPiece = rook;
                    Debug.Log("RookPiece component found and initialized");
                }
                else
                {
                    Debug.LogError("RookPiece component not found on prefab!");
                }
                break;
            case PieceType.Bishop:
                var bishop = obj.GetComponent<BishopPiece>();
                if (bishop != null)
                {
                    bishop.Initialize(startGridPos, this);
                    addedPiece = bishop;
                    Debug.Log("BishopPiece component found and initialized");
                }
                else
                {
                    Debug.LogError("BishopPiece component not found on prefab!");
                }
                break;
            case PieceType.Knight:
                var knight = obj.GetComponent<KnightPiece>();
                if (knight != null)
                {
                    knight.Initialize(startGridPos, this);
                    addedPiece = knight;
                    Debug.Log("KnightPiece component found and initialized");
                }
                else
                {
                    Debug.LogError("KnightPiece component not found on prefab!");
                }
                break;
            case PieceType.Queen:
                var queen = obj.GetComponent<QueenPiece>();
                if (queen != null)
                {
                    queen.Initialize(startGridPos, this);
                    addedPiece = queen;
                    Debug.Log("QueenPiece component found and initialized");
                }
                else
                {
                    Debug.LogError("QueenPiece component not found on prefab!");
                }
                break;
        }
        
        if (addedPiece != null)
        {
            activeChessPieces.Add(addedPiece);
            Debug.Log($"Successfully spawned {selectedType} at spawn cell {randomSpawnIndex}. Total active pieces: {activeChessPieces.Count}");
        }
        else
        {
            Debug.LogError($"Failed to add {selectedType} to active pieces list!");
            Destroy(obj);
        }
    }
    
    PieceType GetRandomPieceType()
    {
        float random = Random.Range(0f, 1f);
        float cumulative = 0f;
        
        for (int i = 0; i < pieceSpawnRates.Length; i++)
        {
            cumulative += pieceSpawnRates[i];
            if (random <= cumulative)
            {
                return (PieceType)i;
            }
        }
        
        return PieceType.Pawn; // Fallback
    }
    
    IEnumerator FallRoutine()
    {
        while (gameActive)
        {
            yield return new WaitForSeconds(1f / fallSpeed);
            
            Debug.Log($"FallRoutine: Active pieces count: {activeChessPieces.Count}");
            
            // Sadece normal mode'daki piece'ler düşsün
            for (int i = activeChessPieces.Count - 1; i >= 0; i--)
            {
                if (activeChessPieces[i] != null)
                {
                    bool inChessMode = IsInChessMode(activeChessPieces[i]);
                    Debug.Log($"Piece {i}: InChessMode = {inChessMode}");
                    
                    if (!inChessMode)
                    {
                        Debug.Log($"Calling Fall() for piece {i}");
                        CallFallMethod(activeChessPieces[i]);
                    }
                }
                else
                {
                    Debug.Log($"Piece {i} is null, removing from list");
                    activeChessPieces.RemoveAt(i);
                }
            }
        }
    }
    
    IEnumerator ChessMoveRoutine()
    {
        while (gameActive)
        {
            yield return new WaitForSeconds(1.5f);
            
            // Sadece chess mode'daki piece'ler hamle yapsın
            for (int i = activeChessPieces.Count - 1; i >= 0; i--)
            {
                if (activeChessPieces[i] != null)
                {
                    bool inChessMode = IsInChessMode(activeChessPieces[i]);
                    if (inChessMode)
                    {
                        CallFallMethod(activeChessPieces[i]);
                    }
                }
            }
        }
    }
    
    bool IsInChessMode(MonoBehaviour piece)
    {
        if (piece is PawnPiece pawn) return pawn.IsInChessMode();
        if (piece is RookPiece rook) return rook.IsInChessMode();
        if (piece is BishopPiece bishop) return bishop.IsInChessMode();
        if (piece is KnightPiece knight) return knight.IsInChessMode();
        if (piece is QueenPiece queen) return queen.IsInChessMode();
        return false;
    }
    
    void CallFallMethod(MonoBehaviour piece)
    {
        Debug.Log($"CallFallMethod called for piece type: {piece.GetType().Name}");
        
        if (piece is PawnPiece pawn) 
        {
            Debug.Log("Calling PawnPiece.Fall()");
            pawn.Fall();
        }
        else if (piece is RookPiece rook) 
        {
            Debug.Log("Calling RookPiece.Fall()");
            rook.Fall();
        }
        else if (piece is BishopPiece bishop) 
        {
            Debug.Log("Calling BishopPiece.Fall()");
            bishop.Fall();
        }
        else if (piece is KnightPiece knight) 
        {
            Debug.Log("Calling KnightPiece.Fall()");
            knight.Fall();
        }
        else if (piece is QueenPiece queen) 
        {
            Debug.Log("Calling QueenPiece.Fall()");
            queen.Fall();
        }
        else
        {
            Debug.LogError($"Unknown piece type: {piece.GetType().Name}");
        }
    }
    
    void CheckPlayerCollision()
    {
        if (playerMovement == null) return;
        
        Vector2Int playerPos = playerMovement.GetCurrentGridPosition();
        
        // Satranç taşları ile çarpışma kontrolü
        foreach (var piece in activeChessPieces)
        {
            if (piece != null)
            {
                Vector2Int piecePos = GetPiecePosition(piece);
                if (piecePos == playerPos)
                {
                    GameOver();
                    return;
                }
            }
        }
        
        // Yerleşmiş taşlarla çarpışma kontrolü
        if (IsValidGridPosition(playerPos) && gridOccupied[playerPos.x, playerPos.y])
        {
            GameOver();
        }
    }
    
    Vector2Int GetPiecePosition(MonoBehaviour piece)
    {
        if (piece is PawnPiece pawn) return pawn.GetGridPosition();
        if (piece is RookPiece rook) return rook.GetGridPosition();
        if (piece is BishopPiece bishop) return bishop.GetGridPosition();
        if (piece is KnightPiece knight) return knight.GetGridPosition();
        if (piece is QueenPiece queen) return queen.GetGridPosition();
        return Vector2Int.zero;
    }
    
    void GameOver()
    {
        gameActive = false;
        
        if (gameOverText != null)
        {
            gameOverText.text = $"GAME OVER!\nScore: {score}\nPieces Survived: {piecesSurvived}\nTap to Restart";
            gameOverText.gameObject.SetActive(true);
        }
        
        Debug.Log($"Game Over! Final Score: {score}, Pieces Survived: {piecesSurvived}");
    }
    
    void IncreaseDifficulty()
    {
        // Zaman geçtikçe zorluk artır
        fallSpeed += difficultyIncreaseRate * Time.deltaTime;
        spawnInterval = Mathf.Max(1f, spawnInterval - difficultyIncreaseRate * Time.deltaTime);
    }
    
    void UpdateUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
        
        if (pieceCountText != null)
        {
            pieceCountText.text = $"Pieces: {piecesSurvived}";
        }
    }
    
    // Public methods for all piece types
    public void OnPieceDestroyed(MonoBehaviour piece)
    {
        activeChessPieces.Remove(piece);
        
        // Skor ver
        piecesSurvived++;
        score += GetPieceScore(piece);
        UpdateUI();
        
        Debug.Log($"Piece destroyed! Score: +{GetPieceScore(piece)}");
    }
    
    public void OnPieceReachedBottom(MonoBehaviour piece)
    {
        // Piece alt sınıra ulaştı veya çarpışma durdu
        Vector2Int gridPos = GetPiecePosition(piece);
        
        if (IsValidGridPosition(gridPos))
        {
            // Grid'e yerleştir
            gridOccupied[gridPos.x, gridPos.y] = true;
            
            // Skor ver
            int placementScore = GetPieceScore(piece) / 2;
            score += placementScore;
            UpdateUI();
            
            Debug.Log($"Piece placed at grid position: {gridPos}, Score: +{placementScore}");
        }
    }
    
    int GetPieceScore(MonoBehaviour piece)
    {
        if (piece is PawnPiece) return 10;
        if (piece is RookPiece) return 20;
        if (piece is BishopPiece) return 25;
        if (piece is KnightPiece) return 30;
        if (piece is QueenPiece) return 50;
        return 10;
    }
    
    public bool IsGridPositionOccupied(Vector2Int pos)
    {
        if (!IsValidGridPosition(pos)) return true;
        
        // Grid'de başka piece var mı kontrol et
        foreach (var piece in activeChessPieces)
        {
            if (piece != null && GetPiecePosition(piece) == pos)
            {
                return true;
            }
        }
        
        return gridOccupied[pos.x, pos.y];
    }
    
    public RectTransform GetGridCell(Vector2Int gridPos)
    {
        int arrayIndex = (gridPos.y * GRID_WIDTH) + gridPos.x;
        
        if (arrayIndex >= 0 && arrayIndex < gridCells.Length && gridCells[arrayIndex] != null)
        {
            return gridCells[arrayIndex];
        }
        
        return null;
    }
    
    public Vector2Int GetPlayerPosition()
    {
        if (playerMovement != null)
        {
            return playerMovement.GetCurrentGridPosition();
        }
        return Vector2Int.zero;
    }
    
    bool IsValidGridPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < GRID_WIDTH && pos.y >= 0 && pos.y < GRID_HEIGHT;
    }
    
    // Restart game
    public void RestartGame()
    {
        if (gameActive) return;
        
        Debug.Log("Restarting game...");
        
        // Tüm coroutine'leri durdur
        StopAllCoroutines();
        
        // Tüm piece'leri temizle
        foreach (var piece in activeChessPieces)
        {
            if (piece != null) Destroy(piece.gameObject);
        }
        activeChessPieces.Clear();
        
        // Grid'i temizle
        gridOccupied = new bool[GRID_WIDTH, GRID_HEIGHT];
        
        // Skorları sıfırla
        score = 0;
        piecesSurvived = 0;
        fallSpeed = 1f;
        spawnInterval = 3f;
        gameActive = true;
        
        // Player'ı başlangıç pozisyonuna al
        if (playerMovement != null)
        {
            playerMovement.SetGridPosition(new Vector2Int(1, 1));
        }
        
        // UI'yi güncelle
        UpdateUI();
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(false);
        }
        
        // Spawn'ı yeniden başlat
        StartCoroutine(SpawnRoutine());
        StartCoroutine(FallRoutine());
        StartCoroutine(ChessMoveRoutine());
        
        Debug.Log("Game restarted successfully!");
    }
    
    // Touch input for restart
    void OnMouseDown()
    {
        if (!gameActive)
        {
            RestartGame();
        }
    }
    
    // Public method for UI button restart
    public void OnRestartButtonClicked()
    {
        if (!gameActive)
        {
            RestartGame();
        }
    }
    
    // Debug info
    [ContextMenu("Show Game Status")]
    void ShowGameStatus()
    {
        Debug.Log($"=== CHESS GAME STATUS ===");
        Debug.Log($"Game Active: {gameActive}");
        Debug.Log($"Active Pieces: {activeChessPieces.Count}");
        Debug.Log($"Score: {score}");
        Debug.Log($"Pieces Survived: {piecesSurvived}");
        Debug.Log($"Fall Speed: {fallSpeed}");
        Debug.Log($"Spawn Interval: {spawnInterval}");
        Debug.Log($"Player Position: {GetPlayerPosition()}");
        
        int occupiedCells = 0;
        for (int x = 0; x < GRID_WIDTH; x++)
        {
            for (int y = 0; y < GRID_HEIGHT; y++)
            {
                if (gridOccupied[x, y]) occupiedCells++;
            }
        }
        Debug.Log($"Occupied Grid Cells: {occupiedCells}/16");
    }
}