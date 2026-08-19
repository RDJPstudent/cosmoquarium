using UnityEngine;

// Fired by FishShooter. Flies in a straight line toward wherever the target
// predator was at the moment of firing. On hitting ANY predator-flagged Fish,
// deals a guaranteed instance of the owner's click-damage (no chance roll) -
// functions exactly like the target being clicked. Destroyed with no effect
// if it hits a wall first.
[RequireComponent(typeof(Rigidbody2D))]
public class ClickShotProjectile : MonoBehaviour
{
    protected Vector2 direction;
    protected float speed;
    protected Fish owner;

    public virtual void Initialize(Vector2 moveDirection, float moveSpeed, Fish shooterFish)
    {
        direction = moveDirection.normalized;
        speed = moveSpeed;
        owner = shooterFish;
    }

    protected virtual void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[ClickShotProjectile] Triggered by: {other.gameObject.name}"); // TEMP DEBUG - remove once confirmed

        if (other.gameObject.name.Contains("Wall"))
        {
            Destroy(gameObject);
            return;
        }

        Fish hitFish = other.GetComponent<Fish>();
        if (hitFish != null && hitFish.isPredator)
        {
            if (owner != null)
            {
                owner.DealClickDamageTo(hitFish, owner.gameObject);
            }
            Destroy(gameObject);
        }
    }
}