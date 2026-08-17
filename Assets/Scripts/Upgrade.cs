using UnityEngine;

// Base class for all upgrade pickups. Falls under gravity until reaching the top wall,
// then floats slowly downward while attracting nearby fish, passing through both walls
// and fish physically (all movement is controlled manually). Guaranteed 100% "eat" on
// contact with any eligible fish (no chance roll, and fish already holding an upgrade
// are skipped). Subclasses override ApplyUpgradeEffect() to define what it actually does.
[RequireComponent(typeof(Rigidbody2D))]
public class Upgrade : MonoBehaviour
{
    protected enum UpgradeState { Falling, Floating, Resting }

    [Header("Fall")]
    public float fallGravityScale = 1f;

    [Header("Float")]
    public float floatSpeed = 0.3f;

    [Header("Attraction")]
    public float attractionRadius = 3f;
    public float attractionStrength = 2f;
    public bool attractPredators = false;

    [Header("Pickup Feedback")]
    public GameObject sparkleEffectPrefab;
    public float sparkleLifetime = 1.5f;
    public Color fishColorOnConsume = Color.white; // color the eating fish's sprite changes to - set per upgrade type in the Inspector

    protected Rigidbody2D rb;
    protected UpgradeState currentState;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = fallGravityScale;
        currentState = UpgradeState.Falling;
    }

    protected virtual void Update()
    {
        if (currentState == UpgradeState.Floating || currentState == UpgradeState.Resting)
        {
            AttractNearbyFish();
        }
    }

    protected virtual void FixedUpdate()
    {
        if (currentState == UpgradeState.Floating)
        {
            rb.linearVelocity = Vector2.down * floatSpeed;
        }
        else if (currentState == UpgradeState.Resting)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    // Pulls eligible fish (excludes predators unless attractPredators is on,
    // and excludes fish that already have an upgrade) toward this pickup
    protected virtual void AttractNearbyFish()
    {
        Fish[] allFish = FindObjectsByType<Fish>(FindObjectsSortMode.None);

        foreach (Fish fish in allFish)
        {
            if (!attractPredators && fish.isPredator) continue;
            if (fish.hasUpgrade) continue;

            float distance = Vector2.Distance(fish.transform.position, transform.position);
            if (distance <= attractionRadius)
            {
                Vector2 direction = ((Vector2)transform.position - (Vector2)fish.transform.position).normalized;
                fish.transform.position += (Vector3)(direction * attractionStrength * Time.deltaTime);
            }
        }
    }

    // Requires this object's Collider2D to have "Is Trigger" checked, so the upgrade
    // passes physically through walls and fish rather than bouncing/stopping on contact
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        switch (currentState)
        {
            case UpgradeState.Falling:
                if (other.gameObject.name.Contains("Wall_Top"))
                {
                    rb.gravityScale = 0f;
                    currentState = UpgradeState.Floating;
                }
                break;

            case UpgradeState.Floating:
                if (other.gameObject.name.Contains("Wall_Bottom"))
                {
                    rb.linearVelocity = Vector2.zero;
                    currentState = UpgradeState.Resting;
                }
                break;
        }

        Fish hitFish = other.gameObject.GetComponent<Fish>();

        if (hitFish != null && (attractPredators || !hitFish.isPredator) && !hitFish.hasUpgrade)
        {
            hitFish.hasUpgrade = true;
            SpawnSparkle(hitFish);
            ApplyUpgradeEffect(hitFish);
            Destroy(gameObject);
        }
    }

    protected virtual void SpawnSparkle(Fish eater)
    {
        if (sparkleEffectPrefab == null)
            return;

        GameObject sparkle = Instantiate(sparkleEffectPrefab, eater.transform.position, Quaternion.identity);
        Destroy(sparkle, sparkleLifetime);
    }

    // Override in a subclass to define what this specific upgrade actually does.
    // Base behavior here tints the fish - subclasses can call base.ApplyUpgradeEffect(eater)
    // to keep the color change, then add their own additional effect on top.
    protected virtual void ApplyUpgradeEffect(Fish eater)
    {
        eater.SetSpriteColor(fishColorOnConsume);
    }
}