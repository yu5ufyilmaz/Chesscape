using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Main Panels")]
    [SerializeField] private GameObject levelSelectionPanel; // ANA EKRAN - İlk açıldığında bu görünür
    [SerializeField] private GameObject shopPanel; // Sol alt butona tıklayınca
    [SerializeField] private GameObject leaderboardPanel; // Sağ alt butona tıklayınca
    [SerializeField] private GameObject avatarSelectionPanel; // Avatar butonuna tıklayınca
    [SerializeField] private GameObject notEnoughHealthPopup;
    
    [Header("Header UI - Always Visible")]
    [SerializeField] private Button avatarButton;
    [SerializeField] private TextMeshProUGUI usernameText;
    [SerializeField] private TextMeshProUGUI healthCountText;
    [SerializeField] private TextMeshProUGUI coinCountText;
    [SerializeField] private TextMeshProUGUI gemCountText;
    [SerializeField] private Slider healthRegenSlider;
    
    [Header("Bottom Navigation - Always Visible")]
    [SerializeField] private Button shopButton; // Sol alt
    [SerializeField] private Button playButton; // Orta (Level Selection'a geri döner)
    [SerializeField] private Button leaderboardButton; // Sağ alt
    
    [Header("Level Selection")]
    [SerializeField] private Transform levelButtonsParent;
    [SerializeField] private GameObject levelButtonPrefab;
    
    [Header("Game Data")]
    [SerializeField] private PlayerData playerData;
    
    // Singleton pattern
    public static UIManager instance;
    
    // Static değişken - hangi level'ı Game Scene'de yükleyeceğimizi bilmek için
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
    
    private void Update()
    {
        
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (playerData != null)
            {
                playerData.currentHealth = playerData.maxHealth;
                UpdateHeaderUI();
                Debug.Log("Health filled to maximum! Current health: " + playerData.currentHealth);
            }
            else
            {
                Debug.LogWarning("PlayerData is null, cannot fill health!");
            }
        }
#endif
    }
    
    void InitializeUI()
    {
        // İlk açıldığında Level Selection Panel aktif
        ShowLevelSelection();
        UpdateHeaderUI();
        
        // Hide popups initially
        if (notEnoughHealthPopup != null)
            notEnoughHealthPopup.SetActive(false);
    }
    
    void SetupButtonEvents()
    {
        // Header buttons
        if (avatarButton != null)
            avatarButton.onClick.AddListener(ShowAvatarSelection);
        
        // Bottom navigation buttons
        if (shopButton != null)
            shopButton.onClick.AddListener(ShowShop); // Sol alt → Shop Panel
            
        if (playButton != null)
            playButton.onClick.AddListener(ShowLevelSelection); // Orta → Level Selection (Ana ekran)
            
        if (leaderboardButton != null)
            leaderboardButton.onClick.AddListener(ShowLeaderboard); // Sağ alt → Leaderboard Panel
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
    
    // ===== PANEL MANAGEMENT =====
    
    public void ShowLevelSelection()
    {
        HideAllPanels();
        
        if (levelSelectionPanel != null)
            levelSelectionPanel.SetActive(true);
            
        Debug.Log("Level Selection Active (Main Screen)");
    }
    
    public void ShowShop()
    {
        HideAllPanels();
        
        if (shopPanel != null)
            shopPanel.SetActive(true);
            
        Debug.Log("Shop Panel Active");
    }
    
    public void ShowLeaderboard()
    {
        HideAllPanels();
        
        if (leaderboardPanel != null)
            leaderboardPanel.SetActive(true);
            
        Debug.Log("Leaderboard Panel Active");
    }
    
    public void ShowAvatarSelection()
    {
        // Avatar Selection overlay olarak açılır (diğer paneli kapatmaz)
        if (avatarSelectionPanel != null)
            avatarSelectionPanel.SetActive(true);
    }
    
    void HideAllPanels()
    {
        if (levelSelectionPanel != null)
            levelSelectionPanel.SetActive(false);
            
        if (shopPanel != null)
            shopPanel.SetActive(false);
            
        if (leaderboardPanel != null)
            leaderboardPanel.SetActive(false);
    }
    
    // ===== GAME SCENE LOADING =====
    
    public void LoadGameScene(int levelNumber)
    {
        if (playerData == null || !playerData.CanPlayLevel())
        {
            ShowNotEnoughHealthPopup();
            return;
        }
        
        // Can kullan
        playerData.UseHealth();
        
        // Hangi level seçildiğini kaydet
        selectedLevel = levelNumber;
        
        Debug.Log($"Loading Game Scene for Level {levelNumber}");
        
        // Game Scene'e geç
        SceneManager.LoadScene("GameScene");
    }
    
    // ===== POPUP MANAGEMENT =====
    
    public void ShowNotEnoughHealthPopup()
    {
        if (notEnoughHealthPopup != null)
        {
            notEnoughHealthPopup.SetActive(true);
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
    
    // ===== UI UPDATES =====
    
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
                playerData.AddHealth(1);
                UpdateHeaderUI();
                Debug.Log("Health regenerated! Current health: " + playerData.currentHealth);
            }
            yield return new WaitForSeconds(1f);
        }
    }
    
    public void RefreshUI()
    {
        UpdateHeaderUI();
        GenerateLevelButtons();
    }
    
    // ===== CLOSE PANEL METHODS =====
    
    public void CloseAvatarPanel()
    {
        if (avatarSelectionPanel != null)
            avatarSelectionPanel.SetActive(false);
    }
    
    public void CloseShopPanel()
    {
        // Shop panel'dan Level Selection'a geri dön
        ShowLevelSelection();
    }
    
    public void CloseLeaderboardPanel()
    {
        // Leaderboard panel'dan Level Selection'a geri dön
        ShowLevelSelection();
    }
    
    public void CloseNotEnoughHealthPopup()
    {
        if (notEnoughHealthPopup != null)
            notEnoughHealthPopup.SetActive(false);
    }
    
    // ===== PUBLIC METHODS FOR PANELS =====
    
    // Shop Panel'dan çağrılacak
    public void OnShopBackButton()
    {
        ShowLevelSelection();
    }
    
    // Leaderboard Panel'dan çağrılacak  
    public void OnLeaderboardBackButton()
    {
        ShowLevelSelection();
    }
    
    // ===== DEBUG METHODS =====
    
    [ContextMenu("Test Show Shop")]
    void TestShowShop()
    {
        ShowShop();
    }
    
    [ContextMenu("Test Show Leaderboard")]
    void TestShowLeaderboard()
    {
        ShowLeaderboard();
    }
    
    [ContextMenu("Test Show Level Selection")]
    void TestShowLevelSelection()
    {
        ShowLevelSelection();
    }
    
    [ContextMenu("Test Load Level 1")]
    void TestLoadLevel1()
    {
        LoadGameScene(1);
    }
}