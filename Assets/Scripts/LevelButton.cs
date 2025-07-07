using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Text levelNumberText;
    [SerializeField] private Text difficultyText;
    [SerializeField] private GameObject[] stars;
    [SerializeField] private GameObject lockIcon;
    [SerializeField] private Button levelButton;
    [SerializeField] private Image backgroundImage;
    
    [Header("Visual Settings")]
    [SerializeField] private Color unlockedColor = Color.white;
    [SerializeField] private Color lockedColor = Color.gray;
    [SerializeField] private Color completedColor = Color.green;
    
    private int levelNumber;
    private LevelData levelData;
    
    public void SetupLevel(int level, LevelData data)
    {
        levelNumber = level;
        levelData = data;
        
        if (levelNumberText != null)
            levelNumberText.text = level.ToString();
            
        if (difficultyText != null)
            difficultyText.text = GetDifficultyText(data.difficulty);
        
        // Setup stars
        if (stars != null)
        {
            for (int i = 0; i < stars.Length; i++)
            {
                if (stars[i] != null)
                    stars[i].SetActive(i < data.starsEarned);
            }
        }
        
        // Setup lock state
        bool isUnlocked = data.isUnlocked;
        if (lockIcon != null)
            lockIcon.SetActive(!isUnlocked);
            
        if (levelButton != null)
        {
            levelButton.interactable = isUnlocked;
            levelButton.onClick.RemoveAllListeners();
            levelButton.onClick.AddListener(() => OnLevelSelected());
        }
        
        // Visual state
        if (backgroundImage != null)
        {
            if (!isUnlocked)
            {
                backgroundImage.color = lockedColor;
            }
            else if (data.starsEarned > 0)
            {
                backgroundImage.color = completedColor;
            }
            else
            {
                backgroundImage.color = unlockedColor;
            }
        }
    }
    

    void OnLevelSelected()
    {
        if (PlayerData.instance == null)
        {
            Debug.LogError("PlayerData instance is null!");
            return;
        }
        
        if (PlayerData.instance.CanPlayLevel())
        {
            UIManager uiManager = FindObjectOfType<UIManager>();
            if (uiManager != null)
            {
                uiManager.LoadGameScene(levelNumber);
            }
        }
        else
        {
            UIManager uiManager = FindObjectOfType<UIManager>();
            if (uiManager != null)
            {
                uiManager.ShowNotEnoughHealthPopup();
            }
        }
    }
    
    void LoadLevel(int levelNumber)
    {
        // Can kullan
        PlayerData.instance.UseHealth();
        
        Debug.Log($"Loading Level {levelNumber}");
        
        // UI'yi oyun moduna geç
        UIManager uiManager = FindObjectOfType<UIManager>();
        if (uiManager != null)
        {
            uiManager.StartGame();
        }
        
        // Burada level yükleme mantığınızı ekleyebilirsiniz
        // Örneğin: SceneManager.LoadScene("GameScene");
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
}