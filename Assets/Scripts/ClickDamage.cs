using UnityEngine;
using System.Collections;

public class ClickDamage : MonoBehaviour
{
    [Header("Click Feedback")]
    public Color flashColor = Color.white;
    public float flashDuration = 0.15f;

    protected Fish targetFish;
    protected SpriteRenderer spriteRenderer;
    protected Color originalColor;
    protected Coroutine flashRoutine;

    void Awake()
    {
        targetFish = GetComponent<Fish>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    void OnMouseDown()
    {
        if (targetFish != null)
        {
            targetFish.DealClickDamageTo(targetFish);
        }

        PlayFlashFeedback();
    }

    protected virtual void PlayFlashFeedback()
    {
        if (spriteRenderer == null) return;

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