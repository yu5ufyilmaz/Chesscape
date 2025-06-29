using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FallingObject : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private Image objectImage;
    [SerializeField] private Color[] possibleColors = { Color.red, Color.blue, Color.green, Color.yellow, Color.magenta, Color.cyan };
    
    [Header("Animation Settings")]
    [SerializeField] private float moveAnimationDuration = 0.2f;
    [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    private Vector2Int gridPosition;
    private FallingObjectsManager manager;
    private RectTransform rectTransform;
    private bool isMoving = false;
    private bool hasReachedBottom = false;
    
    // Grid sistem
    private const int GRID_WIDTH = 4;
    private const int GRID_HEIGHT = 4;
    
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        
        // Image component'ini al veya ekle
        if (objectImage == null)
        {
            objectImage = GetComponent<Image>();
            if (objectImage == null)
            {
                objectImage = gameObject.AddComponent<Image>();
            }
        }
        
        // Rastgele renk seç
        SetRandomColor();
        
        // Boyut ayarla
        rectTransform.sizeDelta = new Vector2(80f, 80f);
    }
    
    public void Initialize(Vector2Int startGridPos, FallingObjectsManager gameManager)
    {
        gridPosition = startGridPos;
        manager = gameManager;
        
        // Başlangıç pozisyonu zaten spawn cell'inde ayarlandı
        // Ekstra pozisyon ayarlama gerekmiyor
        Debug.Log($"Initialized falling object at grid position: {gridPosition}");
    }
    
    void SetRandomColor()
    {
        if (possibleColors.Length > 0)
        {
            Color randomColor = possibleColors[Random.Range(0, possibleColors.Length)];
            objectImage.color = randomColor;
        }
    }
    
    public void Fall()
    {
        if (isMoving || hasReachedBottom || manager == null) return;
        
        Vector2Int nextPosition = new Vector2Int(gridPosition.x, gridPosition.y + 1);
        
        // Spawn alanından grid'e geçiş
        if (gridPosition.y < 0)
        {
            nextPosition = new Vector2Int(gridPosition.x, 0); // Grid'in ilk satırına geç
        }
        
        // Alt sınır kontrolü
        if (nextPosition.y >= GRID_HEIGHT)
        {
            // Alt sınıra ulaştı - objeyi yok et
            hasReachedBottom = true;
            manager.OnObjectDestroyed(this);
            DestroyObject();
            return;
        }
        
        // Çarpışma kontrolü (sadece grid içindeyse)
        if (nextPosition.y >= 0 && manager.IsGridPositionOccupied(nextPosition))
        {
            // Çarpışma var, burada dur
            hasReachedBottom = true;
            manager.OnObjectReachedBottom(this);
            return;
        }
        
        // Hareket et
        gridPosition = nextPosition;
        
        // Grid cell'in child'ı yap ve pozisyonu otomatik hizala
        MoveToGridCell(gridPosition);
        
        Debug.Log($"Object moved to grid position: {gridPosition}");
    }
    
    void MoveToGridCell(Vector2Int gridPos)
    {
        if (manager == null) return;
        
        // Target grid cell'i al
        RectTransform targetCell = manager.GetGridCell(gridPos);
        
        if (targetCell != null)
        {
            // Parent'ı grid cell yap
            transform.SetParent(targetCell);
            
            // Pozisyonu merkeze hizala (animasyonlu)
            StartCoroutine(AnimateToCenter());
        }
    }
    
    IEnumerator AnimateToCenter()
    {
        isMoving = true;
        Vector2 startPosition = rectTransform.anchoredPosition;
        Vector2 targetPosition = Vector2.zero; // Parent'ın merkezi
        float elapsed = 0f;
        
        while (elapsed < moveAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveAnimationDuration;
            
            // Easing curve kullan
            float curveValue = moveCurve.Evaluate(t);
            
            rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, curveValue);
            yield return null;
        }
        
        rectTransform.anchoredPosition = targetPosition;
        rectTransform.localScale = Vector3.one; // Scale'i normalize et
        isMoving = false;
    }
    
    IEnumerator AnimateMovement(Vector2 targetPosition)
    {
        // Bu method artık kullanılmıyor, AnimateToCenter kullanılıyor
        isMoving = true;
        Vector2 startPosition = rectTransform.anchoredPosition;
        float elapsed = 0f;
        
        while (elapsed < moveAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveAnimationDuration;
            
            // Easing curve kullan
            float curveValue = moveCurve.Evaluate(t);
            
            rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, curveValue);
            yield return null;
        }
        
        rectTransform.anchoredPosition = targetPosition;
        isMoving = false;
    }
    
    // Public getters
    public Vector2Int GetGridPosition() => gridPosition;
    public bool IsMoving() => isMoving;
    public bool HasReachedBottom() => hasReachedBottom;
    
    // Objeyi yok etme
    public void DestroyObject()
    {
        if (manager != null)
        {
            manager.OnObjectDestroyed(this);
        }
        
        // Direkt yok et (grid'den geçti)
        Destroy(gameObject);
    }
    
    IEnumerator DestroyAnimation()
    {
        // Küçülme animasyonu
        Vector2 originalSize = rectTransform.sizeDelta;
        float duration = 0.3f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Küçül ve şeffaflaş
            rectTransform.sizeDelta = Vector2.Lerp(originalSize, Vector2.zero, t);
            
            Color color = objectImage.color;
            color.a = Mathf.Lerp(1f, 0f, t);
            objectImage.color = color;
            
            yield return null;
        }
        
        Destroy(gameObject);
    }
    
    // Debug için
    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        
        // Grid pozisyonunu göster
        Gizmos.color = hasReachedBottom ? Color.red : Color.yellow;
        Vector3 worldPos = transform.position;
        Gizmos.DrawWireCube(worldPos, Vector3.one * 40f);
        
        // Grid koordinatlarını label olarak göster
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(worldPos + Vector3.up * 30f, $"Grid: {gridPosition}");
        #endif
    }
    
    // Çarpışma efekti
    public void OnCollisionWithPlayer()
    {
        // Player ile çarpışma efekti
        StartCoroutine(CollisionEffect());
    }
    
    IEnumerator CollisionEffect()
    {
        // Titreşim efekti
        Vector2 originalPos = rectTransform.anchoredPosition;
        float duration = 0.2f;
        float elapsed = 0f;
        float intensity = 10f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Rastgele titreşim
            Vector2 shake = new Vector2(
                Random.Range(-intensity, intensity),
                Random.Range(-intensity, intensity)
            ) * (1f - t); // Zamanla azalt
            
            rectTransform.anchoredPosition = originalPos + shake;
            
            // Renk değişimi
            Color flashColor = Color.white;
            objectImage.color = Color.Lerp(objectImage.color, flashColor, t * 5f);
            
            yield return null;
        }
        
        rectTransform.anchoredPosition = originalPos;
    }
}