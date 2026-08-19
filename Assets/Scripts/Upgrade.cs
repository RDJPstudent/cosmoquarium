using UnityEngine;

// Base class for all upgrade pickups. If spawned above the top wall, falls under
// gravity until reaching it, then floats slowly downward while attracting eligible
// fish. If spawned already inside the tank, skips straight to floating. Passes
// physically through walls and fish (all movement is controlled manually).
// Guaranteed 100% "eat" on contact with any eligible fish (no chance roll).
// Subclasses override ApplyUpgradeEffect() to define what it actually does.
[RequireComponent(typeof(Rigidbody2D))]
public class Upgrade : MonoBehaviour
{
    protected enum UpgradeState { Falling, Floating, Resting }

    [Header("Identity")]
    public string upgradeId;

    [Header("Fall")]
    public float fallGravityScale = 1f;

    [Header("Float")]
    public float floatSpeed = 0.3f;

    [Header("Attraction")]
    public float attractionRadius = 3f;
    public float attractionStrength = 2f;
    public bool attractPredators = false;

    [Header("Eligibility")]
    public bool requireGoldProducer = false;

    [Header("Pickup Feedback")]
    public GameObject sparkleEffectPrefab;
    public float sparkleLifetime = 1.5f;
    public Color fishColorOnConsume = Color.white;

    protected Rigidbody2D rb;
    protected UpgradeState currentState;

    // Override to return false in a subclass that shouldn't lock the fish into
    // "has an upgrade" state (e.g. an upgrade that clones the fish rather than
    // permanently modifying it) - such upgrades skip the hasUpgrade check/lock entirely.
    protected virtual bool RequiresUpgradeSlot => true;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        GameObject wallTop = GameObject.Find("Wall_Top");

        if (wallTop != null && transform.position.y < wallTop.transform.position.y)
        {
            rb.gravityScale = 0f;
            currentState = UpgradeState.Floating;
        }
        else
        {
            rb.gravityScale = fallGravityScale;
            currentState = UpgradeState.Falling;
        }
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

    protected virtual void AttractNearbyFish()
    {
        Fish[] allFish = FindObjectsByType<Fish>(FindObjectsSortMode.None);

        foreach (Fish fish in allFish)
        {
            if (!attractPredators && fish.isPredator) continue;
            if (RequiresUpgradeSlot && fish.hasUpgrade) continue;
            if (requireGoldProducer && !fish.isGoldProducer) continue;

            float distance = Vector2.Distance(fish.transform.position, transform.position);
            if (distance <= attractionRadius)
            {
                Vector2 direction = ((Vector2)transform.position - (Vector2)fish.transform.position).normalized;
                fish.transform.position += (Vector3)(direction * attractionStrength * Time.deltaTime);
            }
        }
    }

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

        bool eligibleForUpgradeSlot = hitFish != null && (!RequiresUpgradeSlot || !hitFish.hasUpgrade);

        if (hitFish != null
            && (attractPredators || !hitFish.isPredator)
            && eligibleForUpgradeSlot
            && (!requireGoldProducer || hitFish.isGoldProducer))
        {
            if (RequiresUpgradeSlot)
            {
                hitFish.hasUpgrade = true;
            }

            if (!string.IsNullOrEmpty(upgradeId))
            {
                GameManager.RemoveUpgrade(upgradeId);
            }

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

    protected virtual void ApplyUpgradeEffect(Fish eater)
    {
        eater.SetSpriteColor(fishColorOnConsume);
    }
}