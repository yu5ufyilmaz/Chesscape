using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject levelSelectionPanel;
    [SerializeField] private GameObject avatarSelectionPanel;
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject leaderboardPanel;
    [SerializeField] private GameObject gameUIPanel;
    [SerializeField] private GameObject notEnoughHealthPopup;
    
    [Header("Header UI")]
    [SerializeField] private Button avatarButton;
    [SerializeField] private Text usernameText;
    [SerializeField] private Text healthCountText;
    [SerializeField] private Text coinCountText;
    [SerializeField] private Text gemCountText;
    [SerializeField] private Slider healthRegenSlider;
    
    [Header("Bottom Navigation")]
    [SerializeField] private Button shopButton;
    [SerializeField] private Button leaderboardButton;
    [SerializeField] private Button playButton;
    
    [Header("Level Selection")]
    [SerializeField] private Transform levelButtonsParent;
    [SerializeField] private GameObject levelButtonPrefab;
    
    [Header("Game Data")]
    [SerializeField] private PlayerData playerData;
    
    // Singleton pattern
    public static UIManager instance;
    
    public static int selectedLevel = 1;
    
    private void Awake()
    {
        // Singleton setup
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        InitializeUI();
        SetupButtonEvents();
        GenerateLevelButtons();
        StartCoroutine(HealthRegenTimer());
    }
    
    public void LoadGameScene(int levelNumber)
    {
        if (!PlayerData.instance.CanPlayLevel())
        {
            ShowNotEnoughHealthPopup();
            return;
        }

        PlayerData.instance.UseHealth();
        
        selectedLevel = levelNumber;
  
        SceneManager.LoadScene("GameScene");
    }
    void InitializeUI()
    {
        // Start with level selection panel active
        ShowLevelSelection();
        UpdateHeaderUI();
        
        // Hide popups initially
        if (notEnoughHealthPopup != null)
            notEnoughHealthPopup.SetActive(false);
    }
    
    void SetupButtonEvents()
    {
        if (avatarButton != null)
            avatarButton.onClick.AddListener(ShowAvatarSelection);
            
        if (shopButton != null)
            shopButton.onClick.AddListener(ShowShop);
            
        if (leaderboardButton != null)
            leaderboardButton.onClick.AddListener(ShowLeaderboard);
            
        if (playButton != null)
            playButton.onClick.AddListener(ShowLevelSelection);
    }
    
    void GenerateLevelButtons()
    {
        if (levelButtonsParent == null || levelButtonPrefab == null)
        {
            Debug.LogWarning("Level buttons parent or prefab is not assigned!");
            return;
        }
        
        // Clear existing buttons
        foreach (Transform child in levelButtonsParent)
        {
            Destroy(child.gameObject);
        }
        
        // Create level buttons
        for (int i = 1; i <= 50; i++)
        {
            GameObject levelBtn = Instantiate(levelButtonPrefab, levelButtonsParent);
            LevelButton levelButton = levelBtn.GetComponent<LevelButton>();
            
            if (levelButton != null && playerData != null)
            {
                levelButton.SetupLevel(i, playerData.GetLevelData(i));
            }
        }
    }
    
    public void ShowLevelSelection()
    {
        HideAllPanels();
        if (levelSelectionPanel != null)
            levelSelectionPanel.SetActive(true);
    }
    
    public void ShowAvatarSelection()
    {
        if (avatarSelectionPanel != null)
            avatarSelectionPanel.SetActive(true);
    }
    
    public void ShowShop()
    {
        if (shopPanel != null)
            shopPanel.SetActive(true);
    }
    
    public void ShowLeaderboard()
    {
        if (leaderboardPanel != null)
            leaderboardPanel.SetActive(true);
    }
    
    public void StartGame()
    {
        HideAllPanels();
        if (gameUIPanel != null)
            gameUIPanel.SetActive(true);
            
        // ChessGameManager'ı başlat
        ChessGameManager gameManager = FindObjectOfType<ChessGameManager>();
        if (gameManager != null)
        {
            // Oyunu restart et veya başlat
            gameManager.RestartGame();
        }
    }
    
    public void ShowNotEnoughHealthPopup()
    {
        if (notEnoughHealthPopup != null)
        {
            notEnoughHealthPopup.SetActive(true);
            
            // 2 saniye sonra otomatik kapat
            StartCoroutine(HidePopupAfterDelay(2f));
        }
        else
        {
            Debug.LogWarning("Not enough health to play!");
        }
    }
    
    IEnumerator HidePopupAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (notEnoughHealthPopup != null)
            notEnoughHealthPopup.SetActive(false);
    }
    
    void HideAllPanels()
    {
        if (levelSelectionPanel != null)
            levelSelectionPanel.SetActive(false);
            
        if (avatarSelectionPanel != null)
            avatarSelectionPanel.SetActive(false);
            
        if (shopPanel != null)
            shopPanel.SetActive(false);
            
        if (leaderboardPanel != null)
            leaderboardPanel.SetActive(false);
            
        if (gameUIPanel != null)
            gameUIPanel.SetActive(false);
    }
    
    void UpdateHeaderUI()
    {
        if (playerData == null) return;
        
        if (usernameText != null)
            usernameText.text = playerData.username;
            
        if (healthCountText != null)
            healthCountText.text = playerData.currentHealth + "/" + playerData.maxHealth;
            
        if (coinCountText != null)
            coinCountText.text = playerData.coins.ToString();
            
        if (gemCountText != null)
            gemCountText.text = playerData.gems.ToString();
            
        // Health regen slider güncelle
        if (healthRegenSlider != null)
        {
            if (playerData.currentHealth >= playerData.maxHealth)
            {
                healthRegenSlider.gameObject.SetActive(false);
            }
            else
            {
                healthRegenSlider.gameObject.SetActive(true);
                // Slider değeri health regen timer'a göre ayarlanacak
            }
        }
    }
    
    IEnumerator HealthRegenTimer()
    {
        while (true)
        {
            if (playerData != null && playerData.currentHealth < playerData.maxHealth)
            {
                yield return new WaitForSeconds(playerData.healthRegenTime);
                playerData.currentHealth++;
                UpdateHeaderUI();
                Debug.Log("Health regenerated! Current health: " + playerData.currentHealth);
            }
            yield return new WaitForSeconds(1f);
        }
    }
    
    // Public method to update UI when data changes
    public void RefreshUI()
    {
        UpdateHeaderUI();
        GenerateLevelButtons();
    }
    
    // Close panel methods for UI buttons
    public void CloseAvatarPanel()
    {
        if (avatarSelectionPanel != null)
            avatarSelectionPanel.SetActive(false);
    }
    
    public void CloseShopPanel()
    {
        if (shopPanel != null)
            shopPanel.SetActive(false);
    }
    
    public void CloseLeaderboardPanel()
    {
        if (leaderboardPanel != null)
            leaderboardPanel.SetActive(false);
    }
    
    public void CloseNotEnoughHealthPopup()
    {
        if (notEnoughHealthPopup != null)
            notEnoughHealthPopup.SetActive(false);
    }
}