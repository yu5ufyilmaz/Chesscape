using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class QueenPiece : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private Image pieceImage;
    [SerializeField] private Color pieceColor = Color.magenta;
    
    [Header("Movement Settings")]
    [SerializeField] private float moveAnimationDuration = 0.35f;
    [SerializeField] private float chessMoveDelay = 0.6f; // Vezir hızlı chess mode'a geçer
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
        rectTransform.sizeDelta = new Vector2(75f, 75f); // Vezir biraz daha büyük
    }
    
    public void Initialize(Vector2Int startGridPos, ChessGameManager gameManager)
    {
        gridPosition = startGridPos;
        manager = gameManager;
        Debug.Log($"Queen initialized at {gridPosition}");
    }
    
    public void Fall()
    {
        if (isMoving || manager == null) return;
        
        if (isInChessMode)
        {
            MakeQueenMove();
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
        
        Debug.Log($"Queen at position: {gridPosition}, chess mode: {isInChessMode}");
    }
    
    IEnumerator ChessModeDelay()
    {
        yield return new WaitForSeconds(chessMoveDelay);
        isInChessMode = true;
        Debug.Log("Queen entered chess mode");
    }
    
    void MakeQueenMove()
    {
        // Vezir: En güçlü - hem dikey hem çapraz, hem hızlı hem uzun mesafe
        List<Vector2Int> possibleMoves = new List<Vector2Int>();
        
        // Dikey aşağı (Kale hamlesi) - 1 ile 3 arası
        for (int distance = 1; distance <= 3; distance++)
        {
            Vector2Int verticalMove = new Vector2Int(gridPosition.x, gridPosition.y + distance);
            if (IsValidMove(verticalMove))
            {
                possibleMoves.Add(verticalMove);
            }
        }
        
        // Çapraz aşağı (Fil hamlesi) - 1 ile 2 arası
        for (int distance = 1; distance <= 2; distance++)
        {
            // Sol alt çapraz
            Vector2Int leftDiagonal = new Vector2Int(gridPosition.x - distance, gridPosition.y + distance);
            if (IsValidMove(leftDiagonal))
            {
                possibleMoves.Add(leftDiagonal);
            }
            
            // Sağ alt çapraz
            Vector2Int rightDiagonal = new Vector2Int(gridPosition.x + distance, gridPosition.y + distance);
            if (IsValidMove(rightDiagonal))
            {
                possibleMoves.Add(rightDiagonal);
            }
        }
        
        if (possibleMoves.Count > 0)
        {
            Vector2Int playerPos = manager.GetPlayerPosition();
            
            // %90 ihtimalle player'a en yakın hamleyi seç (çok agresif)
            if (playerPos != Vector2Int.zero && Random.Range(0f, 1f) < 0.9f)
            {
                possibleMoves.Sort((a, b) => 
                    Vector2Int.Distance(a, playerPos).CompareTo(Vector2Int.Distance(b, playerPos))
                );
                
                // En yakın 2 hamle arasından seç (unpredictability)
                int selectIndex = Random.Range(0, Mathf.Min(2, possibleMoves.Count));
                gridPosition = possibleMoves[selectIndex];
                Debug.Log("Queen aggressively targeted player");
            }
            else
            {
                // En uzak mesafe hamlesini seç (maksimum hareket)
                possibleMoves.Sort((a, b) => 
                    Vector2Int.Distance(gridPosition, b).CompareTo(Vector2Int.Distance(gridPosition, a))
                );
                gridPosition = possibleMoves[0];
                Debug.Log("Queen made maximum distance move");
            }
            
            MoveToGridCell(gridPosition);
        }
        else
        {
            // Hamle yoksa yok ol
            manager.OnPieceDestroyed(this);
            DestroyPiece();
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
        
        while (elapsed < moveAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveAnimationDuration;
            float curveValue = moveCurve.Evaluate(t);
            
            rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, curveValue);
            
            // Vezir efekti - parlama ve büyüme
            float pulse = 1f + Mathf.Sin(t * Mathf.PI * 6f) * 0.1f;
            rectTransform.localScale = Vector3.one * pulse;
            
            // Hafif parıltı efekti
            Color color = pieceImage.color;
            color.a = 0.8f + Mathf.Sin(t * Mathf.PI * 8f) * 0.2f;
            pieceImage.color = color;
            
            yield return null;
        }
        
        rectTransform.anchoredPosition = targetPosition;
        rectTransform.localScale = Vector3.one;
        
        // Rengi normale döndür
        Color finalColor = pieceColor;
        finalColor.a = 1f;
        pieceImage.color = finalColor;
        
        isMoving = false;
    }
    
    public void DestroyPiece()
    {
        StartCoroutine(DestroyAnimation());
    }
    
    IEnumerator DestroyAnimation()
    {
        float duration = 0.6f;
        float elapsed = 0f;
        Vector2 originalSize = rectTransform.sizeDelta;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Vezir kaybolma efekti - patlama benzeri
            if (t < 0.3f)
            {
                // Önce büyü
                float scale = Mathf.Lerp(1f, 1.5f, t / 0.3f);
                rectTransform.localScale = Vector3.one * scale;
            }
            else
            {
                // Sonra küçül
                float scale = Mathf.Lerp(1.5f, 0f, (t - 0.3f) / 0.7f);
                rectTransform.localScale = Vector3.one * scale;
            }
            
            // Döndürme efekti
            rectTransform.rotation = Quaternion.Lerp(Quaternion.identity, Quaternion.Euler(0, 0, 540f), t);
            
            // Parıltı efekti
            Color color = pieceImage.color;
            if (t < 0.5f)
            {
                color.a = 1f;
                color = Color.Lerp(pieceColor, Color.white, t * 2f);
            }
            else
            {
                color.a = Mathf.Lerp(1f, 0f, (t - 0.5f) * 2f);
            }
            pieceImage.color = color;
            
            yield return null;
        }
        
        Destroy(gameObject);
    }
    
    public Vector2Int GetGridPosition() => gridPosition;
    public bool IsMoving() => isMoving;
    public bool IsInChessMode() => isInChessMode;
}