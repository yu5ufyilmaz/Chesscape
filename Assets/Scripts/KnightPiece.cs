using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class KnightPiece : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private Image pieceImage;
    [SerializeField] private Color pieceColor = Color.yellow;
    
    [Header("Movement Settings")]
    [SerializeField] private float moveAnimationDuration = 0.4f;
    [SerializeField] private float chessMoveDelay = 0.5f; // At en hızlı chess mode'a geçer
    [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    private Vector2Int gridPosition;
    private ChessGameManager manager;
    private RectTransform rectTransform;
    private bool isMoving = false;
    private bool hasReachedGrid = false;
    private bool isInChessMode = false;
    
    private const int GRID_WIDTH = 4;
    private const int GRID_HEIGHT = 4;
    
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        
        if (pieceImage == null)
        {
            pieceImage = GetComponent<Image>();
            if (pieceImage == null)
            {
                pieceImage = gameObject.AddComponent<Image>();
            }
        }
        
        pieceImage.color = pieceColor;
        rectTransform.sizeDelta = new Vector2(70f, 70f);
    }
    
    public void Initialize(Vector2Int startGridPos, ChessGameManager gameManager)
    {
        gridPosition = startGridPos;
        manager = gameManager;
        Debug.Log($"Knight initialized at {gridPosition}");
    }
    
    public void Fall()
    {
        if (isMoving || manager == null) return;
        
        if (isInChessMode)
        {
            MakeKnightMove();
            return;
        }
        
        Vector2Int nextPosition = new Vector2Int(gridPosition.x, gridPosition.y + 1);
        
        if (gridPosition.y < 0)
        {
            nextPosition = new Vector2Int(gridPosition.x, 0);
        }
        
        if (nextPosition.y >= GRID_HEIGHT)
        {
            manager.OnPieceDestroyed(this);
            DestroyPiece();
            return;
        }
        
        if (nextPosition.y >= 0 && manager.IsGridPositionOccupied(nextPosition))
        {
            hasReachedGrid = true;
            isInChessMode = true;
            StartCoroutine(ChessModeDelay());
            return;
        }
        
        gridPosition = nextPosition;
        MoveToGridCell(gridPosition);
        
        if (gridPosition.y >= 0 && !hasReachedGrid)
        {
            hasReachedGrid = true;
            StartCoroutine(ChessModeDelay());
        }
        
        Debug.Log($"Knight at position: {gridPosition}, chess mode: {isInChessMode}");
    }
    
    IEnumerator ChessModeDelay()
    {
        yield return new WaitForSeconds(chessMoveDelay);
        isInChessMode = true;
        Debug.Log("Knight entered chess mode");
    }
    
    void MakeKnightMove()
    {
        // At: Sadece aşağı doğru L hamleleri - en öngörülemez
        List<Vector2Int> possibleMoves = new List<Vector2Int>();
        
        // Aşağı doğru L hamleleri
        Vector2Int[] knightMoves = {
            new Vector2Int(-2, 1),  // 2 sol, 1 aşağı
            new Vector2Int(-1, 2),  // 1 sol, 2 aşağı
            new Vector2Int(1, 2),   // 1 sağ, 2 aşağı
            new Vector2Int(2, 1),   // 2 sağ, 1 aşağı
        };
        
        foreach (Vector2Int move in knightMoves)
        {
            Vector2Int targetPos = gridPosition + move;
            if (IsValidMove(targetPos))
            {
                possibleMoves.Add(targetPos);
            }
        }
        
        if (possibleMoves.Count > 0)
        {
            Vector2Int playerPos = manager.GetPlayerPosition();
            
            // %80 ihtimalle player'a en yakın L hamlesini seç
            if (playerPos != Vector2Int.zero && Random.Range(0f, 1f) < 0.8f)
            {
                possibleMoves.Sort((a, b) => 
                    Vector2Int.Distance(a, playerPos).CompareTo(Vector2Int.Distance(b, playerPos))
                );
                gridPosition = possibleMoves[0];
                Debug.Log("Knight targeted player with L-move");
            }
            else
            {
                gridPosition = possibleMoves[Random.Range(0, possibleMoves.Count)];
                Debug.Log("Knight made random L-move");
            }
            
            MoveToGridCell(gridPosition);
        }
        else
        {
            // L hamlesi yoksa normal aşağı dene
            Vector2Int straightDown = new Vector2Int(gridPosition.x, gridPosition.y + 1);
            if (IsValidMove(straightDown))
            {
                gridPosition = straightDown;
                MoveToGridCell(gridPosition);
                Debug.Log("Knight moved straight down");
            }
            else
            {
                // Hiçbir hamle yoksa yok ol
                manager.OnPieceDestroyed(this);
                DestroyPiece();
            }
        }
    }
    
    bool IsValidMove(Vector2Int move)
    {
        if (move.x < 0 || move.x >= GRID_WIDTH || move.y < 0 || move.y >= GRID_HEIGHT)
            return false;
        
        if (manager.IsGridPositionOccupied(move))
            return false;
        
        return true;
    }
    
    void MoveToGridCell(Vector2Int gridPos)
    {
        if (manager == null) return;
        
        RectTransform targetCell = manager.GetGridCell(gridPos);
        if (targetCell != null)
        {
            transform.SetParent(targetCell);
            StartCoroutine(AnimateToCenter());
        }
    }
    
    IEnumerator AnimateToCenter()
    {
        isMoving = true;
        Vector2 startPosition = rectTransform.anchoredPosition;
        Vector2 targetPosition = Vector2.zero;
        float elapsed = 0f;
        
        // L şekli hareket animasyonu - yay çizer
        Vector2 midPoint = Vector2.Lerp(startPosition, targetPosition, 0.5f);
        midPoint += new Vector2(Random.Range(-30f, 30f), -20f); // L şekli efekti
        
        while (elapsed < moveAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveAnimationDuration;
            float curveValue = moveCurve.Evaluate(t);
            
            // Bezier curve ile L şekli hareket
            Vector2 currentPos;
            if (t < 0.5f)
            {
                currentPos = Vector2.Lerp(startPosition, midPoint, t * 2f);
            }
            else
            {
                currentPos = Vector2.Lerp(midPoint, targetPosition, (t - 0.5f) * 2f);
            }
            
            rectTransform.anchoredPosition = currentPos;
            
            // L hareket efekti - döndür
            rectTransform.rotation = Quaternion.Lerp(Quaternion.identity, Quaternion.Euler(0, 0, 30f), Mathf.PingPong(t * 3f, 1f));
            
            yield return null;
        }
        
        rectTransform.anchoredPosition = targetPosition;
        rectTransform.rotation = Quaternion.identity;
        rectTransform.localScale = Vector3.one;
        isMoving = false;
    }
    
    public void DestroyPiece()
    {
        StartCoroutine(DestroyAnimation());
    }
    
    IEnumerator DestroyAnimation()
    {
        float duration = 0.5f;
        float elapsed = 0f;
        Vector2 originalSize = rectTransform.sizeDelta;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // L şekli kaybolma efekti
            rectTransform.sizeDelta = Vector2.Lerp(originalSize, Vector2.zero, t);
            rectTransform.rotation = Quaternion.Lerp(Quaternion.identity, Quaternion.Euler(0, 0, 720f), t);
            
            // Zıplama efekti
            Vector2 bounce = Vector2.up * Mathf.Sin(t * Mathf.PI * 3f) * 10f * (1f - t);
            rectTransform.anchoredPosition = bounce;
            
            Color color = pieceImage.color;
            color.a = Mathf.Lerp(1f, 0f, t);
            pieceImage.color = color;
            
            yield return null;
        }
        
        Destroy(gameObject);
    }
    
    public Vector2Int GetGridPosition() => gridPosition;
    public bool IsMoving() => isMoving;
    public bool IsInChessMode() => isInChessMode;
}