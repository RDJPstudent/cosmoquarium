using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Fish : MonoBehaviour
{
    // The three phases of one "pulse cycle"
    protected enum PulseState { Pulsing, Decelerating, Resting }

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
                    // 0 -> 1 progress through the pulse duration
                    float t = stateTimer / pulseDuration;

                    // Ease-out curve: fast initial acceleration that tapers off,
                    // feels more like a muscle-powered burst than a linear ramp
                    float easedT = 1f - Mathf.Pow(1f - t, 2f);

                    rb.linearVelocity = targetDirection * (pulseSpeed * easedT);
                    break;
                }

            case PulseState.Decelerating:
                {
                    // 0 -> 1 progress through the deceleration duration
                    float t = stateTimer / decelDuration;

                    // Lerp speed down from pulseSpeed to 0 over the decel window
                    float currentSpeed = Mathf.Lerp(pulseSpeed, 0f, t);

                    rb.linearVelocity = targetDirection * currentSpeed;
                    break;
                }

            case PulseState.Resting:
                // Fully stopped while waiting for the next pulse
                rb.linearVelocity = Vector2.zero;
                break;
        }
    }

    // Switches state and resets the timer - centralizes state transitions in one place
    protected virtual void EnterState(PulseState newState)
    {
        currentState = newState;
        stateTimer = 0f;
    }

    // Picks a new random direction for the next pulse, clamped to the allowed tilt range
    protected virtual void PickNewDirection()
    {
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        targetDirection = ClampToHorizontalTilt(randomDirection);
    }

    // Clamps a direction so it never tilts more than maxTiltAngle away from
    // whichever horizontal baseline (right = 0 degrees, left = 180 degrees) it's closer to.
    protected virtual Vector2 ClampToHorizontalTilt(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        bool facingRight = direction.x >= 0f;
        float reference = facingRight ? 0f : 180f;

        // DeltaAngle safely handles the -180/180 wraparound
        float deviation = Mathf.DeltaAngle(reference, angle);
        deviation = Mathf.Clamp(deviation, -maxTiltAngle, maxTiltAngle);

        float clampedAngle = reference + deviation;
        float rad = clampedAngle * Mathf.Deg2Rad;

        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
    }

    // Handles facing direction using two independent mechanisms:
    // 1. A squash-and-swap turn animation for left/right facing (see UpdateTurnFlip below)
    // 2. Z-axis rotation ONLY for the tilt (max +/- maxTiltAngle) - never a full rotation
    protected virtual void FaceMovementDirection()
    {
        if (targetDirection.sqrMagnitude < 0.0001f)
            return;

        bool desiredFacingRight = targetDirection.x >= 0f;

        // If the desired facing differs from our current settled facing, and we're not
        // already mid-turn, kick off a new flip animation
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

        // Tilt rotation - independent of the flip animation, always tracks the live target direction
        float rawAngle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;
        float reference = desiredFacingRight ? 0f : 180f;
        float deviation = Mathf.Clamp(Mathf.DeltaAngle(reference, rawAngle), -maxTiltAngle, maxTiltAngle);

        Quaternion targetRotation = Quaternion.Euler(0f, 0f, deviation);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    // Animates a quick horizontal squash while the fish turns, and swaps the x-sign
    // (actual left/right flip) at the peak of the squash, when the fish is thinnest.
    // Squashing the SAME axis that flips (x) keeps the motion reading as a horizontal turn,
    // rather than squashing y which would read as an unrelated vertical flip.
    protected virtual void UpdateTurnFlip()
    {
        flipTimer += Time.deltaTime;
        float t = Mathf.Clamp01(flipTimer / flipDuration);

        // Triangle wave: rises from 0 to 1 at the midpoint, then back down to 0 - peak squash at t = 0.5
        float squashT = 1f - Mathf.Abs((t * 2f) - 1f);
        float widthMultiplier = Mathf.Lerp(1f, squashAmount, squashT);

        Vector3 scale = transform.localScale;

        float unsignedX = baseScaleX * widthMultiplier;

        // Before the swap point, shrink toward zero using the OLD facing sign.
        // After the swap point, grow back out using the NEW facing sign.
        bool sideToUse = hasSwappedThisFlip ? pendingFacingRight : isFacingRight;
        scale.x = sideToUse ? unsignedX : -unsignedX;
        scale.y = baseScaleY; // y stays untouched - no vertical squash at all

        // Swap the actual facing direction once, right at the point of maximum squash
        if (!hasSwappedThisFlip && t >= 0.5f)
        {
            hasSwappedThisFlip = true;
        }

        transform.localScale = scale;

        // Flip animation complete - snap cleanly back to full proportions
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

    // Called automatically by Unity when this fish's collider hits another collider
    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name.Contains("Wall"))
        {
            // Use the collision's contact normal to reflect the direction properly,
            // like a ball bouncing off a surface, rather than picking something random
            Vector2 wallNormal = collision.GetContact(0).normal;
            Vector2 reflectedDirection = Vector2.Reflect(targetDirection, wallNormal).normalized;

            // Clamp the bounce direction too, so wall bounces still respect the tilt limit
            targetDirection = ClampToHorizontalTilt(reflectedDirection);

            // Immediately restart the pulse cycle in the new (bounced) direction,
            // so the fish visibly turns/tilts to face it and pulses away from the wall
            EnterState(PulseState.Pulsing);
        }
    }
}