using UnityEngine;
using System.Collections.Generic;

// Inherits from Fish - reuses rotation/tilt/facing/HP systems,
// but overrides movement entirely for floaty zigzag behavior instead of pulsing
public class Alien : Fish
{
    [Header("Floaty Movement")]
    public float floatSpeed = 1.2f;          // base drifting speed, generally slower/lazier than fish pulses
    public float velocitySmoothing = 2f;     // how quickly actual velocity catches up to desired velocity (lower = more floaty/laggy)

    [Header("Zigzag")]
    public float zigzagFrequency = 1.5f;     // how fast the side-to-side wobble oscillates
    public float zigzagAmplitude = 0.6f;     // how strong the side-to-side wobble is

    [Header("Seeking")]
    public float seekStrength = 0.7f;        // 0-1, how strongly the alien steers toward its target (1 = direct, 0 = ignores target)
    public float clumsyNoise = 0.4f;         // how much random wobble gets mixed into the seek direction, simulating clumsy tracking

    [Header("Eating")]
    public float eatCooldown = 2f;           // time after an eat attempt before another can be tried
    [Range(0f, 100f)]
    public float eatChancePercent = 25f;     // percent chance to successfully eat on each attempt
    public int damagePerAttack = 1;          // how much HP the target loses on a successful eat attempt

    [Header("Target Tracking")]
    public float targetCheckInterval = 1f;   // periodic safety-check interval, backup in case target loss isn't caught elsewhere

    protected Fish targetFish;               // the fish this alien is currently hunting
    protected float zigzagTimer;             // drives the sine wave for zigzag motion
    protected bool isTouchingTarget;         // true while actively colliding with the current target fish
    protected float eatCooldownTimer = 0f;   // counts down; attempts can only happen when this is <= 0
    protected float targetCheckTimer = 0f;   // counts up toward the next periodic safety-check

    protected override void Awake()
    {
        base.Awake(); // still grabs rb, baseScaleX/Y, initial facing, and HP setup from Fish
    }

    protected override void Start()
    {
        // Skip Fish's pulse-based Start() entirely (no PickNewDirection/EnterState needed) -
        // Alien picks its own initial wander direction instead
        targetDirection = Random.insideUnitCircle.normalized;

        AcquireRandomTarget();
    }

    protected override void Update()
    {
        // NOTE: deliberately does NOT call base.Update() - that would run the pulse
        // state machine, which Alien doesn't use. We handle movement direction ourselves below.

        // Immediate check - catches most loss cases the instant the reference goes null
        if (targetFish == null)
        {
            AcquireRandomTarget();
        }
        else
        {
            // Periodic safety-check as a backup, in case the target became invalid
            // in a way that a simple null check doesn't reliably catch every frame
            targetCheckTimer += Time.deltaTime;
            if (targetCheckTimer >= targetCheckInterval)
            {
                targetCheckTimer = 0f;

                if (targetFish == null || targetFish.gameObject == null)
                {
                    DevTools.LogTargetLost(gameObject, "target became invalid");
                    AcquireRandomTarget();
                }
            }
        }

        UpdateZigzagDirection();
        FaceMovementDirection(); // reused from Fish - handles the squash/flip + tilt rotation

        HandleEating();
    }

    protected override void FixedUpdate()
    {
        // NOTE: deliberately does NOT call base.FixedUpdate() - Alien uses continuous
        // floaty velocity instead of the Pulsing/Decelerating/Resting states.

        Vector2 desiredVelocity = targetDirection * floatSpeed;

        // Lower velocitySmoothing = laggier, more "floaty" catch-up to the desired direction
        rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, desiredVelocity, velocitySmoothing * Time.fixedDeltaTime);
    }

    // Finds every Fish currently in the scene (excluding itself and other Aliens) and picks one at random.
    // Called on Start, whenever the current target becomes null, or via the periodic safety-check.
    protected virtual void AcquireRandomTarget()
    {
        string previousTargetName = targetFish != null ? targetFish.name : null;

        // FindObjectsByType grabs every active Fish-derived component in the scene, including Alien itself
        Fish[] allFish = FindObjectsByType<Fish>(FindObjectsSortMode.None);

        // Build a list of valid targets: exclude self, and exclude any predator-flagged fish
        List<Fish> validTargets = new List<Fish>();
        foreach (Fish fish in allFish)
        {
            if (fish == this) continue;
            if (fish.isPredator) continue;
            validTargets.Add(fish);
        }

        if (validTargets.Count > 0)
        {
            int randomIndex = Random.Range(0, validTargets.Count);
            targetFish = validTargets[randomIndex];
            DevTools.LogTargetAcquired(gameObject, targetFish.gameObject);
        }
        else
        {
            if (previousTargetName != null)
            {
                DevTools.LogTargetLost(gameObject, previousTargetName);
            }
            targetFish = null; // no fish left in the tank to hunt
        }

        isTouchingTarget = false; // reset touch state whenever the target changes
        targetCheckTimer = 0f;
    }

    // Combines base wandering, zigzag oscillation, and (if present) seeking toward the target
    // into a single blended direction, then clamps it to the same tilt limit as regular fish
    protected virtual void UpdateZigzagDirection()
    {
        zigzagTimer += Time.deltaTime * zigzagFrequency;

        // Perpendicular offset to whatever direction we're currently facing, creates the side-to-side wobble
        Vector2 perpendicular = new Vector2(-targetDirection.y, targetDirection.x);
        Vector2 zigzagOffset = perpendicular * Mathf.Sin(zigzagTimer) * zigzagAmplitude;

        Vector2 baseDirection = targetDirection;

        if (targetFish != null)
        {
            Vector2 towardTarget = ((Vector2)targetFish.transform.position - (Vector2)transform.position).normalized;

            // Add random noise to the seek direction each frame, simulating clumsy/imprecise tracking
            Vector2 clumsyOffset = Random.insideUnitCircle * clumsyNoise;
            Vector2 noisySeek = (towardTarget + clumsyOffset).normalized;

            // Blend between current wandering direction and the (noisy) seek direction
            baseDirection = Vector2.Lerp(baseDirection, noisySeek, seekStrength).normalized;
        }

        Vector2 combined = (baseDirection + zigzagOffset).normalized;
        targetDirection = ClampToHorizontalTilt(combined); // reused from Fish - keeps the same +/-55 degree tilt limit
    }

    // Ticks the cooldown down over time, and attempts an eat roll immediately upon
    // touching the target IF the cooldown has already expired.
    protected virtual void HandleEating()
    {
        if (eatCooldownTimer > 0f)
        {
            eatCooldownTimer -= Time.deltaTime;
        }

        if (targetFish == null || !isTouchingTarget)
            return; // nothing to attempt right now

        if (eatCooldownTimer <= 0f)
        {
            AttemptEat();
        }
    }

    // Rolls the eat chance and starts the cooldown regardless of success or failure
    protected virtual void AttemptEat()
    {
        float roll = Random.Range(0f, 100f);
        bool success = roll <= eatChancePercent;

        DevTools.LogEatAttempt(gameObject, targetFish.gameObject, roll, eatChancePercent, success);

        eatCooldownTimer = eatCooldown; // reset cooldown either way - can't try again until this clears

        if (success)
        {
            EatTarget();
        }
    }

    // Handles a successful eat attempt - damages the target instead of instantly destroying it.
    // If the damage kills it, clears the reference so a new target gets picked next Update.
    protected virtual void EatTarget()
    {
        if (targetFish == null) return;

        string eatenName = targetFish.name;
        bool killed = targetFish.TakeDamage(damagePerAttack, gameObject);

        if (killed)
        {
            DevTools.LogTargetLost(gameObject, eatenName);
            targetFish = null;
            isTouchingTarget = false;
        }
        // else: fish survived, still touching, will attempt again once cooldown clears
    }

    // Aliens ignore wall collisions (Layer Collision Matrix already prevents this from
    // firing for walls - this is a safety net), but DO care about colliding with the target fish
    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        DevTools.LogCollision(gameObject, collision.gameObject);

        if (targetFish != null && collision.gameObject == targetFish.gameObject)
        {
            isTouchingTarget = true;

            // Try eating right away on contact, if the cooldown has already expired
            if (eatCooldownTimer <= 0f)
            {
                AttemptEat();
            }
        }
    }

    // Called automatically by Unity when a collision ends
    protected virtual void OnCollisionExit2D(Collision2D collision)
    {
        if (targetFish != null && collision.gameObject == targetFish.gameObject)
        {
            isTouchingTarget = false;
        }
    }
}