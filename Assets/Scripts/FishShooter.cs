using UnityEngine;

// Added to a fish's GameObject at runtime when it eats a Shooting upgrade.
// Periodically fires a projectile at the nearest predator-flagged Fish, dealing
// a guaranteed hit (no chance roll) using this fish's own click-damage profile.
public class FishShooter : MonoBehaviour
{
    public GameObject projectilePrefab;
    public float fireInterval = 3f;
    public float projectileSpeed = 6f;

    protected Fish ownerFish;
    protected float fireTimer;

    void Awake()
    {
        ownerFish = GetComponent<Fish>();
    }

    void Update()
    {
        fireTimer += Time.deltaTime;
        if (fireTimer >= fireInterval)
        {
            fireTimer = 0f;
            TryFireAtClosestPredator();
        }
    }

    protected virtual void TryFireAtClosestPredator()
    {
        if (projectilePrefab == null || ownerFish == null) return;

        Fish[] allFish = FindObjectsByType<Fish>(FindObjectsSortMode.None);
        Fish closestPredator = null;
        float closestDist = float.MaxValue;

        foreach (Fish fish in allFish)
        {
            if (!fish.isPredator) continue;

            float dist = Vector2.Distance(transform.position, fish.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestPredator = fish;
            }
        }

        if (closestPredator == null) return;

        Vector2 direction = ((Vector2)closestPredator.transform.position - (Vector2)transform.position).normalized;

        GameObject projObj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        ClickShotProjectile proj = projObj.GetComponent<ClickShotProjectile>();
        if (proj != null)
        {
            proj.Initialize(direction, projectileSpeed, ownerFish);
        }
    }
}