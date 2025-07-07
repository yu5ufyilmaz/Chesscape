using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI levelNumberText;
    [SerializeField] private TextMeshProUGUI difficultyText;
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
        // Can kontrolü
        if (PlayerData.instance == null)
        {
            Debug.LogError("PlayerData instance is null!");
            return;
        }
        
        if (!PlayerData.instance.CanPlayLevel())
        {
            // Can yoksa popup göster
            UIManager uiManager = FindObjectOfType<UIManager>();
            if (uiManager != null)
            {
                uiManager.ShowNotEnoughHealthPopup();
            }
            else
            {
                Debug.LogWarning("Not enough health to play this level!");
            }
            return;
        }
        
        // UIManager üzerinden Game Scene'e geç
        UIManager uiManager2 = FindObjectOfType<UIManager>();
        if (uiManager2 != null)
        {
            uiManager2.LoadGameScene(levelNumber);
        }
        else
        {
            Debug.LogError("UIManager not found!");
        }
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