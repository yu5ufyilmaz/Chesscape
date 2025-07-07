using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameSceneManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Text levelNumberText;
    [SerializeField] private Text difficultyText;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text timerText;
    [SerializeField] private Text healthText;
    [SerializeField] private Text piecesSurvivedText;
    [SerializeField] private Text streakText;
    
    [Header("Buttons")]
    [SerializeField] private Button backButton;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    
    [Header("Panels")]
    [SerializeField] private GameObject gameUIPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private GameObject loadingPanel;
    
    [Header("Power-ups")]
    [SerializeField] private PowerUpButton clearRowButton;
    [SerializeField] private PowerUpButton freezeButton;
    [SerializeField] private PowerUpButton bombButton;
    
    [Header("Game References")]
    [SerializeField] private ChessGameManager chessGameManager;
    [SerializeField] private GridPlayerMovement playerMovement;
    
    [Header("Notifications")]
    [SerializeField] private Text comboNotificationText;
    [SerializeField] private Text powerUpNotificationText;
    [SerializeField] private GameObject notificationPanel;
    
    [Header("Game Over UI")]
    [SerializeField] private Text finalScoreText;
    [SerializeField] private Text finalTimeText;
    [SerializeField] private Text coinsEarnedText;
    [SerializeField] private GameObject[] starsDisplay;
    [SerializeField] private Button tryAgainButton;
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button gameOverMainMenuButton;
    
    // Game variables
    private int currentLevel;
    private int gameScore = 0;
    private int piecesSurvived = 0;
    private int currentStreak = 0;
    private float gameTime = 0f;
    private bool gameActive = false;
    private bool isPaused = false;
    
    // Static variable to receive level from MainMenu
    public static int selectedLevel = 1;
    
    void Start()
    {
        StartCoroutine(InitializeGame());
    }
    
    IEnumerator InitializeGame()
    {
        // Loading panel göster
        ShowLoadingPanel();
        
        // MainMenu'den gelen level bilgisini al
        currentLevel = selectedLevel;
        
        // UI'ı hazırla
        SetupUI();
        SetupButtons();
        SetupPowerUps();
        
        // Kısa loading delay
        yield return new WaitForSeconds(1.5f);
        
        // Oyunu başlat
        StartGame();
    }
    
    void SetupUI()
    {
        // Level bilgilerini set et
        if (levelNumberText != null)
            levelNumberText.text = "Level " + currentLevel;
        
        // Difficulty bilgisi
        if (PlayerData.instance != null && difficultyText != null)
        {
            LevelData levelData = PlayerData.instance.GetLevelData(currentLevel);
            difficultyText.text = GetDifficultyText(levelData.difficulty);
        }
        
        // Initial values
        UpdateScore(0);
        UpdatePiecesSurvived(0);
        UpdateStreak(0);
        UpdateHealth();
    }
    
    void SetupButtons()
    {
        // Navigation buttons
        if (backButton != null)
            backButton.onClick.AddListener(BackToMainMenu);
            
        if (pauseButton != null)
            pauseButton.onClick.AddListener(PauseGame);
            
        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);
            
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
            
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(BackToMainMenu);
        
        // Game over buttons
        if (tryAgainButton != null)
            tryAgainButton.onClick.AddListener(RestartGame);
            
        if (nextLevelButton != null)
            nextLevelButton.onClick.AddListener(LoadNextLevel);
            
        if (gameOverMainMenuButton != null)
            gameOverMainMenuButton.onClick.AddListener(BackToMainMenu);
    }
    
    void SetupPowerUps()
    {
        // Power-up button events
        if (clearRowButton != null)
        {
            clearRowButton.onPowerUpUsed += UseClearRowPowerUp;
            clearRowButton.SetCount(3);
        }
        
        if (freezeButton != null)
        {
            freezeButton.onPowerUpUsed += UseFreezePowerUp;
            freezeButton.SetCount(2);
        }
        
        if (bombButton != null)
        {
            bombButton.onPowerUpUsed += UseBombPowerUp;
            bombButton.SetCount(1);
        }
    }
    
    void ShowLoadingPanel()
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(true);
            
        if (gameUIPanel != null)
            gameUIPanel.SetActive(false);
            
        if (pausePanel != null)
            pausePanel.SetActive(false);
            
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
            
        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(false);
    }
    
    void StartGame()
    {
        // Loading'i gizle, Game UI'ı göster
        if (loadingPanel != null)
            loadingPanel.SetActive(false);
            
        if (gameUIPanel != null)
            gameUIPanel.SetActive(true);
        
        // Chess Game Manager'ı başlat
        if (chessGameManager != null)
        {
            chessGameManager.gameObject.SetActive(true);
            chessGameManager.RestartGame();
        }
        
        // Game variables'ı reset et
        gameScore = 0;
        piecesSurvived = 0;
        currentStreak = 0;
        gameTime = 0f;
        gameActive = true;
        isPaused = false;
        
        // UI'ı güncelle
        UpdateAllUI();
        
        Debug.Log($"Game started - Level {currentLevel}");
    }
    
    void Update()
    {
        if (gameActive && !isPaused)
        {
            // Timer güncelle
            gameTime += Time.deltaTime;
            UpdateTimer();
        }
    }
    
    void UpdateTimer()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(gameTime / 60f);
            int seconds = Mathf.FloorToInt(gameTime % 60f);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }
    
    void UpdateAllUI()
    {
        UpdateScore(gameScore);
        UpdatePiecesSurvived(piecesSurvived);
        UpdateStreak(currentStreak);
        UpdateHealth();
    }
    
    public void UpdateScore(int score)
    {
        gameScore = score;
        if (scoreText != null)
            scoreText.text = "Score: " + score.ToString("N0");
    }
    
    public void UpdatePiecesSurvived(int count)
    {
        piecesSurvived = count;
        if (piecesSurvivedText != null)
            piecesSurvivedText.text = "Survived: " + count;
    }
    
    public void UpdateStreak(int streak)
    {
        currentStreak = streak;
        if (streakText != null)
            streakText.text = "Streak: " + streak;
        
        // Show combo notification for high streaks
        if (streak > 2)
        {
            ShowComboNotification("x" + streak + " Combo!");
        }
    }
    
    void UpdateHealth()
    {
        if (PlayerData.instance != null && healthText != null)
        {
            healthText.text = PlayerData.instance.currentHealth + "/" + PlayerData.instance.maxHealth;
        }
    }
    
    public void ShowComboNotification(string text)
    {
        if (comboNotificationText != null)
        {
            comboNotificationText.text = text;
            StartCoroutine(ShowNotificationTemporary(comboNotificationText, 2f));
        }
    }
    
    public void ShowPowerUpNotification(string powerUpName)
    {
        if (powerUpNotificationText != null)
        {
            powerUpNotificationText.text = powerUpName + " Activated!";
            StartCoroutine(ShowNotificationTemporary(powerUpNotificationText, 1.5f));
        }
    }
    
    IEnumerator ShowNotificationTemporary(Text notification, float duration)
    {
        notification.gameObject.SetActive(true);
        yield return new WaitForSeconds(duration);
        notification.gameObject.SetActive(false);
    }
    
    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        
        if (pausePanel != null)
            pausePanel.SetActive(true);
    }
    
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        
        if (pausePanel != null)
            pausePanel.SetActive(false);
    }
    
    public void RestartGame()
    {
        // Can kontrolü
        if (PlayerData.instance == null || !PlayerData.instance.CanPlayLevel())
        {
            // Can yoksa main menu'ye gönder
            ShowNotification("Not enough health!");
            StartCoroutine(DelayedBackToMainMenu(2f));
            return;
        }
        
        // Can kullan
        PlayerData.instance.UseHealth();
        
        // Game'i restart et
        Time.timeScale = 1f;
        
        // Panel'ları kapat
        if (pausePanel != null)
            pausePanel.SetActive(false);
            
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
            
        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(false);
        
        // Oyunu yeniden başlat
        StartGame();
    }
    
    public void LoadNextLevel()
    {
        // Sonraki level'ı yükle
        selectedLevel = currentLevel + 1;
        
        // Aynı sahneyi reload et
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void BackToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
    
    IEnumerator DelayedBackToMainMenu(float delay)
    {
        yield return new WaitForSeconds(delay);
        BackToMainMenu();
    }
    
    public void OnLevelComplete(int finalScore, int stars)
    {
        gameActive = false;
        
        // Level'ı complete et
        if (PlayerData.instance != null)
        {
            PlayerData.instance.CompleteLevel(currentLevel, stars, finalScore);
        }
        
        // Level complete panel'ı göster
        ShowLevelCompletePanel(finalScore, stars);
    }
    
    public void OnGameOver()
    {
        gameActive = false;
        
        // Game over panel'ı göster
        ShowGameOverPanel();
    }
    
    void ShowLevelCompletePanel(int finalScore, int stars)
    {
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);
        }
        else if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        
        // Final stats
        if (finalScoreText != null)
            finalScoreText.text = "Final Score: " + finalScore.ToString("N0");
            
        if (finalTimeText != null)
            finalTimeText.text = "Time: " + GetFormattedTime();
        
        // Stars display
        if (starsDisplay != null)
        {
            for (int i = 0; i < starsDisplay.Length; i++)
            {
                if (starsDisplay[i] != null)
                    starsDisplay[i].SetActive(i < stars);
            }
        }
        
        // Button states
        if (tryAgainButton != null)
            tryAgainButton.interactable = PlayerData.instance != null && PlayerData.instance.CanPlayLevel();
            
        if (nextLevelButton != null)
            nextLevelButton.gameObject.SetActive(true);
        
        // Calculate coins earned
        int coinsEarned = stars * 10 + Mathf.FloorToInt(finalScore / 100f);
        if (coinsEarnedText != null)
            coinsEarnedText.text = "Coins Earned: " + coinsEarned;
    }
    
    void ShowGameOverPanel()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        
        // Final stats
        if (finalScoreText != null)
            finalScoreText.text = "Final Score: " + gameScore.ToString("N0");
            
        if (finalTimeText != null)
            finalTimeText.text = "Time Played: " + GetFormattedTime();
        
        // Hide stars for game over
        if (starsDisplay != null)
        {
            for (int i = 0; i < starsDisplay.Length; i++)
            {
                if (starsDisplay[i] != null)
                    starsDisplay[i].SetActive(false);
            }
        }
        
        // Button states
        if (tryAgainButton != null)
            tryAgainButton.interactable = PlayerData.instance != null && PlayerData.instance.CanPlayLevel();
            
        if (nextLevelButton != null)
            nextLevelButton.gameObject.SetActive(false);
        
        if (coinsEarnedText != null)
            coinsEarnedText.text = "Coins Earned: 0";
    }
    
    string GetFormattedTime()
    {
        int minutes = Mathf.FloorToInt(gameTime / 60f);
        int seconds = Mathf.FloorToInt(gameTime % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
    
    // Power-up implementations
    void UseClearRowPowerUp()
    {
        ShowPowerUpNotification("Clear Row");
        
        // Implement clear row logic with ChessGameManager
        if (chessGameManager != null)
        {
            // Clear bottom row - bu metodu ChessGameManager'a eklemeniz gerekebilir
            Debug.Log("Clear Row Power-up activated!");
            // chessGameManager.ClearBottomRow();
        }
    }
    
    void UseFreezePowerUp()
    {
        ShowPowerUpNotification("Freeze");
        
        // Implement freeze logic
        if (chessGameManager != null)
        {
            // Freeze pieces for 3 seconds - bu metodu ChessGameManager'a eklemeniz gerekebilir
            Debug.Log("Freeze Power-up activated!");
            // chessGameManager.FreezePieces(3f);
        }
    }
    
    void UseBombPowerUp()
    {
        ShowPowerUpNotification("Bomb");
        
        // Implement bomb logic
        if (chessGameManager != null && playerMovement != null)
        {
            // Explode around player position - bu metodu ChessGameManager'a eklemeniz gerekebilir
            Vector2Int playerPos = playerMovement.GetCurrentGridPosition();
            Debug.Log("Bomb Power-up activated at position: " + playerPos);
            // chessGameManager.ExplodeArea(playerPos, 1);
        }
    }
    
    void ShowNotification(string message)
    {
        Debug.Log("Notification: " + message);
        // Implement notification display
    }
    
    string GetDifficultyText(int difficulty)
    {
        switch (difficulty)
        {
            case 0: return "Easy";
            case 1: return "Normal";
            case 2: return "Hard";
            case 3: return "Very Hard";
            default: return "Normal";
        }
    }
    
    // Public methods for ChessGameManager to call
    public void OnPieceDestroyed(int scoreGained)
    {
        UpdateScore(gameScore + scoreGained);
        UpdatePiecesSurvived(piecesSurvived + 1);
        UpdateStreak(currentStreak + 1);
    }
    
    public void OnPlayerHit()
    {
        UpdateStreak(0);
        // Check for game over conditions
    }
    
    // Debug methods
    [ContextMenu("Test Level Complete")]
    void TestLevelComplete()
    {
        OnLevelComplete(gameScore, 3);
    }
    
    [ContextMenu("Test Game Over")]
    void TestGameOver()
    {
        OnGameOver();
    }
}