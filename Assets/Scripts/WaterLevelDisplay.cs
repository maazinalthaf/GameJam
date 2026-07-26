using UnityEngine;

/// <summary>
/// Attach to your water layer (the visual water sprite).
/// Updates the visual to match GameManager.waterLevel with smooth transitions.
/// Also displays the rising baseline water level visually.
/// </summary>
public class WaterLevelDisplay : MonoBehaviour
{
    [Header("Water Visual Settings")]
    [Tooltip("The SpriteRenderer that represents the water")]
    public SpriteRenderer waterSprite;

    [Tooltip("The SpriteRenderer for baseline indicator (small line/marker)")]
    public SpriteRenderer baselineIndicator;

    [Header("Animation Settings")]
    [Tooltip("Which axis to scale for water level")]
    public ScaleAxis scaleAxis = ScaleAxis.Y;

    public enum ScaleAxis { X, Y }

    [Header("Visual Feedback")]
    [Tooltip("Color when water is low (normal)")]
    public Color normalColor = new Color(0.2f, 0.6f, 1f, 0.8f);

    [Tooltip("Color when water is medium (warning)")]
    public Color warningColor = new Color(1f, 0.8f, 0.2f, 0.8f);

    [Tooltip("Color when water is high (danger)")]
    public Color dangerColor = new Color(1f, 0.2f, 0.1f, 0.8f);

    [Tooltip("Water starts turning yellow above this level")]
    [Range(0, 100)] public float warningThreshold = 50f;

    [Tooltip("Water starts turning red above this level")]
    [Range(0, 100)] public float dangerThreshold = 70f;

    [Header("Pulse Effect (High Water)")]
    public bool enablePulse = true;
    public float pulseSpeed = 1.5f;
    private float pulseTimer;

    [Header("Baseline Visual")]
    [Tooltip("Color of the baseline marker")]
    public Color baselineColor = Color.yellow;
    [Tooltip("Show baseline changes with a flash effect")]
    public bool flashOnBaselineChange = true;
    public float flashDuration = 0.5f;
    private float flashTimer = 0f;
    private bool isFlashing = false;

    // Smoothing
    private float currentVisualLevel = 0f;
    private float smoothSpeed = 3f;

    // Component references
    private float spriteHeight;
    private float spriteWidth;
    private Vector3 originalScale;
    private Vector3 originalPosition;

    void Start()
    {
        // Get sprite renderer if not assigned
        if (waterSprite == null)
            waterSprite = GetComponent<SpriteRenderer>();

        if (waterSprite == null)
        {
            Debug.LogError("No SpriteRenderer assigned to WaterLevelDisplay!");
            return;
        }

        // Store sprite dimensions
        spriteHeight = waterSprite.sprite.bounds.size.y;
        spriteWidth = waterSprite.sprite.bounds.size.x;

        // Store original transform values
        originalScale = waterSprite.transform.localScale;
        originalPosition = waterSprite.transform.localPosition;

        // Initialize baseline indicator
        if (baselineIndicator != null)
        {
            baselineIndicator.color = baselineColor;
        }

        // Set initial water level
        if (GameManager.Instance != null)
        {
            currentVisualLevel = GameManager.Instance.waterLevel / 100f;
            UpdateWaterVisual(currentVisualLevel);
        }
    }

    void Update()
    {
        if (GameManager.Instance == null || waterSprite == null) return;

        float targetLevel = GameManager.Instance.waterLevel / 100f; // 0-1
        float waterLevel = GameManager.Instance.waterLevel;
        float baselineLevel = GameManager.Instance.waterNormalLevel / 100f;

        // Smooth the visual transition
        currentVisualLevel = Mathf.Lerp(currentVisualLevel, targetLevel, Time.deltaTime * smoothSpeed);

        // Update the visual
        UpdateWaterVisual(currentVisualLevel);

        // Update color based on danger
        UpdateWaterColor(waterLevel);

        // Update baseline indicator position
        UpdateBaselineIndicator(baselineLevel);

        // Handle flash effect for baseline changes
        UpdateFlashEffect();
    }

    void UpdateWaterVisual(float normalizedLevel)
    {
        if (waterSprite == null) return;

        // Get current transform values
        Vector3 scale = waterSprite.transform.localScale;
        Vector3 pos = waterSprite.transform.localPosition;

        if (scaleAxis == ScaleAxis.Y)
        {
            // Scale Y (water rises up from bottom)
            float minScale = 0.01f; // Minimum to avoid going invisible
            scale.y = Mathf.Max(normalizedLevel, minScale);

            // Adjust position to keep it anchored at the bottom
            // Assuming the sprite's pivot is at the bottom
            float halfHeight = spriteHeight * 0.5f;
            pos.y = -halfHeight + (halfHeight * normalizedLevel);
        }
        else // ScaleAxis.X
        {
            // Scale X (water fills from left)
            float minScale = 0.01f;
            scale.x = Mathf.Max(normalizedLevel, minScale);

            // Adjust position to keep it anchored at the left
            float halfWidth = spriteWidth * 0.5f;
            pos.x = -halfWidth + (halfWidth * normalizedLevel);
        }

        waterSprite.transform.localScale = scale;
        waterSprite.transform.localPosition = pos;
    }

    void UpdateWaterColor(float waterLevel)
    {
        if (waterSprite == null) return;

        Color targetColor;

        if (waterLevel >= dangerThreshold)
        {
            // Danger zone - red with pulse
            targetColor = dangerColor;

            if (enablePulse)
            {
                pulseTimer += Time.deltaTime * pulseSpeed;
                float pulse = Mathf.PingPong(pulseTimer, 1f);
                float alpha = Mathf.Lerp(0.7f, 1f, pulse);
                targetColor.a = alpha;
            }
        }
        else if (waterLevel >= warningThreshold)
        {
            // Warning zone - yellow/orange
            float t = (waterLevel - warningThreshold) / (dangerThreshold - warningThreshold);
            targetColor = Color.Lerp(warningColor, dangerColor, t);
        }
        else
        {
            // Normal zone - blue
            targetColor = normalColor;
        }

        waterSprite.color = targetColor;
    }

    void UpdateBaselineIndicator(float baselineLevel)
    {
        if (baselineIndicator == null) return;

        // Position the indicator at the baseline level
        float halfHeight = spriteHeight * 0.5f;
        Vector3 pos = baselineIndicator.transform.localPosition;
        pos.y = -halfHeight + (spriteHeight * baselineLevel);
        baselineIndicator.transform.localPosition = pos;

        // Make indicator visible only if baseline is above 15% (changes will show)
        bool showIndicator = baselineLevel > 0.15f;
        baselineIndicator.gameObject.SetActive(showIndicator);
    }

    void UpdateFlashEffect()
    {
        if (!isFlashing || baselineIndicator == null) return;

        flashTimer -= Time.deltaTime;
        if (flashTimer <= 0f)
        {
            isFlashing = false;
            baselineIndicator.color = baselineColor;
        }
        else
        {
            // Flash the baseline indicator
            float flash = Mathf.PingPong(flashTimer * 4f, 1f);
            baselineIndicator.color = Color.Lerp(baselineColor, Color.white, flash);
        }
    }

    // Called by GameManager or AnimatedWaterLevel when baseline changes
    public void OnBaselineChanged(float newBaseline)
    {
        if (flashOnBaselineChange && baselineIndicator != null)
        {
            isFlashing = true;
            flashTimer = flashDuration;
        }

        Debug.Log($"Water baseline changed to {newBaseline}%!");
    }

    // Public method to force update (useful for initialization)
    public void ForceUpdate()
    {
        if (GameManager.Instance != null && waterSprite != null)
        {
            currentVisualLevel = GameManager.Instance.waterLevel / 100f;
            UpdateWaterVisual(currentVisualLevel);
        }
    }

    // Called when water level is critical (above 90%)
    public void CriticalWaterAlert()
    {
        if (enablePulse)
        {
            pulseSpeed = 3f; // Faster pulse when critical
        }
    }

    // Reset to normal pulse speed
    public void ResetPulseSpeed()
    {
        pulseSpeed = 1.5f;
    }
}