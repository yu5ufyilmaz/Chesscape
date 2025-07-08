#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelConfigData))]
public class LevelConfigInspector : Editor
{
    private bool showLevelList = true;
    private Vector2 scrollPosition;
    
    public override void OnInspectorGUI()
    {
        LevelConfigData levelConfigData = (LevelConfigData)target;
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Level Configuration Tools", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Default Levels (1-50)"))
        {
            AddDefaultLevels(levelConfigData);
        }
        
        if (GUILayout.Button("Clear All Levels"))
        {
            ClearAllLevels(levelConfigData);
        }
        
        if (GUILayout.Button("Add New Level"))
        {
            AddNewLevel(levelConfigData);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        // Level listesi toggle
        showLevelList = EditorGUILayout.Foldout(showLevelList, $"All Levels ({levelConfigData.levels.Count})", true);
        
        if (showLevelList && levelConfigData.levels != null && levelConfigData.levels.Count > 0)
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(400));
            
            for (int i = 0; i < levelConfigData.levels.Count; i++)
            {
                var level = levelConfigData.levels[i];
                if (level != null)
                {
                    EditorGUILayout.BeginVertical("box");
                    
                    // Level header
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"Level {level.levelNumber}", EditorStyles.boldLabel, GUILayout.Width(80));
                    
                    // Level name
                    level.levelName = EditorGUILayout.TextField(level.levelName, GUILayout.Width(150));
                    
                    // Delete button
                    GUI.color = Color.red;
                    if (GUILayout.Button("X", GUILayout.Width(25)))
                    {
                        levelConfigData.levels.RemoveAt(i);
                        EditorUtility.SetDirty(levelConfigData);
                        break;
                    }
                    GUI.color = Color.white;
                    
                    EditorGUILayout.EndHorizontal();
                    
                    // Basic settings
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Spawn Interval:", GUILayout.Width(100));
                    level.spawnInterval = EditorGUILayout.FloatField(level.spawnInterval, GUILayout.Width(60));
                    
                    EditorGUILayout.LabelField("Fall Speed:", GUILayout.Width(80));
                    level.fallSpeed = EditorGUILayout.FloatField(level.fallSpeed, GUILayout.Width(60));
                    
                    EditorGUILayout.LabelField("Duration:", GUILayout.Width(60));
                    level.gameDuration = EditorGUILayout.FloatField(level.gameDuration, GUILayout.Width(60));
                    EditorGUILayout.EndHorizontal();
                    
                    // Piece counts - BU KISIM ÖNEMLİ!
                    EditorGUILayout.LabelField("Piece Counts:", EditorStyles.boldLabel);
                    EditorGUILayout.BeginHorizontal();
                    
                    EditorGUILayout.LabelField("P:", GUILayout.Width(15));
                    level.pawnCount = EditorGUILayout.IntField(level.pawnCount, GUILayout.Width(40));
                    
                    EditorGUILayout.LabelField("R:", GUILayout.Width(15));
                    level.rookCount = EditorGUILayout.IntField(level.rookCount, GUILayout.Width(40));
                    
                    EditorGUILayout.LabelField("B:", GUILayout.Width(15));
                    level.bishopCount = EditorGUILayout.IntField(level.bishopCount, GUILayout.Width(40));
                    
                    EditorGUILayout.LabelField("N:", GUILayout.Width(15));
                    level.knightCount = EditorGUILayout.IntField(level.knightCount, GUILayout.Width(40));
                    
                    EditorGUILayout.LabelField("Q:", GUILayout.Width(15));
                    level.queenCount = EditorGUILayout.IntField(level.queenCount, GUILayout.Width(40));
                    
                    // Total count
                    int totalPieces = level.pawnCount + level.rookCount + level.bishopCount + level.knightCount + level.queenCount;
                    GUI.color = totalPieces > 0 ? Color.green : Color.yellow;
                    EditorGUILayout.LabelField($"= {totalPieces}", GUILayout.Width(50));
                    GUI.color = Color.white;
                    
                    EditorGUILayout.EndHorizontal();
                    
                    // Enable checkboxes
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Enable:", GUILayout.Width(50));
                    
                    EditorGUILayout.LabelField("Bishop:", GUILayout.Width(50));
                    level.enableBishopPieces = EditorGUILayout.Toggle(level.enableBishopPieces, GUILayout.Width(20));
                    
                    EditorGUILayout.LabelField("Knight:", GUILayout.Width(50));
                    level.enableKnightPieces = EditorGUILayout.Toggle(level.enableKnightPieces, GUILayout.Width(20));
                    
                    EditorGUILayout.LabelField("Queen:", GUILayout.Width(50));
                    level.enableQueenPieces = EditorGUILayout.Toggle(level.enableQueenPieces, GUILayout.Width(20));
                    
                    EditorGUILayout.EndHorizontal();
                    
                    // Quick actions
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Only Pawn", GUILayout.Width(80)))
                    {
                        SetOnlyPawn(level);
                    }
                    if (GUILayout.Button("Only Queen", GUILayout.Width(80)))
                    {
                        SetOnlyQueen(level);
                    }
                    if (GUILayout.Button("Balanced", GUILayout.Width(80)))
                    {
                        SetBalanced(level);
                    }
                    if (GUILayout.Button("Auto Set", GUILayout.Width(80)))
                    {
                        AutoSetPieceCounts(level);
                    }
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space();
                }
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        if (GUI.changed)
        {
            EditorUtility.SetDirty(levelConfigData);
        }
    }
    
    void AddNewLevel(LevelConfigData levelConfigData)
    {
        LevelConfig newLevel = new LevelConfig();
        newLevel.levelNumber = levelConfigData.levels.Count + 1;
        newLevel.levelName = $"Level {newLevel.levelNumber}";
        newLevel.spawnInterval = 2.0f;
        newLevel.fallSpeed = 1.0f;
        newLevel.gameDuration = 60f;
        newLevel.pawnCount = 10;
        newLevel.rookCount = 5;
        newLevel.bishopCount = 0;
        newLevel.knightCount = 0;
        newLevel.queenCount = 0;
        newLevel.playerStartHealth = 3;
        
        levelConfigData.levels.Add(newLevel);
        EditorUtility.SetDirty(levelConfigData);
    }
    
    void SetOnlyPawn(LevelConfig level)
    {
        int total = level.pawnCount + level.rookCount + level.bishopCount + level.knightCount + level.queenCount;
        if (total == 0) total = 15; // Default
        
        level.pawnCount = total;
        level.rookCount = 0;
        level.bishopCount = 0;
        level.knightCount = 0;
        level.queenCount = 0;
        
        EditorUtility.SetDirty(target);
    }
    
    void SetOnlyQueen(LevelConfig level)
    {
        int total = level.pawnCount + level.rookCount + level.bishopCount + level.knightCount + level.queenCount;
        if (total == 0) total = 10; // Default
        
        level.pawnCount = 0;
        level.rookCount = 0;
        level.bishopCount = 0;
        level.knightCount = 0;
        level.queenCount = total;
        level.enableQueenPieces = true;
        
        EditorUtility.SetDirty(target);
    }
    
    void SetBalanced(LevelConfig level)
    {
        int total = level.pawnCount + level.rookCount + level.bishopCount + level.knightCount + level.queenCount;
        if (total == 0) total = 20; // Default
        
        int perPiece = total / 5;
        level.pawnCount = perPiece;
        level.rookCount = perPiece;
        level.bishopCount = perPiece;
        level.knightCount = perPiece;
        level.queenCount = perPiece;
        level.enableBishopPieces = true;
        level.enableKnightPieces = true;
        level.enableQueenPieces = true;
        
        EditorUtility.SetDirty(target);
    }
    
    void AddDefaultLevels(LevelConfigData levelConfigData)
    {
        levelConfigData.levels.Clear();
        
        for (int i = 1; i <= 50; i++)
        {
            LevelConfig config = new LevelConfig();
            config.levelNumber = i;
            config.levelName = $"Level {i}";
            config.spawnInterval = Mathf.Max(0.8f, 4f - (i * 0.06f));
            config.fallSpeed = 0.8f + (i * 0.04f);
            config.gameDuration = 60f + (i * 2f);
            config.targetScore = 50 + (i * 10);
            config.playerStartHealth = 3;
            
            // Taş sayıları level grubuna göre
            if (i <= 5)
            {
                // İlk 5 level: Sadece Pawn ve Rook
                config.pawnCount = 7 + i;      // 8, 9, 10, 11, 12
                config.rookCount = 2 + (i/2);  // 2, 2, 3, 3, 4
                config.bishopCount = 0;
                config.knightCount = 0;
                config.queenCount = 0;
                config.enableBishopPieces = false;
                config.enableKnightPieces = false;
                config.enableQueenPieces = false;
            }
            else if (i <= 15)
            {
                // 6-15 level: Bishop eklenir
                config.pawnCount = 8 + i;        // Artan piyon sayısı
                config.rookCount = 4 + (i/3);    // Artan kale sayısı
                config.bishopCount = 2 + (i/5);  // Artan fil sayısı
                config.knightCount = 0;
                config.queenCount = 0;
                config.enableBishopPieces = true;
                config.enableKnightPieces = false;
                config.enableQueenPieces = false;
            }
            else if (i <= 30)
            {
                // 16-30 level: Knight eklenir
                config.pawnCount = 10 + i;       // Artan piyon
                config.rookCount = 6 + (i/4);    // Artan kale
                config.bishopCount = 4 + (i/5);  // Artan fil
                config.knightCount = 2 + (i/8);  // Artan at
                config.queenCount = 0;
                config.enableBishopPieces = true;
                config.enableKnightPieces = true;
                config.enableQueenPieces = false;
            }
            else
            {
                // 31+ level: Queen eklenir
                config.pawnCount = 12 + i;        // Artan piyon
                config.rookCount = 8 + (i/5);     // Artan kale
                config.bishopCount = 6 + (i/6);   // Artan fil
                config.knightCount = 4 + (i/7);   // Artan at
                config.queenCount = 1 + (i/10);   // Artan vezir
                config.enableBishopPieces = true;
                config.enableKnightPieces = true;
                config.enableQueenPieces = true;
            }
            
            levelConfigData.levels.Add(config);
        }
        
        EditorUtility.SetDirty(levelConfigData);
        Debug.Log("Added 50 default levels with piece count system!");
    }
    
    void AutoSetPieceCounts(LevelConfig level)
    {
        int levelNum = level.levelNumber;
        
        // Level grubuna göre otomatik sayı ayarla
        if (levelNum <= 5)
        {
            level.pawnCount = 7 + levelNum;
            level.rookCount = 2 + (levelNum/2);
            level.bishopCount = 0;
            level.knightCount = 0;
            level.queenCount = 0;
        }
        else if (levelNum <= 15)
        {
            level.pawnCount = 8 + levelNum;
            level.rookCount = 4 + (levelNum/3);
            level.bishopCount = 2 + (levelNum/5);
            level.knightCount = 0;
            level.queenCount = 0;
        }
        else if (levelNum <= 30)
        {
            level.pawnCount = 10 + levelNum;
            level.rookCount = 6 + (levelNum/4);
            level.bishopCount = 4 + (levelNum/5);
            level.knightCount = 2 + (levelNum/8);
            level.queenCount = 0;
        }
        else
        {
            level.pawnCount = 12 + levelNum;
            level.rookCount = 8 + (levelNum/5);
            level.bishopCount = 6 + (levelNum/6);
            level.knightCount = 4 + (levelNum/7);
            level.queenCount = 1 + (levelNum/10);
        }
        
        EditorUtility.SetDirty(target);
        Debug.Log($"Auto-set piece counts for Level {levelNum}. Total: {level.pawnCount + level.rookCount + level.bishopCount + level.knightCount + level.queenCount}");
    }
    
    void ClearAllLevels(LevelConfigData levelConfigData)
    {
        levelConfigData.levels.Clear();
        EditorUtility.SetDirty(levelConfigData);
        Debug.Log("All levels cleared!");
    }
}
#endif