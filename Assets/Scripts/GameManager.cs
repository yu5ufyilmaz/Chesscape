using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    [Header("Game Settings")]
    public float waveDelay = 2f;
    public GameObject enemyPrefab;
    
    [Header("UI References")]
    public Text waveText;
    public Text scoreText;
    public GameObject gameOverPanel;
    public GameObject nextWavePreview;
    
    [Header("Wave Configurations")]
    public WaveConfig[] waveConfigs;
    
    private int currentWave = 1;
    private int score = 0;
    private List<EnemyPiece> activeEnemies = new List<EnemyPiece>();
    private PlayerController player;
    private GridManager gridManager;
    private bool gameRunning = false;
    
    [System.Serializable]
    public class EnemyData
    {
        public PieceType type;
        public Vector2Int position;
    }
    
    [System.Serializable]
    public class WaveConfig
    {
        public EnemyData[] enemies;
    }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        gridManager = FindObjectOfType<GridManager>();
        StartGame();
    }
    
    public void StartGame()
    {
        currentWave = 1;
        score = 0;
        gameRunning = true;
        
        UpdateUI();
        StartWave();
    }
    
    void StartWave()
    {
        if (!gameRunning) return;
        
        ClearActiveEnemies();
        SpawnWaveEnemies();
        ShowNextWavePreview();
        
        // Düşmanları hareket ettir
        Invoke("MoveAllEnemies", 1f);
    }
    
    void SpawnWaveEnemies()
    {
        if (currentWave - 1 >= waveConfigs.Length) return;
        
        WaveConfig config = waveConfigs[currentWave - 1];
        
        foreach (var enemyData in config.enemies)
        {
            GameObject enemyObj = Instantiate(enemyPrefab, transform);
            EnemyPiece enemy = enemyObj.GetComponent<EnemyPiece>();
            enemy.Initialize(enemyData.position, enemyData.type);
            activeEnemies.Add(enemy);
        }
    }
    
    void MoveAllEnemies()
    {
        if (!gameRunning) return;
        
        foreach (var enemy in activeEnemies.ToArray())
        {
            if (enemy != null)
                enemy.MovePiece();
        }
        
        // Çarpışma kontrolü
        Invoke("CheckCollisions", 0.5f);
    }
    
    void CheckCollisions()
    {
        Vector2Int playerPos = player.GetGridPosition();
        
        foreach (var enemy in activeEnemies)
        {
            if (enemy != null && enemy.GetGridPosition() == playerPos)
            {
                GameOver();
                return;
            }
        }
        
        // Eğer tüm düşmanlar gittiyse yeni dalga
        if (activeEnemies.Count == 0)
        {
            score += currentWave * 100;
            currentWave++;
            UpdateUI();
            Invoke("StartWave", waveDelay);
        }
        else
        {
            // Düşmanlar hala varsa tekrar hareket ettir
            Invoke("MoveAllEnemies", 1.5f);
        }
    }
    
    void ShowNextWavePreview()
    {
        // Sonraki dalga önizlemesi göster
        if (nextWavePreview != null && currentWave < waveConfigs.Length)
        {
            // UI kodları buraya...
        }
    }
    
    public void OnPlayerMoved()
    {
        // Oyuncu hareket etti, gerekirse kontroller buraya
    }
    
    public void OnEnemyDestroyed(EnemyPiece enemy)
    {
        activeEnemies.Remove(enemy);
    }
    
    void GameOver()
    {
        gameRunning = false;
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }
    
    void ClearActiveEnemies()
    {
        foreach (var enemy in activeEnemies)
        {
            if (enemy != null)
                Destroy(enemy.gameObject);
        }
        activeEnemies.Clear();
    }
    
    void UpdateUI()
    {
        if (waveText != null)
            waveText.text = "Dalga: " + currentWave;
        if (scoreText != null)
            scoreText.text = "Skor: " + score;
    }
    
    public void RestartGame()
    {
        gameOverPanel.SetActive(false);
        StartGame();
    }
}