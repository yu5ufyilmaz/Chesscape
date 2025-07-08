using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class LevelConfig
{
    [Header("Level Info")]
    public int levelNumber;
    public string levelName;
    public int targetScore = 100;
    
    [Header("Spawn Settings")]
    public float spawnInterval = 3f;           // Taş düşme aralığı (saniye)
    public float fallSpeed = 1f;              // Taşların düşme hızı
    public int maxPieces = 20;                // Bu levelde kaç taş düşecek
    public float gameDuration = 60f;          // Level süresi (saniye) - 0 ise sınırsız
    
    [Header("Piece Spawn Counts (Direkt Sayı)")]
    public int pawnCount = 10;      // Kaç tane Piyon gelecek
    public int rookCount = 3;       // Kaç tane Kale gelecek  
    public int bishopCount = 2;     // Kaç tane Fil gelecek
    public int knightCount = 0;     // Kaç tane At gelecek
    public int queenCount = 0;      // Kaç tane Vezir gelecek
    
    [Header("Difficulty Settings")]
    public bool enableQueenPieces = false;    // Vezir aktif mi
    public bool enableKnightPieces = true;    // At aktif mi
    public bool enableBishopPieces = true;    // Fil aktif mi
    public float difficultyMultiplier = 1f;   // Zorluk çarpanı
    
    [Header("Player Settings")]
    public int playerStartHealth = 3;         // Oyuncu başlangıç canı
    
    public float[] GetPieceRates()
    {
        // Toplam taş sayısını hesapla (sadece count > 0 olanları)
        int totalPieces = 0;
        if (pawnCount > 0) totalPieces += pawnCount;
        if (rookCount > 0) totalPieces += rookCount;
        if (bishopCount > 0) totalPieces += bishopCount;
        if (knightCount > 0) totalPieces += knightCount;
        if (queenCount > 0) totalPieces += queenCount;
        
        if (totalPieces == 0)
        {
            Debug.LogWarning("No pieces configured! Defaulting to 1 Pawn.");
            return new float[] { 1f, 0f, 0f, 0f, 0f };
        }
        
        // Her piece type'ın oranını hesapla (sadece count > 0 olanları)
        float[] rates = new float[5];
        rates[0] = pawnCount > 0 ? (float)pawnCount / totalPieces : 0f;     // Pawn oranı
        rates[1] = rookCount > 0 ? (float)rookCount / totalPieces : 0f;     // Rook oranı
        rates[2] = bishopCount > 0 ? (float)bishopCount / totalPieces : 0f; // Bishop oranı
        rates[3] = knightCount > 0 ? (float)knightCount / totalPieces : 0f; // Knight oranı
        rates[4] = queenCount > 0 ? (float)queenCount / totalPieces : 0f;   // Queen oranı
        
        Debug.Log($"Piece distribution - " +
                  $"Pawn: {pawnCount}/{totalPieces} ({rates[0]*100:F1}%), " +
                  $"Rook: {rookCount}/{totalPieces} ({rates[1]*100:F1}%), " +
                  $"Bishop: {bishopCount}/{totalPieces} ({rates[2]*100:F1}%), " +
                  $"Knight: {knightCount}/{totalPieces} ({rates[3]*100:F1}%), " +
                  $"Queen: {queenCount}/{totalPieces} ({rates[4]*100:F1}%)");
        
        return rates;
    }
    
    public int GetTotalPieceCount()
    {
        // Sadece count > 0 olan piece'leri say
        int total = 0;
        if (pawnCount > 0) total += pawnCount;
        if (rookCount > 0) total += rookCount;
        if (bishopCount > 0) total += bishopCount;
        if (knightCount > 0) total += knightCount;
        if (queenCount > 0) total += queenCount;
        return total;
    }
    
    public bool HasPieceType(PieceType type)
    {
        switch (type)
        {
            case PieceType.Pawn: return pawnCount > 0;
            case PieceType.Rook: return rookCount > 0;
            case PieceType.Bishop: return bishopCount > 0 && enableBishopPieces;
            case PieceType.Knight: return knightCount > 0 && enableKnightPieces;
            case PieceType.Queen: return queenCount > 0 && enableQueenPieces;
            default: return false;
        }
    }
    
    public int GetStarsForHealth(int remainingHealth)
    {
        if (remainingHealth >= 3) return 3;
        else if (remainingHealth >= 2) return 2;
        else if (remainingHealth >= 1) return 1;
        else return 0;
    }
}

[CreateAssetMenu(fileName = "New Level Config", menuName = "Game/Level Config")]
public class LevelConfigData : ScriptableObject
{
    [Header("All Level Configurations")]
    public List<LevelConfig> levels = new List<LevelConfig>();
    
    public LevelConfig GetLevelConfig(int levelNumber)
    {
        foreach (var level in levels)
        {
            if (level.levelNumber == levelNumber)
                return level;
        }
        
        // Default level config döndür
        LevelConfig defaultConfig = new LevelConfig();
        defaultConfig.levelNumber = levelNumber;
        defaultConfig.levelName = $"Level {levelNumber}";
        defaultConfig.spawnInterval = Mathf.Max(1f, 4f - (levelNumber * 0.1f)); // Her level biraz daha hızlı
        defaultConfig.fallSpeed = 1f + (levelNumber * 0.05f);
        defaultConfig.maxPieces = 15 + (levelNumber * 2);
        defaultConfig.gameDuration = 60f;
        
        // İlk 5 level sadece pawn ve rook
        if (levelNumber <= 5)
        {
            defaultConfig.enableBishopPieces = false;
            defaultConfig.enableKnightPieces = false;
            defaultConfig.enableQueenPieces = false;
            defaultConfig.pawnCount = 8;
            defaultConfig.rookCount = 2;
            defaultConfig.bishopCount = 0;
            defaultConfig.knightCount = 0;
            defaultConfig.queenCount = 0;
        }
        // 6-15 level bishop ekle
        else if (levelNumber <= 15)
        {
            defaultConfig.enableBishopPieces = true;
            defaultConfig.enableKnightPieces = false;
            defaultConfig.enableQueenPieces = false;
            defaultConfig.pawnCount = 10;
            defaultConfig.rookCount = 6;
            defaultConfig.bishopCount = 4;
            defaultConfig.knightCount = 0;
            defaultConfig.queenCount = 0;
        }
        // 16-30 level knight ekle
        else if (levelNumber <= 30)
        {
            defaultConfig.enableBishopPieces = true;
            defaultConfig.enableKnightPieces = true;
            defaultConfig.enableQueenPieces = false;
            defaultConfig.pawnCount = 12;
            defaultConfig.rookCount = 8;
            defaultConfig.bishopCount = 6;
            defaultConfig.knightCount = 4;
            defaultConfig.queenCount = 0;
        }
        // 31+ level queen ekle
        else
        {
            defaultConfig.enableBishopPieces = true;
            defaultConfig.enableKnightPieces = true;
            defaultConfig.enableQueenPieces = true;
            defaultConfig.pawnCount = 15;
            defaultConfig.rookCount = 10;
            defaultConfig.bishopCount = 8;
            defaultConfig.knightCount = 6;
            defaultConfig.queenCount = 3;
        }
        
        return defaultConfig;
    }
}