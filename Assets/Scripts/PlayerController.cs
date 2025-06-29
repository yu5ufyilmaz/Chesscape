using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Player Settings")]
    public float moveSpeed = 5f;
    
    private Vector2Int gridPosition = new Vector2Int(1, 2); // Alt ortada başla
    private GridManager gridManager;
    private bool canMove = true;
    
    void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
        UpdatePosition();
    }
    
    void Update()
    {
        if (!canMove) return;
        
        HandleInput();
    }
    
    void HandleInput()
    {
        Vector2Int moveDirection = Vector2Int.zero;
        
        // Klavye girişi
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            moveDirection = Vector2Int.up;
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            moveDirection = Vector2Int.down;
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            moveDirection = Vector2Int.left;
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            moveDirection = Vector2Int.right;
        
        // Touch girişi (mobil için)
        HandleTouchInput(ref moveDirection);
        
        if (moveDirection != Vector2Int.zero)
        {
            TryMove(moveDirection);
        }
    }
    
    void HandleTouchInput(ref Vector2Int moveDirection)
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                Vector3 touchWorldPos = Camera.main.ScreenToWorldPoint(touch.position);
                Vector2Int touchGridPos = gridManager.GetGridPosition(touchWorldPos);
                
                Vector2Int difference = touchGridPos - gridPosition;
                
                // En büyük farkı bul (sadece 1 kare hareket)
                if (Mathf.Abs(difference.x) > Mathf.Abs(difference.y))
                    moveDirection = new Vector2Int(difference.x > 0 ? 1 : -1, 0);
                else if (difference.y != 0)
                    moveDirection = new Vector2Int(0, difference.y > 0 ? 1 : -1);
            }
        }
    }
    
    void TryMove(Vector2Int direction)
    {
        Vector2Int newPosition = gridPosition + direction;
        
        if (gridManager.IsValidPosition(newPosition.x, newPosition.y))
        {
            gridPosition = newPosition;
            UpdatePosition();
            
            // GameManager'a hareket bildir
            GameManager.Instance?.OnPlayerMoved();
        }
    }
    
    void UpdatePosition()
    {
        Vector3 targetPos = gridManager.GetWorldPosition(gridPosition.x, gridPosition.y);
        transform.position = Vector3.Lerp(transform.position, targetPos, moveSpeed * Time.deltaTime);
    }
    
    public Vector2Int GetGridPosition()
    {
        return gridPosition;
    }
    
    public void SetGridPosition(Vector2Int newPos)
    {
        gridPosition = newPos;
        UpdatePosition();
    }
    
    public void SetCanMove(bool value)
    {
        canMove = value;
    }
}