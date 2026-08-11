using UnityEngine;

// Attach to the burst projectile prefab. Flies outward in a fixed direction,
// attempts an eat-chance roll on touching a fish (destroying itself either way),
// and simply destroys itself with no roll if it touches a wall.
[RequireComponent(typeof(Rigidbody2D))]
public class BurstProjectile : MonoBehaviour
{
    protected Vector2 direction;
    protected float speed;
    protected int damageAmount;
    protected float eatChancePercent;
    protected GameObject shooter; // the Burster that spawned this, for clearer DevTools logs

    // Called by Burster right after Instantiate to configure this projectile's behavior
    public virtual void Initialize(Vector2 moveDirection, float moveSpeed, int damage, float chancePercent, GameObject sourceShooter)
    {
        direction = moveDirection.normalized;
        speed = moveSpeed;
        damageAmount = damage;
        eatChancePercent = chancePercent;
        shooter = sourceShooter;

        Debug.Log($"[BurstProjectile] Initialize called - direction: {direction}, speed: {speed}"); // TEMP DEBUG - remove once confirmed working
    }

    protected virtual void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    // Requires this object's Collider2D to have "Is Trigger" checked, and a Rigidbody2D
    // (set to Kinematic body type) so OnTriggerEnter2D fires reliably without physics interference
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        // Hit a wall - just disappear, no roll
        if (other.gameObject.name.Contains("Wall"))
        {
            Destroy(gameObject);
            return;
        }

        // Hit a fish - attempt an eat-chance roll (only against non-predator fish, same rule as Alien/Burster hunting)
        Fish hitFish = other.GetComponent<Fish>();
        if (hitFish != null && !hitFish.isPredator)
        {
            float roll = Random.Range(0f, 100f);
            bool success = roll <= eatChancePercent;

            DevTools.LogEatAttempt(shooter != null ? shooter : gameObject, hitFish.gameObject, roll, eatChancePercent, success);

            if (success)
            {
                hitFish.TakeDamage(damageAmount, shooter != null ? shooter : gameObject);
            }

            Destroy(gameObject); // projectile is consumed on hitting a fish either way
        }
    }
}