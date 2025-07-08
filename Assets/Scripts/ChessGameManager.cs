using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
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
    
    [Header("Grid References")]
    [SerializeField] private Transform gridParent;
    [SerializeField] private RectTransform[] gridCells;
    [SerializeField] private GridPlayerMovement playerMovement;
    
    [Header("Level Configuration")]
    [SerializeField] private LevelConfigData levelConfigData;
    
    [Header("Game UI")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text pieceCountText;
    [SerializeField] private Text gameOverText;
    [SerializeField] private Text levelText;
    [SerializeField] private Text timerText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject gameCompletePanel;
    [SerializeField] private GameObject[] healthIcons; // 3 can ikonu
    [SerializeField] private Button restartButton;
    [SerializeField] private Button homeButton;
    [SerializeField] private Button nextLevelButton;
    
    // Grid sistem
    private const int GRID_WIDTH = 4;
    private const int GRID_HEIGHT = 4;
    
    // Aktif satranç taşları
    private List<MonoBehaviour> activeChessPieces = new List<MonoBehaviour>();
    
    // Grid durumu
    private bool[,] gridOccupied = new bool[GRID_WIDTH, GRID_HEIGHT];
    
    // Game state
    private bool gameActive = true;
    private int score = 0;
    private int piecesSpawned = 0;
    private int playerHealth = 3;
    private float gameTimer = 0f;
    private LevelConfig currentLevelConfig;
    private int currentLevel;
    
    void Start()
    {
        // Hangi level yükleneceğini al
        currentLevel = UIManager.selectedLevel;
        
        // Level config'ini yükle
        LoadLevelConfig();
        
        // Grid ve spawn setup
        SetupGridAndSpawn();
        
        // Player setup
        SetupPlayer();
        
        // UI setup
        SetupUI();
        
        // Game başlat
        StartGame();
    }
    
    void LoadLevelConfig()
    {
        Debug.Log($"=== LOADING LEVEL {currentLevel} CONFIG ===");
        Debug.Log($"levelConfigData is null: {levelConfigData == null}");
        
        if (levelConfigData != null)
        {
            Debug.Log($"levelConfigData.levels count: {levelConfigData.levels.Count}");
            currentLevelConfig = levelConfigData.GetLevelConfig(currentLevel);
            Debug.Log($"✅ LOADED FROM ASSET: Level {currentLevel}");
        }
        else
        {
            // Default config
            currentLevelConfig = new LevelConfig();
            currentLevelConfig.levelNumber = currentLevel;
            currentLevelConfig.levelName = $"Level {currentLevel}";
            currentLevelConfig.pawnCount = 10;
            currentLevelConfig.rookCount = 0;
            currentLevelConfig.bishopCount = 0;
            currentLevelConfig.knightCount = 0;
            currentLevelConfig.queenCount = 0;
            currentLevelConfig.spawnInterval = 2f;
            currentLevelConfig.fallSpeed = 1f;
            currentLevelConfig.gameDuration = 60f;
            Debug.LogError("❌ LEVEL CONFIG DATA NOT ASSIGNED! Using emergency default config (Pawn only)!");
        }
        
        // Level konfigürasyonunu detaylı logla
        Debug.Log($"=== FINAL LEVEL {currentLevel} CONFIG ===");
        Debug.Log($"Level Name: {currentLevelConfig.levelName}");
        Debug.Log($"Piece Counts - Pawn: {currentLevelConfig.pawnCount}, Rook: {currentLevelConfig.rookCount}, Bishop: {currentLevelConfig.bishopCount}, Knight: {currentLevelConfig.knightCount}, Queen: {currentLevelConfig.queenCount}");
        Debug.Log($"Enable Flags - Bishop: {currentLevelConfig.enableBishopPieces}, Knight: {currentLevelConfig.enableKnightPieces}, Queen: {currentLevelConfig.enableQueenPieces}");
        Debug.Log($"Total Pieces: {currentLevelConfig.GetTotalPieceCount()}");
        
        // Piece rates'i de test et
        float[] testRates = currentLevelConfig.GetPieceRates();
        Debug.Log($"Calculated rates - P:{testRates[0]*100:F1}%, R:{testRates[1]*100:F1}%, B:{testRates[2]*100:F1}%, N:{testRates[3]*100:F1}%, Q:{testRates[4]*100:F1}%");
    }
    
    void SetupGridAndSpawn()
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
    }
    
    void SetupPlayer()
    {
        // Player referansını bul
        if (playerMovement == null)
        {
            playerMovement = FindObjectOfType<GridPlayerMovement>();
        }
        
        // Player sağlığını ayarla
        playerHealth = currentLevelConfig.playerStartHealth;
        UpdateHealthUI();
        
        // Player'ı başlangıç pozisyonuna yerleştir
        if (playerMovement != null)
        {
            playerMovement.SetGridPosition(new Vector2Int(1, 1));
        }
    }
    
    void SetupUI()
    {
        UpdateUI();
        
        // Button events
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartLevel);
            
        if (homeButton != null)
            homeButton.onClick.AddListener(GoToMainMenu);
            
        if (nextLevelButton != null)
            nextLevelButton.onClick.AddListener(LoadNextLevel);
        
        // Panels'i gizle
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
            
        if (gameCompletePanel != null)
            gameCompletePanel.SetActive(false);
    }
    
    void StartGame()
    {
        gameActive = true;
        gameTimer = 0f;
        
        // Routines'i başlat
        StartCoroutine(SpawnRoutine());
        StartCoroutine(FallRoutine());
        StartCoroutine(ChessMoveRoutine());
        
        if (currentLevelConfig.gameDuration > 0)
        {
            StartCoroutine(GameTimerRoutine());
        }
        
        Debug.Log($"Game started! Level: {currentLevel}, Duration: {currentLevelConfig.gameDuration}s, Max Pieces: {currentLevelConfig.maxPieces}");
    }
    
    void Update()
    {
        if (!gameActive) return;
        
        // Oyuncu çarpışma kontrolü
        CheckPlayerCollision();
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
        int totalPiecesToSpawn = currentLevelConfig.GetTotalPieceCount();
        
        while (gameActive && piecesSpawned < totalPiecesToSpawn)
        {
            yield return new WaitForSeconds(currentLevelConfig.spawnInterval);
            SpawnChessPiece();
            piecesSpawned++;
        }
        
        // Tüm piece'ler spawn olduysa, aktif piece kalmadığında level tamamla
        if (piecesSpawned >= totalPiecesToSpawn)
        {
            StartCoroutine(CheckLevelComplete());
        }
    }
    
    IEnumerator CheckLevelComplete()
    {
        // Aktif piece kalmadığında level tamamla
        while (gameActive && activeChessPieces.Count > 0)
        {
            yield return new WaitForSeconds(1f);
        }
        
        if (gameActive)
        {
            LevelComplete();
        }
    }
    
    IEnumerator GameTimerRoutine()
    {
        while (gameActive && gameTimer < currentLevelConfig.gameDuration)
        {
            yield return new WaitForSeconds(1f);
            gameTimer += 1f;
            UpdateTimerUI();
        }
        
        if (gameActive)
        {
            LevelComplete();
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
        
        // Piece type seç (level config'e göre)
        PieceType selectedType = GetRandomPieceType();
        int prefabIndex = (int)selectedType;
        
        if (prefabIndex >= chessPiecePrefabs.Length || chessPiecePrefabs[prefabIndex] == null) 
        {
            Debug.LogError($"Prefab {prefabIndex} for {selectedType} is missing!");
            return;
        }
        
        // Piece'i spawn et
        GameObject obj = Instantiate(chessPiecePrefabs[prefabIndex], spawnCell.transform);
        
        // RectTransform'unu ayarla
        RectTransform objRect = obj.GetComponent<RectTransform>();
        objRect.anchoredPosition = Vector2.zero;
        objRect.localScale = Vector3.one;
        
        // Piece'i initialize et
        Vector2Int startGridPos = new Vector2Int(randomSpawnIndex, -1);
        
        // Initialize based on type
        MonoBehaviour addedPiece = InitializePiece(obj, selectedType, startGridPos);
        
        if (addedPiece != null)
        {
            activeChessPieces.Add(addedPiece);
            Debug.Log($"Spawned {selectedType} at spawn cell {randomSpawnIndex}. Total active pieces: {activeChessPieces.Count}");
        }
        else
        {
            Debug.LogError($"Failed to initialize {selectedType}!");
            Destroy(obj);
        }
    }
    
    MonoBehaviour InitializePiece(GameObject obj, PieceType type, Vector2Int startGridPos)
    {
        switch (type)
        {
            case PieceType.Pawn:
                var pawn = obj.GetComponent<PawnPiece>();
                if (pawn != null)
                {
                    pawn.Initialize(startGridPos, this);
                    return pawn;
                }
                break;
            case PieceType.Rook:
                var rook = obj.GetComponent<RookPiece>();
                if (rook != null)
                {
                    rook.Initialize(startGridPos, this);
                    return rook;
                }
                break;
            case PieceType.Bishop:
                var bishop = obj.GetComponent<BishopPiece>();
                if (bishop != null)
                {
                    bishop.Initialize(startGridPos, this);
                    return bishop;
                }
                break;
            case PieceType.Knight:
                var knight = obj.GetComponent<KnightPiece>();
                if (knight != null)
                {
                    knight.Initialize(startGridPos, this);
                    return knight;
                }
                break;
            case PieceType.Queen:
                var queen = obj.GetComponent<QueenPiece>();
                if (queen != null)
                {
                    queen.Initialize(startGridPos, this);
                    return queen;
                }
                break;
        }
        
        return null;
    }
    
    PieceType GetRandomPieceType()
    {
        float[] rates = currentLevelConfig.GetPieceRates();
        
        // Önce hangi piece'lerin mevcut olduğunu kontrol et
        bool hasPawn = rates[0] > 0f;
        bool hasRook = rates[1] > 0f;
        bool hasBishop = rates[2] > 0f;
        bool hasKnight = rates[3] > 0f;
        bool hasQueen = rates[4] > 0f;
        
        Debug.Log($"Available pieces - Pawn:{hasPawn} Rook:{hasRook} Bishop:{hasBishop} Knight:{hasKnight} Queen:{hasQueen}");
        
        float random = Random.Range(0f, 1f);
        float cumulative = 0f;
        
        for (int i = 0; i < rates.Length; i++)
        {
            if (rates[i] > 0f) // Sadece rate > 0 olan piece'leri kontrol et
            {
                cumulative += rates[i];
                if (random <= cumulative)
                {
                    Debug.Log($"Selected piece type: {(PieceType)i} (random: {random:F3}, cumulative: {cumulative:F3}, count: {GetPieceCount((PieceType)i)})");
                    return (PieceType)i;
                }
            }
        }
        
        // Fallback - ilk mevcut piece'i bul
        for (int i = 0; i < rates.Length; i++)
        {
            if (rates[i] > 0f)
            {
                Debug.Log($"Fallback selected: {(PieceType)i}");
                return (PieceType)i;
            }
        }
        
        Debug.LogError("No valid piece type found! This should not happen!");
        return PieceType.Pawn;
    }
    
    int GetPieceCount(PieceType type)
    {
        switch (type)
        {
            case PieceType.Pawn: return currentLevelConfig.pawnCount;
            case PieceType.Rook: return currentLevelConfig.rookCount;
            case PieceType.Bishop: return currentLevelConfig.bishopCount;
            case PieceType.Knight: return currentLevelConfig.knightCount;
            case PieceType.Queen: return currentLevelConfig.queenCount;
            default: return 0;
        }
    }
    
    IEnumerator FallRoutine()
    {
        while (gameActive)
        {
            yield return new WaitForSeconds(1f / currentLevelConfig.fallSpeed);
            
            // Normal mode piece'ler düşsün
            for (int i = activeChessPieces.Count - 1; i >= 0; i--)
            {
                if (activeChessPieces[i] != null)
                {
                    bool inChessMode = IsInChessMode(activeChessPieces[i]);
                    
                    if (!inChessMode)
                    {
                        CallFallMethod(activeChessPieces[i]);
                    }
                }
                else
                {
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
            
            // Chess mode piece'ler hamle yapsın
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
        if (piece is PawnPiece pawn) pawn.Fall();
        else if (piece is RookPiece rook) rook.Fall();
        else if (piece is BishopPiece bishop) bishop.Fall();
        else if (piece is KnightPiece knight) knight.Fall();
        else if (piece is QueenPiece queen) queen.Fall();
    }
    
    void CheckPlayerCollision()
    {
        if (playerMovement == null) return;
        
        Vector2Int playerPos = playerMovement.GetCurrentGridPosition();
        
        // Aktif taşlarla çarpışma
        foreach (var piece in activeChessPieces)
        {
            if (piece != null)
            {
                Vector2Int piecePos = GetPiecePosition(piece);
                if (piecePos == playerPos)
                {
                    PlayerHit();
                    return;
                }
            }
        }
        
        // Yerleşmiş taşlarla çarpışma
        if (IsValidGridPosition(playerPos) && gridOccupied[playerPos.x, playerPos.y])
        {
            PlayerHit();
        }
    }
    
    void PlayerHit()
    {
        playerHealth--;
        UpdateHealthUI();
        
        Debug.Log($"Player hit! Remaining health: {playerHealth}");
        
        if (playerHealth <= 0)
        {
            GameOver();
        }
        else
        {
            // Player'ı güvenli pozisyona taşı
            ResetPlayerPosition();
        }
    }
    
    void ResetPlayerPosition()
    {
        if (playerMovement != null)
        {
            // Güvenli bir pozisyon bul
            for (int x = 0; x < GRID_WIDTH; x++)
            {
                for (int y = 0; y < GRID_HEIGHT; y++)
                {
                    Vector2Int testPos = new Vector2Int(x, y);
                    if (!IsGridPositionOccupied(testPos))
                    {
                        playerMovement.SetGridPosition(testPos);
                        return;
                    }
                }
            }
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
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        
        Debug.Log($"Game Over! Final Score: {score}");
    }
    
    void LevelComplete()
    {
        gameActive = false;
        
        // Yıldız hesapla
        int stars = currentLevelConfig.GetStarsForHealth(playerHealth);
        
        // PlayerData'ya kaydet
        if (PlayerData.instance != null)
        {
            PlayerData.instance.CompleteLevel(currentLevel, stars, score);
        }
        
        if (gameCompletePanel != null)
        {
            gameCompletePanel.SetActive(true);
            
            // Level complete UI'sini güncelle
            UpdateLevelCompleteUI(stars);
        }
        
        Debug.Log($"Level Complete! Stars: {stars}, Score: {score}");
    }
    
    void UpdateLevelCompleteUI(int stars)
    {
        // Burada yıldızları ve sonuçları göster
        // Panel içindeki UI elementlerini güncelle
    }
    
    void UpdateUI()
    {
        int totalPieces = currentLevelConfig.GetTotalPieceCount();
        
        if (scoreText != null)
            scoreText.text = $"Score: {score}";
            
        if (pieceCountText != null)
            pieceCountText.text = $"Pieces: {piecesSpawned}/{totalPieces}";
            
        if (levelText != null)
            levelText.text = $"Level {currentLevel}";
    }
    
    void UpdateTimerUI()
    {
        if (timerText != null && currentLevelConfig.gameDuration > 0)
        {
            float remainingTime = currentLevelConfig.gameDuration - gameTimer;
            int minutes = Mathf.FloorToInt(remainingTime / 60);
            int seconds = Mathf.FloorToInt(remainingTime % 60);
            timerText.text = $"{minutes:00}:{seconds:00}";
        }
    }
    
    void UpdateHealthUI()
    {
        if (healthIcons != null)
        {
            for (int i = 0; i < healthIcons.Length; i++)
            {
                if (healthIcons[i] != null)
                {
                    healthIcons[i].SetActive(i < playerHealth);
                }
            }
        }
    }
    
    // Public methods
    public void OnPieceDestroyed(MonoBehaviour piece)
    {
        activeChessPieces.Remove(piece);
        
        // Skor ver
        score += GetPieceScore(piece);
        UpdateUI();
        
        Debug.Log($"Piece destroyed! Score: +{GetPieceScore(piece)}");
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
        
        // Aktif piece'lerle kontrol
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
    
    // UI Button methods
    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu"); // Ana menü scene'inin adı
    }
    
    public void LoadNextLevel()
    {
        UIManager.selectedLevel = currentLevel + 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}