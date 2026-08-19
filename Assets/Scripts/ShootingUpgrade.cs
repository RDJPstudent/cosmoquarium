using UnityEngine;

// Grants the eating fish the ability to periodically fire a projectile at the
// nearest predator, dealing a guaranteed instance of its own click-damage
// (clickDamageAmount x clickDamageHits) on hit - functions exactly like the
// fish being clicked, just applied remotely and automatically.
public class ShootingUpgrade : Upgrade
{
    [Header("Shooting Setup")]
    public GameObject projectilePrefab;
    public float fireInterval = 3f;
    public float projectileSpeed = 6f;

    protected override void ApplyUpgradeEffect(Fish eater)
    {
        base.ApplyUpgradeEffect(eater);

        FishShooter shooter = eater.gameObject.AddComponent<FishShooter>();
        shooter.projectilePrefab = projectilePrefab;
        shooter.fireInterval = fireInterval;
        shooter.projectileSpeed = projectileSpeed;

        Debug.Log($"[ShootingUpgrade] {eater.gameObject.name} can now shoot predators.");
    }
}