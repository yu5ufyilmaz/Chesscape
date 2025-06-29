using UnityEngine;

public enum PieceType
{
    Pawn,
    Rook,
    Knight,
    Bishop
}

public class EnemyPiece : MonoBehaviour
{
    [Header("Enemy Settings")]
    public PieceType pieceType;
    public float moveSpeed = 3f;
    
    private Vector2Int gridPosition;
    private GridManager gridManager;
    private bool isMoving = false;
    
    void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
        UpdateVisualPosition();
    }
    
    public void Initialize(Vector2Int startPos, PieceType type)
    {
        gridPosition = startPos;
        pieceType = type;
        SetSpriteByType();
        UpdateVisualPosition();
    }
    
    void SetSpriteByType()
    {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer == null) return;
        
        // Sprite'ları piece type'a göre ayarla
        switch (pieceType)
        {
            case PieceType.Pawn:
                renderer.color = Color.red;
                break;
            case PieceType.Rook:
                renderer.color = Color.blue;
                break;
            case PieceType.Knight:
                renderer.color = Color.green;
                break;
            case PieceType.Bishop:
                renderer.color = Color.yellow;
                break;
        }
    }
    
    public void MovePiece()
    {
        if (isMoving) return;
        
        Vector2Int newPosition = CalculateNextMove();
        
        // Eğer tahtadan çıkıyorsa yok et
        if (!gridManager.IsValidPosition(newPosition.x, newPosition.y))
        {
            DestroyPiece();
            return;
        }
        
        gridPosition = newPosition;
        StartCoroutine(MoveToPosition());
    }
    
    Vector2Int CalculateNextMove()
    {
        switch (pieceType)
        {
            case PieceType.Pawn:
            case PieceType.Rook:
                // Düz aşağı
                return new Vector2Int(gridPosition.x, gridPosition.y + 1);
                
            case PieceType.Knight:
                // L şeklinde aşağı
                Vector2Int[] knightMoves = {
                    new Vector2Int(gridPosition.x + 1, gridPosition.y + 2),
                    new Vector2Int(gridPosition.x - 1, gridPosition.y + 2),
                    new Vector2Int(gridPosition.x + 2, gridPosition.y + 1),
                    new Vector2Int(gridPosition.x - 2, gridPosition.y + 1)
                };
                
                // Geçerli hareketi bul
                foreach (var move in knightMoves)
                {
                    if (move.y > gridPosition.y) // Sadece aşağı hareket
                        return move;
                }
                return new Vector2Int(gridPosition.x, gridPosition.y + 1); // Fallback
                
            case PieceType.Bishop:
                // Çapraz aşağı
                if (Random.value > 0.5f)
                    return new Vector2Int(gridPosition.x + 1, gridPosition.y + 1);
                else
                    return new Vector2Int(gridPosition.x - 1, gridPosition.y + 1);
                    
            default:
                return gridPosition;
        }
    }
    
    System.Collections.IEnumerator MoveToPosition()
    {
        isMoving = true;
        Vector3 startPos = transform.position;
        Vector3 targetPos = gridManager.GetWorldPosition(gridPosition.x, gridPosition.y);
        
        float elapsed = 0f;
        float duration = 1f / moveSpeed;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }
        
        transform.position = targetPos;
        isMoving = false;
    }
    
    void UpdateVisualPosition()
    {
        if (gridManager != null)
        {
            transform.position = gridManager.GetWorldPosition(gridPosition.x, gridPosition.y);
        }
    }
    
    void DestroyPiece()
    {
        GameManager.Instance?.OnEnemyDestroyed(this);
        Destroy(gameObject);
    }
    
    public Vector2Int GetGridPosition()
    {
        return gridPosition;
    }
}