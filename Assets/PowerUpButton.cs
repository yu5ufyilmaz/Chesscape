using UnityEngine;
using UnityEngine.UI;
using System.Collections;
public class PowerUpButton : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button button;
    [SerializeField] private Image iconImage;
    [SerializeField] private Text countText;
    [SerializeField] private GameObject cooldownOverlay;
    [SerializeField] private GameObject disabledOverlay;
    [SerializeField] private Image cooldownFillImage;
    
    [Header("Power-up Settings")]
    [SerializeField] private string powerUpName = "Power-Up";
    [SerializeField] private Sprite powerUpIcon;
    [SerializeField] private int maxCount = 3;
    [SerializeField] private float cooldownTime = 5f;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color disabledColor = Color.gray;
    
    // Events
    public System.Action onPowerUpUsed;
    
    // Private variables
    private int currentCount;
    private bool isOnCooldown = false;
    private float cooldownTimer = 0f;
    
    void Awake()
    {
        // Auto-assign components if not assigned
        if (button == null)
            button = GetComponent<Button>();
            
        if (iconImage == null)
            iconImage = GetComponentInChildren<Image>();
            
        if (countText == null)
            countText = GetComponentInChildren<Text>();
    }
    
    void Start()
    {
        // Initialize power-up
        currentCount = maxCount;
        SetupButton();
        UpdateVisuals();
    }
    
    void Update()
    {
        // Update cooldown
        if (isOnCooldown)
        {
            cooldownTimer -= Time.deltaTime;
            
            // Update cooldown fill
            if (cooldownFillImage != null)
            {
                float fillAmount = cooldownTimer / cooldownTime;
                cooldownFillImage.fillAmount = fillAmount;
            }
            
            // Check if cooldown finished
            if (cooldownTimer <= 0f)
            {
                EndCooldown();
            }
        }
    }
    
    void SetupButton()
    {
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(Use);
        }
        
        // Set icon if provided
        if (iconImage != null && powerUpIcon != null)
        {
            iconImage.sprite = powerUpIcon;
        }
    }
    
    public void SetCount(int count)
    {
        currentCount = Mathf.Max(0, count);
        UpdateVisuals();
    }
    
    public void AddCount(int amount)
    {
        currentCount = Mathf.Min(currentCount + amount, maxCount);
        UpdateVisuals();
    }
    
    public bool CanUse()
    {
        return currentCount > 0 && !isOnCooldown && button != null && button.interactable;
    }
    
    public void Use()
    {
        if (!CanUse())
        {
            Debug.LogWarning($"Cannot use {powerUpName}: Count={currentCount}, Cooldown={isOnCooldown}");
            return;
        }
        
        // Use power-up
        currentCount--;
        StartCooldown();
        UpdateVisuals();
        
        // Trigger event
        onPowerUpUsed?.Invoke();
        
        Debug.Log($"{powerUpName} used! Remaining: {currentCount}");
        
        // Play use effect
        PlayUseEffect();
    }
    
    void StartCooldown()
    {
        isOnCooldown = true;
        cooldownTimer = cooldownTime;
        
        if (cooldownOverlay != null)
            cooldownOverlay.SetActive(true);
            
        if (cooldownFillImage != null)
        {
            cooldownFillImage.fillAmount = 1f;
            cooldownFillImage.gameObject.SetActive(true);
        }
    }
    
    void EndCooldown()
    {
        isOnCooldown = false;
        cooldownTimer = 0f;
        
        if (cooldownOverlay != null)
            cooldownOverlay.SetActive(false);
            
        if (cooldownFillImage != null)
            cooldownFillImage.gameObject.SetActive(false);
            
        UpdateVisuals();
    }
    
    void UpdateVisuals()
    {
        // Update count text
        if (countText != null)
        {
            countText.text = "x" + currentCount.ToString();
        }
        
        // Update button state
        bool canUse = CanUse();
        
        if (button != null)
        {
            button.interactable = canUse;
        }
        
        // Update disabled overlay
        if (disabledOverlay != null)
        {
            disabledOverlay.SetActive(currentCount == 0 && !isOnCooldown);
        }
        
        // Update visual colors
        if (iconImage != null)
        {
            iconImage.color = canUse ? normalColor : disabledColor;
        }
    }
    
    void PlayUseEffect()
    {
        // Simple scale animation
        StartCoroutine(ScaleEffect());
    }
    
    IEnumerator ScaleEffect()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * 1.2f;
        float duration = 0.1f;
        
        // Scale up
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }
        
        // Scale down
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }
        
        transform.localScale = originalScale;
    }
    
    // Public methods for external control
    public void ResetPowerUp()
    {
        currentCount = maxCount;
        isOnCooldown = false;
        cooldownTimer = 0f;
        
        if (cooldownOverlay != null)
            cooldownOverlay.SetActive(false);
            
        if (cooldownFillImage != null)
            cooldownFillImage.gameObject.SetActive(false);
            
        UpdateVisuals();
    }
    
    public void SetCooldownTime(float newCooldownTime)
    {
        cooldownTime = newCooldownTime;
    }
    
    public void SetMaxCount(int newMaxCount)
    {
        maxCount = newMaxCount;
        currentCount = Mathf.Min(currentCount, maxCount);
        UpdateVisuals();
    }
    
    public void ForceEndCooldown()
    {
        EndCooldown();
    }
    
    // Getters
    public int GetCurrentCount() => currentCount;
    public int GetMaxCount() => maxCount;
    public bool IsOnCooldown() => isOnCooldown;
    public float GetCooldownTimeRemaining() => cooldownTimer;
    public string GetPowerUpName() => powerUpName;
    
    // Debug info
    [ContextMenu("Use Power-Up")]
    void DebugUsePowerUp()
    {
        Use();
    }
    
    [ContextMenu("Reset Power-Up")]
    void DebugResetPowerUp()
    {
        ResetPowerUp();
    }
    
    [ContextMenu("Add Count")]
    void DebugAddCount()
    {
        AddCount(1);
    }
}