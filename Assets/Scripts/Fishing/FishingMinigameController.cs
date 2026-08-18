using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// Runs the timing-based catch minigame: an indicator ping-pongs between 0 and 1,
/// the player clicks to "stop" it, and success depends on whether it landed inside the hit zone.
/// No visual bar yet - this is pure logic, tested via Console output, ready for a UI layer later.
public class FishingMinigameController : MonoBehaviour
{
    [Header("Timing")]
    [Tooltip("Max seconds the player has to click before it auto-fails.")]
    public float minigameDuration = 3f;
    [Tooltip("Higher = indicator moves faster/back-and-forth more times per second.")]
    public float pingPongSpeed = 1f;

    [Header("Hit Zone")]
    [Range(0f, 1f)] public float hitZoneCenter = 0.5f;
    [Tooltip("Base width of the hit zone, as a fraction of the full 0-1 range.")]
    public float baseHitZoneWidth = 0.2f;
    [Tooltip("Extra width added per level of the Line Strength upgrade.")]
    public float hitZoneWidthPerLevel = 0.03f;

    // Set by FishingUpgradeManager once that script exists. Defaults to 0 (no upgrade purchased yet).
    public int lineStrengthLevel = 0;

    private float currentPosition;

    /// Starts the minigame. Calls onComplete(true) if the player clicked inside the hit zone,
    /// onComplete(false) if they missed or ran out of time.
    public void StartMinigame(Action<bool> onComplete)
    {
        StartCoroutine(RunMinigame(onComplete));
    }

    private IEnumerator RunMinigame(Action<bool> onComplete)
    {
        Debug.Log("[FishingMinigameController] Minigame started - click when ready!");
        float elapsed = 0f;
        float startTime = Time.time;

        while (elapsed < minigameDuration)
        {
            currentPosition = Mathf.PingPong((Time.time - startTime) * pingPongSpeed, 1f);

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                bool success = IsWithinHitZone(currentPosition);
                Debug.Log($"[FishingMinigameController] Clicked at position {currentPosition:F2}. Success: {success}");
                onComplete(success);
                yield break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        Debug.Log("[FishingMinigameController] Time ran out. Auto-fail.");
        onComplete(false);
    }

    private bool IsWithinHitZone(float position)
    {
        float zoneWidth = GetCurrentHitZoneWidth();
        float halfWidth = zoneWidth / 2f;
        return Mathf.Abs(position - hitZoneCenter) <= halfWidth;
    }

    /// Current hit zone width, factoring in the Line Strength upgrade.
    /// Exposed publicly so a future UI bar can draw the zone at the correct size.
    public float GetCurrentHitZoneWidth()
    {
        return baseHitZoneWidth + (lineStrengthLevel * hitZoneWidthPerLevel);
    }

    /// Current indicator position (0-1). Exposed publicly so a future UI bar can render it moving.
    public float GetCurrentPosition()
    {
        return currentPosition;
    }
}
