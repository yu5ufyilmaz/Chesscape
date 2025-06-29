using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RookPiece : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private Image pieceImage;
    [SerializeField] private Color pieceColor = Color.blue;
    
    [Header("Movement Settings")]
    [SerializeField] private float moveAnimationDuration = 0.25f;
    [SerializeField] private float chessMoveDelay = 0.8f; // Kale daha hızlı chess mode'a geçer
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
        Debug.Log($"Rook initialized at {gridPosition}");
    }
    
    public void Fall()
    {
        if (isMoving || manager == null) return;
        
        if (isInChessMode)
        {
            MakeRookMove();
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
        
        Debug.Log($"Rook at position: {gridPosition}, chess mode: {isInChessMode}");
    }
    
    IEnumerator ChessModeDelay()
    {
        yield return new WaitForSeconds(chessMoveDelay);
        isInChessMode = true;
        Debug.Log("Rook entered chess mode");
    }
    
    void MakeRookMove()
    {
        // Kale: Aynı kolonda en uzak noktaya atlar (2-3 kare)
        int jumpDistance = Random.Range(2, 4); // 2 veya 3 kare
        Vector2Int targetMove = new Vector2Int(gridPosition.x, gridPosition.y + jumpDistance);
        
        // Eğer çok uzaksa, mümkün olan en uzak noktaya git
        for (int distance = jumpDistance; distance >= 1; distance--)
        {
            Vector2Int testMove = new Vector2Int(gridPosition.x, gridPosition.y + distance);
            if (IsValidMove(testMove))
            {
                gridPosition = testMove;
                MoveToGridCell(gridPosition);
                Debug.Log($"Rook jumped {distance} steps down");
                return;
            }
        }
        
        // Hiçbir hamle yoksa yok ol
        manager.OnPieceDestroyed(this);
        DestroyPiece();
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
            yield return null;
        }
        
        rectTransform.anchoredPosition = targetPosition;
        rectTransform.localScale = Vector3.one;
        isMoving = false;
    }
    
    public void DestroyPiece()
    {
        StartCoroutine(DestroyAnimation());
    }
    
    IEnumerator DestroyAnimation()
    {
        float duration = 0.4f;
        float elapsed = 0f;
        Vector2 originalSize = rectTransform.sizeDelta;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            rectTransform.sizeDelta = Vector2.Lerp(originalSize, Vector2.zero, t);
            rectTransform.rotation = Quaternion.Lerp(Quaternion.identity, Quaternion.Euler(0, 0, 180f), t);
            
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