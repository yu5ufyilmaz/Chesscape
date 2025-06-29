using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class FallingObjectsManager : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject fallingObjectPrefab;
    [SerializeField] private RectTransform[] spawnCells; // Yukarıdaki 4 spawn karesi
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private float fallSpeed = 1f;
    [SerializeField] private float difficultyIncreaseRate = 0.1f;
    
    [Header("Grid References")]
    [SerializeField] private Transform gridParent;
    [SerializeField] private RectTransform[] gridCells; // GridPlayerMovement'tan aynı array
    [SerializeField] private GridPlayerMovement playerMovement;
    
    [Header("Game Settings")]
    [SerializeField] private bool gameActive = true;
    [SerializeField] private int score = 0;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI gameOverText;
    
    // Grid sistem
    private const int GRID_WIDTH = 4;
    private const int GRID_HEIGHT = 4;
    
    // Aktif düşen objeler
    private List<FallingObject> activeFallingObjects = new List<FallingObject>();
    
    // Grid durumu - hangi hücrelerde obje var
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
    }
    
    void Update()
    {
        if (!gameActive) return;
        
        // Oyuncu çarpışma kontrolü
        CheckPlayerCollision();
        
        // Zorluk artırma
        IncreaseDifficulty();
    }
    
    void AutoFillSpawnCells()
    {
        // "SpawnGrid" veya "Spawn" isimli parent objeyi bul
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
        else
        {
            Debug.LogWarning("SpawnGrid veya Spawn parent objesi bulunamadı!");
        }
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
        }
    }
    
    IEnumerator SpawnRoutine()
    {
        while (gameActive)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnFallingObject();
        }
    }
    
    void SpawnFallingObject()
    {
        if (spawnCells == null || spawnCells.Length == 0) return;
        
        // Rastgele spawn pozisyonu seç (0-3 arası)
        int randomSpawnIndex = Random.Range(0, spawnCells.Length);
        RectTransform spawnCell = spawnCells[randomSpawnIndex];
        
        if (spawnCell == null) return;
        
        // Objeyi spawn cell'inde oluştur
        GameObject obj = Instantiate(fallingObjectPrefab, spawnCell.transform);
        
        // RectTransform'unu spawn cell'ine hizala
        RectTransform objRect = obj.GetComponent<RectTransform>();
        objRect.anchoredPosition = Vector2.zero; // Parent'ın merkezinde
        objRect.localScale = Vector3.one;
        
        // FallingObject component'ini ekle
        FallingObject fallingObj = obj.GetComponent<FallingObject>();
        if (fallingObj == null)
        {
            fallingObj = obj.AddComponent<FallingObject>();
        }
        
        // Grid pozisyonunu ayarla - spawn cell index'i grid'in üst sırasına karşılık gelir
        Vector2Int startGridPos = new Vector2Int(randomSpawnIndex, -1); // Grid'in üstünde başla
        fallingObj.Initialize(startGridPos, this);
        
        activeFallingObjects.Add(fallingObj);
        
        Debug.Log($"Spawned object at spawn cell {randomSpawnIndex}, target grid column: {randomSpawnIndex}");
    }
    
    IEnumerator FallRoutine()
    {
        while (gameActive)
        {
            yield return new WaitForSeconds(1f / fallSpeed);
            
            // Tüm objeler bir adım aşağı insin
            for (int i = activeFallingObjects.Count - 1; i >= 0; i--)
            {
                if (activeFallingObjects[i] != null)
                {
                    activeFallingObjects[i].Fall();
                }
            }
        }
    }
    
    public void OnObjectReachedBottom(FallingObject obj)
    {
        // Obje alt sınıra ulaştı veya çarpışma durdu
        Vector2Int gridPos = obj.GetGridPosition();
        
        if (IsValidGridPosition(gridPos))
        {
            // Grid'e yerleştir
            gridOccupied[gridPos.x, gridPos.y] = true;
            
            // Objeyi grid pozisyonuna taşı
            MoveObjectToGridPosition(obj, gridPos);
            
            // Skorunu artır
            score += 10;
            UpdateUI();
            
            // Aktif listeden çıkar
            activeFallingObjects.Remove(obj);
            
            Debug.Log($"Object placed at grid position: {gridPos}");
        }
        else
        {
            // Geçersiz pozisyon, objeyi yok et
            activeFallingObjects.Remove(obj);
            Destroy(obj.gameObject);
        }
    }
    
    public void OnObjectDestroyed(FallingObject obj)
    {
        // Obje grid'den geçip gitti - skor ver
        activeFallingObjects.Remove(obj);
        score += 5; // Grid'den geçme skoru
        UpdateUI();
        
        Debug.Log($"Object passed through grid. Score: +5");
    }
    
    void MoveObjectToGridPosition(FallingObject obj, Vector2Int gridPos)
    {
        RectTransform targetCell = GetGridCell(gridPos);
        
        if (targetCell != null)
        {
            // Parent'ı grid cell yap
            obj.transform.SetParent(targetCell);
            
            // Pozisyonu merkeze hizala
            RectTransform objRect = obj.GetComponent<RectTransform>();
            objRect.anchoredPosition = Vector2.zero;
            objRect.localScale = Vector3.one;
        }
    }
    
    void CheckPlayerCollision()
    {
        if (playerMovement == null) return;
        
        Vector2Int playerPos = playerMovement.GetCurrentGridPosition();
        
        // Düşen objelerle çarpışma kontrolü
        foreach (var obj in activeFallingObjects)
        {
            if (obj.GetGridPosition() == playerPos)
            {
                GameOver();
                return;
            }
        }
        
        // Yerleşmiş objelerle çarpışma kontrolü
        if (IsValidGridPosition(playerPos) && gridOccupied[playerPos.x, playerPos.y])
        {
            GameOver();
        }
    }
    
    void GameOver()
    {
        gameActive = false;
        
        if (gameOverText != null)
        {
            gameOverText.text = $"GAME OVER!\nScore: {score}\nTap to Restart";
            gameOverText.gameObject.SetActive(true);
        }
        
        Debug.Log($"Game Over! Final Score: {score}");
    }
    
    void IncreaseDifficulty()
    {
        // Zaman geçtikçe zorluk artır
        fallSpeed += difficultyIncreaseRate * Time.deltaTime;
        spawnInterval = Mathf.Max(0.5f, spawnInterval - difficultyIncreaseRate * Time.deltaTime);
    }
    
    void UpdateUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
    }
    
    bool IsValidGridPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < GRID_WIDTH && pos.y >= 0 && pos.y < GRID_HEIGHT;
    }
    
    public bool IsGridPositionOccupied(Vector2Int pos)
    {
        if (!IsValidGridPosition(pos)) return true;
        return gridOccupied[pos.x, pos.y];
    }
    
    public RectTransform GetGridCell(Vector2Int gridPos)
    {
        // Grid pozisyonunu array index'ine çevir
        int arrayIndex = (gridPos.y * GRID_WIDTH) + gridPos.x;
        
        if (arrayIndex >= 0 && arrayIndex < gridCells.Length && gridCells[arrayIndex] != null)
        {
            return gridCells[arrayIndex];
        }
        
        return null;
    }
    
    public Vector2 GetGridWorldPosition(Vector2Int gridPos)
    {
        // Bu metod artık daha az kullanılacak ama backward compatibility için kalıyor
        RectTransform cell = GetGridCell(gridPos);
        if (cell != null)
        {
            return cell.anchoredPosition;
        }
        
        // Eğer grid dışındaysa (spawn alanındaysa), spawn cell pozisyonunu döndür
        if (gridPos.y < 0 && gridPos.x >= 0 && gridPos.x < spawnCells.Length && spawnCells[gridPos.x] != null)
        {
            return spawnCells[gridPos.x].anchoredPosition;
        }
        
        return Vector2.zero;
    }
    
    // Public methods
    public void RestartGame()
    {
        if (gameActive) return;
        
        // Tüm objeleri temizle
        foreach (var obj in activeFallingObjects)
        {
            if (obj != null) Destroy(obj.gameObject);
        }
        activeFallingObjects.Clear();
        
        // Grid'i temizle
        gridOccupied = new bool[GRID_WIDTH, GRID_HEIGHT];
        
        // Yerleşmiş objeleri temizle
        FallingObject[] allObjects = FindObjectsOfType<FallingObject>();
        foreach (var obj in allObjects)
        {
            Destroy(obj.gameObject);
        }
        
        // Oyunu sıfırla
        score = 0;
        fallSpeed = 1f;
        spawnInterval = 2f;
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
    }
    
    // Touch input for restart
    void OnMouseDown()
    {
        if (!gameActive)
        {
            RestartGame();
        }
    }
}