using UnityEngine;
using System.Collections;

public class ClickDamage : MonoBehaviour
{
    public int clickDamageAmount = 1;

    [Header("Click Feedback")]
    public Color flashColor = Color.white;   // color the sprite briefly flashes to
    public float flashDuration = 0.15f;      // how long the flash lasts before fading back

    protected Fish targetFish;
    protected SpriteRenderer spriteRenderer;
    protected Color originalColor;
    protected Coroutine flashRoutine; // tracks the currently running flash, so rapid clicks don't stack weirdly

    void Awake()
    {
        targetFish = GetComponent<Fish>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color; // remember the sprite's normal color to restore afterward
        }
    }

    void OnMouseDown()
    {
        if (targetFish != null)
        {
            targetFish.TakeDamage(clickDamageAmount);
        }

        PlayFlashFeedback();
    }

    protected virtual void PlayFlashFeedback()
    {
        if (spriteRenderer == null) return;

        // If a flash is already in progress (rapid clicking), stop it first so they don't overlap weirdly
        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
        }

        flashRoutine = StartCoroutine(FlashCoroutine());
    }

    protected virtual IEnumerator FlashCoroutine()
    {
        spriteRenderer.color = flashColor;

        yield return new WaitForSeconds(flashDuration);

        spriteRenderer.color = originalColor;
        flashRoutine = null;
    }
}