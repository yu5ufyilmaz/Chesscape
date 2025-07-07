using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class LevelData
{
    public bool isUnlocked;
    public int starsEarned;
    public int bestScore;
    public int difficulty;
    
    public LevelData()
    {
        isUnlocked = false;
        starsEarned = 0;
        bestScore = 0;
        difficulty = 0;
    }
}

[CreateAssetMenu(fileName = "PlayerData", menuName = "Game/Player Data")]
public class PlayerData : ScriptableObject
{
    [Header("Player Info")]
    public string username = "Player";
    public int selectedAvatarIndex = 0;
    
    [Header("Resources")]
    public int currentHealth = 5;
    public int maxHealth = 5;
    public int coins = 100;
    public int gems = 10;
    
    [Header("Health System")]
    public float healthRegenTime = 300f; // 5 minutes in seconds
    
    [Header("Level Progress")]
    public List<LevelData> levelDataList = new List<LevelData>();
    
    // Singleton instance
    public static PlayerData instance;
    
    void OnEnable()
    {
        // Set singleton instance
        instance = this;
        InitializeLevels();
    }
    
    void InitializeLevels()
    {
        // Initialize levels if list is empty
        if (levelDataList.Count == 0)
        {
            for (int i = 0; i < 50; i++)
            {
                LevelData newLevel = new LevelData();
                newLevel.isUnlocked = (i == 0); // Only first level unlocked initially
                newLevel.starsEarned = 0;
                newLevel.bestScore = 0;
                newLevel.difficulty = Mathf.FloorToInt(i / 10); // Difficulty increases every 10 levels
                levelDataList.Add(newLevel);
            }
            
            Debug.Log("Initialized " + levelDataList.Count + " levels");
        }
    }
    
    public LevelData GetLevelData(int levelIndex)
    {
        // Convert 1-based index to 0-based
        int arrayIndex = levelIndex - 1;
        
        if (arrayIndex >= 0 && arrayIndex < levelDataList.Count)
        {
            return levelDataList[arrayIndex];
        }
        
        // Return default level data if index is out of range
        Debug.LogWarning($"Level index {levelIndex} is out of range. Returning default level data.");
        return new LevelData();
    }
    
    public bool CanPlayLevel()
    {
        return currentHealth > 0;
    }
    
    public void UseHealth()
    {
        if (currentHealth > 0)
        {
            currentHealth--;
            Debug.Log("Health used. Remaining health: " + currentHealth);
            
            // Update UI if UIManager exists
            if (UIManager.instance != null)
            {
                UIManager.instance.RefreshUI();
            }
        }
    }
    
    public void AddHealth(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log("Health added. Current health: " + currentHealth);
        
        // Update UI if UIManager exists
        if (UIManager.instance != null)
        {
            UIManager.instance.RefreshUI();
        }
    }
    
    public void AddCoins(int amount)
    {
        coins += amount;
        Debug.Log("Coins added: " + amount + ". Total coins: " + coins);
        
        // Update UI if UIManager exists
        if (UIManager.instance != null)
        {
            UIManager.instance.RefreshUI();
        }
    }
    
    public void SpendCoins(int amount)
    {
        if (coins >= amount)
        {
            coins -= amount;
            Debug.Log("Coins spent: " + amount + ". Remaining coins: " + coins);
            
            // Update UI if UIManager exists
            if (UIManager.instance != null)
            {
                UIManager.instance.RefreshUI();
            }
        }
        else
        {
            Debug.LogWarning("Not enough coins to spend " + amount);
        }
    }
    
    public void AddGems(int amount)
    {
        gems += amount;
        Debug.Log("Gems added: " + amount + ". Total gems: " + gems);
        
        // Update UI if UIManager exists
        if (UIManager.instance != null)
        {
            UIManager.instance.RefreshUI();
        }
    }
    
    public void SpendGems(int amount)
    {
        if (gems >= amount)
        {
            gems -= amount;
            Debug.Log("Gems spent: " + amount + ". Remaining gems: " + gems);
            
            // Update UI if UIManager exists
            if (UIManager.instance != null)
            {
                UIManager.instance.RefreshUI();
            }
        }
        else
        {
            Debug.LogWarning("Not enough gems to spend " + amount);
        }
    }
    
    public void CompleteLevel(int levelIndex, int stars, int score)
    {
        // Convert 1-based index to 0-based
        int arrayIndex = levelIndex - 1;
        
        if (arrayIndex >= 0 && arrayIndex < levelDataList.Count)
        {
            LevelData level = levelDataList[arrayIndex];
            
            // Update stars (only if better than previous)
            level.starsEarned = Mathf.Max(level.starsEarned, stars);
            
            // Update best score
            level.bestScore = Mathf.Max(level.bestScore, score);
            
            // Unlock next level
            if (arrayIndex + 1 < levelDataList.Count)
            {
                levelDataList[arrayIndex + 1].isUnlocked = true;
                Debug.Log("Level " + (levelIndex + 1) + " unlocked!");
            }
            
            // Award coins based on stars
            int coinsEarned = stars * 10;
            AddCoins(coinsEarned);
            
            // Award gems for 3-star completion
            if (stars == 3)
            {
                AddGems(1);
            }
            
            Debug.Log($"Level {levelIndex} completed with {stars} stars and score {score}");
            
            // Update UI if UIManager exists
            if (UIManager.instance != null)
            {
                UIManager.instance.RefreshUI();
            }
        }
    }
    
    public void SetAvatarIndex(int index)
    {
        selectedAvatarIndex = index;
        Debug.Log("Avatar changed to index: " + index);
    }
    
    // Save/Load methods (you can implement these with PlayerPrefs or file system)
    public void SaveData()
    {
        // Save to PlayerPrefs or file
        PlayerPrefs.SetString("Username", username);
        PlayerPrefs.SetInt("CurrentHealth", currentHealth);
        PlayerPrefs.SetInt("Coins", coins);
        PlayerPrefs.SetInt("Gems", gems);
        PlayerPrefs.SetInt("SelectedAvatar", selectedAvatarIndex);
        
        // Save level data (you might want to use JSON for this)
        // For simplicity, we'll save just a few key values
        for (int i = 0; i < levelDataList.Count && i < 50; i++)
        {
            PlayerPrefs.SetInt($"Level_{i}_Unlocked", levelDataList[i].isUnlocked ? 1 : 0);
            PlayerPrefs.SetInt($"Level_{i}_Stars", levelDataList[i].starsEarned);
            PlayerPrefs.SetInt($"Level_{i}_Score", levelDataList[i].bestScore);
        }
        
        PlayerPrefs.Save();
        Debug.Log("Player data saved");
    }
    
    public void LoadData()
    {
        if (PlayerPrefs.HasKey("Username"))
        {
            username = PlayerPrefs.GetString("Username", "Player");
            currentHealth = PlayerPrefs.GetInt("CurrentHealth", maxHealth);
            coins = PlayerPrefs.GetInt("Coins", 100);
            gems = PlayerPrefs.GetInt("Gems", 10);
            selectedAvatarIndex = PlayerPrefs.GetInt("SelectedAvatar", 0);
            
            // Load level data
            for (int i = 0; i < levelDataList.Count && i < 50; i++)
            {
                levelDataList[i].isUnlocked = PlayerPrefs.GetInt($"Level_{i}_Unlocked", i == 0 ? 1 : 0) == 1;
                levelDataList[i].starsEarned = PlayerPrefs.GetInt($"Level_{i}_Stars", 0);
                levelDataList[i].bestScore = PlayerPrefs.GetInt($"Level_{i}_Score", 0);
            }
            
            Debug.Log("Player data loaded");
        }
    }
    
    // Reset method for testing
    [ContextMenu("Reset Player Data")]
    public void ResetData()
    {
        username = "Player";
        currentHealth = maxHealth;
        coins = 100;
        gems = 10;
        selectedAvatarIndex = 0;
        
        levelDataList.Clear();
        InitializeLevels();
        
        Debug.Log("Player data reset");
        
        // Update UI if UIManager exists
        if (UIManager.instance != null)
        {
            UIManager.instance.RefreshUI();
        }
    }
}