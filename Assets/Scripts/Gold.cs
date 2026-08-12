using UnityEngine;
using System.Collections;

// Attach to the Gold prefab. Falls downward each frame. If clicked before it
// touches the bottom wall, adds to the player's gold count and is destroyed.
// If it touches the bottom wall first, flashes red and is destroyed with no reward.
[RequireComponent(typeof(Rigidbody2D))]
public class Gold : MonoBehaviour
{
    [Header("Falling")]
    public float fallSpeed = 1f;       // how fast the gold sinks downward

    [Header("Reward")]
    public int goldValue = 1;          // how much gold this adds to the player's total if clicked in time

    [Header("Miss Feedback")]
    public Color missFlashColor = Color.red;
    public float missFlashDuration = 0.15f;

    protected bool isResolved = false; // true once either collected or missed, prevents double-triggering
    protected SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (isResolved) return;

        // Sinks straight downward at a constant speed
        transform.position += Vector3.down * fallSpeed * Time.deltaTime;
    }

    // Player clicked this gold before it touched the bottom
    void OnMouseDown()
    {
        if (isResolved) return;

        isResolved = true;
        GameManager.AddGold(goldValue);
        Destroy(gameObject);
    }

    // Requires this object's Collider2D to have "Is Trigger" checked, and a Rigidbody2D
    // (set to Kinematic body type) so OnTriggerEnter2D fires reliably
    void OnTriggerEnter2D(Collider2D other)
    {
        if (isResolved) return;

        if (other.gameObject.name.Contains("Wall_Bottom"))
        {
            isResolved = true;
            StartCoroutine(FlashRedAndDestroy());
        }
    }

    protected virtual IEnumerator FlashRedAndDestroy()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = missFlashColor;
        }

        yield return new WaitForSeconds(missFlashDuration);

        Destroy(gameObject);
    }
}