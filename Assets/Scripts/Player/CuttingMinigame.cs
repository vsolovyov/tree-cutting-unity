using UnityEngine;

public class CuttingMinigame : MonoBehaviour
{
    public enum HitResult { Miss, Good, Perfect }

    [Header("Timing Settings")]
    public float baseSpeed = 0.8f;
    public float speedIncreasePerPerfect = 0.08f;
    public float maxSpeed = 2f;

    [Header("Zone Settings")]
    public float baseGreenZoneWidth = 0.18f;  // Smaller since we have two zones
    public float minGreenZoneWidth = 0.08f;
    public float zoneShrinkPerPerfect = 0.01f;
    public float perfectZoneRatio = 0.35f;

    // Two zones - one at bottom, one at top
    public float bottomZoneCenter = 0.15f;
    public float topZoneCenter = 0.85f;

    [Header("Damage")]
    public int baseDamage = 1;
    public int perfectDamageBonus = 1;
    public float comboDamageMultiplier = 0.2f;

    [Header("State")]
    public bool IsActive { get; private set; }
    public int ComboCount { get; private set; }
    public float IndicatorPosition { get; private set; } // 0-1 (bottom to top)
    public float GreenZoneWidth { get; private set; }
    public float PerfectZoneWidth => GreenZoneWidth * perfectZoneRatio;

    // Expose zone centers for UI
    public float BottomZoneCenter => bottomZoneCenter;
    public float TopZoneCenter => topZoneCenter;

    float currentSpeed;
    int direction = 1;

    public void Activate()
    {
        IsActive = true;
        ResetState();
        IndicatorPosition = 0f;
        direction = 1;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    void ResetState()
    {
        ComboCount = 0;
        currentSpeed = baseSpeed;
        GreenZoneWidth = baseGreenZoneWidth;
    }

    void Update()
    {
        if (!IsActive) return;

        // Oscillate indicator bottom to top
        IndicatorPosition += direction * currentSpeed * Time.deltaTime;

        if (IndicatorPosition >= 1f)
        {
            IndicatorPosition = 1f;
            direction = -1;
        }
        else if (IndicatorPosition <= 0f)
        {
            IndicatorPosition = 0f;
            direction = 1;
        }
    }

    public (HitResult result, int damage) TryHit()
    {
        if (!IsActive)
            return (HitResult.Miss, 0);

        // Check distance from BOTH zone centers
        float distFromBottom = Mathf.Abs(IndicatorPosition - bottomZoneCenter);
        float distFromTop = Mathf.Abs(IndicatorPosition - topZoneCenter);
        float distFromCenter = Mathf.Min(distFromBottom, distFromTop);

        float halfGreen = GreenZoneWidth / 2f;
        float halfPerfect = PerfectZoneWidth / 2f;

        if (distFromCenter <= halfPerfect)
        {
            // Perfect hit
            ComboCount++;
            currentSpeed = Mathf.Min(currentSpeed + speedIncreasePerPerfect, maxSpeed);
            GreenZoneWidth = Mathf.Max(GreenZoneWidth - zoneShrinkPerPerfect, minGreenZoneWidth);

            int damage = baseDamage + perfectDamageBonus + Mathf.FloorToInt(ComboCount * comboDamageMultiplier);
            return (HitResult.Perfect, damage);
        }
        else if (distFromCenter <= halfGreen)
        {
            // Good hit
            int damage = baseDamage;
            return (HitResult.Good, damage);
        }
        else
        {
            // Miss
            ResetState();
            return (HitResult.Miss, 0);
        }
    }
}
