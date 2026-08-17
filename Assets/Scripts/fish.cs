using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Fish : MonoBehaviour
{
    // The three phases of one "pulse cycle"
    protected enum PulseState { Pulsing, Decelerating, Resting }

    [Header("Faction")]
    public bool isPredator = false; // true for Alien/Bug/Mother/Burster (any hunter) - predators exclude each other as targets
    public bool hasUpgrade = false; // true once this fish has collected an upgrade - limits fish to holding 1 at a time

    [Header("Spawn Economy")]
    public float spawnValue = 10f; // how much of the night's spawn budget this predator "costs" - set per prefab in the Inspector

    [Header("Pulse Movement")]
    public float pulseSpeed = 4f;        // top speed reached at the end of the pulse burst
    public float pulseDuration = 0.5f;   // time spent accelerating forward
    public float decelDuration = 0.5f;   // time spent slowing to a stop after the pulse
    public float restDuration = 0.3f;    // brief pause before picking a new direction and pulsing again

    [Header("Rotation")]
    public float rotationSpeed = 720f;   // degrees per second the fish can turn to face its direction
    public float maxTiltAngle = 55f;     // how far the fish can tilt up/down from horizontal (its "neutral" facing)

    [Header("Turn Flip")]
    public float flipDuration = 0.15f;   // how long the whole turn-flip animation takes
    public float squashAmount = 0.4f;    // how thin (width-wise) the fish gets at the peak of the turn

    [Header("Health")]
    public int maxHP = 2;                // total hit points before this fish dies
    protected int currentHP;             // current hit points, set to maxHP on spawn

    protected Rigidbody2D rb;
    protected Vector2 targetDirection;   // the direction this pulse is moving toward (world space, already tilt-clamped)
    protected PulseState currentState;
    protected float stateTimer;

    protected float baseScaleX;          // original unsigned scale.x, captured once
    protected float baseScaleY;          // original unsigned scale.y, captured once

    protected bool isFacingRight;        // which way the fish is currently settled facing
    protected bool isFlipping;           // true while a turn animation is in progress
    protected bool pendingFacingRight;   // the facing direction we're flipping TOWARD
    protected float flipTimer;           // counts up during the flip animation
    protected bool hasSwappedThisFlip;   // ensures the x-sign swap only happens once per flip

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        baseScaleX = Mathf.Abs(transform.localScale.x);
        baseScaleY = Mathf.Abs(transform.localScale.y);

        // Start facing whichever way the fish's initial scale suggests
        isFacingRight = transform.localScale.x >= 0f;

        currentHP = maxHP;
    }

    protected virtual void Start()
    {
        PickNewDirection();
        EnterState(PulseState.Pulsing); // start the cycle immediately
    }

    protected virtual void Update()
    {
        stateTimer += Time.deltaTime;

        // Check whether it's time to move to the next phase of the pulse cycle
        switch (currentState)
        {
            case PulseState.Pulsing:
                if (stateTimer >= pulseDuration)
                    EnterState(PulseState.Decelerating);
                break;

            case PulseState.Decelerating:
                if (stateTimer >= decelDuration)
                    EnterState(PulseState.Resting);
                break;

            case PulseState.Resting:
                if (stateTimer >= restDuration)
                {
                    PickNewDirection();
                    EnterState(PulseState.Pulsing);
                }
                break;
        }

        FaceMovementDirection();
    }

    protected virtual void FixedUpdate()
    {
        switch (currentState)
        {
            case PulseState.Pulsing:
                {
                    float t = stateTimer / pulseDuration;
                    float easedT = 1f - Mathf.Pow(1f - t, 2f);
                    rb.linearVelocity = targetDirection * (pulseSpeed * easedT);
                    break;
                }

            case PulseState.Decelerating:
                {
                    float t = stateTimer / decelDuration;
                    float currentSpeed = Mathf.Lerp(pulseSpeed, 0f, t);
                    rb.linearVelocity = targetDirection * currentSpeed;
                    break;
                }

            case PulseState.Resting:
                rb.linearVelocity = Vector2.zero;
                break;
        }
    }

    protected virtual void EnterState(PulseState newState)
    {
        currentState = newState;
        stateTimer = 0f;
    }

    protected virtual void PickNewDirection()
    {
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        targetDirection = ClampToHorizontalTilt(randomDirection);
    }

    protected virtual Vector2 ClampToHorizontalTilt(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        bool facingRight = direction.x >= 0f;
        float reference = facingRight ? 0f : 180f;

        float deviation = Mathf.DeltaAngle(reference, angle);
        deviation = Mathf.Clamp(deviation, -maxTiltAngle, maxTiltAngle);

        float clampedAngle = reference + deviation;
        float rad = clampedAngle * Mathf.Deg2Rad;

        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
    }

    protected virtual void FaceMovementDirection()
    {
        if (targetDirection.sqrMagnitude < 0.0001f)
            return;

        bool desiredFacingRight = targetDirection.x >= 0f;

        if (desiredFacingRight != isFacingRight && !isFlipping)
        {
            isFlipping = true;
            flipTimer = 0f;
            pendingFacingRight = desiredFacingRight;
            hasSwappedThisFlip = false;
        }

        if (isFlipping)
        {
            UpdateTurnFlip();
        }

        float rawAngle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;
        float reference = desiredFacingRight ? 0f : 180f;
        float deviation = Mathf.Clamp(Mathf.DeltaAngle(reference, rawAngle), -maxTiltAngle, maxTiltAngle);

        Quaternion targetRotation = Quaternion.Euler(0f, 0f, deviation);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    protected virtual void UpdateTurnFlip()
    {
        flipTimer += Time.deltaTime;
        float t = Mathf.Clamp01(flipTimer / flipDuration);

        float squashT = 1f - Mathf.Abs((t * 2f) - 1f);
        float widthMultiplier = Mathf.Lerp(1f, squashAmount, squashT);

        Vector3 scale = transform.localScale;

        float unsignedX = baseScaleX * widthMultiplier;

        bool sideToUse = hasSwappedThisFlip ? pendingFacingRight : isFacingRight;
        scale.x = sideToUse ? unsignedX : -unsignedX;
        scale.y = baseScaleY;

        if (!hasSwappedThisFlip && t >= 0.5f)
        {
            hasSwappedThisFlip = true;
        }

        transform.localScale = scale;

        if (t >= 1f)
        {
            isFlipping = false;
            isFacingRight = pendingFacingRight;

            Vector3 finalScale = transform.localScale;
            finalScale.y = baseScaleY;
            finalScale.x = isFacingRight ? baseScaleX : -baseScaleX;
            transform.localScale = finalScale;
        }
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        DevTools.LogCollision(gameObject, collision.gameObject);

        if (collision.gameObject.name.Contains("Wall"))
        {
            Vector2 wallNormal = collision.GetContact(0).normal;
            Vector2 reflectedDirection = Vector2.Reflect(targetDirection, wallNormal).normalized;

            targetDirection = ClampToHorizontalTilt(reflectedDirection);

            EnterState(PulseState.Pulsing);
        }
    }

    public virtual bool TakeDamage(int amount, GameObject attacker = null)
    {
        currentHP -= amount;
        DevTools.LogDamage(attacker != null ? attacker : gameObject, gameObject, amount, currentHP);

        if (currentHP <= 0)
        {
            Die();
            return true;
        }
        return false;
    }

    protected virtual void Die()
    {
        DevTools.LogDeath(gameObject);
        Destroy(gameObject);
    }

    // Changes this fish's sprite color - used by upgrades or other effects that need
    // to visually mark a fish (e.g. tinting it after consuming an upgrade)
    public virtual void SetSpriteColor(Color color)
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = color;
        }
    }
}